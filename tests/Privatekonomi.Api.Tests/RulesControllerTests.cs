using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Xunit;

namespace Privatekonomi.Api.Tests;

/// <summary>
/// Unit tests for the RulesController API endpoints.
/// Tests the /api/rules endpoint which provides user categorization rules.
/// </summary>
public class RulesControllerTests
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

    private async Task<Category> CreateTestCategoryAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PrivatekonomyContext>();

        var category = new Category
        {
            Name = "Test Category",
            Color = "#FF0000",
            IsSystemCategory = false,
            TaxRelated = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return category;
    }

    private async Task<CategoryRule> CreateTestRuleAsync(IServiceProvider services, int categoryId)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PrivatekonomyContext>();

        var rule = new CategoryRule
        {
            Pattern = "Test Pattern",
            MatchType = PatternMatchType.Contains,
            CategoryId = categoryId,
            Priority = 50,
            IsActive = true,
            CaseSensitive = false,
            Field = MatchField.Both,
            RuleType = RuleType.User,
            CreatedAt = DateTime.UtcNow
        };

        context.CategoryRules.Add(rule);
        await context.SaveChangesAsync();

        return rule;
    }

    [Fact]
    public async Task GetAllRules_ReturnsEmptyList_WhenNoRulesExist()
    {
        // Arrange
        await using var factory = CreateFactory($"RulesTest_{Guid.NewGuid()}");
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/rules");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rules = await response.Content.ReadFromJsonAsync<List<CategoryRule>>();
        Assert.NotNull(rules);
        Assert.Empty(rules);
    }

    [Fact]
    public async Task GetAllRules_ReturnsList_WhenRulesExist()
    {
        // Arrange
        await using var factory = CreateFactory($"RulesTest_{Guid.NewGuid()}");
        var client = factory.CreateClient();
        
        var category = await CreateTestCategoryAsync(factory.Services);
        await CreateTestRuleAsync(factory.Services, category.CategoryId);

        // Act
        var response = await client.GetAsync("/api/rules");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rules = await response.Content.ReadFromJsonAsync<List<CategoryRule>>();
        Assert.NotNull(rules);
        Assert.Single(rules);
    }

    [Fact]
    public async Task GetActiveRules_ReturnsOnlyActiveRules()
    {
        // Arrange
        await using var factory = CreateFactory($"RulesTest_{Guid.NewGuid()}");
        var client = factory.CreateClient();
        
        var category = await CreateTestCategoryAsync(factory.Services);
        
        // Create an active rule
        await CreateTestRuleAsync(factory.Services, category.CategoryId);
        
        // Create an inactive rule
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PrivatekonomyContext>();
            var inactiveRule = new CategoryRule
            {
                Pattern = "Inactive Pattern",
                MatchType = PatternMatchType.Contains,
                CategoryId = category.CategoryId,
                Priority = 30,
                IsActive = false,
                CaseSensitive = false,
                Field = MatchField.Both,
                RuleType = RuleType.User,
                CreatedAt = DateTime.UtcNow
            };
            context.CategoryRules.Add(inactiveRule);
            await context.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync("/api/rules/active");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rules = await response.Content.ReadFromJsonAsync<List<CategoryRule>>();
        Assert.NotNull(rules);
        Assert.Single(rules);
        Assert.True(rules.All(r => r.IsActive));
    }

    [Fact]
    public async Task GetRule_ReturnsNotFound_WhenRuleDoesNotExist()
    {
        // Arrange
        await using var factory = CreateFactory($"RulesTest_{Guid.NewGuid()}");
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/rules/9999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetRule_ReturnsRule_WhenRuleExists()
    {
        // Arrange
        await using var factory = CreateFactory($"RulesTest_{Guid.NewGuid()}");
        var client = factory.CreateClient();
        
        var category = await CreateTestCategoryAsync(factory.Services);
        var createdRule = await CreateTestRuleAsync(factory.Services, category.CategoryId);

        // Act
        var response = await client.GetAsync($"/api/rules/{createdRule.CategoryRuleId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rule = await response.Content.ReadFromJsonAsync<CategoryRule>();
        Assert.NotNull(rule);
        Assert.Equal(createdRule.Pattern, rule.Pattern);
    }

    [Fact]
    public async Task CreateRule_CreatesUserRule()
    {
        // Arrange
        await using var factory = CreateFactory($"RulesTest_{Guid.NewGuid()}");
        var client = factory.CreateClient();
        
        var category = await CreateTestCategoryAsync(factory.Services);

        var newRule = new CategoryRule
        {
            Pattern = "ICA",
            MatchType = PatternMatchType.Contains,
            CategoryId = category.CategoryId,
            Priority = 100,
            IsActive = true,
            CaseSensitive = false,
            Field = MatchField.Both,
            Description = "Rule for ICA transactions"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/rules", newRule);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdRule = await response.Content.ReadFromJsonAsync<CategoryRule>();
        Assert.NotNull(createdRule);
        Assert.Equal("ICA", createdRule.Pattern);
        Assert.Equal(RuleType.User, createdRule.RuleType);
    }

    [Fact]
    public async Task UpdateRule_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        await using var factory = CreateFactory($"RulesTest_{Guid.NewGuid()}");
        var client = factory.CreateClient();
        
        var category = await CreateTestCategoryAsync(factory.Services);
        var existingRule = await CreateTestRuleAsync(factory.Services, category.CategoryId);

        // Create a rule with mismatched ID in URL vs body
        var updatePayload = new
        {
            CategoryRuleId = existingRule.CategoryRuleId + 1, // Mismatch
            Pattern = "Updated Pattern",
            CategoryId = existingRule.CategoryId,
            Priority = 75,
            IsActive = true,
            CaseSensitive = false,
            MatchType = 1, // Contains
            Field = 2, // Both
            RuleType = 1, // User
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/rules/{existingRule.CategoryRuleId}", updatePayload);

        // Assert - should fail due to ID mismatch
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRule_RemovesRule()
    {
        // Arrange
        await using var factory = CreateFactory($"RulesTest_{Guid.NewGuid()}");
        var client = factory.CreateClient();
        
        var category = await CreateTestCategoryAsync(factory.Services);
        var existingRule = await CreateTestRuleAsync(factory.Services, category.CategoryId);

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/rules/{existingRule.CategoryRuleId}");
        var getResponse = await client.GetAsync($"/api/rules/{existingRule.CategoryRuleId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task TestRule_ReturnsMatchingRule()
    {
        // Arrange
        await using var factory = CreateFactory($"RulesTest_{Guid.NewGuid()}");
        var client = factory.CreateClient();
        
        var category = await CreateTestCategoryAsync(factory.Services);
        await CreateTestRuleAsync(factory.Services, category.CategoryId);

        var testRequest = new { Description = "This contains Test Pattern in it", Payee = "" };

        // Act
        var response = await client.PostAsJsonAsync("/api/rules/test", testRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var matchingRule = await response.Content.ReadFromJsonAsync<CategoryRule>();
        Assert.NotNull(matchingRule);
        Assert.Equal("Test Pattern", matchingRule.Pattern);
    }

    [Fact]
    public async Task TestRule_ReturnsNotFound_WhenNoMatchingRule()
    {
        // Arrange
        await using var factory = CreateFactory($"RulesTest_{Guid.NewGuid()}");
        var client = factory.CreateClient();

        var testRequest = new { Description = "Some unmatched description", Payee = "" };

        // Act
        var response = await client.PostAsJsonAsync("/api/rules/test", testRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
