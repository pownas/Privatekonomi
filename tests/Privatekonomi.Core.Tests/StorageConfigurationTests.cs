using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Privatekonomi.Core.Configuration;
using Privatekonomi.Core.Data;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class StorageConfigurationTests
{
    [Fact]
    public void InMemoryStorage_ShouldBeConfigurable()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Provider"] = "InMemory",
                ["Storage:ConnectionString"] = "",
                ["Storage:SeedTestData"] = "false"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddPrivatekonomyStorage(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var context = serviceProvider.GetRequiredService<PrivatekonomyContext>();

        // Assert
        Assert.NotNull(context);
        Assert.True(context.Database.IsInMemory());
    }

    [Fact]
    public void SqliteStorage_ShouldBeConfigurable()
    {
        // Arrange
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Provider"] = "Sqlite",
                ["Storage:ConnectionString"] = $"Data Source={dbPath}",
                ["Storage:SeedTestData"] = "false"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddPrivatekonomyStorage(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var context = serviceProvider.GetRequiredService<PrivatekonomyContext>();

        // Assert
        Assert.NotNull(context);
        Assert.False(context.Database.IsInMemory());
        Assert.True(context.Database.IsSqlite());

        // Cleanup
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }
    }

    [Fact]
    public void StorageSettings_ShouldBindFromConfiguration()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Provider"] = "Sqlite",
                ["Storage:ConnectionString"] = "Data Source=test.db",
                ["Storage:SeedTestData"] = "true"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<StorageSettings>(configuration.GetSection("Storage"));

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var settings = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<StorageSettings>>().Value;

        // Assert
        Assert.Equal("Sqlite", settings.Provider);
        Assert.Equal("Data Source=test.db", settings.ConnectionString);
        Assert.True(settings.SeedTestData);
    }

    [Fact]
    public async Task SqliteStorage_ShouldPersistData()
    {
        // Arrange
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Provider"] = "Sqlite",
                ["Storage:ConnectionString"] = $"Data Source={dbPath}",
                ["Storage:SeedTestData"] = "false"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddPrivatekonomyStorage(configuration);

        // Act - Create database and add data
        using (var serviceProvider = services.BuildServiceProvider())
        {
            var context = serviceProvider.GetRequiredService<PrivatekonomyContext>();
            await context.Database.EnsureCreatedAsync();

            context.Categories.Add(new Core.Models.Category
            {
                Name = "Test Category",
                Color = "#FF0000",
                TaxRelated = false,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // Act - Retrieve data with new context
        using (var serviceProvider = services.BuildServiceProvider())
        {
            var context = serviceProvider.GetRequiredService<PrivatekonomyContext>();
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Test Category");

            // Assert
            Assert.NotNull(category);
            Assert.Equal("Test Category", category.Name);
            Assert.Equal("#FF0000", category.Color);
        }

        // Cleanup
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }
    }
}
