namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents the projected results of a cash flow scenario
/// </summary>
public class ScenarioProjection
{
    /// <summary>
    /// Name of the scenario
    /// </summary>
    public string ScenarioName { get; set; } = string.Empty;
    
    /// <summary>
    /// Monthly projection data points
    /// </summary>
    public List<MonthlyProjection> MonthlyData { get; set; } = new();
    
    /// <summary>
    /// Final amount after projection period
    /// </summary>
    public decimal FinalAmount { get; set; }
    
    /// <summary>
    /// Total contributions made
    /// </summary>
    public decimal TotalContributions { get; set; }
    
    /// <summary>
    /// Total interest earned
    /// </summary>
    public decimal TotalInterest { get; set; }
    
    /// <summary>
    /// Real value adjusted for inflation (if inflation rate was provided)
    /// </summary>
    public decimal? RealValue { get; set; }
}

/// <summary>
/// Represents projection data for a single month
/// </summary>
public class MonthlyProjection
{
    /// <summary>
    /// Month number (0 = start, 1 = first month, etc.)
    /// </summary>
    public int Month { get; set; }
    
    /// <summary>
    /// Date for this projection point
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Balance at this point including interest
    /// </summary>
    public decimal Balance { get; set; }
    
    /// <summary>
    /// Cumulative contributions up to this point
    /// </summary>
    public decimal CumulativeContributions { get; set; }
    
    /// <summary>
    /// Cumulative interest earned up to this point
    /// </summary>
    public decimal CumulativeInterest { get; set; }
    
    /// <summary>
    /// Monthly savings contribution for this month
    /// </summary>
    public decimal MonthlyContribution { get; set; }
    
    /// <summary>
    /// Interest earned this month
    /// </summary>
    public decimal InterestThisMonth { get; set; }
    
    /// <summary>
    /// Real value adjusted for inflation (if applicable)
    /// </summary>
    public decimal? RealValue { get; set; }
}
