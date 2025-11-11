namespace Privatekonomi.Core.Models;

/// <summary>
/// Result of amortization requirement calculation according to Swedish mortgage regulations
/// </summary>
public class AmortizationRequirement
{
    /// <summary>
    /// The loan being analyzed
    /// </summary>
    public required Loan Loan { get; set; }
    
    /// <summary>
    /// Loan-to-Value ratio (belåningsgrad) in percent
    /// </summary>
    public decimal LoanToValueRatio { get; set; }
    
    /// <summary>
    /// Required minimum annual amortization in SEK according to Swedish rules
    /// </summary>
    public decimal RequiredAnnualAmortization { get; set; }
    
    /// <summary>
    /// Required minimum monthly amortization in SEK
    /// </summary>
    public decimal RequiredMonthlyAmortization { get; set; }
    
    /// <summary>
    /// Current monthly amortization in SEK
    /// </summary>
    public decimal CurrentMonthlyAmortization { get; set; }
    
    /// <summary>
    /// Whether the current amortization meets the requirement
    /// </summary>
    public bool MeetsRequirement { get; set; }
    
    /// <summary>
    /// Shortage in monthly amortization (if any)
    /// </summary>
    public decimal MonthlyShortage { get; set; }
    
    /// <summary>
    /// Description of the applicable rule
    /// </summary>
    public required string RuleDescription { get; set; }
    
    /// <summary>
    /// Which amortization rule applies
    /// </summary>
    public AmortizationRule ApplicableRule { get; set; }
    
    /// <summary>
    /// Years until loan is paid off at current amortization rate
    /// </summary>
    public decimal? YearsToPayoff { get; set; }
}

/// <summary>
/// Swedish amortization rules
/// </summary>
public enum AmortizationRule
{
    /// <summary>
    /// No amortization requirement (LTV ≤ 50%)
    /// </summary>
    NoRequirement,
    
    /// <summary>
    /// 1% annual amortization (50% &lt; LTV ≤ 70%)
    /// </summary>
    OnePercentAnnual,
    
    /// <summary>
    /// 2% annual amortization (LTV &gt; 70%)
    /// </summary>
    TwoPercentAnnual,
    
    /// <summary>
    /// Additional 1% if debt-to-income ratio exceeds 4.5 (skuldkvot)
    /// </summary>
    AdditionalDueToDebtRatio
}
