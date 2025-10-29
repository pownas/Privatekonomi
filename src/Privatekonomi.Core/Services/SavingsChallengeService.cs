using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class SavingsChallengeService : ISavingsChallengeService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;

    public SavingsChallengeService(PrivatekonomyContext context, ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<SavingsChallenge>> GetAllChallengesAsync()
    {
        var query = _context.SavingsChallenges
            .Include(c => c.ProgressEntries)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(c => c.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public async Task<SavingsChallenge?> GetChallengeByIdAsync(int id)
    {
        var query = _context.SavingsChallenges
            .Include(c => c.ProgressEntries)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(c => c.UserId == _currentUserService.UserId);
        }

        return await query.FirstOrDefaultAsync(c => c.SavingsChallengeId == id);
    }

    public async Task<SavingsChallenge> CreateChallengeAsync(SavingsChallenge challenge)
    {
        challenge.CreatedAt = DateTime.UtcNow;
        challenge.StartDate = challenge.StartDate == default ? DateTime.UtcNow : challenge.StartDate;
        challenge.EndDate = challenge.StartDate.AddDays(challenge.DurationDays);
        
        // Only set status to Active if not explicitly set
        if (challenge.Status == default)
        {
            challenge.Status = ChallengeStatus.Active;
        }
        
        // Set user ID for new challenges
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            challenge.UserId = _currentUserService.UserId;
        }
        
        _context.SavingsChallenges.Add(challenge);
        await _context.SaveChangesAsync();
        return challenge;
    }

    public async Task<SavingsChallenge> UpdateChallengeAsync(SavingsChallenge challenge)
    {
        challenge.UpdatedAt = DateTime.UtcNow;
        _context.Entry(challenge).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return challenge;
    }

    public async Task DeleteChallengeAsync(int id)
    {
        var challenge = await _context.SavingsChallenges.FindAsync(id);
        if (challenge != null)
        {
            _context.SavingsChallenges.Remove(challenge);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<SavingsChallenge>> GetActiveChallengesAsync()
    {
        var query = _context.SavingsChallenges
            .Include(c => c.ProgressEntries)
            .Where(c => c.Status == ChallengeStatus.Active)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(c => c.UserId == _currentUserService.UserId);
        }

        return await query.OrderBy(c => c.EndDate).ToListAsync();
    }

    public async Task<IEnumerable<SavingsChallenge>> GetCompletedChallengesAsync()
    {
        var query = _context.SavingsChallenges
            .Include(c => c.ProgressEntries)
            .Where(c => c.Status == ChallengeStatus.Completed)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(c => c.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(c => c.UpdatedAt).ToListAsync();
    }

    public async Task<IEnumerable<SavingsChallenge>> GetChallengesByTypeAsync(ChallengeType type)
    {
        var query = _context.SavingsChallenges
            .Include(c => c.ProgressEntries)
            .Where(c => c.Type == type)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(c => c.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public async Task<SavingsChallengeProgress> RecordProgressAsync(int challengeId, DateTime date, bool completed, decimal amountSaved, string? notes = null)
    {
        var challenge = await _context.SavingsChallenges
            .Include(c => c.ProgressEntries)
            .FirstOrDefaultAsync(c => c.SavingsChallengeId == challengeId);

        if (challenge == null)
        {
            throw new InvalidOperationException($"Challenge with ID {challengeId} not found.");
        }

        // Check if progress for this date already exists
        var existingProgress = challenge.ProgressEntries.FirstOrDefault(p => p.Date.Date == date.Date);
        
        if (existingProgress != null)
        {
            // Update existing progress
            existingProgress.Completed = completed;
            existingProgress.AmountSaved = amountSaved;
            existingProgress.Notes = notes;
            _context.Entry(existingProgress).State = EntityState.Modified;
        }
        else
        {
            // Create new progress entry
            var progress = new SavingsChallengeProgress
            {
                SavingsChallengeId = challengeId,
                Date = date.Date,
                Completed = completed,
                AmountSaved = amountSaved,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.SavingsChallengeProgress.Add(progress);
            existingProgress = progress;
        }

        // Update challenge current amount and streak
        challenge.CurrentAmount += amountSaved;
        challenge.CurrentStreak = await CalculateCurrentStreakAsync(challengeId);
        
        if (challenge.CurrentStreak > challenge.BestStreak)
        {
            challenge.BestStreak = challenge.CurrentStreak;
        }

        // Check if challenge is completed
        if (challenge.DaysCompleted >= challenge.DurationDays || challenge.CurrentAmount >= challenge.TargetAmount)
        {
            challenge.Status = ChallengeStatus.Completed;
        }

        challenge.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return existingProgress;
    }

    public async Task<IEnumerable<SavingsChallengeProgress>> GetChallengeProgressAsync(int challengeId)
    {
        return await _context.SavingsChallengeProgress
            .Where(p => p.SavingsChallengeId == challengeId)
            .OrderBy(p => p.Date)
            .ToListAsync();
    }

    public async Task UpdateChallengeStatusAsync(int challengeId, ChallengeStatus status)
    {
        var challenge = await _context.SavingsChallenges.FindAsync(challengeId);
        if (challenge != null)
        {
            challenge.Status = status;
            challenge.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> CalculateCurrentStreakAsync(int challengeId)
    {
        var progressEntries = await _context.SavingsChallengeProgress
            .Where(p => p.SavingsChallengeId == challengeId && p.Completed)
            .OrderByDescending(p => p.Date)
            .ToListAsync();

        if (!progressEntries.Any())
        {
            return 0;
        }

        int streak = 0;
        DateTime expectedDate = DateTime.UtcNow.Date;

        foreach (var entry in progressEntries)
        {
            if (entry.Date.Date == expectedDate || entry.Date.Date == expectedDate.AddDays(-1))
            {
                streak++;
                expectedDate = entry.Date.Date.AddDays(-1);
            }
            else
            {
                break;
            }
        }

        return streak;
    }

    public async Task<int> GetTotalActiveChallengesAsync()
    {
        var query = _context.SavingsChallenges
            .Where(c => c.Status == ChallengeStatus.Active)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(c => c.UserId == _currentUserService.UserId);
        }

        return await query.CountAsync();
    }

    public async Task<int> GetTotalCompletedChallengesAsync()
    {
        var query = _context.SavingsChallenges
            .Where(c => c.Status == ChallengeStatus.Completed)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(c => c.UserId == _currentUserService.UserId);
        }

        return await query.CountAsync();
    }

    public async Task<decimal> GetTotalAmountSavedAsync()
    {
        var query = _context.SavingsChallenges.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(c => c.UserId == _currentUserService.UserId);
        }

        return await query.SumAsync(c => c.CurrentAmount);
    }
}
