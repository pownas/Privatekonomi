namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a cash flow scenario for financial projection analysis
/// </summary>
public class CashFlowScenario
{
    /// <summary>
    /// Current savings amount (starting capital)
    /// </summary>
    public decimal InitialAmount { get; set; }
    
    /// <summary>
    /// Monthly savings contribution
    /// </summary>
    public decimal MonthlySavings { get; set; }
    
    /// <summary>
    /// Annual interest rate (as percentage, e.g., 5.0 for 5%)
    /// </summary>
    public decimal AnnualInterestRate { get; set; }
    
    /// <summary>
    /// Number of years to project
    /// </summary>
    public int Years { get; set; }
    
    /// <summary>
    /// Optional: Annual inflation rate (as percentage)
    /// </summary>
    public decimal? InflationRate { get; set; }
    
    /// <summary>
    /// Optional: One-time extra contribution
    /// </summary>
    public decimal? ExtraContribution { get; set; }
    
    /// <summary>
    /// Optional: Month when extra contribution is made (1-12)
    /// </summary>
    public int? ExtraContributionMonth { get; set; }
    
    /// <summary>
    /// Optional: Annual increase in monthly savings (as percentage)
    /// </summary>
    public decimal? AnnualSavingsIncrease { get; set; }
    
    /// <summary>
    /// Scenario name for comparison purposes
    /// </summary>
    public string Name { get; set; } = "Scenario";
}
