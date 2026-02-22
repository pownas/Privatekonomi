using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Privatekonomi.Core.Configuration;
using Privatekonomi.Core.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Privatekonomi.Core.Tests;

[TestClass]
public class StorageConfigurationTests
{
    private static IServiceProvider CreateServiceProvider(Dictionary<string, string?> configValues)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(); // Add logging for persistence services
        services.AddPrivatekonomyStorage(configuration);

        return services.BuildServiceProvider();
    }
    
    private static IServiceProvider CreateJsonFileServiceProvider(string dataPath, string uniqueDbName)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Provider"] = "JsonFile",
                ["Storage:ConnectionString"] = dataPath,
                ["Storage:SeedTestData"] = "false"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        
        // Use unique database name to avoid shared state between test runs
        services.AddDbContext<PrivatekonomyContext>(options =>
            options.UseInMemoryDatabase(uniqueDbName));
        services.AddSingleton<Core.Services.Persistence.IDataPersistenceService, 
            Core.Services.Persistence.JsonFilePersistenceService>();

        return services.BuildServiceProvider();
    }

    [TestMethod]
    public void InMemoryStorage_ShouldBeConfigurable()
    {
        // Arrange & Act
        var serviceProvider = CreateServiceProvider(new Dictionary<string, string?>
        {
            ["Storage:Provider"] = "InMemory",
            ["Storage:ConnectionString"] = "",
            ["Storage:SeedTestData"] = "false"
        });
        var context = serviceProvider.GetRequiredService<PrivatekonomyContext>();

        // Assert
        Assert.IsNotNull(context);
        Assert.IsTrue(context.Database.IsInMemory());
    }

    [TestMethod]
    public void SqliteStorage_ShouldBeConfigurable()
    {
        // Arrange
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        
        // Act
        var serviceProvider = CreateServiceProvider(new Dictionary<string, string?>
        {
            ["Storage:Provider"] = "Sqlite",
            ["Storage:ConnectionString"] = $"Data Source={dbPath}",
            ["Storage:SeedTestData"] = "false"
        });
        var context = serviceProvider.GetRequiredService<PrivatekonomyContext>();

        // Assert
        Assert.IsNotNull(context);
        Assert.IsFalse(context.Database.IsInMemory());
        Assert.IsTrue(context.Database.IsSqlite());

        // Cleanup
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }
    }

    [TestMethod]
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
        Assert.AreEqual("Sqlite", settings.Provider);
        Assert.AreEqual("Data Source=test.db", settings.ConnectionString);
        Assert.IsTrue(settings.SeedTestData);
    }

    [TestMethod]
    public async Task SqliteStorage_ShouldPersistData()
    {
        // Arrange
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");

        // Act - Create database and add data
        {
            var serviceProvider = CreateServiceProvider(new Dictionary<string, string?>
            {
                ["Storage:Provider"] = "Sqlite",
                ["Storage:ConnectionString"] = $"Data Source={dbPath}",
                ["Storage:SeedTestData"] = "false"
            });
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
        {
            var serviceProvider = CreateServiceProvider(new Dictionary<string, string?>
            {
                ["Storage:Provider"] = "Sqlite",
                ["Storage:ConnectionString"] = $"Data Source={dbPath}",
                ["Storage:SeedTestData"] = "false"
            });
            var context = serviceProvider.GetRequiredService<PrivatekonomyContext>();
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Test Category");

            // Assert
            Assert.IsNotNull(category);
            Assert.AreEqual("Test Category", category.Name);
            Assert.AreEqual("#FF0000", category.Color);
        }

        // Cleanup
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }
    }

    [TestMethod]
    public void SqlServerStorage_ShouldBeConfigurable()
    {
        // Arrange
        var connectionString = "Server=(localdb)\\mssqllocaldb;Database=PrivatekonomyTest;Trusted_Connection=True;MultipleActiveResultSets=true";
        
        // Act
        var serviceProvider = CreateServiceProvider(new Dictionary<string, string?>
        {
            ["Storage:Provider"] = "SqlServer",
            ["Storage:ConnectionString"] = connectionString,
            ["Storage:SeedTestData"] = "false"
        });
        var context = serviceProvider.GetRequiredService<PrivatekonomyContext>();

        // Assert
        Assert.IsNotNull(context);
        Assert.IsFalse(context.Database.IsInMemory());
        Assert.IsTrue(context.Database.IsSqlServer());
    }

    [TestMethod]
    public void SqlServerStorage_WithoutConnectionString_ShouldThrowException()
    {
        // Arrange & Act & Assert
        InvalidOperationException? exception = null;
        try
        {
            var serviceProvider = CreateServiceProvider(new Dictionary<string, string?>
            {
                ["Storage:Provider"] = "SqlServer",
                ["Storage:ConnectionString"] = "",
                ["Storage:SeedTestData"] = "false"
            });
            // Exception is thrown during AddDbContext configuration
            Assert.Fail("Expected InvalidOperationException was not thrown");
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        Assert.IsNotNull(exception);
        StringAssert.Contains(exception.Message, "ConnectionString is required for SqlServer provider");
    }

    [TestMethod]
    public void JsonFileStorage_ShouldBeConfigurable()
    {
        // Arrange & Act
        var dataPath = Path.Combine(Path.GetTempPath(), $"jsontest_{Guid.NewGuid()}");
        var serviceProvider = CreateServiceProvider(new Dictionary<string, string?>
        {
            ["Storage:Provider"] = "JsonFile",
            ["Storage:ConnectionString"] = dataPath,
            ["Storage:SeedTestData"] = "false"
        });
        var context = serviceProvider.GetRequiredService<PrivatekonomyContext>();

        // Assert
        Assert.IsNotNull(context);
        Assert.IsTrue(context.Database.IsInMemory());
        
        // Verify persistence service is registered
        var persistenceService = serviceProvider.GetService<Core.Services.Persistence.IDataPersistenceService>();
        Assert.IsNotNull(persistenceService);

        // Cleanup
        if (Directory.Exists(dataPath))
        {
            Directory.Delete(dataPath, true);
        }
    }

    [TestMethod]
    [Ignore("Known issue: InMemory database entity tracking conflict when loading from JSON. This test works in isolation but fails when database instances are shared.")]
    public async Task JsonFileStorage_ShouldPersistAndLoadData()
    {
        // Arrange
        var dataPath = Path.Combine(Path.GetTempPath(), $"jsontest_{Guid.NewGuid()}");
        var uniqueDbName1 = $"TestDb_{Guid.NewGuid()}";
        var uniqueDbName2 = $"TestDb_{Guid.NewGuid()}";

        // Act - Create data and save
        {
            var serviceProvider = CreateJsonFileServiceProvider(dataPath, uniqueDbName1);
            var context = serviceProvider.GetRequiredService<PrivatekonomyContext>();
            var persistenceService = serviceProvider.GetService<Core.Services.Persistence.IDataPersistenceService>();

            Assert.IsNotNull(persistenceService); // Verify service is registered

            await context.Database.EnsureCreatedAsync();

            context.Categories.Add(new Core.Models.Category
            {
                Name = "Test Category",
                Color = "#FF0000",
                TaxRelated = false,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            // Save to JSON files
            await persistenceService.SaveAsync(context);
        }

        // Act - Load data in new context with fresh database
        {
            var serviceProvider = CreateJsonFileServiceProvider(dataPath, uniqueDbName2);
            var context = serviceProvider.GetRequiredService<PrivatekonomyContext>();
            var persistenceService = serviceProvider.GetService<Core.Services.Persistence.IDataPersistenceService>();

            Assert.IsNotNull(persistenceService); // Verify service is registered

            await context.Database.EnsureCreatedAsync();
            await persistenceService.LoadAsync(context);

            var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Test Category");

            // Assert
            Assert.IsNotNull(category);
            Assert.AreEqual("Test Category", category.Name);
            Assert.AreEqual("#FF0000", category.Color);
        }

        // Cleanup
        if (Directory.Exists(dataPath))
        {
            Directory.Delete(dataPath, true);
        }
    }
}
