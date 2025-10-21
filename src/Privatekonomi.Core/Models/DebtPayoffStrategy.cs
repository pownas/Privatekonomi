namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a debt payoff strategy analysis
/// </summary>
public class DebtPayoffStrategy
{
    public string StrategyName { get; set; } = string.Empty; // "Snöboll", "Lavin", "Custom"
    public string Description { get; set; } = string.Empty;
    public List<DebtPayoffPlan> PayoffPlans { get; set; } = new();
    public DateTime DebtFreeDate { get; set; }
    public decimal TotalInterestPaid { get; set; }
    public decimal TotalAmountPaid { get; set; }
    public int TotalMonths { get; set; }
    public decimal MonthlySavings { get; set; } // Compared to minimum payments only
}

/// <summary>
/// Represents the payoff plan for a specific loan within a strategy
/// </summary>
public class DebtPayoffPlan
{
    public int LoanId { get; set; }
    public string LoanName { get; set; } = string.Empty;
    public int PayoffOrder { get; set; } // Order in which this loan will be paid off
    public DateTime PayoffDate { get; set; }
    public decimal TotalInterestPaid { get; set; }
    public int MonthsToPayoff { get; set; }
}
