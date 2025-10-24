namespace Privatekonomi.Web.Models;

/// <summary>
/// Represents a ProblemDetails response according to RFC 7807.
/// </summary>
public class ProblemDetailsResponse
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    public string? TraceId { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
