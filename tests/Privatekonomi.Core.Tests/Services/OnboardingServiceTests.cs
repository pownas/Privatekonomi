using Microsoft.AspNetCore.Identity;
using Moq;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests.Services;

public class OnboardingServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly OnboardingService _service;

    public OnboardingServiceTests()
    {
        // Setup UserManager mock
        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
        
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        
        _service = new OnboardingService(_mockUserManager.Object, _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task HasCompletedOnboardingAsync_WhenUserNotFound_ReturnsFalse()
    {
        // Arrange
        var userId = "test-user-id";
        _mockUserManager.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _service.HasCompletedOnboardingAsync(userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasCompletedOnboardingAsync_WhenOnboardingNotCompleted_ReturnsFalse()
    {
        // Arrange
        var userId = "test-user-id";
        var user = new ApplicationUser 
        { 
            Id = userId, 
            OnboardingCompleted = false 
        };
        
        _mockUserManager.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.HasCompletedOnboardingAsync(userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasCompletedOnboardingAsync_WhenOnboardingCompleted_ReturnsTrue()
    {
        // Arrange
        var userId = "test-user-id";
        var user = new ApplicationUser 
        { 
            Id = userId, 
            OnboardingCompleted = true,
            OnboardingCompletedAt = DateTime.UtcNow
        };
        
        _mockUserManager.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.HasCompletedOnboardingAsync(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CompleteOnboardingAsync_WhenUserExists_SetsOnboardingCompleted()
    {
        // Arrange
        var userId = "test-user-id";
        var user = new ApplicationUser 
        { 
            Id = userId, 
            OnboardingCompleted = false 
        };
        
        _mockUserManager.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync(user);
        
        _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _service.CompleteOnboardingAsync(userId);

        // Assert
        Assert.True(user.OnboardingCompleted);
        Assert.NotNull(user.OnboardingCompletedAt);
        _mockUserManager.Verify(m => m.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task CompleteOnboardingAsync_WhenUserNotFound_DoesNothing()
    {
        // Arrange
        var userId = "test-user-id";
        _mockUserManager.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        await _service.CompleteOnboardingAsync(userId);

        // Assert
        _mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task GetCurrentStepAsync_WhenOnboardingNotCompleted_ReturnsZero()
    {
        // Arrange
        var userId = "test-user-id";
        var user = new ApplicationUser 
        { 
            Id = userId, 
            OnboardingCompleted = false 
        };
        
        _mockUserManager.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetCurrentStepAsync(userId);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetCurrentStepAsync_WhenOnboardingCompleted_ReturnsMinusOne()
    {
        // Arrange
        var userId = "test-user-id";
        var user = new ApplicationUser 
        { 
            Id = userId, 
            OnboardingCompleted = true 
        };
        
        _mockUserManager.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetCurrentStepAsync(userId);

        // Assert
        Assert.Equal(-1, result);
    }
}
