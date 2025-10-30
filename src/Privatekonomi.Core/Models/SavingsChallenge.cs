namespace Privatekonomi.Core.Models;

public class SavingsChallenge
{
    public int SavingsChallengeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ChallengeType Type { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public int DurationDays { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ChallengeStatus Status { get; set; } = ChallengeStatus.Active;
    public int CurrentStreak { get; set; } = 0;
    public int BestStreak { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    // Navigation property for progress tracking
    public ICollection<SavingsChallengeProgress> ProgressEntries { get; set; } = new List<SavingsChallengeProgress>();
    
    // Calculated properties
    public int DaysCompleted => ProgressEntries.Count(p => p.Completed);
    public decimal ProgressPercentage => DurationDays > 0 ? (decimal)DaysCompleted / DurationDays * 100 : 0;
    public bool IsCompleted => Status == ChallengeStatus.Completed;
    public int RemainingDays 
    { 
        get 
        {
            if (EndDate.HasValue)
            {
                var remaining = (EndDate.Value - DateTime.UtcNow).Days;
                return remaining > 0 ? remaining : 0;
            }
            return DurationDays - DaysCompleted;
        }
    }
}

public enum ChallengeType
{
    SaveDaily,          // Save X kr/day
    NoRestaurant,       // No restaurant spending
    NoTakeaway,         // No takeaway spending
    NoCoffeeOut,        // No coffee at cafes
    SavePercentOfIncome, // Save X% of income
    SpendingLimit,      // Limit spending in a category
    Custom              // User-defined challenge
}

public enum ChallengeStatus
{
    Active,
    Completed,
    Failed,
    Paused
}
