using AquaSync.Chihiros.Devices;

namespace AquaSync.Chihiros.Scheduling;

/// <summary>
///     Represents an auto-mode lighting schedule with per-channel brightness.
/// </summary>
/// <param name="Sunrise">Time the light begins turning on.</param>
/// <param name="Sunset">Time the light begins turning off.</param>
/// <param name="ChannelBrightness">Brightness (0–100) per color channel.</param>
/// <param name="RampUpMinutes">Minutes to ramp from 0 to max brightness (0–150).</param>
/// <param name="Weekdays">Which days the schedule is active.</param>
public sealed record LightSchedule(
    TimeOnly Sunrise,
    TimeOnly Sunset,
    IReadOnlyDictionary<ColorChannel, byte> ChannelBrightness,
    int RampUpMinutes,
    Weekday Weekdays);
