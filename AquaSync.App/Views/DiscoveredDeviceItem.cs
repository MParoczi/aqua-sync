using AquaSync.Chihiros.Discovery;

namespace AquaSync.App.Views;

public sealed class DiscoveredDeviceItem
{
    public DiscoveredDevice Device { get; set; } = null!;
    public bool IsAvailable { get; set; }

    public string ModelDisplayName =>
        string.IsNullOrEmpty(Device.MatchedProfile?.ModelName)
            ? "Unknown Model"
            : Device.MatchedProfile.ModelName;

    public double ContainerOpacity => IsAvailable ? 1.0 : 0.5;
}
