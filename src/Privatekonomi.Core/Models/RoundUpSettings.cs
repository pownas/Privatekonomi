namespace Privatekonomi.Core.Models;

/// <summary>
/// User settings for Round-up automatic savings feature
/// </summary>
public class RoundUpSettings
{
    public int RoundUpSettingsId { get; set; }
    
    /// <summary>
    /// Whether Round-up savings is enabled for this user
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// Round-up amount in SEK (default: 10)
    /// Transactions will be rounded up to the nearest multiple of this amount
    /// </summary>
    public decimal RoundUpAmount { get; set; } = 10M;
    
    /// <summary>
    /// Goal ID where round-up savings should be deposited
    /// </summary>
    public int? TargetGoalId { get; set; }
    
    /// <summary>
    /// Whether employer matching is enabled (doubles the savings)
    /// </summary>
    public bool EnableEmployerMatching { get; set; }
    
    /// <summary>
    /// Percentage of employer matching (default: 100% = doubles the amount)
    /// </summary>
    public decimal EmployerMatchingPercentage { get; set; } = 100M;
    
    /// <summary>
    /// Maximum employer matching amount per month in SEK
    /// </summary>
    public decimal? EmployerMatchingMonthlyLimit { get; set; }
    
    /// <summary>
    /// Whether salary-based auto-save is enabled
    /// </summary>
    public bool EnableSalaryAutoSave { get; set; }
    
    /// <summary>
    /// Percentage of salary/income to save automatically (default: 10%)
    /// </summary>
    public decimal SalaryAutoSavePercentage { get; set; } = 10M;
    
    /// <summary>
    /// Only apply round-up to transactions above this amount (to exclude small transactions)
    /// </summary>
    public decimal? MinimumTransactionAmount { get; set; }
    
    /// <summary>
    /// Only apply round-up to transactions below this amount (to exclude large transactions)
    /// </summary>
    public decimal? MaximumTransactionAmount { get; set; }
    
    /// <summary>
    /// Whether to apply round-up only to expense transactions (not income)
    /// </summary>
    public bool OnlyExpenses { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    // Navigation property
    public Goal? TargetGoal { get; set; }
}
