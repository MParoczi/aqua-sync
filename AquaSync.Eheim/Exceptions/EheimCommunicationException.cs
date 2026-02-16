namespace AquaSync.Eheim.Exceptions;

/// <summary>
///     Thrown when sending a command to or receiving data from an EHEIM device fails.
/// </summary>
public class EheimCommunicationException : EheimException
{
    public EheimCommunicationException()
    {
    }

    public EheimCommunicationException(string message) : base(message)
    {
    }

    public EheimCommunicationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
