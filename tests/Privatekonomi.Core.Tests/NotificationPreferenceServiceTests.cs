using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class NotificationPreferenceServiceTests
{
    private readonly PrivatekonomyContext _context;
    private readonly NotificationPreferenceService _preferenceService;
    private readonly string _testUserId = "test-user-id";

    public NotificationPreferenceServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PrivatekonomyContext(options);
        _preferenceService = new NotificationPreferenceService(_context);
    }

    [Fact]
    public async Task GetUserPreferencesAsync_ReturnsEmptyListForNewUser()
    {
        // Act
        var preferences = await _preferenceService.GetUserPreferencesAsync(_testUserId);

        // Assert
        Assert.Empty(preferences);
    }

    [Fact]
    public async Task SavePreferenceAsync_CreatesNewPreference()
    {
        // Arrange
        var preference = new NotificationPreference
        {
            UserId = _testUserId,
            NotificationType = SystemNotificationType.BudgetExceeded,
            EnabledChannels = NotificationChannelFlags.InApp | NotificationChannelFlags.Email,
            MinimumPriority = NotificationPriority.Normal,
            IsEnabled = true
        };

        // Act
        var saved = await _preferenceService.SavePreferenceAsync(preference);

        // Assert
        Assert.NotNull(saved);
        Assert.True(saved.NotificationPreferenceId > 0);
        Assert.Equal(_testUserId, saved.UserId);
        Assert.Equal(SystemNotificationType.BudgetExceeded, saved.NotificationType);
    }

    [Fact]
    public async Task SavePreferenceAsync_UpdatesExistingPreference()
    {
        // Arrange
        var preference = new NotificationPreference
        {
            UserId = _testUserId,
            NotificationType = SystemNotificationType.BudgetExceeded,
            EnabledChannels = NotificationChannelFlags.InApp,
            MinimumPriority = NotificationPriority.Normal,
            IsEnabled = true
        };

        var created = await _preferenceService.SavePreferenceAsync(preference);

        // Act - Update
        created.EnabledChannels = NotificationChannelFlags.Email | NotificationChannelFlags.SMS;
        created.IsEnabled = false;
        var updated = await _preferenceService.SavePreferenceAsync(created);

        // Assert
        Assert.Equal(created.NotificationPreferenceId, updated.NotificationPreferenceId);
        Assert.Equal(NotificationChannelFlags.Email | NotificationChannelFlags.SMS, updated.EnabledChannels);
        Assert.False(updated.IsEnabled);
    }

    [Fact]
    public async Task GetPreferenceAsync_ReturnsSpecificPreference()
    {
        // Arrange
        var preference = new NotificationPreference
        {
            UserId = _testUserId,
            NotificationType = SystemNotificationType.LowBalance,
            EnabledChannels = NotificationChannelFlags.InApp | NotificationChannelFlags.SMS,
            MinimumPriority = NotificationPriority.High,
            IsEnabled = true
        };

        await _preferenceService.SavePreferenceAsync(preference);

        // Act
        var retrieved = await _preferenceService.GetPreferenceAsync(_testUserId, SystemNotificationType.LowBalance);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(SystemNotificationType.LowBalance, retrieved.NotificationType);
        Assert.Equal(NotificationChannelFlags.InApp | NotificationChannelFlags.SMS, retrieved.EnabledChannels);
    }

    [Fact]
    public async Task GetPreferenceAsync_ReturnsNullForNonExistent()
    {
        // Act
        var preference = await _preferenceService.GetPreferenceAsync(_testUserId, SystemNotificationType.BudgetExceeded);

        // Assert
        Assert.Null(preference);
    }

    [Fact]
    public async Task SaveDndScheduleAsync_CreatesNewSchedule()
    {
        // Arrange
        var schedule = new DoNotDisturbSchedule
        {
            UserId = _testUserId,
            DayOfWeek = 7,
            StartTime = "22:00",
            EndTime = "08:00",
            IsEnabled = true,
            AllowCritical = true
        };

        // Act
        var saved = await _preferenceService.SaveDndScheduleAsync(schedule);

        // Assert
        Assert.NotNull(saved);
        Assert.True(saved.DoNotDisturbScheduleId > 0);
        Assert.Equal("22:00", saved.StartTime);
        Assert.Equal("08:00", saved.EndTime);
    }

    [Fact]
    public async Task GetDndSchedulesAsync_ReturnsUserSchedules()
    {
        // Arrange
        var schedule1 = new DoNotDisturbSchedule
        {
            UserId = _testUserId,
            DayOfWeek = 7,
            StartTime = "22:00",
            EndTime = "08:00",
            IsEnabled = true
        };

        var schedule2 = new DoNotDisturbSchedule
        {
            UserId = _testUserId,
            DayOfWeek = 6,
            StartTime = "23:00",
            EndTime = "10:00",
            IsEnabled = true
        };

        await _preferenceService.SaveDndScheduleAsync(schedule1);
        await _preferenceService.SaveDndScheduleAsync(schedule2);

        // Act
        var schedules = await _preferenceService.GetDndSchedulesAsync(_testUserId);

        // Assert
        Assert.Equal(2, schedules.Count);
        Assert.All(schedules, s => Assert.Equal(_testUserId, s.UserId));
    }

    [Fact]
    public async Task DeleteDndScheduleAsync_DeletesSchedule()
    {
        // Arrange
        var schedule = new DoNotDisturbSchedule
        {
            UserId = _testUserId,
            DayOfWeek = 7,
            StartTime = "22:00",
            EndTime = "08:00",
            IsEnabled = true
        };

        var saved = await _preferenceService.SaveDndScheduleAsync(schedule);

        // Act
        await _preferenceService.DeleteDndScheduleAsync(saved.DoNotDisturbScheduleId, _testUserId);

        // Assert
        var schedules = await _preferenceService.GetDndSchedulesAsync(_testUserId);
        Assert.Empty(schedules);
    }

    [Fact]
    public async Task SaveIntegrationAsync_CreatesNewIntegration()
    {
        // Arrange
        var integration = new NotificationIntegration
        {
            UserId = _testUserId,
            Channel = NotificationChannel.Slack,
            Configuration = "{\"webhookUrl\": \"https://hooks.slack.com/services/xxx\"}",
            IsEnabled = true
        };

        // Act
        var saved = await _preferenceService.SaveIntegrationAsync(integration);

        // Assert
        Assert.NotNull(saved);
        Assert.True(saved.NotificationIntegrationId > 0);
        Assert.Equal(NotificationChannel.Slack, saved.Channel);
    }

    [Fact]
    public async Task GetIntegrationsAsync_ReturnsUserIntegrations()
    {
        // Arrange
        var slackIntegration = new NotificationIntegration
        {
            UserId = _testUserId,
            Channel = NotificationChannel.Slack,
            Configuration = "{\"webhookUrl\": \"https://hooks.slack.com/services/xxx\"}",
            IsEnabled = true
        };

        var teamsIntegration = new NotificationIntegration
        {
            UserId = _testUserId,
            Channel = NotificationChannel.Teams,
            Configuration = "{\"webhookUrl\": \"https://outlook.office.com/webhook/xxx\"}",
            IsEnabled = true
        };

        await _preferenceService.SaveIntegrationAsync(slackIntegration);
        await _preferenceService.SaveIntegrationAsync(teamsIntegration);

        // Act
        var integrations = await _preferenceService.GetIntegrationsAsync(_testUserId);

        // Assert
        Assert.Equal(2, integrations.Count);
        Assert.Contains(integrations, i => i.Channel == NotificationChannel.Slack);
        Assert.Contains(integrations, i => i.Channel == NotificationChannel.Teams);
    }

    [Fact]
    public async Task DeleteIntegrationAsync_DeletesIntegration()
    {
        // Arrange
        var integration = new NotificationIntegration
        {
            UserId = _testUserId,
            Channel = NotificationChannel.Slack,
            Configuration = "{\"webhookUrl\": \"https://hooks.slack.com/services/xxx\"}",
            IsEnabled = true
        };

        var saved = await _preferenceService.SaveIntegrationAsync(integration);

        // Act
        await _preferenceService.DeleteIntegrationAsync(saved.NotificationIntegrationId, _testUserId);

        // Assert
        var integrations = await _preferenceService.GetIntegrationsAsync(_testUserId);
        Assert.Empty(integrations);
    }

    [Fact]
    public async Task InitializeDefaultPreferencesAsync_CreatesDefaultPreferences()
    {
        // Act
        await _preferenceService.InitializeDefaultPreferencesAsync(_testUserId);

        // Assert
        var preferences = await _preferenceService.GetUserPreferencesAsync(_testUserId);
        var dndSchedules = await _preferenceService.GetDndSchedulesAsync(_testUserId);

        Assert.NotEmpty(preferences);
        Assert.Single(dndSchedules);
        
        // Check that critical notifications have email enabled
        var lowBalancePref = preferences.FirstOrDefault(p => p.NotificationType == SystemNotificationType.LowBalance);
        Assert.NotNull(lowBalancePref);
        Assert.True(lowBalancePref.EnabledChannels.HasFlag(NotificationChannelFlags.Email));
    }

    [Fact]
    public async Task InitializeDefaultPreferencesAsync_DoesNotDuplicatePreferences()
    {
        // Act - Initialize twice
        await _preferenceService.InitializeDefaultPreferencesAsync(_testUserId);
        await _preferenceService.InitializeDefaultPreferencesAsync(_testUserId);

        // Assert
        var preferences = await _preferenceService.GetUserPreferencesAsync(_testUserId);
        
        // Should not have duplicates - group by notification type and check count
        var grouped = preferences.GroupBy(p => p.NotificationType);
        Assert.All(grouped, g => Assert.Single(g));
    }

    [Fact]
    public async Task SavePreferenceAsync_WithDigestMode_SavesDigestSettings()
    {
        // Arrange
        var preference = new NotificationPreference
        {
            UserId = _testUserId,
            NotificationType = SystemNotificationType.GoalMilestone,
            EnabledChannels = NotificationChannelFlags.InApp,
            MinimumPriority = NotificationPriority.Low,
            IsEnabled = true,
            DigestMode = true,
            DigestIntervalHours = 24
        };

        // Act
        var saved = await _preferenceService.SavePreferenceAsync(preference);

        // Assert
        Assert.True(saved.DigestMode);
        Assert.Equal(24, saved.DigestIntervalHours);
    }
}
