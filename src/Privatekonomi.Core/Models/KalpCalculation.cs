namespace Privatekonomi.Core.Models;

/// <summary>
/// KALP (Kvar att leva på) calculation result
/// Shows how much disposable income is left after fixed expenses and loan payments
/// </summary>
public class KalpCalculation
{
    /// <summary>
    /// Total monthly income
    /// </summary>
    public decimal TotalIncome { get; set; }

    /// <summary>
    /// Fixed monthly expenses (rent, utilities, insurance, subscriptions, etc.)
    /// </summary>
    public decimal FixedExpenses { get; set; }

    /// <summary>
    /// Total monthly loan payments (amortization + interest)
    /// </summary>
    public decimal LoanPayments { get; set; }

    /// <summary>
    /// KALP amount = Income - (Fixed expenses + Loan payments)
    /// This is the amount left to live on for variable expenses
    /// </summary>
    public decimal KalpAmount => TotalIncome - (FixedExpenses + LoanPayments);

    /// <summary>
    /// KALP as percentage of income
    /// </summary>
    public decimal KalpPercentage => TotalIncome > 0 ? (KalpAmount / TotalIncome) * 100 : 0;

    /// <summary>
    /// Breakdown of fixed expenses by category
    /// </summary>
    public Dictionary<string, decimal> FixedExpenseBreakdown { get; set; } = new();

    /// <summary>
    /// Breakdown of loan payments by loan type
    /// </summary>
    public Dictionary<string, decimal> LoanPaymentBreakdown { get; set; } = new();

    /// <summary>
    /// Recommended minimum KALP based on Konsumentverket guidelines
    /// </summary>
    public decimal? RecommendedMinimumKalp { get; set; }

    /// <summary>
    /// Whether current KALP meets recommended minimum
    /// </summary>
    public bool MeetsRecommendedMinimum => RecommendedMinimumKalp.HasValue && KalpAmount >= RecommendedMinimumKalp.Value;
}

/// <summary>
/// User input for KALP calculation
/// </summary>
public class KalpInput
{
    /// <summary>
    /// Total monthly income (after tax)
    /// </summary>
    public decimal MonthlyIncome { get; set; }

    /// <summary>
    /// Fixed monthly expenses
    /// </summary>
    public Dictionary<string, decimal> FixedExpenses { get; set; } = new();

    /// <summary>
    /// List of loans with their monthly payments
    /// </summary>
    public List<LoanPayment> Loans { get; set; } = new();

    /// <summary>
    /// Optional: Household members for calculating recommended minimum
    /// </summary>
    public List<KonsumentverketHouseholdMember>? HouseholdMembers { get; set; }
}

/// <summary>
/// Represents a loan payment for KALP calculation
/// </summary>
public class LoanPayment
{
    public string LoanName { get; set; } = string.Empty;
    public string LoanType { get; set; } = string.Empty;
    public decimal MonthlyPayment { get; set; }
}

/// <summary>
/// Comparison between user's KALP and budget
/// </summary>
public class KalpBudgetComparison
{
    /// <summary>
    /// User's KALP calculation
    /// </summary>
    public KalpCalculation KalpCalculation { get; set; } = new();

    /// <summary>
    /// User's budget information
    /// </summary>
    public Budget? Budget { get; set; }

    /// <summary>
    /// Total budgeted variable expenses
    /// </summary>
    public decimal BudgetedVariableExpenses { get; set; }

    /// <summary>
    /// Difference between KALP and budgeted variable expenses
    /// Positive = surplus, Negative = deficit
    /// </summary>
    public decimal Difference => KalpCalculation.KalpAmount - BudgetedVariableExpenses;

    /// <summary>
    /// Whether budget is sustainable (KALP covers budgeted expenses)
    /// </summary>
    public bool IsSustainable => Difference >= 0;

    /// <summary>
    /// Comparison with Konsumentverket reference if available
    /// </summary>
    public KonsumentverketComparisonResult? KonsumentverketComparison { get; set; }
}
