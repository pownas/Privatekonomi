namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a household member's contribution share to a shared budget
/// </summary>
public class HouseholdBudgetShare
{
    public int HouseholdBudgetShareId { get; set; }
    public int BudgetId { get; set; }
    public int HouseholdMemberId { get; set; }
    public decimal SharePercentage { get; set; }
    public decimal? FixedContribution { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public Budget? Budget { get; set; }
    public HouseholdMember? HouseholdMember { get; set; }
}
