using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;

namespace Privatekonomi.Core.Tests;

public class MetricsServiceTests
{
    private readonly Mock<ILogger<MetricsService>> _mockLogger;

    public MetricsServiceTests()
    {
        _mockLogger = new Mock<ILogger<MetricsService>>();
    }

    private PrivatekonomyContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new PrivatekonomyContext(options);
    }

    [Fact]
    public async Task GetCurrentMetricsAsync_ReturnsMetrics()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new MetricsService(context, _mockLogger.Object);

        // Add test users
        var now = DateTime.UtcNow;
        context.Users.AddRange(
            new ApplicationUser { Id = "user1", Email = "user1@test.com", CreatedAt = now.AddMonths(-2), LastLoginAt = now },
            new ApplicationUser { Id = "user2", Email = "user2@test.com", CreatedAt = now.AddMonths(-1), LastLoginAt = now.AddDays(-15) },
            new ApplicationUser { Id = "user3", Email = "user3@test.com", CreatedAt = now.AddDays(-5), LastLoginAt = now }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetCurrentMetricsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.UserMetrics);
        Assert.NotNull(result.EngagementMetrics);
        Assert.NotNull(result.PerformanceMetrics);
        Assert.NotNull(result.SecurityMetrics);
        Assert.Equal(3, result.UserMetrics.TotalUsers);
    }

    [Fact]
    public async Task GetCurrentMetricsAsync_CalculatesMAUCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new MetricsService(context, _mockLogger.Object);

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        
        // Add users - 2 active this month, 1 inactive
        context.Users.AddRange(
            new ApplicationUser { Id = "user1", Email = "user1@test.com", CreatedAt = now.AddMonths(-2), LastLoginAt = now },
            new ApplicationUser { Id = "user2", Email = "user2@test.com", CreatedAt = now.AddMonths(-2), LastLoginAt = now.AddDays(-5) },
            new ApplicationUser { Id = "user3", Email = "user3@test.com", CreatedAt = now.AddMonths(-2), LastLoginAt = now.AddMonths(-2) }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetCurrentMetricsAsync();

        // Assert
        Assert.Equal(2, result.UserMetrics.MAU);
        Assert.Equal(3, result.UserMetrics.TotalUsers);
    }

    [Fact]
    public async Task GetCurrentMetricsAsync_CalculatesDAUCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new MetricsService(context, _mockLogger.Object);

        var now = DateTime.UtcNow;
        var today = now.Date;
        
        // Add users - 1 active today, 2 not active today
        context.Users.AddRange(
            new ApplicationUser { Id = "user1", Email = "user1@test.com", CreatedAt = now.AddMonths(-2), LastLoginAt = today.AddHours(10) },
            new ApplicationUser { Id = "user2", Email = "user2@test.com", CreatedAt = now.AddMonths(-2), LastLoginAt = today.AddDays(-1) },
            new ApplicationUser { Id = "user3", Email = "user3@test.com", CreatedAt = now.AddMonths(-2), LastLoginAt = today.AddDays(-5) }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetCurrentMetricsAsync();

        // Assert
        Assert.Equal(1, result.UserMetrics.DAU);
    }

    [Fact]
    public async Task GetCurrentMetricsAsync_CalculatesTransactionsPerUserCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new MetricsService(context, _mockLogger.Object);

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        
        // Add users
        var user1 = new ApplicationUser { Id = "user1", Email = "user1@test.com", CreatedAt = now.AddMonths(-2), LastLoginAt = now };
        var user2 = new ApplicationUser { Id = "user2", Email = "user2@test.com", CreatedAt = now.AddMonths(-2), LastLoginAt = now };
        context.Users.AddRange(user1, user2);
        
        // Add transactions this month - 6 transactions for 2 active users = 3 per user
        context.Transactions.AddRange(
            new Transaction { TransactionId = 1, UserId = "user1", Date = startOfMonth.AddDays(1), Amount = 100, Description = "Test1" },
            new Transaction { TransactionId = 2, UserId = "user1", Date = startOfMonth.AddDays(2), Amount = 200, Description = "Test2" },
            new Transaction { TransactionId = 3, UserId = "user1", Date = startOfMonth.AddDays(3), Amount = 300, Description = "Test3" },
            new Transaction { TransactionId = 4, UserId = "user2", Date = startOfMonth.AddDays(4), Amount = 400, Description = "Test4" },
            new Transaction { TransactionId = 5, UserId = "user2", Date = startOfMonth.AddDays(5), Amount = 500, Description = "Test5" },
            new Transaction { TransactionId = 6, UserId = "user2", Date = startOfMonth.AddDays(6), Amount = 600, Description = "Test6" }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetCurrentMetricsAsync();

        // Assert
        Assert.Equal(3m, result.EngagementMetrics.TransactionsPerUser);
        Assert.Equal(6, result.EngagementMetrics.TotalTransactionsThisMonth);
    }

    [Fact]
    public async Task GetCurrentMetricsAsync_CalculatesNewUsersThisMonthCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new MetricsService(context, _mockLogger.Object);

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        
        // Add users - 2 new this month, 1 old
        context.Users.AddRange(
            new ApplicationUser { Id = "user1", Email = "user1@test.com", CreatedAt = startOfMonth.AddDays(5), LastLoginAt = now },
            new ApplicationUser { Id = "user2", Email = "user2@test.com", CreatedAt = startOfMonth.AddDays(10), LastLoginAt = now },
            new ApplicationUser { Id = "user3", Email = "user3@test.com", CreatedAt = startOfMonth.AddMonths(-2), LastLoginAt = now }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetCurrentMetricsAsync();

        // Assert
        Assert.Equal(2, result.UserMetrics.NewUsersThisMonth);
        Assert.Equal(3, result.UserMetrics.TotalUsers);
    }

    [Fact]
    public async Task GetMetricsForPeriodAsync_ReturnsMetricsForSpecificPeriod()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new MetricsService(context, _mockLogger.Object);

        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        
        // Add users with activity in January
        context.Users.AddRange(
            new ApplicationUser { Id = "user1", Email = "user1@test.com", CreatedAt = startDate.AddDays(-30), LastLoginAt = startDate.AddDays(10) },
            new ApplicationUser { Id = "user2", Email = "user2@test.com", CreatedAt = startDate.AddDays(-30), LastLoginAt = startDate.AddDays(20) }
        );
        
        // Add transactions in January
        context.Transactions.AddRange(
            new Transaction { TransactionId = 1, UserId = "user1", Date = startDate.AddDays(5), Amount = 100, Description = "Test1" },
            new Transaction { TransactionId = 2, UserId = "user2", Date = startDate.AddDays(15), Amount = 200, Description = "Test2" }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetMetricsForPeriodAsync(startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.UserMetrics.MAU);
        Assert.Equal(2, result.EngagementMetrics.TotalTransactionsThisMonth);
    }

    [Fact]
    public async Task GetHistoricalMetricsAsync_ReturnsCorrectNumberOfSnapshots()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new MetricsService(context, _mockLogger.Object);

        // Add some test data
        var now = DateTime.UtcNow;
        context.Users.Add(new ApplicationUser 
        { 
            Id = "user1", 
            Email = "user1@test.com", 
            CreatedAt = now.AddMonths(-6), 
            LastLoginAt = now 
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetHistoricalMetricsAsync(MetricsPeriodType.Monthly, 6);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, result.Count);
        Assert.All(result, snapshot => Assert.Equal(MetricsPeriodType.Monthly, snapshot.PeriodType));
    }

    [Fact]
    public async Task GetHistoricalMetricsAsync_QuarterlyPeriod_ReturnsQuarterlySnapshots()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new MetricsService(context, _mockLogger.Object);

        // Add test data
        var now = DateTime.UtcNow;
        context.Users.Add(new ApplicationUser 
        { 
            Id = "user1", 
            Email = "user1@test.com", 
            CreatedAt = now.AddMonths(-12), 
            LastLoginAt = now 
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetHistoricalMetricsAsync(MetricsPeriodType.Quarterly, 4);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        Assert.All(result, snapshot => Assert.Equal(MetricsPeriodType.Quarterly, snapshot.PeriodType));
    }

    [Fact]
    public async Task GetCurrentMetricsAsync_WithNoData_ReturnsZeroMetrics()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new MetricsService(context, _mockLogger.Object);

        // Act - no data added
        var result = await service.GetCurrentMetricsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.UserMetrics.TotalUsers);
        Assert.Equal(0, result.UserMetrics.MAU);
        Assert.Equal(0, result.UserMetrics.DAU);
        Assert.Equal(0m, result.EngagementMetrics.TransactionsPerUser);
    }

    [Fact]
    public async Task GetCurrentMetricsAsync_GDPRCompliance_AlwaysReturns100Percent()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new MetricsService(context, _mockLogger.Object);

        // Act
        var result = await service.GetCurrentMetricsAsync();

        // Assert
        Assert.Equal(100m, result.SecurityMetrics.GDPRCompliancePercent);
    }

    [Fact]
    public async Task GetCurrentMetricsAsync_PerformanceMetrics_ReturnsExpectedValues()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new MetricsService(context, _mockLogger.Object);

        // Act
        var result = await service.GetCurrentMetricsAsync();

        // Assert
        Assert.True(result.PerformanceMetrics.UptimePercent >= 99.0m);
        Assert.True(result.PerformanceMetrics.AveragePageLoadTime > 0);
        Assert.True(result.PerformanceMetrics.LighthouseScore >= 0);
        Assert.True(result.PerformanceMetrics.CrashRate >= 0);
    }
}
