using Privatekonomi.Core.Models;

namespace Privatekonomi.Web.Services;

/// <summary>
/// Service for broadcasting notifications to connected clients via SignalR
/// </summary>
public interface INotificationBroadcaster
{
    /// <summary>
    /// Broadcast a notification to a specific user
    /// </summary>
    Task BroadcastNotificationAsync(string userId, Notification notification);
    
    /// <summary>
    /// Update unread count for a specific user
    /// </summary>
    Task UpdateUnreadCountAsync(string userId, int count);
}
