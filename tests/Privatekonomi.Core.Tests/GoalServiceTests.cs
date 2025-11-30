using Microsoft.EntityFrameworkCore;
using Moq;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class GoalServiceTests : IDisposable
{
    private readonly PrivatekonomyContext _context;
    private readonly GoalService _service;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IGoalMilestoneService> _milestoneServiceMock;

    public GoalServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PrivatekonomyContext(options);
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _currentUserServiceMock.Setup(x => x.UserId).Returns("test-user-id");
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        
        _milestoneServiceMock = new Mock<IGoalMilestoneService>();
        
        _service = new GoalService(_context, _currentUserServiceMock.Object, _milestoneServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task UpdateGoalPrioritiesAsync_ShouldUpdateMultipleGoalPriorities()
    {
        // Arrange
        var goal1 = new Goal
        {
            Name = "Goal 1",
            TargetAmount = 10000m,
            Priority = 1,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        var goal2 = new Goal
        {
            Name = "Goal 2",
            TargetAmount = 20000m,
            Priority = 2,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        
        _context.Goals.AddRange(goal1, goal2);
        await _context.SaveChangesAsync();

        var priorities = new Dictionary<int, int>
        {
            { goal1.GoalId, 3 },
            { goal2.GoalId, 1 }
        };

        // Act
        var result = await _service.UpdateGoalPrioritiesAsync(priorities);

        // Assert
        Assert.True(result);
        var updatedGoal1 = await _context.Goals.FindAsync(goal1.GoalId);
        var updatedGoal2 = await _context.Goals.FindAsync(goal2.GoalId);
        Assert.Equal(3, updatedGoal1!.Priority);
        Assert.Equal(1, updatedGoal2!.Priority);
    }

    [Fact]
    public void CalculateCompletionDate_WithValidMonthlySavings_ReturnsCorrectDate()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 12000m,
            CurrentAmount = 0m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        var monthlySavings = 1000m;

        // Act
        var completionDate = _service.CalculateCompletionDate(goal, monthlySavings);

        // Assert
        Assert.NotNull(completionDate);
        var expectedMonths = 12; // 12000 / 1000 = 12 months
        var expectedDate = DateTime.UtcNow.AddMonths(expectedMonths);
        Assert.Equal(expectedDate.Year, completionDate!.Value.Year);
        Assert.Equal(expectedDate.Month, completionDate.Value.Month);
    }

    [Fact]
    public void CalculateCompletionDate_WithZeroMonthlySavings_ReturnsNull()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 12000m,
            CurrentAmount = 0m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };

        // Act
        var completionDate = _service.CalculateCompletionDate(goal, 0m);

        // Assert
        Assert.Null(completionDate);
    }

    [Fact]
    public void CalculateCompletionDate_WithGoalAlreadyCompleted_ReturnsCurrentDate()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000m,
            CurrentAmount = 10000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };

        // Act
        var completionDate = _service.CalculateCompletionDate(goal, 1000m);

        // Assert
        Assert.NotNull(completionDate);
        Assert.True((completionDate.Value - DateTime.UtcNow).TotalMinutes < 1);
    }

    [Fact]
    public void CalculateMonthsToCompletion_WithValidMonthlySavings_ReturnsCorrectMonths()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000m,
            CurrentAmount = 2000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        var monthlySavings = 1000m;

        // Act
        var months = _service.CalculateMonthsToCompletion(goal, monthlySavings);

        // Assert
        Assert.Equal(8, months); // (10000 - 2000) / 1000 = 8 months
    }

    [Fact]
    public void CalculateMonthsToCompletion_WithPartialMonth_RoundsUp()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000m,
            CurrentAmount = 0m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        var monthlySavings = 1500m;

        // Act
        var months = _service.CalculateMonthsToCompletion(goal, monthlySavings);

        // Assert
        Assert.Equal(7, months); // 10000 / 1500 = 6.67, rounds up to 7
    }

    [Fact]
    public void SimulateSavingsChange_WithIncreasedSavings_ShowsEarlierCompletion()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 12000m,
            CurrentAmount = 0m,
            MonthlyAutoSavingsAmount = 1000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        var newMonthlySavings = 1200m;

        // Act
        var result = _service.SimulateSavingsChange(goal, newMonthlySavings);

        // Assert
        Assert.Equal(1000m, result.CurrentMonthlySavings);
        Assert.Equal(1200m, result.NewMonthlySavings);
        Assert.Equal(12, result.CurrentMonthsToCompletion);
        Assert.Equal(10, result.NewMonthsToCompletion);
        Assert.Equal(2, result.MonthsDifference); // 2 months earlier
        Assert.Equal(12000m, result.RemainingAmount);
        Assert.NotNull(result.CurrentCompletionDate);
        Assert.NotNull(result.NewCompletionDate);
        Assert.True(result.NewCompletionDate < result.CurrentCompletionDate);
    }

    [Fact]
    public void SimulateSavingsChange_WithDecreasedSavings_ShowsLaterCompletion()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000m,
            CurrentAmount = 5000m,
            MonthlyAutoSavingsAmount = 1000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        var newMonthlySavings = 500m;

        // Act
        var result = _service.SimulateSavingsChange(goal, newMonthlySavings);

        // Assert
        Assert.Equal(5, result.CurrentMonthsToCompletion);
        Assert.Equal(10, result.NewMonthsToCompletion);
        Assert.Equal(-5, result.MonthsDifference); // 5 months later
        Assert.True(result.NewCompletionDate > result.CurrentCompletionDate);
    }

    [Fact]
    public void SimulateSavingsChange_WithZeroCurrentSavings_CalculatesCorrectly()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 6000m,
            CurrentAmount = 0m,
            MonthlyAutoSavingsAmount = 0m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        var newMonthlySavings = 500m;

        // Act
        var result = _service.SimulateSavingsChange(goal, newMonthlySavings);

        // Assert
        Assert.Equal(0m, result.CurrentMonthlySavings);
        Assert.Equal(500m, result.NewMonthlySavings);
        Assert.Equal(int.MaxValue, result.CurrentMonthsToCompletion);
        Assert.Equal(12, result.NewMonthsToCompletion);
        Assert.Null(result.CurrentCompletionDate);
        Assert.NotNull(result.NewCompletionDate);
    }
}
