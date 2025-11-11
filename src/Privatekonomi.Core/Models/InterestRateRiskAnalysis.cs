namespace Privatekonomi.Core.Models;

/// <summary>
/// Analysis of interest rate risk for a mortgage loan
/// </summary>
public class InterestRateRiskAnalysis
{
    /// <summary>
    /// The loan being analyzed
    /// </summary>
    public required Loan Loan { get; set; }
    
    /// <summary>
    /// Current interest rate
    /// </summary>
    public decimal CurrentInterestRate { get; set; }
    
    /// <summary>
    /// Current monthly cost
    /// </summary>
    public decimal CurrentMonthlyCost { get; set; }
    
    /// <summary>
    /// Whether the loan has fixed interest rate
    /// </summary>
    public bool IsFixedRate { get; set; }
    
    /// <summary>
    /// Date when fixed rate period ends (if applicable)
    /// </summary>
    public DateTime? RateResetDate { get; set; }
    
    /// <summary>
    /// Months until rate reset (if applicable)
    /// </summary>
    public int? MonthsUntilRateReset { get; set; }
    
    /// <summary>
    /// Different interest rate scenarios
    /// </summary>
    public required List<InterestRateScenario> Scenarios { get; set; }
    
    /// <summary>
    /// Risk level assessment
    /// </summary>
    public RiskLevel RiskLevel { get; set; }
    
    /// <summary>
    /// Risk description
    /// </summary>
    public required string RiskDescription { get; set; }
}

/// <summary>
/// Interest rate scenario analysis
/// </summary>
public class InterestRateScenario
{
    /// <summary>
    /// Scenario name (e.g., "+1%", "+2%", "+3%")
    /// </summary>
    public required string ScenarioName { get; set; }
    
    /// <summary>
    /// Interest rate in this scenario
    /// </summary>
    public decimal InterestRate { get; set; }
    
    /// <summary>
    /// Monthly cost in this scenario
    /// </summary>
    public decimal MonthlyCost { get; set; }
    
    /// <summary>
    /// Increase in monthly cost compared to current
    /// </summary>
    public decimal MonthlyIncrease { get; set; }
    
    /// <summary>
    /// Annual increase in cost
    /// </summary>
    public decimal AnnualIncrease { get; set; }
}

/// <summary>
/// Risk level assessment
/// </summary>
public enum RiskLevel
{
    /// <summary>
    /// Low risk (long fixed rate period or low LTV)
    /// </summary>
    Low,
    
    /// <summary>
    /// Medium risk (moderate fixed rate period or moderate LTV)
    /// </summary>
    Medium,
    
    /// <summary>
    /// High risk (variable rate or short fixed period with high LTV)
    /// </summary>
    High
}
