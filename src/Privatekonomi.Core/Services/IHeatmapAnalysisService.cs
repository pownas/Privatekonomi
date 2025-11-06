using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for analyzing expense patterns and generating heatmap visualizations
/// </summary>
public interface IHeatmapAnalysisService
{
    /// <summary>
    /// Generate expense heatmap data for a given time period
    /// </summary>
    /// <param name="startDate">Start date of analysis period</param>
    /// <param name="endDate">End date of analysis period</param>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="householdId">Optional household filter</param>
    /// <returns>Heatmap data with weekday × hour breakdown</returns>
    Task<ExpenseHeatmapData> GenerateHeatmapAsync(
        DateTime startDate, 
        DateTime endDate, 
        int? categoryId = null,
        int? householdId = null);
    
    /// <summary>
    /// Get expense pattern insights for a time period
    /// </summary>
    /// <param name="startDate">Start date of analysis period</param>
    /// <param name="endDate">End date of analysis period</param>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="householdId">Optional household filter</param>
    /// <returns>Pattern insights including expensive days, peaks, and impulse purchases</returns>
    Task<PatternInsights> GetPatternInsightsAsync(
        DateTime startDate,
        DateTime endDate,
        int? categoryId = null,
        int? householdId = null);
}
