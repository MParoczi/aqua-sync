using System.Collections.Concurrent;
using AquaSync.App.Contracts.Services;
using AquaSync.App.Models;
using AquaSync.Chihiros.Devices;
using AquaSync.Chihiros.Discovery;
using AquaSync.Chihiros.Scheduling;

namespace AquaSync.App.Services;

public sealed class LampService : ILampService
{
    private const string LampsFolder = "lamps";

    private readonly IDataService _dataService;
    private readonly IDeviceScanner _scanner;
    private readonly ConcurrentDictionary<string, IChihirosDevice> _connections = new(StringComparer.OrdinalIgnoreCase);

    public LampService(IDataService dataService, IDeviceScanner scanner)
    {
        _dataService = dataService;
        _scanner = scanner;
    }

    // ── Persistence ──────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<LampConfiguration>> GetLampsForAquariumAsync(
        Guid aquariumId,
        CancellationToken cancellationToken = default)
    {
        var all = await _dataService.ReadAllAsync<LampConfiguration>(LampsFolder).ConfigureAwait(false);
        return all.Where(l => l.AquariumId == aquariumId)
                  .OrderBy(l => l.CreatedAt)
                  .ToList();
    }

    public async Task<bool> IsAddressAssignedAsync(
        string bluetoothAddress,
        CancellationToken cancellationToken = default)
    {
        var all = await _dataService.ReadAllAsync<LampConfiguration>(LampsFolder).ConfigureAwait(false);
        return all.Any(l => string.Equals(l.BluetoothAddress, bluetoothAddress, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<LampConfiguration> AddLampAsync(
        Guid aquariumId,
        DiscoveredDevice device,
        CancellationToken cancellationToken = default)
    {
        var address = device.BluetoothAddress.ToString("X12");

        if (await IsAddressAssignedAsync(address, cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException($"Device {address} is already assigned to an aquarium.");

        var lamp = new LampConfiguration
        {
            Id = Guid.NewGuid(),
            AquariumId = aquariumId,
            BluetoothAddress = address,
            DeviceName = device.Name,
            ModelName = device.MatchedProfile?.ModelName ?? string.Empty,
            Mode = LampMode.Off,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        await _dataService.SaveAsync(LampsFolder, lamp.Id.ToString(), lamp).ConfigureAwait(false);
        return lamp;
    }

    public async Task RemoveLampAsync(Guid lampId, CancellationToken cancellationToken = default)
    {
        var lamp = await _dataService.ReadAsync<LampConfiguration>(LampsFolder, lampId.ToString()).ConfigureAwait(false);
        if (lamp is not null)
            await DisconnectAsync(lamp.BluetoothAddress).ConfigureAwait(false);

        await _dataService.DeleteAsync(LampsFolder, lampId.ToString()).ConfigureAwait(false);
    }

    public async Task SaveModeAsync(Guid lampId, LampMode mode, CancellationToken cancellationToken = default)
    {
        var lamp = await _dataService.ReadAsync<LampConfiguration>(LampsFolder, lampId.ToString()).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Lamp {lampId} not found.");

        lamp.Mode = mode;
        await _dataService.SaveAsync(LampsFolder, lamp.Id.ToString(), lamp).ConfigureAwait(false);
    }

    public async Task SaveManualBrightnessAsync(
        Guid lampId,
        Dictionary<string, byte> brightness,
        CancellationToken cancellationToken = default)
    {
        var lamp = await _dataService.ReadAsync<LampConfiguration>(LampsFolder, lampId.ToString()).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Lamp {lampId} not found.");

        lamp.ManualBrightness = brightness;
        await _dataService.SaveAsync(LampsFolder, lamp.Id.ToString(), lamp).ConfigureAwait(false);
    }

    public async Task SaveScheduleAsync(
        Guid lampId,
        ScheduleConfiguration schedule,
        CancellationToken cancellationToken = default)
    {
        if (schedule.Sunrise >= schedule.Sunset)
            throw new ArgumentException("Sunrise must be earlier than sunset.");

        var gapMinutes = (schedule.Sunset.ToTimeSpan() - schedule.Sunrise.ToTimeSpan()).TotalMinutes;
        if (gapMinutes < schedule.RampUpMinutes + 1)
            throw new ArgumentException(
                "The interval from sunrise to sunset must be at least the ramp-up duration plus one minute.");

        if (schedule.RampUpMinutes is < 0 or > 150)
            throw new ArgumentException("Ramp-up duration must be between 0 and 150 minutes.");

        if (schedule.ActiveDays == Weekday.None)
            throw new ArgumentException("At least one day must be selected.");

        var lamp = await _dataService.ReadAsync<LampConfiguration>(LampsFolder, lampId.ToString()).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Lamp {lampId} not found.");

        lamp.Schedule = schedule;
        await _dataService.SaveAsync(LampsFolder, lamp.Id.ToString(), lamp).ConfigureAwait(false);
    }

    // ── Discovery ─────────────────────────────────────────────────────────────

    public async Task ScanAsync(IProgress<DiscoveredDevice> progress, CancellationToken cancellationToken = default)
    {
        await _scanner.ScanAsync(TimeSpan.FromMinutes(10), progress, cancellationToken).ConfigureAwait(false);
    }

    public DeviceProfile? GetProfileForModel(string modelName)
    {
        if (string.IsNullOrEmpty(modelName))
            return null;

        return DeviceProfiles.All.FirstOrDefault(p => p.ModelName == modelName);
    }

    // ── Device Control ────────────────────────────────────────────────────────

    public async Task<IChihirosDevice?> ConnectAsync(
        LampConfiguration lamp,
        CancellationToken cancellationToken = default)
    {
        if (_connections.TryGetValue(lamp.BluetoothAddress, out var existing))
            return existing;

        var profile = GetProfileForModel(lamp.ModelName);
        if (profile is null)
            return null;

        var address = Convert.ToUInt64(lamp.BluetoothAddress, 16);
        var device = new ChihirosDevice(address, lamp.DeviceName, profile);

        // Subscribe before adding to cache so we never miss the event.
        var key = lamp.BluetoothAddress;
        device.Disconnected += (_, _) => _connections.TryRemove(key, out _);

        await device.ConnectAsync(cancellationToken).ConfigureAwait(false);
        _connections[lamp.BluetoothAddress] = device;

        return device;
    }

    public async Task DisconnectAsync(string bluetoothAddress)
    {
        if (_connections.TryRemove(bluetoothAddress, out var device))
            await device.DisconnectAsync().ConfigureAwait(false);
    }
}
