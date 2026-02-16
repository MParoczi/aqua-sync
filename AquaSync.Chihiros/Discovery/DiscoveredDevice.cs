using AquaSync.Chihiros.Devices;

namespace AquaSync.Chihiros.Discovery;

/// <summary>
///     Represents a Chihiros device found during BLE scanning.
/// </summary>
/// <param name="BluetoothAddress">Raw BLE address (used to connect via <see cref="ChihirosDevice" />).</param>
/// <param name="Name">BLE local name advertised by the device.</param>
/// <param name="Rssi">Signal strength at time of discovery.</param>
/// <param name="MatchedProfile">Auto-detected profile, or <c>null</c> if the device model is unknown.</param>
public sealed record DiscoveredDevice(
    ulong BluetoothAddress,
    string Name,
    short Rssi,
    DeviceProfile? MatchedProfile);
