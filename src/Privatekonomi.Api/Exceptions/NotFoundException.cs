namespace Privatekonomi.Api.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public NotFoundException(string resourceName, object key) 
        : base($"{resourceName} with key '{key}' was not found.")
    {
    }
}
