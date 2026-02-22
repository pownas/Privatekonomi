using Privatekonomi.Core.Services;

namespace Privatekonomi.Api.Tests.Infrastructure;

public sealed class TestCurrentUserService : ICurrentUserService
{
    public bool IsAuthenticated { get; init; }
    public string? UserId { get; init; }
}