namespace Privatekonomi.Core.Models;

public class AllowanceTransaction
{
    public int AllowanceTransactionId { get; set; }
    public int ChildAllowanceId { get; set; }
    public int? AllowanceTaskId { get; set; }
    public decimal Amount { get; set; }
    public AllowanceTransactionType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public ChildAllowance? ChildAllowance { get; set; }
    public AllowanceTask? AllowanceTask { get; set; }
}

public enum AllowanceTransactionType
{
    Deposit,        // Insättning (veckopeng/månadspeng)
    TaskReward,     // Belöning för uppgift
    Withdrawal,     // Uttag
    Adjustment      // Justering
}
