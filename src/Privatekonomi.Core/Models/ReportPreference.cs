namespace Privatekonomi.Core.Models;

/// <summary>
/// User preferences for monthly report delivery
/// </summary>
public class ReportPreference
{
    public int ReportPreferenceId { get; set; }
    
    /// <summary>
    /// Whether to send reports via email
    /// </summary>
    public bool SendEmail { get; set; }
    
    /// <summary>
    /// Whether to show reports in the app
    /// </summary>
    public bool ShowInApp { get; set; } = true;
    
    /// <summary>
    /// Email address for report delivery (if different from user's primary email)
    /// </summary>
    public string? EmailAddress { get; set; }
    
    /// <summary>
    /// Preferred day of month to receive report (1-28, 0 = first day of following month)
    /// </summary>
    public int PreferredDeliveryDay { get; set; } = 1;
    
    /// <summary>
    /// Whether to include detailed category breakdown
    /// </summary>
    public bool IncludeCategoryDetails { get; set; } = true;
    
    /// <summary>
    /// Whether to include top merchants
    /// </summary>
    public bool IncludeTopMerchants { get; set; } = true;
    
    /// <summary>
    /// Whether to include budget comparison
    /// </summary>
    public bool IncludeBudgetComparison { get; set; } = true;
    
    /// <summary>
    /// Whether to include trend analysis
    /// </summary>
    public bool IncludeTrendAnalysis { get; set; } = true;
    
    /// <summary>
    /// Whether monthly report generation is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
