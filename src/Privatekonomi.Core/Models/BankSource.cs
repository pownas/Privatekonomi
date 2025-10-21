namespace Privatekonomi.Core.Models;

public class BankSource
{
    public int BankSourceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000";
    public string? Logo { get; set; }
    
    // Enhanced fields from proposed Account model
    public string AccountType { get; set; } = "checking"; // checking, savings, credit_card, investment, cash
    public string Currency { get; set; } = "SEK"; // Account currency
    public string? Institution { get; set; } // Bank institution name
    public decimal InitialBalance { get; set; } // Starting balance when account was opened
    public DateTime? OpenedDate { get; set; } // When account was opened
    public DateTime? ClosedDate { get; set; } // When account was closed (null if active)
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    
    // Computed property for current balance (not stored in DB)
    public decimal CurrentBalance => InitialBalance + Transactions.Sum(t => t.IsIncome ? t.Amount : -t.Amount);
}
