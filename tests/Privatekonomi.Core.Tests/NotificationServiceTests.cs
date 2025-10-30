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
}
