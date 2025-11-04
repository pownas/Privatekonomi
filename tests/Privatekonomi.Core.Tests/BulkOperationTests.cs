using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;

namespace Privatekonomi.Core.Tests;

public class BulkOperationTests
{
    private PrivatekonomyContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new PrivatekonomyContext(options);
    }

    [Fact]
    public async Task BulkDeleteTransactionsAsync_ShouldDeleteMultipleTransactions()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        var mockCategoryRuleService = new Mock<ICategoryRuleService>();
        var mockAuditLogService = new Mock<IAuditLogService>();

        var transactions = new List<Transaction>
        {
            new() { TransactionId = 1, Amount = 100, Description = "Test 1", Date = DateTime.Now },
            new() { TransactionId = 2, Amount = 200, Description = "Test 2", Date = DateTime.Now },
            new() { TransactionId = 3, Amount = 300, Description = "Test 3", Date = DateTime.Now }
        };

        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();

        var service = new TransactionService(context, mockCategoryRuleService.Object, 
            mockAuditLogService.Object, mockCurrentUserService.Object);

        // Act
        var result = await service.BulkDeleteTransactionsAsync(new List<int> { 1, 2 });

        // Assert
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.True(result.IsSuccess);
        Assert.Equal(1, await context.Transactions.CountAsync());
    }

    [Fact]
    public async Task BulkDeleteTransactionsAsync_ShouldNotDeleteLockedTransactions()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        var mockCategoryRuleService = new Mock<ICategoryRuleService>();
        var mockAuditLogService = new Mock<IAuditLogService>();

        var transactions = new List<Transaction>
        {
            new() { TransactionId = 1, Amount = 100, Description = "Test 1", Date = DateTime.Now, IsLocked = true },
            new() { TransactionId = 2, Amount = 200, Description = "Test 2", Date = DateTime.Now }
        };

        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();

        var service = new TransactionService(context, mockCategoryRuleService.Object, 
            mockAuditLogService.Object, mockCurrentUserService.Object);

        // Act
        var result = await service.BulkDeleteTransactionsAsync(new List<int> { 1, 2 });

        // Assert
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        Assert.True(result.IsPartialSuccess);
        Assert.Contains("locked", result.Errors[0]);
        Assert.Equal(1, await context.Transactions.CountAsync());
    }

    [Fact]
    public async Task BulkCategorizeTransactionsAsync_ShouldCategorizeMultipleTransactions()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        var mockCategoryRuleService = new Mock<ICategoryRuleService>();
        var mockAuditLogService = new Mock<IAuditLogService>();

        var category = new Category { CategoryId = 1, Name = "Food", Color = "#FF0000" };
        context.Categories.Add(category);

        var transactions = new List<Transaction>
        {
            new() { TransactionId = 1, Amount = 100, Description = "Test 1", Date = DateTime.Now },
            new() { TransactionId = 2, Amount = 200, Description = "Test 2", Date = DateTime.Now }
        };

        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();

        var service = new TransactionService(context, mockCategoryRuleService.Object, 
            mockAuditLogService.Object, mockCurrentUserService.Object);

        // Act
        var categories = new List<(int CategoryId, decimal? Amount)> { (1, null) };
        var result = await service.BulkCategorizeTransactionsAsync(new List<int> { 1, 2 }, categories);

        // Assert
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.True(result.IsSuccess);
        
        var tx1 = await context.Transactions.Include(t => t.TransactionCategories)
            .FirstAsync(t => t.TransactionId == 1);
        Assert.Single(tx1.TransactionCategories);
        Assert.Equal(1, tx1.TransactionCategories.First().CategoryId);
    }

    [Fact]
    public async Task BulkCategorizeTransactionsAsync_ShouldFailWithInvalidCategory()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        var mockCategoryRuleService = new Mock<ICategoryRuleService>();
        var mockAuditLogService = new Mock<IAuditLogService>();

        var transactions = new List<Transaction>
        {
            new() { TransactionId = 1, Amount = 100, Description = "Test 1", Date = DateTime.Now }
        };

        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();

        var service = new TransactionService(context, mockCategoryRuleService.Object, 
            mockAuditLogService.Object, mockCurrentUserService.Object);

        // Act
        var categories = new List<(int CategoryId, decimal? Amount)> { (999, null) };
        var result = await service.BulkCategorizeTransactionsAsync(new List<int> { 1 }, categories);

        // Assert
        Assert.Equal(0, result.SuccessCount);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("Invalid category", result.Errors[0]);
    }

    [Fact]
    public async Task BulkLinkToHouseholdAsync_ShouldLinkMultipleTransactions()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        var mockCategoryRuleService = new Mock<ICategoryRuleService>();
        var mockAuditLogService = new Mock<IAuditLogService>();

        var household = new Household { HouseholdId = 1, Name = "Test Household" };
        context.Households.Add(household);

        var transactions = new List<Transaction>
        {
            new() { TransactionId = 1, Amount = 100, Description = "Test 1", Date = DateTime.Now },
            new() { TransactionId = 2, Amount = 200, Description = "Test 2", Date = DateTime.Now }
        };

        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();

        var service = new TransactionService(context, mockCategoryRuleService.Object, 
            mockAuditLogService.Object, mockCurrentUserService.Object);

        // Act
        var result = await service.BulkLinkToHouseholdAsync(new List<int> { 1, 2 }, 1);

        // Assert
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.True(result.IsSuccess);
        
        var tx1 = await context.Transactions.FindAsync(1);
        Assert.Equal(1, tx1!.HouseholdId);
    }

    [Fact]
    public async Task BulkLinkToHouseholdAsync_ShouldUnlinkWhenHouseholdIdIsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        var mockCategoryRuleService = new Mock<ICategoryRuleService>();
        var mockAuditLogService = new Mock<IAuditLogService>();

        var household = new Household { HouseholdId = 1, Name = "Test Household" };
        context.Households.Add(household);

        var transactions = new List<Transaction>
        {
            new() { TransactionId = 1, Amount = 100, Description = "Test 1", Date = DateTime.Now, HouseholdId = 1 }
        };

        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();

        var service = new TransactionService(context, mockCategoryRuleService.Object, 
            mockAuditLogService.Object, mockCurrentUserService.Object);

        // Act
        var result = await service.BulkLinkToHouseholdAsync(new List<int> { 1 }, null);

        // Assert
        Assert.Equal(1, result.SuccessCount);
        Assert.True(result.IsSuccess);
        
        var tx1 = await context.Transactions.FindAsync(1);
        Assert.Null(tx1!.HouseholdId);
    }

    [Fact]
    public async Task CreateOperationSnapshotAsync_ShouldCreateSnapshot()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.UserId).Returns("user123");
        var mockCategoryRuleService = new Mock<ICategoryRuleService>();
        var mockAuditLogService = new Mock<IAuditLogService>();

        var category = new Category { CategoryId = 1, Name = "Food", Color = "#FF0000" };
        context.Categories.Add(category);

        var transaction = new Transaction 
        { 
            TransactionId = 1, 
            Amount = 100, 
            Description = "Test", 
            Date = DateTime.Now,
            HouseholdId = 5
        };
        context.Transactions.Add(transaction);

        var transactionCategory = new TransactionCategory
        {
            TransactionId = 1,
            CategoryId = 1,
            Amount = 100
        };
        context.TransactionCategories.Add(transactionCategory);
        
        await context.SaveChangesAsync();

        var service = new TransactionService(context, mockCategoryRuleService.Object, 
            mockAuditLogService.Object, mockCurrentUserService.Object);

        // Act
        var snapshot = await service.CreateOperationSnapshotAsync(new List<int> { 1 }, 
            BulkOperationType.Categorize);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(BulkOperationType.Categorize, snapshot.OperationType);
        Assert.Equal("user123", snapshot.UserId);
        Assert.Single(snapshot.AffectedTransactionIds);
        Assert.Single(snapshot.TransactionSnapshots!);
        Assert.Equal(5, snapshot.TransactionSnapshots![0].HouseholdId);
        Assert.Single(snapshot.TransactionSnapshots[0].CategoryIds);
        Assert.Equal(1, snapshot.TransactionSnapshots[0].CategoryIds[0]);
    }

    [Fact]
    public async Task UndoBulkOperationAsync_ShouldRestoreCategorization()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        var mockCategoryRuleService = new Mock<ICategoryRuleService>();
        var mockAuditLogService = new Mock<IAuditLogService>();

        var category1 = new Category { CategoryId = 1, Name = "Food", Color = "#FF0000" };
        var category2 = new Category { CategoryId = 2, Name = "Transport", Color = "#00FF00" };
        context.Categories.AddRange(category1, category2);

        var transaction = new Transaction 
        { 
            TransactionId = 1, 
            Amount = 100, 
            Description = "Test", 
            Date = DateTime.Now
        };
        context.Transactions.Add(transaction);

        var originalCategory = new TransactionCategory
        {
            TransactionId = 1,
            CategoryId = 1,
            Amount = 100
        };
        context.TransactionCategories.Add(originalCategory);
        
        await context.SaveChangesAsync();

        var service = new TransactionService(context, mockCategoryRuleService.Object, 
            mockAuditLogService.Object, mockCurrentUserService.Object);

        // Create snapshot before change
        var snapshot = await service.CreateOperationSnapshotAsync(new List<int> { 1 }, 
            BulkOperationType.Categorize);

        // Change categorization
        await service.BulkCategorizeTransactionsAsync(new List<int> { 1 }, 
            new List<(int CategoryId, decimal? Amount)> { (2, 100) });

        // Verify change
        var txAfterChange = await context.Transactions.Include(t => t.TransactionCategories)
            .FirstAsync(t => t.TransactionId == 1);
        Assert.Equal(2, txAfterChange.TransactionCategories.First().CategoryId);

        // Act - Undo
        var result = await service.UndoBulkOperationAsync(snapshot);

        // Assert
        Assert.Equal(1, result.SuccessCount);
        Assert.True(result.IsSuccess);
        
        var txAfterUndo = await context.Transactions.Include(t => t.TransactionCategories)
            .FirstAsync(t => t.TransactionId == 1);
        Assert.Equal(1, txAfterUndo.TransactionCategories.First().CategoryId);
    }

    [Fact]
    public async Task UndoBulkOperationAsync_ShouldNotUndoDelete()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        var mockCategoryRuleService = new Mock<ICategoryRuleService>();
        var mockAuditLogService = new Mock<IAuditLogService>();

        var service = new TransactionService(context, mockCategoryRuleService.Object, 
            mockAuditLogService.Object, mockCurrentUserService.Object);

        var snapshot = new BulkOperationSnapshot
        {
            OperationType = BulkOperationType.Delete,
            AffectedTransactionIds = new List<int> { 1, 2 }
        };

        // Act
        var result = await service.UndoBulkOperationAsync(snapshot);

        // Assert
        Assert.Equal(2, result.FailureCount);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("Cannot undo delete", result.Errors[0]);
    }

    [Fact]
    public async Task BulkDeleteTransactionsAsync_ShouldHandleLargeNumberOfTransactions()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        var mockCategoryRuleService = new Mock<ICategoryRuleService>();
        var mockAuditLogService = new Mock<IAuditLogService>();

        // Create 150 transactions to test performance requirement (100+ transactions)
        var transactions = Enumerable.Range(1, 150).Select(i => new Transaction
        {
            TransactionId = i,
            Amount = i * 10,
            Description = $"Test {i}",
            Date = DateTime.Now.AddDays(-i)
        }).ToList();

        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();

        var service = new TransactionService(context, mockCategoryRuleService.Object, 
            mockAuditLogService.Object, mockCurrentUserService.Object);

        var transactionIds = Enumerable.Range(1, 150).ToList();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await service.BulkDeleteTransactionsAsync(transactionIds);
        stopwatch.Stop();

        // Assert
        Assert.Equal(150, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.True(result.IsSuccess);
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, 
            $"Bulk delete took {stopwatch.ElapsedMilliseconds}ms, should be < 2000ms");
        Assert.Equal(0, await context.Transactions.CountAsync());
    }
}
