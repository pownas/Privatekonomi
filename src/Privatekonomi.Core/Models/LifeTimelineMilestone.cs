namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a significant life milestone in the financial timeline
/// </summary>
public class LifeTimelineMilestone
{
    public int LifeTimelineMilestoneId { get; set; }
    
    /// <summary>
    /// Name of the milestone (e.g., "Köpa bostad", "Barn", "Pension")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the milestone
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Type of milestone: "HousePurchase", "Child", "Retirement", "Education", "Career", "Other"
    /// </summary>
    public string MilestoneType { get; set; } = string.Empty;
    
    /// <summary>
    /// Expected or planned date for the milestone
    /// </summary>
    public DateTime PlannedDate { get; set; }
    
    /// <summary>
    /// Estimated cost or financial impact (negative for costs, positive for income)
    /// </summary>
    public decimal EstimatedCost { get; set; }
    
    /// <summary>
    /// Monthly savings required to reach this milestone
    /// </summary>
    public decimal? RequiredMonthlySavings { get; set; }
    
    /// <summary>
    /// Current progress towards this milestone (0-100)
    /// </summary>
    public decimal ProgressPercentage { get; set; }
    
    /// <summary>
    /// Amount already saved for this milestone
    /// </summary>
    public decimal CurrentSavings { get; set; }
    
    /// <summary>
    /// Priority level: 1 (highest) to 5 (lowest)
    /// </summary>
    public int Priority { get; set; } = 3;
    
    /// <summary>
    /// Whether this milestone has been completed
    /// </summary>
    public bool IsCompleted { get; set; }
    
    /// <summary>
    /// Notes about the milestone
    /// </summary>
    public string? Notes { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
