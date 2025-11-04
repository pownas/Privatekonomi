using Microsoft.AspNetCore.SignalR;
using Privatekonomi.Core.Services;
using Privatekonomi.Web.Hubs;

namespace Privatekonomi.Web.Services;

/// <summary>
/// Background service that periodically checks budgets and sends alerts
/// </summary>
public class BudgetAlertBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<BudgetAlertHub> _hubContext;
    private readonly ILogger<BudgetAlertBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30); // Check every 30 minutes

    public BudgetAlertBackgroundService(
        IServiceProvider serviceProvider,
        IHubContext<BudgetAlertHub> hubContext,
        ILogger<BudgetAlertBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Budget Alert Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAllBudgetsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking budgets in background service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Budget Alert Background Service stopped");
    }

    private async Task CheckAllBudgetsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var budgetAlertService = scope.ServiceProvider.GetRequiredService<IBudgetAlertService>();
        var budgetService = scope.ServiceProvider.GetRequiredService<IBudgetService>();

        try
        {
            _logger.LogInformation("Starting budget check at {Time}", DateTime.Now);

            // Get all active budgets
            var activeBudgets = await budgetService.GetActiveBudgetsAsync();
            
            foreach (var budget in activeBudgets)
            {
                try
                {
                    // Check budget and get any new alerts
                    var alerts = await budgetAlertService.CheckBudgetAsync(budget.BudgetId);
                    
                    // If new alerts were created, notify the user via SignalR
                    if (alerts.Any() && !string.IsNullOrEmpty(budget.UserId))
                    {
                        await _hubContext.Clients.Group($"user_{budget.UserId}")
                            .SendAsync("ReceiveBudgetAlert", new 
                            { 
                                budgetId = budget.BudgetId, 
                                budgetName = budget.Name,
                                alerts = alerts.Select(a => new
                                {
                                    a.BudgetAlertId,
                                    a.ThresholdPercentage,
                                    a.CurrentPercentage,
                                    a.SpentAmount,
                                    a.PlannedAmount,
                                    a.ForecastDaysUntilExceeded,
                                    a.DailyRate,
                                    CategoryName = a.BudgetCategory?.Category?.Name
                                })
                            });
                        
                        _logger.LogInformation(
                            "Sent {AlertCount} budget alerts for budget {BudgetId} to user {UserId}",
                            alerts.Count(), budget.BudgetId, budget.UserId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking budget {BudgetId}", budget.BudgetId);
                }
            }

            _logger.LogInformation("Completed budget check at {Time}", DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CheckAllBudgetsAsync");
            throw;
        }
    }
}
