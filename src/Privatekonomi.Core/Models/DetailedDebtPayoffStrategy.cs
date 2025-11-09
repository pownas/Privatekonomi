namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a detailed debt payoff strategy with month-by-month amortization schedules
/// </summary>
public class DetailedDebtPayoffStrategy
{
    public string StrategyName { get; set; } = string.Empty; // "Snöboll", "Lavin"
    public string Description { get; set; } = string.Empty;
    public DateTime DebtFreeDate { get; set; }
    public decimal TotalInterestPaid { get; set; }
    public decimal TotalAmountPaid { get; set; }
    public int TotalMonths { get; set; }
    
    /// <summary>
    /// Monthly payment schedule showing all loan payments for each month
    /// </summary>
    public List<MonthlyStrategyPayment> MonthlySchedule { get; set; } = new();
    
    /// <summary>
    /// Summary for each loan showing payoff order and totals
    /// </summary>
    public List<DetailedLoanSummary> LoanSummaries { get; set; } = new();
}

/// <summary>
/// Represents the payment breakdown for all loans in a specific month
/// </summary>
public class MonthlyStrategyPayment
{
    public int MonthNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal TotalPrincipal { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal RemainingTotalDebt { get; set; }
    
    /// <summary>
    /// Individual loan payments for this month
    /// </summary>
    public List<LoanPaymentDetail> LoanPayments { get; set; } = new();
}

/// <summary>
/// Represents a single loan's payment in a specific month
/// </summary>
public class LoanPaymentDetail
{
    public int LoanId { get; set; }
    public string LoanName { get; set; } = string.Empty;
    public decimal BeginningBalance { get; set; }
    public decimal Payment { get; set; }
    public decimal Principal { get; set; }
    public decimal Interest { get; set; }
    public decimal EndingBalance { get; set; }
    public bool IsFocusLoan { get; set; } // True if this is the loan receiving extra payments
    public bool IsPaidOff { get; set; } // True if loan was paid off this month
}

/// <summary>
/// Summary information for a specific loan within a strategy
/// </summary>
public class DetailedLoanSummary
{
    public int LoanId { get; set; }
    public string LoanName { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int PayoffOrder { get; set; }
    public DateTime PayoffDate { get; set; }
    public int MonthsToPayoff { get; set; }
    public decimal TotalInterestPaid { get; set; }
    public decimal TotalAmountPaid { get; set; }
}
