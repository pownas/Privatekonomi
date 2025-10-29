using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for managing life timeline planning and scenarios
/// </summary>
public class LifeTimelinePlannerService : ILifeTimelinePlannerService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;

    public LifeTimelinePlannerService(PrivatekonomyContext context, ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    #region Milestone Management

    public async Task<IEnumerable<LifeTimelineMilestone>> GetAllMilestonesAsync()
    {
        var query = _context.LifeTimelineMilestones.AsQueryable();

        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(m => m.UserId == _currentUserService.UserId);
        }

        return await query.OrderBy(m => m.PlannedDate).ToListAsync();
    }

    public async Task<LifeTimelineMilestone?> GetMilestoneByIdAsync(int id)
    {
        var query = _context.LifeTimelineMilestones.AsQueryable();

        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(m => m.UserId == _currentUserService.UserId);
        }

        return await query.FirstOrDefaultAsync(m => m.LifeTimelineMilestoneId == id);
    }

    public async Task<LifeTimelineMilestone> CreateMilestoneAsync(LifeTimelineMilestone milestone)
    {
        milestone.CreatedAt = DateTime.UtcNow;
        
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            milestone.UserId = _currentUserService.UserId;
        }
        
        _context.LifeTimelineMilestones.Add(milestone);
        await _context.SaveChangesAsync();
        return milestone;
    }

    public async Task<LifeTimelineMilestone> UpdateMilestoneAsync(LifeTimelineMilestone milestone)
    {
        milestone.UpdatedAt = DateTime.UtcNow;
        _context.LifeTimelineMilestones.Update(milestone);
        await _context.SaveChangesAsync();
        return milestone;
    }

    public async Task DeleteMilestoneAsync(int id)
    {
        var milestone = await _context.LifeTimelineMilestones.FindAsync(id);
        if (milestone != null)
        {
            _context.LifeTimelineMilestones.Remove(milestone);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Scenario Management

    public async Task<IEnumerable<LifeTimelineScenario>> GetAllScenariosAsync()
    {
        var query = _context.LifeTimelineScenarios.AsQueryable();

        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(s => s.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(s => s.IsActive).ThenBy(s => s.Name).ToListAsync();
    }

    public async Task<LifeTimelineScenario?> GetScenarioByIdAsync(int id)
    {
        var query = _context.LifeTimelineScenarios.AsQueryable();

        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(s => s.UserId == _currentUserService.UserId);
        }

        return await query.FirstOrDefaultAsync(s => s.LifeTimelineScenarioId == id);
    }

    public async Task<LifeTimelineScenario> CreateScenarioAsync(LifeTimelineScenario scenario)
    {
        scenario.CreatedAt = DateTime.UtcNow;
        
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            scenario.UserId = _currentUserService.UserId;
        }
        
        _context.LifeTimelineScenarios.Add(scenario);
        await _context.SaveChangesAsync();
        return scenario;
    }

    public async Task<LifeTimelineScenario> UpdateScenarioAsync(LifeTimelineScenario scenario)
    {
        scenario.UpdatedAt = DateTime.UtcNow;
        _context.LifeTimelineScenarios.Update(scenario);
        await _context.SaveChangesAsync();
        return scenario;
    }

    public async Task DeleteScenarioAsync(int id)
    {
        var scenario = await _context.LifeTimelineScenarios.FindAsync(id);
        if (scenario != null)
        {
            _context.LifeTimelineScenarios.Remove(scenario);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<LifeTimelineScenario?> GetActiveScenarioAsync()
    {
        var query = _context.LifeTimelineScenarios.AsQueryable();

        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(s => s.UserId == _currentUserService.UserId);
        }

        return await query.FirstOrDefaultAsync(s => s.IsActive);
    }

    public async Task SetActiveScenarioAsync(int scenarioId)
    {
        var query = _context.LifeTimelineScenarios.AsQueryable();

        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(s => s.UserId == _currentUserService.UserId);
        }

        var scenarios = await query.ToListAsync();
        
        foreach (var scenario in scenarios)
        {
            scenario.IsActive = scenario.LifeTimelineScenarioId == scenarioId;
            scenario.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Planning and Calculations

    public async Task<decimal> CalculateRequiredMonthlySavingsAsync(int milestoneId)
    {
        var milestone = await GetMilestoneByIdAsync(milestoneId);
        if (milestone == null) return 0;

        var monthsToGo = (decimal)((milestone.PlannedDate - DateTime.UtcNow).TotalDays / 30.44);
        if (monthsToGo <= 0) return 0;

        var remainingAmount = milestone.EstimatedCost - milestone.CurrentSavings;
        if (remainingAmount <= 0) return 0;

        return Math.Round(remainingAmount / monthsToGo, 2);
    }

    public async Task<decimal> CalculateProjectedRetirementWealthAsync(int scenarioId)
    {
        var scenario = await GetScenarioByIdAsync(scenarioId);
        if (scenario == null) return 0;

        var yearsToRetirement = await GetYearsToRetirementAsync();
        if (yearsToRetirement <= 0) return 0;

        // Simple compound interest calculation
        // Future Value = Monthly Savings × ((1 + r)^n - 1) / r
        // where r = monthly interest rate, n = number of months
        var monthlyRate = scenario.ExpectedReturnRate / 100 / 12;
        var months = yearsToRetirement * 12;

        if (monthlyRate == 0)
        {
            return scenario.MonthlySavings * months;
        }

        var futureValue = scenario.MonthlySavings * 
            ((decimal)Math.Pow((double)(1 + monthlyRate), months) - 1) / monthlyRate;

        return Math.Round(futureValue, 2);
    }

    public async Task<Dictionary<string, decimal>> GetMilestonesByTypeAsync()
    {
        var query = _context.LifeTimelineMilestones.AsQueryable();

        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(m => m.UserId == _currentUserService.UserId);
        }

        return await query
            .GroupBy(m => m.MilestoneType)
            .Select(g => new { Type = g.Key, Cost = g.Sum(m => m.EstimatedCost) })
            .ToDictionaryAsync(x => x.Type, x => x.Cost);
    }

    public async Task<int> GetYearsToRetirementAsync()
    {
        var activeScenario = await GetActiveScenarioAsync();
        if (activeScenario == null) return 35; // Default to 35 years if no scenario

        // Try to get user's birth date from salary history or default to 30 years old
        // In a real scenario, we would get this from user profile
        var currentAge = 30; // Default assumption
        var yearsToRetirement = activeScenario.RetirementAge - currentAge;
        
        // Return at least 1 year to avoid division issues
        return Math.Max(1, yearsToRetirement);
    }

    public async Task<decimal> GetTotalMilestoneCostsAsync()
    {
        var query = _context.LifeTimelineMilestones.AsQueryable();

        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(m => m.UserId == _currentUserService.UserId);
        }

        return await query.Where(m => !m.IsCompleted).SumAsync(m => m.EstimatedCost);
    }

    #endregion
}
