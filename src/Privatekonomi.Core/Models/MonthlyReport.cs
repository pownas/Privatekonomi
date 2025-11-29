namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a generated monthly financial report
/// </summary>
public class MonthlyReport : ITemporalEntity
{
    public int MonthlyReportId { get; set; }
    
    /// <summary>
    /// The year and month this report is for (stored as first day of month)
    /// </summary>
    public DateTime ReportMonth { get; set; }
    
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
    /// Income change compared to previous month (percentage)
    /// </summary>
    public decimal IncomeChangePercent { get; set; }
    
    /// <summary>
    /// Expense change compared to previous month (percentage)
    /// </summary>
    public decimal ExpenseChangePercent { get; set; }
    
    /// <summary>
    /// JSON serialized list of category summaries
    /// </summary>
    public string? CategorySummariesJson { get; set; }
    
    /// <summary>
    /// JSON serialized list of top merchants
    /// </summary>
    public string? TopMerchantsJson { get; set; }
    
    /// <summary>
    /// JSON serialized budget outcome data
    /// </summary>
    public string? BudgetOutcomeJson { get; set; }
    
    /// <summary>
    /// JSON serialized insights and recommendations
    /// </summary>
    public string? InsightsJson { get; set; }
    
    /// <summary>
    /// When the report was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; }
    
    /// <summary>
    /// Status of the report: Draft, Generated, Sent
    /// </summary>
    public ReportStatus Status { get; set; }
    
    // Temporal tracking
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    // Household support
    public int? HouseholdId { get; set; }
    public Household? Household { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property for deliveries
    public ICollection<ReportDelivery> Deliveries { get; set; } = new List<ReportDelivery>();
}

/// <summary>
/// Status of a monthly report
/// </summary>
public enum ReportStatus
{
    Draft = 0,
    Generated = 1,
    Sent = 2
}
