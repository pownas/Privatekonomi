using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for aggregating dashboard data from multiple sources.
/// Provides ekonomisk översikt with balance, budget status, upcoming bills, and insights.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Get aggregated dashboard data for the current user
    /// </summary>
    /// <param name="accountIds">Optional list of account IDs to include in balance calculation. If null, all accounts are included.</param>
    /// <returns>Aggregated dashboard data</returns>
    Task<DashboardData> GetDashboardDataAsync(int[]? accountIds = null);
    
    /// <summary>
    /// Get balance summary for selected accounts
    /// </summary>
    /// <param name="accountIds">Optional list of account IDs. If null, all accounts are included.</param>
    /// <returns>Balance summary</returns>
    Task<BalanceSummary> GetBalanceSummaryAsync(int[]? accountIds = null);
    
    /// <summary>
    /// Get current month's budget status
    /// </summary>
    /// <returns>Budget status summary</returns>
    Task<BudgetStatusSummary> GetBudgetStatusAsync();
    
    /// <summary>
    /// Get list of upcoming bills
    /// </summary>
    /// <param name="daysAhead">Number of days ahead to look for bills (default: 30)</param>
    /// <param name="limit">Maximum number of bills to return (default: 5)</param>
    /// <returns>List of upcoming bills</returns>
    Task<List<BillSummary>> GetUpcomingBillsAsync(int daysAhead = 30, int limit = 5);
    
    /// <summary>
    /// Get recent insights and alerts
    /// </summary>
    /// <param name="limit">Maximum number of insights to return (default: 5)</param>
    /// <returns>List of insights</returns>
    Task<List<InsightItem>> GetRecentInsightsAsync(int limit = 5);
}
