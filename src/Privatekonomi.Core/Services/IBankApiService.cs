using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Interface for bank API integrations (PSD2 and proprietary APIs)
/// </summary>
public interface IBankApiService
{
    /// <summary>
    /// Bank name this service handles
    /// </summary>
    string BankName { get; }
    
    /// <summary>
    /// Initiates OAuth2 authorization flow and returns authorization URL
    /// </summary>
    Task<string> GetAuthorizationUrlAsync(string redirectUri, string state);
    
    /// <summary>
    /// Exchanges authorization code for access token
    /// </summary>
    Task<BankConnection> ExchangeCodeForTokenAsync(string code, string redirectUri);
    
    /// <summary>
    /// Refreshes an expired access token
    /// </summary>
    Task<BankConnection> RefreshTokenAsync(BankConnection connection);
    
    /// <summary>
    /// Gets list of accounts from the bank
    /// </summary>
    Task<List<BankApiAccount>> GetAccountsAsync(BankConnection connection);
    
    /// <summary>
    /// Fetches transactions for a specific account and date range
    /// </summary>
    Task<List<BankApiTransaction>> GetTransactionsAsync(
        BankConnection connection, 
        string accountId, 
        DateTime fromDate, 
        DateTime toDate);
    
    /// <summary>
    /// Imports transactions from bank API to database
    /// </summary>
    Task<BankApiImportResult> ImportTransactionsAsync(
        BankConnection connection,
        string accountId,
        DateTime fromDate,
        DateTime toDate,
        bool skipDuplicates = true);
}
