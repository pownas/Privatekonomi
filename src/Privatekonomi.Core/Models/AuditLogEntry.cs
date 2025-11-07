namespace Privatekonomi.Core.Models;

/// <summary>
/// Enhanced audit log entry for RBAC events and general system auditing
/// </summary>
public class AuditLogEntry
{
    public int AuditLogEntryId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Vem (Who)
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    
    // Vad (What)
    public string Action { get; set; } = string.Empty;
    public AuditCategory Category { get; set; }
    public AuditSeverity Severity { get; set; }
    
    // Var (Where)
    public int? HouseholdId { get; set; }
    public string? ResourceType { get; set; }
    public int? ResourceId { get; set; }
    
    // Detaljer (Details)
    public string? Details { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    
    // Metadata
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Categories for audit log events
/// </summary>
public enum AuditCategory
{
    RoleAndPermission,
    AccessAttempt,
    Membership,
    DataModification,
    SystemEvent
}

/// <summary>
/// Severity levels for audit events
/// </summary>
public enum AuditSeverity
{
    Low,
    Medium,
    High,
    Critical
}
