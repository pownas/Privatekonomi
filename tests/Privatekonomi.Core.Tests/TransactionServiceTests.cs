using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class TransactionServiceTests : IDisposable
{
    private readonly PrivatekonomyContext _context;
    private readonly Mock<IAuditLogService> _mockAuditLogService;
    private readonly Mock<ICategoryRuleService> _mockCategoryRuleService;
    private readonly TransactionService _transactionService;

    public TransactionServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PrivatekonomyContext(options);
        _mockAuditLogService = new Mock<IAuditLogService>();
        _mockCategoryRuleService = new Mock<ICategoryRuleService>();

        _transactionService = new TransactionService(
            _context,
            _mockCategoryRuleService.Object,
            _mockAuditLogService.Object,
            null);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task UpdateTransactionWithAuditAsync_ValidUpdate_SuccessfullyUpdatesTransaction()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100m,
            Date = DateTime.UtcNow.Date,
            Description = "Original Description",
            Payee = "Original Payee",
            Notes = "Original Notes",
            Tags = "tag1,tag2",
            IsLocked = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _transactionService.UpdateTransactionWithAuditAsync(
            transaction.TransactionId,
            200m,
            DateTime.UtcNow.Date.AddDays(1),
            "Updated Description",
            "Updated Payee",
            "Updated Notes",
            "tag3,tag4",
            null,
            transaction.UpdatedAt,
            "testuser",
            "127.0.0.1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200m, result.Amount);
        Assert.Equal(DateTime.UtcNow.Date.AddDays(1), result.Date);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal("Updated Payee", result.Payee);
        Assert.Equal("Updated Notes", result.Notes);
        Assert.Equal("tag3,tag4", result.Tags);
        Assert.NotNull(result.UpdatedAt);

        // Verify audit log was called
        _mockAuditLogService.Verify(x => x.LogTransactionUpdateAsync(
            It.IsAny<Transaction>(),
            It.IsAny<Transaction>(),
            "testuser",
            "127.0.0.1"), Times.Once);
    }

    [Fact]
    public async Task UpdateTransactionWithAuditAsync_AmountZero_ThrowsArgumentException()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100m,
            Date = DateTime.UtcNow.Date,
            Description = "Test",
            IsLocked = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _transactionService.UpdateTransactionWithAuditAsync(
                transaction.TransactionId,
                0m, // Invalid amount
                DateTime.UtcNow.Date,
                "Description",
                null,
                null,
                null,
                null,
                null,
                null,
                null));
    }

    [Fact]
    public async Task UpdateTransactionWithAuditAsync_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100m,
            Date = DateTime.UtcNow.Date,
            Description = "Test",
            IsLocked = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _transactionService.UpdateTransactionWithAuditAsync(
                transaction.TransactionId,
                -50m, // Invalid amount
                DateTime.UtcNow.Date,
                "Description",
                null,
                null,
                null,
                null,
                null,
                null,
                null));
    }

    [Fact]
    public async Task UpdateTransactionWithAuditAsync_EmptyDescription_ThrowsArgumentException()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100m,
            Date = DateTime.UtcNow.Date,
            Description = "Test",
            IsLocked = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _transactionService.UpdateTransactionWithAuditAsync(
                transaction.TransactionId,
                100m,
                DateTime.UtcNow.Date,
                "", // Invalid description
                null,
                null,
                null,
                null,
                null,
                null,
                null));
    }

    [Fact]
    public async Task UpdateTransactionWithAuditAsync_LockedTransaction_ThrowsInvalidOperationException()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100m,
            Date = DateTime.UtcNow.Date,
            Description = "Test",
            IsLocked = true, // Transaction is locked
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _transactionService.UpdateTransactionWithAuditAsync(
                transaction.TransactionId,
                100m,
                DateTime.UtcNow.Date,
                "Description",
                null,
                null,
                null,
                null,
                null,
                null,
                null));

        Assert.Contains("locked", exception.Message);
    }

    [Fact]
    public async Task UpdateTransactionWithAuditAsync_ConcurrentModification_ThrowsInvalidOperationException()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100m,
            Date = DateTime.UtcNow.Date,
            Description = "Test",
            IsLocked = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow // Current timestamp
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _transactionService.UpdateTransactionWithAuditAsync(
                transaction.TransactionId,
                100m,
                DateTime.UtcNow.Date,
                "Description",
                null,
                null,
                null,
                null,
                DateTime.UtcNow.AddMinutes(-5), // Old timestamp indicating concurrent modification
                null,
                null));

        Assert.Contains("modified by another user", exception.Message);
    }

    [Fact]
    public async Task UpdateTransactionWithAuditAsync_TransactionNotFound_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _transactionService.UpdateTransactionWithAuditAsync(
                999, // Non-existent ID
                100m,
                DateTime.UtcNow.Date,
                "Description",
                null,
                null,
                null,
                null,
                null,
                null,
                null));

        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task UpdateTransactionWithAuditAsync_WithCategories_UpdatesCategoriesCorrectly()
    {
        // Arrange
        var category1 = new Category { CategoryId = 1, Name = "Category1" };
        var category2 = new Category { CategoryId = 2, Name = "Category2" };
        _context.Categories.AddRange(category1, category2);

        var transaction = new Transaction
        {
            Amount = 100m,
            Date = DateTime.UtcNow.Date,
            Description = "Test",
            IsLocked = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        var categories = new List<(int CategoryId, decimal Amount)>
        {
            (1, 60m),
            (2, 40m)
        };

        // Act
        var result = await _transactionService.UpdateTransactionWithAuditAsync(
            transaction.TransactionId,
            100m,
            DateTime.UtcNow.Date,
            "Description",
            null,
            null,
            null,
            categories,
            null,
            null,
            null);

        // Assert
        var updatedTransaction = await _context.Transactions
            .Include(t => t.TransactionCategories)
            .FirstOrDefaultAsync(t => t.TransactionId == transaction.TransactionId);

        Assert.NotNull(updatedTransaction);
        Assert.Equal(2, updatedTransaction.TransactionCategories.Count);
        Assert.Contains(updatedTransaction.TransactionCategories, tc => tc.CategoryId == 1 && tc.Amount == 60m);
        Assert.Contains(updatedTransaction.TransactionCategories, tc => tc.CategoryId == 2 && tc.Amount == 40m);
    }

    [Fact]
    public async Task UpdateTransactionWithAuditAsync_WithNullUpdatedAt_AllowsUpdate()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100m,
            Date = DateTime.UtcNow.Date,
            Description = "Test",
            IsLocked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act - No exception should be thrown when clientUpdatedAt is null
        var result = await _transactionService.UpdateTransactionWithAuditAsync(
            transaction.TransactionId,
            150m,
            DateTime.UtcNow.Date,
            "Updated Description",
            null,
            null,
            null,
            null,
            null, // No timestamp for optimistic locking
            null,
            null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(150m, result.Amount);
    }
}
