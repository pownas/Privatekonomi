using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Data;

public class PrivatekonomyContext : DbContext
{
    public PrivatekonomyContext(DbContextOptions<PrivatekonomyContext> options)
        : base(options)
    {
    }

    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<TransactionCategory> TransactionCategories { get; set; }
    public DbSet<Loan> Loans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).IsRequired().HasMaxLength(7);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Date).IsRequired();
        });

        modelBuilder.Entity<TransactionCategory>(entity =>
        {
            entity.HasKey(e => e.TransactionCategoryId);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            
            entity.HasOne(e => e.Transaction)
                .WithMany(t => t.TransactionCategories)
                .HasForeignKey(e => e.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Category)
                .WithMany(c => c.TransactionCategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(e => e.LoanId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.InterestRate).HasPrecision(5, 2);
            entity.Property(e => e.Amortization).HasPrecision(18, 2);
        });

        // Seed initial categories
        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, Name = "Mat & Dryck", Color = "#FF6B6B" },
            new Category { CategoryId = 2, Name = "Transport", Color = "#4ECDC4" },
            new Category { CategoryId = 3, Name = "Boende", Color = "#45B7D1" },
            new Category { CategoryId = 4, Name = "Nöje", Color = "#FFA07A" },
            new Category { CategoryId = 5, Name = "Shopping", Color = "#98D8C8" },
            new Category { CategoryId = 6, Name = "Hälsa", Color = "#6BCF7F" },
            new Category { CategoryId = 7, Name = "Lön", Color = "#4CAF50" },
            new Category { CategoryId = 8, Name = "Sparande", Color = "#2196F3" },
            new Category { CategoryId = 9, Name = "Övrigt", Color = "#9E9E9E" }
        );
    }
}
