using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Implementation of notification service
/// </summary>
public class NotificationService : INotificationService
{
    private readonly PrivatekonomyContext _context;
    private readonly INotificationPreferenceService _preferenceService;
    private readonly ILogger<NotificationService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public NotificationService(
        PrivatekonomyContext context,
        INotificationPreferenceService preferenceService,
        ILogger<NotificationService> logger,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _preferenceService = preferenceService;
        _logger = logger;
        _serviceProvider = serviceProvider;
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
        // Check if DND is active (unless critical)
        if (priority != NotificationPriority.Critical && await IsDoNotDisturbActiveAsync(userId))
        {
            _logger.LogInformation("Notification suppressed due to DND for user {UserId}", userId);
            // Store notification but don't send
            return await CreateNotificationAsync(userId, type, title, message, NotificationChannel.InApp, priority, data, actionUrl, sendImmediately: false);
        }

        // Get user preferences for this notification type
        var preference = await _preferenceService.GetPreferenceAsync(userId, type);
        
        // Use default if no preference exists
        if (preference == null || !preference.IsEnabled)
        {
            // If disabled, only send critical notifications
            if (priority != NotificationPriority.Critical)
            {
                _logger.LogInformation("Notification type {Type} is disabled for user {UserId}", type, userId);
                return await CreateNotificationAsync(userId, type, title, message, NotificationChannel.InApp, priority, data, actionUrl, sendImmediately: false);
            }
            
            // Use default channels for critical notifications
            preference = new NotificationPreference
            {
                UserId = userId,
                NotificationType = type,
                EnabledChannels = NotificationChannelFlags.InApp | NotificationChannelFlags.Email,
                MinimumPriority = NotificationPriority.Normal
            };
        }

        // Check priority threshold
        if (priority < preference.MinimumPriority)
        {
            _logger.LogInformation("Notification priority {Priority} below threshold {Threshold} for user {UserId}",
                priority, preference.MinimumPriority, userId);
            return await CreateNotificationAsync(userId, type, title, message, NotificationChannel.InApp, priority, data, actionUrl, sendImmediately: false);
        }

        // Check digest mode
        if (preference.DigestMode && priority != NotificationPriority.Critical)
        {
            // Store for digest, don't send immediately
            return await CreateNotificationAsync(userId, type, title, message, NotificationChannel.InApp, priority, data, actionUrl, sendImmediately: false);
        }

        // Send through enabled channels
        var notifications = new List<Notification>();
        var enabledChannels = preference.EnabledChannels;

        if (enabledChannels.HasFlag(NotificationChannelFlags.InApp))
        {
            var notification = await CreateNotificationAsync(userId, type, title, message, NotificationChannel.InApp, priority, data, actionUrl);
            notifications.Add(notification);
        }

        if (enabledChannels.HasFlag(NotificationChannelFlags.Email))
        {
            var notification = await SendEmailNotificationAsync(userId, type, title, message, priority, data, actionUrl);
            notifications.Add(notification);
        }

        if (enabledChannels.HasFlag(NotificationChannelFlags.SMS) && priority >= NotificationPriority.High)
        {
            var notification = await SendSmsNotificationAsync(userId, type, title, message, priority, data, actionUrl);
            notifications.Add(notification);
        }

        if (enabledChannels.HasFlag(NotificationChannelFlags.Push))
        {
            var notification = await SendPushNotificationAsync(userId, type, title, message, priority, data, actionUrl);
            notifications.Add(notification);
        }

        if (enabledChannels.HasFlag(NotificationChannelFlags.Slack))
        {
            var notification = await SendSlackNotificationAsync(userId, type, title, message, priority, data, actionUrl);
            notifications.Add(notification);
        }

        if (enabledChannels.HasFlag(NotificationChannelFlags.Teams))
        {
            var notification = await SendTeamsNotificationAsync(userId, type, title, message, priority, data, actionUrl);
            notifications.Add(notification);
        }

        // Return the in-app notification or the first one created
        return notifications.FirstOrDefault() ?? 
               await CreateNotificationAsync(userId, type, title, message, NotificationChannel.InApp, priority, data, actionUrl);
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(100) // Limit to recent 100
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteNotificationAsync(int notificationId, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<bool> IsDoNotDisturbActiveAsync(string userId)
    {
        var now = DateTime.Now;
        var currentTime = now.TimeOfDay;
        var currentDayOfWeek = (int)now.DayOfWeek;

        var schedules = await _context.DoNotDisturbSchedules
            .Where(s => s.UserId == userId && s.IsEnabled)
            .ToListAsync();

        foreach (var schedule in schedules)
        {
            // Check if schedule applies to current day (7 = all days)
            if (schedule.DayOfWeek != 7 && schedule.DayOfWeek != currentDayOfWeek)
            {
                continue;
            }

            var startTime = TimeSpan.Parse(schedule.StartTime);
            var endTime = TimeSpan.Parse(schedule.EndTime);

            // Handle schedules that span midnight
            if (startTime > endTime)
            {
                if (currentTime >= startTime || currentTime <= endTime)
                {
                    return true;
                }
            }
            else
            {
                if (currentTime >= startTime && currentTime <= endTime)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public async Task SendDigestNotificationsAsync()
    {
        // Get users with digest mode enabled
        var digestPreferences = await _context.NotificationPreferences
            .Where(p => p.DigestMode && p.IsEnabled)
            .ToListAsync();

        var userIds = digestPreferences.Select(p => p.UserId).Distinct();

        foreach (var userId in userIds)
        {
            // Get unsent notifications for this user
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.SentAt == null && n.Channel == NotificationChannel.InApp)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            if (!notifications.Any())
            {
                continue;
            }

            // Group by notification type and create digest
            var groupedNotifications = notifications.GroupBy(n => n.Type);
            var digestMessage = "Du har nya notifikationer:\n\n";

            foreach (var group in groupedNotifications)
            {
                digestMessage += $"• {GetNotificationTypeDisplayName(group.Key)}: {group.Count()} st\n";
            }

            // Send digest notification
            await SendEmailNotificationAsync(
                userId,
                SystemNotificationType.SystemAlert,
                "Notifikationssammanfattning",
                digestMessage,
                NotificationPriority.Normal,
                null,
                null);

            // Mark notifications as sent
            foreach (var notification in notifications)
            {
                notification.SentAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }

    // Private helper methods
    private async Task<Notification> CreateNotificationAsync(
        string userId,
        SystemNotificationType type,
        string title,
        string message,
        NotificationChannel channel,
        NotificationPriority priority,
        string? data,
        string? actionUrl,
        bool sendImmediately = true)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Channel = channel,
            Priority = priority,
            Data = data,
            ActionUrl = actionUrl,
            CreatedAt = DateTime.UtcNow,
            SentAt = sendImmediately ? DateTime.UtcNow : null
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return notification;
    }

    private async Task<Notification> SendEmailNotificationAsync(
        string userId,
        SystemNotificationType type,
        string title,
        string message,
        NotificationPriority priority,
        string? data,
        string? actionUrl)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Channel = NotificationChannel.Email,
            Priority = priority,
            Data = data,
            ActionUrl = actionUrl,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            // Get user email
            var user = await _context.Users.FindAsync(userId);
            if (user?.Email != null)
            {
                // Email service would be injected and called here
                // For now, just log
                _logger.LogInformation("Email notification would be sent to {Email}: {Title}", user.Email, title);
                notification.SentAt = DateTime.UtcNow;
            }
            else
            {
                notification.ErrorMessage = "User email not found";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification");
            notification.ErrorMessage = ex.Message;
        }

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return notification;
    }

    private async Task<Notification> SendSmsNotificationAsync(
        string userId,
        SystemNotificationType type,
        string title,
        string message,
        NotificationPriority priority,
        string? data,
        string? actionUrl)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Channel = NotificationChannel.SMS,
            Priority = priority,
            Data = data,
            ActionUrl = actionUrl,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            // SMS service would be injected and called here
            _logger.LogInformation("SMS notification would be sent to user {UserId}: {Title}", userId, title);
            notification.SentAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS notification");
            notification.ErrorMessage = ex.Message;
        }

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return notification;
    }

    private async Task<Notification> SendPushNotificationAsync(
        string userId,
        SystemNotificationType type,
        string title,
        string message,
        NotificationPriority priority,
        string? data,
        string? actionUrl)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Channel = NotificationChannel.Push,
            Priority = priority,
            Data = data,
            ActionUrl = actionUrl,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            // Push service would be injected and called here
            _logger.LogInformation("Push notification would be sent to user {UserId}: {Title}", userId, title);
            notification.SentAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification");
            notification.ErrorMessage = ex.Message;
        }

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return notification;
    }

