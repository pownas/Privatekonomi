using Microsoft.AspNetCore.Identity;
using Moq;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Privatekonomi.Core.Tests.Services;

[TestClass]
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
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        
        _service = new OnboardingService(_mockUserManager.Object, _mockCurrentUserService.Object);
    }

    [TestMethod]
    public async Task HasCompletedOnboardingAsync_WhenUserNotFound_ReturnsFalse()
    {
        // Arrange
        var userId = "test-user-id";
        _mockUserManager.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _service.HasCompletedOnboardingAsync(userId);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
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
        Assert.IsFalse(result);
    }

    [TestMethod]
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
        Assert.IsTrue(result);
    }

    [TestMethod]
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
        Assert.IsTrue(user.OnboardingCompleted);
        Assert.IsNotNull(user.OnboardingCompletedAt);
        _mockUserManager.Verify(m => m.UpdateAsync(user), Times.Once);
    }

    [TestMethod]
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

    [TestMethod]
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
        Assert.AreEqual(0, result);
    }

    [TestMethod]
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
        Assert.AreEqual(-1, result);
    }
}
