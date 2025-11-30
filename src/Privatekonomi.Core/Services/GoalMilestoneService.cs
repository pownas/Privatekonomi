using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for managing goal milestones.
/// </summary>
public class GoalMilestoneService : IGoalMilestoneService
{
    private readonly PrivatekonomyContext _context;
    private readonly INotificationService? _notificationService;
    private readonly ICurrentUserService? _currentUserService;

    public GoalMilestoneService(
        PrivatekonomyContext context,
        INotificationService? notificationService = null,
        ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<GoalMilestone>> GetMilestonesByGoalIdAsync(int goalId)
    {
        return await _context.GoalMilestones
            .Where(m => m.GoalId == goalId)
            .OrderBy(m => m.Percentage)
            .ToListAsync();
    }

    public async Task CreateAutomaticMilestonesAsync(int goalId)
    {
        var goal = await _context.Goals.FindAsync(goalId);
        if (goal == null)
        {
            throw new ArgumentException($"Goal with ID {goalId} not found.");
        }

        // Check if automatic milestones already exist
        var existingMilestones = await _context.GoalMilestones
            .Where(m => m.GoalId == goalId && m.IsAutomatic)
            .ToListAsync();

        if (existingMilestones.Any())
        {
            return; // Automatic milestones already created
        }

        var percentages = new[] { 25, 50, 75, 100 };
        var milestones = new List<GoalMilestone>();

        foreach (var percentage in percentages)
        {
            var targetAmount = goal.TargetAmount * percentage / 100;
            var milestone = new GoalMilestone
            {
                GoalId = goalId,
                TargetAmount = targetAmount,
                Percentage = percentage,
                Description = $"{percentage}% av målet uppnått",
                IsReached = goal.CurrentAmount >= targetAmount,
                ReachedAt = goal.CurrentAmount >= targetAmount ? DateTime.UtcNow : null,
                IsAutomatic = true,
                CreatedAt = DateTime.UtcNow
            };
            milestones.Add(milestone);
        }

        _context.GoalMilestones.AddRange(milestones);
        await _context.SaveChangesAsync();
    }

    public async Task<GoalMilestone> CreateCustomMilestoneAsync(GoalMilestone milestone)
    {
        milestone.IsAutomatic = false;
        milestone.CreatedAt = DateTime.UtcNow;
        milestone.IsReached = false;

        var goal = await _context.Goals.FindAsync(milestone.GoalId);
        if (goal != null && goal.CurrentAmount >= milestone.TargetAmount)
        {
            milestone.IsReached = true;
            milestone.ReachedAt = DateTime.UtcNow;
        }

        _context.GoalMilestones.Add(milestone);
        await _context.SaveChangesAsync();
        return milestone;
    }

    public async Task<IEnumerable<GoalMilestone>> CheckAndUpdateMilestonesAsync(int goalId, decimal currentAmount)
    {
        var milestones = await _context.GoalMilestones
            .Where(m => m.GoalId == goalId && !m.IsReached && m.TargetAmount <= currentAmount)
            .ToListAsync();

        var newlyReachedMilestones = new List<GoalMilestone>();

        foreach (var milestone in milestones)
        {
            milestone.IsReached = true;
            milestone.ReachedAt = DateTime.UtcNow;
            newlyReachedMilestones.Add(milestone);

            // Send notification if notification service is available
            if (_notificationService != null && _currentUserService?.UserId != null)
            {
                var goal = await _context.Goals.FindAsync(goalId);
                if (goal != null)
                {
                    var title = milestone.IsAutomatic 
                        ? $"🎉 {milestone.Percentage}% av målet uppnått!"
                        : "🎉 Milstolpe uppnådd!";
                    
                    var message = milestone.IsAutomatic 
                        ? $"Du har nått {milestone.Percentage}% av ditt sparmål '{goal.Name}'! Bra jobbat!"
                        : $"Du har nått en milstolpe i ditt sparmål '{goal.Name}': {milestone.Description}";

                    await _notificationService.SendNotificationAsync(
                        _currentUserService.UserId,
                        SystemNotificationType.GoalMilestone,
                        title,
                        message,
                        NotificationPriority.Normal,
                        null,
                        $"/goals?goalId={goalId}"
                    );
                }
            }
        }

        if (newlyReachedMilestones.Any())
        {
            await _context.SaveChangesAsync();
        }

        return newlyReachedMilestones;
    }

    public async Task<IEnumerable<GoalMilestone>> GetReachedMilestonesAsync(int goalId)
    {
        return await _context.GoalMilestones
            .Where(m => m.GoalId == goalId && m.IsReached)
            .OrderBy(m => m.ReachedAt)
            .ToListAsync();
    }

    public async Task DeleteMilestoneAsync(int milestoneId)
    {
        var milestone = await _context.GoalMilestones.FindAsync(milestoneId);
        if (milestone != null)
        {
            _context.GoalMilestones.Remove(milestone);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<GoalMilestone?> GetMilestoneByIdAsync(int milestoneId)
    {
        return await _context.GoalMilestones.FindAsync(milestoneId);
    }
}