    private async Task<Notification> SendSlackNotificationAsync(
        string userId,
        SystemNotificationType type,
        string title,
        string message,
        NotificationPriority priority,
        string? data,
        string? actionUrl)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Channel = NotificationChannel.Slack,
            Priority = priority,
            Data = data,
            ActionUrl = actionUrl,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            // Get Slack integration
            var integration = await _context.NotificationIntegrations
                .FirstOrDefaultAsync(i => i.UserId == userId && 
                                        i.Channel == NotificationChannel.Slack && 
                                        i.IsEnabled);

            if (integration != null)
            {
                // Slack service would be injected and called here
                _logger.LogInformation("Slack notification would be sent to user {UserId}: {Title}", userId, title);
                notification.SentAt = DateTime.UtcNow;
            }
            else
            {
                notification.ErrorMessage = "Slack integration not configured";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Slack notification");
            notification.ErrorMessage = ex.Message;
        }

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return notification;
    }

    private async Task<Notification> SendTeamsNotificationAsync(
        string userId,
        SystemNotificationType type,
        string title,
        string message,
        NotificationPriority priority,
        string? data,
        string? actionUrl)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Channel = NotificationChannel.Teams,
            Priority = priority,
            Data = data,
            ActionUrl = actionUrl,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            // Get Teams integration
            var integration = await _context.NotificationIntegrations
                .FirstOrDefaultAsync(i => i.UserId == userId && 
                                        i.Channel == NotificationChannel.Teams && 
                                        i.IsEnabled);

            if (integration != null)
            {
                // Teams service would be injected and called here
                _logger.LogInformation("Teams notification would be sent to user {UserId}: {Title}", userId, title);
                notification.SentAt = DateTime.UtcNow;
            }
            else
            {
                notification.ErrorMessage = "Teams integration not configured";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Teams notification");
            notification.ErrorMessage = ex.Message;
        }

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return notification;
    }

    private string GetNotificationTypeDisplayName(SystemNotificationType type)
    {
        return type switch
        {
            SystemNotificationType.BudgetExceeded => "Budgetöverdrag",
            SystemNotificationType.BudgetWarning => "Budgetvarning",
            SystemNotificationType.LowBalance => "Låg balans",
            SystemNotificationType.UpcomingBill => "Kommande räkning",
            SystemNotificationType.BillDue => "Räkning förfaller",
            SystemNotificationType.BillOverdue => "Försenad räkning",
            SystemNotificationType.GoalAchieved => "Sparmål uppnått",
            SystemNotificationType.GoalMilestone => "Sparmål milstolpe",
            SystemNotificationType.InvestmentChange => "Investeringsförändring",
            SystemNotificationType.SignificantGain => "Stor vinst",
            SystemNotificationType.SignificantLoss => "Stor förlust",
            SystemNotificationType.UnusualTransaction => "Ovanlig transaktion",
            SystemNotificationType.LargeTransaction => "Stor transaktion",
            SystemNotificationType.BankSyncFailed => "Banksynk misslyckades",
            SystemNotificationType.BankSyncSuccess => "Banksynk lyckades",
            SystemNotificationType.HouseholdActivity => "Hushållsaktivitet",
            SystemNotificationType.HouseholdInvitation => "Hushållsinbjudan",
            SystemNotificationType.SubscriptionPriceIncrease => "Prenumerationspris ökat",
            SystemNotificationType.SubscriptionRenewal => "Prenumerationsförnyelse",
            SystemNotificationType.SystemMaintenance => "Systemunderhåll",
            SystemNotificationType.SystemAlert => "Systemvarning",
            _ => type.ToString()
        };
    }
}
