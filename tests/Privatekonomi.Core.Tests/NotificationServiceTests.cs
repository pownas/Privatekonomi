using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class NotificationServiceTests
{
    private readonly PrivatekonomyContext _context;
    private readonly INotificationPreferenceService _preferenceService;
    private readonly NotificationService _notificationService;
    private readonly string _testUserId = "test-user-id";

    public NotificationServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PrivatekonomyContext(options);
        _preferenceService = new NotificationPreferenceService(_context);
        
        var loggerMock = new Mock<ILogger<NotificationService>>();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        
        _notificationService = new NotificationService(
            _context,
            _preferenceService,
            loggerMock.Object,
            serviceProvider);
    }

    [Fact]
    public async Task SendNotificationAsync_CreatesNotification()
    {
        // Act
        var notification = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.BudgetExceeded,
            "Test Title",
            "Test Message",
            NotificationPriority.Normal);

        // Assert
        Assert.NotNull(notification);
        Assert.Equal(_testUserId, notification.UserId);
        Assert.Equal(SystemNotificationType.BudgetExceeded, notification.Type);
        Assert.Equal("Test Title", notification.Title);
        Assert.Equal("Test Message", notification.Message);
        Assert.False(notification.IsRead);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ReturnsUserNotifications()
    {
        // Arrange
        await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.BudgetExceeded,
            "Test 1",
            "Message 1");

        await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.LowBalance,
            "Test 2",
            "Message 2");

        // Act
        var notifications = await _notificationService.GetUserNotificationsAsync(_testUserId);

        // Assert
        Assert.NotEmpty(notifications);
        Assert.True(notifications.Count >= 2);
        Assert.All(notifications, n => Assert.Equal(_testUserId, n.UserId));
    }

    [Fact]
    public async Task GetUserNotificationsAsync_UnreadOnly_ReturnsOnlyUnread()
    {
        // Arrange
        var notification1 = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.BudgetExceeded,
            "Test 1",
            "Message 1");

        var notification2 = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.LowBalance,
            "Test 2",
            "Message 2");

        await _notificationService.MarkAsReadAsync(notification1.NotificationId, _testUserId);

        // Act
        var unreadNotifications = await _notificationService.GetUserNotificationsAsync(_testUserId, unreadOnly: true);

        // Assert
        Assert.NotEmpty(unreadNotifications);
        Assert.All(unreadNotifications, n => Assert.False(n.IsRead));
    }

    [Fact]
    public async Task MarkAsReadAsync_MarksNotificationAsRead()
    {
        // Arrange
        var notification = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.BudgetExceeded,
            "Test",
            "Message");

        // Act
        await _notificationService.MarkAsReadAsync(notification.NotificationId, _testUserId);

        // Assert
        var updated = await _context.Notifications.FindAsync(notification.NotificationId);
        Assert.NotNull(updated);
        Assert.True(updated.IsRead);
        Assert.NotNull(updated.ReadAt);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_MarksAllNotificationsAsRead()
    {
        // Arrange
        await _notificationService.SendNotificationAsync(_testUserId, SystemNotificationType.BudgetExceeded, "Test 1", "Message 1");
        await _notificationService.SendNotificationAsync(_testUserId, SystemNotificationType.LowBalance, "Test 2", "Message 2");
        await _notificationService.SendNotificationAsync(_testUserId, SystemNotificationType.GoalAchieved, "Test 3", "Message 3");

        // Act
        await _notificationService.MarkAllAsReadAsync(_testUserId);

        // Assert
        var notifications = await _context.Notifications.Where(n => n.UserId == _testUserId).ToListAsync();
        Assert.All(notifications, n => Assert.True(n.IsRead));
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var notification1 = await _notificationService.SendNotificationAsync(_testUserId, SystemNotificationType.BudgetExceeded, "Test 1", "Message 1");
        await _notificationService.SendNotificationAsync(_testUserId, SystemNotificationType.LowBalance, "Test 2", "Message 2");
        await _notificationService.SendNotificationAsync(_testUserId, SystemNotificationType.GoalAchieved, "Test 3", "Message 3");

        await _notificationService.MarkAsReadAsync(notification1.NotificationId, _testUserId);

        // Act
        var unreadCount = await _notificationService.GetUnreadCountAsync(_testUserId);

        // Assert
        Assert.Equal(2, unreadCount);
    }

    [Fact]
    public async Task DeleteNotificationAsync_DeletesNotification()
    {
        // Arrange
        var notification = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.BudgetExceeded,
            "Test",
            "Message");

        // Act
        await _notificationService.DeleteNotificationAsync(notification.NotificationId, _testUserId);

        // Assert
        var deleted = await _context.Notifications.FindAsync(notification.NotificationId);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task IsDoNotDisturbActiveAsync_WithNoDndSchedule_ReturnsFalse()
    {
        // Act
        var isDndActive = await _notificationService.IsDoNotDisturbActiveAsync(_testUserId);

        // Assert
        Assert.False(isDndActive);
    }

    [Fact]
    public async Task IsDoNotDisturbActiveAsync_WithActiveDndSchedule_ReturnsTrue()
    {
        // Arrange
        var now = DateTime.Now;
        var startTime = now.AddMinutes(-30).ToString("HH:mm");
        var endTime = now.AddMinutes(30).ToString("HH:mm");

        var dndSchedule = new DoNotDisturbSchedule
        {
            UserId = _testUserId,
            DayOfWeek = 7, // All days
            StartTime = startTime,
            EndTime = endTime,
            IsEnabled = true
        };

        await _preferenceService.SaveDndScheduleAsync(dndSchedule);

        // Act
        var isDndActive = await _notificationService.IsDoNotDisturbActiveAsync(_testUserId);

        // Assert
        Assert.True(isDndActive);
    }

    [Fact]
    public async Task SendNotificationAsync_WithDisabledNotificationType_DoesNotSendImmediately()
    {
        // Arrange
        var preference = new NotificationPreference
        {
            UserId = _testUserId,
            NotificationType = SystemNotificationType.BudgetExceeded,
            IsEnabled = false,
            EnabledChannels = NotificationChannelFlags.InApp
        };

        await _preferenceService.SavePreferenceAsync(preference);

        // Act
        var notification = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.BudgetExceeded,
            "Test",
            "Message",
            NotificationPriority.Normal);

        // Assert
        Assert.NotNull(notification);
        Assert.Null(notification.SentAt); // Not sent immediately because type is disabled
    }

    [Fact]
    public async Task SendNotificationAsync_WithCriticalPriority_BypassesDisabledSetting()
    {
        // Arrange
        var preference = new NotificationPreference
        {
            UserId = _testUserId,
            NotificationType = SystemNotificationType.BudgetExceeded,
            IsEnabled = false,
            EnabledChannels = NotificationChannelFlags.InApp
        };

        await _preferenceService.SavePreferenceAsync(preference);

        // Act
        var notification = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.BudgetExceeded,
            "Critical Test",
            "Critical Message",
            NotificationPriority.Critical);

        // Assert
        Assert.NotNull(notification);
        Assert.NotNull(notification.SentAt); // Critical notifications are sent even if disabled
    }

    [Fact]
    public async Task SendNotificationAsync_WithActionUrl_StoresActionUrl()
    {
        // Act
        var notification = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.GoalAchieved,
            "Goal Achieved",
            "Your savings goal has been reached!",
            NotificationPriority.Normal,
            data: null,
            actionUrl: "/goals/123");

        // Assert
        Assert.NotNull(notification);
        Assert.Equal("/goals/123", notification.ActionUrl);
    }

    [Fact]
    public async Task SnoozeNotificationAsync_WithOneHourDuration_SetsSnoozeUntilCorrectly()
    {
        // Arrange
        var notification = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.UpcomingBill,
            "Bill Reminder",
            "Bill due tomorrow");

        // Act
        var beforeSnooze = DateTime.UtcNow;
        await _notificationService.SnoozeNotificationAsync(notification.NotificationId, _testUserId, SnoozeDuration.OneHour);
        var afterSnooze = DateTime.UtcNow;

        // Assert
        var snoozedNotification = await _context.Notifications.FindAsync(notification.NotificationId);
        Assert.NotNull(snoozedNotification);
        Assert.NotNull(snoozedNotification.SnoozeUntil);
        Assert.True(snoozedNotification.SnoozeUntil >= beforeSnooze.AddHours(1));
        Assert.True(snoozedNotification.SnoozeUntil <= afterSnooze.AddHours(1));
        Assert.Equal(1, snoozedNotification.SnoozeCount);
    }

    [Fact]
    public async Task SnoozeNotificationAsync_WithOneDayDuration_SetsSnoozeUntilCorrectly()
    {
        // Arrange
        var notification = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.BillDue,
            "Bill Due",
            "Bill due today");

        // Act
        await _notificationService.SnoozeNotificationAsync(notification.NotificationId, _testUserId, SnoozeDuration.OneDay);

        // Assert
        var snoozedNotification = await _context.Notifications.FindAsync(notification.NotificationId);
        Assert.NotNull(snoozedNotification);
        Assert.NotNull(snoozedNotification.SnoozeUntil);
        Assert.True(snoozedNotification.SnoozeUntil >= DateTime.UtcNow.AddHours(23));
        Assert.True(snoozedNotification.SnoozeUntil <= DateTime.UtcNow.AddHours(25));
        Assert.Equal(1, snoozedNotification.SnoozeCount);
    }

    [Fact]
    public async Task SnoozeNotificationAsync_WithOneWeekDuration_SetsSnoozeUntilCorrectly()
    {
        // Arrange
        var notification = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.BillOverdue,
            "Overdue Bill",
            "Bill is overdue");

        // Act
        await _notificationService.SnoozeNotificationAsync(notification.NotificationId, _testUserId, SnoozeDuration.OneWeek);

        // Assert
        var snoozedNotification = await _context.Notifications.FindAsync(notification.NotificationId);
        Assert.NotNull(snoozedNotification);
        Assert.NotNull(snoozedNotification.SnoozeUntil);
        Assert.True(snoozedNotification.SnoozeUntil >= DateTime.UtcNow.AddDays(6.9));
        Assert.True(snoozedNotification.SnoozeUntil <= DateTime.UtcNow.AddDays(7.1));
        Assert.Equal(1, snoozedNotification.SnoozeCount);
    }

    [Fact]
    public async Task SnoozeNotificationAsync_MultipleSnoozes_IncrementsSnoozeCount()
    {
        // Arrange
        var notification = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.UpcomingBill,
            "Bill Reminder",
            "Bill due tomorrow");

        // Act
        await _notificationService.SnoozeNotificationAsync(notification.NotificationId, _testUserId, SnoozeDuration.OneHour);
        await _notificationService.SnoozeNotificationAsync(notification.NotificationId, _testUserId, SnoozeDuration.OneHour);
        await _notificationService.SnoozeNotificationAsync(notification.NotificationId, _testUserId, SnoozeDuration.OneHour);

        // Assert
        var snoozedNotification = await _context.Notifications.FindAsync(notification.NotificationId);
        Assert.NotNull(snoozedNotification);
        Assert.Equal(3, snoozedNotification.SnoozeCount);
    }

    [Fact]
    public async Task SnoozeNotificationAsync_WithInvalidNotificationId_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _notificationService.SnoozeNotificationAsync(999, _testUserId, SnoozeDuration.OneHour));
    }

    [Fact]
    public async Task MarkReminderAsCompletedAsync_MarksNotificationAsReadAndCompleted()
    {
        // Arrange
        var bill = new Bill
        {
            UserId = _testUserId,
            Name = "Test Bill",
            Amount = 1000,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7),
            Status = "Pending"
        };
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();

        var reminder = new BillReminder
        {
            BillId = bill.BillId,
            ReminderDate = DateTime.UtcNow,
            IsSent = true,
            SentDate = DateTime.UtcNow
        };
        _context.BillReminders.Add(reminder);
        await _context.SaveChangesAsync();

        var notification = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.UpcomingBill,
            "Bill Reminder",
            "Bill due soon");
        
        notification.BillReminderId = reminder.BillReminderId;
        await _context.SaveChangesAsync();

        // Act
        await _notificationService.MarkReminderAsCompletedAsync(notification.NotificationId, _testUserId);

        // Assert
        var completedNotification = await _context.Notifications.FindAsync(notification.NotificationId);
        Assert.NotNull(completedNotification);
        Assert.True(completedNotification.IsRead);
        Assert.NotNull(completedNotification.ReadAt);

        var completedReminder = await _context.BillReminders.FindAsync(reminder.BillReminderId);
        Assert.NotNull(completedReminder);
        Assert.True(completedReminder.IsCompleted);
        Assert.NotNull(completedReminder.CompletedDate);

        var paidBill = await _context.Bills.FindAsync(bill.BillId);
        Assert.NotNull(paidBill);
        Assert.Equal("Paid", paidBill.Status);
        Assert.NotNull(paidBill.PaidDate);
    }

    [Fact]
    public async Task GetActiveNotificationsAsync_ExcludesSnoozedNotifications()
    {
        // Arrange
        var notification1 = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.BudgetWarning,
            "Active Notification",
            "This is active");

        var notification2 = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.UpcomingBill,
            "Snoozed Notification",
            "This is snoozed");

        await _notificationService.SnoozeNotificationAsync(notification2.NotificationId, _testUserId, SnoozeDuration.OneDay);

        // Act
        var activeNotifications = await _notificationService.GetActiveNotificationsAsync(_testUserId);

        // Assert
        Assert.NotEmpty(activeNotifications);
        Assert.Contains(activeNotifications, n => n.NotificationId == notification1.NotificationId);
        Assert.DoesNotContain(activeNotifications, n => n.NotificationId == notification2.NotificationId);
    }

    [Fact]
    public async Task GetActiveNotificationsAsync_IncludesExpiredSnoozes()
    {
        // Arrange
        var notification = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.UpcomingBill,
            "Expired Snooze",
            "This snooze has expired");

        // Manually set an expired snooze
        notification.SnoozeUntil = DateTime.UtcNow.AddHours(-1);
        await _context.SaveChangesAsync();

        // Act
        var activeNotifications = await _notificationService.GetActiveNotificationsAsync(_testUserId);

        // Assert
        Assert.Contains(activeNotifications, n => n.NotificationId == notification.NotificationId);
    }

    [Fact]
    public async Task ShouldEscalateReminderAsync_WithHighSnoozeCount_ReturnsTrue()
    {
        // Arrange
        var bill = new Bill
        {
            UserId = _testUserId,
            Name = "Test Bill",
            Amount = 1000,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7),
            Status = "Pending"
        };
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();

        var reminder = new BillReminder
        {
            BillId = bill.BillId,
            ReminderDate = DateTime.UtcNow,
            IsSent = true,
            SnoozeCount = 5 // High snooze count
        };
        _context.BillReminders.Add(reminder);
        await _context.SaveChangesAsync();

        var notification = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.UpcomingBill,
            "Bill Reminder",
            "Bill due soon");
        
        notification.BillReminderId = reminder.BillReminderId;
        await _context.SaveChangesAsync();

        // Act
        var shouldEscalate = await _notificationService.ShouldEscalateReminderAsync(notification.NotificationId);

        // Assert
        Assert.True(shouldEscalate);
    }

    [Fact]
    public async Task ShouldEscalateReminderAsync_WithLowSnoozeCount_ReturnsFalse()
    {
        // Arrange
        var bill = new Bill
        {
            UserId = _testUserId,
            Name = "Test Bill",
            Amount = 1000,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7),
            Status = "Pending"
        };
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();

        var reminder = new BillReminder
        {
            BillId = bill.BillId,
            ReminderDate = DateTime.UtcNow,
            IsSent = true,
            SnoozeCount = 1
        };
        _context.BillReminders.Add(reminder);
        await _context.SaveChangesAsync();

        var notification = await _notificationService.SendNotificationAsync(
            _testUserId,
            SystemNotificationType.UpcomingBill,
            "Bill Reminder",
            "Bill due soon");
        
        notification.BillReminderId = reminder.BillReminderId;
        await _context.SaveChangesAsync();

        // Act
        var shouldEscalate = await _notificationService.ShouldEscalateReminderAsync(notification.NotificationId);

        // Assert
        Assert.False(shouldEscalate);
    }
}
