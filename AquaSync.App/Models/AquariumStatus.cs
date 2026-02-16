using System.Text.Json.Serialization;

namespace AquaSync.App.Models;

/// <summary>
/// The lifecycle status of an aquarium profile.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AquariumStatus
{
    Active,
    Archived,
}
