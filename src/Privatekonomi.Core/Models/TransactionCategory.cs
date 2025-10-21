namespace Privatekonomi.Core.Models;

public class TransactionCategory
{
    public int TransactionCategoryId { get; set; }
    public int TransactionId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; } = 100; // Percentage of transaction assigned to this category
    
    public Transaction Transaction { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
