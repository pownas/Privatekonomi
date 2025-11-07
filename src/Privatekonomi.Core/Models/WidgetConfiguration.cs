namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents the configuration of a widget in a dashboard layout.
/// Includes position, size, and widget-specific settings.
/// </summary>
public class WidgetConfiguration
{
    public int WidgetConfigId { get; set; }
    
    /// <summary>
    /// Foreign key to the dashboard layout
    /// </summary>
    public int LayoutId { get; set; }
    
    /// <summary>
    /// Type of widget to display
    /// </summary>
    public WidgetType Type { get; set; }
    
    /// <summary>
    /// Grid row position (0-based)
    /// </summary>
    public int Row { get; set; }
    
    /// <summary>
    /// Grid column position (0-based)
    /// </summary>
    public int Column { get; set; }
    
    /// <summary>
    /// Width in grid units (1-12 for responsive grid)
    /// </summary>
    public int Width { get; set; }
    
    /// <summary>
    /// Height in grid units
    /// </summary>
    public int Height { get; set; }
    
    /// <summary>
    /// JSON string containing widget-specific settings
    /// </summary>
    public string? Settings { get; set; }
    
    // Navigation properties
    public DashboardLayout? Layout { get; set; }
}

/// <summary>
/// Enum representing available widget types
/// </summary>
public enum WidgetType
{
    NetWorth,
    CashFlow,
    Goals,
    Loans,
    Investments,
    BudgetOverview,
    UpcomingBills,
    MonthlyExpenses,
    CategoryPieChart,
    TrendChart,
    QuickActions,
    MonthSummary,
    PeriodComparison,
    RecentTransactions,
    UnmappedTransactions
}
