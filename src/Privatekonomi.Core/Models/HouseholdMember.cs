namespace Privatekonomi.Core.Models;

public class HouseholdMember
{
    public int HouseholdMemberId { get; set; }
    public int HouseholdId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime JoinedDate { get; set; }
    public DateTime? LeftDate { get; set; }
    
    public Household? Household { get; set; }
    public ICollection<ExpenseShare> ExpenseShares { get; set; } = new List<ExpenseShare>();
}
