using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Privatekonomi.Core.Services;

namespace Privatekonomi.Web.Hubs;

/// <summary>
/// SignalR hub for real-time budget alert notifications
/// </summary>
[Authorize]
public class BudgetAlertHub : Hub
{
    private readonly IBudgetAlertService _budgetAlertService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<BudgetAlertHub> _logger;

    public BudgetAlertHub(
        IBudgetAlertService budgetAlertService,
        ICurrentUserService currentUserService,
        ILogger<BudgetAlertHub> logger)
    {
        _budgetAlertService = budgetAlertService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _currentUserService.UserId;
        if (!string.IsNullOrEmpty(userId))
        {
            // Add to user-specific group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} connected to BudgetAlertHub", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _currentUserService.UserId;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} disconnected from BudgetAlertHub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Get active alerts for the current user
    /// </summary>
    public async Task<object> GetActiveAlerts()
    {
        try
        {
            var alerts = await _budgetAlertService.GetActiveAlertsAsync();
            return new { success = true, alerts };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active alerts");
            return new { success = false, error = ex.Message };
        }
    }

    /// <summary>
    /// Acknowledge an alert
    /// </summary>
    public async Task<object> AcknowledgeAlert(int alertId)
    {
        try
        {
            await _budgetAlertService.AcknowledgeAlertAsync(alertId);
            return new { success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging alert {AlertId}", alertId);
            return new { success = false, error = ex.Message };
        }
    }

    /// <summary>
    /// Check budget and send alerts if needed
    /// </summary>
    public async Task<object> CheckBudget(int budgetId)
    {
        try
        {
            var alerts = await _budgetAlertService.CheckBudgetAsync(budgetId);
            
            // Notify user of new alerts
            if (alerts.Any())
            {
                var userId = _currentUserService.UserId;
                if (!string.IsNullOrEmpty(userId))
                {
                    await Clients.Group($"user_{userId}")
                        .SendAsync("ReceiveBudgetAlert", new { budgetId, alerts });
                }
            }
            
            return new { success = true, alertsCreated = alerts.Count() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking budget {BudgetId}", budgetId);
            return new { success = false, error = ex.Message };
        }
    }

    /// <summary>
    /// Get budget freeze status
    /// </summary>
    public async Task<object> GetBudgetFreezeStatus(int budgetId, int? categoryId = null)
    {
        try
        {
            var isFrozen = await _budgetAlertService.IsBudgetFrozenAsync(budgetId, categoryId);
            return new { success = true, isFrozen };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking freeze status for budget {BudgetId}", budgetId);
            return new { success = false, error = ex.Message };
        }
    }

    /// <summary>
    /// Unfreeze a budget
    /// </summary>
    public async Task<object> UnfreezeBudget(int budgetFreezeId)
    {
        try
        {
            await _budgetAlertService.UnfreezeBudgetAsync(budgetFreezeId);
            
            var userId = _currentUserService.UserId;
            if (!string.IsNullOrEmpty(userId))
            {
                await Clients.Group($"user_{userId}")
                    .SendAsync("BudgetUnfrozen", new { budgetFreezeId });
            }
            
            return new { success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unfreezing budget {BudgetFreezeId}", budgetFreezeId);
            return new { success = false, error = ex.Message };
        }
    }
}
