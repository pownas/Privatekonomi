using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using System.Security.Cryptography;

namespace Privatekonomi.Core.Services;

public class SocialFeatureService : ISocialFeatureService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SocialFeatureService(PrivatekonomyContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    private string GetCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        return _currentUserService.UserId;
    }

    #region Privacy Settings

    public async Task<UserPrivacySettings> GetPrivacySettingsAsync(string userId)
    {
        var settings = await _context.UserPrivacySettings
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            // Create default privacy settings (all disabled by default - GDPR compliant)
            settings = new UserPrivacySettings
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.UserPrivacySettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return settings;
    }

    public async Task<UserPrivacySettings> UpdatePrivacySettingsAsync(UserPrivacySettings settings)
    {
        var userId = GetCurrentUserId();
        
        if (settings.UserId != userId)
        {
            throw new UnauthorizedAccessException("Cannot update another user's privacy settings");
        }

        settings.UpdatedAt = DateTime.UtcNow;
        _context.Entry(settings).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return settings;
    }

    public async Task<bool> CanUseSocialFeaturesAsync(string userId)
    {
        var settings = await GetPrivacySettingsAsync(userId);
        return settings.EnableSocialFeatures;
    }

    #endregion

    #region Goal Sharing

    public async Task<GoalShare> CreateGoalShareAsync(int goalId, GoalShare shareSettings)
    {
        var userId = GetCurrentUserId();
        
        // Check if user owns the goal
        var goal = await _context.Goals.FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId);
        if (goal == null)
        {
            throw new UnauthorizedAccessException("Goal not found or access denied");
        }

        // Check privacy settings
        var privacySettings = await GetPrivacySettingsAsync(userId);
        if (!privacySettings.AllowGoalSharing)
        {
            throw new InvalidOperationException("Goal sharing is not enabled in privacy settings");
        }

        shareSettings.GoalId = goalId;
        shareSettings.UserId = userId;
        shareSettings.ShareToken = GenerateShareToken();
        shareSettings.CreatedAt = DateTime.UtcNow;
        shareSettings.IsActive = true;

        _context.GoalShares.Add(shareSettings);
        await _context.SaveChangesAsync();

        return shareSettings;
    }

    public async Task<GoalShare> CreateSharedGoalShareAsync(int sharedGoalId, GoalShare shareSettings)
    {
        var userId = GetCurrentUserId();
        
        // Check if user is a participant in the shared goal
        var sharedGoal = await _context.SharedGoals
            .Include(sg => sg.Participants)
            .FirstOrDefaultAsync(sg => sg.SharedGoalId == sharedGoalId);
            
        if (sharedGoal == null || !sharedGoal.Participants.Any(p => p.UserId == userId && p.InvitationStatus == InvitationStatus.Accepted))
        {
            throw new UnauthorizedAccessException("Shared goal not found or access denied");
        }

        // Check privacy settings
        var privacySettings = await GetPrivacySettingsAsync(userId);
        if (!privacySettings.AllowGoalSharing)
        {
            throw new InvalidOperationException("Goal sharing is not enabled in privacy settings");
        }

        shareSettings.SharedGoalId = sharedGoalId;
        shareSettings.UserId = userId;
        shareSettings.ShareToken = GenerateShareToken();
        shareSettings.CreatedAt = DateTime.UtcNow;
        shareSettings.IsActive = true;

        _context.GoalShares.Add(shareSettings);
        await _context.SaveChangesAsync();

        return shareSettings;
    }

    public async Task<GoalShare?> GetGoalShareByTokenAsync(string token)
    {
        var share = await _context.GoalShares
            .Include(s => s.Goal)
            .Include(s => s.SharedGoal)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.ShareToken == token && s.IsActive);

        if (share != null && share.ExpiresAt.HasValue && share.ExpiresAt.Value < DateTime.UtcNow)
        {
            // Share has expired
            return null;
        }

        return share;
    }

    public async Task IncrementShareViewCountAsync(string token)
    {
        var share = await _context.GoalShares.FirstOrDefaultAsync(s => s.ShareToken == token);
        if (share != null)
        {
            share.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<GoalShare>> GetUserSharesAsync()
    {
        var userId = GetCurrentUserId();
        
        return await _context.GoalShares
            .Include(s => s.Goal)
            .Include(s => s.SharedGoal)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task RevokeShareAsync(int shareId)
    {
        var userId = GetCurrentUserId();
        
        var share = await _context.GoalShares.FindAsync(shareId);
        if (share == null || share.UserId != userId)
        {
            throw new UnauthorizedAccessException("Share not found or access denied");
        }

        share.IsActive = false;
        await _context.SaveChangesAsync();
    }

    private string GenerateShareToken()
    {
        // Generate a secure random token
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes).Replace("/", "_").Replace("+", "-").Replace("=", "");
    }

    #endregion

    #region Savings Groups

    public async Task<SavingsGroup> CreateSavingsGroupAsync(SavingsGroup group)
    {
        var userId = GetCurrentUserId();
        
        var privacySettings = await GetPrivacySettingsAsync(userId);
        if (!privacySettings.AllowSavingsGroups)
        {
            throw new InvalidOperationException("Savings groups are not enabled in privacy settings");
        }

        group.CreatedByUserId = userId;
        group.CreatedAt = DateTime.UtcNow;
        group.IsActive = true;

        _context.SavingsGroups.Add(group);
        await _context.SaveChangesAsync();

        // Add creator as owner member
        var ownerMember = new SavingsGroupMember
        {
            SavingsGroupId = group.SavingsGroupId,
            UserId = userId,
            Role = GroupMemberRole.Owner,
            JoinedAt = DateTime.UtcNow,
            Status = GroupMemberStatus.Active,
            ShowRealName = !privacySettings.AnonymousByDefault
        };

        _context.SavingsGroupMembers.Add(ownerMember);
        await _context.SaveChangesAsync();

        return group;
    }

    public async Task<SavingsGroup?> GetSavingsGroupAsync(int groupId)
    {
        var userId = GetCurrentUserId();
        
        var group = await _context.SavingsGroups
            .Include(g => g.Members).ThenInclude(m => m.User)
            .Include(g => g.CreatedByUser)
            .FirstOrDefaultAsync(g => g.SavingsGroupId == groupId);

        if (group == null)
            return null;

        // Verify user has access
        if (!group.Members.Any(m => m.UserId == userId && m.Status == GroupMemberStatus.Active))
        {
            throw new UnauthorizedAccessException("User does not have access to this group");
        }

        return group;
    }

    public async Task<IEnumerable<SavingsGroup>> GetUserGroupsAsync()
    {
        var userId = GetCurrentUserId();
        
        // Get all memberships for the user first
        var membershipGroupIds = await _context.SavingsGroupMembers
            .Where(m => m.UserId == userId && m.Status == GroupMemberStatus.Active)
            .Select(m => m.SavingsGroupId)
            .ToListAsync();
        
        // Then get the groups
        return await _context.SavingsGroups
            .Include(g => g.Members)
            .Include(g => g.CreatedByUser)
            .Where(g => membershipGroupIds.Contains(g.SavingsGroupId))
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<SavingsGroup> UpdateSavingsGroupAsync(SavingsGroup group)
    {
        var userId = GetCurrentUserId();
        
        // Check if user is owner or admin
        var member = await _context.SavingsGroupMembers
            .FirstOrDefaultAsync(m => m.SavingsGroupId == group.SavingsGroupId && m.UserId == userId);

        if (member == null || (member.Role != GroupMemberRole.Owner && member.Role != GroupMemberRole.Admin))
        {
            throw new UnauthorizedAccessException("Only group owners and admins can update group settings");
        }

        group.UpdatedAt = DateTime.UtcNow;
        _context.Entry(group).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return group;
    }

    public async Task DeleteSavingsGroupAsync(int groupId)
    {
        var userId = GetCurrentUserId();
        
        var group = await _context.SavingsGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.SavingsGroupId == groupId);

        if (group == null)
        {
            throw new InvalidOperationException("Group not found");
        }

        // Check if user is owner
        if (group.CreatedByUserId != userId)
        {
            throw new UnauthorizedAccessException("Only group owner can delete the group");
        }

        group.IsActive = false;
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Group Members

    public async Task<SavingsGroupMember> InviteMemberAsync(int groupId, string userId, GroupMemberRole role)
    {
        var currentUserId = GetCurrentUserId();
        
        // Check if current user is owner or admin
        var currentMember = await _context.SavingsGroupMembers
            .FirstOrDefaultAsync(m => m.SavingsGroupId == groupId && m.UserId == currentUserId);

        if (currentMember == null || (currentMember.Role != GroupMemberRole.Owner && currentMember.Role != GroupMemberRole.Admin))
        {
            throw new UnauthorizedAccessException("Only group owners and admins can invite members");
        }

        // Check if user is already a member
        var existingMember = await _context.SavingsGroupMembers
            .FirstOrDefaultAsync(m => m.SavingsGroupId == groupId && m.UserId == userId);

        if (existingMember != null)
        {
            throw new InvalidOperationException("User is already a member of this group");
        }

        // Check invited user's privacy settings
        var privacySettings = await GetPrivacySettingsAsync(userId);
        if (!privacySettings.AllowSavingsGroups)
        {
            throw new InvalidOperationException("User has not enabled savings groups");
        }

        var member = new SavingsGroupMember
        {
            SavingsGroupId = groupId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow,
            Status = GroupMemberStatus.Pending,
            ShowRealName = !privacySettings.AnonymousByDefault
        };

        _context.SavingsGroupMembers.Add(member);
        await _context.SaveChangesAsync();

        return member;
    }

    public async Task AcceptGroupInvitationAsync(int groupId)
    {
        var userId = GetCurrentUserId();
        
        var member = await _context.SavingsGroupMembers
            .FirstOrDefaultAsync(m => m.SavingsGroupId == groupId && m.UserId == userId);

        if (member == null)
        {
            throw new InvalidOperationException("Invitation not found");
        }

        member.Status = GroupMemberStatus.Active;
        await _context.SaveChangesAsync();
    }

    public async Task RejectGroupInvitationAsync(int groupId)
    {
        var userId = GetCurrentUserId();
        
        var member = await _context.SavingsGroupMembers
            .FirstOrDefaultAsync(m => m.SavingsGroupId == groupId && m.UserId == userId);

        if (member == null)
        {
            throw new InvalidOperationException("Invitation not found");
        }

        _context.SavingsGroupMembers.Remove(member);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<SavingsGroupMember>> GetGroupMembersAsync(int groupId)
    {
        var userId = GetCurrentUserId();
        
        // Verify user has access to the group
        var hasAccess = await _context.SavingsGroupMembers
            .AnyAsync(m => m.SavingsGroupId == groupId && m.UserId == userId && m.Status == GroupMemberStatus.Active);

        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("User does not have access to this group");
        }

        return await _context.SavingsGroupMembers
            .Include(m => m.User)
            .Where(m => m.SavingsGroupId == groupId)
            .OrderByDescending(m => m.Role)
            .ThenBy(m => m.JoinedAt)
            .ToListAsync();
    }

    public async Task RemoveMemberAsync(int groupId, string userId)
    {
        var currentUserId = GetCurrentUserId();
        
        // Check if current user is owner or admin
        var currentMember = await _context.SavingsGroupMembers
            .FirstOrDefaultAsync(m => m.SavingsGroupId == groupId && m.UserId == currentUserId);

        if (currentMember == null || (currentMember.Role != GroupMemberRole.Owner && currentMember.Role != GroupMemberRole.Admin))
        {
            throw new UnauthorizedAccessException("Only group owners and admins can remove members");
        }

        var memberToRemove = await _context.SavingsGroupMembers
            .FirstOrDefaultAsync(m => m.SavingsGroupId == groupId && m.UserId == userId);

        if (memberToRemove == null)
        {
            throw new InvalidOperationException("Member not found");
        }

        // Cannot remove the owner
        if (memberToRemove.Role == GroupMemberRole.Owner)
        {
            throw new InvalidOperationException("Cannot remove the group owner");
        }

        _context.SavingsGroupMembers.Remove(memberToRemove);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateMemberPrivacyAsync(int memberId, SavingsGroupMember memberSettings)
    {
        var userId = GetCurrentUserId();
        
        var member = await _context.SavingsGroupMembers.FindAsync(memberId);
        if (member == null || member.UserId != userId)
        {
            throw new UnauthorizedAccessException("Member not found or access denied");
        }

        member.ShowRealName = memberSettings.ShowRealName;
        member.DisplayName = memberSettings.DisplayName;
        member.ShareProgress = memberSettings.ShareProgress;
        member.ShareGoalCount = memberSettings.ShareGoalCount;
        member.ShareTotalSavings = memberSettings.ShareTotalSavings;

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Group Goals

    public async Task<GroupGoal> ShareGoalToGroupAsync(int groupId, int goalId, bool isAnonymous)
    {
        var userId = GetCurrentUserId();
        
        // Verify user is a member of the group
        var member = await _context.SavingsGroupMembers
            .FirstOrDefaultAsync(m => m.SavingsGroupId == groupId && m.UserId == userId && m.Status == GroupMemberStatus.Active);

        if (member == null)
        {
            throw new UnauthorizedAccessException("User is not a member of this group");
        }

        // Verify user owns the goal
        var goal = await _context.Goals.FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId);
        if (goal == null)
        {
            throw new UnauthorizedAccessException("Goal not found or access denied");
        }

        // Check if goal is already shared to this group
        var existingShare = await _context.GroupGoals
            .FirstOrDefaultAsync(gg => gg.SavingsGroupId == groupId && gg.GoalId == goalId);

        if (existingShare != null)
        {
            throw new InvalidOperationException("Goal is already shared to this group");
        }

        var groupGoal = new GroupGoal
        {
            SavingsGroupId = groupId,
            GoalId = goalId,
            UserId = userId,
            IsAnonymous = isAnonymous,
            SharedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.GroupGoals.Add(groupGoal);
        await _context.SaveChangesAsync();

        return groupGoal;
    }

    public async Task<IEnumerable<GroupGoal>> GetGroupGoalsAsync(int groupId)
    {
        var userId = GetCurrentUserId();
        
        // Verify user has access to the group
        var hasAccess = await _context.SavingsGroupMembers
            .AnyAsync(m => m.SavingsGroupId == groupId && m.UserId == userId && m.Status == GroupMemberStatus.Active);

        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("User does not have access to this group");
        }

        return await _context.GroupGoals
            .Include(gg => gg.Goal)
            .Include(gg => gg.User)
            .Where(gg => gg.SavingsGroupId == groupId && gg.IsActive)
            .OrderByDescending(gg => gg.SharedAt)
            .ToListAsync();
    }

    public async Task UnshareGoalFromGroupAsync(int groupGoalId)
    {
        var userId = GetCurrentUserId();
        
        var groupGoal = await _context.GroupGoals.FindAsync(groupGoalId);
        if (groupGoal == null || groupGoal.UserId != userId)
        {
            throw new UnauthorizedAccessException("Group goal not found or access denied");
        }

        groupGoal.IsActive = false;
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Comments and Likes

    public async Task<GroupComment> AddCommentAsync(int groupId, string content, int? groupGoalId = null)
    {
        var userId = GetCurrentUserId();
        
        // Verify user is a member of the group
        var member = await _context.SavingsGroupMembers
            .FirstOrDefaultAsync(m => m.SavingsGroupId == groupId && m.UserId == userId && m.Status == GroupMemberStatus.Active);

        if (member == null)
        {
            throw new UnauthorizedAccessException("User is not a member of this group");
        }

        var comment = new GroupComment
        {
            SavingsGroupId = groupId,
            GroupGoalId = groupGoalId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        _context.GroupComments.Add(comment);
        await _context.SaveChangesAsync();

        return comment;
    }

    public async Task<IEnumerable<GroupComment>> GetGroupCommentsAsync(int groupId, int? groupGoalId = null)
    {
        var userId = GetCurrentUserId();
        
        // Verify user has access to the group
        var hasAccess = await _context.SavingsGroupMembers
            .AnyAsync(m => m.SavingsGroupId == groupId && m.UserId == userId && m.Status == GroupMemberStatus.Active);

        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("User does not have access to this group");
        }

        var query = _context.GroupComments
            .Include(c => c.User)
            .Include(c => c.Likes)
            .Where(c => c.SavingsGroupId == groupId && c.DeletedAt == null);

        if (groupGoalId.HasValue)
        {
            query = query.Where(c => c.GroupGoalId == groupGoalId.Value);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task DeleteCommentAsync(int commentId)
    {
        var userId = GetCurrentUserId();
        
        var comment = await _context.GroupComments.FindAsync(commentId);
        if (comment == null)
        {
            throw new InvalidOperationException("Comment not found");
        }

        // Only the comment author or group owner can delete
        if (comment.UserId != userId)
        {
            var isGroupOwner = await _context.SavingsGroupMembers
                .AnyAsync(m => m.SavingsGroupId == comment.SavingsGroupId && m.UserId == userId && m.Role == GroupMemberRole.Owner);

            if (!isGroupOwner)
            {
                throw new UnauthorizedAccessException("Only the comment author or group owner can delete comments");
            }
        }

        comment.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<CommentLike> LikeCommentAsync(int commentId)
    {
        var userId = GetCurrentUserId();
        
        // Check if already liked
        var existingLike = await _context.CommentLikes
            .FirstOrDefaultAsync(l => l.GroupCommentId == commentId && l.UserId == userId);

        if (existingLike != null)
        {
            throw new InvalidOperationException("Comment already liked");
        }

        var like = new CommentLike
        {
            GroupCommentId = commentId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CommentLikes.Add(like);
        await _context.SaveChangesAsync();

        return like;
    }

    public async Task UnlikeCommentAsync(int commentId)
    {
        var userId = GetCurrentUserId();
        
        var like = await _context.CommentLikes
            .FirstOrDefaultAsync(l => l.GroupCommentId == commentId && l.UserId == userId);

        if (like == null)
        {
            throw new InvalidOperationException("Like not found");
        }

        _context.CommentLikes.Remove(like);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Leaderboards

    public async Task<IEnumerable<LeaderboardEntry>> GetHouseholdLeaderboardAsync(int householdId)
    {
        var userId = GetCurrentUserId();
        
        // Verify user is a member of the household
        var isMember = await _context.HouseholdMembers
            .AnyAsync(hm => hm.HouseholdId == householdId && hm.UserId == userId);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("User is not a member of this household");
        }

        // Get all household members
        var members = await _context.HouseholdMembers
            .Include(hm => hm.User)
            .Where(hm => hm.HouseholdId == householdId)
            .ToListAsync();

        var leaderboard = new List<LeaderboardEntry>();

        foreach (var member in members)
        {
            // Check privacy settings
            var privacySettings = await GetPrivacySettingsAsync(member.UserId);
            if (!privacySettings.AllowLeaderboards)
                continue;

            var goals = await _context.Goals
                .Where(g => g.UserId == member.UserId)
                .ToListAsync();

            var completedGoals = goals.Count(g => g.CurrentAmount >= g.TargetAmount);
            var totalSaved = goals.Sum(g => g.CurrentAmount);
            var activeGoals = goals.Count(g => g.CurrentAmount < g.TargetAmount);
            var averageProgress = goals.Any() 
                ? goals.Average(g => g.TargetAmount > 0 ? (g.CurrentAmount / g.TargetAmount) * 100 : 0) 
                : 0;

            leaderboard.Add(new LeaderboardEntry
            {
                DisplayName = member.User?.UserName ?? "Unknown",
                CompletedGoals = completedGoals,
                TotalSaved = totalSaved,
                ActiveGoals = activeGoals,
                AverageProgress = averageProgress,
                IsCurrentUser = member.UserId == userId
            });
        }

        // Rank by total saved
        var rankedLeaderboard = leaderboard
            .OrderByDescending(l => l.TotalSaved)
            .Select((entry, index) =>
            {
                entry.Rank = index + 1;
                return entry;
            })
            .ToList();

        return rankedLeaderboard;
    }

    public async Task<IEnumerable<LeaderboardEntry>> GetGroupLeaderboardAsync(int groupId)
    {
        var userId = GetCurrentUserId();
        
        // Verify user is a member of the group
        var isMember = await _context.SavingsGroupMembers
            .AnyAsync(m => m.SavingsGroupId == groupId && m.UserId == userId && m.Status == GroupMemberStatus.Active);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("User is not a member of this group");
        }

        var group = await _context.SavingsGroups
            .Include(g => g.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.SavingsGroupId == groupId);

        if (group == null)
        {
            throw new InvalidOperationException("Group not found");
        }

        var leaderboard = new List<LeaderboardEntry>();

        foreach (var member in group.Members.Where(m => m.Status == GroupMemberStatus.Active))
        {
            if (!member.ShareProgress)
                continue;

            var goals = await _context.Goals
                .Where(g => g.UserId == member.UserId)
                .ToListAsync();

            var completedGoals = goals.Count(g => g.CurrentAmount >= g.TargetAmount);
            var totalSaved = member.ShareTotalSavings ? goals.Sum(g => g.CurrentAmount) : 0;
            var activeGoals = member.ShareGoalCount ? goals.Count(g => g.CurrentAmount < g.TargetAmount) : 0;
            var averageProgress = goals.Any() 
                ? goals.Average(g => g.TargetAmount > 0 ? (g.CurrentAmount / g.TargetAmount) * 100 : 0) 
                : 0;

            var displayName = member.ShowRealName 
                ? member.User?.UserName ?? "Unknown"
                : member.DisplayName ?? $"User {member.SavingsGroupMemberId}";

            if (group.AnonymousMembers)
            {
                displayName = $"User {member.SavingsGroupMemberId}";
            }

            leaderboard.Add(new LeaderboardEntry
            {
                DisplayName = displayName,
                CompletedGoals = completedGoals,
                TotalSaved = totalSaved,
                ActiveGoals = activeGoals,
                AverageProgress = averageProgress,
                IsCurrentUser = member.UserId == userId
            });
        }

        // Rank by average progress
        var rankedLeaderboard = leaderboard
            .OrderByDescending(l => l.AverageProgress)
            .Select((entry, index) =>
            {
                entry.Rank = index + 1;
                return entry;
            })
            .ToList();

        return rankedLeaderboard;
    }

    #endregion

    #region Community Comparison

    public async Task<CommunityStats> GetCommunityStatsAsync()
    {
        // Get all users who have opted in to community comparison
        var participatingUsers = await _context.UserPrivacySettings
            .Where(s => s.AllowCommunityComparison)
            .Select(s => s.UserId)
            .ToListAsync();

        if (!participatingUsers.Any())
        {
            return new CommunityStats();
        }

        var allGoals = await _context.Goals
            .Where(g => participatingUsers.Contains(g.UserId!))
            .ToListAsync();

        var userSavingsTotals = allGoals
            .GroupBy(g => g.UserId)
            .Select(g => g.Sum(goal => goal.CurrentAmount))
            .OrderBy(s => s)
            .ToList();

        var userGoalCounts = allGoals
            .GroupBy(g => g.UserId)
            .Select(g => g.Count())
            .ToList();

        var userCompletionRates = allGoals
            .GroupBy(g => g.UserId)
            .Select(g =>
            {
                var goals = g.ToList();
                var completed = goals.Count(goal => goal.CurrentAmount >= goal.TargetAmount);
                return goals.Count > 0 ? (decimal)completed / goals.Count * 100 : 0;
            })
            .ToList();

        return new CommunityStats
        {
            TotalUsers = participatingUsers.Count,
            TotalGoals = allGoals.Count,
            AverageSavings = userSavingsTotals.Any() ? userSavingsTotals.Average() : 0,
            MedianSavings = userSavingsTotals.Any() ? GetMedian(userSavingsTotals) : 0,
            AverageGoalCount = userGoalCounts.Any() ? (decimal)userGoalCounts.Average() : 0,
            AverageCompletionRate = userCompletionRates.Any() ? userCompletionRates.Average() : 0
        };
    }

    public async Task<ComparisonResult> CompareToCommunityAsync()
    {
        var userId = GetCurrentUserId();
        
        var privacySettings = await GetPrivacySettingsAsync(userId);
        if (!privacySettings.AllowCommunityComparison)
        {
            throw new InvalidOperationException("Community comparison is not enabled in privacy settings");
        }

        var communityStats = await GetCommunityStatsAsync();

        var userGoals = await _context.Goals
            .Where(g => g.UserId == userId)
            .ToListAsync();

        var userTotalSavings = userGoals.Sum(g => g.CurrentAmount);
        var userGoalCount = userGoals.Count;
        var userCompletionRate = userGoals.Any()
            ? (decimal)userGoals.Count(g => g.CurrentAmount >= g.TargetAmount) / userGoals.Count * 100
            : 0;

        // Calculate percentile rank
        var participatingUsers = await _context.UserPrivacySettings
            .Where(s => s.AllowCommunityComparison)
            .Select(s => s.UserId)
            .ToListAsync();

        var allUserSavings = await _context.Goals
            .Where(g => participatingUsers.Contains(g.UserId!))
            .GroupBy(g => g.UserId)
            .Select(g => g.Sum(goal => goal.CurrentAmount))
            .ToListAsync();

        var lowerCount = allUserSavings.Count(s => s < userTotalSavings);
        var percentileRank = allUserSavings.Any() ? (decimal)lowerCount / allUserSavings.Count * 100 : 50;

        return new ComparisonResult
        {
            UserTotalSavings = userTotalSavings,
            CommunityAverageSavings = communityStats.AverageSavings,
            PercentileRank = percentileRank,
            UserGoalCount = userGoalCount,
            CommunityAverageGoalCount = communityStats.AverageGoalCount,
            UserCompletionRate = userCompletionRate,
            CommunityAverageCompletionRate = communityStats.AverageCompletionRate
        };
    }

    private decimal GetMedian(List<decimal> values)
    {
        if (!values.Any())
            return 0;

        var sorted = values.OrderBy(v => v).ToList();
        var mid = sorted.Count / 2;

        if (sorted.Count % 2 == 0)
        {
            return (sorted[mid - 1] + sorted[mid]) / 2;
        }
        else
        {
            return sorted[mid];
        }
    }

    #endregion
}
