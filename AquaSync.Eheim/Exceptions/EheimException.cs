namespace AquaSync.Eheim.Exceptions;

/// <summary>
///     Base exception for all AquaSync.Eheim errors.
/// </summary>
public class EheimException : Exception
{
    public EheimException()
    {
    }

    public EheimException(string message) : base(message)
    {
    }

    public EheimException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
