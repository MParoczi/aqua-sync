using AquaSync.Eheim.Devices.Enums;

namespace AquaSync.Eheim.Devices;

/// <summary>
/// Interface for an EHEIM professionel 5e filter device.
/// All observable properties are backed by BehaviorSubject — new subscribers receive the latest value immediately.
/// Flow rates are always normalized to metric (L/h). Frequencies are in Hz.
/// </summary>
public interface IEheimFilter : IEheimDevice
{
    // Filter model
    EheimFilterModel FilterModel { get; }

    // State (read-only observables)
    IObservable<bool> IsActive { get; }
    IObservable<double> CurrentSpeed { get; }
    IObservable<FilterMode> FilterMode { get; }
    IObservable<double> ServiceHours { get; }
    IObservable<TimeSpan> OperatingTime { get; }

    // Manual mode
    IObservable<double> ManualSpeed { get; }
    IReadOnlyList<double> AvailableManualSpeeds { get; }

    // Constant flow mode
    IObservable<int> ConstantFlowIndex { get; }
    IReadOnlyList<int> AvailableFlowRates { get; }

    // Bio mode
    IObservable<int> DaySpeed { get; }
    IObservable<int> NightSpeed { get; }
    IObservable<TimeOnly> DayStartTime { get; }
    IObservable<TimeOnly> NightStartTime { get; }

    // Pulse mode
    IObservable<int> HighPulseSpeed { get; }
    IObservable<int> LowPulseSpeed { get; }
    IObservable<TimeSpan> HighPulseTime { get; }
    IObservable<TimeSpan> LowPulseTime { get; }

    // Commands
    Task SetActiveAsync(bool active, CancellationToken cancellationToken = default);
    Task SetFilterModeAsync(FilterMode mode, CancellationToken cancellationToken = default);
    Task SetManualSpeedAsync(double hz, CancellationToken cancellationToken = default);
    Task SetConstantFlowAsync(int flowIndex, CancellationToken cancellationToken = default);
    Task SetDaySpeedAsync(int flowIndex, CancellationToken cancellationToken = default);
    Task SetNightSpeedAsync(int flowIndex, CancellationToken cancellationToken = default);
    Task SetDayStartTimeAsync(TimeOnly time, CancellationToken cancellationToken = default);
    Task SetNightStartTimeAsync(TimeOnly time, CancellationToken cancellationToken = default);
    Task SetHighPulseSpeedAsync(int flowIndex, CancellationToken cancellationToken = default);
    Task SetLowPulseSpeedAsync(int flowIndex, CancellationToken cancellationToken = default);
    Task SetHighPulseTimeAsync(TimeSpan duration, CancellationToken cancellationToken = default);
    Task SetLowPulseTimeAsync(TimeSpan duration, CancellationToken cancellationToken = default);
}
