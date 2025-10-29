namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a dividend payment from an investment
/// </summary>
public class Dividend
{
    public int DividendId { get; set; }
    
    /// <summary>
    /// Reference to the investment that paid the dividend
    /// </summary>
    public int InvestmentId { get; set; }
    public Investment? Investment { get; set; }
    
    /// <summary>
    /// Date when the dividend was paid
    /// </summary>
    public DateTime PaymentDate { get; set; }
    
    /// <summary>
    /// Ex-dividend date (last day to own the stock to receive dividend)
    /// </summary>
    public DateTime? ExDividendDate { get; set; }
    
    /// <summary>
    /// Amount per share/unit
    /// </summary>
    public decimal AmountPerShare { get; set; }
    
    /// <summary>
    /// Total dividend amount received
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Number of shares held at the time of dividend
    /// </summary>
    public decimal SharesHeld { get; set; }
    
    /// <summary>
    /// Currency of the dividend payment
    /// </summary>
    public string Currency { get; set; } = "SEK";
    
    /// <summary>
    /// Tax withheld on the dividend (if any)
    /// </summary>
    public decimal? TaxWithheld { get; set; }
    
    /// <summary>
    /// Whether the dividend was reinvested (DRIP)
    /// </summary>
    public bool IsReinvested { get; set; }
    
    /// <summary>
    /// Number of shares purchased through reinvestment
    /// </summary>
    public decimal? ReinvestedShares { get; set; }
    
    /// <summary>
    /// Price per share at reinvestment
    /// </summary>
    public decimal? ReinvestmentPrice { get; set; }
    
    /// <summary>
    /// Notes about the dividend payment
    /// </summary>
    public string? Notes { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
