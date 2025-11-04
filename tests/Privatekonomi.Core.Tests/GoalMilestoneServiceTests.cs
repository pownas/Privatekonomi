using Microsoft.EntityFrameworkCore;
using Moq;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class GoalMilestoneServiceTests : IDisposable
{
    private readonly PrivatekonomyContext _context;
    private readonly GoalMilestoneService _service;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public GoalMilestoneServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PrivatekonomyContext(options);
        _notificationServiceMock = new Mock<INotificationService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _currentUserServiceMock.Setup(x => x.UserId).Returns("test-user-id");
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        
        _service = new GoalMilestoneService(_context, _notificationServiceMock.Object, _currentUserServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAutomaticMilestonesAsync_CreatesCorrectMilestones()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000m,
            CurrentAmount = 0m,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        // Act
        await _service.CreateAutomaticMilestonesAsync(goal.GoalId);

        // Assert
        var milestones = await _context.GoalMilestones
            .Where(m => m.GoalId == goal.GoalId)
            .OrderBy(m => m.Percentage)
            .ToListAsync();

        Assert.Equal(4, milestones.Count);
        Assert.Equal(25, milestones[0].Percentage);
        Assert.Equal(2500m, milestones[0].TargetAmount);
        Assert.Equal(50, milestones[1].Percentage);
        Assert.Equal(5000m, milestones[1].TargetAmount);
        Assert.Equal(75, milestones[2].Percentage);
        Assert.Equal(7500m, milestones[2].TargetAmount);
        Assert.Equal(100, milestones[3].Percentage);
        Assert.Equal(10000m, milestones[3].TargetAmount);
        Assert.All(milestones, m => Assert.True(m.IsAutomatic));
    }

    [Fact]
    public async Task CreateAutomaticMilestonesAsync_DoesNotDuplicateMilestones()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000m,
            CurrentAmount = 0m,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        // Act
        await _service.CreateAutomaticMilestonesAsync(goal.GoalId);
        await _service.CreateAutomaticMilestonesAsync(goal.GoalId); // Second call

        // Assert
        var milestones = await _context.GoalMilestones
            .Where(m => m.GoalId == goal.GoalId && m.IsAutomatic)
            .ToListAsync();

        Assert.Equal(4, milestones.Count); // Should still be 4, not 8
    }

    [Fact]
    public async Task CreateCustomMilestoneAsync_SuccessfullyCreatesCustomMilestone()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000m,
            CurrentAmount = 3000m,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        var customMilestone = new GoalMilestone
        {
            GoalId = goal.GoalId,
            TargetAmount = 3500m,
            Percentage = 35,
            Description = "Custom milestone at 35%"
        };

        // Act
        var result = await _service.CreateCustomMilestoneAsync(customMilestone);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsAutomatic);
        Assert.Equal(35, result.Percentage);
        Assert.Equal(3500m, result.TargetAmount);
        Assert.False(result.IsReached); // Should not be reached yet (3000 < 3500)
    }

    [Fact]
    public async Task CreateCustomMilestoneAsync_MarksAsReachedIfAlreadyMet()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000m,
            CurrentAmount = 4000m,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        var customMilestone = new GoalMilestone
        {
            GoalId = goal.GoalId,
            TargetAmount = 3000m,
            Percentage = 30,
            Description = "Custom milestone at 30%"
        };

        // Act
        var result = await _service.CreateCustomMilestoneAsync(customMilestone);

        // Assert
        Assert.True(result.IsReached);
        Assert.NotNull(result.ReachedAt);
    }

    [Fact]
    public async Task CheckAndUpdateMilestonesAsync_MarksReachedMilestones()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000m,
            CurrentAmount = 0m,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        await _service.CreateAutomaticMilestonesAsync(goal.GoalId);

        // Act
        var reachedMilestones = await _service.CheckAndUpdateMilestonesAsync(goal.GoalId, 6000m);

        // Assert
        var milestones = await _context.GoalMilestones
            .Where(m => m.GoalId == goal.GoalId)
            .OrderBy(m => m.Percentage)
            .ToListAsync();

        Assert.Equal(2, reachedMilestones.Count()); // 25% and 50% should be reached
        Assert.True(milestones[0].IsReached); // 25%
        Assert.True(milestones[1].IsReached); // 50%
        Assert.False(milestones[2].IsReached); // 75%
        Assert.False(milestones[3].IsReached); // 100%
        Assert.NotNull(milestones[0].ReachedAt);
        Assert.NotNull(milestones[1].ReachedAt);
    }

    [Fact]
    public async Task CheckAndUpdateMilestonesAsync_SendsNotifications()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000m,
            CurrentAmount = 0m,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        await _service.CreateAutomaticMilestonesAsync(goal.GoalId);

        // Act
        await _service.CheckAndUpdateMilestonesAsync(goal.GoalId, 3000m);

        // Assert
        _notificationServiceMock.Verify(
            x => x.SendNotificationAsync(
                It.IsAny<string>(),
                SystemNotificationType.GoalMilestone,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NotificationPriority>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once); // Should send notification for 25% milestone
    }

    [Fact]
    public async Task GetMilestonesByGoalIdAsync_ReturnsOrderedMilestones()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000m,
            CurrentAmount = 0m,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        await _service.CreateAutomaticMilestonesAsync(goal.GoalId);

        // Act
        var milestones = await _service.GetMilestonesByGoalIdAsync(goal.GoalId);

        // Assert
        var milestoneList = milestones.ToList();
        Assert.Equal(4, milestoneList.Count);
        Assert.Equal(25, milestoneList[0].Percentage);
        Assert.Equal(50, milestoneList[1].Percentage);
        Assert.Equal(75, milestoneList[2].Percentage);
        Assert.Equal(100, milestoneList[3].Percentage);
    }

    [Fact]
    public async Task GetReachedMilestonesAsync_ReturnsOnlyReachedMilestones()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000m,
            CurrentAmount = 0m,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        await _service.CreateAutomaticMilestonesAsync(goal.GoalId);
        await _service.CheckAndUpdateMilestonesAsync(goal.GoalId, 5500m);

        // Act
        var reachedMilestones = await _service.GetReachedMilestonesAsync(goal.GoalId);

        // Assert
        var milestoneList = reachedMilestones.ToList();
        Assert.Equal(2, milestoneList.Count); // 25% and 50%
        Assert.All(milestoneList, m => Assert.True(m.IsReached));
    }

    [Fact]
    public async Task DeleteMilestoneAsync_SuccessfullyDeletesMilestone()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000m,
            CurrentAmount = 0m,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        var milestone = new GoalMilestone
        {
            GoalId = goal.GoalId,
            TargetAmount = 3000m,
            Percentage = 30,
            Description = "Custom milestone",
            IsAutomatic = false,
            IsReached = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.GoalMilestones.Add(milestone);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteMilestoneAsync(milestone.GoalMilestoneId);

        // Assert
        var deletedMilestone = await _context.GoalMilestones.FindAsync(milestone.GoalMilestoneId);
        Assert.Null(deletedMilestone);
    }

    [Fact]
    public async Task GetMilestoneByIdAsync_ReturnsCorrectMilestone()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000m,
            CurrentAmount = 0m,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        await _service.CreateAutomaticMilestonesAsync(goal.GoalId);
        var allMilestones = await _context.GoalMilestones.Where(m => m.GoalId == goal.GoalId).ToListAsync();
        var firstMilestone = allMilestones.First();

        // Act
        var result = await _service.GetMilestoneByIdAsync(firstMilestone.GoalMilestoneId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(firstMilestone.GoalMilestoneId, result.GoalMilestoneId);
        Assert.Equal(firstMilestone.Percentage, result.Percentage);
    }
}
