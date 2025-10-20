namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a transaction fetched from a bank API
/// </summary>
public class BankApiTransaction
{
    public string TransactionId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime? BookingDate { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SEK";
    public string Description { get; set; } = string.Empty;
    public string? Creditor { get; set; }
    public string? Debtor { get; set; }
    public string? Reference { get; set; }
    public bool IsIncome { get; set; }
    
    /// <summary>
    /// External account ID this transaction belongs to
    /// </summary>
    public string? AccountId { get; set; }
}
