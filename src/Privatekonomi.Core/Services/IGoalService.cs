using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IGoalService
{
    Task<IEnumerable<Goal>> GetAllGoalsAsync();
    Task<Goal?> GetGoalByIdAsync(int id);
    Task<Goal> CreateGoalAsync(Goal goal);
    Task<Goal> UpdateGoalAsync(Goal goal);
    Task DeleteGoalAsync(int id);
    Task<IEnumerable<Goal>> GetActiveGoalsAsync();
    Task<decimal> GetTotalProgress();
    Task<Goal> UpdateGoalProgressAsync(int id, decimal currentAmount);
    Task<IEnumerable<Goal>> GetGoalsByPriorityAsync(int priority);
    Task<bool> UpdateGoalPrioritiesAsync(Dictionary<int, int> goalPriorities);
    
    // Simulation methods
    DateTime? CalculateCompletionDate(Goal goal, decimal monthlySavings);
    int CalculateMonthsToCompletion(Goal goal, decimal monthlySavings);
    SavingsSimulationResult SimulateSavingsChange(Goal goal, decimal newMonthlySavings);
}

public class SavingsSimulationResult
{
    public int CurrentMonthsToCompletion { get; set; }
    public int NewMonthsToCompletion { get; set; }
    public int MonthsDifference { get; set; }
    public DateTime? CurrentCompletionDate { get; set; }
    public DateTime? NewCompletionDate { get; set; }
    public decimal CurrentMonthlySavings { get; set; }
    public decimal NewMonthlySavings { get; set; }
    public decimal RemainingAmount { get; set; }
}
