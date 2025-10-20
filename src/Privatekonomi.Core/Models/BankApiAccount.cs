namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a bank account from a bank API
/// </summary>
public class BankApiAccount
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? Iban { get; set; }
    public string? AccountNumber { get; set; }
    public string? ClearingNumber { get; set; }
    public string Currency { get; set; } = "SEK";
    public decimal? Balance { get; set; }
    public string AccountType { get; set; } = "checking"; // checking, savings, credit_card, investment
}
