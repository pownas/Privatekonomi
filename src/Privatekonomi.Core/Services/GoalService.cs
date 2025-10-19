using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class GoalService : IGoalService
{
    private readonly PrivatekonomyContext _context;

    public GoalService(PrivatekonomyContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Goal>> GetAllGoalsAsync()
    {
        return await _context.Goals
            .OrderBy(g => g.TargetDate)
            .ToListAsync();
    }

    public async Task<Goal?> GetGoalByIdAsync(int id)
    {
        return await _context.Goals.FindAsync(id);
    }

    public async Task<Goal> CreateGoalAsync(Goal goal)
    {
        goal.CreatedDate = DateTime.Now;
        goal.Status = GoalStatus.Active;
        
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();
        return goal;
    }

    public async Task<Goal> UpdateGoalAsync(Goal goal)
    {
        _context.Entry(goal).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return goal;
    }

    public async Task DeleteGoalAsync(int id)
    {
        var goal = await _context.Goals.FindAsync(id);
        if (goal != null)
        {
            _context.Goals.Remove(goal);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Goal>> GetActiveGoalsAsync()
    {
        return await _context.Goals
            .Where(g => g.Status == GoalStatus.Active)
            .OrderBy(g => g.TargetDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalProgress()
    {
        var activeGoals = await _context.Goals
            .Where(g => g.Status == GoalStatus.Active)
            .ToListAsync();

        if (!activeGoals.Any())
            return 0;

        var totalTarget = activeGoals.Sum(g => g.TargetAmount);
        var totalCurrent = activeGoals.Sum(g => g.CurrentAmount);

        return totalTarget > 0 ? (totalCurrent / totalTarget) * 100 : 0;
    }
}
