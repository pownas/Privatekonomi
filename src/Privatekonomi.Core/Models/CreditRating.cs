namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a credit rating check from UC or similar credit bureau
/// </summary>
public class CreditRating
{
    public int CreditRatingId { get; set; }
    
    /// <summary>
    /// Link to household
    /// </summary>
    public int HouseholdId { get; set; }
    
    /// <summary>
    /// Credit rating provider: "UC", "Creditsafe", "Bisnode"
    /// </summary>
    public string Provider { get; set; } = string.Empty;
    
    /// <summary>
    /// Credit rating (e.g., "AAA", "AA", "A", "B", "C")
    /// </summary>
    public string Rating { get; set; } = string.Empty;
    
    /// <summary>
    /// Numeric credit score (if available)
    /// </summary>
    public int? Score { get; set; }
    
    /// <summary>
    /// Date when credit rating was checked
    /// </summary>
    public DateTime CheckedDate { get; set; }
    
    /// <summary>
    /// Number of payment remarks (betalningsanmärkningar)
    /// </summary>
    public int PaymentRemarks { get; set; }
    
    /// <summary>
    /// Total debt according to credit report
    /// </summary>
    public decimal? TotalDebt { get; set; }
    
    /// <summary>
    /// Credit limit
    /// </summary>
    public decimal? CreditLimit { get; set; }
    
    /// <summary>
    /// Credit utilization percentage
    /// </summary>
    public decimal? CreditUtilization { get; set; }
    
    /// <summary>
    /// Notes about the credit rating
    /// </summary>
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public Household? Household { get; set; }
}
