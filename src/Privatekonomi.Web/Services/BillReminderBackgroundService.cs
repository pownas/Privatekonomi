using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;

namespace Privatekonomi.Web.Services;

/// <summary>
/// Background service that periodically processes bill reminders and generates recurring bill occurrences
/// </summary>
public class BillReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BillReminderBackgroundService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    public BillReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<BillReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Bill Reminder Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBillRemindersAsync();
                await ProcessDueSchedulesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bill reminder background service");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }

        _logger.LogInformation("Bill Reminder Background Service stopped");
    }

    private async Task ProcessBillRemindersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var billService = scope.ServiceProvider.GetRequiredService<IBillService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        try
        {
            var pendingReminders = await billService.GetPendingRemindersAsync();

            foreach (var reminder in pendingReminders)
            {
                if (reminder.Bill == null) continue;

                try
                {
                    var bill = reminder.Bill;
                    var daysUntilDue = (int)(bill.DueDate.Date - DateTime.UtcNow.Date).TotalDays;
                    var isOverdue = daysUntilDue < 0;

                    var title = isOverdue
                        ? $"Förfallen räkning: {bill.Name}"
                        : $"Påminnelse: {bill.Name} förfaller om {daysUntilDue} dagar";

                    var message = isOverdue
                        ? $"Räkningen {bill.Name} på {bill.Amount:N0} kr förföll {bill.DueDate:yyyy-MM-dd}."
                        : $"Räkningen {bill.Name} på {bill.Amount:N0} kr förfaller {bill.DueDate:yyyy-MM-dd}.";

                    var notificationType = isOverdue
                        ? SystemNotificationType.BillOverdue
                        : SystemNotificationType.BillDue;

                    var priority = isOverdue ? NotificationPriority.High : NotificationPriority.Normal;

                    await notificationService.SendNotificationAsync(
                        bill.UserId,
                        notificationType,
                        title,
                        message,
                        priority,
                        actionUrl: "/bills");

                    await billService.MarkReminderAsSentAsync(reminder.BillReminderId);

                    _logger.LogInformation(
                        "Sent bill reminder {ReminderId} for bill {BillId} ({BillName}) to user {UserId}",
                        reminder.BillReminderId, bill.BillId, bill.Name, bill.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing reminder {ReminderId}", reminder.BillReminderId);
                }
            }

            if (pendingReminders.Count > 0)
            {
                _logger.LogInformation("Processed {Count} pending bill reminders", pendingReminders.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessBillRemindersAsync");
            throw;
        }
    }

    private async Task ProcessDueSchedulesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var billService = scope.ServiceProvider.GetRequiredService<IBillService>();

        try
        {
            var dueSchedules = await billService.GetDueSchedulesAsync();

            foreach (var schedule in dueSchedules)
            {
                try
                {
                    var newBill = await billService.GenerateNextOccurrenceAsync(schedule);
                    _logger.LogInformation(
                        "Generated recurring bill occurrence {BillId} ({BillName}) from schedule {ScheduleId}",
                        newBill.BillId, newBill.Name, schedule.BillScheduleId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating occurrence for schedule {ScheduleId}", schedule.BillScheduleId);
                }
            }

            if (dueSchedules.Count > 0)
            {
                _logger.LogInformation("Generated {Count} recurring bill occurrences", dueSchedules.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessDueSchedulesAsync");
            throw;
        }
    }
}
