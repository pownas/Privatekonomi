using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Privatekonomi.Core.Data;

/// <summary>
/// Design-time factory for creating PrivatekonomyContext for EF migrations
/// </summary>
public class PrivatekonomyContextFactory : IDesignTimeDbContextFactory<PrivatekonomyContext>
{
    public PrivatekonomyContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PrivatekonomyContext>();
        
        // Use SQLite for migrations
        optionsBuilder.UseSqlite("Data Source=privatekonomi.db");
        
        return new PrivatekonomyContext(optionsBuilder.Options);
    }
}
