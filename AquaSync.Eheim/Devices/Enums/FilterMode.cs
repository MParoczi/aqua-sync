namespace AquaSync.Eheim.Devices.Enums;

/// <summary>
///     Filter operation modes for the professionel 5e.
///     Values match the protocol bitmask values.
/// </summary>
public enum FilterMode
{
    Manual = 16,
    ConstantFlow = 1,
    Pulse = 8,
    Bio = 4
}
