using Microsoft.EntityFrameworkCore;
using Moq;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class SocialFeatureServiceTests : IDisposable
{
    private readonly PrivatekonomyContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly SocialFeatureService _socialFeatureService;
    private readonly string _testUserId = "test-user-id";

    public SocialFeatureServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PrivatekonomyContext(options);
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(_testUserId);

        _socialFeatureService = new SocialFeatureService(_context, _mockCurrentUserService.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private Goal CreateGoal(string name, string? userId = null, decimal targetAmount = 1000, decimal currentAmount = 0)
    {
        return new Goal
        {
            Name = name,
            UserId = userId ?? _testUserId,
            TargetAmount = targetAmount,
            CurrentAmount = currentAmount,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
    }

    #region Privacy Settings Tests

    [Fact]
    public async Task GetPrivacySettingsAsync_FirstTime_CreatesDefaultSettings()
    {
        // Act
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(_testUserId, settings.UserId);
        Assert.False(settings.EnableSocialFeatures); // Default is opt-out (GDPR compliant)
        Assert.False(settings.AllowGoalSharing);
        Assert.False(settings.AllowSavingsGroups);
        Assert.False(settings.AllowLeaderboards);
        Assert.False(settings.AllowCommunityComparison);
        Assert.True(settings.AnonymousByDefault); // Privacy-first
    }

    [Fact]
    public async Task UpdatePrivacySettingsAsync_ValidUpdate_UpdatesSettings()
    {
        // Arrange
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.EnableSocialFeatures = true;
        settings.AllowGoalSharing = true;

        // Act
        var updatedSettings = await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        // Assert
        Assert.True(updatedSettings.EnableSocialFeatures);
        Assert.True(updatedSettings.AllowGoalSharing);
        Assert.NotNull(updatedSettings.UpdatedAt);
    }

    [Fact]
    public async Task CanUseSocialFeaturesAsync_WhenDisabled_ReturnsFalse()
    {
        // Act
        var canUse = await _socialFeatureService.CanUseSocialFeaturesAsync(_testUserId);

        // Assert
        Assert.False(canUse);
    }

    [Fact]
    public async Task CanUseSocialFeaturesAsync_WhenEnabled_ReturnsTrue()
    {
        // Arrange
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.EnableSocialFeatures = true;
        await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        // Act
        var canUse = await _socialFeatureService.CanUseSocialFeaturesAsync(_testUserId);

        // Assert
        Assert.True(canUse);
    }

    #endregion

    #region Goal Sharing Tests

    [Fact]
    public async Task CreateGoalShareAsync_WhenSharingDisabled_ThrowsException()
    {
        // Arrange
        var goal = CreateGoal("Test Goal");
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        var shareSettings = new GoalShare { ShowCurrentAmount = true };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _socialFeatureService.CreateGoalShareAsync(goal.GoalId, shareSettings));
    }

    [Fact]
    public async Task CreateGoalShareAsync_WhenSharingEnabled_CreatesShare()
    {
        // Arrange
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.AllowGoalSharing = true;
        await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        var goal = CreateGoal("Test Goal", targetAmount: 1000, currentAmount: 500);
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        var shareSettings = new GoalShare 
        { 
            ShowCurrentAmount = true,
            ShowTargetAmount = true,
            ShowTargetDate = true
        };

        // Act
        var share = await _socialFeatureService.CreateGoalShareAsync(goal.GoalId, shareSettings);

        // Assert
        Assert.NotNull(share);
        Assert.Equal(goal.GoalId, share.GoalId);
        Assert.Equal(_testUserId, share.UserId);
        Assert.NotEmpty(share.ShareToken);
        Assert.True(share.IsActive);
        Assert.True(share.ShowCurrentAmount);
    }

    [Fact]
    public async Task GetGoalShareByTokenAsync_ValidToken_ReturnsShare()
    {
        // Arrange
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.AllowGoalSharing = true;
        await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        var goal = new Goal 
        { 
            Name = "Test Goal", 
            UserId = _testUserId,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        var share = await _socialFeatureService.CreateGoalShareAsync(goal.GoalId, new GoalShare());
        
        // Verify it was saved
        var savedShare = await _context.GoalShares.FindAsync(share.GoalShareId);
        Assert.NotNull(savedShare);

        // Act
        var retrievedShare = await _socialFeatureService.GetGoalShareByTokenAsync(share.ShareToken);

        // Assert
        Assert.NotNull(retrievedShare);
        Assert.Equal(share.ShareToken, retrievedShare.ShareToken);
    }

    [Fact]
    public async Task IncrementShareViewCountAsync_ValidToken_IncrementsCount()
    {
        // Arrange
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.AllowGoalSharing = true;
        await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        var goal = CreateGoal("Test Goal");
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        var share = await _socialFeatureService.CreateGoalShareAsync(goal.GoalId, new GoalShare());
        Assert.Equal(0, share.ViewCount);

        // Act
        await _socialFeatureService.IncrementShareViewCountAsync(share.ShareToken);
        await _socialFeatureService.IncrementShareViewCountAsync(share.ShareToken);

        // Assert
        var updatedShare = await _context.GoalShares.FindAsync(share.GoalShareId);
        Assert.Equal(2, updatedShare!.ViewCount);
    }

    [Fact]
    public async Task RevokeShareAsync_ValidShare_DeactivatesShare()
    {
        // Arrange
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.AllowGoalSharing = true;
        await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        var goal = CreateGoal("Test Goal");
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        var share = await _socialFeatureService.CreateGoalShareAsync(goal.GoalId, new GoalShare());

        // Act
        await _socialFeatureService.RevokeShareAsync(share.GoalShareId);

        // Assert
        var updatedShare = await _context.GoalShares.FindAsync(share.GoalShareId);
        Assert.False(updatedShare!.IsActive);
    }

    #endregion

    #region Savings Group Tests

    [Fact]
    public async Task CreateSavingsGroupAsync_WhenGroupsDisabled_ThrowsException()
    {
        // Arrange
        var group = new SavingsGroup { Name = "Test Group" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _socialFeatureService.CreateSavingsGroupAsync(group));
    }

    [Fact]
    public async Task CreateSavingsGroupAsync_WhenGroupsEnabled_CreatesGroupAndOwnerMember()
    {
        // Arrange
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.AllowSavingsGroups = true;
        await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        var group = new SavingsGroup 
        { 
            Name = "Test Group",
            Description = "A test savings group",
            GroupType = SavingsGroupType.Friends
        };

        // Act
        var createdGroup = await _socialFeatureService.CreateSavingsGroupAsync(group);

        // Assert
        Assert.NotNull(createdGroup);
        Assert.Equal(_testUserId, createdGroup.CreatedByUserId);
        Assert.True(createdGroup.IsActive);

        // Verify owner member was created
        var members = await _context.SavingsGroupMembers
            .Where(m => m.SavingsGroupId == createdGroup.SavingsGroupId)
            .ToListAsync();
        Assert.Single(members);
        Assert.Equal(GroupMemberRole.Owner, members[0].Role);
        Assert.Equal(GroupMemberStatus.Active, members[0].Status);
    }

    [Fact]
    public async Task GetUserGroupsAsync_ReturnsOnlyUserGroups()
    {
        // Arrange
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.AllowSavingsGroups = true;
        await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        var group1 = await _socialFeatureService.CreateSavingsGroupAsync(
            new SavingsGroup { Name = "User's Group" });

        // Create another group with different user
        var otherUserId = "other-user-id";
        var group2 = new SavingsGroup { Name = "Other's Group", CreatedByUserId = otherUserId, CreatedAt = DateTime.UtcNow };
        _context.SavingsGroups.Add(group2);
        await _context.SaveChangesAsync();

        // Act
        var userGroups = await _socialFeatureService.GetUserGroupsAsync();

        // Assert
        Assert.Single(userGroups);
        Assert.Contains(userGroups, g => g.SavingsGroupId == group1.SavingsGroupId);
    }

    #endregion

    #region Group Member Tests

    [Fact]
    public async Task InviteMemberAsync_ByOwner_CreatesInvitation()
    {
        // Arrange
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.AllowSavingsGroups = true;
        await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        var group = await _socialFeatureService.CreateSavingsGroupAsync(
            new SavingsGroup { Name = "Test Group" });

        var invitedUserId = "invited-user-id";
        var invitedSettings = new UserPrivacySettings 
        { 
            UserId = invitedUserId, 
            AllowSavingsGroups = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.UserPrivacySettings.Add(invitedSettings);
        await _context.SaveChangesAsync();

        // Act
        var member = await _socialFeatureService.InviteMemberAsync(
            group.SavingsGroupId, invitedUserId, GroupMemberRole.Member);

        // Assert
        Assert.NotNull(member);
        Assert.Equal(invitedUserId, member.UserId);
        Assert.Equal(GroupMemberStatus.Pending, member.Status);
        Assert.Equal(GroupMemberRole.Member, member.Role);
    }

    [Fact]
    public async Task AcceptGroupInvitationAsync_ValidInvitation_ActivatesMember()
    {
        // Arrange
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.AllowSavingsGroups = true;
        await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        var ownerUserId = "owner-user-id";
        var ownerSettings = new UserPrivacySettings 
        { 
            UserId = ownerUserId, 
            AllowSavingsGroups = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.UserPrivacySettings.Add(ownerSettings);
        await _context.SaveChangesAsync();

        var group = new SavingsGroup { Name = "Test Group", CreatedByUserId = ownerUserId, CreatedAt = DateTime.UtcNow };
        _context.SavingsGroups.Add(group);
        await _context.SaveChangesAsync();

        var ownerMember = new SavingsGroupMember
        {
            SavingsGroupId = group.SavingsGroupId,
            UserId = ownerUserId,
            Role = GroupMemberRole.Owner,
            Status = GroupMemberStatus.Active,
            JoinedAt = DateTime.UtcNow
        };
        _context.SavingsGroupMembers.Add(ownerMember);

        var member = new SavingsGroupMember
        {
            SavingsGroupId = group.SavingsGroupId,
            UserId = _testUserId,
            Role = GroupMemberRole.Member,
            Status = GroupMemberStatus.Pending,
            JoinedAt = DateTime.UtcNow
        };
        _context.SavingsGroupMembers.Add(member);
        await _context.SaveChangesAsync();

        // Act
        await _socialFeatureService.AcceptGroupInvitationAsync(group.SavingsGroupId);

        // Assert
        var updatedMember = await _context.SavingsGroupMembers
            .FirstOrDefaultAsync(m => m.SavingsGroupId == group.SavingsGroupId && m.UserId == _testUserId);
        Assert.Equal(GroupMemberStatus.Active, updatedMember!.Status);
    }

    #endregion

    #region Group Goal Tests

    [Fact]
    public async Task ShareGoalToGroupAsync_ValidGoal_SharesGoal()
    {
        // Arrange
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.AllowSavingsGroups = true;
        await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        var group = await _socialFeatureService.CreateSavingsGroupAsync(
            new SavingsGroup { Name = "Test Group" });

        var goal = new Goal { Name = "Test Goal", UserId = _testUserId, TargetAmount = 1000 };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        // Act
        var groupGoal = await _socialFeatureService.ShareGoalToGroupAsync(
            group.SavingsGroupId, goal.GoalId, isAnonymous: false);

        // Assert
        Assert.NotNull(groupGoal);
        Assert.Equal(group.SavingsGroupId, groupGoal.SavingsGroupId);
        Assert.Equal(goal.GoalId, groupGoal.GoalId);
        Assert.Equal(_testUserId, groupGoal.UserId);
        Assert.False(groupGoal.IsAnonymous);
        Assert.True(groupGoal.IsActive);
    }

    [Fact]
    public async Task GetGroupGoalsAsync_ReturnsActiveGoals()
    {
        // Arrange
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.AllowSavingsGroups = true;
        await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        var group = await _socialFeatureService.CreateSavingsGroupAsync(
            new SavingsGroup { Name = "Test Group" });

        var goal1 = CreateGoal("Goal 1");
        var goal2 = CreateGoal("Goal 2");
        _context.Goals.AddRange(goal1, goal2);
        await _context.SaveChangesAsync();

        await _socialFeatureService.ShareGoalToGroupAsync(group.SavingsGroupId, goal1.GoalId, false);
        await _socialFeatureService.ShareGoalToGroupAsync(group.SavingsGroupId, goal2.GoalId, true);

        // Act
        var groupGoals = await _socialFeatureService.GetGroupGoalsAsync(group.SavingsGroupId);

        // Assert
        Assert.Equal(2, groupGoals.Count());
    }

    #endregion

    #region Comment and Like Tests

    [Fact]
    public async Task AddCommentAsync_ValidComment_CreatesComment()
    {
        // Arrange
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.AllowSavingsGroups = true;
        await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        var group = await _socialFeatureService.CreateSavingsGroupAsync(
            new SavingsGroup { Name = "Test Group" });

        // Act
        var comment = await _socialFeatureService.AddCommentAsync(
            group.SavingsGroupId, "Great progress everyone!");

        // Assert
        Assert.NotNull(comment);
        Assert.Equal("Great progress everyone!", comment.Content);
        Assert.Equal(_testUserId, comment.UserId);
    }

    [Fact]
    public async Task LikeCommentAsync_ValidComment_CreatesLike()
    {
        // Arrange
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.AllowSavingsGroups = true;
        await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        var group = await _socialFeatureService.CreateSavingsGroupAsync(
            new SavingsGroup { Name = "Test Group" });

        var comment = await _socialFeatureService.AddCommentAsync(
            group.SavingsGroupId, "Test comment");

        // Act
        var like = await _socialFeatureService.LikeCommentAsync(comment.GroupCommentId);

        // Assert
        Assert.NotNull(like);
        Assert.Equal(comment.GroupCommentId, like.GroupCommentId);
        Assert.Equal(_testUserId, like.UserId);
    }

    [Fact]
    public async Task UnlikeCommentAsync_ValidLike_RemovesLike()
    {
        // Arrange
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.AllowSavingsGroups = true;
        await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        var group = await _socialFeatureService.CreateSavingsGroupAsync(
            new SavingsGroup { Name = "Test Group" });

        var comment = await _socialFeatureService.AddCommentAsync(
            group.SavingsGroupId, "Test comment");

        await _socialFeatureService.LikeCommentAsync(comment.GroupCommentId);

        // Act
        await _socialFeatureService.UnlikeCommentAsync(comment.GroupCommentId);

        // Assert
        var likes = await _context.CommentLikes
            .Where(l => l.GroupCommentId == comment.GroupCommentId)
            .ToListAsync();
        Assert.Empty(likes);
    }

    #endregion

    #region Leaderboard Tests

    [Fact]
    public async Task GetHouseholdLeaderboardAsync_WithMultipleMembers_ReturnsRankedLeaderboard()
    {
        // Arrange
        var household = new Household { Name = "Test Household", CreatedDate = DateTime.UtcNow };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var member1 = new HouseholdMember { HouseholdId = household.HouseholdId, UserId = _testUserId };
        _context.HouseholdMembers.Add(member1);
        await _context.SaveChangesAsync();

        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.AllowLeaderboards = true;
        await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        var goal1 = CreateGoal("Goal 1", targetAmount: 1000, currentAmount: 800);
        var goal2 = CreateGoal("Goal 2", targetAmount: 500, currentAmount: 500);
        _context.Goals.AddRange(goal1, goal2);
        await _context.SaveChangesAsync();

        // Act
        var leaderboard = await _socialFeatureService.GetHouseholdLeaderboardAsync(household.HouseholdId);

        // Assert
        var entries = leaderboard.ToList();
        Assert.Single(entries);
        Assert.Equal(1, entries[0].CompletedGoals);
        Assert.Equal(1300m, entries[0].TotalSaved);
        Assert.Equal(1, entries[0].ActiveGoals);
        Assert.True(entries[0].IsCurrentUser);
    }

    #endregion

    #region Community Comparison Tests

    [Fact]
    public async Task GetCommunityStatsAsync_WithParticipants_ReturnsStats()
    {
        // Arrange
        var user1Settings = new UserPrivacySettings 
        { 
            UserId = "user1", 
            AllowCommunityComparison = true,
            CreatedAt = DateTime.UtcNow
        };
        var user2Settings = new UserPrivacySettings 
        { 
            UserId = "user2", 
            AllowCommunityComparison = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.UserPrivacySettings.AddRange(user1Settings, user2Settings);

        var goal1 = CreateGoal("Goal 1", userId: "user1", targetAmount: 1000, currentAmount: 1000);
        var goal2 = CreateGoal("Goal 2", userId: "user2", targetAmount: 2000, currentAmount: 1500);
        _context.Goals.AddRange(goal1, goal2);
        await _context.SaveChangesAsync();

        // Act
        var stats = await _socialFeatureService.GetCommunityStatsAsync();

        // Assert
        Assert.Equal(2, stats.TotalUsers);
        Assert.Equal(2, stats.TotalGoals);
        Assert.True(stats.AverageSavings > 0);
    }

    [Fact]
    public async Task CompareToCommunityAsync_WhenDisabled_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _socialFeatureService.CompareToCommunityAsync());
    }

    [Fact]
    public async Task CompareToCommunityAsync_WhenEnabled_ReturnsComparison()
    {
        // Arrange
        var settings = await _socialFeatureService.GetPrivacySettingsAsync(_testUserId);
        settings.AllowCommunityComparison = true;
        await _socialFeatureService.UpdatePrivacySettingsAsync(settings);

        var goal = CreateGoal("Test Goal", targetAmount: 1000, currentAmount: 800);
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        // Act
        var comparison = await _socialFeatureService.CompareToCommunityAsync();

        // Assert
        Assert.NotNull(comparison);
        Assert.Equal(800m, comparison.UserTotalSavings);
        Assert.Equal(1, comparison.UserGoalCount);
    }

    #endregion
}
