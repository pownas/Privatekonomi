using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using System.Security.Claims;

namespace Privatekonomi.Web.Hubs;

/// <summary>
/// SignalR hub for real-time notification delivery
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(
        INotificationService notificationService,
        ILogger<NotificationHub> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            _logger.LogInformation("User {UserId} connected to notification hub", userId);
            
            // Send current unread count to the newly connected client
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            await Clients.Caller.SendAsync("UpdateUnreadCount", unreadCount);
        }
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            _logger.LogInformation("User {UserId} disconnected from notification hub", userId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    public async Task MarkAsRead(int notificationId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return;
        }

        try
        {
            await _notificationService.MarkAsReadAsync(notificationId, userId);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            
            // Update the client's unread count
            await Clients.Caller.SendAsync("UpdateUnreadCount", unreadCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", 
                notificationId, userId);
        }
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    public async Task MarkAllAsRead()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return;
        }

        try
        {
            await _notificationService.MarkAllAsReadAsync(userId);
            
            // Update the client's unread count to 0
            await Clients.Caller.SendAsync("UpdateUnreadCount", 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Delete a notification
    /// </summary>
    public async Task DeleteNotification(int notificationId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return;
        }

        try
        {
            await _notificationService.DeleteNotificationAsync(notificationId, userId);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            
            // Update the client's unread count
            await Clients.Caller.SendAsync("UpdateUnreadCount", unreadCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId} for user {UserId}", 
                notificationId, userId);
        }
    }
}
