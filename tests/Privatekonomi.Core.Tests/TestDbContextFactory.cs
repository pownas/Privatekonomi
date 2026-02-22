using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;

namespace Privatekonomi.Core.Tests;

internal sealed class TestDbContextFactory(DbContextOptions<PrivatekonomyContext> options)
    : IDbContextFactory<PrivatekonomyContext>
{
    public PrivatekonomyContext CreateDbContext() => new(options);
}
