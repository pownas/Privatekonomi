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
    private const string TestUserId = "test-user-123";

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
    public async Task GetNetWorthReportAsync_WithNoData_ReturnsZeroNetWorth()
    {
        // Act
        var report = await _reportService.GetNetWorthReportAsync(TestUserId);

        // Assert
        Assert.NotNull(report);
        Assert.Equal(0, report.TotalAssets);
        Assert.Equal(0, report.TotalInvestments);
        Assert.Equal(0, report.TotalLiabilities);
        Assert.Equal(0, report.NetWorth);
        Assert.Empty(report.Assets);
        Assert.Empty(report.Liabilities);
    }

    [Fact]
    public async Task GetNetWorthReportAsync_WithAssets_CalculatesCorrectNetWorth()
    {
        // Arrange
        var asset = new Asset
        {
            UserId = TestUserId,
            Name = "Test Asset",
            Type = "Property",
            CurrentValue = 500000m,
            PurchaseValue = 400000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetNetWorthReportAsync(TestUserId);

        // Assert
        Assert.Equal(500000m, report.TotalAssets);
        Assert.Equal(0, report.TotalLiabilities);
        Assert.Equal(500000m, report.NetWorth);
        Assert.Single(report.Assets);
        Assert.Equal("Test Asset", report.Assets[0].Name);
    }

    [Fact]
    public async Task GetNetWorthReportAsync_WithInvestments_IncludesInTotalAssets()
    {
        // Arrange
        var investment = new Investment
        {
            UserId = TestUserId,
            Name = "Test Stock",
            Type = "Aktie",
            Quantity = 100,
            PurchasePrice = 50m,
            CurrentPrice = 75m,
            PurchaseDate = DateTime.UtcNow.AddMonths(-6),
            LastUpdated = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Investments.Add(investment);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetNetWorthReportAsync(TestUserId);

        // Assert
        Assert.Equal(7500m, report.TotalAssets); // 100 * 75
        Assert.Equal(7500m, report.TotalInvestments);
        Assert.Equal(7500m, report.NetWorth);
        Assert.Single(report.Assets);
        Assert.Equal("Investment", report.Assets[0].Type);
    }

    [Fact]
    public async Task GetNetWorthReportAsync_WithLoans_SubtractsFromNetWorth()
    {
        // Arrange
        var asset = new Asset
        {
            UserId = TestUserId,
            Name = "Property",
            Type = "Fastighet",
            CurrentValue = 3000000m,
            PurchaseValue = 2500000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        var loan = new Loan
        {
            UserId = TestUserId,
            Name = "Mortgage",
            Type = "Bolån",
            Amount = 2000000m,
            InterestRate = 3.5m,
            Amortization = 5000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Assets.Add(asset);
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetNetWorthReportAsync(TestUserId);

        // Assert
        Assert.Equal(3000000m, report.TotalAssets);
        Assert.Equal(2000000m, report.TotalLiabilities);
        Assert.Equal(1000000m, report.NetWorth);
        Assert.Single(report.Liabilities);
        Assert.Equal("Mortgage", report.Liabilities[0].Name);
    }

    [Fact]
    public async Task GetNetWorthReportAsync_WithMultipleUsers_FiltersCorrectly()
    {
        // Arrange
        var userAsset = new Asset
        {
            UserId = TestUserId,
            Name = "User Asset",
            Type = "Property",
            CurrentValue = 100000m,
            PurchaseValue = 80000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        var otherAsset = new Asset
        {
            UserId = "other-user",
            Name = "Other Asset",
            Type = "Property",
            CurrentValue = 500000m,
            PurchaseValue = 400000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Assets.AddRange(userAsset, otherAsset);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetNetWorthReportAsync(TestUserId);

        // Assert
        Assert.Equal(100000m, report.TotalAssets);
        Assert.Single(report.Assets);
        Assert.Equal("User Asset", report.Assets[0].Name);
    }

    [Fact]
    public async Task GetNetWorthReportAsync_ReturnsHistoricalData()
    {
        // Arrange
        var asset = new Asset
        {
            UserId = TestUserId,
            Name = "Test Asset",
            Type = "Property",
            CurrentValue = 100000m,
            PurchaseValue = 80000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetNetWorthReportAsync(TestUserId);

        // Assert
        Assert.NotNull(report.History);
        Assert.NotEmpty(report.History);
        // History should contain 12 months of data
        Assert.Equal(12, report.History.Count);
    }

    [Fact]
    public async Task GetNetWorthReportAsync_WithSnapshots_UsesSnapshotData()
    {
        // Arrange
        var snapshot = new NetWorthSnapshot
        {
            UserId = TestUserId,
            Date = DateTime.Today.AddMonths(-1),
            TotalAssets = 50000m,
            TotalLiabilities = 10000m,
            NetWorth = 40000m,
            CreatedAt = DateTime.UtcNow
        };
        _context.NetWorthSnapshots.Add(snapshot);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetNetWorthReportAsync(TestUserId);

        // Assert
        Assert.NotNull(report.History);
        Assert.NotEmpty(report.History);
    }

    [Fact]
    public async Task GetNetWorthReportAsync_CalculatesPercentageChange()
    {
        // Arrange - Create two snapshots for different months
        var oldSnapshot = new NetWorthSnapshot
        {
            UserId = TestUserId,
            Date = DateTime.Today.AddMonths(-2).AddDays(-DateTime.Today.Day + 1),
            TotalAssets = 100000m,
            TotalLiabilities = 20000m,
            NetWorth = 80000m,
            CreatedAt = DateTime.UtcNow
        };
        var recentSnapshot = new NetWorthSnapshot
        {
            UserId = TestUserId,
            Date = DateTime.Today.AddMonths(-1).AddDays(-DateTime.Today.Day + 1),
            TotalAssets = 120000m,
            TotalLiabilities = 20000m,
            NetWorth = 100000m,
            CreatedAt = DateTime.UtcNow
        };
        _context.NetWorthSnapshots.AddRange(oldSnapshot, recentSnapshot);
        
        // Add current assets
        var asset = new Asset
        {
            UserId = TestUserId,
            Name = "Current Asset",
            Type = "Property",
            CurrentValue = 120000m,
            PurchaseValue = 100000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        var loan = new Loan
        {
            UserId = TestUserId,
            Name = "Loan",
            Type = "Lån",
            Amount = 20000m,
            InterestRate = 5m,
            Amortization = 500m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Assets.Add(asset);
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetNetWorthReportAsync(TestUserId);

        // Assert
        Assert.NotEqual(0, report.PercentageChange);
    }

    [Fact]
    public async Task GetNetWorthReportAsync_WithNullUserId_ReturnsAllUsersData()
    {
        // Arrange
        var asset1 = new Asset
        {
            UserId = "user1",
            Name = "Asset 1",
            Type = "Property",
            CurrentValue = 100000m,
            PurchaseValue = 80000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        var asset2 = new Asset
        {
            UserId = "user2",
            Name = "Asset 2",
            Type = "Property",
            CurrentValue = 200000m,
            PurchaseValue = 180000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Assets.AddRange(asset1, asset2);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetNetWorthReportAsync(null);

        // Assert
        Assert.Equal(300000m, report.TotalAssets); // Sum of both users
        Assert.Equal(2, report.Assets.Count);
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

    [Fact]
    public async Task GetSpendingPatternReportAsync_WithNoData_ReturnsEmptyReport()
    {
        // Arrange
        var fromDate = DateTime.Today.AddMonths(-1);
        var toDate = DateTime.Today;

        // Act
        var report = await _reportService.GetSpendingPatternReportAsync(fromDate, toDate);

        // Assert
        Assert.NotNull(report);
        Assert.Equal(0, report.TotalSpending);
        Assert.Equal(0, report.AverageMonthlySpending);
        Assert.Empty(report.CategoryDistribution);
        Assert.Empty(report.TopCategories);
        Assert.Empty(report.MonthlyData);
    }

    [Fact]
    public async Task GetSpendingPatternReportAsync_CalculatesTotalSpending()
    {
        // Arrange
        var category = new Category { CategoryId = 1, Name = "Mat", Color = "#FF0000" };
        _context.Categories.Add(category);
        
        var transaction1 = new Transaction
        {
            TransactionId = 1,
            Amount = 500m,
            Date = DateTime.Today.AddDays(-10),
            IsIncome = false,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow,
            TransactionCategories = new List<TransactionCategory>
            {
                new TransactionCategory { CategoryId = 1, Category = category }
            }
        };
        
        var transaction2 = new Transaction
        {
            TransactionId = 2,
            Amount = 300m,
            Date = DateTime.Today.AddDays(-5),
            IsIncome = false,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow,
            TransactionCategories = new List<TransactionCategory>
            {
                new TransactionCategory { CategoryId = 1, Category = category }
            }
        };

        _context.Transactions.AddRange(transaction1, transaction2);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetSpendingPatternReportAsync(
            DateTime.Today.AddMonths(-1), 
            DateTime.Today);

        // Assert
        Assert.Equal(800m, report.TotalSpending);
        Assert.NotEmpty(report.CategoryDistribution);
        Assert.Single(report.CategoryDistribution);
        Assert.Equal("Mat", report.CategoryDistribution[0].CategoryName);
        Assert.Equal(800m, report.CategoryDistribution[0].Amount);
    }

    [Fact]
    public async Task GetSpendingPatternReportAsync_CalculatesCategoryPercentages()
    {
        // Arrange
        var category1 = new Category { CategoryId = 1, Name = "Mat", Color = "#FF0000" };
        var category2 = new Category { CategoryId = 2, Name = "Transport", Color = "#00FF00" };
        _context.Categories.AddRange(category1, category2);

        var transaction1 = new Transaction
        {
            TransactionId = 1,
            Amount = 700m,
            Date = DateTime.Today.AddDays(-10),
            IsIncome = false,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow,
            TransactionCategories = new List<TransactionCategory>
            {
                new TransactionCategory { CategoryId = 1, Category = category1 }
            }
        };

        var transaction2 = new Transaction
        {
            TransactionId = 2,
            Amount = 300m,
            Date = DateTime.Today.AddDays(-5),
            IsIncome = false,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow,
            TransactionCategories = new List<TransactionCategory>
            {
                new TransactionCategory { CategoryId = 2, Category = category2 }
            }
        };

        _context.Transactions.AddRange(transaction1, transaction2);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetSpendingPatternReportAsync(
            DateTime.Today.AddMonths(-1),
            DateTime.Today);

        // Assert
        Assert.Equal(1000m, report.TotalSpending);
        Assert.Equal(2, report.CategoryDistribution.Count);
        
        var matCategory = report.CategoryDistribution.First(c => c.CategoryName == "Mat");
        Assert.Equal(70m, matCategory.Percentage); // 700/1000 * 100
        
        var transportCategory = report.CategoryDistribution.First(c => c.CategoryName == "Transport");
        Assert.Equal(30m, transportCategory.Percentage); // 300/1000 * 100
    }

    [Fact]
    public async Task GetSpendingPatternReportAsync_IdentifiesTopCategories()
    {
        // Arrange - Create 6 categories with different amounts
        var categories = new List<Category>
        {
            new Category { CategoryId = 1, Name = "Cat1", Color = "#FF0000" },
            new Category { CategoryId = 2, Name = "Cat2", Color = "#00FF00" },
            new Category { CategoryId = 3, Name = "Cat3", Color = "#0000FF" },
            new Category { CategoryId = 4, Name = "Cat4", Color = "#FFFF00" },
            new Category { CategoryId = 5, Name = "Cat5", Color = "#FF00FF" },
            new Category { CategoryId = 6, Name = "Cat6", Color = "#00FFFF" }
        };
        _context.Categories.AddRange(categories);

        var amounts = new[] { 1000m, 800m, 600m, 400m, 200m, 100m };
        for (int i = 0; i < 6; i++)
        {
            var transaction = new Transaction
            {
                TransactionId = i + 1,
                Amount = amounts[i],
                Date = DateTime.Today.AddDays(-5),
                IsIncome = false,
                UserId = TestUserId,
                CreatedAt = DateTime.UtcNow,
                ValidFrom = DateTime.UtcNow,
                TransactionCategories = new List<TransactionCategory>
                {
                    new TransactionCategory { CategoryId = i + 1, Category = categories[i] }
                }
            };
            _context.Transactions.Add(transaction);
        }
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetSpendingPatternReportAsync(
            DateTime.Today.AddMonths(-1),
            DateTime.Today);

        // Assert - Should only have top 5 categories
        Assert.Equal(5, report.TopCategories.Count);
        Assert.Equal("Cat1", report.TopCategories[0].CategoryName);
        Assert.Equal(1000m, report.TopCategories[0].Amount);
        Assert.Equal("Cat5", report.TopCategories[4].CategoryName);
        Assert.Equal(200m, report.TopCategories[4].Amount);
    }

    [Fact]
    public async Task GetSpendingPatternReportAsync_CalculatesMonthlyData()
    {
        // Arrange
        var category = new Category { CategoryId = 1, Name = "Mat", Color = "#FF0000" };
        _context.Categories.Add(category);

        // Add transactions across 2 months
        var month1Transaction = new Transaction
        {
            TransactionId = 1,
            Amount = 500m,
            Date = DateTime.Today.AddMonths(-2).AddDays(5),
            IsIncome = false,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow,
            TransactionCategories = new List<TransactionCategory>
            {
                new TransactionCategory { CategoryId = 1, Category = category }
            }
        };

        var month2Transaction = new Transaction
        {
            TransactionId = 2,
            Amount = 700m,
            Date = DateTime.Today.AddMonths(-1).AddDays(5),
            IsIncome = false,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow,
            TransactionCategories = new List<TransactionCategory>
            {
                new TransactionCategory { CategoryId = 1, Category = category }
            }
        };

        _context.Transactions.AddRange(month1Transaction, month2Transaction);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetSpendingPatternReportAsync(
            DateTime.Today.AddMonths(-3),
            DateTime.Today);

        // Assert
        Assert.NotEmpty(report.MonthlyData);
        Assert.Equal(2, report.MonthlyData.Count);
        Assert.Equal(500m, report.MonthlyData[0].TotalAmount);
        Assert.Equal(700m, report.MonthlyData[1].TotalAmount);
    }

    [Fact]
    public async Task GetSpendingPatternReportAsync_DetectsTrends()
    {
        // Arrange
        var category = new Category { CategoryId = 1, Name = "Mat", Color = "#FF0000" };
        _context.Categories.Add(category);

        // Create increasing trend: 100, 200, 300, 400, 500, 600
        for (int i = 0; i < 6; i++)
        {
            var transaction = new Transaction
            {
                TransactionId = i + 1,
                Amount = (i + 1) * 100m,
                Date = DateTime.Today.AddMonths(-6 + i).AddDays(5),
                IsIncome = false,
                UserId = TestUserId,
                CreatedAt = DateTime.UtcNow,
                ValidFrom = DateTime.UtcNow,
                TransactionCategories = new List<TransactionCategory>
                {
                    new TransactionCategory { CategoryId = 1, Category = category }
                }
            };
            _context.Transactions.Add(transaction);
        }
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetSpendingPatternReportAsync(
            DateTime.Today.AddMonths(-7),
            DateTime.Today);

        // Assert
        Assert.NotEmpty(report.Trends);
        var overallTrend = report.Trends.FirstOrDefault(t => t.CategoryName == "Total utgifter");
        Assert.NotNull(overallTrend);
        Assert.Equal("Increasing", overallTrend.TrendType);
    }

    [Fact]
    public async Task GetSpendingPatternReportAsync_GeneratesRecommendations()
    {
        // Arrange
        var category = new Category { CategoryId = 1, Name = "Mat", Color = "#FF0000" };
        _context.Categories.Add(category);

        // Create a category with high percentage (> 20%)
        var transaction = new Transaction
        {
            TransactionId = 1,
            Amount = 5000m,
            Date = DateTime.Today.AddDays(-5),
            IsIncome = false,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow,
            TransactionCategories = new List<TransactionCategory>
            {
                new TransactionCategory { CategoryId = 1, Category = category }
            }
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetSpendingPatternReportAsync(
            DateTime.Today.AddMonths(-1),
            DateTime.Today);

        // Assert
        Assert.NotEmpty(report.Recommendations);
        // Should have recommendation about high percentage category
        var highPercentageRec = report.Recommendations.FirstOrDefault(r => r.Type == "BudgetAlert");
        Assert.NotNull(highPercentageRec);
    }

    [Fact]
    public async Task GetSpendingPatternReportAsync_FiltersByHousehold()
    {
        // Arrange
        var category = new Category { CategoryId = 1, Name = "Mat", Color = "#FF0000" };
        _context.Categories.Add(category);

        var household1Transaction = new Transaction
        {
            TransactionId = 1,
            Amount = 500m,
            Date = DateTime.Today.AddDays(-5),
            IsIncome = false,
            HouseholdId = 1,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow,
            TransactionCategories = new List<TransactionCategory>
            {
                new TransactionCategory { CategoryId = 1, Category = category }
            }
        };

        var household2Transaction = new Transaction
        {
            TransactionId = 2,
            Amount = 300m,
            Date = DateTime.Today.AddDays(-5),
            IsIncome = false,
            HouseholdId = 2,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow,
            TransactionCategories = new List<TransactionCategory>
            {
                new TransactionCategory { CategoryId = 1, Category = category }
            }
        };

        _context.Transactions.AddRange(household1Transaction, household2Transaction);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetSpendingPatternReportAsync(
            DateTime.Today.AddMonths(-1),
            DateTime.Today,
            householdId: 1);

        // Assert
        Assert.Equal(500m, report.TotalSpending);
    }
}
