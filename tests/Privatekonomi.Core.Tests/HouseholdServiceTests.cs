using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class HouseholdServiceTests : IDisposable
{
    private readonly PrivatekonomyContext _context;
    private readonly HouseholdService _householdService;

    public HouseholdServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PrivatekonomyContext(options);
        _householdService = new HouseholdService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region Activity Tests

    [Fact]
    public async Task CreateActivityAsync_CreatesActivitySuccessfully()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var activity = new HouseholdActivity
        {
            HouseholdId = household.HouseholdId,
            Title = "Cleaned the kitchen",
            Description = "Deep cleaning",
            Type = HouseholdActivityType.Cleaning
        };

        // Act
        var result = await _householdService.CreateActivityAsync(activity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Cleaned the kitchen", result.Title);
        Assert.Equal(HouseholdActivityType.Cleaning, result.Type);
        Assert.NotEqual(default, result.CreatedDate);
        Assert.NotEqual(default, result.CompletedDate);
    }

    [Fact]
    public async Task GetActivitiesAsync_ReturnsActivitiesOrderedByDate()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var activity1 = new HouseholdActivity
        {
            HouseholdId = household.HouseholdId,
            Title = "Activity 1",
            CompletedDate = DateTime.Now.AddDays(-2)
        };
        var activity2 = new HouseholdActivity
        {
            HouseholdId = household.HouseholdId,
            Title = "Activity 2",
            CompletedDate = DateTime.Now.AddDays(-1)
        };

        await _householdService.CreateActivityAsync(activity1);
        await _householdService.CreateActivityAsync(activity2);

        // Act
        var result = await _householdService.GetActivitiesAsync(household.HouseholdId);

        // Assert
        var activities = result.ToList();
        Assert.Equal(2, activities.Count);
        Assert.Equal("Activity 2", activities[0].Title); // Most recent first
        Assert.Equal("Activity 1", activities[1].Title);
    }

    [Fact]
    public async Task GetActivitiesAsync_FiltersWithDateRange()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var activity1 = new HouseholdActivity
        {
            HouseholdId = household.HouseholdId,
            Title = "Old Activity",
            CompletedDate = DateTime.Now.AddMonths(-2)
        };
        var activity2 = new HouseholdActivity
        {
            HouseholdId = household.HouseholdId,
            Title = "Recent Activity",
            CompletedDate = DateTime.Now
        };

        await _householdService.CreateActivityAsync(activity1);
        await _householdService.CreateActivityAsync(activity2);

        // Act
        var result = await _householdService.GetActivitiesAsync(
            household.HouseholdId,
            startDate: DateTime.Now.AddDays(-1)
        );

        // Assert
        var activities = result.ToList();
        Assert.Single(activities);
        Assert.Equal("Recent Activity", activities[0].Title);
    }

    [Fact]
    public async Task DeleteActivityAsync_RemovesActivity()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var activity = new HouseholdActivity
        {
            HouseholdId = household.HouseholdId,
            Title = "Test Activity"
        };
        var created = await _householdService.CreateActivityAsync(activity);

        // Act
        var result = await _householdService.DeleteActivityAsync(created.HouseholdActivityId);

        // Assert
        Assert.True(result);
        var activities = await _householdService.GetActivitiesAsync(household.HouseholdId);
        Assert.Empty(activities);
    }

    #endregion

    #region Task Tests

    [Fact]
    public async Task CreateTaskAsync_CreatesTaskSuccessfully()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var task = new HouseholdTask
        {
            HouseholdId = household.HouseholdId,
            Title = "Buy groceries",
            Description = "Milk, eggs, bread",
            Priority = HouseholdTaskPriority.High,
            DueDate = DateTime.Now.AddDays(1)
        };

        // Act
        var result = await _householdService.CreateTaskAsync(task);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Buy groceries", result.Title);
        Assert.Equal(HouseholdTaskPriority.High, result.Priority);
        Assert.False(result.IsCompleted);
        Assert.Null(result.CompletedDate);
        Assert.NotEqual(default, result.CreatedDate);
    }

    [Fact]
    public async Task GetTasksAsync_ReturnsTasksOrderedByPriority()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var task1 = new HouseholdTask
        {
            HouseholdId = household.HouseholdId,
            Title = "Low priority task",
            Priority = HouseholdTaskPriority.Low
        };
        var task2 = new HouseholdTask
        {
            HouseholdId = household.HouseholdId,
            Title = "High priority task",
            Priority = HouseholdTaskPriority.High
        };

        await _householdService.CreateTaskAsync(task1);
        await _householdService.CreateTaskAsync(task2);

        // Act
        var result = await _householdService.GetTasksAsync(household.HouseholdId);

        // Assert
        var tasks = result.ToList();
        Assert.Equal(2, tasks.Count);
        Assert.Equal("High priority task", tasks[0].Title); // Higher priority first
    }

    [Fact]
    public async Task GetTasksAsync_ExcludesCompletedTasksWhenRequested()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var task1 = new HouseholdTask
        {
            HouseholdId = household.HouseholdId,
            Title = "Incomplete task"
        };
        var task2 = new HouseholdTask
        {
            HouseholdId = household.HouseholdId,
            Title = "Completed task"
        };

        await _householdService.CreateTaskAsync(task1);
        var created = await _householdService.CreateTaskAsync(task2);
        await _householdService.MarkTaskCompleteAsync(created.HouseholdTaskId);

        // Act
        var result = await _householdService.GetTasksAsync(household.HouseholdId, includeCompleted: false);

        // Assert
        var tasks = result.ToList();
        Assert.Single(tasks);
        Assert.Equal("Incomplete task", tasks[0].Title);
    }

    [Fact]
    public async Task MarkTaskCompleteAsync_MarksTaskAsCompleted()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        var member = new HouseholdMember
        {
            HouseholdId = household.HouseholdId,
            Name = "John Doe"
        };
        _context.HouseholdMembers.Add(member);
        await _context.SaveChangesAsync();

        var task = new HouseholdTask
        {
            HouseholdId = household.HouseholdId,
            Title = "Test task"
        };
        var created = await _householdService.CreateTaskAsync(task);

        // Act
        var result = await _householdService.MarkTaskCompleteAsync(
            created.HouseholdTaskId,
            member.HouseholdMemberId
        );

        // Assert
        Assert.True(result);
        var updated = await _context.HouseholdTasks.FindAsync(created.HouseholdTaskId);
        Assert.NotNull(updated);
        Assert.True(updated.IsCompleted);
        Assert.NotNull(updated.CompletedDate);
        Assert.Equal(member.HouseholdMemberId, updated.CompletedByMemberId);
    }

    [Fact]
    public async Task MarkTaskIncompleteAsync_MarksTaskAsIncomplete()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var task = new HouseholdTask
        {
            HouseholdId = household.HouseholdId,
            Title = "Test task"
        };
        var created = await _householdService.CreateTaskAsync(task);
        await _householdService.MarkTaskCompleteAsync(created.HouseholdTaskId);

        // Act
        var result = await _householdService.MarkTaskIncompleteAsync(created.HouseholdTaskId);

        // Assert
        Assert.True(result);
        var updated = await _context.HouseholdTasks.FindAsync(created.HouseholdTaskId);
        Assert.NotNull(updated);
        Assert.False(updated.IsCompleted);
        Assert.Null(updated.CompletedDate);
        Assert.Null(updated.CompletedByMemberId);
    }

    [Fact]
    public async Task SearchTasksAsync_FindsTasksByTitle()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var task1 = new HouseholdTask
        {
            HouseholdId = household.HouseholdId,
            Title = "Buy groceries"
        };
        var task2 = new HouseholdTask
        {
            HouseholdId = household.HouseholdId,
            Title = "Clean house"
        };

        await _householdService.CreateTaskAsync(task1);
        await _householdService.CreateTaskAsync(task2);

        // Act
        var result = await _householdService.SearchTasksAsync(household.HouseholdId, "groceries");

        // Assert
        var tasks = result.ToList();
        Assert.Single(tasks);
        Assert.Equal("Buy groceries", tasks[0].Title);
    }

    [Fact]
    public async Task SearchTasksAsync_FindsTasksByDescription()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var task = new HouseholdTask
        {
            HouseholdId = household.HouseholdId,
            Title = "Shopping",
            Description = "Buy milk and eggs"
        };

        await _householdService.CreateTaskAsync(task);

        // Act
        var result = await _householdService.SearchTasksAsync(household.HouseholdId, "milk");

        // Assert
        var tasks = result.ToList();
        Assert.Single(tasks);
        Assert.Equal("Shopping", tasks[0].Title);
    }

    [Fact]
    public async Task DeleteTaskAsync_RemovesTask()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var task = new HouseholdTask
        {
            HouseholdId = household.HouseholdId,
            Title = "Test task"
        };
        var created = await _householdService.CreateTaskAsync(task);

        // Act
        var result = await _householdService.DeleteTaskAsync(created.HouseholdTaskId);

        // Assert
        Assert.True(result);
        var tasks = await _householdService.GetTasksAsync(household.HouseholdId);
        Assert.Empty(tasks);
    }

    #endregion
    
    #region Activity Image Tests

    [Fact]
    public async Task AddActivityImageAsync_AddsImageSuccessfully()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var activity = new HouseholdActivity
        {
            HouseholdId = household.HouseholdId,
            Title = "Test Activity",
            Type = HouseholdActivityType.General
        };
        var createdActivity = await _householdService.CreateActivityAsync(activity);

        // Act
        var image = await _householdService.AddActivityImageAsync(
            createdActivity.HouseholdActivityId,
            "test-image.jpg",
            "image/jpeg",
            1024000,
            "Test caption"
        );

        // Assert
        Assert.NotNull(image);
        Assert.Equal("test-image.jpg", image.ImagePath);
        Assert.Equal("image/jpeg", image.MimeType);
        Assert.Equal(1024000, image.FileSize);
        Assert.Equal("Test caption", image.Caption);
        Assert.Equal(1, image.DisplayOrder);
    }

    [Fact]
    public async Task AddActivityImageAsync_SetsCorrectDisplayOrder()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var activity = new HouseholdActivity
        {
            HouseholdId = household.HouseholdId,
            Title = "Test Activity",
            Type = HouseholdActivityType.General
        };
        var createdActivity = await _householdService.CreateActivityAsync(activity);

        // Act
        var image1 = await _householdService.AddActivityImageAsync(
            createdActivity.HouseholdActivityId,
            "image1.jpg",
            "image/jpeg",
            1024000
        );
        var image2 = await _householdService.AddActivityImageAsync(
            createdActivity.HouseholdActivityId,
            "image2.jpg",
            "image/jpeg",
            1024000
        );
        var image3 = await _householdService.AddActivityImageAsync(
            createdActivity.HouseholdActivityId,
            "image3.jpg",
            "image/jpeg",
            1024000
        );

        // Assert
        Assert.Equal(1, image1.DisplayOrder);
        Assert.Equal(2, image2.DisplayOrder);
        Assert.Equal(3, image3.DisplayOrder);
    }

    [Fact]
    public async Task GetActivityImagesAsync_ReturnsImagesInOrder()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var activity = new HouseholdActivity
        {
            HouseholdId = household.HouseholdId,
            Title = "Test Activity",
            Type = HouseholdActivityType.General
        };
        var createdActivity = await _householdService.CreateActivityAsync(activity);

        await _householdService.AddActivityImageAsync(
            createdActivity.HouseholdActivityId, "image3.jpg", "image/jpeg", 1024000
        );
        await _householdService.AddActivityImageAsync(
            createdActivity.HouseholdActivityId, "image1.jpg", "image/jpeg", 1024000
        );
        await _householdService.AddActivityImageAsync(
            createdActivity.HouseholdActivityId, "image2.jpg", "image/jpeg", 1024000
        );

        // Act
        var images = await _householdService.GetActivityImagesAsync(createdActivity.HouseholdActivityId);

        // Assert
        var imageList = images.ToList();
        Assert.Equal(3, imageList.Count);
        Assert.Equal("image3.jpg", imageList[0].ImagePath);
        Assert.Equal("image1.jpg", imageList[1].ImagePath);
        Assert.Equal("image2.jpg", imageList[2].ImagePath);
    }

    [Fact]
    public async Task DeleteActivityImageAsync_DeletesImageSuccessfully()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var activity = new HouseholdActivity
        {
            HouseholdId = household.HouseholdId,
            Title = "Test Activity",
            Type = HouseholdActivityType.General
        };
        var createdActivity = await _householdService.CreateActivityAsync(activity);
        var image = await _householdService.AddActivityImageAsync(
            createdActivity.HouseholdActivityId,
            "test-image.jpg",
            "image/jpeg",
            1024000
        );

        // Act
        var result = await _householdService.DeleteActivityImageAsync(image.HouseholdActivityImageId);

        // Assert
        Assert.True(result);
        var images = await _householdService.GetActivityImagesAsync(createdActivity.HouseholdActivityId);
        Assert.Empty(images);
    }

    [Fact]
    public async Task UpdateImageOrderAsync_UpdatesOrderSuccessfully()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var activity = new HouseholdActivity
        {
            HouseholdId = household.HouseholdId,
            Title = "Test Activity",
            Type = HouseholdActivityType.General
        };
        var createdActivity = await _householdService.CreateActivityAsync(activity);
        var image = await _householdService.AddActivityImageAsync(
            createdActivity.HouseholdActivityId,
            "test-image.jpg",
            "image/jpeg",
            1024000
        );

        // Act
        var result = await _householdService.UpdateImageOrderAsync(image.HouseholdActivityImageId, 5);

        // Assert
        Assert.True(result);
        var updatedImage = await _context.HouseholdActivityImages.FindAsync(image.HouseholdActivityImageId);
        Assert.NotNull(updatedImage);
        Assert.Equal(5, updatedImage.DisplayOrder);
    }

    [Fact]
    public async Task GetActivitiesAsync_IncludesImages()
    {
        // Arrange
        var household = new Household { Name = "Test Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var activity = new HouseholdActivity
        {
            HouseholdId = household.HouseholdId,
            Title = "Test Activity",
            Type = HouseholdActivityType.General
        };
        var createdActivity = await _householdService.CreateActivityAsync(activity);
        
        await _householdService.AddActivityImageAsync(
            createdActivity.HouseholdActivityId, "image1.jpg", "image/jpeg", 1024000
        );
        await _householdService.AddActivityImageAsync(
            createdActivity.HouseholdActivityId, "image2.jpg", "image/jpeg", 1024000
        );

        // Act
        var activities = await _householdService.GetActivitiesAsync(household.HouseholdId);

        // Assert
        var activityList = activities.ToList();
        Assert.Single(activityList);
        Assert.Equal(2, activityList[0].Images.Count);
    }

    #endregion
}
