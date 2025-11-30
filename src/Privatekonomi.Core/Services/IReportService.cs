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
    
    /// <summary>
    /// Generate a monthly report for a specific month
    /// </summary>
    Task<MonthlyReportData> GenerateMonthlyReportAsync(int year, int month, string? userId = null, int? householdId = null);
    
    /// <summary>
    /// Get a stored monthly report by month
    /// </summary>
    Task<MonthlyReport?> GetMonthlyReportAsync(int year, int month, string? userId = null, int? householdId = null);
    
    /// <summary>
    /// Get all monthly reports for a user
    /// </summary>
    Task<IEnumerable<MonthlyReport>> GetMonthlyReportsAsync(string? userId = null, int? householdId = null, int limit = 12);
    
    /// <summary>
    /// Save a generated monthly report to the database
    /// </summary>
    Task<MonthlyReport> SaveMonthlyReportAsync(MonthlyReportData reportData, string? userId = null, int? householdId = null);
    
    /// <summary>
    /// Get report preferences for a user
    /// </summary>
    Task<ReportPreference> GetReportPreferencesAsync(string userId);
    
    /// <summary>
    /// Save report preferences for a user
    /// </summary>
    Task<ReportPreference> SaveReportPreferencesAsync(ReportPreference preferences);

    /// <summary>
    /// Get a historical overview of economic state at a specific date.
    /// Used for "time travel" feature to view past economic status.
    /// </summary>
    /// <param name="asOfDate">The date to retrieve historical data for</param>
    /// <param name="userId">Optional user ID filter</param>
    /// <returns>Historical overview report for the specified date</returns>
    Task<HistoricalOverviewReport> GetHistoricalOverviewAsync(DateTime asOfDate, string? userId = null);

    /// <summary>
    /// Get key dates where economic data changed significantly.
    /// Used for timeline navigation suggestions.
    /// </summary>
    /// <param name="userId">Optional user ID filter</param>
    /// <param name="limit">Maximum number of dates to return</param>
    /// <returns>List of significant dates with brief descriptions</returns>
    Task<List<TimelineKeyDate>> GetTimelineKeyDatesAsync(string? userId = null, int limit = 12);

    /// <summary>
    /// Get the date of the user's earliest financial activity (first transaction).
    /// Used to show "your journey started" information.
    /// </summary>
    /// <param name="userId">Optional user ID filter</param>
    /// <returns>The journey start info with earliest transaction date and total days tracked</returns>
    Task<JourneyStartInfo?> GetJourneyStartInfoAsync(string? userId = null);
}

/// <summary>
/// Information about when the user's financial journey started
/// </summary>
public class JourneyStartInfo
{
    /// <summary>
    /// Date of the earliest recorded transaction
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Number of days since the journey started
    /// </summary>
    public int DaysTracked { get; set; }
    
    /// <summary>
    /// Total number of transactions recorded
    /// </summary>
    public int TotalTransactions { get; set; }
}

/// <summary>
/// Key date for timeline navigation
/// </summary>
public class TimelineKeyDate
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty; // "NetWorthPeak", "NetWorthLow", "MajorPurchase", "MonthEnd"
    public decimal? NetWorth { get; set; }
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

/// <summary>
/// Complete monthly report data for a specific month
/// </summary>
public class MonthlyReportData
{
    /// <summary>
    /// Year of the report
    /// </summary>
    public int Year { get; set; }
    
    /// <summary>
    /// Month of the report (1-12)
    /// </summary>
    public int Month { get; set; }
    
    /// <summary>
    /// Swedish name of the month
    /// </summary>
    public string MonthName { get; set; } = string.Empty;
    
    /// <summary>
    /// Total income for the month
    /// </summary>
    public decimal TotalIncome { get; set; }
    
    /// <summary>
    /// Total expenses for the month
    /// </summary>
    public decimal TotalExpenses { get; set; }
    
    /// <summary>
    /// Net flow (income - expenses)
    /// </summary>
    public decimal NetFlow { get; set; }
    
    /// <summary>
    /// Savings rate as percentage (net flow / income * 100)
    /// </summary>
    public decimal SavingsRate { get; set; }
    
    /// <summary>
    /// Comparison with previous month
    /// </summary>
    public MonthlyComparison? PreviousMonthComparison { get; set; }
    
    /// <summary>
    /// Breakdown of spending by category
    /// </summary>
    public List<ReportCategorySummary> CategorySummaries { get; set; } = new();
    
    /// <summary>
    /// Top merchants/payees by spending
    /// </summary>
    public List<TopMerchant> TopMerchants { get; set; } = new();
    
    /// <summary>
    /// Budget performance for the month
    /// </summary>
    public List<BudgetOutcome> BudgetOutcomes { get; set; } = new();
    
    /// <summary>
    /// Key insights and recommendations
    /// </summary>
    public List<ReportInsight> Insights { get; set; } = new();
    
    /// <summary>
    /// Number of transactions in the month
    /// </summary>
    public int TransactionCount { get; set; }
    
    /// <summary>
    /// When the report was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Comparison data between two months
/// </summary>
public class MonthlyComparison
{
    public decimal IncomeChange { get; set; }
    public decimal IncomeChangePercent { get; set; }
    public decimal ExpenseChange { get; set; }
    public decimal ExpenseChangePercent { get; set; }
    public decimal NetFlowChange { get; set; }
    public decimal NetFlowChangePercent { get; set; }
    public string TrendDirection { get; set; } = string.Empty; // "Improving", "Worsening", "Stable"
}

/// <summary>
/// Category summary for the monthly report
/// </summary>
public class ReportCategorySummary
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#000000";
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public int TransactionCount { get; set; }
    public decimal? PreviousMonthAmount { get; set; }
    public decimal? ChangePercent { get; set; }
}

/// <summary>
/// Budget performance for a category in the monthly report
/// </summary>
public class BudgetOutcome
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal BudgetedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Difference { get; set; }
    public decimal PercentageUsed { get; set; }
    public string Status { get; set; } = string.Empty; // "UnderBudget", "OnTrack", "OverBudget"
}

/// <summary>
/// An insight or recommendation in the monthly report
/// </summary>
public class ReportInsight
{
    public string Type { get; set; } = string.Empty; // "Positive", "Warning", "Info", "Action"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public decimal? Amount { get; set; }
}
