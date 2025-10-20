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
    public DbSet<Investment> Investments { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<BudgetCategory> BudgetCategories { get; set; }
    public DbSet<BankSource> BankSources { get; set; }
    public DbSet<Household> Households { get; set; }
    public DbSet<HouseholdMember> HouseholdMembers { get; set; }
    public DbSet<SharedExpense> SharedExpenses { get; set; }
    public DbSet<ExpenseShare> ExpenseShares { get; set; }
    public DbSet<Goal> Goals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BankSource>(entity =>
        {
            entity.HasKey(e => e.BankSourceId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).IsRequired().HasMaxLength(7);
            entity.Property(e => e.Logo).HasMaxLength(500);
            entity.Property(e => e.AccountType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.Institution).HasMaxLength(200);
            entity.Property(e => e.InitialBalance).HasPrecision(18, 2);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Ignore computed property
            entity.Ignore(e => e.CurrentBalance);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).IsRequired().HasMaxLength(7);
            entity.Property(e => e.DefaultBudgetMonthly).HasPrecision(18, 2);
            entity.Property(e => e.TaxRelated).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Self-referencing relationship for hierarchical categories
            entity.HasOne(e => e.Parent)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.Payee).HasMaxLength(200);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.ImportSource).HasMaxLength(100);
            entity.Property(e => e.Imported).IsRequired();
            entity.Property(e => e.Cleared).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Additional properties from OpenAPI spec
            entity.Property(e => e.Payee).HasMaxLength(200);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            
            entity.HasOne(e => e.BankSource)
                .WithMany(b => b.Transactions)
                .HasForeignKey(e => e.BankSourceId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Indexes for performance optimization
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => new { e.BankSourceId, e.Date });
            entity.HasIndex(e => e.Payee);
        });

        modelBuilder.Entity<TransactionCategory>(entity =>
        {
            entity.HasKey(e => e.TransactionCategoryId);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Percentage).HasPrecision(5, 2);
            
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
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.CreatedAt).IsRequired();
        });
        
        modelBuilder.Entity<Investment>(entity =>
        {
            entity.HasKey(e => e.InvestmentId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.PurchasePrice).HasPrecision(18, 2);
            entity.Property(e => e.CurrentPrice).HasPrecision(18, 2);
            entity.Property(e => e.PurchaseDate).IsRequired();
            entity.Property(e => e.LastUpdated).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // New properties for bank and account information
            entity.Property(e => e.AccountNumber).HasMaxLength(50);
            entity.Property(e => e.ShortName).HasMaxLength(50);
            entity.Property(e => e.ISIN).HasMaxLength(12);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Country).HasMaxLength(2);
            entity.Property(e => e.Market).HasMaxLength(50);
            
            // Relation to BankSource
            entity.HasOne(e => e.BankSource)
                .WithMany()
                .HasForeignKey(e => e.BankSourceId)
                .OnDelete(DeleteBehavior.SetNull);
                
            // Indexes for faster searching
            entity.HasIndex(e => e.ISIN);
            entity.HasIndex(e => e.AccountNumber);
            
            // Ignore computed properties
            entity.Ignore(e => e.TotalValue);
            entity.Ignore(e => e.TotalCost);
            entity.Ignore(e => e.ProfitLoss);
            entity.Ignore(e => e.ProfitLossPercentage);
        });

        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.BudgetId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired();
            entity.Property(e => e.Period).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<BudgetCategory>(entity =>
        {
            entity.HasKey(e => e.BudgetCategoryId);
            entity.Property(e => e.PlannedAmount).HasPrecision(18, 2);
            
            entity.HasOne(e => e.Budget)
                .WithMany(b => b.BudgetCategories)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Goal configuration
        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasKey(e => e.GoalId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TargetAmount).HasPrecision(18, 2);
            entity.Property(e => e.CurrentAmount).HasPrecision(18, 2);
            entity.Property(e => e.Priority).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.FundedFromBankSource)
                .WithMany()
                .HasForeignKey(e => e.FundedFromBankSourceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Seed initial categories
        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, Name = "Mat & Dryck", Color = "#FF6B6B", TaxRelated = false, CreatedAt = DateTime.UtcNow },
            new Category { CategoryId = 2, Name = "Transport", Color = "#4ECDC4", TaxRelated = false, CreatedAt = DateTime.UtcNow },
            new Category { CategoryId = 3, Name = "Boende", Color = "#45B7D1", TaxRelated = false, CreatedAt = DateTime.UtcNow },
            new Category { CategoryId = 4, Name = "Nöje", Color = "#FFA07A", TaxRelated = false, CreatedAt = DateTime.UtcNow },
            new Category { CategoryId = 5, Name = "Shopping", Color = "#98D8C8", TaxRelated = false, CreatedAt = DateTime.UtcNow },
            new Category { CategoryId = 6, Name = "Hälsa", Color = "#6BCF7F", TaxRelated = false, CreatedAt = DateTime.UtcNow },
            new Category { CategoryId = 7, Name = "Lön", Color = "#4CAF50", TaxRelated = false, CreatedAt = DateTime.UtcNow },
            new Category { CategoryId = 8, Name = "Sparande", Color = "#2196F3", TaxRelated = false, CreatedAt = DateTime.UtcNow },
            new Category { CategoryId = 9, Name = "Övrigt", Color = "#9E9E9E", TaxRelated = false, CreatedAt = DateTime.UtcNow }
        );

        // Seed initial bank sources
        modelBuilder.Entity<BankSource>().HasData(
            new BankSource { BankSourceId = 1, Name = "ICA-banken", Color = "#DC143C", AccountType = "checking", Currency = "SEK", InitialBalance = 0, CreatedAt = DateTime.UtcNow }, // röd (Crimson)
            new BankSource { BankSourceId = 2, Name = "Swedbank", Color = "#FF8C00", AccountType = "checking", Currency = "SEK", InitialBalance = 0, CreatedAt = DateTime.UtcNow }, // mörk orange (Dark Orange)
            new BankSource { BankSourceId = 3, Name = "SEB", Color = "#0066CC", AccountType = "checking", Currency = "SEK", InitialBalance = 0, CreatedAt = DateTime.UtcNow }, // blå
            new BankSource { BankSourceId = 4, Name = "Nordea", Color = "#00A9CE", AccountType = "checking", Currency = "SEK", InitialBalance = 0, CreatedAt = DateTime.UtcNow }, // ljusblå
            new BankSource { BankSourceId = 5, Name = "Handelsbanken", Color = "#003366", AccountType = "checking", Currency = "SEK", InitialBalance = 0, CreatedAt = DateTime.UtcNow }, // mörk blå
            new BankSource { BankSourceId = 6, Name = "Avanza", Color = "#006400", AccountType = "investment", Currency = "SEK", InitialBalance = 0, CreatedAt = DateTime.UtcNow } // mörkgrön (Dark Green)
        );

        // Household configuration
        modelBuilder.Entity<Household>(entity =>
        {
            entity.HasKey(e => e.HouseholdId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedDate).IsRequired();
        });

        modelBuilder.Entity<HouseholdMember>(entity =>
        {
            entity.HasKey(e => e.HouseholdMemberId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.JoinedDate).IsRequired();
            
            entity.HasOne(e => e.Household)
                .WithMany(h => h.Members)
                .HasForeignKey(e => e.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SharedExpense>(entity =>
        {
            entity.HasKey(e => e.SharedExpenseId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.ExpenseDate).IsRequired();
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.SplitMethod).IsRequired();
            
            entity.HasOne(e => e.Household)
                .WithMany(h => h.SharedExpenses)
                .HasForeignKey(e => e.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExpenseShare>(entity =>
        {
            entity.HasKey(e => e.ExpenseShareId);
            entity.Property(e => e.ShareAmount).HasPrecision(18, 2);
            entity.Property(e => e.SharePercentage).HasPrecision(5, 2);
            entity.Property(e => e.RoomSize).HasPrecision(10, 2);
            
            entity.HasOne(e => e.SharedExpense)
                .WithMany(se => se.ExpenseShares)
                .HasForeignKey(e => e.SharedExpenseId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.HouseholdMember)
                .WithMany(hm => hm.ExpenseShares)
                .HasForeignKey(e => e.HouseholdMemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasKey(e => e.GoalId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TargetAmount).HasPrecision(18, 2);
            entity.Property(e => e.CurrentAmount).HasPrecision(18, 2);
            entity.Property(e => e.TargetDate).IsRequired();
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Color).IsRequired().HasMaxLength(7);
        });
    }
}
