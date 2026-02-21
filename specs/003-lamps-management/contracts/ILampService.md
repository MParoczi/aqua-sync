# Service Contract: ILampService

**Namespace**: `AquaSync.App.Contracts.Services`
**Implementation**: `AquaSync.App.Services.LampService`
**Lifetime**: Singleton

---

## Overview

`ILampService` is the single entry point for all lamp-related operations:
1. **Persistence** — CRUD for `LampConfiguration` via `IDataService`
2. **Discovery** — wraps `IDeviceScanner` for BLE scanning
3. **Device control** — manages `IChihirosDevice` connection cache and proxies BLE commands

---

## Interface Definition

```csharp
public interface ILampService
{
    // ── Persistence ──────────────────────────────────────────────────────────

    /// Returns all lamps assigned to the specified aquarium, ordered by CreatedAt ascending.
    Task<IReadOnlyList<LampConfiguration>> GetLampsForAquariumAsync(
        Guid aquariumId,
        CancellationToken cancellationToken = default);

    /// Returns true if a lamp with the given BLE address is already assigned to any aquarium.
    Task<bool> IsAddressAssignedAsync(
        string bluetoothAddress,
        CancellationToken cancellationToken = default);

    /// Creates a new LampConfiguration for the device and saves it.
    /// Throws InvalidOperationException if the address is already assigned.
    Task<LampConfiguration> AddLampAsync(
        Guid aquariumId,
        DiscoveredDevice device,
        CancellationToken cancellationToken = default);

    /// Permanently deletes the lamp configuration and evicts any cached connection.
    Task RemoveLampAsync(
        Guid lampId,
        CancellationToken cancellationToken = default);

    /// Persists a mode change. Does not send BLE commands — caller is responsible.
    Task SaveModeAsync(
        Guid lampId,
        LampMode mode,
        CancellationToken cancellationToken = default);

    /// Persists manual brightness values. Does not send BLE commands — caller is responsible.
    Task SaveManualBrightnessAsync(
        Guid lampId,
        Dictionary<string, byte> brightness,
        CancellationToken cancellationToken = default);

    /// Persists a schedule configuration. Does not program the device — caller is responsible.
    /// Throws ArgumentException if validation rules are violated.
    Task SaveScheduleAsync(
        Guid lampId,
        ScheduleConfiguration schedule,
        CancellationToken cancellationToken = default);

    // ── Discovery ─────────────────────────────────────────────────────────────

    /// Starts a continuous BLE scan, reporting each discovered device via progress.
    /// Runs until the CancellationToken is cancelled (user taps Stop) or timeout (10 min).
    /// Callers should use IProgress<DiscoveredDevice> constructed on the UI thread.
    Task ScanAsync(
        IProgress<DiscoveredDevice> progress,
        CancellationToken cancellationToken = default);

    /// Returns the DeviceProfile matching the given model name, or null if unknown.
    DeviceProfile? GetProfileForModel(string modelName);

    // ── Device Control ────────────────────────────────────────────────────────

    /// Connects to the physical device. Returns the connected IChihirosDevice instance.
    /// Caches the instance; subsequent calls return the cached instance if already connected.
    /// Returns null if the device profile is unknown (unmanaged lamp).
    Task<IChihirosDevice?> ConnectAsync(
        LampConfiguration lamp,
        CancellationToken cancellationToken = default);

    /// Disconnects and disposes the cached IChihirosDevice for the given address.
    /// No-op if no cached connection exists.
    Task DisconnectAsync(string bluetoothAddress);
}
```

---

## Behavior Notes

### AddLampAsync
- Sets `LampConfiguration.Mode = LampMode.Off` (initial state per clarification)
- Sets `LampConfiguration.ManualBrightness = {}` (empty; will populate on first manual use)
- Sets `LampConfiguration.Schedule = null`
- Sets `LampConfiguration.ModelName = device.MatchedProfile?.ModelName ?? string.Empty`
- Throws `InvalidOperationException` if `IsAddressAssignedAsync()` returns `true`

### SaveScheduleAsync
Validates before persisting:
- `schedule.Sunrise < schedule.Sunset`
- `schedule.Sunrise.ToTimeSpan() + TimeSpan.FromMinutes(schedule.RampUpMinutes) <= schedule.Sunset.ToTimeSpan()`
- `schedule.RampUpMinutes` in [0, 150]
- `schedule.ActiveDays != Weekday.None`

### ConnectAsync
- Builds `ChihirosDevice(ulong address, string name, DeviceProfile profile)`
- Address conversion: `Convert.ToUInt64(lamp.BluetoothAddress, 16)`
- Profile lookup: `GetProfileForModel(lamp.ModelName)` — if null (unmanaged), returns null without connecting
- Calls `device.ConnectAsync(cancellationToken)`
- Caches under `lamp.BluetoothAddress` key
- Subscribes to `device.Disconnected` to evict from cache on unexpected disconnection

### ScanAsync
- Calls `DeviceScanner.ScanAsync(TimeSpan.FromMinutes(10), progress, cancellationToken)`
- The `progress` object should be constructed on the UI thread (captures `SynchronizationContext`) to ensure callbacks run on the UI thread
- Caller cancels the token to stop scanning

---

## Error Handling

| Scenario | Behavior |
|----------|----------|
| Device already assigned | `AddLampAsync` throws `InvalidOperationException` |
| Invalid schedule | `SaveScheduleAsync` throws `ArgumentException` with message |
| Connect fails (BLE error) | `ConnectAsync` propagates `DeviceConnectionException` |
| Device unreachable during command | Caller catches `ChihirosException`, shows InfoBar error |
| Unmanaged lamp (no profile) | `ConnectAsync` returns `null`; caller disables control UI |
