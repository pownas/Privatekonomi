namespace Privatekonomi.Core.Models;

public class Transaction
{
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool IsIncome { get; set; }
    public int? BankSourceId { get; set; }
    
    // Enhanced fields from proposed model
    public string Currency { get; set; } = "SEK"; // Default to Swedish Krona
    public string? Payee { get; set; } // Who received/sent the payment
    public string? Tags { get; set; } // Comma-separated tags for flexible categorization
    public string? Notes { get; set; } // User notes/comments about the transaction
    public int? RecurringId { get; set; } // Link to recurring transaction template
    public bool Imported { get; set; } // Whether transaction was imported from CSV/API
    public string? ImportSource { get; set; } // Source of import (e.g., "ICA-banken CSV", "Swedbank API")
    public bool Cleared { get; set; } // Whether transaction has been reconciled/cleared
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public BankSource? BankSource { get; set; }
    public ICollection<TransactionCategory> TransactionCategories { get; set; } = new List<TransactionCategory>();
}
