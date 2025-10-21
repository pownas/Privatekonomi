using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for logging security-critical operations
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Logs a security-critical action
    /// </summary>
    Task LogAsync(string action, string entityType, int? entityId = null, string? details = null, string? userId = null, string? ipAddress = null);
}
