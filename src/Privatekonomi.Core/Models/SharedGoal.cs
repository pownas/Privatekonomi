namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a savings goal shared between multiple users
/// </summary>
public class SharedGoal
{
    public int SharedGoalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public int Priority { get; set; } = 3; // 1 (highest) to 5 (lowest)
    public SharedGoalStatus Status { get; set; } = SharedGoalStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Creator of the shared goal
    public string CreatedByUserId { get; set; } = string.Empty;
    public ApplicationUser? CreatedByUser { get; set; }
    
    // Navigation properties
    public ICollection<SharedGoalParticipant> Participants { get; set; } = new List<SharedGoalParticipant>();
    public ICollection<SharedGoalProposal> Proposals { get; set; } = new List<SharedGoalProposal>();
    public ICollection<SharedGoalTransaction> Transactions { get; set; } = new List<SharedGoalTransaction>();
    public ICollection<SharedGoalNotification> Notifications { get; set; } = new List<SharedGoalNotification>();
}

public enum SharedGoalStatus
{
    Active,
    Completed,
    Archived
}
