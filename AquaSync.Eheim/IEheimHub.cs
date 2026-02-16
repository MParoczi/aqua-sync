using AquaSync.Eheim.Devices;

namespace AquaSync.Eheim;

/// <summary>
///     Manages the connection to an EHEIM Digital hub and exposes discovered devices.
/// </summary>
public interface IEheimHub : IAsyncDisposable
{
    bool IsConnected { get; }
    IObservable<IEheimDevice> DeviceDiscovered { get; }
    IReadOnlyDictionary<string, IEheimDevice> Devices { get; }
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
