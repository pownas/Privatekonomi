using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IReportService
{
    /// <summary>
    /// Get cash flow data for a date range
    /// </summary>
    Task<CashFlowReport> GetCashFlowReportAsync(DateTime fromDate, DateTime toDate, string groupBy = "month");
    
    /// <summary>
    /// Get net worth calculation (assets - liabilities)
    /// </summary>
    Task<NetWorthReport> GetNetWorthReportAsync();
    
    /// <summary>
    /// Get trend analysis for a specific category
    /// </summary>
    Task<TrendAnalysis> GetTrendAnalysisAsync(int? categoryId, int months = 6);
    
    /// <summary>
    /// Get top merchants/payees by spending
    /// </summary>
    Task<IEnumerable<TopMerchant>> GetTopMerchantsAsync(int limit = 10, DateTime? fromDate = null, DateTime? toDate = null);
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
    public decimal TotalLiabilities { get; set; }
    public decimal NetWorth { get; set; }
    public List<AssetItem> Assets { get; set; } = new();
    public List<LiabilityItem> Liabilities { get; set; } = new();
    public DateTime CalculatedAt { get; set; }
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
