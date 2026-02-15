namespace AquaSync.Chihiros.Exceptions;

/// <summary>
/// Base exception for all AquaSync.Chihiros errors.
/// </summary>
public class ChihirosException : Exception
{
    public ChihirosException() { }
    public ChihirosException(string message) : base(message) { }
    public ChihirosException(string message, Exception innerException) : base(message, innerException) { }
}
