using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class RoundUpService : IRoundUpService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;
    private readonly IGoalService _goalService;

    public RoundUpService(
        PrivatekonomyContext context,
        IGoalService goalService,
        ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _currentUserService = currentUserService;
        _goalService = goalService;
    }

    public async Task<RoundUpSettings> GetOrCreateSettingsAsync()
    {
        var userId = _currentUserService?.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException("User must be authenticated");
        }

        var settings = await _context.RoundUpSettings
            .Include(s => s.TargetGoal)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            settings = new RoundUpSettings
            {
                UserId = userId,
                IsEnabled = false,
                RoundUpAmount = 10M,
                OnlyExpenses = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.RoundUpSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return settings;
    }

    public async Task<RoundUpSettings> UpdateSettingsAsync(RoundUpSettings settings)
    {
        var userId = _currentUserService?.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException("User must be authenticated");
        }

        // Ensure the settings belong to the current user
        if (settings.UserId != userId)
        {
            throw new UnauthorizedAccessException("Cannot update settings for another user");
        }

        settings.UpdatedAt = DateTime.UtcNow;
        _context.Entry(settings).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return settings;
    }

    public decimal CalculateRoundUp(decimal amount, decimal roundUpTo = 10M)
    {
        // Make amount positive for calculation
        var absAmount = Math.Abs(amount);
        
        // Calculate the rounded-up amount
        var roundedAmount = Math.Ceiling(absAmount / roundUpTo) * roundUpTo;
        
        // Return the difference (what to save)
        return roundedAmount - absAmount;
    }

    public async Task<RoundUpTransaction?> ProcessRoundUpForTransactionAsync(int transactionId)
    {
        var userId = _currentUserService?.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        // Get settings
        var settings = await GetOrCreateSettingsAsync();
        if (!settings.IsEnabled || settings.TargetGoalId == null)
        {
            return null;
        }

        // Get transaction
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.UserId == userId);

        if (transaction == null)
        {
            return null;
        }

        // Check if already processed
        var existingRoundUp = await _context.RoundUpTransactions
            .FirstOrDefaultAsync(r => r.TransactionId == transactionId);
        if (existingRoundUp != null)
        {
            return existingRoundUp;
        }

        // Apply filters
        if (settings.OnlyExpenses && transaction.IsIncome)
        {
            return null;
        }

        var absAmount = Math.Abs(transaction.Amount);
        
        if (settings.MinimumTransactionAmount.HasValue && absAmount < settings.MinimumTransactionAmount.Value)
        {
            return null;
        }

        if (settings.MaximumTransactionAmount.HasValue && absAmount > settings.MaximumTransactionAmount.Value)
        {
            return null;
        }

        // Calculate round-up
        var roundUpAmount = CalculateRoundUp(absAmount, settings.RoundUpAmount);
        
        // If no round-up needed (already rounded), skip
        if (roundUpAmount == 0)
        {
            return null;
        }

        var roundedAmount = absAmount + roundUpAmount;

        // Calculate employer matching
        var employerMatchingAmount = 0M;
        if (settings.EnableEmployerMatching)
        {
            employerMatchingAmount = roundUpAmount * (settings.EmployerMatchingPercentage / 100M);
            
            // Check monthly limit
            if (settings.EmployerMatchingMonthlyLimit.HasValue)
            {
                var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var currentMonthMatching = await _context.RoundUpTransactions
                    .Where(r => r.UserId == userId && 
                               r.CreatedAt >= monthStart && 
                               r.Type == RoundUpType.StandardRoundUp)
                    .SumAsync(r => r.EmployerMatchingAmount);

                var remainingLimit = settings.EmployerMatchingMonthlyLimit.Value - currentMonthMatching;
                employerMatchingAmount = Math.Min(employerMatchingAmount, remainingLimit);
            }
        }

        var totalSaved = roundUpAmount + employerMatchingAmount;

        // Create round-up transaction
        var roundUpTransaction = new RoundUpTransaction
        {
            TransactionId = transactionId,
            OriginalAmount = absAmount,
            RoundedAmount = roundedAmount,
            RoundUpAmount = roundUpAmount,
            EmployerMatchingAmount = employerMatchingAmount,
            TotalSaved = totalSaved,
            GoalId = settings.TargetGoalId,
            Type = RoundUpType.StandardRoundUp,
            IsAutomatic = true,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        _context.RoundUpTransactions.Add(roundUpTransaction);

        // Update goal current amount
        if (settings.TargetGoalId.HasValue)
        {
            var goal = await _context.Goals.FindAsync(settings.TargetGoalId.Value);
            if (goal != null)
            {
                goal.CurrentAmount += totalSaved;
                goal.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return roundUpTransaction;
    }

    public async Task<RoundUpTransaction?> ProcessSalaryAutoSaveAsync(int transactionId)
    {
        var userId = _currentUserService?.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        // Get settings
        var settings = await GetOrCreateSettingsAsync();
        if (!settings.EnableSalaryAutoSave || settings.TargetGoalId == null)
        {
            return null;
        }

        // Get transaction
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.UserId == userId);

        if (transaction == null || !transaction.IsIncome)
        {
            return null;
        }

        // Check if already processed
        var existingAutoSave = await _context.RoundUpTransactions
            .FirstOrDefaultAsync(r => r.TransactionId == transactionId && r.Type == RoundUpType.SalaryAutoSave);
        if (existingAutoSave != null)
        {
            return existingAutoSave;
        }

        // Calculate auto-save amount
        var autoSaveAmount = transaction.Amount * (settings.SalaryAutoSavePercentage / 100M);

        // Create round-up transaction
        var roundUpTransaction = new RoundUpTransaction
        {
            TransactionId = transactionId,
            OriginalAmount = transaction.Amount,
            RoundedAmount = transaction.Amount,
            RoundUpAmount = autoSaveAmount,
            EmployerMatchingAmount = 0,
            TotalSaved = autoSaveAmount,
            GoalId = settings.TargetGoalId,
            Type = RoundUpType.SalaryAutoSave,
            IsAutomatic = true,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        _context.RoundUpTransactions.Add(roundUpTransaction);

        // Update goal current amount
        if (settings.TargetGoalId.HasValue)
        {
            var goal = await _context.Goals.FindAsync(settings.TargetGoalId.Value);
            if (goal != null)
            {
                goal.CurrentAmount += autoSaveAmount;
                goal.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return roundUpTransaction;
    }

    public async Task<IEnumerable<RoundUpTransaction>> GetRoundUpTransactionsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var userId = _currentUserService?.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Enumerable.Empty<RoundUpTransaction>();
        }

        var query = _context.RoundUpTransactions
            .Include(r => r.Transaction)
            .Include(r => r.Goal)
            .Where(r => r.UserId == userId)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= toDate.Value);
        }

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<RoundUpStatistics> GetStatisticsAsync(DateTime fromDate, DateTime toDate)
    {
        var userId = _currentUserService?.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return new RoundUpStatistics();
        }

        var transactions = await _context.RoundUpTransactions
            .Where(r => r.UserId == userId && 
                       r.CreatedAt >= fromDate && 
                       r.CreatedAt <= toDate)
            .ToListAsync();

        if (!transactions.Any())
        {
            return new RoundUpStatistics();
        }

        var standardRoundUps = transactions.Where(r => r.Type == RoundUpType.StandardRoundUp).ToList();
        var salaryAutoSaves = transactions.Where(r => r.Type == RoundUpType.SalaryAutoSave).ToList();

        return new RoundUpStatistics
        {
            TotalRoundUp = standardRoundUps.Sum(r => r.RoundUpAmount),
            TotalEmployerMatching = transactions.Sum(r => r.EmployerMatchingAmount),
            TotalSalaryAutoSave = salaryAutoSaves.Sum(r => r.RoundUpAmount),
            TotalSaved = transactions.Sum(r => r.TotalSaved),
            TransactionCount = transactions.Count,
            AverageRoundUp = standardRoundUps.Any() ? standardRoundUps.Average(r => r.RoundUpAmount) : 0,
            LargestRoundUp = standardRoundUps.Any() ? standardRoundUps.Max(r => r.RoundUpAmount) : 0
        };
    }

    public async Task<decimal> GetMonthlyTotalAsync()
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var monthEnd = monthStart.AddMonths(1);
        
        var stats = await GetStatisticsAsync(monthStart, monthEnd);
        return stats.TotalSaved;
    }
}
