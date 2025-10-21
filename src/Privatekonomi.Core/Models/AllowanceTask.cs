namespace Privatekonomi.Core.Models;

public class AllowanceTask
{
    public int AllowanceTaskId { get; set; }
    public int ChildAllowanceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal RewardAmount { get; set; }
    public AllowanceTaskStatus Status { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public ChildAllowance? ChildAllowance { get; set; }
    public ICollection<AllowanceTransaction> AllowanceTransactions { get; set; } = new List<AllowanceTransaction>();
}

public enum AllowanceTaskStatus
{
    Pending,        // Väntande
    InProgress,     // Pågående
    Completed,      // Klar (väntar på godkännande)
    Approved,       // Godkänd (belöning utbetald)
    Rejected        // Avvisad
}
