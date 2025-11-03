using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;

namespace Privatekonomi.Web.Services;

/// <summary>
/// Enhanced notification service that broadcasts notifications via SignalR
/// </summary>
public class RealtimeNotificationService : INotificationService
{
    private readonly INotificationService _innerService;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<RealtimeNotificationService> _logger;

    public RealtimeNotificationService(
        INotificationService innerService,
        INotificationBroadcaster broadcaster,
        ILogger<RealtimeNotificationService> logger)
    {
        _innerService = innerService;
        _broadcaster = broadcaster;
        _logger = logger;
    }

    public async Task<Notification> SendNotificationAsync(
        string userId,
        SystemNotificationType type,
        string title,
        string message,
        NotificationPriority priority = NotificationPriority.Normal,
        string? data = null,
        string? actionUrl = null)
    {
        // Create the notification using the inner service
        var notification = await _innerService.SendNotificationAsync(
            userId, type, title, message, priority, data, actionUrl);

        // Broadcast to connected clients if the notification was sent (has SentAt)
        if (notification.SentAt.HasValue)
        {
            try
            {
                await _broadcaster.BroadcastNotificationAsync(userId, notification);
                
                // Also update the unread count
                var unreadCount = await _innerService.GetUnreadCountAsync(userId);
                await _broadcaster.UpdateUnreadCountAsync(userId, unreadCount);
                
                _logger.LogInformation("Broadcasted notification {NotificationId} to user {UserId}", 
                    notification.NotificationId, userId);
            }
            catch (Exception ex)
            {
                // Don't fail if broadcasting fails - notification is still saved
                _logger.LogError(ex, "Failed to broadcast notification {NotificationId} to user {UserId}", 
                    notification.NotificationId, userId);
            }
        }

        return notification;
    }

    public Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
        => _innerService.GetUserNotificationsAsync(userId, unreadOnly);

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        await _innerService.MarkAsReadAsync(notificationId, userId);
        
        // Update unread count for the user
        try
        {
            var count = await _innerService.GetUnreadCountAsync(userId);
            await _broadcaster.UpdateUnreadCountAsync(userId, count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update unread count for user {UserId}", userId);
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await _innerService.MarkAllAsReadAsync(userId);
        
        // Update unread count to 0
        try
        {
            await _broadcaster.UpdateUnreadCountAsync(userId, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update unread count for user {UserId}", userId);
        }
    }

    public async Task DeleteNotificationAsync(int notificationId, string userId)
    {
        await _innerService.DeleteNotificationAsync(notificationId, userId);
        
        // Update unread count for the user
        try
        {
            var count = await _innerService.GetUnreadCountAsync(userId);
            await _broadcaster.UpdateUnreadCountAsync(userId, count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update unread count for user {UserId}", userId);
        }
    }

    public Task<int> GetUnreadCountAsync(string userId)
        => _innerService.GetUnreadCountAsync(userId);

    public Task<bool> IsDoNotDisturbActiveAsync(string userId)
        => _innerService.IsDoNotDisturbActiveAsync(userId);

    public Task SendDigestNotificationsAsync()
        => _innerService.SendDigestNotificationsAsync();
}
