namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a milestone for a savings goal to track and celebrate progress.
/// Milestones can be automatic (25%, 50%, 75%, 100%) or custom.
/// </summary>
public class GoalMilestone
{
    public int GoalMilestoneId { get; set; }
    public int GoalId { get; set; }
    public decimal TargetAmount { get; set; }
    public int Percentage { get; set; } // 25, 50, 75, 100, or custom
    public string? Description { get; set; }
    public bool IsReached { get; set; }
    public DateTime? ReachedAt { get; set; }
    public bool IsAutomatic { get; set; } // true for 25%, 50%, 75%, 100%; false for custom
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public Goal? Goal { get; set; }
}
