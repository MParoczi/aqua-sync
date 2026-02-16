using System.Text.Json.Serialization;

namespace AquaSync.App.Models;

/// <summary>
///     The type of substrate or additive layer.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubstrateType
{
    Substrate,
    Additive,
    SoilCap
}
