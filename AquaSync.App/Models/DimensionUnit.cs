using System.Text.Json.Serialization;

namespace AquaSync.App.Models;

/// <summary>
///     Unit of measurement for aquarium dimensions and substrate layer depth.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DimensionUnit
{
    Centimeters,
    Inches
}
