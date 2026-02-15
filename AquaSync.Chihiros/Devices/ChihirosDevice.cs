using AquaSync.Chihiros.Exceptions;
using AquaSync.Chihiros.Protocol;
using AquaSync.Chihiros.Scheduling;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace AquaSync.Chihiros.Devices;

/// <summary>
/// Controls a Chihiros BLE LED device via WinRT Bluetooth LE APIs.
/// </summary>
public sealed class ChihirosDevice : IChihirosDevice
{
    private readonly ulong _bluetoothAddress;
    private readonly SemaphoreSlim _commandLock = new(1, 1);

    private BluetoothLEDevice? _bleDevice;
    private GattCharacteristic? _rxCharacteristic;
    private GattCharacteristic? _txCharacteristic;
    private MessageId _messageId = new();
    private bool _disposed;

    public ChihirosDevice(ulong bluetoothAddress, string name, DeviceProfile profile)
    {
        _bluetoothAddress = bluetoothAddress;
        Name = name;
        Profile = profile;
    }

    // --- Identity ---

    public string Address => _bluetoothAddress.ToString("X12");
    public string Name { get; }
    public DeviceProfile Profile { get; }
    public bool IsConnected => _bleDevice?.ConnectionStatus == BluetoothConnectionStatus.Connected;

    // --- Events ---

    public event EventHandler? Connected;
    public event EventHandler<string>? Disconnected;

    // --- Connection ---

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var device = await BluetoothLEDevice.FromBluetoothAddressAsync(_bluetoothAddress)
            .AsTask(cancellationToken).ConfigureAwait(false);

        if (device is null)
            throw new DeviceNotFoundException($"No BLE device found at address {Address}.");

        device.ConnectionStatusChanged += OnConnectionStatusChanged;
        _bleDevice = device;

        // Resolve GATT services
        var servicesResult = await device.GetGattServicesForUuidAsync(UartConstants.ServiceUuid)
            .AsTask(cancellationToken).ConfigureAwait(false);

        if (servicesResult.Status != GattCommunicationStatus.Success || servicesResult.Services.Count == 0)
            throw new CharacteristicMissingException($"UART service not found on device {Name}.");

        var service = servicesResult.Services[0];

        // Resolve RX characteristic (write target)
        var rxResult = await service.GetCharacteristicsForUuidAsync(UartConstants.RxCharacteristicUuid)
            .AsTask(cancellationToken).ConfigureAwait(false);

        if (rxResult.Status != GattCommunicationStatus.Success || rxResult.Characteristics.Count == 0)
            throw new CharacteristicMissingException($"RX characteristic not found on device {Name}.");

        _rxCharacteristic = rxResult.Characteristics[0];

        // Resolve TX characteristic (notification source)
        var txResult = await service.GetCharacteristicsForUuidAsync(UartConstants.TxCharacteristicUuid)
            .AsTask(cancellationToken).ConfigureAwait(false);

        if (txResult.Status != GattCommunicationStatus.Success || txResult.Characteristics.Count == 0)
            throw new CharacteristicMissingException($"TX characteristic not found on device {Name}.");

        _txCharacteristic = txResult.Characteristics[0];

