namespace AquaSync.Eheim.Discovery;

/// <summary>
/// Represents an EHEIM hub found via mDNS/Zeroconf discovery.
/// </summary>
public sealed record DiscoveredHub(string Host, string IpAddress, string Name);
