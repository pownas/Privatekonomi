using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Data;

public class PrivatekonomyContext : IdentityDbContext<ApplicationUser>
{
    public PrivatekonomyContext(DbContextOptions<PrivatekonomyContext> options)
        : base(options)
    {
    }

    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<TransactionCategory> TransactionCategories { get; set; }
    public DbSet<CategoryRule> CategoryRules { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<Investment> Investments { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<BudgetCategory> BudgetCategories { get; set; }
    public DbSet<BankSource> BankSources { get; set; }
    public DbSet<BankConnection> BankConnections { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Household> Households { get; set; }
    public DbSet<HouseholdMember> HouseholdMembers { get; set; }
    public DbSet<SharedExpense> SharedExpenses { get; set; }
    public DbSet<ExpenseShare> ExpenseShares { get; set; }
    public DbSet<ChildAllowance> ChildAllowances { get; set; }
    public DbSet<AllowanceTransaction> AllowanceTransactions { get; set; }
    public DbSet<AllowanceTask> AllowanceTasks { get; set; }
    public DbSet<Goal> Goals { get; set; }
    
    // Pockets for savings accounts
    public DbSet<Pocket> Pockets { get; set; }
    public DbSet<PocketTransaction> PocketTransactions { get; set; }
    
    // Shared Goals
    public DbSet<SharedGoal> SharedGoals { get; set; }
    public DbSet<SharedGoalParticipant> SharedGoalParticipants { get; set; }
    public DbSet<SharedGoalProposal> SharedGoalProposals { get; set; }
    public DbSet<SharedGoalProposalVote> SharedGoalProposalVotes { get; set; }
    public DbSet<SharedGoalTransaction> SharedGoalTransactions { get; set; }
    public DbSet<SharedGoalNotification> SharedGoalNotifications { get; set; }
    
    // Swedish-specific models
    public DbSet<TaxDeduction> TaxDeductions { get; set; }
    public DbSet<CapitalGain> CapitalGains { get; set; }
    public DbSet<CommuteDeduction> CommuteDeductions { get; set; }
    public DbSet<CreditRating> CreditRatings { get; set; }

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
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.UserId);
            
            // Ignore computed property
            entity.Ignore(e => e.CurrentBalance);
        });

        modelBuilder.Entity<BankConnection>(entity =>
        {
            entity.HasKey(e => e.BankConnectionId);
            entity.Property(e => e.ApiType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ExternalAccountId).HasMaxLength(100);
            entity.Property(e => e.AccessToken).HasMaxLength(2000);
            entity.Property(e => e.RefreshToken).HasMaxLength(2000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.BankSource)
                .WithMany()
                .HasForeignKey(e => e.BankSourceId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => e.BankSourceId);
            entity.HasIndex(e => e.ExternalAccountId);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserId).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.CreatedAt);
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

