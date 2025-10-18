using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class HouseholdService : IHouseholdService
{
    private readonly PrivatekonomyContext _context;

    public HouseholdService(PrivatekonomyContext context)
    {
        _context = context;
    }

    // Household operations
    public async Task<IEnumerable<Household>> GetAllHouseholdsAsync()
    {
        return await _context.Households
            .Include(h => h.Members)
            .Include(h => h.SharedExpenses)
            .ToListAsync();
    }

    public async Task<Household?> GetHouseholdByIdAsync(int id)
    {
        return await _context.Households
            .Include(h => h.Members)
            .Include(h => h.SharedExpenses)
                .ThenInclude(se => se.ExpenseShares)
                    .ThenInclude(es => es.HouseholdMember)
            .FirstOrDefaultAsync(h => h.HouseholdId == id);
    }

    public async Task<Household> CreateHouseholdAsync(Household household)
    {
        household.CreatedDate = DateTime.Now;
        _context.Households.Add(household);
        await _context.SaveChangesAsync();
        return household;
    }

    public async Task<Household> UpdateHouseholdAsync(Household household)
    {
        _context.Households.Update(household);
        await _context.SaveChangesAsync();
        return household;
    }

    public async Task<bool> DeleteHouseholdAsync(int id)
    {
        var household = await _context.Households.FindAsync(id);
        if (household == null)
            return false;

        _context.Households.Remove(household);
        await _context.SaveChangesAsync();
        return true;
    }

    // Member operations
    public async Task<HouseholdMember> AddMemberAsync(HouseholdMember member)
    {
        member.JoinedDate = DateTime.Now;
        member.IsActive = true;
        _context.HouseholdMembers.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task<HouseholdMember> UpdateMemberAsync(HouseholdMember member)
    {
        _context.HouseholdMembers.Update(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task<bool> RemoveMemberAsync(int memberId)
    {
        var member = await _context.HouseholdMembers.FindAsync(memberId);
        if (member == null)
            return false;

        member.IsActive = false;
        member.LeftDate = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<HouseholdMember>> GetHouseholdMembersAsync(int householdId)
    {
        return await _context.HouseholdMembers
            .Where(m => m.HouseholdId == householdId)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    // Shared expense operations
    public async Task<SharedExpense> CreateSharedExpenseAsync(SharedExpense expense)
    {
        expense.CreatedDate = DateTime.Now;
        _context.SharedExpenses.Add(expense);
        await _context.SaveChangesAsync();
        return expense;
    }

    public async Task<SharedExpense> UpdateSharedExpenseAsync(SharedExpense expense)
    {
        _context.SharedExpenses.Update(expense);
        await _context.SaveChangesAsync();
        return expense;
    }

    public async Task<bool> DeleteSharedExpenseAsync(int expenseId)
    {
        var expense = await _context.SharedExpenses.FindAsync(expenseId);
        if (expense == null)
            return false;

        _context.SharedExpenses.Remove(expense);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<SharedExpense>> GetHouseholdExpensesAsync(int householdId)
    {
        return await _context.SharedExpenses
            .Include(se => se.ExpenseShares)
                .ThenInclude(es => es.HouseholdMember)
            .Where(se => se.HouseholdId == householdId)
            .OrderByDescending(se => se.ExpenseDate)
            .ToListAsync();
    }

    public async Task<SharedExpense?> GetSharedExpenseByIdAsync(int expenseId)
    {
        return await _context.SharedExpenses
            .Include(se => se.ExpenseShares)
                .ThenInclude(es => es.HouseholdMember)
            .FirstOrDefaultAsync(se => se.SharedExpenseId == expenseId);
    }

    // Expense distribution
    public async Task DistributeExpenseEquallyAsync(int expenseId)
    {
        var expense = await GetSharedExpenseByIdAsync(expenseId);
        if (expense == null)
            throw new InvalidOperationException("Expense not found");

        var activeMembers = await _context.HouseholdMembers
            .Where(m => m.HouseholdId == expense.HouseholdId && m.IsActive)
            .ToListAsync();

        if (!activeMembers.Any())
            throw new InvalidOperationException("No active members in household");

        // Remove existing shares
        var existingShares = _context.ExpenseShares.Where(es => es.SharedExpenseId == expenseId);
        _context.ExpenseShares.RemoveRange(existingShares);

        // Create new equal shares
        var shareAmount = expense.TotalAmount / activeMembers.Count;
        foreach (var member in activeMembers)
        {
            _context.ExpenseShares.Add(new ExpenseShare
            {
                SharedExpenseId = expenseId,
                HouseholdMemberId = member.HouseholdMemberId,
                ShareAmount = shareAmount,
                SharePercentage = 100m / activeMembers.Count
            });
        }

        expense.SplitMethod = SplitMethod.Equal;
        await _context.SaveChangesAsync();
    }

    public async Task DistributeExpenseByPercentageAsync(int expenseId, Dictionary<int, decimal> memberPercentages)
    {
        var expense = await GetSharedExpenseByIdAsync(expenseId);
        if (expense == null)
            throw new InvalidOperationException("Expense not found");

        if (Math.Abs(memberPercentages.Values.Sum() - 100m) > 0.01m)
            throw new InvalidOperationException("Percentages must sum to 100%");

        // Remove existing shares
        var existingShares = _context.ExpenseShares.Where(es => es.SharedExpenseId == expenseId);
        _context.ExpenseShares.RemoveRange(existingShares);

        // Create new percentage-based shares
        foreach (var (memberId, percentage) in memberPercentages)
        {
            _context.ExpenseShares.Add(new ExpenseShare
            {
                SharedExpenseId = expenseId,
                HouseholdMemberId = memberId,
                ShareAmount = expense.TotalAmount * (percentage / 100m),
                SharePercentage = percentage
            });
        }

        expense.SplitMethod = SplitMethod.ByPercentage;
        await _context.SaveChangesAsync();
    }

    public async Task DistributeExpenseByAmountAsync(int expenseId, Dictionary<int, decimal> memberAmounts)
    {
        var expense = await GetSharedExpenseByIdAsync(expenseId);
        if (expense == null)
            throw new InvalidOperationException("Expense not found");

        if (Math.Abs(memberAmounts.Values.Sum() - expense.TotalAmount) > 0.01m)
            throw new InvalidOperationException("Amounts must sum to total expense amount");

        // Remove existing shares
        var existingShares = _context.ExpenseShares.Where(es => es.SharedExpenseId == expenseId);
        _context.ExpenseShares.RemoveRange(existingShares);

        // Create new amount-based shares
        foreach (var (memberId, amount) in memberAmounts)
        {
            _context.ExpenseShares.Add(new ExpenseShare
            {
                SharedExpenseId = expenseId,
                HouseholdMemberId = memberId,
                ShareAmount = amount,
                SharePercentage = (amount / expense.TotalAmount) * 100m
            });
        }

        expense.SplitMethod = SplitMethod.ByAmount;
        await _context.SaveChangesAsync();
    }

    public async Task DistributeExpenseByRoomSizeAsync(int expenseId, Dictionary<int, decimal> memberRoomSizes)
    {
        var expense = await GetSharedExpenseByIdAsync(expenseId);
        if (expense == null)
            throw new InvalidOperationException("Expense not found");

        var totalRoomSize = memberRoomSizes.Values.Sum();
        if (totalRoomSize <= 0)
            throw new InvalidOperationException("Total room size must be greater than zero");

        // Remove existing shares
        var existingShares = _context.ExpenseShares.Where(es => es.SharedExpenseId == expenseId);
        _context.ExpenseShares.RemoveRange(existingShares);

        // Create new room size-based shares
        foreach (var (memberId, roomSize) in memberRoomSizes)
        {
            var percentage = (roomSize / totalRoomSize) * 100m;
            _context.ExpenseShares.Add(new ExpenseShare
            {
                SharedExpenseId = expenseId,
                HouseholdMemberId = memberId,
                ShareAmount = expense.TotalAmount * (roomSize / totalRoomSize),
                SharePercentage = percentage,
                RoomSize = roomSize
            });
        }

        expense.SplitMethod = SplitMethod.ByRoomSize;
        await _context.SaveChangesAsync();
    }

    // Reporting
    public async Task<Dictionary<int, decimal>> GetMemberTotalSharesAsync(int householdId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ExpenseShares
            .Include(es => es.SharedExpense)
            .Include(es => es.HouseholdMember)
            .Where(es => es.HouseholdMember!.HouseholdId == householdId);

        if (startDate.HasValue)
            query = query.Where(es => es.SharedExpense!.ExpenseDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(es => es.SharedExpense!.ExpenseDate <= endDate.Value);

        var shares = await query.ToListAsync();

        return shares
            .GroupBy(es => es.HouseholdMemberId)
            .ToDictionary(g => g.Key, g => g.Sum(es => es.ShareAmount));
    }

    public async Task<IEnumerable<ExpenseShare>> GetMemberExpenseSharesAsync(int memberId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ExpenseShares
            .Include(es => es.SharedExpense)
            .Include(es => es.HouseholdMember)
            .Where(es => es.HouseholdMemberId == memberId);

        if (startDate.HasValue)
            query = query.Where(es => es.SharedExpense!.ExpenseDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(es => es.SharedExpense!.ExpenseDate <= endDate.Value);

        return await query
            .OrderByDescending(es => es.SharedExpense!.ExpenseDate)
            .ToListAsync();
    }
}
