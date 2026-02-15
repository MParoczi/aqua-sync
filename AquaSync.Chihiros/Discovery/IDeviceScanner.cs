namespace AquaSync.Chihiros.Discovery;

/// <summary>
/// Scans for Chihiros BLE devices.
/// </summary>
public interface IDeviceScanner
{
    /// <summary>
    /// Scan for Chihiros devices by UART service UUID for the given duration.
    /// </summary>
    Task<IReadOnlyList<DiscoveredDevice>> ScanAsync(
        TimeSpan timeout,
        IProgress<DiscoveredDevice>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Scan for Chihiros devices by known BLE name prefixes for the given duration.
    /// </summary>
    Task<IReadOnlyList<DiscoveredDevice>> ScanByNameAsync(
        TimeSpan timeout,
        IProgress<DiscoveredDevice>? progress = null,
        CancellationToken cancellationToken = default);
}
