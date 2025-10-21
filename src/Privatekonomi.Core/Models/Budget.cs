namespace Privatekonomi.Core.Models;

public class Budget
{
    public int BudgetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public BudgetPeriod Period { get; set; }
    public int? HouseholdId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    public Household? Household { get; set; }
    public ICollection<BudgetCategory> BudgetCategories { get; set; } = new List<BudgetCategory>();
}

public enum BudgetPeriod
{
    Monthly,
    Yearly
}
