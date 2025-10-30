using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for managing and sending notifications
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send a notification to a user through configured channels
    /// </summary>
    Task<Notification> SendNotificationAsync(
        string userId, 
        SystemNotificationType type, 
        string title, 
        string message,
        NotificationPriority priority = NotificationPriority.Normal,
        string? data = null,
        string? actionUrl = null);
    
    /// <summary>
    /// Get all notifications for a user
    /// </summary>
    Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
    
    /// <summary>
    /// Mark a notification as read
    /// </summary>
    Task MarkAsReadAsync(int notificationId, string userId);
    
    /// <summary>
    /// Mark all notifications as read for a user
    /// </summary>
    Task MarkAllAsReadAsync(string userId);
    
    /// <summary>
    /// Delete a notification
    /// </summary>
    Task DeleteNotificationAsync(int notificationId, string userId);
    
    /// <summary>
    /// Get unread notification count for a user
    /// </summary>
    Task<int> GetUnreadCountAsync(string userId);
    
    /// <summary>
    /// Check if notifications should be sent based on DND schedule
    /// </summary>
    Task<bool> IsDoNotDisturbActiveAsync(string userId);
    
    /// <summary>
    /// Send digest notifications for users with digest mode enabled
    /// </summary>
    Task SendDigestNotificationsAsync();
}
