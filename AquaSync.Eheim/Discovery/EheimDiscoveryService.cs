using System.Reactive.Linq;
using Zeroconf;

namespace AquaSync.Eheim.Discovery;

/// <summary>
/// Discovers EHEIM Digital hubs on the local network using mDNS (Zeroconf).
/// Scans for the "_http._tcp.local." service type and filters for EHEIM devices.
/// </summary>
public sealed class EheimDiscoveryService : IEheimDiscoveryService
{
    private const string ServiceType = "_http._tcp.local.";
    private const string EheimServiceName = "eheimdigital";

    public IObservable<DiscoveredHub> Scan(TimeSpan timeout)
    {
        return Observable.Create<DiscoveredHub>(async (observer, ct) =>
        {
            try
            {
                var responses = await ZeroconfResolver.ResolveAsync(
                    ServiceType,
                    scanTime: timeout,
                    cancellationToken: ct);

                foreach (var host in responses)
                {
                    if (!host.DisplayName.Contains(EheimServiceName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    observer.OnNext(new DiscoveredHub(
                        Host: host.DisplayName,
                        IpAddress: host.IPAddress,
                        Name: host.DisplayName));
                }
            }
            catch (OperationCanceledException)
            {
                // Scan was cancelled — complete normally
            }

            observer.OnCompleted();
        });
    }
}
