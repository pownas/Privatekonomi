using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for managing user notification preferences
/// </summary>
public interface INotificationPreferenceService
{
    /// <summary>
    /// Get notification preferences for a user
    /// </summary>
    Task<List<NotificationPreference>> GetUserPreferencesAsync(string userId);
    
    /// <summary>
    /// Get preference for a specific notification type
    /// </summary>
    Task<NotificationPreference?> GetPreferenceAsync(string userId, SystemNotificationType type);
    
    /// <summary>
    /// Update or create a notification preference
    /// </summary>
    Task<NotificationPreference> SavePreferenceAsync(NotificationPreference preference);
    
    /// <summary>
    /// Get Do Not Disturb schedules for a user
    /// </summary>
    Task<List<DoNotDisturbSchedule>> GetDndSchedulesAsync(string userId);
    
    /// <summary>
    /// Save a Do Not Disturb schedule
    /// </summary>
    Task<DoNotDisturbSchedule> SaveDndScheduleAsync(DoNotDisturbSchedule schedule);
    
    /// <summary>
    /// Delete a Do Not Disturb schedule
    /// </summary>
    Task DeleteDndScheduleAsync(int scheduleId, string userId);
    
    /// <summary>
    /// Get notification integrations for a user
    /// </summary>
    Task<List<NotificationIntegration>> GetIntegrationsAsync(string userId);
    
    /// <summary>
    /// Save a notification integration
    /// </summary>
    Task<NotificationIntegration> SaveIntegrationAsync(NotificationIntegration integration);
    
    /// <summary>
    /// Delete a notification integration
    /// </summary>
    Task DeleteIntegrationAsync(int integrationId, string userId);
    
    /// <summary>
    /// Initialize default preferences for a new user
    /// </summary>
    Task InitializeDefaultPreferencesAsync(string userId);
}
