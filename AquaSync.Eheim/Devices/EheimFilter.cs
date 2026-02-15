using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Nodes;
using AquaSync.Eheim.Data;
using AquaSync.Eheim.Devices.Enums;
using AquaSync.Eheim.Exceptions;
using AquaSync.Eheim.Protocol;
using AquaSync.Eheim.Protocol.Packets;
using AquaSync.Eheim.Transport;

namespace AquaSync.Eheim.Devices;

/// <summary>
/// EHEIM professionel 5e filter device.
/// Exposes per-property BehaviorSubject observables and read-modify-write command methods.
/// </summary>
internal sealed class EheimFilter : EheimDevice, IEheimFilter
{
    private FilterDataPacket? _filterData;

    // State observables
    private readonly BehaviorSubject<bool> _isActive = new(false);
    private readonly BehaviorSubject<double> _currentSpeed = new(0);
    private readonly BehaviorSubject<FilterMode> _filterMode = new(Enums.FilterMode.Manual);
    private readonly BehaviorSubject<double> _serviceHours = new(0);
    private readonly BehaviorSubject<TimeSpan> _operatingTime = new(TimeSpan.Zero);

    // Manual mode
    private readonly BehaviorSubject<double> _manualSpeed = new(0);

    // Constant flow mode
    private readonly BehaviorSubject<int> _constantFlowIndex = new(0);

    // Bio mode
    private readonly BehaviorSubject<int> _daySpeed = new(0);
    private readonly BehaviorSubject<int> _nightSpeed = new(0);
    private readonly BehaviorSubject<TimeOnly> _dayStartTime = new(TimeOnly.MinValue);
    private readonly BehaviorSubject<TimeOnly> _nightStartTime = new(TimeOnly.MinValue);

    // Pulse mode
    private readonly BehaviorSubject<int> _highPulseSpeed = new(0);
    private readonly BehaviorSubject<int> _lowPulseSpeed = new(0);
    private readonly BehaviorSubject<TimeSpan> _highPulseTime = new(TimeSpan.Zero);
    private readonly BehaviorSubject<TimeSpan> _lowPulseTime = new(TimeSpan.Zero);

    public EheimFilterModel FilterModel { get; }
    public IReadOnlyList<double> AvailableManualSpeeds { get; }
    public IReadOnlyList<int> AvailableFlowRates { get; }

    // IEheimFilter observable properties
    public IObservable<bool> IsActive => _isActive.AsObservable();
    public IObservable<double> CurrentSpeed => _currentSpeed.AsObservable();
    public IObservable<FilterMode> FilterMode => _filterMode.AsObservable();
    public IObservable<double> ServiceHours => _serviceHours.AsObservable();
    public IObservable<TimeSpan> OperatingTime => _operatingTime.AsObservable();
    public IObservable<double> ManualSpeed => _manualSpeed.AsObservable();
    public IObservable<int> ConstantFlowIndex => _constantFlowIndex.AsObservable();
    public IObservable<int> DaySpeed => _daySpeed.AsObservable();
    public IObservable<int> NightSpeed => _nightSpeed.AsObservable();
    public IObservable<TimeOnly> DayStartTime => _dayStartTime.AsObservable();
    public IObservable<TimeOnly> NightStartTime => _nightStartTime.AsObservable();
    public IObservable<int> HighPulseSpeed => _highPulseSpeed.AsObservable();
    public IObservable<int> LowPulseSpeed => _lowPulseSpeed.AsObservable();
    public IObservable<TimeSpan> HighPulseTime => _highPulseTime.AsObservable();
    public IObservable<TimeSpan> LowPulseTime => _lowPulseTime.AsObservable();

    internal EheimFilter(IEheimTransport transport, UsrDtaPacket usrDta, int filterVersion, string tankConfig)
        : base(transport, usrDta, FlowRateTable.ResolveFilterModel(filterVersion, tankConfig).ToString())
    {
        FilterModel = FlowRateTable.ResolveFilterModel(filterVersion, tankConfig);
        AvailableManualSpeeds = FlowRateTable.GetManualSpeeds(filterVersion);
        AvailableFlowRates = FlowRateTable.GetFlowRates(filterVersion);
    }

