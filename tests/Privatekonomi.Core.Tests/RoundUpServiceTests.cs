using Microsoft.EntityFrameworkCore;
using Moq;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class RoundUpServiceTests
{
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IGoalService> _mockGoalService;
    private readonly PrivatekonomyContext _context;
    private readonly RoundUpService _service;
    private const string TestUserId = "test-user-123";

    public RoundUpServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new PrivatekonomyContext(options);

        // Setup mocks
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockCurrentUserService.Setup(s => s.UserId).Returns(TestUserId);
        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);

        _mockGoalService = new Mock<IGoalService>();

        _service = new RoundUpService(_context, _mockGoalService.Object, _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task GetOrCreateSettingsAsync_CreatesNewSettings_WhenNoneExist()
    {
        // Act
        var settings = await _service.GetOrCreateSettingsAsync();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(TestUserId, settings.UserId);
        Assert.False(settings.IsEnabled);
        Assert.Equal(10M, settings.RoundUpAmount);
        Assert.True(settings.OnlyExpenses);
    }

    [Fact]
    public async Task GetOrCreateSettingsAsync_ReturnsExistingSettings_WhenAlreadyExists()
    {
        // Arrange
        var existingSettings = new RoundUpSettings
        {
            UserId = TestUserId,
            IsEnabled = true,
            RoundUpAmount = 5M,
            CreatedAt = DateTime.UtcNow
        };
        _context.RoundUpSettings.Add(existingSettings);
        await _context.SaveChangesAsync();

        // Act
        var settings = await _service.GetOrCreateSettingsAsync();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(existingSettings.RoundUpSettingsId, settings.RoundUpSettingsId);
        Assert.True(settings.IsEnabled);
        Assert.Equal(5M, settings.RoundUpAmount);
    }

    [Theory]
    [InlineData(127, 10, 3)]      // 127 -> 130 (3 kr saved)
    [InlineData(245, 10, 5)]      // 245 -> 250 (5 kr saved)
    [InlineData(587, 10, 3)]      // 587 -> 590 (3 kr saved)
    [InlineData(100, 10, 0)]      // 100 -> 100 (0 kr saved, already rounded)
    [InlineData(99.50, 10, 0.50)] // 99.50 -> 100 (0.50 kr saved)
    [InlineData(1, 10, 9)]        // 1 -> 10 (9 kr saved)
    public void CalculateRoundUp_ReturnsCorrectAmount(decimal amount, decimal roundUpTo, decimal expected)
    {
        // Act
        var result = _service.CalculateRoundUp(amount, roundUpTo);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ProcessRoundUpForTransactionAsync_CreatesRoundUpTransaction_WhenEnabled()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 1000,
            CurrentAmount = 0,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        var settings = new RoundUpSettings
        {
            UserId = TestUserId,
            IsEnabled = true,
            RoundUpAmount = 10M,
            TargetGoalId = goal.GoalId,
            OnlyExpenses = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.RoundUpSettings.Add(settings);

        var transaction = new Transaction
        {
            Amount = 127M,
            Description = "ICA Purchase",
            Date = DateTime.UtcNow,
            IsIncome = false,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var roundUp = await _service.ProcessRoundUpForTransactionAsync(transaction.TransactionId);

        // Assert
        Assert.NotNull(roundUp);
        Assert.Equal(127M, roundUp.OriginalAmount);
        Assert.Equal(130M, roundUp.RoundedAmount);
        Assert.Equal(3M, roundUp.RoundUpAmount);
        Assert.Equal(3M, roundUp.TotalSaved);
        Assert.Equal(goal.GoalId, roundUp.GoalId);
        Assert.Equal(RoundUpType.StandardRoundUp, roundUp.Type);

        // Verify goal was updated
        var updatedGoal = await _context.Goals.FindAsync(goal.GoalId);
        Assert.NotNull(updatedGoal);
        Assert.Equal(3M, updatedGoal.CurrentAmount);
    }

    [Fact]
    public async Task ProcessRoundUpForTransactionAsync_ReturnsNull_WhenDisabled()
    {
        // Arrange
        var settings = new RoundUpSettings
        {
            UserId = TestUserId,
            IsEnabled = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.RoundUpSettings.Add(settings);

        var transaction = new Transaction
        {
            Amount = 127M,
            Description = "Test",
            Date = DateTime.UtcNow,
            IsIncome = false,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var roundUp = await _service.ProcessRoundUpForTransactionAsync(transaction.TransactionId);

        // Assert
        Assert.Null(roundUp);
    }

    [Fact]
    public async Task ProcessRoundUpForTransactionAsync_SkipsIncomeTransactions_WhenOnlyExpensesEnabled()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 1000,
            CurrentAmount = 0,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        var settings = new RoundUpSettings
        {
            UserId = TestUserId,
            IsEnabled = true,
            RoundUpAmount = 10M,
            TargetGoalId = goal.GoalId,
            OnlyExpenses = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.RoundUpSettings.Add(settings);

        var transaction = new Transaction
        {
            Amount = 5000M,
            Description = "Salary",
            Date = DateTime.UtcNow,
            IsIncome = true,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var roundUp = await _service.ProcessRoundUpForTransactionAsync(transaction.TransactionId);

        // Assert
        Assert.Null(roundUp);
    }

    [Fact]
    public async Task ProcessRoundUpForTransactionAsync_IncludesEmployerMatching_WhenEnabled()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 1000,
            CurrentAmount = 0,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        var settings = new RoundUpSettings
        {
            UserId = TestUserId,
            IsEnabled = true,
            RoundUpAmount = 10M,
            TargetGoalId = goal.GoalId,
            EnableEmployerMatching = true,
            EmployerMatchingPercentage = 100M, // 100% = doubles the amount
            CreatedAt = DateTime.UtcNow
        };
        _context.RoundUpSettings.Add(settings);

        var transaction = new Transaction
        {
            Amount = 127M,
            Description = "ICA Purchase",
            Date = DateTime.UtcNow,
            IsIncome = false,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var roundUp = await _service.ProcessRoundUpForTransactionAsync(transaction.TransactionId);

        // Assert
        Assert.NotNull(roundUp);
        Assert.Equal(3M, roundUp.RoundUpAmount);
        Assert.Equal(3M, roundUp.EmployerMatchingAmount); // 100% of 3 kr
        Assert.Equal(6M, roundUp.TotalSaved); // 3 + 3

        // Verify goal was updated with total
        var updatedGoal = await _context.Goals.FindAsync(goal.GoalId);
        Assert.NotNull(updatedGoal);
        Assert.Equal(6M, updatedGoal.CurrentAmount);
    }

    [Fact]
    public async Task ProcessSalaryAutoSaveAsync_CreatesSalaryAutoSave_WhenEnabled()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000,
            CurrentAmount = 0,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        var settings = new RoundUpSettings
        {
            UserId = TestUserId,
            EnableSalaryAutoSave = true,
            SalaryAutoSavePercentage = 10M,
            TargetGoalId = goal.GoalId,
            CreatedAt = DateTime.UtcNow
        };
        _context.RoundUpSettings.Add(settings);

        var transaction = new Transaction
        {
            Amount = 5000M,
            Description = "Salary",
            Date = DateTime.UtcNow,
            IsIncome = true,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var autoSave = await _service.ProcessSalaryAutoSaveAsync(transaction.TransactionId);

        // Assert
        Assert.NotNull(autoSave);
        Assert.Equal(5000M, autoSave.OriginalAmount);
        Assert.Equal(500M, autoSave.RoundUpAmount); // 10% of 5000
        Assert.Equal(500M, autoSave.TotalSaved);
        Assert.Equal(RoundUpType.SalaryAutoSave, autoSave.Type);

        // Verify goal was updated
        var updatedGoal = await _context.Goals.FindAsync(goal.GoalId);
        Assert.NotNull(updatedGoal);
        Assert.Equal(500M, updatedGoal.CurrentAmount);
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsCorrectStatistics()
    {
        // Arrange
        var fromDate = new DateTime(2024, 1, 1);
        var toDate = new DateTime(2024, 1, 31);

        var roundUps = new List<RoundUpTransaction>
        {
            new RoundUpTransaction
            {
                OriginalAmount = 127M,
                RoundedAmount = 130M,
                RoundUpAmount = 3M,
                EmployerMatchingAmount = 3M,
                TotalSaved = 6M,
                Type = RoundUpType.StandardRoundUp,
                CreatedAt = new DateTime(2024, 1, 15),
                UserId = TestUserId
            },
            new RoundUpTransaction
            {
                OriginalAmount = 245M,
                RoundedAmount = 250M,
                RoundUpAmount = 5M,
                EmployerMatchingAmount = 5M,
                TotalSaved = 10M,
                Type = RoundUpType.StandardRoundUp,
                CreatedAt = new DateTime(2024, 1, 20),
                UserId = TestUserId
            },
            new RoundUpTransaction
            {
                OriginalAmount = 5000M,
                RoundedAmount = 5000M,
                RoundUpAmount = 500M,
                EmployerMatchingAmount = 0M,
                TotalSaved = 500M,
                Type = RoundUpType.SalaryAutoSave,
                CreatedAt = new DateTime(2024, 1, 25),
                UserId = TestUserId
            }
        };

        _context.RoundUpTransactions.AddRange(roundUps);
        await _context.SaveChangesAsync();

        // Act
        var stats = await _service.GetStatisticsAsync(fromDate, toDate);

        // Assert
        Assert.Equal(8M, stats.TotalRoundUp); // 3 + 5
        Assert.Equal(8M, stats.TotalEmployerMatching); // 3 + 5
        Assert.Equal(500M, stats.TotalSalaryAutoSave);
        Assert.Equal(516M, stats.TotalSaved); // 6 + 10 + 500
        Assert.Equal(3, stats.TransactionCount);
        Assert.Equal(4M, stats.AverageRoundUp); // (3 + 5) / 2
        Assert.Equal(5M, stats.LargestRoundUp);
    }

    [Fact]
    public async Task GetMonthlyTotalAsync_ReturnsCurrentMonthTotal()
    {
        // Arrange
        var currentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        
        var roundUps = new List<RoundUpTransaction>
        {
            new RoundUpTransaction
            {
                RoundUpAmount = 3M,
                TotalSaved = 6M,
                Type = RoundUpType.StandardRoundUp,
                CreatedAt = currentMonth.AddDays(5),
                UserId = TestUserId
            },
            new RoundUpTransaction
            {
                RoundUpAmount = 5M,
                TotalSaved = 10M,
                Type = RoundUpType.StandardRoundUp,
                CreatedAt = currentMonth.AddDays(10),
                UserId = TestUserId
            },
            // This one should not be included (previous month)
            new RoundUpTransaction
            {
                RoundUpAmount = 100M,
                TotalSaved = 200M,
                Type = RoundUpType.StandardRoundUp,
                CreatedAt = currentMonth.AddMonths(-1),
                UserId = TestUserId
            }
        };

        _context.RoundUpTransactions.AddRange(roundUps);
        await _context.SaveChangesAsync();

        // Act
        var total = await _service.GetMonthlyTotalAsync();

        // Assert
        Assert.Equal(16M, total); // 6 + 10
    }
}
