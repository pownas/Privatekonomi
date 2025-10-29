using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class HealthScoreServiceTests : IDisposable
{
    private readonly PrivatekonomyContext _context;
    private readonly ReportService _reportService;

    public HealthScoreServiceTests()
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
    public async Task GetHealthScoreAsync_NoData_ReturnsLowScore()
    {
        // Act
        var result = await _reportService.GetHealthScoreAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalScore >= 0 && result.TotalScore <= 100);
        // 0% savings rate gets 5 points (not negative at least)
        Assert.Equal(5, result.SavingsRate.Score);
        Assert.Equal(0, result.EmergencyFund.Score);
        Assert.Equal(0, result.IncomeStability.Score); // No income data
    }

    [Fact]
    public async Task GetHealthScoreAsync_ExcellentSavingsRate_Returns20Points()
    {
        // Arrange - 25% savings rate (5000 income, 3750 expenses)
        var threeMonthsAgo = DateTime.Today.AddMonths(-3);
        
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 5000m,
            Date = threeMonthsAgo.AddDays(5),
            Description = "Salary",
            IsIncome = true
        });
        
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 3750m,
            Date = threeMonthsAgo.AddDays(10),
            Description = "Expenses",
            IsIncome = false
        });

        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetHealthScoreAsync();

        // Assert
        Assert.Equal(20, result.SavingsRate.Score);
        Assert.Equal(20, result.SavingsRate.MaxScore);
        Assert.Equal("Utmärkt", result.SavingsRate.Status);
        Assert.True(result.SavingsRate.Value >= 20);
    }

    [Fact]
    public async Task GetHealthScoreAsync_NoDebt_Returns20Points()
    {
        // Arrange - No loans
        // Act
        var result = await _reportService.GetHealthScoreAsync();

        // Assert
        Assert.Equal(20, result.DebtLevel.Score);
        Assert.Equal(20, result.DebtLevel.MaxScore);
        Assert.Equal("Utmärkt", result.DebtLevel.Status);
    }

    [Fact]
    public async Task GetHealthScoreAsync_HighDebt_ReturnsLowScore()
    {
        // Arrange - High debt (500% of annual income)
        await _context.Loans.AddAsync(new Loan
        {
            Name = "High debt loan",
            Amount = 500000m,
            Type = "Personal",
            InterestRate = 5.0m,
            Amortization = 1000m
        });

        var oneYearAgo = DateTime.Today.AddYears(-1);
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 100000m,
            Date = oneYearAgo.AddDays(5),
            Description = "Annual income",
            IsIncome = true
        });

        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetHealthScoreAsync();

        // Assert
        Assert.True(result.DebtLevel.Score < 10);
        Assert.Equal(20, result.DebtLevel.MaxScore);
    }

    [Fact]
    public async Task GetHealthScoreAsync_SixMonthsEmergencyFund_Returns20Points()
    {
        // Arrange - 6 months of expenses saved
        var threeMonthsAgo = DateTime.Today.AddMonths(-3);
        
        // Monthly expenses of 10,000
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 10000m,
            Date = threeMonthsAgo.AddDays(5),
            Description = "Monthly expenses",
            IsIncome = false
        });

        // Bank balance of 60,000 (6 months) - using InitialBalance
        await _context.BankSources.AddAsync(new BankSource
        {
            Name = "Checking account",
            InitialBalance = 60000m,
            AccountType = "checking"
        });

        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetHealthScoreAsync();

        // Assert
        Assert.Equal(20, result.EmergencyFund.Score);
        Assert.Equal(20, result.EmergencyFund.MaxScore);
        Assert.Equal("Utmärkt", result.EmergencyFund.Status);
        Assert.True(result.EmergencyFund.Value >= 6);
    }

    [Fact]
    public async Task GetHealthScoreAsync_StableIncome_Returns10Points()
    {
        // Arrange - Stable monthly income (very low variation)
        var sixMonthsAgo = DateTime.Today.AddMonths(-6);
        
        for (int i = 0; i < 6; i++)
        {
            await _context.Transactions.AddAsync(new Transaction
            {
                Amount = 50000m, // Exactly same amount each month
                Date = sixMonthsAgo.AddMonths(i).AddDays(5),
                Description = $"Salary month {i}",
                IsIncome = true
            });
        }

        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetHealthScoreAsync();

        // Assert
        Assert.Equal(10, result.IncomeStability.Score);
        Assert.Equal(10, result.IncomeStability.MaxScore);
        Assert.Equal("Utmärkt", result.IncomeStability.Status);
    }

    [Fact]
    public async Task GetHealthScoreAsync_DiversifiedInvestments_ReturnsHighScore()
    {
        // Arrange - 4 equal investments (good diversification)
        await _context.Investments.AddAsync(new Investment
        {
            Name = "Stock A",
            Quantity = 100,
            CurrentPrice = 250m,
            PurchasePrice = 200m,
            Type = "Stock",
            PurchaseDate = DateTime.Today.AddYears(-1)
        });
        
        await _context.Investments.AddAsync(new Investment
        {
            Name = "Stock B",
            Quantity = 100,
            CurrentPrice = 250m,
            PurchasePrice = 200m,
            Type = "Stock",
            PurchaseDate = DateTime.Today.AddYears(-1)
        });
        
        await _context.Investments.AddAsync(new Investment
        {
            Name = "Bond",
            Quantity = 100,
            CurrentPrice = 250m,
            PurchasePrice = 200m,
            Type = "Bond",
            PurchaseDate = DateTime.Today.AddYears(-1)
        });
        
        await _context.Investments.AddAsync(new Investment
        {
            Name = "Fund",
            Quantity = 100,
            CurrentPrice = 250m,
            PurchasePrice = 200m,
            Type = "Fund",
            PurchaseDate = DateTime.Today.AddYears(-1)
        });

        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetHealthScoreAsync();

        // Assert
        Assert.True(result.InvestmentDiversification.Score >= 12);
        Assert.Equal(15, result.InvestmentDiversification.MaxScore);
    }

    [Fact]
    public async Task GetHealthScoreAsync_CalculatesCorrectHealthLevel()
    {
        // Arrange - Create data for high score
        var threeMonthsAgo = DateTime.Today.AddMonths(-3);
        
        // Good savings rate
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 50000m,
            Date = threeMonthsAgo.AddDays(5),
            Description = "Income",
            IsIncome = true
        });
        
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 35000m,
            Date = threeMonthsAgo.AddDays(10),
            Description = "Expenses",
            IsIncome = false
        });

        // Emergency fund
        await _context.BankSources.AddAsync(new BankSource
        {
            Name = "Savings",
            InitialBalance = 200000m,
            AccountType = "savings"
        });

        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetHealthScoreAsync();

        // Assert
        Assert.NotNull(result.HealthLevel);
        Assert.True(result.TotalScore > 0);
        Assert.Contains(result.HealthLevel, new[] { "Utmärkt", "Bra", "Godkänt", "Behöver förbättras" });
    }

    [Fact]
    public async Task GetHealthScoreAsync_IdentifiesStrengths()
    {
        // Arrange - Create excellent savings rate
        var threeMonthsAgo = DateTime.Today.AddMonths(-3);
        
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 50000m,
            Date = threeMonthsAgo.AddDays(5),
            Description = "Income",
            IsIncome = true
        });
        
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 30000m,
            Date = threeMonthsAgo.AddDays(10),
            Description = "Expenses",
            IsIncome = false
        });

        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetHealthScoreAsync();

        // Assert
        Assert.NotEmpty(result.Strengths);
    }

    [Fact]
    public async Task GetHealthScoreAsync_IdentifiesImprovementAreas()
    {
        // Arrange - Create poor savings rate scenario
        var threeMonthsAgo = DateTime.Today.AddMonths(-3);
        
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 10000m,
            Date = threeMonthsAgo.AddDays(5),
            Description = "Low Income",
            IsIncome = true
        });
        
        await _context.Transactions.AddAsync(new Transaction
        {
            Amount = 9900m,
            Date = threeMonthsAgo.AddDays(10),
            Description = "High Expenses",
            IsIncome = false
        });

        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetHealthScoreAsync();

        // Assert
        Assert.NotEmpty(result.ImprovementAreas);
    }
}
