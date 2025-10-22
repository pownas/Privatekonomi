namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a vote on a shared goal proposal
/// </summary>
public class SharedGoalProposalVote
{
    public int SharedGoalProposalVoteId { get; set; }
    public int SharedGoalProposalId { get; set; }
    public SharedGoalProposal? SharedGoalProposal { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    
    public VoteType Vote { get; set; }
    public DateTime VotedAt { get; set; }
    public string? Comment { get; set; }
}

public enum VoteType
{
    Approve,
    Reject
}
