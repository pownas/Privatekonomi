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
                IsIncome = isIncome
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
                Type = "Aktie",
                Quantity = 100,
                PurchasePrice = 245.50m,
                CurrentPrice = 268.75m,
                PurchaseDate = DateTime.Now.AddMonths(-6),
                LastUpdated = DateTime.Now.AddDays(-1)
            },
            new Investment
            {
                InvestmentId = 2,
                Name = "SEB A",
                Type = "Aktie",
                Quantity = 50,
                PurchasePrice = 152.30m,
                CurrentPrice = 165.20m,
                PurchaseDate = DateTime.Now.AddMonths(-8),
                LastUpdated = DateTime.Now.AddDays(-2)
            },
            new Investment
            {
                InvestmentId = 3,
                Name = "Investor B",
                Type = "Aktie",
                Quantity = 75,
                PurchasePrice = 289.00m,
                CurrentPrice = 312.50m,
                PurchaseDate = DateTime.Now.AddMonths(-4),
                LastUpdated = DateTime.Now.AddDays(-1)
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
                Type = "Aktie",
                Quantity = 250,
                PurchasePrice = 58.20m,
                CurrentPrice = 62.15m,
                PurchaseDate = DateTime.Now.AddMonths(-5),
                LastUpdated = DateTime.Now
            },
            new Investment
            {
                InvestmentId = 8,
                Name = "Nordea",
                Type = "Aktie",
                Quantity = 80,
                PurchasePrice = 125.40m,
                CurrentPrice = 118.90m,
                PurchaseDate = DateTime.Now.AddMonths(-3),
                LastUpdated = DateTime.Now.AddDays(-1)
            }
        };

        context.Investments.AddRange(investments);
        context.SaveChanges();
    }
}
