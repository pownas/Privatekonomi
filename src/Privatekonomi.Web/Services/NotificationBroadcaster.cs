using Microsoft.AspNetCore.SignalR;
using Privatekonomi.Core.Models;
using Privatekonomi.Web.Hubs;

namespace Privatekonomi.Web.Services;

/// <summary>
/// Implementation of notification broadcaster using SignalR
/// </summary>
public class NotificationBroadcaster : INotificationBroadcaster
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationBroadcaster> _logger;

    public NotificationBroadcaster(
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationBroadcaster> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task BroadcastNotificationAsync(string userId, Notification notification)
    {
        try
        {
            // Send to specific user's connections
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification);
            
            _logger.LogInformation("Broadcasted notification {NotificationId} to user {UserId}", 
                notification.NotificationId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast notification {NotificationId} to user {UserId}", 
                notification.NotificationId, userId);
        }
    }

    public async Task UpdateUnreadCountAsync(string userId, int count)
    {
        try
        {
            await _hubContext.Clients.User(userId).SendAsync("UpdateUnreadCount", count);
            
            _logger.LogInformation("Updated unread count to {Count} for user {UserId}", count, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update unread count for user {UserId}", userId);
        }
    }
}
