namespace Privatekonomi.Core.Models;

public class CategoryStatistics
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#000000";
    
    // Total amounts for the selected period
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetAmount { get; set; }
    
    // Monthly data
    public List<MonthlyAmount> MonthlyBreakdown { get; set; } = new();
    
    // Averages
    public decimal Average12Months { get; set; }
    public decimal Average24Months { get; set; }
    
    // Trend indicators
    public decimal TrendPercentage { get; set; } // Positive = increasing, Negative = decreasing
    public bool IsIncreasing { get; set; }
}

public class MonthlyAmount
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty; // e.g., "2024-01"
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal NetAmount { get; set; }
    public int TransactionCount { get; set; }
}
