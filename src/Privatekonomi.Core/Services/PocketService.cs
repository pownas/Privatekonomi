using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class PocketService : IPocketService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;

    public PocketService(PrivatekonomyContext context, ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<Pocket>> GetAllPocketsAsync()
    {
        var query = _context.Pockets
            .Include(p => p.BankSource)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(p => p.UserId == _currentUserService.UserId);
        }

        return await query.OrderBy(p => p.BankSourceId).ThenBy(p => p.Priority).ToListAsync();
    }

    public async Task<IEnumerable<Pocket>> GetPocketsByBankSourceAsync(int bankSourceId)
    {
        var query = _context.Pockets
            .Include(p => p.BankSource)
            .Where(p => p.BankSourceId == bankSourceId)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(p => p.UserId == _currentUserService.UserId);
        }

        return await query.OrderBy(p => p.Priority).ToListAsync();
    }

    public async Task<Pocket?> GetPocketByIdAsync(int id)
    {
        var query = _context.Pockets
            .Include(p => p.BankSource)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(p => p.UserId == _currentUserService.UserId);
        }

        return await query.FirstOrDefaultAsync(p => p.PocketId == id);
    }

    public async Task<Pocket> CreatePocketAsync(Pocket pocket)
    {
        pocket.CreatedAt = DateTime.UtcNow;
        
        // Set user ID for new pockets
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            pocket.UserId = _currentUserService.UserId;
        }
        
        _context.Pockets.Add(pocket);
        await _context.SaveChangesAsync();
        return pocket;
    }

    public async Task<Pocket> UpdatePocketAsync(Pocket pocket)
    {
        pocket.UpdatedAt = DateTime.UtcNow;
        _context.Entry(pocket).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return pocket;
    }

    public async Task DeletePocketAsync(int id)
    {
        var pocket = await _context.Pockets.FindAsync(id);
        if (pocket != null)
        {
            _context.Pockets.Remove(pocket);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Pocket> AllocateMoneyAsync(int pocketId, decimal amount, string? description = null)
    {
        var pocket = await _context.Pockets.FindAsync(pocketId);
        if (pocket == null)
        {
            throw new InvalidOperationException($"Ficka med ID {pocketId} hittades inte.");
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Beloppet måste vara större än noll.", nameof(amount));
        }

        pocket.CurrentAmount += amount;
        pocket.UpdatedAt = DateTime.UtcNow;

        var transaction = new PocketTransaction
        {
            PocketId = pocketId,
            Amount = amount,
            Type = "Deposit",
            Description = description ?? "Insättning",
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UserId = _currentUserService?.UserId
        };

        _context.PocketTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        
        return pocket;
    }

    public async Task<Pocket> WithdrawMoneyAsync(int pocketId, decimal amount, string? description = null)
    {
        var pocket = await _context.Pockets.FindAsync(pocketId);
        if (pocket == null)
        {
            throw new InvalidOperationException($"Ficka med ID {pocketId} hittades inte.");
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Beloppet måste vara större än noll.", nameof(amount));
        }

        if (pocket.CurrentAmount < amount)
        {
            throw new InvalidOperationException($"Otillräckligt saldo i fickan. Tillgängligt: {pocket.CurrentAmount:C}, Begärt: {amount:C}");
        }

        pocket.CurrentAmount -= amount;
        pocket.UpdatedAt = DateTime.UtcNow;

        var transaction = new PocketTransaction
        {
            PocketId = pocketId,
            Amount = -amount,
            Type = "Withdrawal",
            Description = description ?? "Uttag",
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UserId = _currentUserService?.UserId
        };

        _context.PocketTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        
        return pocket;
    }

    public async Task TransferMoneyAsync(int fromPocketId, int toPocketId, decimal amount, string? description = null)
    {
        if (fromPocketId == toPocketId)
        {
            throw new ArgumentException("Från-ficka och till-ficka kan inte vara densamma.");
        }

        var fromPocket = await _context.Pockets.FindAsync(fromPocketId);
        var toPocket = await _context.Pockets.FindAsync(toPocketId);

        if (fromPocket == null)
        {
            throw new InvalidOperationException($"Från-ficka med ID {fromPocketId} hittades inte.");
        }

        if (toPocket == null)
        {
            throw new InvalidOperationException($"Till-ficka med ID {toPocketId} hittades inte.");
        }

        if (fromPocket.BankSourceId != toPocket.BankSourceId)
        {
            throw new InvalidOperationException("Fickor måste tillhöra samma sparkonto för överföring.");
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Beloppet måste vara större än noll.", nameof(amount));
        }

        if (fromPocket.CurrentAmount < amount)
        {
            throw new InvalidOperationException($"Otillräckligt saldo i från-fickan. Tillgängligt: {fromPocket.CurrentAmount:C}, Begärt: {amount:C}");
        }

        fromPocket.CurrentAmount -= amount;
        fromPocket.UpdatedAt = DateTime.UtcNow;

        toPocket.CurrentAmount += amount;
        toPocket.UpdatedAt = DateTime.UtcNow;

        var withdrawalTransaction = new PocketTransaction
        {
            PocketId = fromPocketId,
            Amount = -amount,
            Type = "Transfer",
            Description = description ?? $"Överföring till {toPocket.Name}",
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            RelatedPocketId = toPocketId,
            UserId = _currentUserService?.UserId
        };

        var depositTransaction = new PocketTransaction
        {
            PocketId = toPocketId,
            Amount = amount,
            Type = "Transfer",
            Description = description ?? $"Överföring från {fromPocket.Name}",
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            RelatedPocketId = fromPocketId,
            UserId = _currentUserService?.UserId
        };

        _context.PocketTransactions.Add(withdrawalTransaction);
        _context.PocketTransactions.Add(depositTransaction);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<PocketTransaction>> GetPocketTransactionsAsync(int pocketId)
    {
        return await _context.PocketTransactions
            .Where(pt => pt.PocketId == pocketId)
            .OrderByDescending(pt => pt.TransactionDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalAllocatedForBankSourceAsync(int bankSourceId)
    {
        var query = _context.Pockets
            .Where(p => p.BankSourceId == bankSourceId)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(p => p.UserId == _currentUserService.UserId);
        }

        return await query.SumAsync(p => p.CurrentAmount);
    }
}
