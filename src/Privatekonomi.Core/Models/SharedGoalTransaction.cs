namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a transaction (deposit or withdrawal) for a shared goal
/// </summary>
public class SharedGoalTransaction
{
    public int SharedGoalTransactionId { get; set; }
    public int SharedGoalId { get; set; }
    public SharedGoal? SharedGoal { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string? Description { get; set; }
    
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum TransactionType
{
    Deposit,
    Withdrawal
}
