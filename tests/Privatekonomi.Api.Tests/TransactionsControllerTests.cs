using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Privatekonomi.Api.Models;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Xunit;

namespace Privatekonomi.Api.Tests;

public class TransactionsControllerTests
{
    private WebApplicationFactory<Program> CreateFactory(string databaseName)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext configuration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<PrivatekonomyContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database for testing
                    services.AddDbContext<PrivatekonomyContext>(options =>
                    {
                        options.UseInMemoryDatabase(databaseName);
                    });
                });
            });
    }

    private async Task<Transaction> CreateTestTransactionAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PrivatekonomyContext>();

        var transaction = new Transaction
        {
            Amount = 100m,
            Date = DateTime.UtcNow.Date,
            Description = "Test Transaction",
            Payee = "Test Payee",
            Notes = "Test Notes",
            Tags = "test,tag",
            IsLocked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        return transaction;
    }

    [Fact]
    public async Task UpdateTransaction_ValidRequest_ReturnsOkAndUpdatesTransaction()
    {
        // Arrange
        var factory = CreateFactory("Test_" + Guid.NewGuid());
        var client = factory.CreateClient();
        var transaction = await CreateTestTransactionAsync(factory.Services);

        var updateRequest = new UpdateTransactionRequest
        {
            Amount = 200m,
            Date = DateTime.UtcNow.Date.AddDays(1),
            Description = "Updated Description",
            Payee = "Updated Payee",
            Notes = "Updated Notes",
            Tags = "updated,tag",
            UpdatedAt = transaction.UpdatedAt
        };

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/transactions/{transaction.TransactionId}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedTransaction = await response.Content.ReadFromJsonAsync<Transaction>();
        Assert.NotNull(updatedTransaction);
        Assert.Equal(200m, updatedTransaction.Amount);
        Assert.Equal("Updated Description", updatedTransaction.Description);
        Assert.Equal("Updated Payee", updatedTransaction.Payee);
        Assert.Equal("Updated Notes", updatedTransaction.Notes);
        Assert.Equal("updated,tag", updatedTransaction.Tags);
    }

    [Fact]
    public async Task UpdateTransaction_InvalidAmount_ReturnsBadRequest()
    {
        // Arrange
        var factory = CreateFactory("Test_" + Guid.NewGuid());
        var client = factory.CreateClient();
        var transaction = await CreateTestTransactionAsync(factory.Services);

        var updateRequest = new UpdateTransactionRequest
        {
            Amount = 0m, // Invalid amount
            Date = DateTime.UtcNow.Date,
            Description = "Valid Description",
            UpdatedAt = transaction.UpdatedAt
        };

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/transactions/{transaction.TransactionId}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTransaction_EmptyDescription_ReturnsBadRequest()
    {
        // Arrange
        var factory = CreateFactory("Test_" + Guid.NewGuid());
        var client = factory.CreateClient();
        var transaction = await CreateTestTransactionAsync(factory.Services);

        var updateRequest = new UpdateTransactionRequest
        {
            Amount = 100m,
            Date = DateTime.UtcNow.Date,
            Description = "", // Invalid description
            UpdatedAt = transaction.UpdatedAt
        };

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/transactions/{transaction.TransactionId}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTransaction_LockedTransaction_ReturnsForbidden()
    {
        // Arrange
        var factory = CreateFactory("Test_" + Guid.NewGuid());
        var client = factory.CreateClient();
        
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PrivatekonomyContext>();

        var transaction = new Transaction
        {
            Amount = 100m,
            Date = DateTime.UtcNow.Date,
            Description = "Test Transaction",
            IsLocked = true, // Transaction is locked
            CreatedAt = DateTime.UtcNow
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var updateRequest = new UpdateTransactionRequest
        {
            Amount = 200m,
            Date = DateTime.UtcNow.Date,
            Description = "Updated Description"
        };

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/transactions/{transaction.TransactionId}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTransaction_ConcurrentModification_ReturnsConflict()
    {
        // Arrange
        var factory = CreateFactory("Test_" + Guid.NewGuid());
        var client = factory.CreateClient();
        var transaction = await CreateTestTransactionAsync(factory.Services);

        var updateRequest = new UpdateTransactionRequest
        {
            Amount = 200m,
            Date = DateTime.UtcNow.Date,
            Description = "Updated Description",
            UpdatedAt = DateTime.UtcNow.AddMinutes(-10) // Old timestamp
        };

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/transactions/{transaction.TransactionId}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTransaction_TransactionNotFound_ReturnsNotFound()
    {
        // Arrange
        var factory = CreateFactory("Test_" + Guid.NewGuid());
        var client = factory.CreateClient();
        
        var updateRequest = new UpdateTransactionRequest
        {
            Amount = 200m,
            Date = DateTime.UtcNow.Date,
            Description = "Updated Description"
        };

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/transactions/999999", // Non-existent ID
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTransaction_WithCategories_UpdatesCategoriesCorrectly()
    {
        // Arrange
        var factory = CreateFactory("Test_" + Guid.NewGuid());
        var client = factory.CreateClient();
        
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PrivatekonomyContext>();

        // Create categories
        var category1 = new Category { Name = "Category1" };
        var category2 = new Category { Name = "Category2" };
        context.Categories.AddRange(category1, category2);
        await context.SaveChangesAsync();

        // Create transaction
        var transaction = await CreateTestTransactionAsync(factory.Services);

        var updateRequest = new UpdateTransactionRequest
        {
            Amount = 100m,
            Date = DateTime.UtcNow.Date,
            Description = "Test Transaction",
            Categories = new List<TransactionCategoryDto>
            {
                new() { CategoryId = category1.CategoryId, Amount = 60m },
                new() { CategoryId = category2.CategoryId, Amount = 40m }
            },
            UpdatedAt = transaction.UpdatedAt
        };

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/transactions/{transaction.TransactionId}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify categories were updated in the database
        var updatedTransaction = await context.Transactions
            .Include(t => t.TransactionCategories)
            .FirstOrDefaultAsync(t => t.TransactionId == transaction.TransactionId);

        Assert.NotNull(updatedTransaction);
        Assert.Equal(2, updatedTransaction.TransactionCategories.Count);
        Assert.Contains(updatedTransaction.TransactionCategories, tc => tc.CategoryId == category1.CategoryId && tc.Amount == 60m);
        Assert.Contains(updatedTransaction.TransactionCategories, tc => tc.CategoryId == category2.CategoryId && tc.Amount == 40m);
    }

    [Fact]
    public async Task UpdateTransaction_WithoutOptimisticLocking_AllowsUpdate()
    {
        // Arrange
        var factory = CreateFactory("Test_" + Guid.NewGuid());
        var client = factory.CreateClient();
        var transaction = await CreateTestTransactionAsync(factory.Services);

        var updateRequest = new UpdateTransactionRequest
        {
            Amount = 200m,
            Date = DateTime.UtcNow.Date,
            Description = "Updated Description",
            // No UpdatedAt provided - optimistic locking is optional
        };

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/transactions/{transaction.TransactionId}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task QuickCategorize_ValidRequest_ReturnsOkAndCategorizes()
    {
        // Arrange
        var factory = CreateFactory("Test_QuickCategorize_" + Guid.NewGuid());
        var client = factory.CreateClient();
        
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PrivatekonomyContext>();

        // Create a category
        var category = new Category { Name = "Matvaror", Color = "#4CAF50" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        // Create transaction
        var transaction = await CreateTestTransactionAsync(factory.Services);

        var request = new QuickCategorizeRequest
        {
            CategoryId = category.CategoryId,
            CreateRule = false
        };

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/transactions/{transaction.TransactionId}/categorize",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<QuickCategorizeResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Transaction);
        Assert.Null(result.CreatedRule);
    }

    [Fact]
    public async Task QuickCategorize_WithCreateRule_CreatesRuleAndCategorizes()
    {
        // Arrange
        var factory = CreateFactory("Test_QuickCategorize_Rule_" + Guid.NewGuid());
        var client = factory.CreateClient();
        
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PrivatekonomyContext>();

        // Create a category
        var category = new Category { Name = "Transport", Color = "#2196F3" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        // Create transaction
        var transaction = await CreateTestTransactionAsync(factory.Services);

        var request = new QuickCategorizeRequest
        {
            CategoryId = category.CategoryId,
            CreateRule = true,
            RulePattern = "Test Payee"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/transactions/{transaction.TransactionId}/categorize",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<QuickCategorizeResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Transaction);
        Assert.NotNull(result.CreatedRule);
        Assert.Equal("Test Payee", result.CreatedRule!.Pattern);
        Assert.Equal(category.CategoryId, result.CreatedRule.CategoryId);
    }

    [Fact]
    public async Task QuickCategorize_TransactionNotFound_ReturnsNotFound()
    {
        // Arrange
        var factory = CreateFactory("Test_QuickCategorize_NotFound_" + Guid.NewGuid());
        var client = factory.CreateClient();
        
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PrivatekonomyContext>();

        // Create a category
        var category = new Category { Name = "Underhållning", Color = "#9C27B0" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var request = new QuickCategorizeRequest
        {
            CategoryId = category.CategoryId,
            CreateRule = false
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/transactions/999999/categorize", // Non-existent ID
            request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task QuickCategorize_CategoryNotFound_ReturnsNotFound()
    {
        // Arrange
        var factory = CreateFactory("Test_QuickCategorize_CategoryNotFound_" + Guid.NewGuid());
        var client = factory.CreateClient();
        
        // Create transaction
        var transaction = await CreateTestTransactionAsync(factory.Services);

        var request = new QuickCategorizeRequest
        {
            CategoryId = 999999, // Non-existent category
            CreateRule = false
        };

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/transactions/{transaction.TransactionId}/categorize",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
