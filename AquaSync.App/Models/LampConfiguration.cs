namespace AquaSync.App.Models;

public sealed class LampConfiguration
{
    public Guid Id { get; set; }
    public Guid AquariumId { get; set; }

    /// <summary>12-character uppercase hex BLE address, e.g. "A1B2C3D4E5F6".</summary>
    public string BluetoothAddress { get; set; } = string.Empty;

    public string DeviceName { get; set; } = string.Empty;

    /// <summary>Empty string means the lamp has no recognized device profile (unmanaged).</summary>
    public string ModelName { get; set; } = string.Empty;

    public LampMode Mode { get; set; } = LampMode.Off;

    /// <summary>ColorChannel.ToString() string keys → brightness 0–100.</summary>
    public Dictionary<string, byte> ManualBrightness { get; set; } = [];

    public ScheduleConfiguration? Schedule { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
