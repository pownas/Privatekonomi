namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents analysis of extra payment impact
/// </summary>
public class ExtraPaymentAnalysis
{
    public decimal ExtraMonthlyPayment { get; set; }
    public DateTime OriginalPayoffDate { get; set; }
    public DateTime NewPayoffDate { get; set; }
    public int MonthsSaved { get; set; }
    public decimal OriginalTotalInterest { get; set; }
    public decimal NewTotalInterest { get; set; }
    public decimal InterestSavings { get; set; }
    public decimal TotalExtraPayments { get; set; }
    public decimal NetSavings { get; set; } // Interest savings minus extra payments
}
