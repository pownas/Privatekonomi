using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Privatekonomi.Core.Data;

namespace Privatekonomi.Core.Configuration;

/// <summary>
/// Extension methods for configuring storage providers
/// </summary>
public static class StorageExtensions
{
    /// <summary>
    /// Configures the PrivatekonomyContext based on StorageSettings configuration
    /// </summary>
    public static IServiceCollection AddPrivatekonomyStorage(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var storageSettings = new StorageSettings();
        configuration.GetSection("Storage").Bind(storageSettings);
        
        services.Configure<StorageSettings>(configuration.GetSection("Storage"));
        
        switch (storageSettings.Provider.ToLowerInvariant())
        {
            case "sqlite":
                services.AddDbContext<PrivatekonomyContext>(options =>
                {
                    var connectionString = storageSettings.ConnectionString;
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        connectionString = "Data Source=privatekonomi.db";
                    }
                    options.UseSqlite(connectionString);
                });
                break;
                
            case "jsonfile":
                // JsonFile uses InMemory with persistence layer
                services.AddDbContext<PrivatekonomyContext>(options =>
                    options.UseInMemoryDatabase("PrivatekonomyDb"));
                // TODO: Add JSON file persistence service
                break;
                
            case "inmemory":
            default:
                services.AddDbContext<PrivatekonomyContext>(options =>
                    options.UseInMemoryDatabase("PrivatekonomyDb"));
                break;
        }
        
        return services;
    }
}
