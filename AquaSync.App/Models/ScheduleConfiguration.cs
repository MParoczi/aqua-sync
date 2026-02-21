using AquaSync.Chihiros.Scheduling;

namespace AquaSync.App.Models;

public sealed class ScheduleConfiguration
{
    public TimeOnly Sunrise { get; set; }
    public TimeOnly Sunset { get; set; }
    public int RampUpMinutes { get; set; }
    public Dictionary<string, byte> ChannelBrightness { get; set; } = [];
    public Weekday ActiveDays { get; set; }
}
