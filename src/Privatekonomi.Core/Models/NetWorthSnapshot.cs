namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a snapshot of net worth at a specific point in time.
/// Used for tracking net worth history and visualizing trends.
/// </summary>
public class NetWorthSnapshot
{
    public int NetWorthSnapshotId { get; set; }
    
    /// <summary>
    /// Date when this snapshot was taken
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Total assets at this point in time
    /// </summary>
    public decimal TotalAssets { get; set; }
    
    /// <summary>
    /// Total liabilities at this point in time
    /// </summary>
    public decimal TotalLiabilities { get; set; }
    
    /// <summary>
    /// Net worth calculated as TotalAssets - TotalLiabilities
    /// </summary>
    public decimal NetWorth { get; set; }
    
    /// <summary>
    /// Breakdown of asset values
    /// </summary>
    public decimal? BankBalance { get; set; }
    public decimal? InvestmentValue { get; set; }
    public decimal? PhysicalAssetValue { get; set; }
    
    /// <summary>
    /// Breakdown of liability values
    /// </summary>
    public decimal? LoanBalance { get; set; }
    
    /// <summary>
    /// Whether this snapshot was manually created or automatically calculated
    /// </summary>
    public bool IsManual { get; set; }
    
    /// <summary>
    /// Optional notes about this snapshot
    /// </summary>
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