    internal override void UpdateFromMessage(JsonNode message)
    {
        var title = message["title"]?.GetValue<string>();
        if (title != MessageTitle.FilterData) return;

        var packet = message.Deserialize<FilterDataPacket>();
        if (packet is null) return;

        _filterData = packet;
        PushState(packet);
    }

    internal override async Task RequestUpdateAsync(CancellationToken cancellationToken)
    {
        await Transport.SendAsync(PacketBuilder.GetFilterData(MacAddress), cancellationToken).ConfigureAwait(false);
    }

    private void PushState(FilterDataPacket p)
    {
        _isActive.OnNext(p.FilterActive != 0);
        _currentSpeed.OnNext(p.Freq / 100.0);
        _filterMode.OnNext((Enums.FilterMode)(p.PumpMode & 0xFF));
        _serviceHours.OnNext(p.ServiceHour);
        _operatingTime.OnNext(TimeSpan.FromMinutes(p.RunTime));
        _manualSpeed.OnNext(p.FreqSoll / 100.0);
        _constantFlowIndex.OnNext(p.SollStep);
        _daySpeed.OnNext(p.NmDfsSollDay);
        _nightSpeed.OnNext(p.NmDfsSollNight);
        _dayStartTime.OnNext(MinutesToTimeOnly(p.EndTimeNightMode));
        _nightStartTime.OnNext(MinutesToTimeOnly(p.StartTimeNightMode));
        _highPulseSpeed.OnNext(p.PmDfsSollHigh);
        _lowPulseSpeed.OnNext(p.PmDfsSollLow);
        _highPulseTime.OnNext(TimeSpan.FromSeconds(p.PmTimeHigh));
        _lowPulseTime.OnNext(TimeSpan.FromSeconds(p.PmTimeLow));
    }

    // --- Commands (read-modify-write) ---

    public async Task SetActiveAsync(bool active, CancellationToken cancellationToken = default)
    {
        await Transport.SendAsync(PacketBuilder.SetFilterPump(MacAddress, active), cancellationToken).ConfigureAwait(false);
    }

    public async Task SetFilterModeAsync(Enums.FilterMode mode, CancellationToken cancellationToken = default)
    {
        var data = EnsureFilterData();
        switch (mode)
        {
            case Enums.FilterMode.Manual:
                await Transport.SendAsync(
                    PacketBuilder.StartManualMode(MacAddress, data.FreqSoll), cancellationToken).ConfigureAwait(false);
                break;
            case Enums.FilterMode.ConstantFlow:
                await Transport.SendAsync(
                    PacketBuilder.StartConstantFlowMode(MacAddress, data.SollStep), cancellationToken).ConfigureAwait(false);
                break;
            case Enums.FilterMode.Pulse:
                await Transport.SendAsync(
                    PacketBuilder.StartPulseMode(MacAddress,
                        data.PmDfsSollHigh, data.PmDfsSollLow,
                        data.PmTimeHigh, data.PmTimeLow), cancellationToken).ConfigureAwait(false);
                break;
            case Enums.FilterMode.Bio:
                await SendBioModeAsync(data, cancellationToken).ConfigureAwait(false);
                break;
        }
    }

    public async Task SetManualSpeedAsync(double hz, CancellationToken cancellationToken = default)
    {
        await Transport.SendAsync(
            PacketBuilder.StartManualMode(MacAddress, (int)(hz * 100)), cancellationToken).ConfigureAwait(false);
    }

    public async Task SetConstantFlowAsync(int flowIndex, CancellationToken cancellationToken = default)
    {
        await Transport.SendAsync(
            PacketBuilder.StartConstantFlowMode(MacAddress, flowIndex), cancellationToken).ConfigureAwait(false);
    }

    public async Task SetDaySpeedAsync(int flowIndex, CancellationToken cancellationToken = default)
    {
        var data = EnsureFilterData();
        await Transport.SendAsync(
            PacketBuilder.StartBioMode(MacAddress,
                flowIndex, data.NmDfsSollNight,
                data.EndTimeNightMode, data.StartTimeNightMode,
                data.Sync, data.PartnerName), cancellationToken).ConfigureAwait(false);
    }

