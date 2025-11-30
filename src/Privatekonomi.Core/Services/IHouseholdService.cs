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

    // Activity (Timeline) operations
    Task<IEnumerable<HouseholdActivity>> GetActivitiesAsync(int householdId, DateTime? startDate = null, DateTime? endDate = null);
    Task<HouseholdActivity> CreateActivityAsync(HouseholdActivity activity);
    Task<HouseholdActivity> UpdateActivityAsync(HouseholdActivity activity);
    Task<bool> DeleteActivityAsync(int activityId);
    
    // Activity Image operations
    Task<HouseholdActivityImage> AddActivityImageAsync(int activityId, string imagePath, string mimeType, long fileSize, string? caption = null);
    Task<bool> DeleteActivityImageAsync(int imageId);
    Task<IEnumerable<HouseholdActivityImage>> GetActivityImagesAsync(int activityId);
    Task<bool> UpdateImageOrderAsync(int imageId, int newOrder);

    // Task (To-Do) operations
    Task<IEnumerable<HouseholdTask>> GetTasksAsync(int householdId, bool? includeCompleted = null);
    Task<IEnumerable<HouseholdTask>> SearchTasksAsync(int householdId, string searchTerm);
    Task<IEnumerable<HouseholdTask>> GetTasksByStatusAsync(int householdId, HouseholdTaskStatus status);
    Task<Dictionary<HouseholdTaskStatus, IEnumerable<HouseholdTask>>> GetTasksGroupedByStatusAsync(int householdId, HouseholdActivityType? category = null);
    Task<HouseholdTask> CreateTaskAsync(HouseholdTask task);
    Task<HouseholdTask> UpdateTaskAsync(HouseholdTask task);
    Task<bool> UpdateTaskStatusAsync(int taskId, HouseholdTaskStatus newStatus);
    Task<bool> DeleteTaskAsync(int taskId);
    Task<bool> MarkTaskCompleteAsync(int taskId, int? completedByMemberId = null);
    Task<bool> MarkTaskIncompleteAsync(int taskId);
    Task<HouseholdTask?> CreateNextRecurrenceAsync(int taskId);
    Task ProcessRecurringTasksAsync(int householdId);
    
    // Shared Budget operations
    Task<Budget> CreateSharedBudgetAsync(Budget budget, Dictionary<int, decimal> memberContributions);
    Task<Budget> UpdateSharedBudgetAsync(Budget budget, Dictionary<int, decimal>? memberContributions = null);
    Task<IEnumerable<Budget>> GetHouseholdBudgetsAsync(int householdId);
    Task<Dictionary<int, decimal>> GetBudgetContributionsAsync(int budgetId);
    
    // Debt Settlement operations
    Task<DebtSettlement> CreateDebtAsync(int householdId, int debtorMemberId, int creditorMemberId, decimal amount, string? description = null);
    Task<bool> SettleDebtAsync(int debtSettlementId, string? settlementNote = null);
    Task<bool> CancelDebtAsync(int debtSettlementId);
    Task<IEnumerable<DebtSettlement>> GetHouseholdDebtsAsync(int householdId, DebtSettlementStatus? status = null);
    Task<Dictionary<int, decimal>> GetMemberDebtBalanceAsync(int householdId);
    Task<IEnumerable<DebtSettlement>> CalculateOptimalSettlementAsync(int householdId);
}
