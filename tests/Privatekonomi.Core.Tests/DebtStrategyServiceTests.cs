using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class DebtStrategyServiceTests
{
    private PrivatekonomyContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new PrivatekonomyContext(options);
    }

    [Fact]
    public void GenerateAmortizationSchedule_WithValidLoan_ReturnsSchedule()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new DebtStrategyService(context);
        var loan = new Loan
        {
            LoanId = 1,
            Name = "Test Loan",
            Amount = 100000,
            InterestRate = 5.0m,
            Amortization = 2000,
            StartDate = DateTime.Today
        };

        // Act
        var schedule = service.GenerateAmortizationSchedule(loan, 0);

        // Assert
        Assert.NotEmpty(schedule);
        Assert.True(schedule.Count > 0);
        Assert.Equal(1, schedule.First().PaymentNumber);
        Assert.True(schedule.Last().EndingBalance <= 0.01m); // Should be paid off
    }

    [Fact]
    public void GenerateAmortizationSchedule_WithExtraPayment_ReducesMonths()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new DebtStrategyService(context);
        var loan = new Loan
        {
            LoanId = 1,
            Name = "Test Loan",
            Amount = 100000,
            InterestRate = 5.0m,
            Amortization = 2000,
            StartDate = DateTime.Today
        };

        // Act
        var scheduleWithoutExtra = service.GenerateAmortizationSchedule(loan, 0);
        var scheduleWithExtra = service.GenerateAmortizationSchedule(loan, 500);

        // Assert
        Assert.True(scheduleWithExtra.Count < scheduleWithoutExtra.Count);
        Assert.True(scheduleWithExtra.Last().TotalInterestPaid < scheduleWithoutExtra.Last().TotalInterestPaid);
    }

    [Fact]
    public async Task CalculateSnowballStrategy_OrdersBySmallestBalance()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new DebtStrategyService(context);
        
        var loans = new[]
        {
            new Loan { LoanId = 1, Name = "Large Loan", Amount = 50000, InterestRate = 3.0m, Amortization = 1000 },
            new Loan { LoanId = 2, Name = "Small Loan", Amount = 5000, InterestRate = 8.0m, Amortization = 200 },
            new Loan { LoanId = 3, Name = "Medium Loan", Amount = 20000, InterestRate = 5.0m, Amortization = 500 }
        };
        
        context.Loans.AddRange(loans);
        await context.SaveChangesAsync();

        // Act
        var strategy = await service.CalculateSnowballStrategy(2000);

        // Assert
        Assert.Equal("Snöboll", strategy.StrategyName);
        Assert.NotEmpty(strategy.PayoffPlans);
        
        // Small loan should be paid off first
        var firstPayoff = strategy.PayoffPlans.OrderBy(p => p.PayoffOrder).First();
        Assert.Equal("Small Loan", firstPayoff.LoanName);
    }

    [Fact]
    public async Task CalculateAvalancheStrategy_OrdersByHighestInterest()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new DebtStrategyService(context);
        
        var loans = new[]
        {
            new Loan { LoanId = 1, Name = "Low Interest", Amount = 50000, InterestRate = 3.0m, Amortization = 1000 },
            new Loan { LoanId = 2, Name = "High Interest", Amount = 5000, InterestRate = 8.0m, Amortization = 200 },
            new Loan { LoanId = 3, Name = "Medium Interest", Amount = 20000, InterestRate = 5.0m, Amortization = 500 }
        };
        
        context.Loans.AddRange(loans);
        await context.SaveChangesAsync();

        // Act
        var strategy = await service.CalculateAvalancheStrategy(2000);

        // Assert
        Assert.Equal("Lavin", strategy.StrategyName);
        Assert.NotEmpty(strategy.PayoffPlans);
        
        // High interest loan should be paid off first
        var firstPayoff = strategy.PayoffPlans.OrderBy(p => p.PayoffOrder).First();
        Assert.Equal("High Interest", firstPayoff.LoanName);
    }

    [Fact]
    public void AnalyzeExtraPayment_ShowsInterestSavings()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new DebtStrategyService(context);
        var loan = new Loan
        {
            LoanId = 1,
            Name = "Test Loan",
            Amount = 100000,
            InterestRate = 5.0m,
            Amortization = 2000,
            StartDate = DateTime.Today
        };

        // Act
        var analysis = service.AnalyzeExtraPayment(loan, 500);

        // Assert
        Assert.Equal(500, analysis.ExtraMonthlyPayment);
        Assert.True(analysis.MonthsSaved > 0);
        Assert.True(analysis.InterestSavings > 0);
        Assert.True(analysis.NewPayoffDate < analysis.OriginalPayoffDate);
    }

    [Fact]
    public async Task CompareStrategies_ReturnsSnowballAndAvalanche()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new DebtStrategyService(context);
        
        var loans = new[]
        {
            new Loan { LoanId = 1, Name = "Loan 1", Amount = 50000, InterestRate = 3.0m, Amortization = 1000 },
            new Loan { LoanId = 2, Name = "Loan 2", Amount = 5000, InterestRate = 8.0m, Amortization = 200 }
        };
        
        context.Loans.AddRange(loans);
        await context.SaveChangesAsync();

        // Act
        var (snowball, avalanche) = await service.CompareStrategies(2000);

        // Assert
        Assert.Equal("Snöboll", snowball.StrategyName);
        Assert.Equal("Lavin", avalanche.StrategyName);
        Assert.NotEmpty(snowball.PayoffPlans);
        Assert.NotEmpty(avalanche.PayoffPlans);
    }

    [Fact]
    public void ExportAmortizationScheduleToCsv_ReturnsValidCsv()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new DebtStrategyService(context);
        var loan = new Loan
        {
            LoanId = 1,
            Name = "Test Loan",
            Amount = 50000,
            InterestRate = 5.0m,
            Amortization = 1000,
            StartDate = DateTime.Today
        };

        // Act
        var csv = service.ExportAmortizationScheduleToCsv(loan, 0);

        // Assert
        Assert.NotNull(csv);
        Assert.True(csv.Length > 0);
        
        var content = System.Text.Encoding.UTF8.GetString(csv);
        Assert.Contains("Amorteringsplan för: Test Loan", content);
        Assert.Contains("Lånebelopp: 50", content);
        Assert.Contains("Betalning,Datum,Ingående Saldo", content);
    }

    [Fact]
    public async Task ExportStrategyToCsv_ReturnsValidCsv()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new DebtStrategyService(context);
        
        var loans = new List<Loan>
        {
            new Loan { LoanId = 1, Name = "Loan 1", Amount = 50000, InterestRate = 3.0m, Amortization = 1000 },
            new Loan { LoanId = 2, Name = "Loan 2", Amount = 5000, InterestRate = 8.0m, Amortization = 200 }
        };
        
        context.Loans.AddRange(loans);
        await context.SaveChangesAsync();

        var strategy = await service.CalculateSnowballStrategy(2000);

        // Act
        var csv = service.ExportStrategyToCsv(strategy, loans);

        // Assert
        Assert.NotNull(csv);
        Assert.True(csv.Length > 0);
        
        var content = System.Text.Encoding.UTF8.GetString(csv);
        Assert.Contains("Avbetalningsstrategi: Snöboll", content);
        Assert.Contains("Ordning,Lån,Belopp,Ränta", content);
    }

    [Fact]
    public async Task GenerateDetailedStrategy_Snowball_ReturnsMonthlySchedule()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new DebtStrategyService(context);
        
        var loans = new[]
        {
            new Loan { LoanId = 1, Name = "Large Loan", Amount = 50000, InterestRate = 3.0m, Amortization = 1000 },
            new Loan { LoanId = 2, Name = "Small Loan", Amount = 5000, InterestRate = 8.0m, Amortization = 200 }
        };
        
        context.Loans.AddRange(loans);
        await context.SaveChangesAsync();

        // Act
        var detailedStrategy = await service.GenerateDetailedStrategy("Snowball", 2000);

        // Assert
        Assert.Equal("Snowball", detailedStrategy.StrategyName);
        Assert.NotEmpty(detailedStrategy.MonthlySchedule);
        Assert.NotEmpty(detailedStrategy.LoanSummaries);
        Assert.Equal(2, detailedStrategy.LoanSummaries.Count);
        
        // First month should have payments for both loans
        var firstMonth = detailedStrategy.MonthlySchedule.First();
        Assert.Equal(1, firstMonth.MonthNumber);
        Assert.Equal(2, firstMonth.LoanPayments.Count);
        
        // Small loan should be focus loan (payoff order 1)
        var smallLoanSummary = detailedStrategy.LoanSummaries.First(s => s.LoanName == "Small Loan");
        Assert.Equal(1, smallLoanSummary.PayoffOrder);
    }

    [Fact]
    public async Task GenerateDetailedStrategy_Avalanche_ReturnsMonthlySchedule()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new DebtStrategyService(context);
        
        var loans = new[]
        {
            new Loan { LoanId = 1, Name = "Low Interest", Amount = 50000, InterestRate = 3.0m, Amortization = 1000 },
            new Loan { LoanId = 2, Name = "High Interest", Amount = 5000, InterestRate = 8.0m, Amortization = 200 }
        };
        
        context.Loans.AddRange(loans);
        await context.SaveChangesAsync();

        // Act
        var detailedStrategy = await service.GenerateDetailedStrategy("Avalanche", 2000);

        // Assert
        Assert.Equal("Avalanche", detailedStrategy.StrategyName);
        Assert.NotEmpty(detailedStrategy.MonthlySchedule);
        
        // High interest loan should be focus loan (payoff order 1)
        var highInterestSummary = detailedStrategy.LoanSummaries.First(s => s.LoanName == "High Interest");
        Assert.Equal(1, highInterestSummary.PayoffOrder);
    }

    [Fact]
    public async Task GenerateDetailedStrategy_TracksInterestCorrectly()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new DebtStrategyService(context);
        
        var loan = new Loan 
        { 
            LoanId = 1, 
            Name = "Test Loan", 
            Amount = 10000, 
            InterestRate = 6.0m, 
            Amortization = 500 
        };
        
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        // Act
        var detailedStrategy = await service.GenerateDetailedStrategy("Snowball", 1000);

        // Assert
        Assert.True(detailedStrategy.TotalInterestPaid > 0);
        
        var loanSummary = detailedStrategy.LoanSummaries.First();
        Assert.Equal(loan.Amount + loanSummary.TotalInterestPaid, loanSummary.TotalAmountPaid);
    }

    [Fact]
    public async Task CalculateDebtFreeDate_WithNoLoans_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new DebtStrategyService(context);

        // Act
        var debtFreeDate = await service.CalculateDebtFreeDate();

        // Assert
        Assert.Null(debtFreeDate);
    }

    [Fact]
    public async Task CalculateDebtFreeDate_WithLoans_ReturnsLatestPayoffDate()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new DebtStrategyService(context);
        
        var loans = new[]
        {
            new Loan { LoanId = 1, Name = "Quick Loan", Amount = 5000, InterestRate = 5.0m, Amortization = 1000 },
            new Loan { LoanId = 2, Name = "Long Loan", Amount = 50000, InterestRate = 3.0m, Amortization = 500 }
        };
        
        context.Loans.AddRange(loans);
        await context.SaveChangesAsync();

        // Act
        var debtFreeDate = await service.CalculateDebtFreeDate();

        // Assert
        Assert.NotNull(debtFreeDate);
        Assert.True(debtFreeDate.Value > DateTime.Today);
    }
}
