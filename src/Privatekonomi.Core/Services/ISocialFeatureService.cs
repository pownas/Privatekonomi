using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service interface for social features including goal sharing, savings groups, and community comparison
/// </summary>
public interface ISocialFeatureService
{
    // Privacy Settings
    Task<UserPrivacySettings> GetPrivacySettingsAsync(string userId);
    Task<UserPrivacySettings> UpdatePrivacySettingsAsync(UserPrivacySettings settings);
    Task<bool> CanUseSocialFeaturesAsync(string userId);
    
    // Goal Sharing
    Task<GoalShare> CreateGoalShareAsync(int goalId, GoalShare shareSettings);
    Task<GoalShare> CreateSharedGoalShareAsync(int sharedGoalId, GoalShare shareSettings);
    Task<GoalShare?> GetGoalShareByTokenAsync(string token);
    Task IncrementShareViewCountAsync(string token);
    Task<IEnumerable<GoalShare>> GetUserSharesAsync();
    Task RevokeShareAsync(int shareId);
    
    // Savings Groups
    Task<SavingsGroup> CreateSavingsGroupAsync(SavingsGroup group);
    Task<SavingsGroup?> GetSavingsGroupAsync(int groupId);
    Task<IEnumerable<SavingsGroup>> GetUserGroupsAsync();
    Task<SavingsGroup> UpdateSavingsGroupAsync(SavingsGroup group);
    Task DeleteSavingsGroupAsync(int groupId);
    
    // Group Members
    Task<SavingsGroupMember> InviteMemberAsync(int groupId, string userId, GroupMemberRole role);
    Task AcceptGroupInvitationAsync(int groupId);
    Task RejectGroupInvitationAsync(int groupId);
    Task<IEnumerable<SavingsGroupMember>> GetGroupMembersAsync(int groupId);
    Task RemoveMemberAsync(int groupId, string userId);
    Task UpdateMemberPrivacyAsync(int memberId, SavingsGroupMember memberSettings);
    
    // Group Goals
    Task<GroupGoal> ShareGoalToGroupAsync(int groupId, int goalId, bool isAnonymous);
    Task<IEnumerable<GroupGoal>> GetGroupGoalsAsync(int groupId);
    Task UnshareGoalFromGroupAsync(int groupGoalId);
    
    // Comments and Likes
    Task<GroupComment> AddCommentAsync(int groupId, string content, int? groupGoalId = null);
    Task<IEnumerable<GroupComment>> GetGroupCommentsAsync(int groupId, int? groupGoalId = null);
    Task DeleteCommentAsync(int commentId);
    Task<CommentLike> LikeCommentAsync(int commentId);
    Task UnlikeCommentAsync(int commentId);
    
    // Leaderboards
    Task<IEnumerable<LeaderboardEntry>> GetHouseholdLeaderboardAsync(int householdId);
    Task<IEnumerable<LeaderboardEntry>> GetGroupLeaderboardAsync(int groupId);
    
    // Community Comparison (anonymized)
    Task<CommunityStats> GetCommunityStatsAsync();
    Task<ComparisonResult> CompareToCommunityAsync();
}

/// <summary>
/// DTO for leaderboard entries
/// </summary>
public class LeaderboardEntry
{
    public string DisplayName { get; set; } = string.Empty;
    public int CompletedGoals { get; set; }
    public decimal TotalSaved { get; set; }
    public int ActiveGoals { get; set; }
    public decimal AverageProgress { get; set; }
    public int Rank { get; set; }
    public bool IsCurrentUser { get; set; }
}

/// <summary>
/// DTO for anonymized community statistics
/// </summary>
public class CommunityStats
{
    public int TotalUsers { get; set; }
    public int TotalGoals { get; set; }
    public decimal AverageSavings { get; set; }
    public decimal MedianSavings { get; set; }
    public decimal AverageGoalCount { get; set; }
    public decimal AverageCompletionRate { get; set; }
}

/// <summary>
/// DTO for comparing user to community
/// </summary>
public class ComparisonResult
{
    public decimal UserTotalSavings { get; set; }
    public decimal CommunityAverageSavings { get; set; }
    public decimal PercentileRank { get; set; }
    public int UserGoalCount { get; set; }
    public decimal CommunityAverageGoalCount { get; set; }
    public decimal UserCompletionRate { get; set; }
    public decimal CommunityAverageCompletionRate { get; set; }
}
