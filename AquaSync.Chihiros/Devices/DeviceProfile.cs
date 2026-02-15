namespace AquaSync.Chihiros.Devices;

/// <summary>
/// Describes a Chihiros device model: its name, BLE name codes, and color channel layout.
/// </summary>
public sealed record DeviceProfile(
    string ModelName,
    IReadOnlyList<string> ModelCodes,
    IReadOnlyList<ChannelMapping> Channels);
