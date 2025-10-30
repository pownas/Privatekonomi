using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface ISubscriptionService
{
    Task<List<Subscription>> GetSubscriptionsAsync(string userId);
    Task<List<Subscription>> GetActiveSubscriptionsAsync(string userId);
    Task<Subscription?> GetSubscriptionByIdAsync(int subscriptionId, string userId);
    Task<Subscription> CreateSubscriptionAsync(Subscription subscription);
    Task<Subscription> UpdateSubscriptionAsync(Subscription subscription);
    Task DeleteSubscriptionAsync(int subscriptionId, string userId);
    Task<decimal> GetMonthlySubscriptionCostAsync(string userId);
    Task<decimal> GetYearlySubscriptionCostAsync(string userId);
    Task<List<Subscription>> GetSubscriptionsDueSoonAsync(string userId, int daysAhead);
    Task<List<SubscriptionPriceHistory>> GetPriceChangesAsync(string userId);
    Task AddPriceChangeAsync(int subscriptionId, decimal newPrice, string? reason);
    
    // Smart contract management features
    Task<List<Subscription>> GetUnusedSubscriptionsAsync(string userId, int daysUnused = 45);
    Task<List<Subscription>> GetSubscriptionsWithUpcomingCancellationDeadlineAsync(string userId, int daysAhead = 30);
    Task UpdateLastUsedDateAsync(int subscriptionId, DateTime? lastUsedDate);
    Task<List<Subscription>> DetectSubscriptionsFromTransactionsAsync(string userId);
    Task<Subscription?> CreateSubscriptionFromTransactionAsync(int transactionId, string userId);
}
