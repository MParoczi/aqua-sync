namespace AquaSync.Chihiros.Exceptions;

/// <summary>
/// Thrown when a BLE device cannot be found at the specified address.
/// </summary>
public class DeviceNotFoundException : ChihirosException
{
    public DeviceNotFoundException() { }
    public DeviceNotFoundException(string message) : base(message) { }
    public DeviceNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
