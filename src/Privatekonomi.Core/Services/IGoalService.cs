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
}
