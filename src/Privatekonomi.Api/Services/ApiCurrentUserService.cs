using Privatekonomi.Core.Services;

namespace Privatekonomi.Api.Services;

/// <summary>
/// API implementation of ICurrentUserService.
/// In development/demo mode, returns null to allow anonymous access.
/// In production, this should be replaced with proper authentication.
/// </summary>
public class ApiCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Returns the current user ID from authentication claims.
    /// Returns null if user is not authenticated.
    /// </summary>
    public string? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                return user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }
            return null;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
}
