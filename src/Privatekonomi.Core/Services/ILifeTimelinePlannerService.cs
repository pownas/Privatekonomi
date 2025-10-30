using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for managing life timeline planning and scenarios
/// </summary>
public interface ILifeTimelinePlannerService
{
    // Milestone management
    Task<IEnumerable<LifeTimelineMilestone>> GetAllMilestonesAsync();
    Task<LifeTimelineMilestone?> GetMilestoneByIdAsync(int id);
    Task<LifeTimelineMilestone> CreateMilestoneAsync(LifeTimelineMilestone milestone);
    Task<LifeTimelineMilestone> UpdateMilestoneAsync(LifeTimelineMilestone milestone);
    Task DeleteMilestoneAsync(int id);
    
    // Scenario management
    Task<IEnumerable<LifeTimelineScenario>> GetAllScenariosAsync();
    Task<LifeTimelineScenario?> GetScenarioByIdAsync(int id);
    Task<LifeTimelineScenario> CreateScenarioAsync(LifeTimelineScenario scenario);
    Task<LifeTimelineScenario> UpdateScenarioAsync(LifeTimelineScenario scenario);
    Task DeleteScenarioAsync(int id);
    Task<LifeTimelineScenario?> GetActiveScenarioAsync();
    Task SetActiveScenarioAsync(int scenarioId);
    
    // Planning and calculations
    Task<decimal> CalculateRequiredMonthlySavingsAsync(int milestoneId);
    Task<decimal> CalculateProjectedRetirementWealthAsync(int scenarioId);
    Task<Dictionary<string, decimal>> GetMilestonesByTypeAsync();
    Task<int> GetYearsToRetirementAsync();
    Task<decimal> GetTotalMilestoneCostsAsync();
}
