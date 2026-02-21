namespace AquaSync.Chihiros.Devices;

/// <summary>
///     Registry of all known Chihiros device profiles and BLE name-matching logic.
/// </summary>
public static class DeviceProfiles
{
    // --- White-only devices ---

    public static DeviceProfile AII { get; } = new(
        "A II",
        ["DYNA2", "DYNA2N"],
        [new ChannelMapping(ColorChannel.White, 0)]);

    public static DeviceProfile CII { get; } = new(
        "C II",
        ["DYNC2N"],
        [new ChannelMapping(ColorChannel.White, 0)]);

    public static DeviceProfile GenericWhite { get; } = new(
        "Generic White",
        [],
        [new ChannelMapping(ColorChannel.White, 0)]);

    // --- Dual-channel devices ---

    public static DeviceProfile ZLightTiny { get; } = new(
        "Z Light TINY",
        ["DYSSD", "DYZSD"],
        [new ChannelMapping(ColorChannel.White, 0), new ChannelMapping(ColorChannel.Warm, 1)]);

    public static DeviceProfile TinyTerrariumEgg { get; } = new(
        "Tiny Terrarium Egg",
        ["DYDD"],
        [new ChannelMapping(ColorChannel.Red, 0), new ChannelMapping(ColorChannel.Green, 1)]);

    // --- RGB devices ---

    public static DeviceProfile CIIRGB { get; } = new(
        "C II RGB",
        ["DYNCRGP", "DYNCRGB"],
        [new ChannelMapping(ColorChannel.Red, 0), new ChannelMapping(ColorChannel.Green, 1), new ChannelMapping(ColorChannel.Blue, 2)]);

    public static DeviceProfile WRGBII { get; } = new(
        "WRGB II",
        ["DYNWRGB", "DYNW30", "DYNW45", "DYNW60", "DYNW90", "DYNW12P"],
        [new ChannelMapping(ColorChannel.Red, 0), new ChannelMapping(ColorChannel.Green, 1), new ChannelMapping(ColorChannel.Blue, 2)]);

    public static DeviceProfile WRGBIISlim { get; } = new(
        "WRGB II Slim",
        ["DYSILN", "DYSL30", "DYSL45", "DYSL60", "DYSL90", "DYSL120", "DYSL12"],
        [new ChannelMapping(ColorChannel.Red, 0), new ChannelMapping(ColorChannel.Green, 1), new ChannelMapping(ColorChannel.Blue, 2)]);

    public static DeviceProfile GenericRGB { get; } = new(
        "Generic RGB",
        [],
        [new ChannelMapping(ColorChannel.Red, 0), new ChannelMapping(ColorChannel.Green, 1), new ChannelMapping(ColorChannel.Blue, 2)]);

    // --- WRGB devices (4 channels) ---

    public static DeviceProfile WRGBIIPro { get; } = new(
        "WRGB II Pro",
        ["DYWPRO30", "DYWPRO45", "DYWPRO60", "DYWPRO80", "DYWPRO90", "DYWPR120"],
        [
            new ChannelMapping(ColorChannel.Red, 0), new ChannelMapping(ColorChannel.Green, 1), new ChannelMapping(ColorChannel.Blue, 2),
            new ChannelMapping(ColorChannel.White, 3)
        ]);

    public static DeviceProfile UniversalWRGB { get; } = new(
        "Universal WRGB",
        ["DYU550", "DYU600", "DYU700", "DYU800", "DYU920", "DYU1000", "DYU1200", "DYU1500"],
        [
            new ChannelMapping(ColorChannel.Red, 0), new ChannelMapping(ColorChannel.Green, 1), new ChannelMapping(ColorChannel.Blue, 2),
            new ChannelMapping(ColorChannel.White, 3)
        ]);

    public static DeviceProfile GenericWRGB { get; } = new(
        "Generic WRGB",
        [],
        [
            new ChannelMapping(ColorChannel.Red, 0), new ChannelMapping(ColorChannel.Green, 1), new ChannelMapping(ColorChannel.Blue, 2),
            new ChannelMapping(ColorChannel.White, 3)
        ]);

    // --- Ambiguous devices (user picks white/RGB/WRGB at config time) ---

    public static DeviceProfile Commander1 { get; } = new(
        "Commander 1",
        ["DYCOM"],
        [new ChannelMapping(ColorChannel.White, 0)]);

    public static DeviceProfile Commander4 { get; } = new(
        "Commander 4",
        ["DYLED"],
        [new ChannelMapping(ColorChannel.White, 0)]);

    // --- Fallback ---

    public static DeviceProfile Fallback { get; } = new(
        "Fallback",
        [],
        [new ChannelMapping(ColorChannel.White, 0)]);

    /// <summary>
    ///     All known device profiles.
    /// </summary>
    public static IReadOnlyList<DeviceProfile> All { get; } =
    [
        AII, CII, ZLightTiny, TinyTerrariumEgg,
        CIIRGB, WRGBII, WRGBIISlim, WRGBIIPro, UniversalWRGB,
        Commander1, Commander4,
        GenericWhite, GenericRGB, GenericWRGB,
        Fallback
    ];

    // Pre-sorted (code, profile) pairs: longest code first for correct prefix matching.
    // Must be declared after All so it is initialized after all profiles are ready.
    private static readonly (string Code, DeviceProfile Profile)[] _sortedCodeMap = Build_sortedCodeMap();

    private static (string Code, DeviceProfile Profile)[] Build_sortedCodeMap()
    {
        var pairs = new List<(string Code, DeviceProfile Profile)>();
        foreach (var profile in All)
        foreach (var code in profile.ModelCodes)
            pairs.Add((code, profile));

        // Sort by descending code length so longer prefixes match first (e.g., "DYWPRO30" before "DYW")
        pairs.Sort((a, b) => b.Code.Length.CompareTo(a.Code.Length));
        return pairs.ToArray();
    }

    /// <summary>
    ///     Match a BLE local name to a known device profile using longest-prefix matching.
    ///     Returns <c>null</c> if no known profile matches.
    /// </summary>
    public static DeviceProfile? MatchFromName(string bleLocalName)
    {
        if (string.IsNullOrEmpty(bleLocalName))
            return null;

        foreach (var (code, profile) in _sortedCodeMap)
            if (bleLocalName.StartsWith(code, StringComparison.OrdinalIgnoreCase))
                return profile;

        return null;
    }
}
