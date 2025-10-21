using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IChildAllowanceService
{
    // Child Allowance operations
    Task<IEnumerable<ChildAllowance>> GetAllAllowancesAsync(int householdId);
    Task<ChildAllowance?> GetAllowanceByIdAsync(int allowanceId);
    Task<ChildAllowance> CreateAllowanceAsync(ChildAllowance allowance);
    Task<ChildAllowance> UpdateAllowanceAsync(ChildAllowance allowance);
    Task<bool> DeleteAllowanceAsync(int allowanceId);
    
    // Transaction operations
    Task<AllowanceTransaction> AddTransactionAsync(AllowanceTransaction transaction);
    Task<IEnumerable<AllowanceTransaction>> GetAllowanceTransactionsAsync(int allowanceId);
    Task<AllowanceTransaction> ProcessScheduledAllowanceAsync(int allowanceId);
    
    // Task operations
    Task<AllowanceTask> CreateTaskAsync(AllowanceTask task);
    Task<AllowanceTask> UpdateTaskAsync(AllowanceTask task);
    Task<bool> DeleteTaskAsync(int taskId);
    Task<AllowanceTask?> GetTaskByIdAsync(int taskId);
    Task<IEnumerable<AllowanceTask>> GetAllowanceTasksAsync(int allowanceId, AllowanceTaskStatus? status = null);
    Task<AllowanceTask> CompleteTaskAsync(int taskId);
    Task<AllowanceTask> ApproveTaskAsync(int taskId, string approvedBy);
    Task<AllowanceTask> RejectTaskAsync(int taskId);
}
