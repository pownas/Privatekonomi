namespace Privatekonomi.Core.Models;

/// <summary>
/// Result of a bulk operation on transactions
/// </summary>
public class BulkOperationResult
{
    /// <summary>
    /// Number of successfully processed items
    /// </summary>
    public int SuccessCount { get; set; }
    
    /// <summary>
    /// Number of failed items
    /// </summary>
    public int FailureCount { get; set; }
    
    /// <summary>
    /// Total number of items attempted
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// List of error messages for failed operations
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// IDs of successfully processed transactions
    /// </summary>
    public List<int> SuccessfulIds { get; set; } = new();
    
    /// <summary>
    /// IDs of failed transactions
    /// </summary>
    public List<int> FailedIds { get; set; } = new();
    
    /// <summary>
    /// Operation type identifier for undo functionality
    /// </summary>
    public string? OperationType { get; set; }
    
    /// <summary>
    /// Unique identifier for this bulk operation (for undo)
    /// </summary>
    public string? OperationId { get; set; }
    
    /// <summary>
    /// Whether the operation was successful overall
    /// </summary>
    public bool IsSuccess => FailureCount == 0 && SuccessCount > 0;
    
    /// <summary>
    /// Whether the operation was partially successful
    /// </summary>
    public bool IsPartialSuccess => SuccessCount > 0 && FailureCount > 0;
}
