namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a single entry in an amortization schedule
/// </summary>
public class AmortizationScheduleEntry
{
    public int PaymentNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal BeginningBalance { get; set; }
    public decimal Payment { get; set; }
    public decimal Principal { get; set; }
    public decimal Interest { get; set; }
    public decimal ExtraPayment { get; set; }
    public decimal EndingBalance { get; set; }
    public decimal TotalInterestPaid { get; set; }
}
