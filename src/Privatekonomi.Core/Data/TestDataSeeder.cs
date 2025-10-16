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
}
