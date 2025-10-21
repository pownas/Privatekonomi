using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class ChildAllowanceService : IChildAllowanceService
{
    private readonly PrivatekonomyContext _context;

    public ChildAllowanceService(PrivatekonomyContext context)
    {
        _context = context;
    }

    // Child Allowance operations
    public async Task<IEnumerable<ChildAllowance>> GetAllAllowancesAsync(int householdId)
    {
        return await _context.ChildAllowances
            .Include(ca => ca.HouseholdMember)
            .Include(ca => ca.AllowanceTransactions)
            .Include(ca => ca.AllowanceTasks)
            .Where(ca => ca.HouseholdMember!.HouseholdId == householdId)
            .OrderBy(ca => ca.HouseholdMember!.Name)
            .ToListAsync();
    }

    public async Task<ChildAllowance?> GetAllowanceByIdAsync(int allowanceId)
    {
        return await _context.ChildAllowances
            .Include(ca => ca.HouseholdMember)
            .Include(ca => ca.AllowanceTransactions)
            .Include(ca => ca.AllowanceTasks)
            .FirstOrDefaultAsync(ca => ca.ChildAllowanceId == allowanceId);
    }

    public async Task<ChildAllowance> CreateAllowanceAsync(ChildAllowance allowance)
    {
        allowance.CreatedAt = DateTime.Now;
        allowance.CurrentBalance = 0;
        allowance.IsActive = true;
        
        _context.ChildAllowances.Add(allowance);
        await _context.SaveChangesAsync();
        return allowance;
    }

    public async Task<ChildAllowance> UpdateAllowanceAsync(ChildAllowance allowance)
    {
        allowance.UpdatedAt = DateTime.Now;
        _context.ChildAllowances.Update(allowance);
        await _context.SaveChangesAsync();
        return allowance;
    }

    public async Task<bool> DeleteAllowanceAsync(int allowanceId)
    {
        var allowance = await _context.ChildAllowances.FindAsync(allowanceId);
        if (allowance == null)
            return false;

        _context.ChildAllowances.Remove(allowance);
        await _context.SaveChangesAsync();
        return true;
    }

    // Transaction operations
    public async Task<AllowanceTransaction> AddTransactionAsync(AllowanceTransaction transaction)
    {
        transaction.CreatedAt = DateTime.Now;
        
        _context.AllowanceTransactions.Add(transaction);
        
        // Update the allowance balance
        var allowance = await _context.ChildAllowances.FindAsync(transaction.ChildAllowanceId);
        if (allowance != null)
        {
            if (transaction.Type == AllowanceTransactionType.Deposit || 
                transaction.Type == AllowanceTransactionType.TaskReward)
            {
                allowance.CurrentBalance += transaction.Amount;
            }
            else if (transaction.Type == AllowanceTransactionType.Withdrawal)
            {
                allowance.CurrentBalance -= transaction.Amount;
            }
            else if (transaction.Type == AllowanceTransactionType.Adjustment)
            {
                allowance.CurrentBalance += transaction.Amount; // Can be negative for deductions
            }
            
            allowance.UpdatedAt = DateTime.Now;
        }
        
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<IEnumerable<AllowanceTransaction>> GetAllowanceTransactionsAsync(int allowanceId)
    {
        return await _context.AllowanceTransactions
            .Include(at => at.AllowanceTask)
            .Where(at => at.ChildAllowanceId == allowanceId)
            .OrderByDescending(at => at.TransactionDate)
            .ToListAsync();
    }

    public async Task<AllowanceTransaction> ProcessScheduledAllowanceAsync(int allowanceId)
    {
        var allowance = await _context.ChildAllowances.FindAsync(allowanceId);
        if (allowance == null)
            throw new InvalidOperationException("Allowance not found");

        if (!allowance.IsActive)
            throw new InvalidOperationException("Allowance is not active");

        var transaction = new AllowanceTransaction
        {
            ChildAllowanceId = allowanceId,
            Amount = allowance.Amount,
            Type = AllowanceTransactionType.Deposit,
            Description = $"Scheduled {GetFrequencyText(allowance.Frequency)} allowance",
            TransactionDate = DateTime.Now,
            CreatedAt = DateTime.Now
        };

        return await AddTransactionAsync(transaction);
    }

    // Task operations
    public async Task<AllowanceTask> CreateTaskAsync(AllowanceTask task)
    {
        task.CreatedAt = DateTime.Now;
        task.Status = AllowanceTaskStatus.Pending;
        
        _context.AllowanceTasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<AllowanceTask> UpdateTaskAsync(AllowanceTask task)
    {
        _context.AllowanceTasks.Update(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<bool> DeleteTaskAsync(int taskId)
    {
        var task = await _context.AllowanceTasks.FindAsync(taskId);
        if (task == null)
            return false;

        _context.AllowanceTasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AllowanceTask?> GetTaskByIdAsync(int taskId)
    {
        return await _context.AllowanceTasks
            .Include(at => at.ChildAllowance)
            .FirstOrDefaultAsync(at => at.AllowanceTaskId == taskId);
    }

    public async Task<IEnumerable<AllowanceTask>> GetAllowanceTasksAsync(int allowanceId, AllowanceTaskStatus? status = null)
    {
        var query = _context.AllowanceTasks
            .Where(at => at.ChildAllowanceId == allowanceId);

        if (status.HasValue)
            query = query.Where(at => at.Status == status.Value);

        return await query
            .OrderBy(at => at.DueDate)
            .ToListAsync();
    }

    public async Task<AllowanceTask> CompleteTaskAsync(int taskId)
    {
        var task = await _context.AllowanceTasks.FindAsync(taskId);
        if (task == null)
            throw new InvalidOperationException("Task not found");

        if (task.Status == AllowanceTaskStatus.Approved)
            throw new InvalidOperationException("Task is already approved");

        task.Status = AllowanceTaskStatus.Completed;
        task.CompletedDate = DateTime.Now;
        
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<AllowanceTask> ApproveTaskAsync(int taskId, string approvedBy)
    {
        var task = await _context.AllowanceTasks
            .Include(at => at.ChildAllowance)
            .FirstOrDefaultAsync(at => at.AllowanceTaskId == taskId);
            
        if (task == null)
            throw new InvalidOperationException("Task not found");

        if (task.Status != AllowanceTaskStatus.Completed)
            throw new InvalidOperationException("Task must be completed before approval");

        task.Status = AllowanceTaskStatus.Approved;
        task.ApprovedDate = DateTime.Now;
        task.ApprovedBy = approvedBy;

        // Create transaction for task reward
        var transaction = new AllowanceTransaction
        {
            ChildAllowanceId = task.ChildAllowanceId,
            AllowanceTaskId = taskId,
            Amount = task.RewardAmount,
            Type = AllowanceTransactionType.TaskReward,
            Description = $"Reward for task: {task.Name}",
            TransactionDate = DateTime.Now,
            CreatedAt = DateTime.Now
        };

        await AddTransactionAsync(transaction);
        
        return task;
    }

    public async Task<AllowanceTask> RejectTaskAsync(int taskId)
    {
        var task = await _context.AllowanceTasks.FindAsync(taskId);
        if (task == null)
            throw new InvalidOperationException("Task not found");

        task.Status = AllowanceTaskStatus.Rejected;
        await _context.SaveChangesAsync();
        return task;
    }

    private string GetFrequencyText(AllowanceFrequency frequency)
    {
        return frequency switch
        {
            AllowanceFrequency.Weekly => "weekly",
            AllowanceFrequency.BiWeekly => "bi-weekly",
            AllowanceFrequency.Monthly => "monthly",
            _ => frequency.ToString()
        };
    }
}
