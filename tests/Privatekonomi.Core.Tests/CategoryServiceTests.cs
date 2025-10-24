using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class CategoryServiceTests : IDisposable
{
    private readonly PrivatekonomyContext _context;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PrivatekonomyContext(options);
        _categoryService = new CategoryService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateCategoryAsync_GeneratesRandomColorWhenNotProvided()
    {
        // Arrange
        var category = new Category
        {
            Name = "Test Category",
            Color = "#000000" // Default black should be replaced
        };

        // Act
        var result = await _categoryService.CreateCategoryAsync(category);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual("#000000", result.Color);
        Assert.StartsWith("#", result.Color);
        Assert.Equal(7, result.Color.Length); // #RRGGBB format
    }

    [Fact]
    public async Task CreateCategoryAsync_PreservesProvidedColor()
    {
        // Arrange
        var expectedColor = "#FF6B6B";
        var category = new Category
        {
            Name = "Test Category",
            Color = expectedColor
        };

        // Act
        var result = await _categoryService.CreateCategoryAsync(category);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedColor, result.Color);
    }

    [Fact]
    public async Task CreateCategoryAsync_SetsCreatedAt()
    {
        // Arrange
        var category = new Category
        {
            Name = "Test Category",
            Color = "#4ECDC4"
        };

        // Act
        var result = await _categoryService.CreateCategoryAsync(category);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(DateTime.MinValue, result.CreatedAt);
    }

    [Fact]
    public async Task CreateCategoryAsync_CreatesSubcategory()
    {
        // Arrange
        var parentCategory = new Category
        {
            Name = "Parent Category",
            Color = "#FF6B6B"
        };
        await _categoryService.CreateCategoryAsync(parentCategory);

        var subCategory = new Category
        {
            Name = "Sub Category",
            Color = "#4ECDC4",
            ParentId = parentCategory.CategoryId
        };

        // Act
        var result = await _categoryService.CreateCategoryAsync(subCategory);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(parentCategory.CategoryId, result.ParentId);
    }

    [Fact]
    public async Task UpdateCategoryAsync_UpdatesNameAndColor()
    {
        // Arrange
        var category = new Category
        {
            Name = "Original Name",
            Color = "#FF6B6B"
        };
        await _categoryService.CreateCategoryAsync(category);

        // Act
        category.Name = "Updated Name";
        category.Color = "#4ECDC4";
        var result = await _categoryService.UpdateCategoryAsync(category);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("#4ECDC4", result.Color);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task ResetSystemCategoryAsync_RestoresOriginalValues()
    {
        // Arrange
        var category = new Category
        {
            Name = "Modified Name",
            Color = "#000000",
            IsSystemCategory = true,
            OriginalName = "Original Name",
            OriginalColor = "#FF6B6B",
            CreatedAt = DateTime.UtcNow
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _categoryService.ResetSystemCategoryAsync(category.CategoryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Original Name", result.Name);
        Assert.Equal("#FF6B6B", result.Color);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task ResetSystemCategoryAsync_ReturnsNullForNonSystemCategory()
    {
        // Arrange
        var category = new Category
        {
            Name = "User Category",
            Color = "#4ECDC4",
            IsSystemCategory = false
        };
        await _categoryService.CreateCategoryAsync(category);

        // Act
        var result = await _categoryService.ResetSystemCategoryAsync(category.CategoryId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ResetSystemCategoryAsync_ReturnsNullForNonExistentCategory()
    {
        // Act
        var result = await _categoryService.ResetSystemCategoryAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsMainCategoriesWithSubcategories()
    {
        // Arrange
        var parentCategory = new Category
        {
            Name = "Parent",
            Color = "#FF6B6B"
        };
        await _categoryService.CreateCategoryAsync(parentCategory);

        var subCategory = new Category
        {
            Name = "Sub",
            Color = "#4ECDC4",
            ParentId = parentCategory.CategoryId
        };
        await _categoryService.CreateCategoryAsync(subCategory);

        // Act
        var result = await _categoryService.GetAllCategoriesAsync();

        // Assert
        var categories = result.ToList();
        Assert.Equal(2, categories.Count);
        
        var parent = categories.FirstOrDefault(c => c.Name == "Parent");
        Assert.NotNull(parent);
        Assert.Single(parent.SubCategories);
        Assert.Equal("Sub", parent.SubCategories.First().Name);
    }

    [Fact]
    public async Task DeleteCategoryAsync_RemovesCategory()
    {
        // Arrange
        var category = new Category
        {
            Name = "Test Category",
            Color = "#4ECDC4"
        };
        await _categoryService.CreateCategoryAsync(category);

        // Act
        await _categoryService.DeleteCategoryAsync(category.CategoryId);

        // Assert
        var result = await _categoryService.GetCategoryByIdAsync(category.CategoryId);
        Assert.Null(result);
    }
}
