using System.Text.Json.Serialization;

namespace AquaSync.Eheim.Protocol.Packets;

/// <summary>
///     Represents a FILTER_DATA message from a professionel 5e filter.
/// </summary>
internal sealed record FilterDataPacket
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("from")]
    public string From { get; init; } = "";

    // Frequency / speed
    [JsonPropertyName("minFreq")]
    public int MinFreq { get; init; }

    [JsonPropertyName("maxFreq")]
    public int MaxFreq { get; init; }

    [JsonPropertyName("maxFreqRglOff")]
    public int MaxFreqRglOff { get; init; }

    [JsonPropertyName("freq")]
    public int Freq { get; init; }

    [JsonPropertyName("freqSoll")]
    public int FreqSoll { get; init; }

    [JsonPropertyName("dfs")]
    public int Dfs { get; init; }

    [JsonPropertyName("dfsFaktor")]
    public int DfsFaktor { get; init; }

    [JsonPropertyName("sollStep")]
    public int SollStep { get; init; }

    [JsonPropertyName("rotSpeed")]
    public int RotSpeed { get; init; }

    // Mode & state
    [JsonPropertyName("pumpMode")]
    public int PumpMode { get; init; }

    [JsonPropertyName("filterActive")]
    public int FilterActive { get; init; }

    [JsonPropertyName("sync")]
    public string Sync { get; init; } = "";

    [JsonPropertyName("partnerName")]
    public string PartnerName { get; init; } = "";

    // Time / service
    [JsonPropertyName("runTime")]
    public int RunTime { get; init; }

    [JsonPropertyName("actualTime")]
    public int ActualTime { get; init; }

    [JsonPropertyName("serviceHour")]
    public int ServiceHour { get; init; }

    [JsonPropertyName("turnOffTime")]
    public int TurnOffTime { get; init; }

    [JsonPropertyName("turnTimeFeeding")]
    public int TurnTimeFeeding { get; init; }

    // Pulse mode
    [JsonPropertyName("pm_dfs_soll_high")]
    public int PmDfsSollHigh { get; init; }

    [JsonPropertyName("pm_dfs_soll_low")]
    public int PmDfsSollLow { get; init; }

    [JsonPropertyName("pm_time_high")]
    public int PmTimeHigh { get; init; }

    [JsonPropertyName("pm_time_low")]
    public int PmTimeLow { get; init; }

    // Bio (nocturnal) mode
    [JsonPropertyName("nm_dfs_soll_day")]
    public int NmDfsSollDay { get; init; }

    [JsonPropertyName("nm_dfs_soll_night")]
    public int NmDfsSollNight { get; init; }

    [JsonPropertyName("end_time_night_mode")]
    public int EndTimeNightMode { get; init; }

    [JsonPropertyName("start_time_night_mode")]
    public int StartTimeNightMode { get; init; }

    // Model identification
    [JsonPropertyName("version")]
    public int FilterVersion { get; init; }

    [JsonPropertyName("isEheim")]
    public int IsEheim { get; init; }
}
