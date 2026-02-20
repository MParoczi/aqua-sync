namespace AquaSync.Eheim.Exceptions;

/// <summary>
///     Thrown when a connection to the EHEIM hub cannot be established or times out.
/// </summary>
public class EheimConnectionException : EheimException
{
    public EheimConnectionException()
    {
    }

    public EheimConnectionException(string message) : base(message)
    {
    }

    public EheimConnectionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
