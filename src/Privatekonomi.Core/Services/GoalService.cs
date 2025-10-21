using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class GoalService : IGoalService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;

    public GoalService(PrivatekonomyContext context, ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<Goal>> GetAllGoalsAsync()
    {
        var query = _context.Goals
            .Include(g => g.FundedFromBankSource)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(g => g.UserId == _currentUserService.UserId);
        }

        return await query.OrderBy(g => g.Priority).ThenBy(g => g.TargetDate).ToListAsync();
    }

    public async Task<Goal?> GetGoalByIdAsync(int id)
    {
        var query = _context.Goals
            .Include(g => g.FundedFromBankSource)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(g => g.UserId == _currentUserService.UserId);
        }

        return await query.FirstOrDefaultAsync(g => g.GoalId == id);
    }

    public async Task<Goal> CreateGoalAsync(Goal goal)
    {
        goal.CreatedAt = DateTime.UtcNow;
        
        // Set user ID for new goals
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            goal.UserId = _currentUserService.UserId;
        }
        
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

    public async Task<IEnumerable<Goal>> GetActiveGoalsAsync()
    {
        // Return goals that are not yet completed (current amount < target or target date in future)
        return await _context.Goals
            .Include(g => g.FundedFromBankSource)
            .Where(g => g.CurrentAmount < g.TargetAmount || (g.TargetDate.HasValue && g.TargetDate.Value > DateTime.UtcNow))
            .OrderBy(g => g.Priority)
            .ThenBy(g => g.TargetDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalProgress()
    {
        // Calculate progress for goals not yet completed
        var activeGoals = await _context.Goals
            .Where(g => g.CurrentAmount < g.TargetAmount)
            .ToListAsync();

        if (!activeGoals.Any())
            return 0;

        var totalTarget = activeGoals.Sum(g => g.TargetAmount);
        var totalCurrent = activeGoals.Sum(g => g.CurrentAmount);

        return totalTarget > 0 ? (totalCurrent / totalTarget) * 100 : 0;
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
