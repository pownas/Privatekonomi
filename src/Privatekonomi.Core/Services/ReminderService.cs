using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Implementation of reminder service with snooze, completion, and escalation features
/// </summary>
public class ReminderService : IReminderService
{
    private readonly PrivatekonomyContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ReminderService> _logger;

    public ReminderService(
        PrivatekonomyContext context,
        INotificationService notificationService,
        ILogger<ReminderService> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<List<Reminder>> GetUserRemindersAsync(string userId, bool activeOnly = true)
    {
        var query = _context.Reminders.Where(r => r.UserId == userId);

        if (activeOnly)
        {
            query = query.Where(r => r.Status == ReminderStatus.Active || r.Status == ReminderStatus.Snoozed);
        }

        return await query
            .OrderBy(r => r.ReminderDate)
            .ToListAsync();
    }

    public async Task<Reminder?> GetReminderByIdAsync(int reminderId, string userId)
    {
        return await _context.Reminders
            .FirstOrDefaultAsync(r => r.ReminderId == reminderId && r.UserId == userId);
    }

    public async Task<Reminder> CreateReminderAsync(Reminder reminder)
    {
        reminder.CreatedAt = DateTime.UtcNow;
        reminder.Status = ReminderStatus.Active;
        
        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created reminder {ReminderId} for user {UserId}", reminder.ReminderId, reminder.UserId);
        
        return reminder;
    }

    public async Task<Reminder> UpdateReminderAsync(Reminder reminder)
    {
        reminder.UpdatedAt = DateTime.UtcNow;
        
        _context.Reminders.Update(reminder);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated reminder {ReminderId}", reminder.ReminderId);
        
        return reminder;
    }

    public async Task DeleteReminderAsync(int reminderId, string userId)
    {
        var reminder = await GetReminderByIdAsync(reminderId, userId);
        if (reminder == null)
        {
            throw new InvalidOperationException($"Reminder {reminderId} not found for user {userId}");
        }

        _context.Reminders.Remove(reminder);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted reminder {ReminderId}", reminderId);
    }

    public async Task SnoozeReminderAsync(int reminderId, string userId, int durationMinutes)
    {
        var reminder = await GetReminderByIdAsync(reminderId, userId);
        if (reminder == null)
        {
            throw new InvalidOperationException($"Reminder {reminderId} not found for user {userId}");
        }

        reminder.SnoozeUntil = DateTime.UtcNow.AddMinutes(durationMinutes);
        reminder.SnoozeCount++;
        reminder.Status = ReminderStatus.Snoozed;
        reminder.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Snoozed reminder {ReminderId} until {SnoozeUntil} (count: {SnoozeCount})", 
            reminderId, reminder.SnoozeUntil, reminder.SnoozeCount);
        
        // Check if escalation is needed
        if (await ShouldEscalateReminderAsync(reminderId))
        {
            await EscalateReminderAsync(reminderId);
        }
    }

    public async Task MarkAsCompletedAsync(int reminderId, string userId)
    {
        var reminder = await GetReminderByIdAsync(reminderId, userId);
        if (reminder == null)
        {
            throw new InvalidOperationException($"Reminder {reminderId} not found for user {userId}");
        }

        reminder.Status = ReminderStatus.Completed;
        reminder.CompletedDate = DateTime.UtcNow;
        reminder.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Marked reminder {ReminderId} as completed", reminderId);
        
        // Send confirmation notification
        await _notificationService.SendNotificationAsync(
            userId,
            SystemNotificationType.GoalAchieved,
            "Påminnelse slutförd",
            $"Du har slutfört påminnelsen: {reminder.Title}",
            NotificationPriority.Low);
    }

    public async Task DismissReminderAsync(int reminderId, string userId)
    {
        var reminder = await GetReminderByIdAsync(reminderId, userId);
        if (reminder == null)
        {
            throw new InvalidOperationException($"Reminder {reminderId} not found for user {userId}");
        }

        reminder.Status = ReminderStatus.Dismissed;
        reminder.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Dismissed reminder {ReminderId}", reminderId);
    }

    public async Task<List<Reminder>> GetActiveRemindersAsync(string userId)
    {
        var now = DateTime.UtcNow;
        
        return await _context.Reminders
            .Where(r => r.UserId == userId &&
                       (r.Status == ReminderStatus.Active || 
                        (r.Status == ReminderStatus.Snoozed && r.SnoozeUntil <= now)))
            .OrderBy(r => r.ReminderDate)
            .ToListAsync();
    }

    public async Task<List<Reminder>> GetDueRemindersAsync(string userId)
    {
        var now = DateTime.UtcNow;
        
        return await _context.Reminders
            .Where(r => r.UserId == userId &&
                       r.Status == ReminderStatus.Active &&
                       r.ReminderDate <= now)
            .OrderBy(r => r.ReminderDate)
            .ToListAsync();
    }

