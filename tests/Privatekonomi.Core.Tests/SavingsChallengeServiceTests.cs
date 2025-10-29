using Microsoft.EntityFrameworkCore;
using Moq;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class SavingsChallengeServiceTests : IDisposable
{
    private readonly PrivatekonomyContext _context;
    private readonly SavingsChallengeService _service;

    public SavingsChallengeServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PrivatekonomyContext(options);
        _service = new SavingsChallengeService(_context, null);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateChallengeAsync_ValidChallenge_SuccessfullyCreatesChallenge()
    {
        // Arrange
        var challenge = new SavingsChallenge
        {
            Name = "30-Day Savings Challenge",
            Description = "Save 100 kr per day for 30 days",
            Type = ChallengeType.SaveDaily,
            TargetAmount = 3000m,
            DurationDays = 30,
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = await _service.CreateChallengeAsync(challenge);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.SavingsChallengeId > 0);
        Assert.Equal(ChallengeStatus.Active, result.Status);
        Assert.NotNull(result.EndDate);
        Assert.Equal(30, (result.EndDate.Value - result.StartDate).Days);
    }

    [Fact]
    public async Task GetAllChallengesAsync_ReturnsChallenges()
    {
        // Arrange
        var challenge1 = new SavingsChallenge
        {
            Name = "Challenge 1",
            Type = ChallengeType.SaveDaily,
            TargetAmount = 1000m,
            DurationDays = 10,
            StartDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        var challenge2 = new SavingsChallenge
        {
            Name = "Challenge 2",
            Type = ChallengeType.NoRestaurant,
            TargetAmount = 2000m,
            DurationDays = 14,
            StartDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _service.CreateChallengeAsync(challenge1);
        await _service.CreateChallengeAsync(challenge2);

        // Act
        var result = await _service.GetAllChallengesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetActiveChallengesAsync_ReturnsOnlyActiveChallenges()
    {
        // Arrange
        var activeChallenge = new SavingsChallenge
        {
            Name = "Active Challenge",
            Type = ChallengeType.SaveDaily,
            TargetAmount = 1000m,
            DurationDays = 10,
            StartDate = DateTime.UtcNow,
            Status = ChallengeStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        var completedChallenge = new SavingsChallenge
        {
            Name = "Completed Challenge",
            Type = ChallengeType.NoRestaurant,
            TargetAmount = 2000m,
            DurationDays = 14,
            StartDate = DateTime.UtcNow.AddDays(-20),
            Status = ChallengeStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        await _service.CreateChallengeAsync(activeChallenge);
        await _service.CreateChallengeAsync(completedChallenge);

        // Act
        var result = await _service.GetActiveChallengesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Active Challenge", result.First().Name);
    }

    [Fact]
    public async Task RecordProgressAsync_ValidProgress_SuccessfullyRecordsProgress()
    {
        // Arrange
        var challenge = new SavingsChallenge
        {
            Name = "Test Challenge",
            Type = ChallengeType.SaveDaily,
            TargetAmount = 1000m,
            DurationDays = 10,
            StartDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _service.CreateChallengeAsync(challenge);

        // Act
        var progress = await _service.RecordProgressAsync(
            created.SavingsChallengeId,
            DateTime.UtcNow,
            true,
            100m,
            "First day completed");

        // Assert
        Assert.NotNull(progress);
        Assert.True(progress.Completed);
        Assert.Equal(100m, progress.AmountSaved);
        Assert.Equal("First day completed", progress.Notes);

        var updatedChallenge = await _service.GetChallengeByIdAsync(created.SavingsChallengeId);
        Assert.NotNull(updatedChallenge);
        Assert.Equal(100m, updatedChallenge.CurrentAmount);
    }

    [Fact]
    public async Task RecordProgressAsync_MultipleProgressEntries_UpdatesStreak()
    {
        // Arrange
        var challenge = new SavingsChallenge
        {
            Name = "Streak Test Challenge",
            Type = ChallengeType.SaveDaily,
            TargetAmount = 1000m,
            DurationDays = 10,
            StartDate = DateTime.UtcNow.AddDays(-3),
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        var created = await _service.CreateChallengeAsync(challenge);

        // Act - Record progress for 3 consecutive days
        await _service.RecordProgressAsync(created.SavingsChallengeId, DateTime.UtcNow.AddDays(-2), true, 100m);
        await _service.RecordProgressAsync(created.SavingsChallengeId, DateTime.UtcNow.AddDays(-1), true, 100m);
        await _service.RecordProgressAsync(created.SavingsChallengeId, DateTime.UtcNow, true, 100m);

        // Assert
        var updatedChallenge = await _service.GetChallengeByIdAsync(created.SavingsChallengeId);
        Assert.NotNull(updatedChallenge);
        Assert.True(updatedChallenge.CurrentStreak >= 1); // At least 1 day streak
        Assert.Equal(300m, updatedChallenge.CurrentAmount);
    }

    [Fact]
    public async Task UpdateChallengeStatusAsync_ValidStatus_SuccessfullyUpdatesStatus()
    {
        // Arrange
        var challenge = new SavingsChallenge
        {
            Name = "Status Test Challenge",
            Type = ChallengeType.SaveDaily,
            TargetAmount = 1000m,
            DurationDays = 10,
            StartDate = DateTime.UtcNow,
            Status = ChallengeStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _service.CreateChallengeAsync(challenge);

        // Act
        await _service.UpdateChallengeStatusAsync(created.SavingsChallengeId, ChallengeStatus.Completed);

        // Assert
        var updatedChallenge = await _service.GetChallengeByIdAsync(created.SavingsChallengeId);
        Assert.NotNull(updatedChallenge);
        Assert.Equal(ChallengeStatus.Completed, updatedChallenge.Status);
    }

    [Fact]
    public async Task GetTotalActiveChallengesAsync_ReturnsCorrectCount()
    {
        // Arrange
        await _service.CreateChallengeAsync(new SavingsChallenge
        {
            Name = "Active 1",
            Type = ChallengeType.SaveDaily,
            TargetAmount = 1000m,
            DurationDays = 10,
            StartDate = DateTime.UtcNow,
            Status = ChallengeStatus.Active,
            CreatedAt = DateTime.UtcNow
        });

        await _service.CreateChallengeAsync(new SavingsChallenge
        {
            Name = "Active 2",
            Type = ChallengeType.NoRestaurant,
            TargetAmount = 2000m,
            DurationDays = 14,
            StartDate = DateTime.UtcNow,
            Status = ChallengeStatus.Active,
            CreatedAt = DateTime.UtcNow
        });

        await _service.CreateChallengeAsync(new SavingsChallenge
        {
            Name = "Completed",
            Type = ChallengeType.NoCoffeeOut,
            TargetAmount = 500m,
            DurationDays = 7,
            StartDate = DateTime.UtcNow.AddDays(-10),
            Status = ChallengeStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        });

        // Act
        var count = await _service.GetTotalActiveChallengesAsync();

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetTotalAmountSavedAsync_ReturnsCorrectTotal()
    {
        // Arrange
        var challenge1 = await _service.CreateChallengeAsync(new SavingsChallenge
        {
            Name = "Challenge 1",
            Type = ChallengeType.SaveDaily,
            TargetAmount = 1000m,
            DurationDays = 10,
            StartDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        var challenge2 = await _service.CreateChallengeAsync(new SavingsChallenge
        {
            Name = "Challenge 2",
            Type = ChallengeType.NoRestaurant,
            TargetAmount = 2000m,
            DurationDays = 14,
            StartDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        await _service.RecordProgressAsync(challenge1.SavingsChallengeId, DateTime.UtcNow, true, 100m);
        await _service.RecordProgressAsync(challenge2.SavingsChallengeId, DateTime.UtcNow, true, 200m);

        // Act
        var total = await _service.GetTotalAmountSavedAsync();

        // Assert
        Assert.Equal(300m, total);
    }

    [Fact]
    public async Task DeleteChallengeAsync_ValidId_SuccessfullyDeletesChallenge()
    {
        // Arrange
        var challenge = new SavingsChallenge
        {
            Name = "Delete Test Challenge",
            Type = ChallengeType.SaveDaily,
            TargetAmount = 1000m,
            DurationDays = 10,
            StartDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _service.CreateChallengeAsync(challenge);

        // Act
        await _service.DeleteChallengeAsync(created.SavingsChallengeId);

        // Assert
        var deletedChallenge = await _service.GetChallengeByIdAsync(created.SavingsChallengeId);
        Assert.Null(deletedChallenge);
    }
}
