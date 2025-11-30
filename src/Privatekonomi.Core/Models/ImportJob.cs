namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents an import job for tracking file imports (CSV/OFX).
/// </summary>
public class ImportJob
{
    public int ImportJobId { get; set; }
    
    /// <summary>
    /// Status of the import job: Pending, Processing, Completed, Failed
    /// </summary>
    public string Status { get; set; } = "Pending";
    
    /// <summary>
    /// The bank name associated with the import (e.g., "ICA-banken", "Swedbank")
    /// </summary>
    public string BankName { get; set; } = string.Empty;
    
    /// <summary>
    /// The file type: CSV or OFX
    /// </summary>
    public string FileType { get; set; } = "CSV";
    
    /// <summary>
    /// Original filename uploaded by the user
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Total number of rows in the file
    /// </summary>
    public int TotalRows { get; set; }
    
    /// <summary>
    /// Number of transactions successfully imported
    /// </summary>
    public int ImportedCount { get; set; }
    
    /// <summary>
    /// Number of duplicate transactions found
    /// </summary>
    public int DuplicateCount { get; set; }
    
    /// <summary>
    /// Number of rows with errors
    /// </summary>
    public int ErrorCount { get; set; }
    
    /// <summary>
    /// Error messages in JSON format
    /// </summary>
    public string? ErrorMessages { get; set; }
    
    /// <summary>
    /// Source of the import: 'manual' for user uploads
    /// </summary>
    public string Source { get; set; } = "manual";
    
    /// <summary>
    /// User who initiated the import
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// When the import job was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// When the import job was started
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// When the import job was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public ApplicationUser? User { get; set; }
}
