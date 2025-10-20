namespace Privatekonomi.Core.Models;

public class Goal
{
    public int GoalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime TargetDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public GoalStatus Status { get; set; }
    public string Color { get; set; } = "#2196F3";
    public int Priority { get; set; } = 3; // 1 (highest) to 5 (lowest)
    public int? FundedFromBankSourceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public BankSource? FundedFromBankSource { get; set; }
}

public enum GoalStatus
{
    Active,
    Completed,
    Cancelled
}
