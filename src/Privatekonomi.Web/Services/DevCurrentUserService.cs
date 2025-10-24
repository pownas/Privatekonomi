using Microsoft.AspNetCore.Identity;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;

namespace Privatekonomi.Web.Services;

/// <summary>
/// Development-only implementation of ICurrentUserService that bypasses authentication.
/// This service returns the test user ID when the DevDisableAuth feature flag is enabled.
/// WARNING: This should ONLY be used in Development environment for testing purposes.
/// </summary>
public class DevCurrentUserService : ICurrentUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private string? _cachedUserId;

    public DevCurrentUserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Returns the test user ID from the seeded test data.
    /// This matches the user created in TestDataSeeder (test@example.com).
    /// </summary>
    public string? UserId
    {
        get
        {
            if (_cachedUserId == null)
            {
                // Get the test user synchronously (not ideal but necessary for property)
                var testUser = _userManager.FindByEmailAsync("test@example.com").GetAwaiter().GetResult();
                _cachedUserId = testUser?.Id;
            }
            return _cachedUserId;
        }
    }

    public bool IsAuthenticated => true;
}
