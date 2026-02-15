using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Nodes;
using AquaSync.Eheim.Devices;
using AquaSync.Eheim.Devices.Enums;
using AquaSync.Eheim.Protocol;
using AquaSync.Eheim.Protocol.Packets;
using AquaSync.Eheim.Transport;

namespace AquaSync.Eheim;

/// <summary>
/// Connects to an EHEIM Digital hub via WebSocket, discovers devices, and routes messages.
/// </summary>
public sealed class EheimHub : IEheimHub
{
    private readonly IEheimTransport _transport;
    private readonly Uri _endpoint;
    private readonly ConcurrentDictionary<string, EheimDevice> _devices = new();
    private readonly Subject<IEheimDevice> _deviceDiscovered = new();
    private IDisposable? _messageSubscription;

    // Temporary storage: we may receive FILTER_DATA before USRDTA for a device,
    // or vice versa. We need both to fully construct an EheimFilter.
    private readonly ConcurrentDictionary<string, UsrDtaPacket> _pendingUsrDta = new();
    private readonly ConcurrentDictionary<string, int> _pendingFilterVersions = new();

    public bool IsConnected => _transport.IsConnected;

    public IObservable<IEheimDevice> DeviceDiscovered => _deviceDiscovered.AsObservable();

    public IReadOnlyDictionary<string, IEheimDevice> Devices =>
        _devices.ToDictionary(kv => kv.Key, kv => (IEheimDevice)kv.Value);

    public EheimHub(string host = "eheimdigital.local")
        : this(new EheimWebSocketTransport(), host)
    {
    }

    internal EheimHub(IEheimTransport transport, string host)
    {
        _transport = transport;
        _endpoint = new Uri($"ws://{host}/ws");
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        await _transport.ConnectAsync(_endpoint, cancellationToken).ConfigureAwait(false);
        _messageSubscription = _transport.Messages.Subscribe(OnMessageReceived);

        // Request device list from the hub
        await _transport.SendAsync(PacketBuilder.GetUsrDta("ALL"), cancellationToken).ConfigureAwait(false);
    }

    public async Task DisconnectAsync()
    {
        _messageSubscription?.Dispose();
        _messageSubscription = null;
        await _transport.DisconnectAsync().ConfigureAwait(false);
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _transport.SendAsync(PacketBuilder.GetUsrDta("ALL"), cancellationToken).ConfigureAwait(false);
        foreach (var device in _devices.Values)
        {
            await device.RequestUpdateAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
        await _transport.DisposeAsync().ConfigureAwait(false);
        _deviceDiscovered.Dispose();
    }

    private void OnMessageReceived(JsonNode message)
    {
        var from = message["from"]?.GetValue<string>();
        var title = message["title"]?.GetValue<string>();

        if (from is null || title is null) return;
        if (from == "USER") return; // Echoed message, ignore

        switch (title)
        {
            case MessageTitle.MeshNetwork:
                HandleMeshNetwork(message);
                break;

            case MessageTitle.UsrDta:
                HandleUsrDta(message);
                break;

            case MessageTitle.FilterData:
                HandleFilterData(from, message);
                break;

            default:
                // Route to the device if known
                if (_devices.TryGetValue(from, out var device))
                {
                    device.UpdateFromMessage(message);
                }
                break;
        }
    }

    private void HandleMeshNetwork(JsonNode message)
    {
        var packet = message.Deserialize<MeshNetworkPacket>();
        if (packet is null) return;

        foreach (var mac in packet.ClientList)
        {
            if (!_devices.ContainsKey(mac) && !_pendingUsrDta.ContainsKey(mac))
            {
                // Request metadata for unknown devices
                _ = _transport.SendAsync(PacketBuilder.GetUsrDta(mac));
            }
        }
    }

    private void HandleUsrDta(JsonNode message)
    {
        var usrDta = message.Deserialize<UsrDtaPacket>();
        if (usrDta is null) return;

        var mac = usrDta.From;

        // If device already exists, just update its metadata
        if (_devices.TryGetValue(mac, out var existing))
        {
            existing.UpdateUsrDta(usrDta);
            return;
        }

        var deviceType = (EheimDeviceType)usrDta.Version;

        if (deviceType == EheimDeviceType.ExtFilter)
        {
            _pendingUsrDta[mac] = usrDta;
            // Request filter data to get the filter version (model)
            _ = _transport.SendAsync(PacketBuilder.GetFilterData(mac));

            // Also check if we already have filter version from a previous FILTER_DATA
            if (_pendingFilterVersions.TryRemove(mac, out var filterVersion))
            {
                TryCreateFilter(mac, usrDta, filterVersion);
            }
        }
        // Other device types are not supported yet — silently ignore
    }

    private void HandleFilterData(string from, JsonNode message)
    {
        // If device exists, route the message to it
        if (_devices.TryGetValue(from, out var device))
        {
            device.UpdateFromMessage(message);
            return;
        }

        // Device not yet created — extract filter version for pending creation
        var filterVersion = message["version"]?.GetValue<int>();
        if (filterVersion.HasValue)
        {
            HandleFilterDataForDiscovery(from, filterVersion.Value);
        }
    }

    private void HandleFilterDataForDiscovery(string mac, int filterVersion)
    {
        if (_devices.ContainsKey(mac)) return;

        if (_pendingUsrDta.TryRemove(mac, out var usrDta))
        {
            TryCreateFilter(mac, usrDta, filterVersion);
        }
        else
        {
            _pendingFilterVersions[mac] = filterVersion;
        }
    }

    private void TryCreateFilter(string mac, UsrDtaPacket usrDta, int filterVersion)
    {
        var filter = new EheimFilter(_transport, usrDta, filterVersion, usrDta.TankConfig);
        if (_devices.TryAdd(mac, filter))
        {
            _deviceDiscovered.OnNext(filter);
            // Request a full update now that the device is registered
            _ = filter.RequestUpdateAsync(CancellationToken.None);
        }
    }
}
