namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a foreign currency account for tracking cash holdings in different currencies
/// </summary>
public class CurrencyAccount
{
    public int CurrencyAccountId { get; set; }
    
    /// <summary>
    /// Currency code (e.g., EUR, USD, GBP, JPY, CHF)
    /// </summary>
    public string Currency { get; set; } = string.Empty;
    
    /// <summary>
    /// Current balance in the foreign currency
    /// </summary>
    public decimal Balance { get; set; }
    
    /// <summary>
    /// Exchange rate to SEK (Swedish Krona)
    /// </summary>
    public decimal ExchangeRate { get; set; }
    
    /// <summary>
    /// Account number in the accounting system (e.g., 1921, 1922, 1923)
    /// </summary>
    public string? AccountNumber { get; set; }
    
    /// <summary>
    /// Optional description or notes about this currency account
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Date when the exchange rate was last updated
    /// </summary>
    public DateTime? ExchangeRateUpdated { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    // Computed property for value in SEK
    public decimal ValueInSEK => Balance * ExchangeRate;
}
