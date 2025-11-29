namespace Privatekonomi.Core.Models;

/// <summary>
/// Aggregated dashboard data for ekonomisk översikt.
/// Collects information from accounts, budgets, bills, and insights.
/// </summary>
public class DashboardData
{
    /// <summary>
    /// Total balance across all selected accounts
    /// </summary>
    public BalanceSummary Balance { get; set; } = new();
    
    /// <summary>
    /// Current month's budget status
    /// </summary>
    public BudgetStatusSummary BudgetStatus { get; set; } = new();
    
    /// <summary>
    /// List of upcoming bills
    /// </summary>
    public List<BillSummary> UpcomingBills { get; set; } = new();
    
    /// <summary>
    /// Recent insights and alerts
    /// </summary>
    public List<InsightItem> Insights { get; set; } = new();
    
    /// <summary>
    /// When the dashboard data was last updated
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Summary of account balances
/// </summary>
public class BalanceSummary
{
    /// <summary>
    /// Total balance across all accounts
    /// </summary>
    public decimal TotalBalance { get; set; }
    
    /// <summary>
    /// Change from previous period (month)
    /// </summary>
    public decimal ChangeFromPrevious { get; set; }
    
    /// <summary>
    /// Percentage change from previous period
    /// </summary>
    public decimal PercentageChange { get; set; }
    
    /// <summary>
    /// Currency for the total balance
    /// </summary>
    public string Currency { get; set; } = "SEK";
    
    /// <summary>
    /// Individual account balances
    /// </summary>
    public List<AccountBalanceItem> Accounts { get; set; } = new();
}

/// <summary>
/// Individual account balance details
/// </summary>
public class AccountBalanceItem
{
    public int AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "SEK";
    public string? Color { get; set; }
}

/// <summary>
/// Summary of current budget status for the month
/// </summary>
public class BudgetStatusSummary
{
    /// <summary>
    /// Whether there are any active budgets
    /// </summary>
    public bool HasActiveBudgets { get; set; }
    
    /// <summary>
    /// Total planned amount for all active budgets
    /// </summary>
    public decimal TotalPlanned { get; set; }
    
    /// <summary>
    /// Total actual spent amount
    /// </summary>
    public decimal TotalSpent { get; set; }
    
    /// <summary>
    /// Remaining amount (TotalPlanned - TotalSpent)
    /// </summary>
    public decimal Remaining { get; set; }
    
    /// <summary>
    /// Percentage of budget used
    /// </summary>
    public decimal PercentageUsed { get; set; }
    
    /// <summary>
    /// Overall budget status
    /// </summary>
    public BudgetHealthStatus Status { get; set; }
    
    /// <summary>
    /// List of individual budget summaries
    /// </summary>
    public List<BudgetItemSummary> Budgets { get; set; } = new();
    
    /// <summary>
    /// Categories that have exceeded their budget
    /// </summary>
    public List<string> OverspentCategories { get; set; } = new();
}

/// <summary>
/// Individual budget item summary
/// </summary>
public class BudgetItemSummary
{
    public int BudgetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Planned { get; set; }
    public decimal Spent { get; set; }
    public decimal PercentageUsed { get; set; }
    public BudgetHealthStatus Status { get; set; }
}

/// <summary>
/// Budget health status indicator
/// </summary>
public enum BudgetHealthStatus
{
    /// <summary>
    /// Budget is on track (less than 75% used)
    /// </summary>
    OnTrack,
    
    /// <summary>
    /// Budget is nearing limit (75-99% used)
    /// </summary>
    Warning,
    
    /// <summary>
    /// Budget is exceeded (100%+ used)
    /// </summary>
    Exceeded
}

/// <summary>
/// Summary of an upcoming bill
/// </summary>
public class BillSummary
{
    public int BillId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SEK";
    public DateTime DueDate { get; set; }
    public int DaysUntilDue { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
    public string? Payee { get; set; }
    public string? CategoryName { get; set; }
}

/// <summary>
/// Individual insight or alert item
/// </summary>
public class InsightItem
{
    public int InsightId { get; set; }
    public InsightType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InsightPriority Priority { get; set; }
    public string? ActionUrl { get; set; }
    public string? IconName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string? Data { get; set; }
}

/// <summary>
/// Types of insights
/// </summary>
public enum InsightType
{
    BudgetAlert,
    SpendingTrend,
    SavingsOpportunity,
    BillReminder,
    GoalProgress,
    UnusualTransaction,
    SubscriptionAlert,
    SystemNotification
}

/// <summary>
/// Insight priority levels
/// </summary>
public enum InsightPriority
{
    Low,
    Medium,
    High,
    Critical
}
