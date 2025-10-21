using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for logging security-critical operations
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly PrivatekonomyContext _context;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(PrivatekonomyContext context, ILogger<AuditLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(string action, string entityType, int? entityId = null, string? details = null, string? userId = null, string? ipAddress = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                UserId = userId,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Audit log created: {Action} on {EntityType} {EntityId} by {UserId} from {IpAddress}",
                action, entityType, entityId, userId ?? "Unknown", ipAddress ?? "Unknown");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log for action {Action}", action);
            // Don't throw - we don't want audit logging failures to break the application
        }
    }
}
