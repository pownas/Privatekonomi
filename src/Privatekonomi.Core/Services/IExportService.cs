namespace Privatekonomi.Core.Services;

public interface IExportService
{
    /// <summary>
    /// Export transactions to CSV format
    /// </summary>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <returns>CSV file content as byte array</returns>
    Task<byte[]> ExportTransactionsToCsvAsync(DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Export transactions to JSON format
    /// </summary>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <returns>JSON file content as byte array</returns>
    Task<byte[]> ExportTransactionsToJsonAsync(DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Export budget data to CSV format
    /// </summary>
    /// <param name="budgetId">Budget ID to export</param>
    /// <returns>CSV file content as byte array</returns>
    Task<byte[]> ExportBudgetToCsvAsync(int budgetId);
    
    /// <summary>
    /// Create a full backup of all data in JSON format
    /// </summary>
    /// <returns>JSON backup file content as byte array</returns>
    Task<byte[]> ExportFullBackupAsync();
}
