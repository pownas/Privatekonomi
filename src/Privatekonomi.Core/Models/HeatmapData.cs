namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents expense heatmap data for a specific time period
/// </summary>
public class ExpenseHeatmapData
{
    /// <summary>
    /// Heatmap cells indexed by [weekday][hour]
    /// Weekday: 0 = Monday, 6 = Sunday
    /// Hour: 0-23
    /// </summary>
    public Dictionary<int, Dictionary<int, HeatmapCell>> HeatmapCells { get; set; } = new();
    
    /// <summary>
    /// Total expenses in the period
    /// </summary>
    public decimal TotalExpenses { get; set; }
    
    /// <summary>
    /// Number of transactions analyzed
    /// </summary>
    public int TransactionCount { get; set; }
    
    /// <summary>
    /// Start date of analysis period
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// End date of analysis period
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Category filter applied (null = all categories)
    /// </summary>
    public int? CategoryId { get; set; }
    
    /// <summary>
    /// Category name if filtered
    /// </summary>
    public string? CategoryName { get; set; }
    
    /// <summary>
    /// Maximum expense amount in any single cell (used for color scaling)
    /// </summary>
    public decimal MaxCellAmount { get; set; }
    
    /// <summary>
    /// Detected patterns and insights
    /// </summary>
    public PatternInsights Insights { get; set; } = new();
}

/// <summary>
/// Represents a single cell in the heatmap grid
/// </summary>
public class HeatmapCell
{
    /// <summary>
    /// Weekday (0 = Monday, 6 = Sunday)
    /// </summary>
    public int Weekday { get; set; }
    
    /// <summary>
    /// Hour of day (0-23)
    /// </summary>
    public int Hour { get; set; }
    
    /// <summary>
    /// Total expense amount for this weekday/hour combination
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Number of transactions in this cell
    /// </summary>
    public int TransactionCount { get; set; }
    
    /// <summary>
    /// Average transaction amount in this cell
    /// </summary>
    public decimal AverageAmount { get; set; }
    
    /// <summary>
    /// Intensity level for color coding (0-3)
    /// 0 = Low, 1 = Medium, 2 = High, 3 = Very High
    /// </summary>
    public int IntensityLevel { get; set; }
}

/// <summary>
/// Detected patterns and insights from expense analysis
/// </summary>
public class PatternInsights
{
    /// <summary>
    /// Most expensive day of the week
    /// </summary>
    public DayInsight MostExpensiveDay { get; set; } = new();
    
    /// <summary>
    /// Least expensive day of the week
    /// </summary>
    public DayInsight LeastExpensiveDay { get; set; } = new();
    
    /// <summary>
    /// Peak expense time periods
    /// </summary>
    public List<TimeSlotInsight> ExpensePeaks { get; set; } = new();
    
    /// <summary>
    /// Detected impulse purchases (late evening/night purchases)
    /// </summary>
    public ImpulsePurchaseInsight ImpulsePurchases { get; set; } = new();
    
    /// <summary>
    /// Most common expense pattern (e.g., "Lunch on weekdays")
    /// </summary>
    public string? CommonPattern { get; set; }
    
    /// <summary>
    /// Top categories by time slot
    /// </summary>
    public List<CategoryTimePattern> CategoryPatterns { get; set; } = new();
}

/// <summary>
/// Insights about a specific day of the week
/// </summary>
public class DayInsight
{
    /// <summary>
    /// Day of week (0 = Monday, 6 = Sunday)
    /// </summary>
    public int Weekday { get; set; }
    
    /// <summary>
    /// Swedish day name
    /// </summary>
    public string DayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Total amount spent on this day
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Number of transactions
    /// </summary>
    public int TransactionCount { get; set; }
    
    /// <summary>
    /// Most common category on this day
    /// </summary>
    public string? TopCategory { get; set; }
}

/// <summary>
/// Insights about a specific time slot
/// </summary>
public class TimeSlotInsight
{
    /// <summary>
    /// Weekday (0 = Monday, 6 = Sunday)
    /// </summary>
    public int Weekday { get; set; }
    
    /// <summary>
    /// Swedish day name
    /// </summary>
    public string DayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Hour range (e.g., "16-20")
    /// </summary>
    public string TimeRange { get; set; } = string.Empty;
    
    /// <summary>
    /// Total amount in this time slot
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Most common category in this time slot
    /// </summary>
    public string? TopCategory { get; set; }
}

/// <summary>
/// Insights about impulse purchases
/// </summary>
public class ImpulsePurchaseInsight
{
    /// <summary>
    /// Total amount of impulse purchases (20:00-00:00)
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Number of impulse purchase transactions
    /// </summary>
    public int TransactionCount { get; set; }
    
    /// <summary>
    /// Percentage of total expenses
    /// </summary>
    public decimal PercentageOfTotal { get; set; }
    
    /// <summary>
    /// Most common day for impulse purchases
    /// </summary>
    public string? MostCommonDay { get; set; }
    
    /// <summary>
    /// Most common category for impulse purchases
    /// </summary>
    public string? TopCategory { get; set; }
    
    /// <summary>
    /// Whether impulse purchases are detected (threshold: >5% of total)
    /// </summary>
    public bool IsDetected { get; set; }
}

/// <summary>
/// Pattern showing which categories are spent on at which times
/// </summary>
public class CategoryTimePattern
{
    /// <summary>
    /// Category name
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;
    
    /// <summary>
    /// Weekday (0 = Monday, 6 = Sunday)
    /// </summary>
    public int Weekday { get; set; }
    
    /// <summary>
    /// Time range
    /// </summary>
    public string TimeRange { get; set; } = string.Empty;
    
    /// <summary>
    /// Amount spent
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Percentage of total in this category
    /// </summary>
    public decimal Percentage { get; set; }
}
