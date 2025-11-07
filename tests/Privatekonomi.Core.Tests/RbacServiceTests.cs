using Microsoft.EntityFrameworkCore;
using Moq;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class RbacServiceTests : IDisposable
{
    private readonly PrivatekonomyContext _context;
    private readonly Mock<IAuditLogService> _mockAuditService;
    private readonly RbacService _rbacService;
    private readonly string _testUserId = "test-user-1";
    private readonly string _testUser2Id = "test-user-2";
    private readonly int _testHouseholdId = 1;

    public RbacServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PrivatekonomyContext(options);
        _mockAuditService = new Mock<IAuditLogService>();
        _rbacService = new RbacService(_context, _mockAuditService.Object);

        // Setup test data synchronously using GetAwaiter().GetResult() to avoid deadlocks
        SetupTestDataSync();
    }

    private void SetupTestDataSync()
    {
        SetupTestData().GetAwaiter().GetResult();
    }

    private async Task SetupTestData()
    {
        // Create household
        var household = new Household
        {
            HouseholdId = _testHouseholdId,
            Name = "Test Household",
            CreatedDate = DateTime.UtcNow
        };
        _context.Households.Add(household);

        // Create household member for test user 1 (Admin)
        var member1 = new HouseholdMember
        {
            HouseholdMemberId = 1,
            HouseholdId = _testHouseholdId,
            UserId = _testUserId,
            Name = "Admin User",
            Email = "admin@test.com",
            IsActive = true,
            JoinedDate = DateTime.UtcNow
        };
        _context.HouseholdMembers.Add(member1);

        // Create household member for test user 2
        var member2 = new HouseholdMember
        {
            HouseholdMemberId = 2,
            HouseholdId = _testHouseholdId,
            UserId = _testUser2Id,
            Name = "Regular User",
            Email = "user@test.com",
            IsActive = true,
            JoinedDate = DateTime.UtcNow
        };
        _context.HouseholdMembers.Add(member2);

        // Assign Admin role to user 1
        var adminRole = new HouseholdRole
        {
            HouseholdRoleId = 1,
            HouseholdMemberId = 1,
            RoleType = HouseholdRoleType.Admin,
            AssignedDate = DateTime.UtcNow,
            AssignedBy = "system",
            IsActive = true
        };
        _context.HouseholdRoles.Add(adminRole);

        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ==================== Role Management Tests ====================

    [Fact]
    public async Task GetUserRoleInHouseholdAsync_ReturnsAdminRole()
    {
        // Act
        var role = await _rbacService.GetUserRoleInHouseholdAsync(_testUserId, _testHouseholdId);

        // Assert
        Assert.NotNull(role);
        Assert.Equal(HouseholdRoleType.Admin, role.RoleType);
        Assert.True(role.IsActive);
    }

    [Fact]
    public async Task GetUserRoleInHouseholdAsync_ReturnsNullForNonMember()
    {
        // Act
        var role = await _rbacService.GetUserRoleInHouseholdAsync("non-existent-user", _testHouseholdId);

        // Assert
        Assert.Null(role);
    }

    [Fact]
    public async Task HasRoleAsync_ReturnsTrueForCorrectRole()
    {
        // Act
        var hasRole = await _rbacService.HasRoleAsync(_testUserId, _testHouseholdId, HouseholdRoleType.Admin);

        // Assert
        Assert.True(hasRole);
    }

    [Fact]
    public async Task HasRoleAsync_ReturnsFalseForIncorrectRole()
    {
        // Act
        var hasRole = await _rbacService.HasRoleAsync(_testUserId, _testHouseholdId, HouseholdRoleType.ViewOnly);

        // Assert
        Assert.False(hasRole);
    }

    [Fact]
    public async Task HasMinimumRoleAsync_ReturnsTrueForSameOrHigherRole()
    {
        // Admin should pass minimum role check for FullAccess, Editor, etc.
        // Act
        var hasMinRole = await _rbacService.HasMinimumRoleAsync(_testUserId, _testHouseholdId, HouseholdRoleType.FullAccess);

        // Assert
        Assert.True(hasMinRole);
    }

    [Fact]
    public async Task AssignRoleAsync_CreatesNewRoleSuccessfully()
    {
        // Arrange
        var targetMemberId = 2; // User 2

        // Act
        var newRole = await _rbacService.AssignRoleAsync(_testUserId, targetMemberId, HouseholdRoleType.Editor);

        // Assert
        Assert.NotNull(newRole);
        Assert.Equal(HouseholdRoleType.Editor, newRole.RoleType);
        Assert.True(newRole.IsActive);
        Assert.Equal(_testUserId, newRole.AssignedBy);
        
        // Verify audit log was called
        _mockAuditService.Verify(x => x.LogAsync(
            "RoleAssigned",
            "HouseholdRole",
            It.IsAny<int?>(),
            It.IsAny<string?>(),
            _testUserId,
            null), Times.Once);
    }

    [Fact]
    public async Task AssignRoleAsync_ThrowsExceptionWhenNonAdminTriesToAssign()
    {
        // Arrange - First assign Editor role to user 2
        await _rbacService.AssignRoleAsync(_testUserId, 2, HouseholdRoleType.Editor);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _rbacService.AssignRoleAsync(_testUser2Id, 1, HouseholdRoleType.FullAccess));
    }

    [Fact]
    public async Task AssignRoleAsync_RevokesExistingRoleWhenAssigningNew()
    {
        // Arrange
        var targetMemberId = 2;
        await _rbacService.AssignRoleAsync(_testUserId, targetMemberId, HouseholdRoleType.Editor);

        // Act - Assign a different role
        await _rbacService.AssignRoleAsync(_testUserId, targetMemberId, HouseholdRoleType.FullAccess);

        // Assert
        var roles = await _context.HouseholdRoles
            .Where(r => r.HouseholdMemberId == targetMemberId)
            .ToListAsync();

        var activeRoles = roles.Where(r => r.IsActive).ToList();
        var revokedRoles = roles.Where(r => !r.IsActive).ToList();

        Assert.Single(activeRoles);
        Assert.Equal(HouseholdRoleType.FullAccess, activeRoles[0].RoleType);
        Assert.Single(revokedRoles);
        Assert.Equal(HouseholdRoleType.Editor, revokedRoles[0].RoleType);
    }

    [Fact]
    public async Task TransferAdminRoleAsync_TransfersAdminRoleSuccessfully()
    {
        // Act
        var newAdminRole = await _rbacService.TransferAdminRoleAsync(_testUserId, _testUser2Id, _testHouseholdId);

        // Assert
        Assert.NotNull(newAdminRole);
        Assert.Equal(HouseholdRoleType.Admin, newAdminRole.RoleType);

        // Verify old admin now has Limited role
        var oldAdminRole = await _rbacService.GetUserRoleInHouseholdAsync(_testUserId, _testHouseholdId);
        Assert.NotNull(oldAdminRole);
        Assert.Equal(HouseholdRoleType.Limited, oldAdminRole.RoleType);

        // Verify new admin has Admin role
        var newRole = await _rbacService.GetUserRoleInHouseholdAsync(_testUser2Id, _testHouseholdId);
        Assert.NotNull(newRole);
        Assert.Equal(HouseholdRoleType.Admin, newRole.RoleType);
    }

    [Fact]
    public async Task RemoveRoleAsync_CannotRemoveLastAdminRole()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _rbacService.RemoveRoleAsync(_testUserId, 1));
    }

    // ==================== Permission Check Tests ====================

    [Fact]
    public async Task HasPermissionAsync_AdminHasAllPermissions()
    {
        // Act
        var hasTransactionPermission = await _rbacService.HasPermissionAsync(_testUserId, _testHouseholdId, "transaction.view.all");
        var hasBudgetPermission = await _rbacService.HasPermissionAsync(_testUserId, _testHouseholdId, "budget.edit.all");

        // Assert
        Assert.True(hasTransactionPermission);
        Assert.True(hasBudgetPermission);
    }

    [Fact]
    public async Task CanPerformActionAsync_RespectsAmountLimits()
    {
        // Arrange - Assign Editor role to user 2
        await _rbacService.AssignRoleAsync(_testUserId, 2, HouseholdRoleType.Editor);

        // Act
        var canCreateSmallTransaction = await _rbacService.CanPerformActionAsync(_testUser2Id, _testHouseholdId, "transaction.create", 1000m);
        var canCreateLargeTransaction = await _rbacService.CanPerformActionAsync(_testUser2Id, _testHouseholdId, "transaction.create", 10000m);

        // Assert
        Assert.True(canCreateSmallTransaction);
        Assert.False(canCreateLargeTransaction); // Editor limit is 5000 SEK
    }

    [Fact]
    public async Task CheckPermissionAsync_ReturnsCorrectAmountLimitForEditor()
    {
        // Arrange - Assign Editor role to user 2
        await _rbacService.AssignRoleAsync(_testUserId, 2, HouseholdRoleType.Editor);

        // Act
        var result = await _rbacService.CheckPermissionAsync(_testUser2Id, _testHouseholdId, "transaction.create");

        // Assert
        Assert.True(result.IsAllowed);
        Assert.Equal(5000m, result.AmountLimit);
        Assert.True(result.RequiresApproval);
    }

    // ==================== Delegation Tests ====================

    [Fact]
    public async Task DelegateRoleAsync_AdminCanDelegateFullAccess()
    {
        // Act
        var delegatedRole = await _rbacService.DelegateRoleAsync(
            _testUserId,
            _testUser2Id,
            _testHouseholdId,
            HouseholdRoleType.FullAccess,
            DateTime.UtcNow.AddDays(30));

        // Assert
        Assert.NotNull(delegatedRole);
        Assert.Equal(HouseholdRoleType.FullAccess, delegatedRole.RoleType);
        Assert.True(delegatedRole.IsDelegated);
        Assert.Equal(_testUserId, delegatedRole.DelegatedBy);
        Assert.NotNull(delegatedRole.DelegationEndDate);
    }

    [Fact]
    public async Task DelegateRoleAsync_ThrowsExceptionWhenExceedingMaxPeriod()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _rbacService.DelegateRoleAsync(
                _testUserId,
                _testUser2Id,
                _testHouseholdId,
                HouseholdRoleType.FullAccess,
                DateTime.UtcNow.AddDays(365))); // Max is 90 days for FullAccess
    }

    [Fact]
    public async Task DelegateRoleAsync_DelegatedRoleCannotDelegateFurther()
    {
        // Arrange - Create a delegation
        await _rbacService.DelegateRoleAsync(
            _testUserId,
            _testUser2Id,
            _testHouseholdId,
            HouseholdRoleType.FullAccess,
            DateTime.UtcNow.AddDays(30));

        // Create a third user
        var member3 = new HouseholdMember
        {
            HouseholdMemberId = 3,
            HouseholdId = _testHouseholdId,
            UserId = "test-user-3",
            Name = "Third User",
            IsActive = true,
            JoinedDate = DateTime.UtcNow
        };
        _context.HouseholdMembers.Add(member3);
        await _context.SaveChangesAsync();

        // Act & Assert - User 2 (with delegated role) tries to delegate to user 3
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _rbacService.DelegateRoleAsync(
                _testUser2Id,
                "test-user-3",
                _testHouseholdId,
                HouseholdRoleType.Editor,
                DateTime.UtcNow.AddDays(7)));
    }

    [Fact]
    public async Task RevokeDelegationAsync_RevokesSuccessfully()
    {
        // Arrange - Create a delegation
        var delegation = await _rbacService.DelegateRoleAsync(
            _testUserId,
            _testUser2Id,
            _testHouseholdId,
            HouseholdRoleType.Editor,
            DateTime.UtcNow.AddDays(30));

        // Act
        var result = await _rbacService.RevokeDelegationAsync(_testUserId, delegation.HouseholdRoleId);

        // Assert
        Assert.True(result);

        var revokedDelegation = await _context.HouseholdRoles.FindAsync(delegation.HouseholdRoleId);
        Assert.NotNull(revokedDelegation);
        Assert.False(revokedDelegation.IsActive);
        Assert.NotNull(revokedDelegation.RevokedDate);
    }

    [Fact]
    public async Task GetActiveDelegationsAsync_ReturnsOnlyActiveDelegations()
    {
        // Arrange - Create two delegations, revoke one
        var delegation1 = await _rbacService.DelegateRoleAsync(
            _testUserId,
            _testUser2Id,
            _testHouseholdId,
            HouseholdRoleType.Editor,
            DateTime.UtcNow.AddDays(30));

        var member3 = new HouseholdMember
        {
            HouseholdMemberId = 3,
            HouseholdId = _testHouseholdId,
            UserId = "test-user-3",
            Name = "Third User",
            IsActive = true,
            JoinedDate = DateTime.UtcNow
        };
        _context.HouseholdMembers.Add(member3);
        await _context.SaveChangesAsync();

        var delegation2 = await _rbacService.DelegateRoleAsync(
            _testUserId,
            "test-user-3",
            _testHouseholdId,
            HouseholdRoleType.ViewOnly,
            DateTime.UtcNow.AddDays(15));

        await _rbacService.RevokeDelegationAsync(_testUserId, delegation1.HouseholdRoleId);

        // Act
        var activeDelegations = await _rbacService.GetActiveDelegationsAsync(_testHouseholdId);

        // Assert
        Assert.Single(activeDelegations);
        Assert.Equal(delegation2.HouseholdRoleId, activeDelegations.First().HouseholdRoleId);
    }

    // ==================== Validation Tests ====================

    [Fact]
    public async Task ValidateRoleAssignmentAsync_FailsWhenNonAdminTriesToAssign()
    {
        // Arrange - Assign Editor role to user 2
        await _rbacService.AssignRoleAsync(_testUserId, 2, HouseholdRoleType.Editor);

        // Act
        var validation = await _rbacService.ValidateRoleAssignmentAsync(_testUser2Id, _testHouseholdId, 1, HouseholdRoleType.FullAccess);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Contains("Only Admin can assign roles", validation.Errors);
    }

    [Fact]
    public async Task ValidateRoleAssignmentAsync_WarnsWhenReplacingAdmin()
    {
        // Act
        var validation = await _rbacService.ValidateRoleAssignmentAsync(_testUserId, _testHouseholdId, 2, HouseholdRoleType.Admin);

        // Assert
        Assert.True(validation.IsValid);
        Assert.Contains(validation.Warnings, w => w.Contains("replace the existing admin"));
    }

    [Fact]
    public async Task ValidateRoleAssignmentAsync_RequiresDateOfBirthForChildRole()
    {
        // Act
        var validation = await _rbacService.ValidateRoleAssignmentAsync(_testUserId, _testHouseholdId, 2, HouseholdRoleType.Child);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Contains("Date of birth required for Child role", validation.Errors);
    }

    [Fact]
    public async Task ValidateHouseholdRolesAsync_PassesWithOneAdmin()
    {
        // Act
        var isValid = await _rbacService.ValidateHouseholdRolesAsync(_testHouseholdId);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task ValidateHouseholdRolesAsync_FailsWithNoAdmin()
    {
        // Arrange - Remove admin role
        var adminRole = await _context.HouseholdRoles.FindAsync(1);
        if (adminRole != null)
        {
            adminRole.IsActive = false;
            await _context.SaveChangesAsync();
        }

        // Act
        var isValid = await _rbacService.ValidateHouseholdRolesAsync(_testHouseholdId);

        // Assert
        Assert.False(isValid);
    }

    // ==================== Utility Tests ====================

    [Fact]
    public void CanDelegate_AdminCanDelegateAllExceptAdmin()
    {
        // Assert
        Assert.False(_rbacService.CanDelegate(HouseholdRoleType.Admin, HouseholdRoleType.Admin));
        Assert.True(_rbacService.CanDelegate(HouseholdRoleType.Admin, HouseholdRoleType.FullAccess));
        Assert.True(_rbacService.CanDelegate(HouseholdRoleType.Admin, HouseholdRoleType.Editor));
        Assert.True(_rbacService.CanDelegate(HouseholdRoleType.Admin, HouseholdRoleType.ViewOnly));
    }

    [Fact]
    public void CanDelegate_FullAccessCanDelegateLimitedRoles()
    {
        // Assert
        Assert.True(_rbacService.CanDelegate(HouseholdRoleType.FullAccess, HouseholdRoleType.Editor));
        Assert.True(_rbacService.CanDelegate(HouseholdRoleType.FullAccess, HouseholdRoleType.ViewOnly));
        Assert.True(_rbacService.CanDelegate(HouseholdRoleType.FullAccess, HouseholdRoleType.Limited));
        Assert.False(_rbacService.CanDelegate(HouseholdRoleType.FullAccess, HouseholdRoleType.FullAccess));
    }

    [Fact]
    public void CanDelegate_OtherRolesCannotDelegate()
    {
        // Assert
        Assert.False(_rbacService.CanDelegate(HouseholdRoleType.Editor, HouseholdRoleType.ViewOnly));
        Assert.False(_rbacService.CanDelegate(HouseholdRoleType.ViewOnly, HouseholdRoleType.Limited));
        Assert.False(_rbacService.CanDelegate(HouseholdRoleType.Limited, HouseholdRoleType.Child));
    }

    [Fact]
    public void GetMaxDelegationPeriod_ReturnsCorrectPeriods()
    {
        // Assert
        Assert.Equal(90, _rbacService.GetMaxDelegationPeriod(HouseholdRoleType.Admin, HouseholdRoleType.FullAccess));
        Assert.Equal(365, _rbacService.GetMaxDelegationPeriod(HouseholdRoleType.Admin, HouseholdRoleType.ViewOnly));
        Assert.Equal(30, _rbacService.GetMaxDelegationPeriod(HouseholdRoleType.FullAccess, HouseholdRoleType.Editor));
    }

    [Fact]
    public void RequiresApprovalForDelegation_ReturnsCorrectValue()
    {
        // Assert
        Assert.False(_rbacService.RequiresApprovalForDelegation(HouseholdRoleType.Admin, HouseholdRoleType.FullAccess));
        Assert.True(_rbacService.RequiresApprovalForDelegation(HouseholdRoleType.FullAccess, HouseholdRoleType.Editor));
        Assert.True(_rbacService.RequiresApprovalForDelegation(HouseholdRoleType.FullAccess, HouseholdRoleType.Limited));
        Assert.False(_rbacService.RequiresApprovalForDelegation(HouseholdRoleType.FullAccess, HouseholdRoleType.ViewOnly));
    }
}
