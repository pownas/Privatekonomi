namespace Privatekonomi.Core.Models;

/// <summary>
/// Report representing the economic state at a specific historical date.
/// Used for the "time travel" feature to view past economic status.
/// </summary>
public class HistoricalOverviewReport
{
    /// <summary>
    /// The date for which this historical snapshot represents
    /// </summary>
    public DateTime AsOfDate { get; set; }

    /// <summary>
    /// Total net worth at the specified date (assets - liabilities)
    /// </summary>
    public decimal NetWorth { get; set; }

    /// <summary>
    /// Total assets value at the specified date
    /// </summary>
    public decimal TotalAssets { get; set; }

    /// <summary>
    /// Total liabilities at the specified date
    /// </summary>
    public decimal TotalLiabilities { get; set; }

    /// <summary>
    /// Total bank account balances at the specified date
    /// </summary>
    public decimal TotalBankBalance { get; set; }

    /// <summary>
    /// Total investment value at the specified date
    /// </summary>
    public decimal TotalInvestments { get; set; }

    /// <summary>
    /// Total physical assets value at the specified date
    /// </summary>
    public decimal TotalPhysicalAssets { get; set; }

    /// <summary>
    /// Total loan balance at the specified date
    /// </summary>
    public decimal TotalLoans { get; set; }

    /// <summary>
    /// Detailed breakdown of accounts at the specified date
    /// </summary>
    public List<HistoricalAccountItem> Accounts { get; set; } = new();

    /// <summary>
    /// Detailed breakdown of investments at the specified date
    /// </summary>
    public List<HistoricalInvestmentItem> Investments { get; set; } = new();

    /// <summary>
    /// Detailed breakdown of assets at the specified date
    /// </summary>
    public List<HistoricalAssetItem> Assets { get; set; } = new();

    /// <summary>
    /// Detailed breakdown of loans at the specified date
    /// </summary>
    public List<HistoricalLoanItem> Loans { get; set; } = new();

    /// <summary>
    /// Income for the month containing the specified date
    /// </summary>
    public decimal MonthlyIncome { get; set; }

    /// <summary>
    /// Expenses for the month containing the specified date
    /// </summary>
    public decimal MonthlyExpenses { get; set; }

    /// <summary>
    /// Net flow (income - expenses) for the month
    /// </summary>
    public decimal MonthlyNetFlow => MonthlyIncome - MonthlyExpenses;

    /// <summary>
    /// Number of transactions for the month
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Comparison with current values
    /// </summary>
    public HistoricalComparison? Comparison { get; set; }

    /// <summary>
    /// When this report was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Historical account balance
/// </summary>
public class HistoricalAccountItem
{
    public int AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "SEK";
    public string? Color { get; set; }
}

/// <summary>
/// Historical investment position
/// </summary>
public class HistoricalInvestmentItem
{
    public int InvestmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal PriceAtDate { get; set; }
    public decimal TotalValue { get; set; }
    public string? Currency { get; set; }
}

/// <summary>
/// Historical asset value
/// </summary>
public class HistoricalAssetItem
{
    public int AssetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Currency { get; set; } = "SEK";
}

/// <summary>
/// Historical loan balance
/// </summary>
public class HistoricalLoanItem
{
    public int LoanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal InterestRate { get; set; }
    public string Currency { get; set; } = "SEK";
}

/// <summary>
/// Comparison between historical and current values
/// </summary>
public class HistoricalComparison
{
    /// <summary>
    /// Change in net worth since the historical date
    /// </summary>
    public decimal NetWorthChange { get; set; }

    /// <summary>
    /// Percentage change in net worth
    /// </summary>
    public decimal NetWorthChangePercent { get; set; }

    /// <summary>
    /// Change in total assets since the historical date
    /// </summary>
    public decimal TotalAssetsChange { get; set; }

    /// <summary>
    /// Change in total liabilities since the historical date
    /// </summary>
    public decimal TotalLiabilitiesChange { get; set; }

    /// <summary>
    /// Number of days between historical date and now
    /// </summary>
    public int DaysElapsed { get; set; }
}
