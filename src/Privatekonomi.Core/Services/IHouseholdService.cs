using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IHouseholdService
{
    // Household operations
    Task<IEnumerable<Household>> GetAllHouseholdsAsync();
    Task<Household?> GetHouseholdByIdAsync(int id);
    Task<Household> CreateHouseholdAsync(Household household);
    Task<Household> UpdateHouseholdAsync(Household household);
    Task<bool> DeleteHouseholdAsync(int id);

    // Member operations
    Task<HouseholdMember> AddMemberAsync(HouseholdMember member);
    Task<HouseholdMember> UpdateMemberAsync(HouseholdMember member);
    Task<bool> RemoveMemberAsync(int memberId);
    Task<IEnumerable<HouseholdMember>> GetHouseholdMembersAsync(int householdId);

    // Shared expense operations
    Task<SharedExpense> CreateSharedExpenseAsync(SharedExpense expense);
    Task<SharedExpense> UpdateSharedExpenseAsync(SharedExpense expense);
    Task<bool> DeleteSharedExpenseAsync(int expenseId);
    Task<IEnumerable<SharedExpense>> GetHouseholdExpensesAsync(int householdId);
    Task<SharedExpense?> GetSharedExpenseByIdAsync(int expenseId);

    // Expense distribution
    Task DistributeExpenseEquallyAsync(int expenseId);
    Task DistributeExpenseByPercentageAsync(int expenseId, Dictionary<int, decimal> memberPercentages);
    Task DistributeExpenseByAmountAsync(int expenseId, Dictionary<int, decimal> memberAmounts);
    Task DistributeExpenseByRoomSizeAsync(int expenseId, Dictionary<int, decimal> memberRoomSizes);

    // Reporting
    Task<Dictionary<int, decimal>> GetMemberTotalSharesAsync(int householdId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<ExpenseShare>> GetMemberExpenseSharesAsync(int memberId, DateTime? startDate = null, DateTime? endDate = null);
}
