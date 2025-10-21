using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface ISharedGoalService
{
    // Shared Goal CRUD
    Task<IEnumerable<SharedGoal>> GetAllSharedGoalsAsync();
    Task<SharedGoal?> GetSharedGoalByIdAsync(int id);
    Task<SharedGoal> CreateSharedGoalAsync(SharedGoal sharedGoal);
    Task<SharedGoal> UpdateSharedGoalAsync(SharedGoal sharedGoal);
    Task DeleteSharedGoalAsync(int id);
    
    // Participant Management
    Task<IEnumerable<SharedGoalParticipant>> GetParticipantsAsync(int sharedGoalId);
    Task<SharedGoalParticipant> InviteParticipantAsync(int sharedGoalId, string userEmail);
    Task<SharedGoalParticipant> AcceptInvitationAsync(int participantId);
    Task<SharedGoalParticipant> RejectInvitationAsync(int participantId);
    Task RemoveParticipantAsync(int sharedGoalId, string userId);
    Task LeaveSharedGoalAsync(int sharedGoalId);
    Task TransferOwnershipAsync(int sharedGoalId, string newOwnerUserId);
    Task<bool> IsParticipantAsync(int sharedGoalId, string userId);
    Task<bool> IsOwnerAsync(int sharedGoalId, string userId);
    
    // Proposal Management
    Task<IEnumerable<SharedGoalProposal>> GetProposalsAsync(int sharedGoalId);
    Task<SharedGoalProposal> CreateProposalAsync(SharedGoalProposal proposal);
    Task<SharedGoalProposalVote> VoteOnProposalAsync(int proposalId, VoteType vote, string? comment = null);
    Task WithdrawProposalAsync(int proposalId);
    Task<bool> CheckAndApplyProposalAsync(int proposalId);
    
    // Transaction Management
    Task<IEnumerable<SharedGoalTransaction>> GetTransactionsAsync(int sharedGoalId);
    Task<SharedGoalTransaction> CreateTransactionAsync(SharedGoalTransaction transaction);
    Task UpdateGoalAmountAsync(int sharedGoalId, decimal amount, TransactionType type, string? description = null);
    
    // Notification Management
    Task<IEnumerable<SharedGoalNotification>> GetNotificationsAsync();
    Task<IEnumerable<SharedGoalNotification>> GetUnreadNotificationsAsync();
    Task<SharedGoalNotification> MarkNotificationAsReadAsync(int notificationId);
    Task DeleteNotificationAsync(int notificationId);
    Task CreateNotificationAsync(int sharedGoalId, string userId, NotificationType type, string message);
    
    // Statistics
    Task<decimal> GetTotalProgressAsync(int sharedGoalId);
    Task<IEnumerable<SharedGoal>> GetActiveSharedGoalsAsync();
}
