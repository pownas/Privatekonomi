using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Privatekonomi.Core.Configuration;
using Privatekonomi.Core.Data;

namespace Privatekonomi.Core.Services.Persistence;

/// <summary>
/// Background service that periodically saves InMemory data to JSON files
/// </summary>
public class JsonFilePersistenceHostedService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JsonFilePersistenceHostedService> _logger;
    private readonly StorageSettings _storageSettings;
    private Timer? _timer;
    private readonly TimeSpan _saveInterval = TimeSpan.FromMinutes(5);

    public JsonFilePersistenceHostedService(
        IServiceProvider serviceProvider,
        ILogger<JsonFilePersistenceHostedService> logger,
        IOptions<StorageSettings> storageSettings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _storageSettings = storageSettings.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_storageSettings.Provider.Equals("JsonFile", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Starting JSON file persistence service with {Interval} save interval", _saveInterval);
            _timer = new Timer(SaveData, null, _saveInterval, _saveInterval);
        }
        return Task.CompletedTask;
    }

    private async void SaveData(object? state)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PrivatekonomyContext>();
            var persistenceService = scope.ServiceProvider.GetService<IDataPersistenceService>();
            
            if (persistenceService != null)
            {
                await persistenceService.SaveAsync(context);
                _logger.LogDebug("Periodic save to JSON files completed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during periodic save to JSON files");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_storageSettings.Provider.Equals("JsonFile", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Stopping JSON file persistence service and saving final state...");
            _timer?.Change(Timeout.Infinite, 0);
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<PrivatekonomyContext>();
                var persistenceService = scope.ServiceProvider.GetService<IDataPersistenceService>();
                
                if (persistenceService != null)
                {
                    await persistenceService.SaveAsync(context);
                    _logger.LogInformation("Final save to JSON files completed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during final save to JSON files");
            }
        }
        return;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
