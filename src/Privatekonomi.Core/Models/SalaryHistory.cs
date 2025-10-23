namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a salary entry in the user's salary history
/// </summary>
public class SalaryHistory
{
    public int SalaryHistoryId { get; set; }
    
    /// <summary>
    /// Monthly salary amount (before tax)
    /// </summary>
    public decimal MonthlySalary { get; set; }
    
    /// <summary>
    /// Year and month of the salary entry (e.g., 2024-01)
    /// </summary>
    public DateTime Period { get; set; }
    
    /// <summary>
    /// Job title or position
    /// </summary>
    public string? JobTitle { get; set; }
    
    /// <summary>
    /// Employer name
    /// </summary>
    public string? Employer { get; set; }
    
    /// <summary>
    /// Employment type: "Heltid", "Deltid", "Timavlönad", etc.
    /// </summary>
    public string? EmploymentType { get; set; }
    
    /// <summary>
    /// Work percentage (100 for full time, 50 for half time, etc.)
    /// </summary>
    public decimal? WorkPercentage { get; set; }
    
    /// <summary>
    /// Notes about salary change (e.g., "Lönerevision", "Nytt jobb", "Befordran")
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Currency (default SEK)
    /// </summary>
    public string Currency { get; set; } = "SEK";
    
    /// <summary>
    /// Is this the current/active salary
    /// </summary>
    public bool IsCurrent { get; set; }
    
    /// <summary>
    /// Timestamp when entry was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Timestamp when entry was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
