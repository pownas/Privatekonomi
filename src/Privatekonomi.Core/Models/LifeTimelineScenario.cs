namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a "what-if" scenario for financial planning
/// </summary>
public class LifeTimelineScenario
{
    public int LifeTimelineScenarioId { get; set; }
    
    /// <summary>
    /// Name of the scenario (e.g., "Optimistisk", "Pessimistisk", "Realistisk")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the scenario
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Expected annual return rate (percentage)
    /// </summary>
    public decimal ExpectedReturnRate { get; set; }
    
    /// <summary>
    /// Monthly savings amount in this scenario
    /// </summary>
    public decimal MonthlySavings { get; set; }
    
    /// <summary>
    /// Expected retirement age in this scenario
    /// </summary>
    public int RetirementAge { get; set; } = 65;
    
    /// <summary>
    /// Expected monthly pension in this scenario
    /// </summary>
    public decimal ExpectedMonthlyPension { get; set; }
    
    /// <summary>
    /// Projected total wealth at retirement
    /// </summary>
    public decimal ProjectedRetirementWealth { get; set; }
    
    /// <summary>
    /// Inflation rate assumption (percentage)
    /// </summary>
    public decimal InflationRate { get; set; } = 2.0m;
    
    /// <summary>
    /// Expected salary increase rate (percentage)
    /// </summary>
    public decimal SalaryIncreaseRate { get; set; } = 2.5m;
    
    /// <summary>
    /// Whether this is the active/selected scenario
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Whether this is a baseline scenario
    /// </summary>
    public bool IsBaseline { get; set; }
    
    /// <summary>
    /// Notes about the scenario assumptions
    /// </summary>
    public string? Notes { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
