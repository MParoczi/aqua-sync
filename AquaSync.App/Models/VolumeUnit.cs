using System.Text.Json.Serialization;

namespace AquaSync.App.Models;

/// <summary>
/// Unit of measurement for aquarium volume.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VolumeUnit
{
    Liters,
    Gallons,
}
