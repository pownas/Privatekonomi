namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a participant in a shared savings goal
/// </summary>
public class SharedGoalParticipant
{
    public int SharedGoalParticipantId { get; set; }
    public int SharedGoalId { get; set; }
    public SharedGoal? SharedGoal { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    
    public ParticipantRole Role { get; set; } = ParticipantRole.Participant;
    public DateTime JoinedAt { get; set; }
    
    // Invitation details
    public InvitationStatus InvitationStatus { get; set; } = InvitationStatus.Accepted;
    public string? InvitedByUserId { get; set; }
    public ApplicationUser? InvitedByUser { get; set; }
    public DateTime? InvitedAt { get; set; }
}

public enum ParticipantRole
{
    Owner,
    Participant
}

public enum InvitationStatus
{
    Pending,
    Accepted,
    Rejected
}
