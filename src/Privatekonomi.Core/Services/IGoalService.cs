using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IGoalService
{
    Task<IEnumerable<Goal>> GetAllGoalsAsync();
    Task<Goal?> GetGoalByIdAsync(int id);
    Task<Goal> CreateGoalAsync(Goal goal);
    Task<Goal> UpdateGoalAsync(Goal goal);
    Task DeleteGoalAsync(int id);
    Task<Goal> UpdateGoalProgressAsync(int id, decimal currentAmount);
    Task<IEnumerable<Goal>> GetGoalsByPriorityAsync(int priority);
}
