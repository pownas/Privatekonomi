using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Data;

public static class TestDataSeeder
{
    public static void SeedTestData(PrivatekonomyContext context)
    {
        // Check if there are already transactions to avoid duplicate seeding
        if (context.Transactions.Any())
        {
            return;
        }

        SeedTransactions(context);
        SeedInvestments(context);
        SeedBudgets(context);
        SeedHouseholds(context);
    }

    private static void SeedTransactions(PrivatekonomyContext context)
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
                BankSourceId = bankSourceIds[random.Next(bankSourceIds.Length)]
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
                BankSourceId = bankSourceIds[random.Next(bankSourceIds.Length)]
            };
            
            transactions.Add(transaction);
            transactionId++;
        }

        // Add all transactions and categories to context
        context.Transactions.AddRange(transactions);
        context.TransactionCategories.AddRange(transactionCategories);
        context.SaveChanges();
    }

    private static void SeedInvestments(PrivatekonomyContext context)
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
                Market = "Stockholm"
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
                Market = "Stockholm"
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
                Market = "Stockholm"
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
                LastUpdated = DateTime.Now.AddDays(-3)
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
                LastUpdated = DateTime.Now.AddDays(-1)
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
                LastUpdated = DateTime.Now.AddDays(-2)
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
                Market = "Stockholm"
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
                Market = "Stockholm"
            }
        };

        context.Investments.AddRange(investments);
        context.SaveChanges();
    }

    private static void SeedBudgets(PrivatekonomyContext context)
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
            Period = BudgetPeriod.Monthly
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
            Period = BudgetPeriod.Monthly
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
}
