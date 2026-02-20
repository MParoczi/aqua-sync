using System.Text.Json.Serialization;

namespace AquaSync.App.Models;

/// <summary>
///     Application theme preference.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AppTheme
{
    System,
    Light,
    Dark
}
