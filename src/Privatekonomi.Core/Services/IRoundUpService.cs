using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for managing Round-up automatic savings
/// </summary>
public interface IRoundUpService
{
    /// <summary>
    /// Get or create round-up settings for the current user
    /// </summary>
    Task<RoundUpSettings> GetOrCreateSettingsAsync();
    
    /// <summary>
    /// Update round-up settings
    /// </summary>
    Task<RoundUpSettings> UpdateSettingsAsync(RoundUpSettings settings);
    
    /// <summary>
    /// Calculate round-up amount for a transaction
    /// </summary>
    /// <param name="amount">Original transaction amount</param>
    /// <param name="roundUpTo">Round up to nearest multiple (default: 10)</param>
    /// <returns>Round-up amount to save</returns>
    decimal CalculateRoundUp(decimal amount, decimal roundUpTo = 10M);
    
    /// <summary>
    /// Process round-up for a transaction
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <returns>Created round-up transaction, or null if not applicable</returns>
    Task<RoundUpTransaction?> ProcessRoundUpForTransactionAsync(int transactionId);
    
    /// <summary>
    /// Process salary-based auto-save for an income transaction
    /// </summary>
    /// <param name="transactionId">Income transaction ID</param>
    /// <returns>Created round-up transaction, or null if not applicable</returns>
    Task<RoundUpTransaction?> ProcessSalaryAutoSaveAsync(int transactionId);
    
    /// <summary>
    /// Get round-up transactions for the current user
    /// </summary>
    /// <param name="fromDate">Start date filter (optional)</param>
    /// <param name="toDate">End date filter (optional)</param>
    Task<IEnumerable<RoundUpTransaction>> GetRoundUpTransactionsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Get round-up statistics for a period
    /// </summary>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    Task<RoundUpStatistics> GetStatisticsAsync(DateTime fromDate, DateTime toDate);
    
    /// <summary>
    /// Get total round-up savings for current month
    /// </summary>
    Task<decimal> GetMonthlyTotalAsync();
}

/// <summary>
/// Statistics for round-up savings
/// </summary>
public class RoundUpStatistics
{
    public decimal TotalRoundUp { get; set; }
    public decimal TotalEmployerMatching { get; set; }
    public decimal TotalSalaryAutoSave { get; set; }
    public decimal TotalSaved { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageRoundUp { get; set; }
    public decimal LargestRoundUp { get; set; }
}
