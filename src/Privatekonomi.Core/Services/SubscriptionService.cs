using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly PrivatekonomyContext _context;
    private readonly IAuditLogService _auditLogService;

    public SubscriptionService(PrivatekonomyContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<List<Subscription>> GetSubscriptionsAsync(string userId)
    {
        return await _context.Subscriptions
            .Where(s => s.UserId == userId)
            .Include(s => s.Category)
            .Include(s => s.PriceHistory)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<List<Subscription>> GetActiveSubscriptionsAsync(string userId)
    {
        return await _context.Subscriptions
            .Where(s => s.UserId == userId && s.IsActive)
            .Include(s => s.Category)
            .Include(s => s.PriceHistory)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Subscription?> GetSubscriptionByIdAsync(int subscriptionId, string userId)
    {
        return await _context.Subscriptions
            .Where(s => s.SubscriptionId == subscriptionId && s.UserId == userId)
            .Include(s => s.Category)
            .Include(s => s.PriceHistory)
            .FirstOrDefaultAsync();
    }

    public async Task<Subscription> CreateSubscriptionAsync(Subscription subscription)
    {
        subscription.CreatedAt = DateTime.UtcNow;
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        
        await _auditLogService.LogAsync("Create", "Subscription", subscription.SubscriptionId, 
            $"Created subscription: {subscription.Name} - {subscription.Price:C} {subscription.BillingFrequency}", subscription.UserId);
        
        return subscription;
    }

    public async Task<Subscription> UpdateSubscriptionAsync(Subscription subscription)
    {
        subscription.UpdatedAt = DateTime.UtcNow;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();
        
        await _auditLogService.LogAsync("Update", "Subscription", subscription.SubscriptionId, 
            $"Updated subscription: {subscription.Name}", subscription.UserId);
        
        return subscription;
    }

    public async Task DeleteSubscriptionAsync(int subscriptionId, string userId)
    {
        var subscription = await GetSubscriptionByIdAsync(subscriptionId, userId);
        if (subscription != null)
        {
            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
            
            await _auditLogService.LogAsync("Delete", "Subscription", subscriptionId, 
                $"Deleted subscription: {subscription.Name}", userId);
        }
    }

    public async Task<decimal> GetMonthlySubscriptionCostAsync(string userId)
    {
        var subscriptions = await GetActiveSubscriptionsAsync(userId);
        decimal monthlyCost = 0;

        foreach (var subscription in subscriptions)
        {
            monthlyCost += subscription.BillingFrequency.ToLower() switch
            {
                "monthly" => subscription.Price,
                "yearly" => subscription.Price / 12,
                "quarterly" => subscription.Price / 3,
                _ => 0
            };
        }

        return monthlyCost;
    }

    public async Task<decimal> GetYearlySubscriptionCostAsync(string userId)
    {
        var subscriptions = await GetActiveSubscriptionsAsync(userId);
        decimal yearlyCost = 0;

        foreach (var subscription in subscriptions)
        {
            yearlyCost += subscription.BillingFrequency.ToLower() switch
            {
                "monthly" => subscription.Price * 12,
                "yearly" => subscription.Price,
                "quarterly" => subscription.Price * 4,
                _ => 0
            };
        }

        return yearlyCost;
    }

    public async Task<List<Subscription>> GetSubscriptionsDueSoonAsync(string userId, int daysAhead)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        return await _context.Subscriptions
            .Where(s => s.UserId == userId && s.IsActive && 
                       s.NextBillingDate != null && s.NextBillingDate <= cutoffDate)
            .Include(s => s.Category)
            .OrderBy(s => s.NextBillingDate)
            .ToListAsync();
    }

    public async Task<List<SubscriptionPriceHistory>> GetPriceChangesAsync(string userId)
    {
        return await _context.SubscriptionPriceHistory
            .Include(ph => ph.Subscription)
            .Where(ph => ph.Subscription != null && ph.Subscription.UserId == userId)
            .OrderByDescending(ph => ph.ChangeDate)
            .ToListAsync();
    }

    public async Task AddPriceChangeAsync(int subscriptionId, decimal newPrice, string? reason)
    {
        var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
        if (subscription == null)
            throw new InvalidOperationException("Subscription not found");

        var priceHistory = new SubscriptionPriceHistory
        {
            SubscriptionId = subscriptionId,
            OldPrice = subscription.Price,
            NewPrice = newPrice,
            ChangeDate = DateTime.UtcNow,
            Reason = reason,
            NotificationSent = false,
            CreatedAt = DateTime.UtcNow
        };

        subscription.Price = newPrice;
        subscription.UpdatedAt = DateTime.UtcNow;

        _context.SubscriptionPriceHistory.Add(priceHistory);
        await _context.SaveChangesAsync();
        
        await _auditLogService.LogAsync("PriceChange", "Subscription", subscriptionId, 
            $"Price changed for {subscription.Name}: {priceHistory.OldPrice:C} -> {newPrice:C}", subscription.UserId);
    }
}
