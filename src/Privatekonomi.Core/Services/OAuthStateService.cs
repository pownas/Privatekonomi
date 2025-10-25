using Microsoft.Extensions.Caching.Memory;

namespace Privatekonomi.Core.Services;

/// <summary>
/// OAuth state service using in-memory cache for state validation
/// </summary>
public class OAuthStateService : IOAuthStateService
{
    private readonly IMemoryCache _cache;
    private const int StateExpirationMinutes = 15;

    public OAuthStateService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public string GenerateState(string bankName)
    {
        var state = $"{Guid.NewGuid():N}";
        var cacheKey = GetCacheKey(state);

        // Store state with bank name and expiration
        _cache.Set(cacheKey, bankName, TimeSpan.FromMinutes(StateExpirationMinutes));

        return state;
    }

    public bool ValidateState(string state, string bankName)
    {
        if (string.IsNullOrEmpty(state))
            return false;

        var cacheKey = GetCacheKey(state);
        
        if (_cache.TryGetValue(cacheKey, out string? cachedBankName))
        {
            // Validate that the bank name matches
            return string.Equals(cachedBankName, bankName, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public void RemoveState(string state)
    {
        if (!string.IsNullOrEmpty(state))
        {
            var cacheKey = GetCacheKey(state);
            _cache.Remove(cacheKey);
        }
    }

    private static string GetCacheKey(string state) => $"oauth_state_{state}";
}
