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

    // Activity (Timeline) operations
    public async Task<IEnumerable<HouseholdActivity>> GetActivitiesAsync(int householdId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.HouseholdActivities
            .Include(a => a.CompletedByMember)
            .Where(a => a.HouseholdId == householdId);

        if (startDate.HasValue)
            query = query.Where(a => a.CompletedDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.CompletedDate <= endDate.Value);

        return await query
            .OrderByDescending(a => a.CompletedDate)
            .ToListAsync();
    }

    public async Task<HouseholdActivity> CreateActivityAsync(HouseholdActivity activity)
    {
        activity.CreatedDate = DateTime.Now;
        if (activity.CompletedDate == default)
            activity.CompletedDate = DateTime.Now;
        
        _context.HouseholdActivities.Add(activity);
        await _context.SaveChangesAsync();
        return activity;
    }

    public async Task<HouseholdActivity> UpdateActivityAsync(HouseholdActivity activity)
    {
        _context.HouseholdActivities.Update(activity);
        await _context.SaveChangesAsync();
        return activity;
    }

    public async Task<bool> DeleteActivityAsync(int activityId)
    {
        var activity = await _context.HouseholdActivities.FindAsync(activityId);
        if (activity == null)
            return false;

        _context.HouseholdActivities.Remove(activity);
        await _context.SaveChangesAsync();
        return true;
    }

    // Task (To-Do) operations
    public async Task<IEnumerable<HouseholdTask>> GetTasksAsync(int householdId, bool? includeCompleted = null)
    {
        var query = _context.HouseholdTasks
            .Include(t => t.AssignedToMember)
            .Include(t => t.CompletedByMember)
            .Where(t => t.HouseholdId == householdId);

        if (includeCompleted.HasValue)
        {
            if (includeCompleted.Value)
            {
                // Include all tasks
            }
            else
            {
                // Only include incomplete tasks
                query = query.Where(t => !t.IsCompleted);
            }
        }

        return await query
            .OrderBy(t => t.IsCompleted)
            .ThenByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<HouseholdTask>> SearchTasksAsync(int householdId, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetTasksAsync(householdId);

        var query = _context.HouseholdTasks
            .Include(t => t.AssignedToMember)
            .Include(t => t.CompletedByMember)
            .Where(t => t.HouseholdId == householdId);

        var lowerSearchTerm = searchTerm.ToLower();
        query = query.Where(t => 
            t.Title.ToLower().Contains(lowerSearchTerm) || 
            (t.Description != null && t.Description.ToLower().Contains(lowerSearchTerm)));

        return await query
            .OrderBy(t => t.IsCompleted)
            .ThenByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<HouseholdTask> CreateTaskAsync(HouseholdTask task)
    {
        task.CreatedDate = DateTime.Now;
        task.IsCompleted = false;
        task.CompletedDate = null;
        
        _context.HouseholdTasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<HouseholdTask> UpdateTaskAsync(HouseholdTask task)
    {
        _context.HouseholdTasks.Update(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<bool> DeleteTaskAsync(int taskId)
    {
        var task = await _context.HouseholdTasks.FindAsync(taskId);
        if (task == null)
            return false;

        _context.HouseholdTasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkTaskCompleteAsync(int taskId, int? completedByMemberId = null)
    {
        var task = await _context.HouseholdTasks.FindAsync(taskId);
        if (task == null)
            return false;

        task.IsCompleted = true;
        task.CompletedDate = DateTime.Now;
        task.CompletedByMemberId = completedByMemberId;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkTaskIncompleteAsync(int taskId)
    {
        var task = await _context.HouseholdTasks.FindAsync(taskId);
        if (task == null)
            return false;

        task.IsCompleted = false;
        task.CompletedDate = null;
        task.CompletedByMemberId = null;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<HouseholdTask>> GetTasksByStatusAsync(int householdId, HouseholdTaskStatus status)
    {
        return await _context.HouseholdTasks
            .Include(t => t.AssignedToMember)
            .Include(t => t.CompletedByMember)
            .Where(t => t.HouseholdId == householdId && t.Status == status)
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<Dictionary<HouseholdTaskStatus, IEnumerable<HouseholdTask>>> GetTasksGroupedByStatusAsync(int householdId, HouseholdActivityType? category = null)
    {
        var query = _context.HouseholdTasks
            .Include(t => t.AssignedToMember)
            .Include(t => t.CompletedByMember)
            .Where(t => t.HouseholdId == householdId);

        if (category.HasValue)
        {
            query = query.Where(t => t.Category == category.Value);
        }

        var tasks = await query
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .ToListAsync();

        return tasks.GroupBy(t => t.Status)
            .ToDictionary(g => g.Key, g => g.AsEnumerable());
    }

    public async Task<bool> UpdateTaskStatusAsync(int taskId, HouseholdTaskStatus newStatus)
    {
        var task = await _context.HouseholdTasks.FindAsync(taskId);
        if (task == null)
            return false;

        task.Status = newStatus;

        // Automatically mark as completed when status changes to Done
        if (newStatus == HouseholdTaskStatus.Done && !task.IsCompleted)
        {
            task.IsCompleted = true;
            task.CompletedDate = DateTime.Now;

            // If this is a recurring task, create the next occurrence
            if (task.IsRecurring)
            {
                await CreateNextRecurrenceAsync(taskId);
            }
        }
        // Mark as incomplete if moved back from Done
        else if (newStatus != HouseholdTaskStatus.Done && task.IsCompleted)
        {
            task.IsCompleted = false;
            task.CompletedDate = null;
            task.CompletedByMemberId = null;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<HouseholdTask?> CreateNextRecurrenceAsync(int taskId)
    {
        var task = await _context.HouseholdTasks.FindAsync(taskId);
        if (task == null || !task.IsRecurring || !task.RecurrencePattern.HasValue)
            return null;

        // Calculate next due date
        var nextDueDate = CalculateNextDueDate(task.DueDate ?? DateTime.Today, task.RecurrencePattern.Value, task.RecurrenceInterval ?? 1);

        // Create new task instance
        var nextTask = new HouseholdTask
        {
            HouseholdId = task.HouseholdId,
            Title = task.Title,
            Description = task.Description,
            Category = task.Category,
            Priority = task.Priority,
            AssignedToMemberId = task.AssignedToMemberId,
            DueDate = nextDueDate,
            IsRecurring = true,
            RecurrencePattern = task.RecurrencePattern,
            RecurrenceInterval = task.RecurrenceInterval,
            ParentTaskId = task.ParentTaskId ?? task.HouseholdTaskId,
            CreatedDate = DateTime.Now,
            Status = HouseholdTaskStatus.ToDo
        };

        _context.HouseholdTasks.Add(nextTask);
        
        // Update the current task's NextDueDate
        task.NextDueDate = nextDueDate;
        
        await _context.SaveChangesAsync();
        return nextTask;
    }

    public async Task ProcessRecurringTasksAsync(int householdId)
    {
        // Find all completed recurring tasks that don't have a next occurrence scheduled
        var completedRecurringTasks = await _context.HouseholdTasks
            .Where(t => t.HouseholdId == householdId 
                && t.IsRecurring 
                && t.IsCompleted 
                && !t.NextDueDate.HasValue)
            .ToListAsync();

        foreach (var task in completedRecurringTasks)
        {
            await CreateNextRecurrenceAsync(task.HouseholdTaskId);
        }
    }

    private DateTime CalculateNextDueDate(DateTime currentDueDate, RecurrencePattern pattern, int interval)
    {
        return pattern switch
        {
            RecurrencePattern.Daily => currentDueDate.AddDays(interval),
            RecurrencePattern.Weekly => currentDueDate.AddDays(7 * interval),
            RecurrencePattern.BiWeekly => currentDueDate.AddDays(14 * interval),
            RecurrencePattern.Monthly => currentDueDate.AddMonths(interval),
            RecurrencePattern.Quarterly => currentDueDate.AddMonths(3 * interval),
            RecurrencePattern.Yearly => currentDueDate.AddYears(interval),
            _ => currentDueDate.AddDays(1)
        };
    }
    
    // Shared Budget operations
    public async Task<Budget> CreateSharedBudgetAsync(Budget budget, Dictionary<int, decimal> memberContributions)
    {
        if (!budget.HouseholdId.HasValue)
            throw new InvalidOperationException("Budget must be associated with a household");
        
        // Validate household exists
        var household = await _context.Households.FindAsync(budget.HouseholdId.Value);
        if (household == null)
            throw new InvalidOperationException("Household not found");
        
        // Validate contributions sum to 100%
        var totalPercentage = memberContributions.Values.Sum();
        if (Math.Abs(totalPercentage - 100m) > 0.01m)
            throw new InvalidOperationException("Contributions must sum to 100%");
        
        budget.CreatedAt = DateTime.Now;
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();
        
        // Validate that all members belong to the household
        var householdMemberIds = await _context.HouseholdMembers
            .Where(m => m.HouseholdId == budget.HouseholdId.Value && m.IsActive)
            .Select(m => m.HouseholdMemberId)
            .ToListAsync();
        
        foreach (var memberId in memberContributions.Keys)
        {
            if (!householdMemberIds.Contains(memberId))
                throw new InvalidOperationException($"Member {memberId} does not belong to household {budget.HouseholdId}");
        }
        
        // Create budget shares
        foreach (var (memberId, percentage) in memberContributions)
        {
            if (percentage < 0 || percentage > 100)
                throw new InvalidOperationException("Percentage must be between 0 and 100");
            
            _context.HouseholdBudgetShares.Add(new HouseholdBudgetShare
            {
                BudgetId = budget.BudgetId,
                HouseholdMemberId = memberId,
                SharePercentage = percentage,
                CreatedAt = DateTime.Now
            });
        }
        
        await _context.SaveChangesAsync();
        return budget;
    }
    
    public async Task<Budget> UpdateSharedBudgetAsync(Budget budget, Dictionary<int, decimal>? memberContributions = null)
    {
        budget.UpdatedAt = DateTime.Now;
        _context.Budgets.Update(budget);
        
        if (memberContributions != null)
        {
            // Validate contributions sum to 100%
            var totalPercentage = memberContributions.Values.Sum();
            if (Math.Abs(totalPercentage - 100m) > 0.01m)
                throw new InvalidOperationException("Contributions must sum to 100%");
            
            // Remove existing shares
            var existingShares = _context.HouseholdBudgetShares.Where(s => s.BudgetId == budget.BudgetId);
            _context.HouseholdBudgetShares.RemoveRange(existingShares);
            
            // Create new shares
            foreach (var (memberId, percentage) in memberContributions)
            {
                _context.HouseholdBudgetShares.Add(new HouseholdBudgetShare
                {
                    BudgetId = budget.BudgetId,
                    HouseholdMemberId = memberId,
                    SharePercentage = percentage,
                    CreatedAt = DateTime.Now
                });
            }
        }
        
        await _context.SaveChangesAsync();
        return budget;
    }
    
    public async Task<IEnumerable<Budget>> GetHouseholdBudgetsAsync(int householdId)
    {
        return await _context.Budgets
            .Include(b => b.BudgetCategories)
            .Include(b => b.HouseholdBudgetShares)
                .ThenInclude(s => s.HouseholdMember)
            .Where(b => b.HouseholdId == householdId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<Dictionary<int, decimal>> GetBudgetContributionsAsync(int budgetId)
    {
        var shares = await _context.HouseholdBudgetShares
            .Where(s => s.BudgetId == budgetId)
            .ToListAsync();
        
        return shares.ToDictionary(s => s.HouseholdMemberId, s => s.SharePercentage);
    }
    
    // Debt Settlement operations
    public async Task<DebtSettlement> CreateDebtAsync(int householdId, int debtorMemberId, int creditorMemberId, decimal amount, string? description = null)
    {
        if (debtorMemberId == creditorMemberId)
            throw new InvalidOperationException("Debtor and creditor cannot be the same member");
        
        if (amount <= 0)
            throw new InvalidOperationException("Amount must be greater than zero");
        
        // Validate that both members belong to the household
        var householdMemberIds = await _context.HouseholdMembers
            .Where(m => m.HouseholdId == householdId && m.IsActive)
            .Select(m => m.HouseholdMemberId)
            .ToListAsync();
        
        if (!householdMemberIds.Contains(debtorMemberId))
            throw new InvalidOperationException($"Debtor member {debtorMemberId} does not belong to household {householdId}");
        
        if (!householdMemberIds.Contains(creditorMemberId))
            throw new InvalidOperationException($"Creditor member {creditorMemberId} does not belong to household {householdId}");
        
        var debt = new DebtSettlement
        {
            HouseholdId = householdId,
            DebtorMemberId = debtorMemberId,
            CreditorMemberId = creditorMemberId,
            Amount = amount,
            Description = description,
            Status = DebtSettlementStatus.Pending,
            CreatedDate = DateTime.Now
        };
        
        _context.DebtSettlements.Add(debt);
        await _context.SaveChangesAsync();
        return debt;
    }
    
    public async Task<bool> SettleDebtAsync(int debtSettlementId, string? settlementNote = null)
    {
        var debt = await _context.DebtSettlements.FindAsync(debtSettlementId);
        if (debt == null)
            return false;
        
        if (debt.Status != DebtSettlementStatus.Pending)
            throw new InvalidOperationException("Only pending debts can be settled");
        
        debt.Status = DebtSettlementStatus.Settled;
        debt.SettledDate = DateTime.Now;
        debt.SettlementNote = settlementNote;
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> CancelDebtAsync(int debtSettlementId)
    {
        var debt = await _context.DebtSettlements.FindAsync(debtSettlementId);
        if (debt == null)
            return false;
        
        if (debt.Status != DebtSettlementStatus.Pending)
            throw new InvalidOperationException("Only pending debts can be cancelled");
        
        debt.Status = DebtSettlementStatus.Cancelled;
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<IEnumerable<DebtSettlement>> GetHouseholdDebtsAsync(int householdId, DebtSettlementStatus? status = null)
    {
        var query = _context.DebtSettlements
            .Include(d => d.DebtorMember)
            .Include(d => d.CreditorMember)
            .Where(d => d.HouseholdId == householdId);
        
        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);
        
        return await query
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }
    
    public async Task<Dictionary<int, decimal>> GetMemberDebtBalanceAsync(int householdId)
    {
        var pendingDebts = await _context.DebtSettlements
            .Where(d => d.HouseholdId == householdId && d.Status == DebtSettlementStatus.Pending)
            .ToListAsync();
        
        var balances = new Dictionary<int, decimal>();
        
        foreach (var debt in pendingDebts)
        {
            // Debtor owes money (negative balance)
            if (!balances.ContainsKey(debt.DebtorMemberId))
                balances[debt.DebtorMemberId] = 0;
            balances[debt.DebtorMemberId] -= debt.Amount;
            
            // Creditor is owed money (positive balance)
            if (!balances.ContainsKey(debt.CreditorMemberId))
                balances[debt.CreditorMemberId] = 0;
            balances[debt.CreditorMemberId] += debt.Amount;
        }
        
        return balances;
    }
    
    public async Task<IEnumerable<DebtSettlement>> CalculateOptimalSettlementAsync(int householdId)
    {
        // Get all active members' debt balances
        var balances = await GetMemberDebtBalanceAsync(householdId);
        
        if (!balances.Any())
            return new List<DebtSettlement>();
        
        // Separate debtors (negative balance) and creditors (positive balance)
        var debtors = balances.Where(b => b.Value < 0).OrderBy(b => b.Value).ToList();
        var creditors = balances.Where(b => b.Value > 0).OrderByDescending(b => b.Value).ToList();
        
        var settlements = new List<DebtSettlement>();
        var debtorIndex = 0;
        var creditorIndex = 0;
        
        // Simplified debt settlement algorithm (greedy approach)
        while (debtorIndex < debtors.Count && creditorIndex < creditors.Count)
        {
            var debtor = debtors[debtorIndex];
            var creditor = creditors[creditorIndex];
            
            var settlementAmount = Math.Min(Math.Abs(debtor.Value), creditor.Value);
            
            settlements.Add(new DebtSettlement
            {
                HouseholdId = householdId,
                DebtorMemberId = debtor.Key,
                CreditorMemberId = creditor.Key,
                Amount = settlementAmount,
                Description = "Automatisk balansering",
                Status = DebtSettlementStatus.Pending,
                CreatedDate = DateTime.Now
            });
            
            // Update balances
            var newDebtorBalance = debtor.Value + settlementAmount;
            var newCreditorBalance = creditor.Value - settlementAmount;
            
            if (Math.Abs(newDebtorBalance) < 0.01m)
                debtorIndex++;
            else
                debtors[debtorIndex] = new KeyValuePair<int, decimal>(debtor.Key, newDebtorBalance);
            
            if (Math.Abs(newCreditorBalance) < 0.01m)
                creditorIndex++;
            else
                creditors[creditorIndex] = new KeyValuePair<int, decimal>(creditor.Key, newCreditorBalance);
        }
        
        return settlements;
    }
}
