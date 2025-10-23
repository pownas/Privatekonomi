using Microsoft.AspNetCore.Identity;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Data;

public static class TestDataSeeder
{
    public static async Task SeedTestDataAsync(PrivatekonomyContext context, UserManager<ApplicationUser> userManager)
    {
        // Check if there are already transactions to avoid duplicate seeding
        if (context.Transactions.Any())
        {
            return;
        }

        // Create test users first
        var testUserId = await SeedUsers(userManager);
        
        // Seed data with user association
        SeedTransactions(context, testUserId);
        SeedInvestments(context, testUserId);
        SeedAssets(context, testUserId);
        SeedLoans(context, testUserId);
        SeedBudgets(context, testUserId);
        SeedCategoryRules(context);
        SeedHouseholds(context);
        SeedGoals(context, testUserId);
        SeedSalaryHistory(context, testUserId);
        SeedNetWorthSnapshots(context, testUserId);
        SeedPockets(context, testUserId);
    }
    
    private static async Task<string> SeedUsers(UserManager<ApplicationUser> userManager)
    {
        // Create a test user
        var testUser = new ApplicationUser
        {
            UserName = "test@example.com",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "Användare",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var existingUser = await userManager.FindByEmailAsync(testUser.Email);
        if (existingUser == null)
        {
            await userManager.CreateAsync(testUser, "Test123!");
            existingUser = testUser;
        }

        return existingUser.Id;
    }

    private static void SeedCategoryRules(PrivatekonomyContext context)
    {
        // Only seed if no rules exist
        if (context.CategoryRules.Any())
        {
            return;
        }

        var categoryRules = new List<CategoryRule>
        {
            // Mat & Dryck (Category 1) - High priority
            new CategoryRule { CategoryRuleId = 1, Pattern = "ICA", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 100, IsActive = true, Description = "ICA stores", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 2, Pattern = "Coop", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 100, IsActive = true, Description = "Coop stores", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 3, Pattern = "Willys", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 100, IsActive = true, Description = "Willys stores", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 4, Pattern = "Hemköp", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 100, IsActive = true, Description = "Hemköp stores", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 5, Pattern = "Restaurant", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 90, IsActive = true, Description = "Restaurants", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 6, Pattern = "Restaurang", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 90, IsActive = true, Description = "Swedish restaurants", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 7, Pattern = "Cafe", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 90, IsActive = true, Description = "Cafes", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 8, Pattern = "McDonalds", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 95, IsActive = true, Description = "McDonald's", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 9, Pattern = "Pizza", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 90, IsActive = true, Description = "Pizza places", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 10, Pattern = "Sushi", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 90, IsActive = true, Description = "Sushi restaurants", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },

            // Transport (Category 2)
            new CategoryRule { CategoryRuleId = 11, Pattern = "SL-kort", MatchType = PatternMatchType.Contains, CategoryId = 2, Priority = 100, IsActive = true, Description = "Stockholm public transport", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 12, Pattern = "Bensin", MatchType = PatternMatchType.Contains, CategoryId = 2, Priority = 95, IsActive = true, Description = "Gas stations", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 13, Pattern = "Circle K", MatchType = PatternMatchType.Contains, CategoryId = 2, Priority = 100, IsActive = true, Description = "Circle K gas", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 14, Pattern = "Preem", MatchType = PatternMatchType.Contains, CategoryId = 2, Priority = 100, IsActive = true, Description = "Preem gas", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 15, Pattern = "Parkering", MatchType = PatternMatchType.Contains, CategoryId = 2, Priority = 95, IsActive = true, Description = "Parking", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 16, Pattern = "Taxi", MatchType = PatternMatchType.Contains, CategoryId = 2, Priority = 95, IsActive = true, Description = "Taxi services", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 17, Pattern = "SJ", MatchType = PatternMatchType.Contains, CategoryId = 2, Priority = 100, IsActive = true, Description = "Swedish railways", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },

            // Boende (Category 3)
            new CategoryRule { CategoryRuleId = 18, Pattern = "Hyra", MatchType = PatternMatchType.Contains, CategoryId = 3, Priority = 100, IsActive = true, Description = "Rent", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 19, Pattern = "Vattenfall", MatchType = PatternMatchType.Contains, CategoryId = 3, Priority = 100, IsActive = true, Description = "Vattenfall electricity", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 20, Pattern = "Telia", MatchType = PatternMatchType.Contains, CategoryId = 3, Priority = 100, IsActive = true, Description = "Telia broadband", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 21, Pattern = "Bredband", MatchType = PatternMatchType.Contains, CategoryId = 3, Priority = 95, IsActive = true, Description = "Broadband", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 22, Pattern = "Försäkring", MatchType = PatternMatchType.Contains, CategoryId = 3, Priority = 95, IsActive = true, Description = "Insurance", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },

            // Nöje (Category 4)
            new CategoryRule { CategoryRuleId = 23, Pattern = "Spotify", MatchType = PatternMatchType.Contains, CategoryId = 4, Priority = 100, IsActive = true, Description = "Spotify subscription", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 24, Pattern = "Netflix", MatchType = PatternMatchType.Contains, CategoryId = 4, Priority = 100, IsActive = true, Description = "Netflix subscription", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 25, Pattern = "Bio", MatchType = PatternMatchType.Contains, CategoryId = 4, Priority = 90, IsActive = true, Description = "Cinema", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 26, Pattern = "SF", MatchType = PatternMatchType.StartsWith, CategoryId = 4, Priority = 95, IsActive = true, Description = "SF Bio cinema", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 27, Pattern = "Gym", MatchType = PatternMatchType.Contains, CategoryId = 4, Priority = 90, IsActive = true, Description = "Gym memberships", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 28, Pattern = "Teater", MatchType = PatternMatchType.Contains, CategoryId = 4, Priority = 90, IsActive = true, Description = "Theatre", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 29, Pattern = "Konsert", MatchType = PatternMatchType.Contains, CategoryId = 4, Priority = 90, IsActive = true, Description = "Concerts", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },

            // Shopping (Category 5)
            new CategoryRule { CategoryRuleId = 30, Pattern = "H&M", MatchType = PatternMatchType.Contains, CategoryId = 5, Priority = 100, IsActive = true, Description = "H&M stores", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 31, Pattern = "IKEA", MatchType = PatternMatchType.Contains, CategoryId = 5, Priority = 100, IsActive = true, Description = "IKEA", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 32, Pattern = "Elgiganten", MatchType = PatternMatchType.Contains, CategoryId = 5, Priority = 100, IsActive = true, Description = "Elgiganten electronics", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 33, Pattern = "Clas Ohlson", MatchType = PatternMatchType.Contains, CategoryId = 5, Priority = 100, IsActive = true, Description = "Clas Ohlson", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 34, Pattern = "Stadium", MatchType = PatternMatchType.Contains, CategoryId = 5, Priority = 100, IsActive = true, Description = "Stadium sports", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 35, Pattern = "Apoteket", MatchType = PatternMatchType.Contains, CategoryId = 5, Priority = 95, IsActive = true, Description = "Pharmacy", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },

            // Hälsa (Category 6)
            new CategoryRule { CategoryRuleId = 36, Pattern = "Folktandvården", MatchType = PatternMatchType.Contains, CategoryId = 6, Priority = 100, IsActive = true, Description = "Public dental care", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 37, Pattern = "Apotek", MatchType = PatternMatchType.Contains, CategoryId = 6, Priority = 95, IsActive = true, Description = "Pharmacy (health)", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 38, Pattern = "Naprapat", MatchType = PatternMatchType.Contains, CategoryId = 6, Priority = 100, IsActive = true, Description = "Naprapathy", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 39, Pattern = "Vitaminer", MatchType = PatternMatchType.Contains, CategoryId = 6, Priority = 90, IsActive = true, Description = "Vitamins", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },

            // Lön (Category 7)
            new CategoryRule { CategoryRuleId = 40, Pattern = "Lön", MatchType = PatternMatchType.Contains, CategoryId = 7, Priority = 100, IsActive = true, Description = "Salary", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 41, Pattern = "Bonus", MatchType = PatternMatchType.Contains, CategoryId = 7, Priority = 95, IsActive = true, Description = "Bonus", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 42, Pattern = "Semesterersättning", MatchType = PatternMatchType.Contains, CategoryId = 7, Priority = 95, IsActive = true, Description = "Vacation pay", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },

            // Sparande (Category 8)
            new CategoryRule { CategoryRuleId = 43, Pattern = "Sparande", MatchType = PatternMatchType.Contains, CategoryId = 8, Priority = 100, IsActive = true, Description = "Savings", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
            new CategoryRule { CategoryRuleId = 44, Pattern = "ISK", MatchType = PatternMatchType.Contains, CategoryId = 8, Priority = 95, IsActive = true, Description = "Investment savings account", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = DateTime.UtcNow },
        };

        context.CategoryRules.AddRange(categoryRules);
        context.SaveChanges();
    }

    private static void SeedTransactions(PrivatekonomyContext context, string userId)
    {

        var random = new Random(42); // Fixed seed for reproducible data
        var startDate = DateTime.Now.AddMonths(-3); // Start from 3 months ago

        var transactions = new List<Transaction>();
        var transactionCategories = new List<TransactionCategory>();

        // Transaction descriptions for different categories
        var descriptions = new Dictionary<int, string[]>
        {
            { 1, new[] { "ICA Maxi", "Coop", "Willys", "Hemköp", "Restaurant", "Restaurang Bella", "Cafe latte", "McDonalds", "Pizzeria", "Sushi bar" } }, // Mat & Dryck
            { 2, new[] { "SL-kort", "Bensin Circle K", "Parkering", "Bensin Preem", "Taxi Stockholm", "Biltvätt", "SJ tåg", "Flygbuss" } }, // Transport
            { 3, new[] { "Hyra lägenhet", "El Vattenfall", "Bredband Telia", "Försäkring Folksam", "Reparation" } }, // Boende
            { 4, new[] { "Bio SF", "Spotify", "Netflix", "Gym medlemskap", "Teater biljett", "Konsert", "Bowlinghallen", "Eventbiljett" } }, // Nöje
            { 5, new[] { "H&M", "Elgiganten", "IKEA", "Clas Ohlson", "Stadium", "Apoteket", "Bokus.com", "Webhallen" } }, // Shopping
            { 6, new[] { "Folktandvården", "Apotek", "Naprapat", "Gym kort", "Vitaminer", "Glasögon Synoptik" } }, // Hälsa
            { 7, new[] { "Lön arbetsgivare", "Bonus", "Semesterersättning" } }, // Lön
            { 8, new[] { "Sparande sparkonto", "Månadsspar aktier", "ISK insättning" } }, // Sparande
            { 9, new[] { "Swish betalning", "Gåva", "Okategoriserad", "Diverse", "Kontantuttag" } } // Övrigt
        };

        // Bank source IDs to simulate transactions from different banks (1-6 as per seeded data)
        var bankSourceIds = new[] { 1, 2, 3, 4, 5, 6 };

        // Amount ranges for different categories (min, max)
        var amountRanges = new Dictionary<int, (decimal min, decimal max)>
        {
            { 1, (50m, 800m) },    // Mat & Dryck
            { 2, (80m, 500m) },    // Transport
            { 3, (200m, 15000m) }, // Boende
            { 4, (100m, 600m) },   // Nöje
            { 5, (150m, 3000m) },  // Shopping
            { 6, (200m, 1500m) },  // Hälsa
            { 7, (25000m, 45000m) }, // Lön
            { 8, (500m, 5000m) },  // Sparande
            { 9, (100m, 1000m) }   // Övrigt
        };

        int transactionId = 1;
        int transactionCategoryId = 1;

        // Generate 50 transactions
        for (int i = 0; i < 50; i++)
        {
            // Randomly select a category (weighted towards common categories)
            var categoryWeights = new[] { 1, 1, 1, 2, 2, 3, 3, 4, 5, 6, 7, 8, 9, 1, 2 };
            var categoryId = categoryWeights[random.Next(categoryWeights.Length)];
            
            // Select a random description for this category
            var descArray = descriptions[categoryId];
            var description = descArray[random.Next(descArray.Length)];
            
            // Generate amount within the range for this category
            var range = amountRanges[categoryId];
            var amount = Math.Round(range.min + (decimal)random.NextDouble() * (range.max - range.min), 2);
            
            // Generate random date within the last 3 months
            var daysBack = random.Next(0, 90);
            var date = startDate.AddDays(daysBack);
            
            // Determine if it's income (category 7 = Lön is always income)
            var isIncome = categoryId == 7;

            var transaction = new Transaction
            {
                TransactionId = transactionId,
                Amount = amount,
                Description = description,
                Date = date,
                IsIncome = isIncome,
                BankSourceId = bankSourceIds[random.Next(bankSourceIds.Length)],
                Currency = "SEK",
                Imported = false,
                Cleared = true,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            transactions.Add(transaction);

            // Create transaction category relationship
            var transactionCategory = new TransactionCategory
            {
                TransactionCategoryId = transactionCategoryId,
                TransactionId = transactionId,
                CategoryId = categoryId,
                Amount = amount
            };

            transactionCategories.Add(transactionCategory);

            transactionId++;
            transactionCategoryId++;
        }

        // Add 5 unmapped transactions (without categories) to demonstrate the feature
        var unmappedDescriptions = new[] { "Okänd transaktion", "Kontant betalning", "Swish från okänd", "Okategoriserad köp", "Diverse utgift" };
        for (int i = 0; i < 5; i++)
        {
            var transaction = new Transaction
            {
                TransactionId = transactionId,
                Amount = Math.Round(100m + (decimal)random.NextDouble() * 500m, 2),
                Description = unmappedDescriptions[i],
                Date = startDate.AddDays(random.Next(0, 90)),
                IsIncome = false,
                BankSourceId = bankSourceIds[random.Next(bankSourceIds.Length)],
                Currency = "SEK",
                Imported = false,
                Cleared = false,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };
            
            transactions.Add(transaction);
            transactionId++;
        }

        // Add all transactions and categories to context
        context.Transactions.AddRange(transactions);
        context.TransactionCategories.AddRange(transactionCategories);
        context.SaveChanges();
    }

    private static void SeedInvestments(PrivatekonomyContext context, string userId)
    {
        var investments = new List<Investment>
        {
            new Investment
            {
                InvestmentId = 1,
                Name = "Volvo B",
                ShortName = "VOLV-B",
                Type = "Aktie",
                Quantity = 100,
                PurchasePrice = 245.50m,
                CurrentPrice = 268.75m,
                PurchaseDate = DateTime.Now.AddMonths(-6),
                LastUpdated = DateTime.Now.AddDays(-1),
                Market = "Stockholm",
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Investment
            {
                InvestmentId = 2,
                Name = "SEB A",
                ShortName = "SEB-A",
                Type = "Aktie",
                Quantity = 50,
                PurchasePrice = 152.30m,
                CurrentPrice = 165.20m,
                PurchaseDate = DateTime.Now.AddMonths(-8),
                LastUpdated = DateTime.Now.AddDays(-2),
                Market = "Stockholm",
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Investment
            {
                InvestmentId = 3,
                Name = "Investor B",
                ShortName = "INVE-B",
                Type = "Aktie",
                Quantity = 75,
                PurchasePrice = 289.00m,
                CurrentPrice = 312.50m,
                PurchaseDate = DateTime.Now.AddMonths(-4),
                LastUpdated = DateTime.Now.AddDays(-1),
                Market = "Stockholm",
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Investment
            {
                InvestmentId = 4,
                Name = "SPP Aktiefond Global",
                Type = "Fond",
                Quantity = 150,
                PurchasePrice = 125.75m,
                CurrentPrice = 138.40m,
                PurchaseDate = DateTime.Now.AddMonths(-12),
                LastUpdated = DateTime.Now.AddDays(-3),
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Investment
            {
                InvestmentId = 5,
                Name = "Länsförsäkringar Sverige",
                Type = "Fond",
                Quantity = 200,
                PurchasePrice = 98.50m,
                CurrentPrice = 104.80m,
                PurchaseDate = DateTime.Now.AddMonths(-10),
                LastUpdated = DateTime.Now.AddDays(-1),
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Investment
            {
                InvestmentId = 6,
                Name = "Avanza Global",
                Type = "Fond",
                Quantity = 120,
                PurchasePrice = 215.30m,
                CurrentPrice = 232.90m,
                PurchaseDate = DateTime.Now.AddMonths(-9),
                LastUpdated = DateTime.Now.AddDays(-2),
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Investment
            {
                InvestmentId = 7,
                Name = "Ericsson B",
                ShortName = "ERIC-B",
                Type = "Aktie",
                Quantity = 250,
                PurchasePrice = 58.20m,
                CurrentPrice = 62.15m,
                PurchaseDate = DateTime.Now.AddMonths(-5),
                LastUpdated = DateTime.Now,
                Market = "Stockholm",
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Investment
            {
                InvestmentId = 8,
                Name = "Nordea",
                ShortName = "NDA-SE",
                Type = "Aktie",
                Quantity = 80,
                PurchasePrice = 125.40m,
                CurrentPrice = 118.90m,
                PurchaseDate = DateTime.Now.AddMonths(-3),
                LastUpdated = DateTime.Now.AddDays(-1),
                Market = "Stockholm",
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            }
        };

        context.Investments.AddRange(investments);
        context.SaveChanges();
    }

    private static void SeedBudgets(PrivatekonomyContext context, string userId)
    {
        // Create a budget for the current month
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var currentMonthBudget = new Budget
        {
            BudgetId = 1,
            Name = "Månadsbudget " + now.ToString("MMMM yyyy", new System.Globalization.CultureInfo("sv-SE")),
            Description = "Budget för den aktuella månaden",
            StartDate = startOfMonth,
            EndDate = endOfMonth,
            Period = BudgetPeriod.Monthly,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        // Add budget categories with planned amounts
        var budgetCategories = new List<BudgetCategory>
        {
            new BudgetCategory { BudgetCategoryId = 1, BudgetId = 1, CategoryId = 1, PlannedAmount = 5000m },  // Mat & Dryck
            new BudgetCategory { BudgetCategoryId = 2, BudgetId = 1, CategoryId = 2, PlannedAmount = 1500m },  // Transport
            new BudgetCategory { BudgetCategoryId = 3, BudgetId = 1, CategoryId = 3, PlannedAmount = 12000m }, // Boende
            new BudgetCategory { BudgetCategoryId = 4, BudgetId = 1, CategoryId = 4, PlannedAmount = 2000m },  // Nöje
            new BudgetCategory { BudgetCategoryId = 5, BudgetId = 1, CategoryId = 5, PlannedAmount = 3000m },  // Shopping
            new BudgetCategory { BudgetCategoryId = 6, BudgetId = 1, CategoryId = 6, PlannedAmount = 1000m },  // Hälsa
        };

        // Create a budget for the previous month
        var prevMonthStart = startOfMonth.AddMonths(-1);
        var prevMonthEnd = startOfMonth.AddDays(-1);

        var previousMonthBudget = new Budget
        {
            BudgetId = 2,
            Name = "Månadsbudget " + prevMonthStart.ToString("MMMM yyyy", new System.Globalization.CultureInfo("sv-SE")),
            Description = "Budget för föregående månad",
            StartDate = prevMonthStart,
            EndDate = prevMonthEnd,
            Period = BudgetPeriod.Monthly,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        var prevBudgetCategories = new List<BudgetCategory>
        {
            new BudgetCategory { BudgetCategoryId = 7, BudgetId = 2, CategoryId = 1, PlannedAmount = 4800m },  // Mat & Dryck
            new BudgetCategory { BudgetCategoryId = 8, BudgetId = 2, CategoryId = 2, PlannedAmount = 1400m },  // Transport
            new BudgetCategory { BudgetCategoryId = 9, BudgetId = 2, CategoryId = 3, PlannedAmount = 12000m }, // Boende
            new BudgetCategory { BudgetCategoryId = 10, BudgetId = 2, CategoryId = 4, PlannedAmount = 1800m }, // Nöje
            new BudgetCategory { BudgetCategoryId = 11, BudgetId = 2, CategoryId = 5, PlannedAmount = 2500m }, // Shopping
            new BudgetCategory { BudgetCategoryId = 12, BudgetId = 2, CategoryId = 6, PlannedAmount = 900m },  // Hälsa
        };

        context.Budgets.AddRange(new[] { currentMonthBudget, previousMonthBudget });
        context.BudgetCategories.AddRange(budgetCategories);
        context.BudgetCategories.AddRange(prevBudgetCategories);
        context.SaveChanges();
    }

    private static void SeedHouseholds(PrivatekonomyContext context)
    {
        // Create a sample household with members and shared expenses
        var household = new Household
        {
            HouseholdId = 1,
            Name = "Familjen Andersson",
            Description = "Hushåll med delad lägenhet i Stockholm",
            CreatedDate = DateTime.Now.AddMonths(-6)
        };

        var members = new List<HouseholdMember>
        {
            new HouseholdMember
            {
                HouseholdMemberId = 1,
                HouseholdId = 1,
                Name = "Anna Andersson",
                Email = "anna@example.com",
                IsActive = true,
                JoinedDate = DateTime.Now.AddMonths(-6)
            },
            new HouseholdMember
            {
                HouseholdMemberId = 2,
                HouseholdId = 1,
                Name = "Erik Andersson",
                Email = "erik@example.com",
                IsActive = true,
                JoinedDate = DateTime.Now.AddMonths(-6)
            },
            new HouseholdMember
            {
                HouseholdMemberId = 3,
                HouseholdId = 1,
                Name = "Sara Johansson",
                Email = "sara@example.com",
                IsActive = true,
                JoinedDate = DateTime.Now.AddMonths(-3)
            }
        };

        // Create shared expenses with different distribution methods
        var sharedExpenses = new List<SharedExpense>
        {
            new SharedExpense
            {
                SharedExpenseId = 1,
                HouseholdId = 1,
                Name = "Hyra lägenhet",
                Description = "Månadshyra för 3-rummare",
                TotalAmount = 15000m,
                Type = ExpenseType.Rent,
                ExpenseDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                CreatedDate = DateTime.Now.AddDays(-5),
                SplitMethod = SplitMethod.Equal
            },
            new SharedExpense
            {
                SharedExpenseId = 2,
                HouseholdId = 1,
                Name = "El",
                Description = "Elräkning för månaden",
                TotalAmount = 1200m,
                Type = ExpenseType.Electricity,
                ExpenseDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15),
                CreatedDate = DateTime.Now.AddDays(-2),
                SplitMethod = SplitMethod.Equal
            },
            new SharedExpense
            {
                SharedExpenseId = 3,
                HouseholdId = 1,
                Name = "Bredband",
                Description = "Telia bredband 500 Mbit/s",
                TotalAmount = 399m,
                Type = ExpenseType.Internet,
                ExpenseDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                CreatedDate = DateTime.Now.AddDays(-10),
                SplitMethod = SplitMethod.Equal
            }
        };

        // Create expense shares (equal distribution)
        var expenseShares = new List<ExpenseShare>();
        foreach (var expense in sharedExpenses)
        {
            var shareAmount = expense.TotalAmount / members.Count;
            foreach (var member in members)
            {
                expenseShares.Add(new ExpenseShare
                {
                    SharedExpenseId = expense.SharedExpenseId,
                    HouseholdMemberId = member.HouseholdMemberId,
                    ShareAmount = shareAmount,
                    SharePercentage = 100m / members.Count
                });
            }
        }

        context.Households.Add(household);
        context.HouseholdMembers.AddRange(members);
        context.SharedExpenses.AddRange(sharedExpenses);
        context.ExpenseShares.AddRange(expenseShares);
        context.SaveChanges();
    }

    private static void SeedGoals(PrivatekonomyContext context, string userId)
    {
        // Create sample savings goals
        var goals = new List<Goal>
        {
            new Goal
            {
                GoalId = 1,
                Name = "Semesterresa till Japan",
                Description = "Spara till en två veckors resa till Japan",
                TargetAmount = 50000m,
                CurrentAmount = 15000m,
                TargetDate = DateTime.Now.AddMonths(12),
                Priority = 1,
                FundedFromBankSourceId = 1,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Goal
            {
                GoalId = 2,
                Name = "Ny laptop",
                Description = "MacBook Pro för arbete och studier",
                TargetAmount = 25000m,
                CurrentAmount = 8500m,
                TargetDate = DateTime.Now.AddMonths(6),
                Priority = 2,
                FundedFromBankSourceId = 1,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Goal
            {
                GoalId = 3,
                Name = "Nödfond",
                Description = "Buffert för oförutsedda utgifter - 3 månadslöner",
                TargetAmount = 90000m,
                CurrentAmount = 45000m,
                TargetDate = DateTime.Now.AddMonths(18),
                Priority = 1,
                FundedFromBankSourceId = 2,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Goal
            {
                GoalId = 4,
                Name = "Kontantinsats lägenhet",
                Description = "Spara till 15% kontantinsats för lägenhet",
                TargetAmount = 300000m,
                CurrentAmount = 120000m,
                TargetDate = DateTime.Now.AddMonths(24),
                Priority = 1,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Goal
            {
                GoalId = 5,
                Name = "Ny cykel",
                Description = "Elcykel för pendling",
                TargetAmount = 15000m,
                CurrentAmount = 12000m,
                TargetDate = DateTime.Now.AddMonths(3),
                Priority = 3,
                FundedFromBankSourceId = 1,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            }
        };

        context.Goals.AddRange(goals);
        context.SaveChanges();
    }

    private static void SeedAssets(PrivatekonomyContext context, string userId)
    {
        var assets = new List<Asset>
        {
            new Asset
            {
                AssetId = 1,
                Name = "Bostadsrätt Södermalm",
                Type = "Fastighet",
                Description = "2:a på Södermalm, Stockholm",
                PurchaseValue = 2800000m,
                CurrentValue = 3200000m,
                PurchaseDate = DateTime.Now.AddYears(-5),
                Location = "Stockholm, Södermalm",
                Currency = "SEK",
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Asset
            {
                AssetId = 2,
                Name = "Volvo V60",
                Type = "Fordon",
                Description = "2020 års modell, diesel",
                PurchaseValue = 285000m,
                CurrentValue = 240000m,
                PurchaseDate = DateTime.Now.AddYears(-3),
                Location = "Stockholm",
                Currency = "SEK",
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Asset
            {
                AssetId = 3,
                Name = "MacBook Pro",
                Type = "Elektronik",
                Description = "MacBook Pro 16\" M3 Pro",
                PurchaseValue = 32000m,
                CurrentValue = 28000m,
                PurchaseDate = DateTime.Now.AddMonths(-6),
                Location = "Hemkontor",
                Currency = "SEK",
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Asset
            {
                AssetId = 4,
                Name = "IKEA Möbler",
                Type = "Möbler",
                Description = "Soffa, matbord, och bokhyllor",
                PurchaseValue = 45000m,
                CurrentValue = 25000m,
                PurchaseDate = DateTime.Now.AddYears(-2),
                Location = "Lägenheten",
                Currency = "SEK",
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Asset
            {
                AssetId = 5,
                Name = "Samsung TV",
                Type = "Elektronik",
                Description = "55\" 4K QLED TV",
                PurchaseValue = 15000m,
                CurrentValue = 10000m,
                PurchaseDate = DateTime.Now.AddYears(-1),
                Location = "Vardagsrum",
                Currency = "SEK",
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Asset
            {
                AssetId = 6,
                Name = "Rolex klocka",
                Type = "Övrigt",
                Description = "Vintage Submariner",
                PurchaseValue = 85000m,
                CurrentValue = 120000m,
                PurchaseDate = DateTime.Now.AddYears(-10),
                Location = "Bankfack",
                Currency = "SEK",
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            }
        };

        context.Assets.AddRange(assets);
        context.SaveChanges();
    }

    private static void SeedLoans(PrivatekonomyContext context, string userId)
    {
        var loans = new List<Loan>
        {
            new Loan
            {
                LoanId = 1,
                Name = "Bolån bostadsrätt",
                Type = "Bolån",
                Amount = 2100000m,
                InterestRate = 4.5m,
                Amortization = 5000m,
                Currency = "SEK",
                StartDate = DateTime.Now.AddYears(-5),
                MaturityDate = DateTime.Now.AddYears(25),
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Loan
            {
                LoanId = 2,
                Name = "Billån Volvo",
                Type = "Privatlån",
                Amount = 150000m,
                InterestRate = 5.9m,
                Amortization = 3500m,
                Currency = "SEK",
                StartDate = DateTime.Now.AddYears(-3),
                MaturityDate = DateTime.Now.AddYears(2),
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new Loan
            {
                LoanId = 3,
                Name = "CSN Studielån",
                Type = "CSN-lån",
                Amount = 180000m,
                InterestRate = 1.5m,
                Amortization = 1200m,
                Currency = "SEK",
                StartDate = DateTime.Now.AddYears(-8),
                MaturityDate = DateTime.Now.AddYears(17),
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            }
        };

        context.Loans.AddRange(loans);
        context.SaveChanges();
    }

    private static void SeedSalaryHistory(PrivatekonomyContext context, string userId)
    {
        // Create realistic salary history showing career progression over 5 years
        var salaryHistories = new List<SalaryHistory>
        {
            // Year 1 - Junior Developer
            new SalaryHistory
            {
                SalaryHistoryId = 1,
                MonthlySalary = 28000m,
                Period = new DateTime(2020, 1, 1),
                JobTitle = "Junior Utvecklare",
                Employer = "Tech Startup AB",
                EmploymentType = "Heltid",
                WorkPercentage = 100,
                Notes = "Första jobbet efter examen",
                Currency = "SEK",
                IsCurrent = false,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            // Year 2 - Salary revision
            new SalaryHistory
            {
                SalaryHistoryId = 2,
                MonthlySalary = 30000m,
                Period = new DateTime(2021, 1, 1),
                JobTitle = "Junior Utvecklare",
                Employer = "Tech Startup AB",
                EmploymentType = "Heltid",
                WorkPercentage = 100,
                Notes = "Årlig lönerevision",
                Currency = "SEK",
                IsCurrent = false,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            // Year 3 - Promotion
            new SalaryHistory
            {
                SalaryHistoryId = 3,
                MonthlySalary = 35000m,
                Period = new DateTime(2022, 7, 1),
                JobTitle = "Utvecklare",
                Employer = "Tech Startup AB",
                EmploymentType = "Heltid",
                WorkPercentage = 100,
                Notes = "Befordran till Utvecklare",
                Currency = "SEK",
                IsCurrent = false,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            // Year 4 - New job
            new SalaryHistory
            {
                SalaryHistoryId = 4,
                MonthlySalary = 42000m,
                Period = new DateTime(2023, 3, 1),
                JobTitle = "Senior Utvecklare",
                Employer = "Innovation Tech Group",
                EmploymentType = "Heltid",
                WorkPercentage = 100,
                Notes = "Nytt jobb med högre lön",
                Currency = "SEK",
                IsCurrent = false,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            // Year 4.5 - Salary revision
            new SalaryHistory
            {
                SalaryHistoryId = 5,
                MonthlySalary = 44000m,
                Period = new DateTime(2024, 1, 1),
                JobTitle = "Senior Utvecklare",
                Employer = "Innovation Tech Group",
                EmploymentType = "Heltid",
                WorkPercentage = 100,
                Notes = "Årlig lönerevision",
                Currency = "SEK",
                IsCurrent = false,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            },
            // Year 5 - Current position
            new SalaryHistory
            {
                SalaryHistoryId = 6,
                MonthlySalary = 48000m,
                Period = new DateTime(2025, 1, 1),
                JobTitle = "Senior Utvecklare",
                Employer = "Innovation Tech Group",
                EmploymentType = "Heltid",
                WorkPercentage = 100,
                Notes = "Lönerevision 2025",
                Currency = "SEK",
                IsCurrent = true,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            }
        };
        
        context.SalaryHistories.AddRange(salaryHistories);
        context.SaveChanges();
    }
  
    private static void SeedPockets(PrivatekonomyContext context, string userId)
    {
        // Create sample pockets on different savings accounts
        var pockets = new List<Pocket>
        {
            // Pockets on ICA-banken (BankSourceId = 1)
            new Pocket
            {
                PocketId = 1,
                Name = "Bilköp",
                Description = "Spara till ny bil - siktar på en elbil",
                TargetAmount = 450000m,
                CurrentAmount = 125000m,
                MonthlyAllocation = 25000m,
                BankSourceId = 1,
                Priority = 1,
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UserId = userId
            },
            new Pocket
            {
                PocketId = 2,
                Name = "Teknikinköp",
                Description = "Ny laptop, telefon och surfplatta",
                TargetAmount = 35000m,
                CurrentAmount = 28500m,
                MonthlyAllocation = 5000m,
                BankSourceId = 1,
                Priority = 2,
                CreatedAt = DateTime.UtcNow.AddMonths(-4),
                UserId = userId
            },
            new Pocket
            {
                PocketId = 3,
                Name = "Resor",
                Description = "Sommarlov i Italien och vinterresa till Japan (kan spara extra för lyxresa)",
                TargetAmount = 75000m,
                CurrentAmount = 42000m,
                MonthlyAllocation = 8000m,
                BankSourceId = 1,
                Priority = 2,
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                UserId = userId
            },

            // Pockets on Swedbank (BankSourceId = 2)
            new Pocket
            {
                PocketId = 4,
                Name = "Renovering",
                Description = "Renovera kök och badrum",
                TargetAmount = 250000m,
                CurrentAmount = 85000m,
                MonthlyAllocation = 15000m,
                BankSourceId = 2,
                Priority = 1,
                CreatedAt = DateTime.UtcNow.AddMonths(-8),
                UserId = userId
            },
            new Pocket
            {
                PocketId = 5,
                Name = "Trädgård",
                Description = "Altandäck, utemöbler och plantering",
                TargetAmount = 60000m,
                CurrentAmount = 15000m,
                MonthlyAllocation = 3000m,
                BankSourceId = 2,
                Priority = 3,
                CreatedAt = DateTime.UtcNow.AddMonths(-2),
                UserId = userId
            },
            new Pocket
            {
                PocketId = 6,
                Name = "Kläder",
                Description = "Vår- och höstgarderob",
                TargetAmount = 15000m,
                CurrentAmount = 12500m,
                BankSourceId = 2,
                Priority = 4,
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                UserId = userId
            },
            new Pocket
            {
                PocketId = 7,
                Name = "Mat & Uppläggning",
                Description = "Extra buffert för matinköp och storkok",
                TargetAmount = 8000m,
                CurrentAmount = 8000m,
                BankSourceId = 2,
                Priority = 2,
                CreatedAt = DateTime.UtcNow.AddMonths(-5),
                UserId = userId
            }
        };

        // Create some pocket transactions to show history
        var pocketTransactions = new List<PocketTransaction>
        {
            // Transactions for "Bilköp" pocket
            new PocketTransaction
            {
                PocketTransactionId = 1,
                PocketId = 1,
                Amount = 50000m,
                Type = "Deposit",
                Description = "Initial insättning från bonus",
                TransactionDate = DateTime.UtcNow.AddMonths(-6),
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UserId = userId
            },
            new PocketTransaction
            {
                PocketTransactionId = 2,
                PocketId = 1,
                Amount = 25000m,
                Type = "Deposit",
                Description = "Månadssparande",
                TransactionDate = DateTime.UtcNow.AddMonths(-5),
                CreatedAt = DateTime.UtcNow.AddMonths(-5),
                UserId = userId
            },
            new PocketTransaction
            {
                PocketTransactionId = 3,
                PocketId = 1,
                Amount = 25000m,
                Type = "Deposit",
                Description = "Månadssparande",
                TransactionDate = DateTime.UtcNow.AddMonths(-4),
                CreatedAt = DateTime.UtcNow.AddMonths(-4),
                UserId = userId
            },
            new PocketTransaction
            {
                PocketTransactionId = 4,
                PocketId = 1,
                Amount = 25000m,
                Type = "Deposit",
                Description = "Månadssparande",
                TransactionDate = DateTime.UtcNow.AddMonths(-3),
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                UserId = userId
            },

            // Transactions for "Teknikinköp" pocket
            new PocketTransaction
            {
                PocketTransactionId = 5,
                PocketId = 2,
                Amount = 15000m,
                Type = "Deposit",
                Description = "Startkapital",
                TransactionDate = DateTime.UtcNow.AddMonths(-4),
                CreatedAt = DateTime.UtcNow.AddMonths(-4),
                UserId = userId
            },
            new PocketTransaction
            {
                PocketTransactionId = 6,
                PocketId = 2,
                Amount = 6500m,
                Type = "Deposit",
                Description = "Extra sparande",
                TransactionDate = DateTime.UtcNow.AddMonths(-3),
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                UserId = userId
            },
            new PocketTransaction
            {
                PocketTransactionId = 7,
                PocketId = 2,
                Amount = 7000m,
                Type = "Deposit",
                Description = "Sparande från extrainkomst",
                TransactionDate = DateTime.UtcNow.AddMonths(-1),
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                UserId = userId
            },

            // Transactions for "Resor" pocket
            new PocketTransaction
            {
                PocketTransactionId = 8,
                PocketId = 3,
                Amount = 30000m,
                Type = "Deposit",
                Description = "Första insättningen för resefond",
                TransactionDate = DateTime.UtcNow.AddMonths(-3),
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                UserId = userId
            },
            new PocketTransaction
            {
                PocketTransactionId = 9,
                PocketId = 3,
                Amount = 12000m,
                Type = "Deposit",
                Description = "Månadssparande",
                TransactionDate = DateTime.UtcNow.AddMonths(-2),
                CreatedAt = DateTime.UtcNow.AddMonths(-2),
                UserId = userId
            },

            // Transactions for "Renovering" pocket
            new PocketTransaction
            {
                PocketTransactionId = 10,
                PocketId = 4,
                Amount = 40000m,
                Type = "Deposit",
                Description = "Initial insättning för renovering",
                TransactionDate = DateTime.UtcNow.AddMonths(-8),
                CreatedAt = DateTime.UtcNow.AddMonths(-8),
                UserId = userId
            },
            new PocketTransaction
            {
                PocketTransactionId = 11,
                PocketId = 4,
                Amount = 15000m,
                Type = "Deposit",
                Description = "Månadssparande",
                TransactionDate = DateTime.UtcNow.AddMonths(-6),
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UserId = userId
            },
            new PocketTransaction
            {
                PocketTransactionId = 12,
                PocketId = 4,
                Amount = 30000m,
                Type = "Deposit",
                Description = "Extra sparande från försäljning",
                TransactionDate = DateTime.UtcNow.AddMonths(-4),
                CreatedAt = DateTime.UtcNow.AddMonths(-4),
                UserId = userId
            },

            // Transaction for "Kläder" pocket (fully funded)
            new PocketTransaction
            {
                PocketTransactionId = 13,
                PocketId = 6,
                Amount = 12500m,
                Type = "Deposit",
                Description = "Klädbudget för året",
                TransactionDate = DateTime.UtcNow.AddMonths(-1),
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                UserId = userId
            },

            // Transaction for "Mat & Uppläggning" pocket (fully funded)
            new PocketTransaction
            {
                PocketTransactionId = 14,
                PocketId = 7,
                Amount = 8000m,
                Type = "Deposit",
                Description = "Matbuffert för månad",
                TransactionDate = DateTime.UtcNow.AddMonths(-5),
                CreatedAt = DateTime.UtcNow.AddMonths(-5),
                UserId = userId
            }
        };

        context.Pockets.AddRange(pockets);
        context.PocketTransactions.AddRange(pocketTransactions);
        context.SaveChanges();
    }

    private static void SeedNetWorthSnapshots(PrivatekonomyContext context, string userId)
    {
        // Create historical net worth snapshots for the last 24 months
        var snapshots = new List<NetWorthSnapshot>();
        var random = new Random(42); // Fixed seed for consistent test data
        
        // Starting values
        decimal startNetWorth = 150000m;
        decimal bankBalance = 50000m;
        decimal investmentValue = 450000m;
        decimal physicalAssetValue = 150000m;
        decimal loanBalance = 500000m;
        
        // Generate monthly snapshots for the last 24 months
        for (int i = 24; i >= 0; i--)
        {
            var date = DateTime.Today.AddMonths(-i);
            
            // Simulate realistic growth/changes
            // Net worth typically grows over time with some volatility
            var monthlyChange = (decimal)(random.NextDouble() * 0.04 - 0.01); // -1% to +3% monthly change
            
            // Update values
            bankBalance += (decimal)(random.Next(-5000, 8000)); // Random cash flow
            bankBalance = Math.Max(10000m, bankBalance); // Keep minimum balance
            
            investmentValue *= (1 + monthlyChange); // Market volatility
            
            physicalAssetValue -= 200m; // Slow depreciation
            
            loanBalance -= 2000m; // Amortization
            loanBalance = Math.Max(300000m, loanBalance); // Don't go below a certain amount
            
            var totalAssets = bankBalance + investmentValue + physicalAssetValue;
            var netWorth = totalAssets - loanBalance;
            
            snapshots.Add(new NetWorthSnapshot
            {
                Date = date,
                TotalAssets = totalAssets,
                TotalLiabilities = loanBalance,
                NetWorth = netWorth,
                BankBalance = bankBalance,
                InvestmentValue = investmentValue,
                PhysicalAssetValue = physicalAssetValue,
                LoanBalance = loanBalance,
                IsManual = false,
                Notes = $"Automatisk snapshot för {date:yyyy-MM-dd}",
                CreatedAt = date,
                UserId = userId
            });
        }
        
        context.NetWorthSnapshots.AddRange(snapshots);
        context.SaveChanges();
    }
}
