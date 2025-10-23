namespace Privatekonomi.Core.Models;

public class PocketTransaction
{
    public int PocketTransactionId { get; set; }
    public int PocketId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // "Deposit", "Withdrawal", "Transfer"
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // For transfers between pockets
    public int? RelatedPocketId { get; set; }
    
    // User who made the transaction
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    // Navigation properties
    public Pocket? Pocket { get; set; }
    public Pocket? RelatedPocket { get; set; }
}
