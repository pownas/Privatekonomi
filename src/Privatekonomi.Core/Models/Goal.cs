namespace Privatekonomi.Core.Models;

public class Goal : ITemporalEntity
{
    public int GoalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public int Priority { get; set; } = 3; // 1 (highest) to 5 (lowest)
    public int? FundedFromBankSourceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Temporal tracking
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    public BankSource? FundedFromBankSource { get; set; }
}
