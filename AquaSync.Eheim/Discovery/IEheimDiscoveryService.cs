namespace AquaSync.Eheim.Discovery;

/// <summary>
/// Discovers EHEIM Digital hubs on the local network via mDNS/Zeroconf.
/// </summary>
public interface IEheimDiscoveryService
{
    IObservable<DiscoveredHub> Scan(TimeSpan timeout);
}
