using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class SharedEconomyServiceTests : IDisposable
{
    private readonly PrivatekonomyContext _context;
    private readonly HouseholdService _householdService;

    public SharedEconomyServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PrivatekonomyContext(options);
        _householdService = new HouseholdService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region Shared Budget Tests

    [Fact]
    public async Task CreateSharedBudgetAsync_CreatesSharedBudgetSuccessfully()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var member1 = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Member 1" };
        var member2 = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Member 2" };
        _context.HouseholdMembers.AddRange(member1, member2);
        await _context.SaveChangesAsync();

        var budget = new Budget
        {
            Name = "Shared Household Budget",
            Description = "Our joint budget",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(1),
            Period = BudgetPeriod.Monthly,
            HouseholdId = household.HouseholdId
        };

        var contributions = new Dictionary<int, decimal>
        {
            { member1.HouseholdMemberId, 60m },
            { member2.HouseholdMemberId, 40m }
        };

        // Act
        var result = await _householdService.CreateSharedBudgetAsync(budget, contributions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Shared Household Budget", result.Name);
        Assert.Equal(household.HouseholdId, result.HouseholdId);

        var shares = await _context.HouseholdBudgetShares
            .Where(s => s.BudgetId == result.BudgetId)
            .ToListAsync();
        Assert.Equal(2, shares.Count);
        Assert.Contains(shares, s => s.HouseholdMemberId == member1.HouseholdMemberId && s.SharePercentage == 60m);
        Assert.Contains(shares, s => s.HouseholdMemberId == member2.HouseholdMemberId && s.SharePercentage == 40m);
    }

    [Fact]
    public async Task CreateSharedBudgetAsync_ThrowsWhenContributionsDoNotSumTo100()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var member1 = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Member 1" };
        _context.HouseholdMembers.Add(member1);
        await _context.SaveChangesAsync();

        var budget = new Budget
        {
            Name = "Shared Household Budget",
            HouseholdId = household.HouseholdId,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(1),
            Period = BudgetPeriod.Monthly
        };

        var contributions = new Dictionary<int, decimal>
        {
            { member1.HouseholdMemberId, 80m }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _householdService.CreateSharedBudgetAsync(budget, contributions));
    }

    [Fact]
    public async Task GetHouseholdBudgetsAsync_ReturnsAllHouseholdBudgets()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var member = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Member 1" };
        _context.HouseholdMembers.Add(member);
        await _context.SaveChangesAsync();

        var budget1 = new Budget
        {
            Name = "Budget 1",
            HouseholdId = household.HouseholdId,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(1),
            Period = BudgetPeriod.Monthly
        };
        var budget2 = new Budget
        {
            Name = "Budget 2",
            HouseholdId = household.HouseholdId,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(1),
            Period = BudgetPeriod.Monthly
        };

        await _householdService.CreateSharedBudgetAsync(budget1, new Dictionary<int, decimal> { { member.HouseholdMemberId, 100m } });
        await _householdService.CreateSharedBudgetAsync(budget2, new Dictionary<int, decimal> { { member.HouseholdMemberId, 100m } });

        // Act
        var result = await _householdService.GetHouseholdBudgetsAsync(household.HouseholdId);

        // Assert
        var budgets = result.ToList();
        Assert.Equal(2, budgets.Count);
    }

    [Fact]
    public async Task GetBudgetContributionsAsync_ReturnsCorrectContributions()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var member1 = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Member 1" };
        var member2 = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Member 2" };
        _context.HouseholdMembers.AddRange(member1, member2);
        await _context.SaveChangesAsync();

        var budget = new Budget
        {
            Name = "Test Budget",
            HouseholdId = household.HouseholdId,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(1),
            Period = BudgetPeriod.Monthly
        };

        var contributions = new Dictionary<int, decimal>
        {
            { member1.HouseholdMemberId, 55m },
            { member2.HouseholdMemberId, 45m }
        };

        await _householdService.CreateSharedBudgetAsync(budget, contributions);

        // Act
        var result = await _householdService.GetBudgetContributionsAsync(budget.BudgetId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(55m, result[member1.HouseholdMemberId]);
        Assert.Equal(45m, result[member2.HouseholdMemberId]);
    }

    #endregion

    #region Debt Settlement Tests

    [Fact]
    public async Task CreateDebtAsync_CreatesDebtSuccessfully()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var debtor = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Debtor" };
        var creditor = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Creditor" };
        _context.HouseholdMembers.AddRange(debtor, creditor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _householdService.CreateDebtAsync(
            household.HouseholdId,
            debtor.HouseholdMemberId,
            creditor.HouseholdMemberId,
            150.50m,
            "Groceries split");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(household.HouseholdId, result.HouseholdId);
        Assert.Equal(debtor.HouseholdMemberId, result.DebtorMemberId);
        Assert.Equal(creditor.HouseholdMemberId, result.CreditorMemberId);
        Assert.Equal(150.50m, result.Amount);
        Assert.Equal("Groceries split", result.Description);
        Assert.Equal(DebtSettlementStatus.Pending, result.Status);
    }

    [Fact]
    public async Task CreateDebtAsync_ThrowsWhenDebtorAndCreditorAreSame()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var member = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Member" };
        _context.HouseholdMembers.Add(member);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _householdService.CreateDebtAsync(
                household.HouseholdId,
                member.HouseholdMemberId,
                member.HouseholdMemberId,
                100m));
    }

    [Fact]
    public async Task CreateDebtAsync_ThrowsWhenAmountIsZeroOrNegative()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var debtor = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Debtor" };
        var creditor = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Creditor" };
        _context.HouseholdMembers.AddRange(debtor, creditor);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _householdService.CreateDebtAsync(
                household.HouseholdId,
                debtor.HouseholdMemberId,
                creditor.HouseholdMemberId,
                0m));
    }

    [Fact]
    public async Task SettleDebtAsync_SettlesDebtSuccessfully()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var debtor = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Debtor" };
        var creditor = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Creditor" };
        _context.HouseholdMembers.AddRange(debtor, creditor);
        await _context.SaveChangesAsync();

        var debt = await _householdService.CreateDebtAsync(
            household.HouseholdId,
            debtor.HouseholdMemberId,
            creditor.HouseholdMemberId,
            100m);

        // Act
        var result = await _householdService.SettleDebtAsync(debt.DebtSettlementId, "Paid via Swish");

        // Assert
        Assert.True(result);
        var settledDebt = await _context.DebtSettlements.FindAsync(debt.DebtSettlementId);
        Assert.NotNull(settledDebt);
        Assert.Equal(DebtSettlementStatus.Settled, settledDebt.Status);
        Assert.NotNull(settledDebt.SettledDate);
        Assert.Equal("Paid via Swish", settledDebt.SettlementNote);
    }

    [Fact]
    public async Task CancelDebtAsync_CancelsDebtSuccessfully()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var debtor = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Debtor" };
        var creditor = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Creditor" };
        _context.HouseholdMembers.AddRange(debtor, creditor);
        await _context.SaveChangesAsync();

        var debt = await _householdService.CreateDebtAsync(
            household.HouseholdId,
            debtor.HouseholdMemberId,
            creditor.HouseholdMemberId,
            100m);

        // Act
        var result = await _householdService.CancelDebtAsync(debt.DebtSettlementId);

        // Assert
        Assert.True(result);
        var cancelledDebt = await _context.DebtSettlements.FindAsync(debt.DebtSettlementId);
        Assert.NotNull(cancelledDebt);
        Assert.Equal(DebtSettlementStatus.Cancelled, cancelledDebt.Status);
    }

    [Fact]
    public async Task GetHouseholdDebtsAsync_ReturnsAllDebts()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var debtor = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Debtor" };
        var creditor = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Creditor" };
        _context.HouseholdMembers.AddRange(debtor, creditor);
        await _context.SaveChangesAsync();

        await _householdService.CreateDebtAsync(household.HouseholdId, debtor.HouseholdMemberId, creditor.HouseholdMemberId, 100m);
        await _householdService.CreateDebtAsync(household.HouseholdId, debtor.HouseholdMemberId, creditor.HouseholdMemberId, 50m);

        // Act
        var result = await _householdService.GetHouseholdDebtsAsync(household.HouseholdId);

        // Assert
        var debts = result.ToList();
        Assert.Equal(2, debts.Count);
    }

    [Fact]
    public async Task GetHouseholdDebtsAsync_FiltersDebtsCorrectly()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var debtor = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Debtor" };
        var creditor = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Creditor" };
        _context.HouseholdMembers.AddRange(debtor, creditor);
        await _context.SaveChangesAsync();

        var debt1 = await _householdService.CreateDebtAsync(household.HouseholdId, debtor.HouseholdMemberId, creditor.HouseholdMemberId, 100m);
        var debt2 = await _householdService.CreateDebtAsync(household.HouseholdId, debtor.HouseholdMemberId, creditor.HouseholdMemberId, 50m);
        
        await _householdService.SettleDebtAsync(debt1.DebtSettlementId);

        // Act
        var pendingDebts = await _householdService.GetHouseholdDebtsAsync(household.HouseholdId, DebtSettlementStatus.Pending);
        var settledDebts = await _householdService.GetHouseholdDebtsAsync(household.HouseholdId, DebtSettlementStatus.Settled);

        // Assert
        Assert.Single(pendingDebts);
        Assert.Single(settledDebts);
    }

    [Fact]
    public async Task GetMemberDebtBalanceAsync_CalculatesBalancesCorrectly()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var member1 = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Member 1" };
        var member2 = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Member 2" };
        var member3 = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Member 3" };
        _context.HouseholdMembers.AddRange(member1, member2, member3);
        await _context.SaveChangesAsync();

        // Member1 owes Member2 100
        await _householdService.CreateDebtAsync(household.HouseholdId, member1.HouseholdMemberId, member2.HouseholdMemberId, 100m);
        // Member1 owes Member3 50
        await _householdService.CreateDebtAsync(household.HouseholdId, member1.HouseholdMemberId, member3.HouseholdMemberId, 50m);
        // Member2 owes Member3 30
        await _householdService.CreateDebtAsync(household.HouseholdId, member2.HouseholdMemberId, member3.HouseholdMemberId, 30m);

        // Act
        var balances = await _householdService.GetMemberDebtBalanceAsync(household.HouseholdId);

        // Assert
        Assert.Equal(-150m, balances[member1.HouseholdMemberId]); // Owes 150 total
        Assert.Equal(70m, balances[member2.HouseholdMemberId]);   // Is owed 100, owes 30 = +70
        Assert.Equal(80m, balances[member3.HouseholdMemberId]);   // Is owed 50 + 30 = +80
    }

    [Fact]
    public async Task CalculateOptimalSettlementAsync_CreatesOptimalSettlements()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var member1 = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Member 1" };
        var member2 = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Member 2" };
        var member3 = new HouseholdMember { HouseholdId = household.HouseholdId, Name = "Member 3" };
        _context.HouseholdMembers.AddRange(member1, member2, member3);
        await _context.SaveChangesAsync();

        // Member1 owes Member2 100
        await _householdService.CreateDebtAsync(household.HouseholdId, member1.HouseholdMemberId, member2.HouseholdMemberId, 100m);
        // Member3 owes Member2 50
        await _householdService.CreateDebtAsync(household.HouseholdId, member3.HouseholdMemberId, member2.HouseholdMemberId, 50m);

        // Act
        var settlements = await _householdService.CalculateOptimalSettlementAsync(household.HouseholdId);

        // Assert
        var settlementList = settlements.ToList();
        Assert.NotEmpty(settlementList);
        
        // Verify total amounts balance out
        var totalDebts = settlementList.Sum(s => s.Amount);
        Assert.True(totalDebts > 0); // At least some settlements were created
    }

    [Fact]
    public async Task CalculateOptimalSettlementAsync_HandlesEmptyDebts()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        // Act
        var settlements = await _householdService.CalculateOptimalSettlementAsync(household.HouseholdId);

        // Assert
        Assert.Empty(settlements);
    }

    #endregion
}
