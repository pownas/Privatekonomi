namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents an audit log entry for security-critical operations
/// </summary>
public class AuditLog
{
    public int AuditLogId { get; set; }
    
    /// <summary>
    /// Type of action performed (e.g., "BankConnectionCreated", "BankConnectionDeleted")
    /// </summary>
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity type affected (e.g., "BankConnection")
    /// </summary>
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the entity affected
    /// </summary>
    public int? EntityId { get; set; }
    
    /// <summary>
    /// User who performed the action
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// Details about the action
    /// </summary>
    public string? Details { get; set; }
    
    /// <summary>
    /// IP address of the request
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// When the action was performed
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
