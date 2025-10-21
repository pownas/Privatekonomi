namespace Privatekonomi.Core.Models;

public class ChildAllowance
{
    public int ChildAllowanceId { get; set; }
    public int HouseholdMemberId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AllowanceFrequency Frequency { get; set; }
    public decimal Amount { get; set; }
    public decimal CurrentBalance { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public HouseholdMember? HouseholdMember { get; set; }
    public ICollection<AllowanceTransaction> AllowanceTransactions { get; set; } = new List<AllowanceTransaction>();
    public ICollection<AllowanceTask> AllowanceTasks { get; set; } = new List<AllowanceTask>();
}

public enum AllowanceFrequency
{
    Weekly,     // Varje vecka
    BiWeekly,   // Varannan vecka
    Monthly     // Månatligen
}
