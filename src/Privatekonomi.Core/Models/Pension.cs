namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a pension account or pension savings
/// </summary>
public class Pension
{
    public int PensionId { get; set; }
    
    /// <summary>
    /// Name of the pension account or provider
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of pension: "Tjänstepension", "Privat pension", "Allmän pension (AP7)", etc.
    /// </summary>
    public string PensionType { get; set; } = string.Empty;
    
    /// <summary>
    /// Pension provider: "AMF", "Alecta", "SEB", "Nordea", "Folksam", etc.
    /// </summary>
    public string? Provider { get; set; }
    
    /// <summary>
    /// Current value of the pension account
    /// </summary>
    public decimal CurrentValue { get; set; }
    
    /// <summary>
    /// Total contributions made to this pension
    /// </summary>
    public decimal TotalContributions { get; set; }
    
    /// <summary>
    /// Monthly contribution amount (if applicable)
    /// </summary>
    public decimal? MonthlyContribution { get; set; }
    
    /// <summary>
    /// Expected monthly pension payment at retirement
    /// </summary>
    public decimal? ExpectedMonthlyPension { get; set; }
    
    /// <summary>
    /// Expected retirement age
    /// </summary>
    public int? RetirementAge { get; set; }
    
    /// <summary>
    /// Date when the account was opened
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Last time the pension value was updated
    /// </summary>
    public DateTime LastUpdated { get; set; }
    
    /// <summary>
    /// Account number or policy number
    /// </summary>
    public string? AccountNumber { get; set; }
    
    /// <summary>
    /// Notes about the pension account
    /// </summary>
    public string? Notes { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    // Calculated properties
    public decimal TotalReturn => CurrentValue - TotalContributions;
    public decimal ReturnPercentage => TotalContributions > 0 ? (TotalReturn / TotalContributions) * 100 : 0;
}
