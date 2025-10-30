using Microsoft.EntityFrameworkCore;
using Moq;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class DashboardLayoutServiceTests
{
    private readonly PrivatekonomyContext _context;
    private readonly DashboardLayoutService _service;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private const string TestUserId = "test-user-123";

    public DashboardLayoutServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new PrivatekonomyContext(options);

        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockCurrentUserService.Setup(x => x.UserId).Returns(TestUserId);
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);

        _service = new DashboardLayoutService(_context, _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task GetUserLayoutsAsync_ReturnsUserLayouts()
    {
        // Arrange
        var layout1 = new DashboardLayout
        {
            UserId = TestUserId,
            Name = "Hem",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var layout2 = new DashboardLayout
        {
            UserId = TestUserId,
            Name = "Investeringar",
            IsDefault = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var layout3 = new DashboardLayout
        {
            UserId = "other-user",
            Name = "Other",
            IsDefault = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.DashboardLayouts.AddRange(layout1, layout2, layout3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUserLayoutsAsync(TestUserId);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, l => Assert.Equal(TestUserId, l.UserId));
    }

    [Fact]
    public async Task GetDefaultLayoutAsync_ReturnsDefaultLayout()
    {
        // Arrange
        var defaultLayout = new DashboardLayout
        {
            UserId = TestUserId,
            Name = "Hem",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var otherLayout = new DashboardLayout
        {
            UserId = TestUserId,
            Name = "Budget",
            IsDefault = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.DashboardLayouts.AddRange(defaultLayout, otherLayout);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetDefaultLayoutAsync(TestUserId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsDefault);
        Assert.Equal("Hem", result.Name);
    }

    [Fact]
    public async Task GetDefaultLayoutAsync_CreatesDefaultWhenNoneExists()
    {
        // Act
        var result = await _service.GetDefaultLayoutAsync(TestUserId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsDefault);
        Assert.Equal("Hem", result.Name);
        Assert.NotEmpty(result.Widgets);
    }

    [Fact]
    public async Task CreateLayoutAsync_CreatesNewLayout()
    {
        // Arrange
        var newLayout = new DashboardLayout
        {
            UserId = TestUserId,
            Name = "Test Layout",
            IsDefault = false,
            Widgets = new List<WidgetConfiguration>
            {
                new WidgetConfiguration
                {
                    Type = WidgetType.NetWorth,
                    Row = 0,
                    Column = 0,
                    Width = 12,
                    Height = 2
                }
            }
        };

        // Act
        var result = await _service.CreateLayoutAsync(newLayout);

        // Assert
        Assert.NotEqual(0, result.LayoutId);
        Assert.Equal("Test Layout", result.Name);
        Assert.Single(result.Widgets);
        Assert.NotEqual(DateTime.MinValue, result.CreatedAt);
        Assert.NotEqual(DateTime.MinValue, result.UpdatedAt);
    }

    [Fact]
    public async Task CreateLayoutAsync_SetsCurrentUserIdWhenAuthenticated()
    {
        // Arrange
        var newLayout = new DashboardLayout
        {
            UserId = "", // Empty, should be set by service
            Name = "Test Layout",
            IsDefault = false
        };

        // Act
        var result = await _service.CreateLayoutAsync(newLayout);

        // Assert
        Assert.Equal(TestUserId, result.UserId);
    }

    [Fact]
    public async Task CreateLayoutAsync_UnsetsOtherDefaultsWhenCreatingDefault()
    {
        // Arrange
        var existingDefault = new DashboardLayout
        {
            UserId = TestUserId,
            Name = "Old Default",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.DashboardLayouts.Add(existingDefault);
        await _context.SaveChangesAsync();

        var newDefault = new DashboardLayout
        {
            UserId = TestUserId,
            Name = "New Default",
            IsDefault = true
        };

        // Act
        await _service.CreateLayoutAsync(newDefault);

        // Assert
        var oldDefault = await _context.DashboardLayouts.FindAsync(existingDefault.LayoutId);
        Assert.NotNull(oldDefault);
        Assert.False(oldDefault.IsDefault);
    }

    [Fact]
    public async Task UpdateLayoutAsync_UpdatesExistingLayout()
    {
        // Arrange
        var layout = new DashboardLayout
        {
            UserId = TestUserId,
            Name = "Original Name",
            IsDefault = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.DashboardLayouts.Add(layout);
        await _context.SaveChangesAsync();

        layout.Name = "Updated Name";

        // Act
        var result = await _service.UpdateLayoutAsync(layout);

        // Assert
        Assert.Equal("Updated Name", result.Name);
        var updated = await _context.DashboardLayouts.FindAsync(layout.LayoutId);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
    }

    [Fact]
    public async Task DeleteLayoutAsync_DeletesLayout()
    {
        // Arrange
        var layout = new DashboardLayout
        {
            UserId = TestUserId,
            Name = "To Delete",
            IsDefault = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.DashboardLayouts.Add(layout);
        await _context.SaveChangesAsync();
        var layoutId = layout.LayoutId;

        // Act
        await _service.DeleteLayoutAsync(layoutId);

        // Assert
        var deleted = await _context.DashboardLayouts.FindAsync(layoutId);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteLayoutAsync_SetsAnotherAsDefaultWhenDeletingDefault()
    {
        // Arrange
        var defaultLayout = new DashboardLayout
        {
            UserId = TestUserId,
            Name = "Default",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var otherLayout = new DashboardLayout
        {
            UserId = TestUserId,
            Name = "Other",
            IsDefault = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.DashboardLayouts.AddRange(defaultLayout, otherLayout);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteLayoutAsync(defaultLayout.LayoutId);

        // Assert
        var updated = await _context.DashboardLayouts.FindAsync(otherLayout.LayoutId);
        Assert.NotNull(updated);
        Assert.True(updated.IsDefault);
    }

    [Fact]
    public async Task SetDefaultLayoutAsync_SetsLayoutAsDefault()
    {
        // Arrange
        var layout1 = new DashboardLayout
        {
            UserId = TestUserId,
            Name = "Layout 1",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var layout2 = new DashboardLayout
        {
            UserId = TestUserId,
            Name = "Layout 2",
            IsDefault = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.DashboardLayouts.AddRange(layout1, layout2);
        await _context.SaveChangesAsync();

        // Act
        await _service.SetDefaultLayoutAsync(layout2.LayoutId, TestUserId);

        // Assert
        var updated1 = await _context.DashboardLayouts.FindAsync(layout1.LayoutId);
        var updated2 = await _context.DashboardLayouts.FindAsync(layout2.LayoutId);
        Assert.NotNull(updated1);
        Assert.NotNull(updated2);
        Assert.False(updated1.IsDefault);
        Assert.True(updated2.IsDefault);
    }

    [Fact]
    public async Task CreateDefaultLayoutForUserAsync_CreatesLayoutWithWidgets()
    {
        // Act
        var result = await _service.CreateDefaultLayoutForUserAsync(TestUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestUserId, result.UserId);
        Assert.Equal("Hem", result.Name);
        Assert.True(result.IsDefault);
        Assert.NotEmpty(result.Widgets);
        Assert.True(result.Widgets.Count >= 5); // Should have at least 5 default widgets
    }

    [Fact]
    public async Task GetLayoutByIdAsync_ReturnsCorrectLayout()
    {
        // Arrange
        var layout = new DashboardLayout
        {
            UserId = TestUserId,
            Name = "Test Layout",
            IsDefault = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Widgets = new List<WidgetConfiguration>
            {
                new WidgetConfiguration
                {
                    Type = WidgetType.CashFlow,
                    Row = 0,
                    Column = 0,
                    Width = 6,
                    Height = 2
                }
            }
        };
        _context.DashboardLayouts.Add(layout);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetLayoutByIdAsync(layout.LayoutId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(layout.LayoutId, result.LayoutId);
        Assert.Equal("Test Layout", result.Name);
        Assert.Single(result.Widgets);
    }
}
