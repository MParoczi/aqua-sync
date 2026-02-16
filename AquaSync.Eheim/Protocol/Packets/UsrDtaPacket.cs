using System.Text.Json.Serialization;

namespace AquaSync.Eheim.Protocol.Packets;

/// <summary>
///     Represents a USRDTA message containing device metadata and user configuration.
/// </summary>
internal sealed record UsrDtaPacket
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("from")]
    public string From { get; init; } = "";

    [JsonPropertyName("to")]
    public string To { get; init; } = "";

    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("aqName")]
    public string AquariumName { get; init; } = "";

    [JsonPropertyName("version")]
    public int Version { get; init; }

    [JsonPropertyName("revision")]
    public List<int> Revision { get; init; } = [];

    [JsonPropertyName("tankconfig")]
    public string TankConfig { get; init; } = "";

    [JsonPropertyName("unit")]
    public int Unit { get; init; }

    [JsonPropertyName("timezone")]
    public int Timezone { get; init; }

    [JsonPropertyName("sysLED")]
    public int SysLed { get; init; }

    [JsonPropertyName("host")]
    public string Host { get; init; } = "";

    [JsonPropertyName("language")]
    public string Language { get; init; } = "";

    [JsonPropertyName("emailAddr")]
    public string? EmailAddress { get; init; }

    [JsonPropertyName("usrName")]
    public string? UserName { get; init; }

    [JsonPropertyName("firmwareAvailable")]
    public int FirmwareAvailable { get; init; }

    [JsonPropertyName("meshing")]
    public int Meshing { get; init; }

    [JsonPropertyName("netmode")]
    public string NetMode { get; init; } = "";
}
