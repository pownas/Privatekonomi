using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for managing bank API connections
/// </summary>
public class BankConnectionService : IBankConnectionService
{
    private readonly PrivatekonomyContext _context;
    private readonly Dictionary<string, IBankApiService> _bankApiServices;

    public BankConnectionService(
        PrivatekonomyContext context,
        IEnumerable<IBankApiService> bankApiServices)
    {
        _context = context;
        _bankApiServices = bankApiServices.ToDictionary(s => s.BankName, s => s, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<List<BankConnection>> GetConnectionsAsync(int? bankSourceId = null)
    {
        var query = _context.BankConnections
            .Include(c => c.BankSource)
            .AsQueryable();

        if (bankSourceId.HasValue)
        {
            query = query.Where(c => c.BankSourceId == bankSourceId.Value);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<BankConnection?> GetConnectionAsync(int connectionId)
    {
        return await _context.BankConnections
            .Include(c => c.BankSource)
            .FirstOrDefaultAsync(c => c.BankConnectionId == connectionId);
    }

    public async Task<BankConnection> CreateConnectionAsync(BankConnection connection)
    {
        connection.CreatedAt = DateTime.UtcNow;
        _context.BankConnections.Add(connection);
        await _context.SaveChangesAsync();
        return connection;
    }

    public async Task<BankConnection> UpdateConnectionAsync(BankConnection connection)
    {
        connection.UpdatedAt = DateTime.UtcNow;
        _context.BankConnections.Update(connection);
        await _context.SaveChangesAsync();
        return connection;
    }

    public async Task DeleteConnectionAsync(int connectionId)
    {
        var connection = await _context.BankConnections.FindAsync(connectionId);
        if (connection != null)
        {
            _context.BankConnections.Remove(connection);
            await _context.SaveChangesAsync();
        }
    }

    public IBankApiService? GetBankApiService(string bankName)
    {
        return _bankApiServices.TryGetValue(bankName, out var service) ? service : null;
    }

    public List<string> GetAvailableBanks()
    {
        return _bankApiServices.Keys.ToList();
    }

    public async Task<BankApiImportResult> SyncTransactionsAsync(
        int connectionId,
        string accountId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool skipDuplicates = true)
    {
        var connection = await GetConnectionAsync(connectionId);
        if (connection == null)
            throw new Exception($"Bank connection {connectionId} not found");

        if (connection.BankSource == null)
            throw new Exception("Bank source not found for connection");

        var bankService = GetBankApiService(connection.BankSource.Name);
        if (bankService == null)
            throw new Exception($"No API service available for {connection.BankSource.Name}");

        // Default date range: last 90 days
        var from = fromDate ?? (connection.LastSyncedAt ?? DateTime.Now.AddDays(-90));
        var to = toDate ?? DateTime.Now;

        return await bankService.ImportTransactionsAsync(connection, accountId, from, to, skipDuplicates);
    }
}
