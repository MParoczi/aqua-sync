namespace AquaSync.Chihiros.Devices;

/// <summary>
/// Maps a logical <see cref="ColorChannel"/> to a protocol channel ID (0–3)
/// that is sent over BLE.
/// </summary>
public readonly record struct ChannelMapping(ColorChannel Channel, byte ProtocolChannelId);
