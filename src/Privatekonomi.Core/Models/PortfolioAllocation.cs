namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents target portfolio allocation settings
/// </summary>
public class PortfolioAllocation
{
    public int PortfolioAllocationId { get; set; }
    
    /// <summary>
    /// Name of the allocation strategy
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Asset class: "Aktier", "Obligationer", "Fastigheter", "Råvaror", "Kontanter", etc.
    /// </summary>
    public string AssetClass { get; set; } = string.Empty;
    
    /// <summary>
    /// Target allocation percentage (0-100)
    /// </summary>
    public decimal TargetPercentage { get; set; }
    
    /// <summary>
    /// Minimum acceptable percentage (for rebalancing)
    /// </summary>
    public decimal? MinPercentage { get; set; }
    
    /// <summary>
    /// Maximum acceptable percentage (for rebalancing)
    /// </summary>
    public decimal? MaxPercentage { get; set; }
    
    /// <summary>
    /// Whether this allocation is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Notes about the allocation strategy
    /// </summary>
    public string? Notes { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
