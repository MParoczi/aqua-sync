namespace AquaSync.Eheim.Devices.Enums;

/// <summary>
/// EHEIM Digital device type identifiers.
/// Corresponds to the "version" field in USRDTA packets.
/// </summary>
internal enum EheimDeviceType
{
    Undefined = 0,
    ExtFilter = 4,
    ExtHeater = 5,
    Feeder = 6,
    PhControl = 9,
    ClassicLedCtrl = 17,
    ClassicVario = 18
}
