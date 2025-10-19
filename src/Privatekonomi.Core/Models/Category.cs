namespace Privatekonomi.Core.Models;

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000";
    
    // Additional fields from OpenAPI spec
    public int? ParentId { get; set; }
    public decimal? DefaultBudgetMonthly { get; set; }
    
    public Category? Parent { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<TransactionCategory> TransactionCategories { get; set; } = new List<TransactionCategory>();
}
