namespace Privatekonomi.Core.Models;

/// <summary>
/// Tracks delivery of monthly reports via different channels
/// </summary>
public class ReportDelivery
{
    public int ReportDeliveryId { get; set; }
    
    /// <summary>
    /// The report this delivery is for
    /// </summary>
    public int MonthlyReportId { get; set; }
    public MonthlyReport? MonthlyReport { get; set; }
    
    /// <summary>
    /// Delivery method: Email, InApp
    /// </summary>
    public DeliveryMethod DeliveryMethod { get; set; }
    
    /// <summary>
    /// Status of delivery: Pending, Sent, Delivered, Failed
    /// </summary>
    public DeliveryStatus Status { get; set; }
    
    /// <summary>
    /// Email address if delivery method is email
    /// </summary>
    public string? EmailAddress { get; set; }
    
    /// <summary>
    /// When the delivery was scheduled
    /// </summary>
    public DateTime ScheduledAt { get; set; }
    
    /// <summary>
    /// When the delivery was sent (null if not yet sent)
    /// </summary>
    public DateTime? SentAt { get; set; }
    
    /// <summary>
    /// When the report was opened/read (for tracking)
    /// </summary>
    public DateTime? OpenedAt { get; set; }
    
    /// <summary>
    /// Error message if delivery failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Report delivery method
/// </summary>
public enum DeliveryMethod
{
    Email = 0,
    InApp = 1
}

/// <summary>
/// Status of report delivery
/// </summary>
public enum DeliveryStatus
{
    Pending = 0,
    Sent = 1,
    Delivered = 2,
    Failed = 3,
    Opened = 4
}
