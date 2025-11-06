using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for managing flexible reminders with snooze and escalation
/// </summary>
public interface IReminderService
{
    /// <summary>
    /// Get all reminders for a user
    /// </summary>
    Task<List<Reminder>> GetUserRemindersAsync(string userId, bool activeOnly = true);
    
    /// <summary>
    /// Get a specific reminder by ID
    /// </summary>
    Task<Reminder?> GetReminderByIdAsync(int reminderId, string userId);
    
    /// <summary>
    /// Create a new reminder
    /// </summary>
    Task<Reminder> CreateReminderAsync(Reminder reminder);
    
    /// <summary>
    /// Update an existing reminder
    /// </summary>
    Task<Reminder> UpdateReminderAsync(Reminder reminder);
    
    /// <summary>
    /// Delete a reminder
    /// </summary>
    Task DeleteReminderAsync(int reminderId, string userId);
    
    /// <summary>
    /// Snooze a reminder for a specified duration
    /// </summary>
    Task SnoozeReminderAsync(int reminderId, string userId, int durationMinutes);
    
    /// <summary>
    /// Mark a reminder as completed
    /// </summary>
    Task MarkAsCompletedAsync(int reminderId, string userId);
    
    /// <summary>
    /// Mark a reminder as dismissed
    /// </summary>
    Task DismissReminderAsync(int reminderId, string userId);
    
    /// <summary>
    /// Get active (non-snoozed, non-completed) reminders
    /// </summary>
    Task<List<Reminder>> GetActiveRemindersAsync(string userId);
    
    /// <summary>
    /// Get reminders that are due now
    /// </summary>
    Task<List<Reminder>> GetDueRemindersAsync(string userId);
    
    /// <summary>
    /// Process follow-ups for reminders that haven't been completed
    /// </summary>
    Task ProcessFollowUpsAsync();
    
    /// <summary>
    /// Check if a reminder should be escalated
    /// </summary>
    Task<bool> ShouldEscalateReminderAsync(int reminderId);
    
    /// <summary>
    /// Escalate a reminder to higher priority notifications
    /// </summary>
    Task EscalateReminderAsync(int reminderId);
    
    /// <summary>
    /// Get user's reminder settings
    /// </summary>
    Task<ReminderSettings> GetUserSettingsAsync(string userId);
    
    /// <summary>
    /// Update user's reminder settings
    /// </summary>
    Task<ReminderSettings> UpdateUserSettingsAsync(ReminderSettings settings);
    
    /// <summary>
    /// Get reminders by type
    /// </summary>
    Task<List<Reminder>> GetRemindersByTypeAsync(string userId, string reminderType);
    
    /// <summary>
    /// Get reminders by related entity
    /// </summary>
    Task<List<Reminder>> GetRemindersByEntityAsync(string userId, string entityType, int entityId);
    
    /// <summary>
    /// Get reminder statistics for a user
    /// </summary>
    Task<ReminderStatistics> GetStatisticsAsync(string userId);
}

/// <summary>
/// Statistics about user's reminders
/// </summary>
public class ReminderStatistics
{
    public int TotalActive { get; set; }
    public int TotalSnoozed { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalDismissed { get; set; }
    public int DueToday { get; set; }
    public int OverdueCount { get; set; }
    public int EscalatedCount { get; set; }
}
