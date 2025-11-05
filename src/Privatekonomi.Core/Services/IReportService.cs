using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IReportService
{
    /// <summary>
    /// Get cash flow data for a date range
    /// </summary>
    Task<CashFlowReport> GetCashFlowReportAsync(DateTime fromDate, DateTime toDate, string groupBy = "month", int? householdId = null);
    
    /// <summary>
    /// Get net worth calculation (assets - liabilities) with historical trend data
    /// </summary>
    Task<NetWorthReport> GetNetWorthReportAsync(string? userId = null);
    
    /// <summary>
    /// Get trend analysis for a specific category
    /// </summary>
    Task<TrendAnalysis> GetTrendAnalysisAsync(int? categoryId, int months = 6, int? householdId = null);
    
    /// <summary>
    /// Get top merchants/payees by spending
    /// </summary>
    Task<IEnumerable<TopMerchant>> GetTopMerchantsAsync(int limit = 10, DateTime? fromDate = null, DateTime? toDate = null, int? householdId = null);
    
    /// <summary>
    /// Get net worth history over time
    /// </summary>
    Task<NetWorthHistoryReport> GetNetWorthHistoryAsync(string groupBy = "month", DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Create a snapshot of current net worth
    /// </summary>
    Task<NetWorthSnapshot> CreateNetWorthSnapshotAsync(bool isManual = false, string? notes = null);
    
    /// <summary>
    /// Get period comparison for dashboard (current month vs previous month, year-over-year)
    /// </summary>
    Task<PeriodComparisonReport> GetPeriodComparisonAsync(DateTime? referenceDate = null, int? householdId = null);
    
    /// <summary>
    /// Get economic health score (0-100) based on multiple financial factors
    /// </summary>
    Task<HealthScoreReport> GetHealthScoreAsync(int? householdId = null);
    
    /// <summary>
    /// Get spending pattern analysis report with category distribution, trends, anomalies and recommendations
    /// </summary>
    Task<SpendingPatternReport> GetSpendingPatternReportAsync(DateTime fromDate, DateTime toDate, int? householdId = null);
}

public class CashFlowReport
{
    public List<CashFlowPeriod> Periods { get; set; } = new();
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetCashFlow { get; set; }
    public string GroupBy { get; set; } = "month";
}

public class CashFlowPeriod
{
    public string Period { get; set; } = string.Empty; // e.g., "2025-01" or "2025-W01"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal NetFlow { get; set; }
}

public class NetWorthReport
{
    public decimal TotalAssets { get; set; }
    public decimal TotalInvestments { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal NetWorth { get; set; }
    public decimal PercentageChange { get; set; }
    public List<AssetItem> Assets { get; set; } = new();
    public List<LiabilityItem> Liabilities { get; set; } = new();
    public List<NetWorthDataPoint> History { get; set; } = new();
    public DateTime CalculatedAt { get; set; }
}

public class NetWorthDataPoint
{
    public DateTime Date { get; set; }
    public decimal Assets { get; set; }
    public decimal Liabilities { get; set; }
    public decimal NetWorth { get; set; }
}

public class AssetItem
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Investment, Asset, Cash
    public decimal Value { get; set; }
}

public class LiabilityItem
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Loan, Credit
    public decimal Amount { get; set; }
}

public class TrendAnalysis
{
    public string CategoryName { get; set; } = string.Empty;
    public List<MonthlyTrend> MonthlyTrends { get; set; } = new();
    public decimal AverageMonthly { get; set; }
    public decimal Trend { get; set; } // Positive = increasing, Negative = decreasing
    public string TrendDirection { get; set; } = string.Empty; // "Increasing", "Decreasing", "Stable"
}

