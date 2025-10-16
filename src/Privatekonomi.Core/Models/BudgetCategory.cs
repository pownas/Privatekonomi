namespace Privatekonomi.Core.Models;

public class BudgetCategory
{
    public int BudgetCategoryId { get; set; }
    public int BudgetId { get; set; }
    public int CategoryId { get; set; }
    public decimal PlannedAmount { get; set; }
    
    public Budget Budget { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
