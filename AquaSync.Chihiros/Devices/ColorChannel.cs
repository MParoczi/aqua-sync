namespace AquaSync.Chihiros.Devices;

/// <summary>
///     Logical color channel types supported by Chihiros LED devices.
///     These are decoupled from protocol channel IDs — see <see cref="ChannelMapping" />.
/// </summary>
public enum ColorChannel
{
    White,
    Warm,
    Red,
    Green,
    Blue
}