        // Subscribe to notifications from TX
        var notifyStatus = await _txCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
            GattClientCharacteristicConfigurationDescriptorValue.Notify)
            .AsTask(cancellationToken).ConfigureAwait(false);

        if (notifyStatus != GattCommunicationStatus.Success)
            throw new DeviceConnectionException($"Failed to subscribe to TX notifications on device {Name}.");

        _txCharacteristic.ValueChanged += OnTxValueChanged;

        Connected?.Invoke(this, EventArgs.Empty);
    }

    public async Task DisconnectAsync()
    {
        if (_txCharacteristic is not null)
        {
            _txCharacteristic.ValueChanged -= OnTxValueChanged;

            try
            {
                await _txCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.None)
                    .AsTask().ConfigureAwait(false);
            }
            catch
            {
                // Best-effort unsubscribe
            }

            _txCharacteristic = null;
        }

        _rxCharacteristic = null;

        if (_bleDevice is not null)
        {
            _bleDevice.ConnectionStatusChanged -= OnConnectionStatusChanged;
            _bleDevice.Dispose();
            _bleDevice = null;
        }
    }

    // --- Manual control ---

    public async Task SetBrightnessAsync(ColorChannel channel, byte brightness, CancellationToken cancellationToken = default)
    {
        var mapping = FindChannelMapping(channel);
        _messageId = _messageId.Next();
        var command = CommandBuilder.CreateManualBrightnessCommand(_messageId, mapping.ProtocolChannelId, brightness);
        await SendCommandAsync(command, cancellationToken).ConfigureAwait(false);
    }

    public async Task TurnOnAsync(CancellationToken cancellationToken = default)
    {
        foreach (var mapping in Profile.Channels)
        {
            await SetBrightnessAsync(mapping.Channel, 100, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task TurnOffAsync(CancellationToken cancellationToken = default)
    {
        foreach (var mapping in Profile.Channels)
        {
            await SetBrightnessAsync(mapping.Channel, 0, cancellationToken).ConfigureAwait(false);
        }
    }

    // --- Auto mode ---

    public async Task EnableAutoModeAsync(CancellationToken cancellationToken = default)
    {
        _messageId = _messageId.Next();
        var switchCmd = CommandBuilder.CreateSwitchToAutoModeCommand(_messageId);
        await SendCommandAsync(switchCmd, cancellationToken).ConfigureAwait(false);

        _messageId = _messageId.Next();
        var timeCmd = CommandBuilder.CreateSetTimeCommand(_messageId, DateTime.Now);
        await SendCommandAsync(timeCmd, cancellationToken).ConfigureAwait(false);
    }

    public async Task SetTimeAsync(DateTime time, CancellationToken cancellationToken = default)
    {
        _messageId = _messageId.Next();
        var command = CommandBuilder.CreateSetTimeCommand(_messageId, time);
        await SendCommandAsync(command, cancellationToken).ConfigureAwait(false);
    }

    // --- Scheduling ---

    public async Task AddScheduleAsync(LightSchedule schedule, CancellationToken cancellationToken = default)
    {
        var (b0, b1, b2) = MapScheduleBrightness(schedule);
        _messageId = _messageId.Next();
        var command = CommandBuilder.CreateAddAutoSettingCommand(
            _messageId,
            schedule.Sunrise,
            schedule.Sunset,
            b0, b1, b2,
            (byte)schedule.RampUpMinutes,
            (byte)schedule.Weekdays);
        await SendCommandAsync(command, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveScheduleAsync(LightSchedule schedule, CancellationToken cancellationToken = default)
    {
        _messageId = _messageId.Next();
        var command = CommandBuilder.CreateDeleteAutoSettingCommand(
            _messageId,
            schedule.Sunrise,
            schedule.Sunset,
            (byte)schedule.RampUpMinutes,
            (byte)schedule.Weekdays);
        await SendCommandAsync(command, cancellationToken).ConfigureAwait(false);
    }

    public async Task ResetSchedulesAsync(CancellationToken cancellationToken = default)
    {
        _messageId = _messageId.Next();
        var command = CommandBuilder.CreateResetAutoSettingsCommand(_messageId);
        await SendCommandAsync(command, cancellationToken).ConfigureAwait(false);
    }

    // --- IAsyncDisposable ---

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        await DisconnectAsync().ConfigureAwait(false);
        _commandLock.Dispose();
    }

    // --- Private helpers ---

    private ChannelMapping FindChannelMapping(ColorChannel channel)
    {
        foreach (var mapping in Profile.Channels)
        {
            if (mapping.Channel == channel)
                return mapping;
        }
        throw new ArgumentException($"Color channel '{channel}' is not supported by device profile '{Profile.ModelName}'.", nameof(channel));
    }

    /// <summary>
    /// Map per-channel brightness from a <see cref="LightSchedule"/> to the 3-byte protocol format.
    /// Protocol slots correspond to channel IDs 0, 1, 2. Channels not present default to 255 (off/unused).
    /// Note: channel 3 (white on WRGB) cannot be set via scheduling due to a protocol limitation.
    /// </summary>
    private (byte Slot0, byte Slot1, byte Slot2) MapScheduleBrightness(LightSchedule schedule)
    {
        byte slot0 = 255, slot1 = 255, slot2 = 255;

        foreach (var mapping in Profile.Channels)
        {
            if (!schedule.ChannelBrightness.TryGetValue(mapping.Channel, out var brightness))
                continue;

            switch (mapping.ProtocolChannelId)
            {
                case 0: slot0 = brightness; break;
                case 1: slot1 = brightness; break;
                case 2: slot2 = brightness; break;
                // Protocol channel 3+ cannot be scheduled (protocol limitation)
            }
        }

        return (slot0, slot1, slot2);
    }

    private async Task SendCommandAsync(byte[] command, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_rxCharacteristic is null)
            throw new DeviceConnectionException("Device is not connected. Call ConnectAsync first.");

        await _commandLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var writer = new DataWriter();
            writer.WriteBytes(command);
            var buffer = writer.DetachBuffer();

            var result = await _rxCharacteristic.WriteValueAsync(buffer, GattWriteOption.WriteWithoutResponse)
                .AsTask(cancellationToken).ConfigureAwait(false);

            if (result != GattCommunicationStatus.Success)
                throw new DeviceConnectionException($"Failed to write command to device {Name}. Status: {result}");
        }
        finally
        {
            _commandLock.Release();
        }
    }

    private void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
    {
        if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
        {
            Disconnected?.Invoke(this, "BLE connection lost.");
        }
    }

    private void OnTxValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
    {
        // Notification received from device — currently unused but available for future response parsing.
    }
}
