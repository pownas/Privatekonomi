namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a notification for a shared goal event
/// </summary>
public class SharedGoalNotification
{
    public int SharedGoalNotificationId { get; set; }
    public int SharedGoalId { get; set; }
    public SharedGoal? SharedGoal { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public enum NotificationType
{
    Invitation,
    InvitationAccepted,
    InvitationRejected,
    ProposalCreated,
    ProposalApproved,
    ProposalRejected,
    ProposalWithdrawn,
    TransactionMade,
    ParticipantJoined,
    ParticipantLeft,
    ParticipantRemoved,
    GoalCompleted,
    OwnershipTransferred
}
