using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Services;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Data;

namespace Privatekonomi.Core.Tests;

public class KalpServiceTests : IDisposable
{
    private readonly PrivatekonomyContext _context;
    private readonly Mock<IKonsumentverketComparisonService> _mockKonsumentverketService;
    private readonly IKalpService _service;

    public KalpServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new PrivatekonomyContext(options);

        _mockKonsumentverketService = new Mock<IKonsumentverketComparisonService>();
        _service = new KalpService(_context, _mockKonsumentverketService.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void CalculateKalp_BasicIncome_ReturnsCorrectCalculation()
    {
        // Arrange
        var input = new KalpInput
        {
            MonthlyIncome = 30000m,
            FixedExpenses = new Dictionary<string, decimal>
            {
                { "Hyra", 10000m },
                { "El", 500m }
            },
            Loans = new List<LoanPayment>
            {
                new LoanPayment { LoanName = "Bolån", LoanType = "Bolån", MonthlyPayment = 5000m }
            }
        };

        // Act
        var result = _service.CalculateKalp(input);

        // Assert
        Assert.Equal(30000m, result.TotalIncome);
        Assert.Equal(10500m, result.FixedExpenses);
        Assert.Equal(5000m, result.LoanPayments);
        Assert.Equal(14500m, result.KalpAmount); // 30000 - 10500 - 5000
        Assert.Equal(48.33m, Math.Round(result.KalpPercentage, 2)); // (14500/30000)*100
    }

    [Fact]
    public void CalculateKalp_NoFixedExpensesOrLoans_ReturnsFullIncome()
    {
        // Arrange
        var input = new KalpInput
        {
            MonthlyIncome = 25000m,
            FixedExpenses = new Dictionary<string, decimal>(),
            Loans = new List<LoanPayment>()
        };

        // Act
        var result = _service.CalculateKalp(input);

        // Assert
        Assert.Equal(25000m, result.TotalIncome);
        Assert.Equal(0m, result.FixedExpenses);
        Assert.Equal(0m, result.LoanPayments);
        Assert.Equal(25000m, result.KalpAmount);
        Assert.Equal(100m, result.KalpPercentage);
    }

    [Fact]
    public void CalculateKalp_ExpensesExceedIncome_ReturnsNegativeKalp()
    {
        // Arrange
        var input = new KalpInput
        {
            MonthlyIncome = 20000m,
            FixedExpenses = new Dictionary<string, decimal>
            {
                { "Hyra", 12000m },
                { "El", 800m }
            },
            Loans = new List<LoanPayment>
            {
                new LoanPayment { LoanName = "Lån", LoanType = "Privatlån", MonthlyPayment = 8000m }
            }
        };

        // Act
        var result = _service.CalculateKalp(input);

        // Assert
        Assert.Equal(20000m, result.TotalIncome);
        Assert.Equal(12800m, result.FixedExpenses);
        Assert.Equal(8000m, result.LoanPayments);
        Assert.Equal(-800m, result.KalpAmount); // 20000 - 12800 - 8000 = -800
    }

    [Fact]
    public void CalculateKalp_MultipleLoansOfSameType_AggregatesCorrectly()
    {
        // Arrange
        var input = new KalpInput
        {
            MonthlyIncome = 40000m,
            FixedExpenses = new Dictionary<string, decimal>
            {
                { "Hyra", 10000m }
            },
            Loans = new List<LoanPayment>
            {
                new LoanPayment { LoanName = "Bolån 1", LoanType = "Bolån", MonthlyPayment = 3000m },
                new LoanPayment { LoanName = "Bolån 2", LoanType = "Bolån", MonthlyPayment = 2000m },
                new LoanPayment { LoanName = "CSN", LoanType = "CSN-lån", MonthlyPayment = 1500m }
            }
        };

        // Act
        var result = _service.CalculateKalp(input);

        // Assert
        Assert.Equal(6500m, result.LoanPayments); // 3000 + 2000 + 1500
        Assert.Equal(23500m, result.KalpAmount); // 40000 - 10000 - 6500
        Assert.True(result.LoanPaymentBreakdown.ContainsKey("Bolån"));
        Assert.Equal(5000m, result.LoanPaymentBreakdown["Bolån"]); // 3000 + 2000
        Assert.Equal(1500m, result.LoanPaymentBreakdown["CSN-lån"]);
    }

    [Fact]
    public void CalculateKalp_WithHouseholdMembers_CalculatesRecommendedMinimum()
    {
        // Arrange
        var members = new List<KonsumentverketHouseholdMember>
        {
            new KonsumentverketHouseholdMember { Age = 35, HasLunchOutOnWeekdays = false }
        };

        var input = new KalpInput
        {
            MonthlyIncome = 30000m,
            FixedExpenses = new Dictionary<string, decimal>
            {
                { "Hyra", 8000m }
            },
            Loans = new List<LoanPayment>(),
            HouseholdMembers = members
        };

        // Mock Konsumentverket service
        _mockKonsumentverketService
            .Setup(s => s.CalculateReferenceCosts(members))
            .Returns(new KonsumentverketComparisonResult
            {
                ReferenceFoodCosts = 3730m,
                ReferenceIndividualCosts = 2090m,
                ReferenceCommonCosts = 3400m
            });

        // Act
        var result = _service.CalculateKalp(input);

        // Assert
        Assert.NotNull(result.RecommendedMinimumKalp);
        Assert.Equal(5820m, result.RecommendedMinimumKalp); // Food (3730) + Individual (2090)
        Assert.True(result.MeetsRecommendedMinimum); // 22000 >= 5820
    }

    [Fact]
    public void CalculateKalp_BelowRecommendedMinimum_ReturnsFalse()
    {
        // Arrange
        var members = new List<KonsumentverketHouseholdMember>
        {
            new KonsumentverketHouseholdMember { Age = 35, HasLunchOutOnWeekdays = false }
        };

        var input = new KalpInput
        {
            MonthlyIncome = 20000m,
            FixedExpenses = new Dictionary<string, decimal>
            {
                { "Hyra", 12000m }
            },
            Loans = new List<LoanPayment>
            {
                new LoanPayment { LoanName = "Lån", LoanType = "Privatlån", MonthlyPayment = 3000m }
            },
            HouseholdMembers = members
        };

        // Mock Konsumentverket service
        _mockKonsumentverketService
            .Setup(s => s.CalculateReferenceCosts(members))
            .Returns(new KonsumentverketComparisonResult
            {
                ReferenceFoodCosts = 3730m,
                ReferenceIndividualCosts = 2090m
            });

        // Act
        var result = _service.CalculateKalp(input);

        // Assert
        Assert.Equal(5000m, result.KalpAmount); // 20000 - 12000 - 3000
        Assert.Equal(5820m, result.RecommendedMinimumKalp);
        Assert.False(result.MeetsRecommendedMinimum); // 5000 < 5820
    }

    [Fact]
    public void CalculateKalp_FixedExpenseBreakdown_ContainsAllCategories()
    {
        // Arrange
        var input = new KalpInput
        {
            MonthlyIncome = 30000m,
            FixedExpenses = new Dictionary<string, decimal>
            {
                { "Hyra", 10000m },
                { "El", 500m },
                { "Internet", 300m },
                { "Försäkring", 200m }
            },
            Loans = new List<LoanPayment>()
        };

        // Act
        var result = _service.CalculateKalp(input);

        // Assert
        Assert.Equal(4, result.FixedExpenseBreakdown.Count);
        Assert.Equal(10000m, result.FixedExpenseBreakdown["Hyra"]);
        Assert.Equal(500m, result.FixedExpenseBreakdown["El"]);
        Assert.Equal(300m, result.FixedExpenseBreakdown["Internet"]);
        Assert.Equal(200m, result.FixedExpenseBreakdown["Försäkring"]);
    }

    [Fact]
    public void CalculateRecommendedMinimumKalp_EmptyHousehold_ReturnsZero()
    {
        // Arrange
        var members = new List<KonsumentverketHouseholdMember>();

        // Act
        var result = _service.CalculateRecommendedMinimumKalp(members);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateRecommendedMinimumKalp_SingleAdult_ReturnsCorrectAmount()
    {
        // Arrange
        var members = new List<KonsumentverketHouseholdMember>
        {
            new KonsumentverketHouseholdMember { Age = 35, HasLunchOutOnWeekdays = false }
        };

        // Mock Konsumentverket service
        _mockKonsumentverketService
            .Setup(s => s.CalculateReferenceCosts(members))
            .Returns(new KonsumentverketComparisonResult
            {
                ReferenceFoodCosts = 3730m,
                ReferenceIndividualCosts = 2090m,
                ReferenceCommonCosts = 3400m
            });

        // Act
        var result = _service.CalculateRecommendedMinimumKalp(members);

        // Assert
        // Should be food + individual costs only (common costs are fixed expenses)
        Assert.Equal(5820m, result); // 3730 + 2090
    }

    [Fact]
    public void CalculateRecommendedMinimumKalp_FamilyOfFour_ReturnsCorrectAmount()
    {
        // Arrange
        var members = new List<KonsumentverketHouseholdMember>
        {
            new KonsumentverketHouseholdMember { Age = 35, HasLunchOutOnWeekdays = true },
            new KonsumentverketHouseholdMember { Age = 38, HasLunchOutOnWeekdays = true },
            new KonsumentverketHouseholdMember { Age = 8, HasLunchOutOnWeekdays = true },
            new KonsumentverketHouseholdMember { Age = 5, HasLunchOutOnWeekdays = true }
        };

        // Mock Konsumentverket service
        _mockKonsumentverketService
            .Setup(s => s.CalculateReferenceCosts(members))
            .Returns(new KonsumentverketComparisonResult
            {
                ReferenceFoodCosts = 8790m,
                ReferenceIndividualCosts = 8160m,
                ReferenceCommonCosts = 6130m
            });

        // Act
        var result = _service.CalculateRecommendedMinimumKalp(members);

        // Assert
        Assert.Equal(16950m, result); // 8790 + 8160
    }

    [Fact]
    public void CalculateKalpWithComparison_WithoutHouseholdMembers_NoKonsumentverketComparison()
    {
        // Arrange
        var input = new KalpInput
        {
            MonthlyIncome = 30000m,
            FixedExpenses = new Dictionary<string, decimal> { { "Hyra", 10000m } },
            Loans = new List<LoanPayment>(),
            HouseholdMembers = null
        };

        // Act
        var result = _service.CalculateKalpWithComparison(input);

        // Assert
        Assert.NotNull(result.KalpCalculation);
        Assert.Equal(30000m, result.KalpCalculation.TotalIncome);
        Assert.Null(result.KonsumentverketComparison);
    }

    [Fact]
    public void CalculateKalpWithComparison_WithHouseholdMembers_IncludesKonsumentverketComparison()
    {
        // Arrange
        var members = new List<KonsumentverketHouseholdMember>
        {
            new KonsumentverketHouseholdMember { Age = 35, HasLunchOutOnWeekdays = false }
        };

        var input = new KalpInput
        {
            MonthlyIncome = 30000m,
            FixedExpenses = new Dictionary<string, decimal> { { "Mat", 4000m } },
            Loans = new List<LoanPayment>(),
            HouseholdMembers = members
        };

        var mockComparisonResult = new KonsumentverketComparisonResult
        {
            ReferenceFoodCosts = 3730m,
            ReferenceIndividualCosts = 2090m
        };

        // Mock Konsumentverket service for both methods
        _mockKonsumentverketService
            .Setup(s => s.CalculateReferenceCosts(It.IsAny<List<KonsumentverketHouseholdMember>>()))
            .Returns(mockComparisonResult);
        
        _mockKonsumentverketService
            .Setup(s => s.CompareWithReference(It.IsAny<List<KonsumentverketHouseholdMember>>(), It.IsAny<UserHouseholdCosts>()))
            .Returns(mockComparisonResult);

        // Act
        var result = _service.CalculateKalpWithComparison(input);

        // Assert
        Assert.NotNull(result.KonsumentverketComparison);
        Assert.Equal(3730m, result.KonsumentverketComparison.ReferenceFoodCosts);
    }
}
