namespace AquaSync.Chihiros.Exceptions;

/// <summary>
///     Thrown when a BLE connection or communication operation fails.
/// </summary>
public class DeviceConnectionException : ChihirosException
{
    public DeviceConnectionException()
    {
    }

    public DeviceConnectionException(string message) : base(message)
    {
    }

    public DeviceConnectionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
