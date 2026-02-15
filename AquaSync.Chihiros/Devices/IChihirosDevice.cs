using AquaSync.Chihiros.Scheduling;

namespace AquaSync.Chihiros.Devices;

/// <summary>
/// Interface for controlling a Chihiros BLE LED device.
/// </summary>
public interface IChihirosDevice : IAsyncDisposable
{
    // --- Identity ---

    /// <summary>BLE address of the device.</summary>
    string Address { get; }

    /// <summary>Display name of the device.</summary>
    string Name { get; }

    /// <summary>The device's hardware profile (model, channels).</summary>
    DeviceProfile Profile { get; }

    /// <summary>Whether a BLE connection is currently established.</summary>
    bool IsConnected { get; }

    // --- Events ---

    /// <summary>Raised when a BLE connection is successfully established.</summary>
    event EventHandler? Connected;

    /// <summary>Raised when the BLE connection drops. The string argument contains the reason.</summary>
    event EventHandler<string>? Disconnected;

    // --- Connection ---

    /// <summary>Establish a BLE connection to the device.</summary>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>Gracefully disconnect from the device.</summary>
    Task DisconnectAsync();

    // --- Manual control ---

    /// <summary>Set brightness (0–100) on a specific color channel.</summary>
    Task SetBrightnessAsync(ColorChannel channel, byte brightness, CancellationToken cancellationToken = default);

    /// <summary>Turn on all channels at full brightness.</summary>
    Task TurnOnAsync(CancellationToken cancellationToken = default);

    /// <summary>Turn off all channels (brightness 0).</summary>
    Task TurnOffAsync(CancellationToken cancellationToken = default);

    // --- Auto mode ---

    /// <summary>Enable auto mode and sync the device clock.</summary>
    Task EnableAutoModeAsync(CancellationToken cancellationToken = default);

    /// <summary>Set the device's internal clock.</summary>
    Task SetTimeAsync(DateTime time, CancellationToken cancellationToken = default);

    // --- Scheduling ---

    /// <summary>Add a lighting schedule. Per-channel brightness is mapped to protocol slots internally.</summary>
    Task AddScheduleAsync(LightSchedule schedule, CancellationToken cancellationToken = default);

    /// <summary>Remove a previously added schedule.</summary>
    Task RemoveScheduleAsync(LightSchedule schedule, CancellationToken cancellationToken = default);

    /// <summary>Reset all schedules on the device.</summary>
    Task ResetSchedulesAsync(CancellationToken cancellationToken = default);
}
