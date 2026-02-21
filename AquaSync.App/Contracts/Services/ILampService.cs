using AquaSync.App.Models;
using AquaSync.Chihiros.Devices;
using AquaSync.Chihiros.Discovery;

namespace AquaSync.App.Contracts.Services;

public interface ILampService
{
    // ── Persistence ──────────────────────────────────────────────────────────

    /// <summary>Returns all lamps assigned to the specified aquarium, ordered by CreatedAt ascending.</summary>
    Task<IReadOnlyList<LampConfiguration>> GetLampsForAquariumAsync(
        Guid aquariumId,
        CancellationToken cancellationToken = default);

    /// <summary>Returns true if a lamp with the given BLE address is already assigned to any aquarium.</summary>
    Task<bool> IsAddressAssignedAsync(
        string bluetoothAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a new LampConfiguration for the device and saves it.
    ///     Throws <see cref="InvalidOperationException"/> if the address is already assigned.
    /// </summary>
    Task<LampConfiguration> AddLampAsync(
        Guid aquariumId,
        DiscoveredDevice device,
        CancellationToken cancellationToken = default);

    /// <summary>Permanently deletes the lamp configuration and evicts any cached connection.</summary>
    Task RemoveLampAsync(
        Guid lampId,
        CancellationToken cancellationToken = default);

    /// <summary>Persists a mode change. Does not send BLE commands — caller is responsible.</summary>
    Task SaveModeAsync(
        Guid lampId,
        LampMode mode,
        CancellationToken cancellationToken = default);

    /// <summary>Persists manual brightness values. Does not send BLE commands — caller is responsible.</summary>
    Task SaveManualBrightnessAsync(
        Guid lampId,
        Dictionary<string, byte> brightness,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Validates and persists a schedule configuration.
    ///     Throws <see cref="ArgumentException"/> with a specific message if validation fails.
    /// </summary>
    Task SaveScheduleAsync(
        Guid lampId,
        ScheduleConfiguration schedule,
        CancellationToken cancellationToken = default);

    // ── Discovery ─────────────────────────────────────────────────────────────

    /// <summary>
    ///     Starts a continuous BLE scan, reporting each discovered device via progress.
    ///     Runs until the CancellationToken is cancelled or the 10-minute timeout elapses.
    /// </summary>
    Task ScanAsync(
        IProgress<DiscoveredDevice> progress,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the DeviceProfile matching the given model name, or null if unknown.</summary>
    DeviceProfile? GetProfileForModel(string modelName);

    // ── Device Control ────────────────────────────────────────────────────────

    /// <summary>
    ///     Connects to the physical device and returns the IChihirosDevice instance.
    ///     Caches the connection; subsequent calls return the cached instance if already connected.
    ///     Returns null if the device profile is unknown (unmanaged lamp).
    /// </summary>
    Task<IChihirosDevice?> ConnectAsync(
        LampConfiguration lamp,
        CancellationToken cancellationToken = default);

    /// <summary>Disconnects and disposes the cached connection for the given address. No-op if not connected.</summary>
    Task DisconnectAsync(string bluetoothAddress);
}
