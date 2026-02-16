using System.Text.Json.Serialization;

namespace AquaSync.App.Models;

/// <summary>
///     The type of aquarium environment.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AquariumType
{
    Freshwater,
    Saltwater,
    Brackish
}