        modelBuilder.Entity<CategoryRule>(entity =>
        {
            entity.HasKey(e => e.CategoryRuleId);
            entity.Property(e => e.Pattern).IsRequired().HasMaxLength(500);
            entity.Property(e => e.MatchType).IsRequired();
            entity.Property(e => e.Priority).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CaseSensitive).IsRequired();
            entity.Property(e => e.Field).IsRequired();
            entity.Property(e => e.RuleType).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // User relationship (nullable for system rules)
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Self-referencing relationship for rule overrides
            entity.HasOne(e => e.OverridesSystemRule)
                .WithMany()
                .HasForeignKey(e => e.OverridesSystemRuleId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.IsActive, e.Priority });
            entity.HasIndex(e => e.RuleType);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.RuleType, e.UserId });
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
            
            entity.HasOne(e => e.Pocket)
                .WithMany()
                .HasForeignKey(e => e.PocketId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes for performance optimization
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PocketId);
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
            entity.Property(e => e.CreditLimit).HasPrecision(18, 2);
            entity.Property(e => e.MinimumPayment).HasPrecision(18, 2);
            entity.Property(e => e.InstallmentFee).HasPrecision(18, 2);
            entity.Property(e => e.ExtraMonthlyPayment).HasPrecision(18, 2);
            entity.Property(e => e.Priority).IsRequired();
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.UserId);
            
            // Ignore computed properties
            entity.Ignore(e => e.CurrentBalance);
            entity.Ignore(e => e.UtilizationRate);
        });
        
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.AssetId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.PurchaseValue).HasPrecision(18, 2);
            entity.Property(e => e.CurrentValue).HasPrecision(18, 2);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.UserId);
            
            // Ignore computed properties
            entity.Ignore(e => e.ValueChange);
            entity.Ignore(e => e.ValueChangePercentage);
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
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Indexes for faster searching
            entity.HasIndex(e => e.ISIN);
            entity.HasIndex(e => e.AccountNumber);
            entity.HasIndex(e => e.UserId);
            
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
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.UserId);
            
            entity.HasOne(e => e.Household)
                .WithMany()
                .HasForeignKey(e => e.HouseholdId)
                .OnDelete(DeleteBehavior.SetNull);
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
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.UserId);
        });

        // Pocket configuration
        modelBuilder.Entity<Pocket>(entity =>
        {
            entity.HasKey(e => e.PocketId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TargetAmount).HasPrecision(18, 2);
            entity.Property(e => e.CurrentAmount).HasPrecision(18, 2);
            entity.Property(e => e.MonthlyAllocation).HasPrecision(18, 2);
            entity.Property(e => e.Priority).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.BankSource)
                .WithMany()
                .HasForeignKey(e => e.BankSourceId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.BankSourceId);
            
            // Ignore computed properties
            entity.Ignore(e => e.ProgressPercentage);
            entity.Ignore(e => e.RemainingAmount);
        });

        // PocketTransaction configuration
        modelBuilder.Entity<PocketTransaction>(entity =>
        {
            entity.HasKey(e => e.PocketTransactionId);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TransactionDate).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.Pocket)
                .WithMany(p => p.PocketTransactions)
                .HasForeignKey(e => e.PocketId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.RelatedPocket)
                .WithMany()
                .HasForeignKey(e => e.RelatedPocketId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.PocketId);
            entity.HasIndex(e => e.TransactionDate);
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

        // Swedish-specific entities configuration
        modelBuilder.Entity<TaxDeduction>(entity =>
        {
            entity.HasKey(e => e.TaxDeductionId);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.DeductibleAmount).HasPrecision(18, 2);
            entity.Property(e => e.ServiceProvider).IsRequired().HasMaxLength(200);
            entity.Property(e => e.OrganizationNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.WorkDescription).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.Transaction)
                .WithMany()
                .HasForeignKey(e => e.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => e.TaxYear);
            entity.HasIndex(e => e.Type);
        });
        
        modelBuilder.Entity<CapitalGain>(entity =>
        {
            entity.HasKey(e => e.CapitalGainId);
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.PurchasePricePerUnit).HasPrecision(18, 2);
            entity.Property(e => e.TotalPurchasePrice).HasPrecision(18, 2);
            entity.Property(e => e.SalePricePerUnit).HasPrecision(18, 2);
            entity.Property(e => e.TotalSalePrice).HasPrecision(18, 2);
            entity.Property(e => e.Gain).HasPrecision(18, 2);
            entity.Property(e => e.SecurityType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SecurityName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ISIN).HasMaxLength(12);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.ExchangeRate).HasPrecision(18, 6);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.Investment)
                .WithMany()
                .HasForeignKey(e => e.InvestmentId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.TaxYear);
            entity.HasIndex(e => e.IsISK);
            entity.HasIndex(e => e.SaleDate);
        });
        
        modelBuilder.Entity<CommuteDeduction>(entity =>
        {
            entity.HasKey(e => e.CommuteDeductionId);
            entity.Property(e => e.FromAddress).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ToAddress).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DistanceKm).HasPrecision(10, 2);
            entity.Property(e => e.TransportMethod).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Cost).HasPrecision(18, 2);
            entity.Property(e => e.DeductibleAmount).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasIndex(e => e.TaxYear);
            entity.HasIndex(e => e.Date);
            
            entity.Ignore(e => e.TotalDistanceKm);
        });
        
        modelBuilder.Entity<CreditRating>(entity =>
        {
            entity.HasKey(e => e.CreditRatingId);
            entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Rating).IsRequired().HasMaxLength(10);
            entity.Property(e => e.TotalDebt).HasPrecision(18, 2);
            entity.Property(e => e.CreditLimit).HasPrecision(18, 2);
            entity.Property(e => e.CreditUtilization).HasPrecision(5, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.Household)
                .WithMany()
                .HasForeignKey(e => e.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => e.CheckedDate);
        });
        
        // Swedish-specific Investment properties
        modelBuilder.Entity<Investment>(entity =>
        {
            entity.Property(e => e.AccountType).HasMaxLength(20);
            entity.Property(e => e.SchablonTax).HasPrecision(18, 2);
            
            entity.HasIndex(e => e.AccountType);
            entity.HasIndex(e => e.SchablonTaxYear);
        });
        
        // Swedish-specific Transaction properties
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.RecipientBankgiro).HasMaxLength(20);
            entity.Property(e => e.RecipientPlusgiro).HasMaxLength(20);
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
            entity.Property(e => e.OCR).HasMaxLength(50);
            
            entity.HasIndex(e => e.PaymentMethod);
            entity.HasIndex(e => e.IsRecurring);
        });
        
        // Swedish-specific Loan properties
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.Property(e => e.PropertyAddress).HasMaxLength(200);
            entity.Property(e => e.PropertyValue).HasPrecision(18, 2);
            entity.Property(e => e.LoanProvider).HasMaxLength(100);
            entity.Property(e => e.CSN_LoanType).HasMaxLength(50);
            entity.Property(e => e.CSN_MonthlyPayment).HasPrecision(18, 2);
            entity.Property(e => e.CSN_RemainingAmount).HasPrecision(18, 2);
            
            entity.HasIndex(e => e.Type);
            entity.Ignore(e => e.LTV);
        });

        // Household configuration
        modelBuilder.Entity<Household>(entity =>
        {
            entity.HasKey(e => e.HouseholdId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedDate).IsRequired();
        });

        // ApplicationUser configuration
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.HouseholdMember)
                .WithOne(hm => hm.User)
                .HasForeignKey<ApplicationUser>(e => e.HouseholdMemberId)
                .OnDelete(DeleteBehavior.SetNull);
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

        // Child Allowance configuration
        modelBuilder.Entity<ChildAllowance>(entity =>
        {
            entity.HasKey(e => e.ChildAllowanceId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.CurrentBalance).HasPrecision(18, 2);
            entity.Property(e => e.Frequency).IsRequired();
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.HouseholdMember)
                .WithMany()
                .HasForeignKey(e => e.HouseholdMemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AllowanceTransaction>(entity =>
        {
            entity.HasKey(e => e.AllowanceTransactionId);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.TransactionDate).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.ChildAllowance)
                .WithMany(ca => ca.AllowanceTransactions)
                .HasForeignKey(e => e.ChildAllowanceId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.AllowanceTask)
                .WithMany(at => at.AllowanceTransactions)
                .HasForeignKey(e => e.AllowanceTaskId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AllowanceTask>(entity =>
        {
            entity.HasKey(e => e.AllowanceTaskId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.RewardAmount).HasPrecision(18, 2);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.DueDate).IsRequired();
            entity.Property(e => e.ApprovedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.ChildAllowance)
                .WithMany(ca => ca.AllowanceTasks)
                .HasForeignKey(e => e.ChildAllowanceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Shared Goals configuration
        modelBuilder.Entity<SharedGoal>(entity =>
        {
            entity.HasKey(e => e.SharedGoalId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TargetAmount).HasPrecision(18, 2);
            entity.Property(e => e.CurrentAmount).HasPrecision(18, 2);
            entity.Property(e => e.Priority).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.CreatedByUserId);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<SharedGoalParticipant>(entity =>
        {
            entity.HasKey(e => e.SharedGoalParticipantId);
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.InvitationStatus).IsRequired();
            entity.Property(e => e.JoinedAt).IsRequired();
            
            entity.HasOne(e => e.SharedGoal)
                .WithMany(sg => sg.Participants)
                .HasForeignKey(e => e.SharedGoalId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.InvitedByUser)
                .WithMany()
                .HasForeignKey(e => e.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.SharedGoalId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.InvitationStatus);
            entity.HasIndex(e => new { e.SharedGoalId, e.UserId }).IsUnique();
        });

        modelBuilder.Entity<SharedGoalProposal>(entity =>
        {
            entity.HasKey(e => e.SharedGoalProposalId);
            entity.Property(e => e.ProposalType).IsRequired();
            entity.Property(e => e.CurrentValue).HasMaxLength(500);
            entity.Property(e => e.ProposedValue).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.SharedGoal)
                .WithMany(sg => sg.Proposals)
                .HasForeignKey(e => e.SharedGoalId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ProposedByUser)
                .WithMany()
                .HasForeignKey(e => e.ProposedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.SharedGoalId);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<SharedGoalProposalVote>(entity =>
        {
            entity.HasKey(e => e.SharedGoalProposalVoteId);
            entity.Property(e => e.Vote).IsRequired();
            entity.Property(e => e.VotedAt).IsRequired();
            entity.Property(e => e.Comment).HasMaxLength(500);
            
            entity.HasOne(e => e.SharedGoalProposal)
                .WithMany(p => p.Votes)
                .HasForeignKey(e => e.SharedGoalProposalId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.SharedGoalProposalId);
            entity.HasIndex(e => new { e.SharedGoalProposalId, e.UserId }).IsUnique();
        });

        modelBuilder.Entity<SharedGoalTransaction>(entity =>
        {
            entity.HasKey(e => e.SharedGoalTransactionId);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TransactionDate).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.SharedGoal)
                .WithMany(sg => sg.Transactions)
                .HasForeignKey(e => e.SharedGoalId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.SharedGoalId);
            entity.HasIndex(e => e.TransactionDate);
        });

        modelBuilder.Entity<SharedGoalNotification>(entity =>
        {
            entity.HasKey(e => e.SharedGoalNotificationId);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IsRead).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.SharedGoal)
                .WithMany(sg => sg.Notifications)
                .HasForeignKey(e => e.SharedGoalId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => e.SharedGoalId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => new { e.UserId, e.IsRead });
        });
    }
}
