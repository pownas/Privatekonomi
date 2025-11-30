using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service interface for managing goal milestones.
/// </summary>
public interface IGoalMilestoneService
{
    /// <summary>
    /// Gets all milestones for a specific goal.
    /// </summary>
    Task<IEnumerable<GoalMilestone>> GetMilestonesByGoalIdAsync(int goalId);
    
    /// <summary>
    /// Creates automatic milestones (25%, 50%, 75%, 100%) for a goal.
    /// </summary>
    Task CreateAutomaticMilestonesAsync(int goalId);
    
    /// <summary>
    /// Creates a custom milestone for a goal.
    /// </summary>
    Task<GoalMilestone> CreateCustomMilestoneAsync(GoalMilestone milestone);
    
    /// <summary>
    /// Checks if any milestones have been reached based on current progress and marks them.
    /// Returns newly reached milestones.
    /// </summary>
    Task<IEnumerable<GoalMilestone>> CheckAndUpdateMilestonesAsync(int goalId, decimal currentAmount);
    
    /// <summary>
    /// Gets all reached milestones for a goal (historical tracking).
    /// </summary>
    Task<IEnumerable<GoalMilestone>> GetReachedMilestonesAsync(int goalId);
    
    /// <summary>
    /// Deletes a milestone.
    /// </summary>
    Task DeleteMilestoneAsync(int milestoneId);
    
    /// <summary>
    /// Gets a specific milestone by ID.
    /// </summary>
    Task<GoalMilestone?> GetMilestoneByIdAsync(int milestoneId);
}
