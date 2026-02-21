using System.Collections.Concurrent;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using AquaSync.Chihiros.Devices;
using AquaSync.Chihiros.Protocol;

namespace AquaSync.Chihiros.Discovery;

/// <summary>
///     Scans for Chihiros BLE devices using <see cref="BluetoothLEAdvertisementWatcher" />.
/// </summary>
public sealed class DeviceScanner : IDeviceScanner
{
    /// <summary>
    ///     Scan for Chihiros devices for the given duration.
    ///     Devices are reported via <paramref name="progress" /> as they are found and returned as a list when complete.
    /// </summary>
    public async Task<IReadOnlyList<DiscoveredDevice>> ScanAsync(
        TimeSpan timeout,
        IProgress<DiscoveredDevice>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var discovered = new ConcurrentDictionary<ulong, DiscoveredDevice>();
        var tcs = new TaskCompletionSource();

        var watcher = new BluetoothLEAdvertisementWatcher
        {
            ScanningMode = BluetoothLEScanningMode.Active,
            AllowExtendedAdvertisements = true
        };

        // Filter by UART service UUID to find Nordic UART devices
        watcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(UartConstants.ServiceUuid);

        watcher.Stopped += (_, args) =>
        {
            if (args.Error != BluetoothError.Success)
                tcs.TrySetException(new InvalidOperationException($"Bluetooth scanner stopped with error: {args.Error}"));
            else
                tcs.TrySetResult();
        };

        watcher.Received += (_, args) =>
        {
            var name = args.Advertisement.LocalName;
            if (string.IsNullOrEmpty(name))
                return;

            var address = args.BluetoothAddress;
            var profile = DeviceProfiles.MatchFromName(name);

            var device = new DiscoveredDevice(address, name, args.RawSignalStrengthInDBm, profile);

            if (discovered.TryAdd(address, device)) progress?.Report(device);
        };

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        using var registration = cts.Token.Register(() => tcs.TrySetResult());

        watcher.Start();

        try
        {
            await tcs.Task.ConfigureAwait(false);
        }
        finally
        {
            watcher.Stop();
        }

        return discovered.Values.ToList().AsReadOnly();
    }

    /// <summary>
    ///     Scan without the UART service UUID filter, matching devices by known BLE name prefixes instead.
    ///     Useful when devices do not advertise service UUIDs in their advertisement packets.
    /// </summary>
    public async Task<IReadOnlyList<DiscoveredDevice>> ScanByNameAsync(
        TimeSpan timeout,
        IProgress<DiscoveredDevice>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var discovered = new ConcurrentDictionary<ulong, DiscoveredDevice>();
        var tcs = new TaskCompletionSource();

        var watcher = new BluetoothLEAdvertisementWatcher
        {
            ScanningMode = BluetoothLEScanningMode.Active,
            AllowExtendedAdvertisements = true
        };

        watcher.Stopped += (_, args) =>
        {
            if (args.Error != BluetoothError.Success)
                tcs.TrySetException(new InvalidOperationException($"Bluetooth scanner stopped with error: {args.Error}"));
            else
                tcs.TrySetResult();
        };

        watcher.Received += (_, args) =>
        {
            var name = args.Advertisement.LocalName;
            if (string.IsNullOrEmpty(name))
                return;

            var profile = DeviceProfiles.MatchFromName(name);

            // Accept if: profile matched by known code prefix, name starts with "DY" (all Chihiros
            // model codes share this prefix, covering unknown/future models), or device advertises
            // the Nordic UART service UUID directly.
            var hasUartUuid = args.Advertisement.ServiceUuids.Contains(UartConstants.ServiceUuid);
            if (profile is null
                && !name.StartsWith("DY", StringComparison.OrdinalIgnoreCase)
                && !hasUartUuid)
                return;

            var address = args.BluetoothAddress;
            var device = new DiscoveredDevice(address, name, args.RawSignalStrengthInDBm, profile);

            if (discovered.TryAdd(address, device)) progress?.Report(device);
        };

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        using var registration = cts.Token.Register(() => tcs.TrySetResult());

        watcher.Start();

        try
        {
            await tcs.Task.ConfigureAwait(false);
        }
        finally
        {
            watcher.Stop();
        }

        return discovered.Values.ToList().AsReadOnly();
    }
}
