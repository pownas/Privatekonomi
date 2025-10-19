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
            .Include(g => g.FundedFromBankSource)
            .OrderBy(g => g.Priority)
            .ThenBy(g => g.TargetDate)
            .ToListAsync();
    }

    public async Task<Goal?> GetGoalByIdAsync(int id)
    {
        return await _context.Goals
            .Include(g => g.FundedFromBankSource)
            .FirstOrDefaultAsync(g => g.GoalId == id);
    }

    public async Task<Goal> CreateGoalAsync(Goal goal)
    {
        goal.CreatedAt = DateTime.UtcNow;
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();
        return goal;
    }

    public async Task<Goal> UpdateGoalAsync(Goal goal)
    {
        goal.UpdatedAt = DateTime.UtcNow;
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

    public async Task<Goal> UpdateGoalProgressAsync(int id, decimal currentAmount)
    {
        var goal = await _context.Goals.FindAsync(id);
        if (goal != null)
        {
            goal.CurrentAmount = currentAmount;
            goal.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return goal!;
    }

    public async Task<IEnumerable<Goal>> GetGoalsByPriorityAsync(int priority)
    {
        return await _context.Goals
            .Include(g => g.FundedFromBankSource)
            .Where(g => g.Priority == priority)
            .OrderBy(g => g.TargetDate)
            .ToListAsync();
    }
}
