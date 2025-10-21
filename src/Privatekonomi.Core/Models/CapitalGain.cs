namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a capital gain/loss from selling securities
/// Used for K4 tax form (Blankett för kapitalinkomster)
/// </summary>
public class CapitalGain
{
    public int CapitalGainId { get; set; }
    
    /// <summary>
    /// Link to the investment that was sold
    /// </summary>
    public int InvestmentId { get; set; }
    
    /// <summary>
    /// Date when the security was sold
    /// </summary>
    public DateTime SaleDate { get; set; }
    
    /// <summary>
    /// Number of shares/units sold
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Purchase price per share/unit
    /// </summary>
    public decimal PurchasePricePerUnit { get; set; }
    
    /// <summary>
    /// Total purchase price
    /// </summary>
    public decimal TotalPurchasePrice { get; set; }
    
    /// <summary>
    /// Sale price per share/unit
    /// </summary>
    public decimal SalePricePerUnit { get; set; }
    
    /// <summary>
    /// Total sale price
    /// </summary>
    public decimal TotalSalePrice { get; set; }
    
    /// <summary>
    /// Capital gain/loss (positive = gain, negative = loss)
    /// </summary>
    public decimal Gain { get; set; }
    
    /// <summary>
    /// Type of security: "Stock", "Fund", "Crypto", "Bond", "Option"
    /// </summary>
    public string SecurityType { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the security
    /// </summary>
    public string SecurityName { get; set; } = string.Empty;
    
    /// <summary>
    /// ISIN code (if applicable)
    /// </summary>
    public string? ISIN { get; set; }
    
    /// <summary>
    /// Tax year this gain applies to
    /// </summary>
    public int TaxYear { get; set; }
    
    /// <summary>
    /// Whether this was sold from an ISK account (different tax rules)
    /// </summary>
    public bool IsISK { get; set; }
    
    /// <summary>
    /// Currency of the transaction
    /// </summary>
    public string Currency { get; set; } = "SEK";
    
    /// <summary>
    /// Exchange rate if currency is not SEK
    /// </summary>
    public decimal? ExchangeRate { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public Investment? Investment { get; set; }
}