    public async Task SetNightSpeedAsync(int flowIndex, CancellationToken cancellationToken = default)
    {
        var data = EnsureFilterData();
        await Transport.SendAsync(
            PacketBuilder.StartBioMode(MacAddress,
                data.NmDfsSollDay, flowIndex,
                data.EndTimeNightMode, data.StartTimeNightMode,
                data.Sync, data.PartnerName), cancellationToken).ConfigureAwait(false);
    }

    public async Task SetDayStartTimeAsync(TimeOnly time, CancellationToken cancellationToken = default)
    {
        var data = EnsureFilterData();
        await Transport.SendAsync(
            PacketBuilder.StartBioMode(MacAddress,
                data.NmDfsSollDay, data.NmDfsSollNight,
                TimeOnlyToMinutes(time), data.StartTimeNightMode,
                data.Sync, data.PartnerName), cancellationToken).ConfigureAwait(false);
    }

    public async Task SetNightStartTimeAsync(TimeOnly time, CancellationToken cancellationToken = default)
    {
        var data = EnsureFilterData();
        await Transport.SendAsync(
            PacketBuilder.StartBioMode(MacAddress,
                data.NmDfsSollDay, data.NmDfsSollNight,
                data.EndTimeNightMode, TimeOnlyToMinutes(time),
                data.Sync, data.PartnerName), cancellationToken).ConfigureAwait(false);
    }

    public async Task SetHighPulseSpeedAsync(int flowIndex, CancellationToken cancellationToken = default)
    {
        var data = EnsureFilterData();
        await Transport.SendAsync(
            PacketBuilder.StartPulseMode(MacAddress,
                flowIndex, data.PmDfsSollLow,
                data.PmTimeHigh, data.PmTimeLow), cancellationToken).ConfigureAwait(false);
    }

    public async Task SetLowPulseSpeedAsync(int flowIndex, CancellationToken cancellationToken = default)
    {
        var data = EnsureFilterData();
        await Transport.SendAsync(
            PacketBuilder.StartPulseMode(MacAddress,
                data.PmDfsSollHigh, flowIndex,
                data.PmTimeHigh, data.PmTimeLow), cancellationToken).ConfigureAwait(false);
    }

    public async Task SetHighPulseTimeAsync(TimeSpan duration, CancellationToken cancellationToken = default)
    {
        var data = EnsureFilterData();
        await Transport.SendAsync(
            PacketBuilder.StartPulseMode(MacAddress,
                data.PmDfsSollHigh, data.PmDfsSollLow,
                (int)duration.TotalSeconds, data.PmTimeLow), cancellationToken).ConfigureAwait(false);
    }

    public async Task SetLowPulseTimeAsync(TimeSpan duration, CancellationToken cancellationToken = default)
    {
        var data = EnsureFilterData();
        await Transport.SendAsync(
            PacketBuilder.StartPulseMode(MacAddress,
                data.PmDfsSollHigh, data.PmDfsSollLow,
                data.PmTimeHigh, (int)duration.TotalSeconds), cancellationToken).ConfigureAwait(false);
    }

    // --- Helpers ---

    private async Task SendBioModeAsync(FilterDataPacket data, CancellationToken cancellationToken)
    {
        await Transport.SendAsync(
            PacketBuilder.StartBioMode(MacAddress,
                data.NmDfsSollDay, data.NmDfsSollNight,
                data.EndTimeNightMode, data.StartTimeNightMode,
                data.Sync, data.PartnerName), cancellationToken).ConfigureAwait(false);
    }

    private FilterDataPacket EnsureFilterData()
    {
        return _filterData ?? throw new EheimCommunicationException(
            "No filter data has been received yet. Wait for the device to report its state before sending commands.");
    }

    private static TimeOnly MinutesToTimeOnly(int minutesFromMidnight)
        => TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(minutesFromMidnight));

    private static int TimeOnlyToMinutes(TimeOnly time)
        => time.Hour * 60 + time.Minute;
}
