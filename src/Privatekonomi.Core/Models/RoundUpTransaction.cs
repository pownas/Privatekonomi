namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a round-up savings transaction
/// </summary>
public class RoundUpTransaction
{
    public int RoundUpTransactionId { get; set; }
    
    /// <summary>
    /// Reference to the original transaction that triggered the round-up
    /// </summary>
    public int TransactionId { get; set; }
    
    /// <summary>
    /// Original transaction amount
    /// </summary>
    public decimal OriginalAmount { get; set; }
    
    /// <summary>
    /// Rounded-up amount
    /// </summary>
    public decimal RoundedAmount { get; set; }
    
    /// <summary>
    /// Round-up savings amount (difference between rounded and original)
    /// </summary>
    public decimal RoundUpAmount { get; set; }
    
    /// <summary>
    /// Employer matching amount (if enabled)
    /// </summary>
    public decimal EmployerMatchingAmount { get; set; }
    
    /// <summary>
    /// Total amount saved (RoundUpAmount + EmployerMatchingAmount)
    /// </summary>
    public decimal TotalSaved { get; set; }
    
    /// <summary>
    /// Goal where the savings were deposited
    /// </summary>
    public int? GoalId { get; set; }
    
    /// <summary>
    /// Type of round-up transaction
    /// </summary>
    public RoundUpType Type { get; set; }
    
    /// <summary>
    /// Whether this round-up was automatically processed
    /// </summary>
    public bool IsAutomatic { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    // User ownership
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    // Navigation properties
    public Transaction? Transaction { get; set; }
    public Goal? Goal { get; set; }
}

/// <summary>
/// Types of round-up transactions
/// </summary>
public enum RoundUpType
{
    /// <summary>
    /// Standard round-up from a transaction (includes employer matching if enabled)
    /// </summary>
    StandardRoundUp,
    
    /// <summary>
    /// Salary-based automatic savings
    /// </summary>
    SalaryAutoSave
}
