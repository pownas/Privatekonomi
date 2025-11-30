namespace Privatekonomi.Core.Models;

/// <summary>
/// Breakdown of monthly mortgage cost
/// </summary>
public class MonthlyCostBreakdown
{
    /// <summary>
    /// Principal amount (loan balance)
    /// </summary>
    public decimal Principal { get; set; }
    
    /// <summary>
    /// Monthly interest payment
    /// </summary>
    public decimal MonthlyInterest { get; set; }
    
    /// <summary>
    /// Monthly amortization
    /// </summary>
    public decimal MonthlyAmortization { get; set; }
    
    /// <summary>
    /// Total monthly payment (interest + amortization)
    /// </summary>
    public decimal TotalMonthlyPayment { get; set; }
    
    /// <summary>
    /// Interest rate used
    /// </summary>
    public decimal InterestRate { get; set; }
    
    /// <summary>
    /// Annual interest cost
    /// </summary>
    public decimal AnnualInterestCost { get; set; }
    
    /// <summary>
    /// Annual amortization
    /// </summary>
    public decimal AnnualAmortization { get; set; }
    
    /// <summary>
    /// Total annual payment
    /// </summary>
    public decimal TotalAnnualPayment { get; set; }
}
