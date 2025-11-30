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
    
    /// <summary>
    /// Get list of years that have transaction data
    /// </summary>
    /// <returns>List of years with transactions</returns>
    Task<List<int>> GetAvailableYearsAsync();
    
    /// <summary>
    /// Export all economic data for a specific year to JSON format
    /// </summary>
    /// <param name="year">Year to export</param>
    /// <returns>JSON file content as byte array</returns>
    Task<byte[]> ExportYearDataToJsonAsync(int year);
    
    /// <summary>
    /// Export all economic data for a specific year to CSV format
    /// </summary>
    /// <param name="year">Year to export</param>
    /// <returns>CSV file content as byte array</returns>
    Task<byte[]> ExportYearDataToCsvAsync(int year);
    
    /// <summary>
    /// Export selected transactions to CSV format
    /// </summary>
    /// <param name="transactionIds">IDs of transactions to export</param>
    /// <returns>CSV file content as byte array</returns>
    Task<byte[]> ExportSelectedTransactionsToCsvAsync(List<int> transactionIds);
    
    /// <summary>
    /// Export selected transactions to JSON format
    /// </summary>
    /// <param name="transactionIds">IDs of transactions to export</param>
    /// <returns>JSON file content as byte array</returns>
    Task<byte[]> ExportSelectedTransactionsToJsonAsync(List<int> transactionIds);
}