    public async Task ProcessFollowUpsAsync()
    {
        _logger.LogInformation("Processing reminder follow-ups");
        
        var now = DateTime.UtcNow;
        
        // Get all active reminders that need follow-up
        var remindersNeedingFollowUp = await _context.Reminders
            .Where(r => r.Status == ReminderStatus.Active &&
                       r.EnableFollowUp &&
                       r.ReminderDate < now &&
                       (r.LastFollowUpDate == null || 
                        r.LastFollowUpDate.Value.AddHours(r.FollowUpIntervalHours) <= now))
            .ToListAsync();

        foreach (var reminder in remindersNeedingFollowUp)
        {
            var settings = await GetUserSettingsAsync(reminder.UserId);
            
            // Calculate follow-up count
            var followUpCount = reminder.LastFollowUpDate == null ? 1 : 
                (int)((now - reminder.LastFollowUpDate.Value).TotalHours / reminder.FollowUpIntervalHours) + 1;
            
            // Check if max follow-ups reached
            if (followUpCount > reminder.MaxFollowUps)
            {
                reminder.Status = ReminderStatus.Expired;
                _logger.LogInformation("Reminder {ReminderId} expired after {FollowUpCount} follow-ups", 
                    reminder.ReminderId, followUpCount);
            }
            else
            {
                // Send follow-up notification
                var priority = followUpCount >= reminder.MaxFollowUps ? NotificationPriority.High : NotificationPriority.Normal;
                
                await _notificationService.SendNotificationAsync(
                    reminder.UserId,
                    SystemNotificationType.BillDue,
                    $"Påminnelse: {reminder.Title}",
                    $"{reminder.Description ?? "Du har en påminnelse som kräver din uppmärksamhet."}",
                    priority,
                    actionUrl: reminder.ActionUrl);
                
                reminder.LastFollowUpDate = now;
                reminder.EscalationLevel = followUpCount;
                
                _logger.LogInformation("Sent follow-up {FollowUpCount} for reminder {ReminderId}", 
                    followUpCount, reminder.ReminderId);
            }
            
            reminder.UpdatedAt = now;
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Processed {Count} reminder follow-ups", remindersNeedingFollowUp.Count);
    }

    public async Task<bool> ShouldEscalateReminderAsync(int reminderId)
    {
        var reminder = await _context.Reminders.FindAsync(reminderId);
        if (reminder == null)
        {
            return false;
        }

        var settings = await GetUserSettingsAsync(reminder.UserId);
        
        return settings.EnableEscalation && 
               reminder.SnoozeCount >= settings.SnoozeThresholdForEscalation;
    }

    public async Task EscalateReminderAsync(int reminderId)
    {
        var reminder = await _context.Reminders.FindAsync(reminderId);
        if (reminder == null)
        {
            throw new InvalidOperationException($"Reminder {reminderId} not found");
        }

        var settings = await GetUserSettingsAsync(reminder.UserId);
        
        reminder.EscalationLevel++;
        reminder.Priority = ReminderPriority.High;
        
        _logger.LogInformation("Escalating reminder {ReminderId} to level {EscalationLevel}", 
            reminderId, reminder.EscalationLevel);
        
        // Send escalation notification through configured channels
        var channels = NotificationChannelFlags.InApp;
        
        if (settings.EscalateToEmail)
        {
            channels |= NotificationChannelFlags.Email;
        }
        
        if (settings.EscalateToPush)
        {
            channels |= NotificationChannelFlags.Push;
        }
        
        await _notificationService.SendNotificationAsync(
            reminder.UserId,
            SystemNotificationType.BillOverdue,
            $"VIKTIGT: {reminder.Title}",
            $"Denna påminnelse har snoozats {reminder.SnoozeCount} gånger och kräver din uppmärksamhet.",
            NotificationPriority.Critical,
            actionUrl: reminder.ActionUrl);
        
        await _context.SaveChangesAsync();
    }

    public async Task<ReminderSettings> GetUserSettingsAsync(string userId)
    {
        var settings = await _context.ReminderSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            // Create default settings
            settings = new ReminderSettings
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.ReminderSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return settings;
    }

    public async Task<ReminderSettings> UpdateUserSettingsAsync(ReminderSettings settings)
    {
        settings.UpdatedAt = DateTime.UtcNow;
        
        _context.ReminderSettings.Update(settings);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated reminder settings for user {UserId}", settings.UserId);
        
        return settings;
    }

    public async Task<List<Reminder>> GetRemindersByTypeAsync(string userId, string reminderType)
    {
        return await _context.Reminders
            .Where(r => r.UserId == userId && r.ReminderType == reminderType)
            .OrderBy(r => r.ReminderDate)
            .ToListAsync();
    }

    public async Task<List<Reminder>> GetRemindersByEntityAsync(string userId, string entityType, int entityId)
    {
        return await _context.Reminders
            .Where(r => r.UserId == userId && 
                       r.RelatedEntityType == entityType && 
                       r.RelatedEntityId == entityId)
            .OrderBy(r => r.ReminderDate)
            .ToListAsync();
    }

    public async Task<ReminderStatistics> GetStatisticsAsync(string userId)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var tomorrow = today.AddDays(1);
        
        var reminders = await _context.Reminders
            .Where(r => r.UserId == userId)
            .ToListAsync();

        return new ReminderStatistics
        {
            TotalActive = reminders.Count(r => r.Status == ReminderStatus.Active),
            TotalSnoozed = reminders.Count(r => r.Status == ReminderStatus.Snoozed),
            TotalCompleted = reminders.Count(r => r.Status == ReminderStatus.Completed),
            TotalDismissed = reminders.Count(r => r.Status == ReminderStatus.Dismissed),
            DueToday = reminders.Count(r => r.ReminderDate >= today && r.ReminderDate < tomorrow),
            OverdueCount = reminders.Count(r => r.Status == ReminderStatus.Active && r.ReminderDate < now),
            EscalatedCount = reminders.Count(r => r.EscalationLevel > 0)
        };
    }
}
