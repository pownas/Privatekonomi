namespace Privatekonomi.Core.Models;

public class Pocket : ITemporalEntity
{
    public int PocketId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public int BankSourceId { get; set; }
    public int Priority { get; set; } = 3; // 1 (highest) to 5 (lowest)
    public decimal MonthlyAllocation { get; set; } = 0; // Automatic monthly contribution
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Temporal tracking
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    // Navigation properties
    public BankSource? BankSource { get; set; }
    public ICollection<PocketTransaction>? PocketTransactions { get; set; }
    
    // Computed properties
    public decimal ProgressPercentage => TargetAmount > 0 ? (CurrentAmount / TargetAmount) * 100 : 0;
    public decimal RemainingAmount => TargetAmount - CurrentAmount;
}
