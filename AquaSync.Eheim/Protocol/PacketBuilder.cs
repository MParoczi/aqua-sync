using System.Text.Json.Nodes;
using AquaSync.Eheim.Protocol.Packets;

namespace AquaSync.Eheim.Protocol;

/// <summary>
///     Builds JSON command packets for the EHEIM WebSocket protocol.
/// </summary>
internal static class PacketBuilder
{
    private const string UserFrom = "USER";

    public static JsonObject GetUsrDta(string to)
    {
        return new JsonObject
        {
            ["title"] = MessageTitle.GetUsrDta,
            ["to"] = to,
            ["from"] = UserFrom
        };
    }

    public static JsonObject GetFilterData(string macAddress)
    {
        return new JsonObject
        {
            ["title"] = MessageTitle.GetFilterData,
            ["to"] = macAddress,
            ["from"] = UserFrom
        };
    }

    public static JsonObject SetFilterPump(string macAddress, bool active)
    {
        return new JsonObject
        {
            ["title"] = MessageTitle.SetFilterPump,
            ["to"] = macAddress,
            ["active"] = active ? 1 : 0,
            ["from"] = UserFrom
        };
    }

    public static JsonObject StartManualMode(string macAddress, int frequencyRaw)
    {
        return new JsonObject
        {
            ["title"] = MessageTitle.StartFilterNormalModeWithoutComp,
            ["to"] = macAddress,
            ["frequency"] = frequencyRaw,
            ["from"] = UserFrom
        };
    }

    public static JsonObject StartConstantFlowMode(string macAddress, int flowRateIndex)
    {
        return new JsonObject
        {
            ["title"] = MessageTitle.StartFilterNormalModeWithComp,
            ["to"] = macAddress,
            ["flow_rate"] = flowRateIndex,
            ["from"] = UserFrom
        };
    }

    public static JsonObject StartPulseMode(
        string macAddress,
        int dfsSollHigh, int dfsSollLow,
        int timeHigh, int timeLow)
    {
        return new JsonObject
        {
            ["title"] = MessageTitle.StartFilterPulseMode,
            ["to"] = macAddress,
            ["dfs_soll_high"] = dfsSollHigh,
            ["dfs_soll_low"] = dfsSollLow,
            ["time_high"] = timeHigh,
            ["time_low"] = timeLow,
            ["from"] = UserFrom
        };
    }

    public static JsonObject StartBioMode(
        string macAddress,
        int dfsSollDay, int dfsSollNight,
        int endTimeNightMode, int startTimeNightMode,
        string sync, string partnerName)
    {
        return new JsonObject
        {
            ["title"] = MessageTitle.StartNocturnalMode,
            ["to"] = macAddress,
            ["dfs_soll_day"] = dfsSollDay,
            ["dfs_soll_night"] = dfsSollNight,
            ["end_time_night_mode"] = endTimeNightMode,
            ["start_time_night_mode"] = startTimeNightMode,
            ["sync"] = sync,
            ["partnerName"] = partnerName,
            ["from"] = UserFrom
        };
    }

    public static JsonObject SetUsrDta(UsrDtaPacket current, JsonObject overrides)
    {
        var packet = new JsonObject
        {
            ["title"] = current.Title,
            ["from"] = current.From,
            ["to"] = current.To,
            ["name"] = current.Name,
            ["aqName"] = current.AquariumName,
            ["version"] = current.Version,
            ["tankconfig"] = current.TankConfig,
            ["unit"] = current.Unit,
            ["timezone"] = current.Timezone,
            ["sysLED"] = current.SysLed,
            ["host"] = current.Host,
            ["language"] = current.Language,
            ["netmode"] = current.NetMode,
            ["meshing"] = current.Meshing,
            ["firmwareAvailable"] = current.FirmwareAvailable
        };

        foreach (var (key, value) in overrides) packet[key] = value?.DeepClone();

        return packet;
    }
}
