namespace Privatekonomi.Api.Exceptions;

/// <summary>
/// Exception thrown when a request is invalid or malformed.
/// </summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }

    public BadRequestException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
