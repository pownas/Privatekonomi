using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for managing budget alerts and monitoring budget thresholds
/// </summary>
public interface IBudgetAlertService
{
    /// <summary>
    /// Check all active budgets and create alerts for threshold violations
    /// </summary>
    Task CheckAllBudgetsAsync();
    
    /// <summary>
    /// Check a specific budget and create alerts if needed
    /// </summary>
    Task<IEnumerable<BudgetAlert>> CheckBudgetAsync(int budgetId);
    
    /// <summary>
    /// Get all active alerts for the current user
    /// </summary>
    Task<IEnumerable<BudgetAlert>> GetActiveAlertsAsync();
    
    /// <summary>
    /// Get all active alerts for a specific budget
    /// </summary>
    Task<IEnumerable<BudgetAlert>> GetActiveAlertsForBudgetAsync(int budgetId);
    
    /// <summary>
    /// Get all active alerts for a specific budget category
    /// </summary>
    Task<IEnumerable<BudgetAlert>> GetActiveAlertsForCategoryAsync(int budgetId, int categoryId);
    
    /// <summary>
    /// Acknowledge an alert (mark as read)
    /// </summary>
    Task AcknowledgeAlertAsync(int alertId);
    
    /// <summary>
    /// Calculate budget usage percentage for a category
    /// </summary>
    Task<decimal> CalculateBudgetUsagePercentageAsync(int budgetId, int categoryId);
    
    /// <summary>
    /// Calculate daily spending rate for a budget category
    /// </summary>
    Task<decimal> CalculateDailyRateAsync(int budgetId, int categoryId);
    
    /// <summary>
    /// Calculate forecast for when budget will be exceeded
    /// </summary>
    Task<int?> CalculateDaysUntilExceededAsync(int budgetId, int categoryId);
    
    /// <summary>
    /// Get or create budget alert settings for current user
    /// </summary>
    Task<BudgetAlertSettings> GetOrCreateSettingsAsync();
    
    /// <summary>
    /// Update budget alert settings
    /// </summary>
    Task<BudgetAlertSettings> UpdateSettingsAsync(BudgetAlertSettings settings);
    
    /// <summary>
    /// Freeze a budget or budget category
    /// </summary>
    Task<BudgetFreeze> FreezeBudgetAsync(int budgetId, int? categoryId = null, string? reason = null);
    
    /// <summary>
    /// Unfreeze a budget or budget category
    /// </summary>
    Task UnfreezeBudgetAsync(int budgetFreezeId);
    
    /// <summary>
    /// Check if a budget or category is frozen
    /// </summary>
    Task<bool> IsBudgetFrozenAsync(int budgetId, int? categoryId = null);
    
    /// <summary>
    /// Get active budget freezes
    /// </summary>
    Task<IEnumerable<BudgetFreeze>> GetActiveFreezesAsync();
}
