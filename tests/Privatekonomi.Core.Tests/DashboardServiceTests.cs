using Microsoft.EntityFrameworkCore;
using Moq;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class DashboardServiceTests
{
    private readonly PrivatekonomyContext _context;
    private readonly DashboardService _service;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IBudgetService> _mockBudgetService;
    private readonly Mock<IBillService> _mockBillService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private const string TestUserId = "test-user-123";

    public DashboardServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new PrivatekonomyContext(options);

        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockCurrentUserService.Setup(x => x.UserId).Returns(TestUserId);
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);

        _mockBudgetService = new Mock<IBudgetService>();
        _mockBillService = new Mock<IBillService>();
        _mockNotificationService = new Mock<INotificationService>();

        // Setup default mock returns
        _mockBudgetService.Setup(x => x.GetActiveBudgetsAsync())
            .ReturnsAsync(new List<Budget>());
        _mockBillService.Setup(x => x.GetBillsDueSoonAsync(TestUserId, It.IsAny<int>()))
            .ReturnsAsync(new List<Bill>());
        _mockBillService.Setup(x => x.GetOverdueBillsAsync(TestUserId))
            .ReturnsAsync(new List<Bill>());
        _mockNotificationService.Setup(x => x.GetActiveNotificationsAsync(TestUserId, It.IsAny<bool>()))
            .ReturnsAsync(new List<Notification>());

        _service = new DashboardService(
            _context,
            _mockBudgetService.Object,
            _mockBillService.Object,
            _mockNotificationService.Object,
            _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task GetDashboardDataAsync_ReturnsAggregatedData()
    {
        // Act
        var result = await _service.GetDashboardDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Balance);
        Assert.NotNull(result.BudgetStatus);
        Assert.NotNull(result.UpcomingBills);
        Assert.NotNull(result.Insights);
        Assert.True(result.LastUpdated <= DateTime.UtcNow);
    }

    [Fact]
    public async Task GetBalanceSummaryAsync_ReturnsEmptyWhenNoAccounts()
    {
        // Act
        var result = await _service.GetBalanceSummaryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalBalance);
        Assert.Empty(result.Accounts);
        Assert.Equal("SEK", result.Currency);
    }

    [Fact]
    public async Task GetBalanceSummaryAsync_CalculatesTotalBalanceFromAccounts()
    {
        // Arrange
        var account1 = new BankSource
        {
            Name = "Checking Account",
            UserId = TestUserId,
            AccountType = "checking",
            InitialBalance = 10000,
            Currency = "SEK",
            Color = "#FF0000"
        };
        var account2 = new BankSource
        {
            Name = "Savings Account",
            UserId = TestUserId,
            AccountType = "savings",
            InitialBalance = 50000,
            Currency = "SEK",
            Color = "#00FF00"
        };

        _context.BankSources.AddRange(account1, account2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetBalanceSummaryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(60000, result.TotalBalance);
        Assert.Equal(2, result.Accounts.Count);
    }

    [Fact]
    public async Task GetBalanceSummaryAsync_IncludesTransactionsInBalance()
    {
        // Arrange
        var account = new BankSource
        {
            Name = "Test Account",
            UserId = TestUserId,
            AccountType = "checking",
            InitialBalance = 10000,
            Currency = "SEK"
        };

        _context.BankSources.Add(account);
        await _context.SaveChangesAsync();

        // Use fixed date for deterministic tests
        var transactionDate = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // Add income transaction
        var incomeTransaction = new Transaction
        {
            BankSourceId = account.BankSourceId,
            Description = "Salary",
            Amount = 25000,
            IsIncome = true,
            Date = transactionDate
        };

        // Add expense transaction
        var expenseTransaction = new Transaction
        {
            BankSourceId = account.BankSourceId,
            Description = "Rent",
            Amount = 8000,
            IsIncome = false,
            Date = transactionDate
        };

        _context.Transactions.AddRange(incomeTransaction, expenseTransaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetBalanceSummaryAsync();

        // Assert
        Assert.NotNull(result);
        // 10000 (initial) + 25000 (income) - 8000 (expense) = 27000
        Assert.Equal(27000, result.TotalBalance);
    }

    [Fact]
    public async Task GetBalanceSummaryAsync_ExcludesClosedAccounts()
    {
        // Arrange
        var activeAccount = new BankSource
        {
            Name = "Active Account",
            UserId = TestUserId,
            AccountType = "checking",
            InitialBalance = 10000,
            Currency = "SEK",
            ClosedDate = null
        };
        var closedAccount = new BankSource
        {
            Name = "Closed Account",
            UserId = TestUserId,
            AccountType = "checking",
            InitialBalance = 50000,
            Currency = "SEK",
            ClosedDate = DateTime.Now.AddDays(-30)
        };

        _context.BankSources.AddRange(activeAccount, closedAccount);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetBalanceSummaryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10000, result.TotalBalance);
        Assert.Single(result.Accounts);
    }

    [Fact]
    public async Task GetBalanceSummaryAsync_FiltersAccountsByIds()
    {
        // Arrange
        var account1 = new BankSource
        {
            Name = "Account 1",
            UserId = TestUserId,
            AccountType = "checking",
            InitialBalance = 10000,
            Currency = "SEK"
        };
        var account2 = new BankSource
        {
            Name = "Account 2",
            UserId = TestUserId,
            AccountType = "savings",
            InitialBalance = 20000,
            Currency = "SEK"
        };

        _context.BankSources.AddRange(account1, account2);
        await _context.SaveChangesAsync();

        // Act - only get account 1
        var result = await _service.GetBalanceSummaryAsync(new[] { account1.BankSourceId });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10000, result.TotalBalance);
        Assert.Single(result.Accounts);
        Assert.Equal("Account 1", result.Accounts.First().Name);
    }

    [Fact]
    public async Task GetBudgetStatusAsync_ReturnsNoActiveBudgetsWhenEmpty()
    {
        // Arrange
        _mockBudgetService.Setup(x => x.GetActiveBudgetsAsync())
            .ReturnsAsync(new List<Budget>());

        // Act
        var result = await _service.GetBudgetStatusAsync();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.HasActiveBudgets);
        Assert.Empty(result.Budgets);
    }

    [Fact]
    public async Task GetBudgetStatusAsync_CalculatesTotalPlannedAndSpent()
    {
        // Arrange
        var category1 = new Category { CategoryId = 1, Name = "Food" };
        var category2 = new Category { CategoryId = 2, Name = "Transport" };

        var budget = new Budget
        {
            BudgetId = 1,
            Name = "Monthly Budget",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 1, 31),
            BudgetCategories = new List<BudgetCategory>
            {
                new BudgetCategory { CategoryId = 1, Category = category1, PlannedAmount = 5000 },
                new BudgetCategory { CategoryId = 2, Category = category2, PlannedAmount = 2000 }
            }
        };

        _mockBudgetService.Setup(x => x.GetActiveBudgetsAsync())
            .ReturnsAsync(new List<Budget> { budget });
        _mockBudgetService.Setup(x => x.GetActualAmountsByCategoryAsync(budget.BudgetId))
            .ReturnsAsync(new Dictionary<int, decimal>
            {
                { 1, 4000 },
                { 2, 1500 }
            });

        // Act
        var result = await _service.GetBudgetStatusAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.HasActiveBudgets);
        Assert.Equal(7000, result.TotalPlanned);
        Assert.Equal(5500, result.TotalSpent);
        Assert.Equal(1500, result.Remaining);
        Assert.Single(result.Budgets);
    }

    [Fact]
    public async Task GetBudgetStatusAsync_IdentifiesOverspentCategories()
    {
        // Arrange
        var category1 = new Category { CategoryId = 1, Name = "Food" };
        var category2 = new Category { CategoryId = 2, Name = "Transport" };

        var budget = new Budget
        {
            BudgetId = 1,
            Name = "Monthly Budget",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 1, 31),
            BudgetCategories = new List<BudgetCategory>
            {
                new BudgetCategory { CategoryId = 1, Category = category1, PlannedAmount = 5000 },
                new BudgetCategory { CategoryId = 2, Category = category2, PlannedAmount = 2000 }
            }
        };

        _mockBudgetService.Setup(x => x.GetActiveBudgetsAsync())
            .ReturnsAsync(new List<Budget> { budget });
        _mockBudgetService.Setup(x => x.GetActualAmountsByCategoryAsync(budget.BudgetId))
            .ReturnsAsync(new Dictionary<int, decimal>
            {
                { 1, 6000 }, // Overspent
                { 2, 1500 }  // Within budget
            });

        // Act
        var result = await _service.GetBudgetStatusAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.OverspentCategories);
        Assert.Contains("Food", result.OverspentCategories);
    }

    [Fact]
    public async Task GetBudgetStatusAsync_DeterminesCorrectStatus()
    {
        // Arrange - Budget at 80% usage (Warning status)
        var category = new Category { CategoryId = 1, Name = "Food" };
        var budget = new Budget
        {
            BudgetId = 1,
            Name = "Monthly Budget",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 1, 31),
            BudgetCategories = new List<BudgetCategory>
            {
                new BudgetCategory { CategoryId = 1, Category = category, PlannedAmount = 10000 }
            }
        };

        _mockBudgetService.Setup(x => x.GetActiveBudgetsAsync())
            .ReturnsAsync(new List<Budget> { budget });
        _mockBudgetService.Setup(x => x.GetActualAmountsByCategoryAsync(budget.BudgetId))
            .ReturnsAsync(new Dictionary<int, decimal>
            {
                { 1, 8000 } // 80% used
            });

        // Act
        var result = await _service.GetBudgetStatusAsync();

        // Assert
        Assert.Equal(BudgetHealthStatus.Warning, result.Status);
        Assert.Equal(80, result.PercentageUsed);
    }

    [Fact]
    public async Task GetUpcomingBillsAsync_ReturnsEmptyForUnauthenticatedUser()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.UserId).Returns((string?)null);

        var service = new DashboardService(
            _context,
            _mockBudgetService.Object,
            _mockBillService.Object,
            _mockNotificationService.Object,
            _mockCurrentUserService.Object);

        // Act
        var result = await service.GetUpcomingBillsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUpcomingBillsAsync_CombinesUpcomingAndOverdueBills()
    {
        // Arrange
        var overdueBill = new Bill
        {
            BillId = 1,
            UserId = TestUserId,
            Name = "Overdue Rent",
            Amount = 10000,
            DueDate = DateTime.Today.AddDays(-5),
            Status = "Pending",
            Currency = "SEK"
        };
        var upcomingBill = new Bill
        {
            BillId = 2,
            UserId = TestUserId,
            Name = "Insurance",
            Amount = 500,
            DueDate = DateTime.Today.AddDays(10),
            Status = "Pending",
            Currency = "SEK"
        };

        _mockBillService.Setup(x => x.GetOverdueBillsAsync(TestUserId))
            .ReturnsAsync(new List<Bill> { overdueBill });
        _mockBillService.Setup(x => x.GetBillsDueSoonAsync(TestUserId, It.IsAny<int>()))
            .ReturnsAsync(new List<Bill> { upcomingBill });

        // Act
        var result = await _service.GetUpcomingBillsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.True(result.First().IsOverdue);
        Assert.Equal("Overdue Rent", result.First().Name);
    }

    [Fact]
    public async Task GetUpcomingBillsAsync_CorrectlyCalculatesDaysUntilDue()
    {
        // Arrange
        var today = DateTime.Today;
        var billDueInFiveDays = new Bill
        {
            BillId = 1,
            UserId = TestUserId,
            Name = "Future Bill",
            Amount = 1000,
            DueDate = today.AddDays(5),
            Status = "Pending",
            Currency = "SEK"
        };

        _mockBillService.Setup(x => x.GetBillsDueSoonAsync(TestUserId, It.IsAny<int>()))
            .ReturnsAsync(new List<Bill> { billDueInFiveDays });

        // Act
        var result = await _service.GetUpcomingBillsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(5, result.First().DaysUntilDue);
        Assert.False(result.First().IsOverdue);
    }

    [Fact]
    public async Task GetRecentInsightsAsync_ReturnsEmptyForUnauthenticatedUser()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.UserId).Returns((string?)null);

        var service = new DashboardService(
            _context,
            _mockBudgetService.Object,
            _mockBillService.Object,
            _mockNotificationService.Object,
            _mockCurrentUserService.Object);

        // Act
        var result = await service.GetRecentInsightsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecentInsightsAsync_MapsNotificationsToInsights()
    {
        // Arrange
        var notification = new Notification
        {
            NotificationId = 1,
            UserId = TestUserId,
            Type = SystemNotificationType.BudgetWarning,
            Title = "Budget Alert",
            Message = "You are approaching your budget limit",
            Priority = NotificationPriority.High,
            ActionUrl = "/budgets",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            IsRead = false
        };

        _mockNotificationService.Setup(x => x.GetActiveNotificationsAsync(TestUserId, It.IsAny<bool>()))
            .ReturnsAsync(new List<Notification> { notification });

        // Act
        var result = await _service.GetRecentInsightsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        var insight = result.First();
        Assert.Equal(InsightType.BudgetAlert, insight.Type);
        Assert.Equal("Budget Alert", insight.Title);
        Assert.Equal(InsightPriority.High, insight.Priority);
        Assert.False(insight.IsRead);
    }

    [Fact]
    public async Task GetRecentInsightsAsync_RespectsLimitParameter()
    {
        // Arrange
        var notifications = Enumerable.Range(1, 10).Select(i => new Notification
        {
            NotificationId = i,
            UserId = TestUserId,
            Type = SystemNotificationType.SystemAlert,
            Title = $"Notification {i}",
            Message = $"Message {i}",
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddMinutes(-i)
        }).ToList();

        _mockNotificationService.Setup(x => x.GetActiveNotificationsAsync(TestUserId, It.IsAny<bool>()))
            .ReturnsAsync(notifications);

        // Act
        var result = await _service.GetRecentInsightsAsync(limit: 3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }
}
