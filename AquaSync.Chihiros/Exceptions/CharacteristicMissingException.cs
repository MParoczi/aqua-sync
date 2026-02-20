namespace AquaSync.Chihiros.Exceptions;

/// <summary>
///     Thrown when a required GATT characteristic (RX or TX) cannot be resolved on the device.
/// </summary>
public class CharacteristicMissingException : ChihirosException
{
    public CharacteristicMissingException()
    {
    }

    public CharacteristicMissingException(string message) : base(message)
    {
    }

    public CharacteristicMissingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
