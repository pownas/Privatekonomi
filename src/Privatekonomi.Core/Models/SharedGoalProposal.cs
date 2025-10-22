namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a proposal to change a shared goal's parameters
/// </summary>
public class SharedGoalProposal
{
    public int SharedGoalProposalId { get; set; }
    public int SharedGoalId { get; set; }
    public SharedGoal? SharedGoal { get; set; }
    
    public string ProposedByUserId { get; set; } = string.Empty;
    public ApplicationUser? ProposedByUser { get; set; }
    
    public ProposalType ProposalType { get; set; }
    public string? CurrentValue { get; set; }
    public string ProposedValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    
    // Navigation properties
    public ICollection<SharedGoalProposalVote> Votes { get; set; } = new List<SharedGoalProposalVote>();
}

public enum ProposalType
{
    ChangeTargetAmount,
    ChangeTargetDate,
    ChangeName,
    ChangeDescription,
    ChangePriority
}

public enum ProposalStatus
{
    Pending,
    Approved,
    Rejected,
    Withdrawn
}
