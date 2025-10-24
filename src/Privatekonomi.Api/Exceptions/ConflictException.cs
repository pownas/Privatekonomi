namespace Privatekonomi.Api.Exceptions;

/// <summary>
/// Exception thrown when a resource conflict occurs (e.g., concurrent modification).
/// Returns HTTP 409 Conflict status code.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
