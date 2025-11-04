using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Implementation of budget alert service
/// </summary>
public class BudgetAlertService : IBudgetAlertService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<BudgetAlertService> _logger;
    
    // Standard threshold percentages
    private static readonly decimal[] StandardThresholds = { 75m, 90m, 100m };

    public BudgetAlertService(
        PrivatekonomyContext context,
        ICurrentUserService? currentUserService,
        INotificationService notificationService,
        ILogger<BudgetAlertService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task CheckAllBudgetsAsync()
    {
        var now = DateTime.Now;
        var budgets = await _context.Budgets
            .Include(b => b.BudgetCategories)
            .ThenInclude(bc => bc.Category)
            .Where(b => b.StartDate <= now && b.EndDate >= now)
            .ToListAsync();

        // Filter by user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            budgets = budgets.Where(b => b.UserId == _currentUserService.UserId).ToList();
        }

        foreach (var budget in budgets)
        {
            await CheckBudgetAsync(budget.BudgetId);
        }
    }

    public async Task<IEnumerable<BudgetAlert>> CheckBudgetAsync(int budgetId)
    {
        var budget = await _context.Budgets
            .Include(b => b.BudgetCategories)
            .ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.BudgetId == budgetId);

        if (budget == null)
        {
            return Enumerable.Empty<BudgetAlert>();
        }

        var settings = await GetOrCreateSettingsAsync();
        var createdAlerts = new List<BudgetAlert>();

        foreach (var budgetCategory in budget.BudgetCategories)
        {
            var usagePercentage = await CalculateBudgetUsagePercentageAsync(budgetId, budgetCategory.CategoryId);
            var thresholds = GetActiveThresholds(settings);

            foreach (var threshold in thresholds)
            {
                // Check if we've crossed this threshold
                if (usagePercentage >= threshold)
                {
                    // Check if alert already exists for this threshold
                    var existingAlert = await _context.BudgetAlerts
                        .FirstOrDefaultAsync(a => 
                            a.BudgetId == budgetId && 
                            a.BudgetCategoryId == budgetCategory.BudgetCategoryId &&
                            a.ThresholdPercentage == threshold &&
                            a.IsActive);

                    if (existingAlert == null)
                    {
                        var alert = await CreateAlertAsync(budget, budgetCategory, threshold, usagePercentage);
                        createdAlerts.Add(alert);
                        
                        // Send notification
                        await SendAlertNotificationAsync(alert, budget, budgetCategory);
                        
                        // Auto-freeze if enabled and threshold is 100%
                        if (settings.EnableBudgetFreeze && threshold >= 100m)
                        {
                            await FreezeBudgetAsync(budgetId, budgetCategory.CategoryId, "Budget överskriden");
                        }
                    }
                }
            }
        }

        return createdAlerts;
    }

    public async Task<IEnumerable<BudgetAlert>> GetActiveAlertsAsync()
    {
        var query = _context.BudgetAlerts
            .Include(a => a.Budget)
            .Include(a => a.BudgetCategory)
            .ThenInclude(bc => bc.Category)
            .Where(a => a.IsActive)
            .AsQueryable();

        // Filter by user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(a => a.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(a => a.TriggeredAt).ToListAsync();
    }

    public async Task<IEnumerable<BudgetAlert>> GetActiveAlertsForBudgetAsync(int budgetId)
    {
        var query = _context.BudgetAlerts
            .Include(a => a.BudgetCategory)
            .ThenInclude(bc => bc.Category)
            .Where(a => a.BudgetId == budgetId && a.IsActive)
            .AsQueryable();

        // Filter by user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(a => a.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(a => a.TriggeredAt).ToListAsync();
    }

    public async Task<IEnumerable<BudgetAlert>> GetActiveAlertsForCategoryAsync(int budgetId, int categoryId)
    {
        var budgetCategory = await _context.BudgetCategories
            .FirstOrDefaultAsync(bc => bc.BudgetId == budgetId && bc.CategoryId == categoryId);

        if (budgetCategory == null)
        {
            return Enumerable.Empty<BudgetAlert>();
        }

        var query = _context.BudgetAlerts
            .Include(a => a.BudgetCategory)
            .ThenInclude(bc => bc.Category)
            .Where(a => a.BudgetCategoryId == budgetCategory.BudgetCategoryId && a.IsActive)
            .AsQueryable();

        // Filter by user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(a => a.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(a => a.TriggeredAt).ToListAsync();
    }

    public async Task AcknowledgeAlertAsync(int alertId)
    {
        var alert = await _context.BudgetAlerts.FindAsync(alertId);
        if (alert != null)
        {
            // Verify user ownership
            if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
            {
                if (alert.UserId != _currentUserService.UserId)
                {
                    throw new UnauthorizedAccessException("Cannot acknowledge alert belonging to another user");
                }
            }

            alert.IsActive = false;
            alert.AcknowledgedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> CalculateBudgetUsagePercentageAsync(int budgetId, int categoryId)
    {
        var budget = await _context.Budgets.FindAsync(budgetId);
        if (budget == null) return 0;

        var budgetCategory = await _context.BudgetCategories
            .FirstOrDefaultAsync(bc => bc.BudgetId == budgetId && bc.CategoryId == categoryId);
        
        if (budgetCategory == null || budgetCategory.PlannedAmount == 0) return 0;

        var spent = await GetSpentAmountAsync(budgetId, categoryId);
        return (spent / budgetCategory.PlannedAmount) * 100m;
    }

    public async Task<decimal> CalculateDailyRateAsync(int budgetId, int categoryId)
    {
        var budget = await _context.Budgets.FindAsync(budgetId);
        if (budget == null) return 0;

        var now = DateTime.Now;
        var daysElapsed = (now - budget.StartDate).Days + 1; // +1 to include today
        
        if (daysElapsed <= 0) return 0;

        var spent = await GetSpentAmountAsync(budgetId, categoryId);
        return spent / daysElapsed;
    }

    public async Task<int?> CalculateDaysUntilExceededAsync(int budgetId, int categoryId)
    {
        var budget = await _context.Budgets.FindAsync(budgetId);
        if (budget == null) return null;

        var budgetCategory = await _context.BudgetCategories
            .FirstOrDefaultAsync(bc => bc.BudgetId == budgetId && bc.CategoryId == categoryId);
        
        if (budgetCategory == null) return null;

        var spent = await GetSpentAmountAsync(budgetId, categoryId);
        var remaining = budgetCategory.PlannedAmount - spent;
        
        // If already exceeded, return 0
        if (remaining <= 0) return 0;

        var dailyRate = await CalculateDailyRateAsync(budgetId, categoryId);
        
        // If no spending yet, can't forecast
        if (dailyRate <= 0) return null;

        var daysUntilExceeded = (int)Math.Ceiling(remaining / dailyRate);
        return daysUntilExceeded;
    }

    public async Task<BudgetAlertSettings> GetOrCreateSettingsAsync()
    {
        var userId = _currentUserService?.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            // Return default settings if no user
            return new BudgetAlertSettings
            {
                UserId = string.Empty,
                EnableAlert75 = true,
                EnableAlert90 = true,
                EnableAlert100 = true,
                EnableForecastWarnings = true,
                ForecastWarningDays = 7
            };
        }

        var settings = await _context.BudgetAlertSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            settings = new BudgetAlertSettings
            {
                UserId = userId,
                EnableAlert75 = true,
                EnableAlert90 = true,
                EnableAlert100 = true,
                EnableForecastWarnings = true,
                ForecastWarningDays = 7
            };

            _context.BudgetAlertSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return settings;
    }

    public async Task<BudgetAlertSettings> UpdateSettingsAsync(BudgetAlertSettings settings)
    {
        var userId = _currentUserService?.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User must be authenticated");
        }

        var existing = await _context.BudgetAlertSettings
            .FirstOrDefaultAsync(s => s.BudgetAlertSettingsId == settings.BudgetAlertSettingsId);

        if (existing == null)
        {
            throw new InvalidOperationException("Settings not found");
        }

        if (existing.UserId != userId)
        {
            throw new UnauthorizedAccessException("Cannot update settings belonging to another user");
        }

        existing.EnableAlert75 = settings.EnableAlert75;
        existing.EnableAlert90 = settings.EnableAlert90;
        existing.EnableAlert100 = settings.EnableAlert100;
        existing.CustomThresholds = settings.CustomThresholds;
        existing.EnableForecastWarnings = settings.EnableForecastWarnings;
        existing.ForecastWarningDays = settings.ForecastWarningDays;
        existing.EnableBudgetFreeze = settings.EnableBudgetFreeze;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<BudgetFreeze> FreezeBudgetAsync(int budgetId, int? categoryId = null, string? reason = null)
    {
        var userId = _currentUserService?.UserId;

        // Check if already frozen
        var existingFreeze = await _context.BudgetFreezes
            .FirstOrDefaultAsync(f => 
                f.BudgetId == budgetId && 
                f.BudgetCategoryId == categoryId &&
                f.IsActive);

        if (existingFreeze != null)
        {
            return existingFreeze;
        }

        var freeze = new BudgetFreeze
        {
            BudgetId = budgetId,
            BudgetCategoryId = categoryId,
            FrozenAt = DateTime.UtcNow,
            Reason = reason,
            IsActive = true,
            UserId = userId
        };

        _context.BudgetFreezes.Add(freeze);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Budget {BudgetId} category {CategoryId} frozen: {Reason}", 
            budgetId, categoryId, reason);

        return freeze;
    }

    public async Task UnfreezeBudgetAsync(int budgetFreezeId)
    {
        var freeze = await _context.BudgetFreezes.FindAsync(budgetFreezeId);
        if (freeze != null)
        {
            // Verify user ownership
            if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
            {
                if (freeze.UserId != _currentUserService.UserId)
                {
                    throw new UnauthorizedAccessException("Cannot unfreeze budget belonging to another user");
                }
            }

            freeze.IsActive = false;
            freeze.UnfrozenAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Budget freeze {FreezeId} lifted", budgetFreezeId);
        }
    }

    public async Task<bool> IsBudgetFrozenAsync(int budgetId, int? categoryId = null)
    {
        var query = _context.BudgetFreezes
            .Where(f => f.BudgetId == budgetId && f.IsActive);

        if (categoryId.HasValue)
        {
            query = query.Where(f => f.BudgetCategoryId == categoryId.Value);
        }

        // Filter by user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(f => f.UserId == _currentUserService.UserId);
        }

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<BudgetFreeze>> GetActiveFreezesAsync()
    {
        var query = _context.BudgetFreezes
            .Include(f => f.Budget)
            .Include(f => f.BudgetCategory)
            .ThenInclude(bc => bc!.Category)
            .Where(f => f.IsActive)
            .AsQueryable();

        // Filter by user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(f => f.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(f => f.FrozenAt).ToListAsync();
    }

    // Private helper methods

    private async Task<decimal> GetSpentAmountAsync(int budgetId, int categoryId)
    {
        var budget = await _context.Budgets.FindAsync(budgetId);
        if (budget == null) return 0;

        var transactions = await _context.Transactions
            .Include(t => t.TransactionCategories)
            .Where(t => t.Date >= budget.StartDate && 
                       t.Date <= budget.EndDate && 
                       !t.IsIncome)
            .ToListAsync();

        decimal spent = 0;
        foreach (var transaction in transactions)
        {
            foreach (var tc in transaction.TransactionCategories)
            {
                if (tc.CategoryId == categoryId)
                {
                    spent += tc.Amount;
                }
            }
        }

        return spent;
    }

    private async Task<BudgetAlert> CreateAlertAsync(
        Budget budget, 
        BudgetCategory budgetCategory, 
        decimal threshold, 
        decimal currentPercentage)
    {
        var spent = await GetSpentAmountAsync(budget.BudgetId, budgetCategory.CategoryId);
        var dailyRate = await CalculateDailyRateAsync(budget.BudgetId, budgetCategory.CategoryId);
        var daysUntilExceeded = await CalculateDaysUntilExceededAsync(budget.BudgetId, budgetCategory.CategoryId);

        var alert = new BudgetAlert
        {
            BudgetId = budget.BudgetId,
            BudgetCategoryId = budgetCategory.BudgetCategoryId,
            ThresholdPercentage = threshold,
            CurrentPercentage = currentPercentage,
            SpentAmount = spent,
            PlannedAmount = budgetCategory.PlannedAmount,
            TriggeredAt = DateTime.UtcNow,
            IsActive = true,
            ForecastDaysUntilExceeded = daysUntilExceeded,
            DailyRate = dailyRate,
            UserId = _currentUserService?.UserId
        };

        _context.BudgetAlerts.Add(alert);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Budget alert created: Budget {BudgetId}, Category {CategoryId}, Threshold {Threshold}%, Current {Current}%",
            budget.BudgetId, budgetCategory.CategoryId, threshold, currentPercentage);

        return alert;
    }

    private async Task SendAlertNotificationAsync(BudgetAlert alert, Budget budget, BudgetCategory budgetCategory)
    {
        var userId = _currentUserService?.UserId;
        if (string.IsNullOrEmpty(userId)) return;

        var category = budgetCategory.Category;
        var title = $"🚨 Budgetvarning: {category?.Name ?? "Okänd kategori"}";
        
        var remaining = alert.PlannedAmount - alert.SpentAmount;
        var now = DateTime.Now;
        var daysRemaining = (budget.EndDate - now).Days;

        var message = $"Du har använt {alert.SpentAmount:N0} kr av {alert.PlannedAmount:N0} kr ({alert.CurrentPercentage:F0}%)\n" +
                     $"Återstående: {remaining:N0} kr för {daysRemaining} dagar";

        if (alert.ForecastDaysUntilExceeded.HasValue && alert.ForecastDaysUntilExceeded.Value < daysRemaining)
        {
            message += $"\n\nPrognos: Budget överskrids om {alert.ForecastDaysUntilExceeded.Value} dagar i nuvarande takt ({alert.DailyRate:F0} kr/dag)";
        }

        var priority = alert.ThresholdPercentage >= 100m ? NotificationPriority.Critical :
                      alert.ThresholdPercentage >= 90m ? NotificationPriority.High :
                      NotificationPriority.Normal;

        await _notificationService.SendNotificationAsync(
            userId,
            SystemNotificationType.BudgetWarning,
            title,
            message,
            priority,
            data: System.Text.Json.JsonSerializer.Serialize(new { alert.BudgetAlertId, alert.BudgetId, CategoryId = budgetCategory.CategoryId }),
            actionUrl: $"/budgets/{budget.BudgetId}");
    }

    private decimal[] GetActiveThresholds(BudgetAlertSettings settings)
    {
        var thresholds = new List<decimal>();

        if (settings.EnableAlert75) thresholds.Add(75m);
        if (settings.EnableAlert90) thresholds.Add(90m);
        if (settings.EnableAlert100) thresholds.Add(100m);

        // Add custom thresholds if specified
        if (!string.IsNullOrEmpty(settings.CustomThresholds))
        {
            var customParts = settings.CustomThresholds.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in customParts)
            {
                if (decimal.TryParse(part.Trim(), out var threshold))
                {
                    thresholds.Add(threshold);
                }
            }
        }

        return thresholds.Distinct().OrderBy(t => t).ToArray();
    }
}
