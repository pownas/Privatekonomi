using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for managing bank API connections
/// </summary>
public interface IBankConnectionService
{
    /// <summary>
    /// Gets all bank connections for a user
    /// </summary>
    Task<List<BankConnection>> GetConnectionsAsync(int? bankSourceId = null);
    
    /// <summary>
    /// Gets a specific bank connection by ID
    /// </summary>
    Task<BankConnection?> GetConnectionAsync(int connectionId);
    
    /// <summary>
    /// Creates a new bank connection
    /// </summary>
    Task<BankConnection> CreateConnectionAsync(BankConnection connection);
    
    /// <summary>
    /// Updates an existing bank connection
    /// </summary>
    Task<BankConnection> UpdateConnectionAsync(BankConnection connection);
    
    /// <summary>
    /// Deletes a bank connection
    /// </summary>
    Task DeleteConnectionAsync(int connectionId);
    
    /// <summary>
    /// Gets the appropriate bank API service for a bank
    /// </summary>
    IBankApiService? GetBankApiService(string bankName);
    
    /// <summary>
    /// Gets all available bank API services
    /// </summary>
    List<string> GetAvailableBanks();
    
    /// <summary>
    /// Synchronizes transactions for a specific connection
    /// </summary>
    Task<BankApiImportResult> SyncTransactionsAsync(
        int connectionId,
        string accountId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool skipDuplicates = true);
}
