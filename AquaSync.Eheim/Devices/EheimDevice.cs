using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json.Nodes;
using AquaSync.Eheim.Protocol;
using AquaSync.Eheim.Protocol.Packets;
using AquaSync.Eheim.Transport;

namespace AquaSync.Eheim.Devices;

/// <summary>
/// Base class for all EHEIM Digital devices. Holds common metadata and the system LED observable.
/// </summary>
internal abstract class EheimDevice : IEheimDevice
{
    private readonly IEheimTransport _transport;
    private readonly BehaviorSubject<int> _systemLedBrightness;

    protected UsrDtaPacket UsrDta { get; private set; }

    public string MacAddress { get; }
    public string Name { get; }
    public string ModelName { get; }
    public string FirmwareVersion { get; }
    public string AquariumName { get; }

    public IObservable<int> SystemLedBrightness => _systemLedBrightness.AsObservable();

    protected IEheimTransport Transport => _transport;

    protected EheimDevice(IEheimTransport transport, UsrDtaPacket usrDta, string modelName)
    {
        _transport = transport;
        UsrDta = usrDta;
        MacAddress = usrDta.From;
        Name = usrDta.Name;
        ModelName = modelName;
        AquariumName = usrDta.AquariumName;
        FirmwareVersion = FormatFirmwareVersion(usrDta.Revision);
        _systemLedBrightness = new BehaviorSubject<int>(usrDta.SysLed);
    }

    public async Task SetSystemLedBrightnessAsync(int value, CancellationToken cancellationToken = default)
    {
        var overrides = new JsonObject { ["sysLED"] = value };
        var packet = PacketBuilder.SetUsrDta(UsrDta, overrides);
        await _transport.SendAsync(packet, cancellationToken).ConfigureAwait(false);
    }

    internal void UpdateUsrDta(UsrDtaPacket usrDta)
    {
        UsrDta = usrDta;
        _systemLedBrightness.OnNext(usrDta.SysLed);
    }

    internal abstract void UpdateFromMessage(JsonNode message);

    internal abstract Task RequestUpdateAsync(CancellationToken cancellationToken);

    private static string FormatFirmwareVersion(List<int> revision)
    {
        if (revision.Count < 2) return "unknown";
        var rv0 = revision[0];
        var rv1 = revision[1];
        return $"{rv0 / 1000}.{rv0 % 1000 / 100}.{rv0 % 100}_{rv1 / 1000}.{rv1 % 1000 / 100}.{rv1 % 100}";
    }
}
