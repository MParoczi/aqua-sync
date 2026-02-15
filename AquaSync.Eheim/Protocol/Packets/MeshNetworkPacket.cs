using System.Text.Json.Serialization;

namespace AquaSync.Eheim.Protocol.Packets;

/// <summary>
/// Represents a MESH_NETWORK message containing all connected device MAC addresses.
/// </summary>
internal sealed record MeshNetworkPacket
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("to")]
    public string To { get; init; } = "";

    [JsonPropertyName("from")]
    public string? From { get; init; }

    [JsonPropertyName("clientList")]
    public List<string> ClientList { get; init; } = [];
}
