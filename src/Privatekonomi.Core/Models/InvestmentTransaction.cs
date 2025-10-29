namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a buy or sell transaction for an investment
/// </summary>
public class InvestmentTransaction
{
    public int InvestmentTransactionId { get; set; }
    
    /// <summary>
    /// Reference to the investment
    /// </summary>
    public int InvestmentId { get; set; }
    public Investment? Investment { get; set; }
    
    /// <summary>
    /// Type of transaction: "Köp" (Buy), "Sälj" (Sell)
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;
    
    /// <summary>
    /// Date of the transaction
    /// </summary>
    public DateTime TransactionDate { get; set; }
    
    /// <summary>
    /// Number of shares/units bought or sold
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Price per share/unit
    /// </summary>
    public decimal PricePerShare { get; set; }
    
    /// <summary>
    /// Total transaction amount (Quantity * PricePerShare)
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Transaction fees/commission
    /// </summary>
    public decimal? Fees { get; set; }
    
    /// <summary>
    /// Currency of the transaction
    /// </summary>
    public string Currency { get; set; } = "SEK";
    
    /// <summary>
    /// Exchange rate if currency conversion was involved
    /// </summary>
    public decimal? ExchangeRate { get; set; }
    
    /// <summary>
    /// Notes about the transaction
    /// </summary>
    public string? Notes { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    // Calculated property
    public decimal TotalCost => TotalAmount + (Fees ?? 0);
}
