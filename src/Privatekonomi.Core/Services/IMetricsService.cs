using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for calculating and retrieving platform metrics for admin dashboard
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Calculate current metrics across all dimensions
    /// </summary>
    Task<AdminMetrics> GetCurrentMetricsAsync();
    
    /// <summary>
    /// Get historical metrics for a specific period
    /// </summary>
    Task<AdminMetrics> GetMetricsForPeriodAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Get metrics grouped by period type (monthly, quarterly, etc.)
    /// </summary>
    Task<List<MetricsSnapshot>> GetHistoricalMetricsAsync(MetricsPeriodType periodType, int count = 12);
}
