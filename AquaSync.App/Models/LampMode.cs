using System.Text.Json.Serialization;

namespace AquaSync.App.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LampMode
{
    Off,
    Manual,
    Automatic,
}
