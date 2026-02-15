using System.Collections.Concurrent;
using AquaSync.Chihiros.Devices;
using AquaSync.Chihiros.Protocol;
using Windows.Devices.Bluetooth.Advertisement;

namespace AquaSync.Chihiros.Discovery;

/// <summary>
/// Scans for Chihiros BLE devices using <see cref="BluetoothLEAdvertisementWatcher"/>.
/// </summary>
public sealed class DeviceScanner : IDeviceScanner
{
    /// <summary>
    /// Scan for Chihiros devices for the given duration.
    /// Devices are reported via <paramref name="progress"/> as they are found and returned as a list when complete.
    /// </summary>
    public async Task<IReadOnlyList<DiscoveredDevice>> ScanAsync(
        TimeSpan timeout,
        IProgress<DiscoveredDevice>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var discovered = new ConcurrentDictionary<ulong, DiscoveredDevice>();
        var tcs = new TaskCompletionSource();

        var watcher = new BluetoothLEAdvertisementWatcher
        {
            ScanningMode = BluetoothLEScanningMode.Active
        };

        // Filter by UART service UUID to find Nordic UART devices
        watcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(UartConstants.ServiceUuid);

        watcher.Received += (_, args) =>
        {
            var name = args.Advertisement.LocalName;
            if (string.IsNullOrEmpty(name))
                return;

            var address = args.BluetoothAddress;
            var profile = DeviceProfiles.MatchFromName(name);

            var device = new DiscoveredDevice(address, name, args.RawSignalStrengthInDBm, profile);

            if (discovered.TryAdd(address, device))
            {
                progress?.Report(device);
            }
        };

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        using var registration = cts.Token.Register(() => tcs.TrySetResult());

        watcher.Start();

        try
        {
            await tcs.Task.ConfigureAwait(false);
        }
        finally
        {
            watcher.Stop();
        }

        return discovered.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Scan without the UART service UUID filter, matching devices by known BLE name prefixes instead.
    /// Useful when devices do not advertise service UUIDs in their advertisement packets.
    /// </summary>
    public async Task<IReadOnlyList<DiscoveredDevice>> ScanByNameAsync(
        TimeSpan timeout,
        IProgress<DiscoveredDevice>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var discovered = new ConcurrentDictionary<ulong, DiscoveredDevice>();
        var tcs = new TaskCompletionSource();

        var watcher = new BluetoothLEAdvertisementWatcher
        {
            ScanningMode = BluetoothLEScanningMode.Active
        };

        watcher.Received += (_, args) =>
        {
            var name = args.Advertisement.LocalName;
            if (string.IsNullOrEmpty(name))
                return;

            var profile = DeviceProfiles.MatchFromName(name);
            if (profile is null)
                return;

            var address = args.BluetoothAddress;
            var device = new DiscoveredDevice(address, name, args.RawSignalStrengthInDBm, profile);

            if (discovered.TryAdd(address, device))
            {
                progress?.Report(device);
            }
        };

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        using var registration = cts.Token.Register(() => tcs.TrySetResult());

        watcher.Start();

        try
        {
            await tcs.Task.ConfigureAwait(false);
        }
        finally
        {
            watcher.Stop();
        }

        return discovered.Values.ToList().AsReadOnly();
    }
}