public class MonthlyTrend
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class TopMerchant
{
    public string Name { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageTransaction { get; set; }
}

public class NetWorthHistoryReport
{
    public List<NetWorthHistoryPeriod> Periods { get; set; } = new();
    public decimal CurrentNetWorth { get; set; }
    public decimal StartNetWorth { get; set; }
    public decimal NetWorthChange { get; set; }
    public decimal NetWorthChangePercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string GroupBy { get; set; } = "month";
}

public class NetWorthHistoryPeriod
{
    public string Period { get; set; } = string.Empty; // e.g., "2025-01"
    public DateTime Date { get; set; }
    public decimal NetWorth { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal? BankBalance { get; set; }
    public decimal? InvestmentValue { get; set; }
    public decimal? PhysicalAssetValue { get; set; }
    public decimal? LoanBalance { get; set; }
}

public class PeriodComparisonReport
{
    public PeriodComparison Income { get; set; } = new();
    public PeriodComparison Expenses { get; set; } = new();
    public PeriodComparison NetFlow { get; set; } = new();
    public List<double> SparklineData { get; set; } = new(); // Last 6 months trend for expenses
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime PreviousPeriodStart { get; set; }
    public DateTime PreviousPeriodEnd { get; set; }
    public DateTime YearAgoPeriodStart { get; set; }
    public DateTime YearAgoPeriodEnd { get; set; }
}

public class PeriodComparison
{
    public decimal CurrentPeriod { get; set; }
    public decimal PreviousPeriod { get; set; }
    public decimal YearAgoPeriod { get; set; }
    public decimal ChangeFromPrevious { get; set; }
    public decimal ChangeFromYearAgo { get; set; }
    public decimal PercentageChangeFromPrevious { get; set; }
    public decimal PercentageChangeFromYearAgo { get; set; }
    public string TrendDirection { get; set; } = string.Empty; // "Improving", "Worsening", "Stable"
}

public class HealthScoreReport
{
    public int TotalScore { get; set; } // 0-100
    public string HealthLevel { get; set; } = string.Empty; // "Excellent", "Good", "Fair", "Poor"
    public HealthScoreComponent SavingsRate { get; set; } = new();
    public HealthScoreComponent DebtLevel { get; set; } = new();
    public HealthScoreComponent EmergencyFund { get; set; } = new();
    public HealthScoreComponent BudgetAdherence { get; set; } = new();
    public HealthScoreComponent InvestmentDiversification { get; set; } = new();
    public HealthScoreComponent IncomeStability { get; set; } = new();
    public List<string> Strengths { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
    public List<HealthScoreHistoryPoint> History { get; set; } = new();
    public DateTime CalculatedAt { get; set; }
}

public class HealthScoreComponent
{
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; } // Actual score achieved
    public int MaxScore { get; set; } // Maximum possible score
    public decimal? Value { get; set; } // The actual metric value (e.g., 15% savings rate)
    public string Unit { get; set; } = string.Empty; // e.g., "%", "månader", "kr"
    public string Status { get; set; } = string.Empty; // "Excellent", "Good", "Fair", "Poor"
    public string Description { get; set; } = string.Empty;
}

public class HealthScoreHistoryPoint
{
    public DateTime Date { get; set; }
    public int Score { get; set; }
}

/// <summary>
/// Comprehensive spending pattern analysis report
/// </summary>
public class SpendingPatternReport
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalSpending { get; set; }
    public decimal AverageMonthlySpending { get; set; }
    public List<CategorySpending> CategoryDistribution { get; set; } = new();
    public List<CategorySpending> TopCategories { get; set; } = new();
    public List<SpendingTrend> Trends { get; set; } = new();
    public List<SpendingAnomaly> Anomalies { get; set; } = new();
    public List<SpendingRecommendation> Recommendations { get; set; } = new();
    public List<MonthlySpendingData> MonthlyData { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Spending data for a specific category
/// </summary>
public class CategorySpending
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#000000";
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageTransactionAmount { get; set; }
    public decimal? PreviousPeriodAmount { get; set; }
    public decimal? ChangeFromPreviousPeriod { get; set; }
    public decimal? ChangePercentage { get; set; }
}

/// <summary>
/// Spending trend analysis for a category or overall spending
/// </summary>
public class SpendingTrend
{
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string TrendType { get; set; } = string.Empty; // "Increasing", "Decreasing", "Stable"
    public decimal TrendPercentage { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<TrendDataPoint> DataPoints { get; set; } = new();
}

/// <summary>
/// Data point for trend visualization
/// </summary>
public class TrendDataPoint
{
    public DateTime Date { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

/// <summary>
/// Detected spending anomaly (unusual patterns)
/// </summary>
public class SpendingAnomaly
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty; // "UnusuallyHigh", "UnusuallyLow", "FrequencyChange"
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal Deviation { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Actionable recommendation based on spending patterns
/// </summary>
public class SpendingRecommendation
{
    public string Type { get; set; } = string.Empty; // "SavingsOpportunity", "BudgetAlert", "TrendWarning"
    public string Priority { get; set; } = string.Empty; // "High", "Medium", "Low"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? PotentialSavings { get; set; }
    public string? CategoryName { get; set; }
}

/// <summary>
/// Monthly spending summary data
/// </summary>
public class MonthlySpendingData
{
    public string Month { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public Dictionary<string, decimal> CategoryBreakdown { get; set; } = new();
}
