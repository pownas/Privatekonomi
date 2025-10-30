using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class ReportServiceTests : IDisposable
{
    private readonly PrivatekonomyContext _context;
    private readonly ReportService _reportService;

    public ReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PrivatekonomyContext(options);
        _reportService = new ReportService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetPeriodComparisonAsync_WithTransactions_ReturnsCorrectComparison()
    {
        // Arrange
        var referenceDate = new DateTime(2025, 1, 15);
        
        // Current month (January 2025) - 1,000 income, 600 expenses
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 1000m,
            Date = new DateTime(2025, 1, 10),
            Description = "Salary January",
            IsIncome = true
        });
        
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 600m,
            Date = new DateTime(2025, 1, 12),
            Description = "Groceries January",
            IsIncome = false
        });

        // Previous month (December 2024) - 1,000 income, 800 expenses
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 1000m,
            Date = new DateTime(2024, 12, 10),
            Description = "Salary December",
            IsIncome = true
        });
        
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 800m,
            Date = new DateTime(2024, 12, 15),
            Description = "Groceries December",
            IsIncome = false
        });

        // Year ago (January 2024) - 900 income, 700 expenses
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 900m,
            Date = new DateTime(2024, 1, 10),
            Description = "Salary January 2024",
            IsIncome = true
        });
        
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 700m,
            Date = new DateTime(2024, 1, 12),
            Description = "Groceries January 2024",
            IsIncome = false
        });

        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetPeriodComparisonAsync(referenceDate);

        // Assert
        Assert.NotNull(result);
        
        // Check income comparison
        Assert.Equal(1000m, result.Income.CurrentPeriod);
        Assert.Equal(1000m, result.Income.PreviousPeriod);
        Assert.Equal(900m, result.Income.YearAgoPeriod);
        Assert.Equal(0m, result.Income.ChangeFromPrevious);
        Assert.Equal(100m, result.Income.ChangeFromYearAgo);
        Assert.Equal(0m, result.Income.PercentageChangeFromPrevious);
        Assert.Equal(11.1m, Math.Round(result.Income.PercentageChangeFromYearAgo, 1));
        
        // Check expenses comparison
        Assert.Equal(600m, result.Expenses.CurrentPeriod);
        Assert.Equal(800m, result.Expenses.PreviousPeriod);
        Assert.Equal(700m, result.Expenses.YearAgoPeriod);
        Assert.Equal(-200m, result.Expenses.ChangeFromPrevious);
        Assert.Equal(-100m, result.Expenses.ChangeFromYearAgo);
        Assert.Equal(-25m, result.Expenses.PercentageChangeFromPrevious);
        Assert.Equal(-14.3m, Math.Round(result.Expenses.PercentageChangeFromYearAgo, 1));
        Assert.Equal("Improving", result.Expenses.TrendDirection);
        
        // Check net flow comparison
        Assert.Equal(400m, result.NetFlow.CurrentPeriod); // 1000 - 600
        Assert.Equal(200m, result.NetFlow.PreviousPeriod); // 1000 - 800
        Assert.Equal(200m, result.NetFlow.YearAgoPeriod); // 900 - 700
        Assert.Equal(200m, result.NetFlow.ChangeFromPrevious);
        Assert.Equal(100m, result.NetFlow.PercentageChangeFromPrevious);
        Assert.Equal("Improving", result.NetFlow.TrendDirection);
    }

    [Fact]
    public async Task GetPeriodComparisonAsync_NoTransactions_ReturnsZeroValues()
    {
        // Arrange
        var referenceDate = new DateTime(2025, 1, 15);

        // Act
        var result = await _reportService.GetPeriodComparisonAsync(referenceDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0m, result.Income.CurrentPeriod);
        Assert.Equal(0m, result.Income.PreviousPeriod);
        Assert.Equal(0m, result.Income.YearAgoPeriod);
        Assert.Equal(0m, result.Expenses.CurrentPeriod);
        Assert.Equal(0m, result.Expenses.PreviousPeriod);
        Assert.Equal(0m, result.Expenses.YearAgoPeriod);
    }

    [Fact]
    public async Task GetPeriodComparisonAsync_IncreasingExpenses_ShowsWorsening()
    {
        // Arrange
        var referenceDate = new DateTime(2025, 1, 15);
        
        // Current month - higher expenses
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 1000m,
            Date = new DateTime(2025, 1, 10),
            Description = "Expensive purchase",
            IsIncome = false
        });

        // Previous month - lower expenses
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 500m,
            Date = new DateTime(2024, 12, 10),
            Description = "Normal purchase",
            IsIncome = false
        });

        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetPeriodComparisonAsync(referenceDate);

        // Assert
        Assert.Equal("Worsening", result.Expenses.TrendDirection);
        Assert.True(result.Expenses.ChangeFromPrevious > 0);
        Assert.True(result.Expenses.PercentageChangeFromPrevious > 0);
    }

    [Fact]
    public async Task GetPeriodComparisonAsync_StableExpenses_ShowsStable()
    {
        // Arrange
        var referenceDate = new DateTime(2025, 1, 15);
        
        // Current month
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 1000m,
            Date = new DateTime(2025, 1, 10),
            Description = "Purchase",
            IsIncome = false
        });

        // Previous month - very similar
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 1020m,
            Date = new DateTime(2024, 12, 10),
            Description = "Purchase",
            IsIncome = false
        });

        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetPeriodComparisonAsync(referenceDate);

        // Assert
        // Change is less than 5%, so should be "Stable"
        Assert.Equal("Stable", result.Expenses.TrendDirection);
    }

    [Fact]
    public async Task GetPeriodComparisonAsync_GeneratesSparklineData()
    {
        // Arrange
        var referenceDate = new DateTime(2025, 1, 15);
        
        // Add transactions for the last 6 months with increasing amounts
        for (int i = 5; i >= 0; i--)
        {
            var date = new DateTime(2025, 1, 1).AddMonths(-i).AddDays(5);
            await _context.Transactions.AddAsync(new Transaction
            {
                Amount = 100m * (6 - i), // Increasing: 100, 200, 300, 400, 500, 600
                Date = date,
                Description = $"Expense {i}",
                IsIncome = false
            });
        }

        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetPeriodComparisonAsync(referenceDate);

        // Assert
        Assert.NotNull(result.SparklineData);
        Assert.Equal(6, result.SparklineData.Count);
        // Verify we have increasing trend (first value should be less than last)
        Assert.True(result.SparklineData[0] < result.SparklineData[5]);
        Assert.Equal(100, result.SparklineData[0]);
        Assert.Equal(600, result.SparklineData[5]);
    }
}
