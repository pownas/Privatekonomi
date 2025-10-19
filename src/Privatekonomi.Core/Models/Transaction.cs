namespace Privatekonomi.Core.Models;

public class Transaction
{
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool IsIncome { get; set; }
    public int? BankSourceId { get; set; }
    
    // Additional fields from OpenAPI spec
    public string? Payee { get; set; }
    public string Currency { get; set; } = "SEK";
    public List<string> Tags { get; set; } = new();
    public bool Imported { get; set; }
    
    public BankSource? BankSource { get; set; }
    public ICollection<TransactionCategory> TransactionCategories { get; set; } = new List<TransactionCategory>();
}
