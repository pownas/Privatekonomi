using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Data;

        await SeedCategoriesAsync(context);
        await SeedBankSourcesAsync(context);
        await SeedCategoryRulesAsync(context);
{
    public static async Task SeedProductionDataAsync(PrivatekonomyContext context)
    private static async Task SeedCategoriesAsync(PrivatekonomyContext context)
        await SeedCategoryRulesAsync(context);
        if (await context.Categories.AnyAsync(category => category.IsSystemCategory))
        {
            return;
        }

        var existingCategories = await context.Categories
            .Where(category => category.IsSystemCategory)
            .ToDictionaryAsync(category => category.Name, category => category);

        var now = DateTime.UtcNow;

        var topLevelCategories = new List<(string Name, string AccountNumber, string Color)>
        {
            ("Mat & Dryck", "5000", "#FF6B6B"),
            ("Transport", "6000", "#4ECDC4"),
            ("Boende", "4000", "#45B7D1"),
            ("Nöje", "7000", "#FFA07A"),
            ("Shopping", "5500", "#98D8C8"),
            ("Hälsa", "7500", "#6BCF7F"),
            ("Lön", "3000", "#4CAF50"),
            ("Sparande", "8000", "#2196F3"),
            ("Övrigt", "6900", "#9E9E9E")
        };

        var categoriesAdded = false;

        foreach (var (name, accountNumber, color) in topLevelCategories)
        {
            if (existingCategories.ContainsKey(name))
            {
                continue;
            }

            var category = new Category
            {
                Name = name,
                AccountNumber = accountNumber,
                Color = color,
                TaxRelated = false,
                IsSystemCategory = true,
                OriginalName = name,
                OriginalColor = color,
                OriginalAccountNumber = accountNumber,
                CreatedAt = now
            };

            await context.Categories.AddAsync(category);
            existingCategories[name] = category;
            categoriesAdded = true;
        }

        if (categoriesAdded)
        {
            await context.SaveChangesAsync();
            existingCategories = await context.Categories
                .Where(category => category.IsSystemCategory)
                .ToDictionaryAsync(category => category.Name, category => category);
        }

        var subCategories = new List<(string Name, string AccountNumber, string Color, string ParentName)>
        {
            ("Livsmedel", "5100", "#FF6B6B", "Mat & Dryck"),
            ("Restaurang", "5200", "#FF5252", "Mat & Dryck"),
            ("Café", "5300", "#FF8A80", "Mat & Dryck"),
            ("Kollektivtrafik", "6100", "#4ECDC4", "Transport"),
            ("Bensin", "6200", "#26A69A", "Transport"),
            ("Parkering", "6500", "#80CBC4", "Transport"),
            ("Hyra/Avgift", "4100", "#45B7D1", "Boende"),
            ("El", "4200", "#29B6F6", "Boende"),
            ("Bredband", "4300", "#81D4FA", "Boende"),
            ("Hemförsäkring", "4400", "#4FC3F7", "Boende"),
            ("Streaming", "7100", "#FFA07A", "Nöje"),
            ("Gym", "7300", "#FF8A65", "Nöje"),
            ("Resor", "7400", "#FFAB91", "Nöje"),
            ("Kläder", "5510", "#98D8C8", "Shopping"),
            ("Hygienartiklar", "5520", "#80CBC4", "Shopping"),
            ("Elektronik", "5550", "#B2DFDB", "Shopping"),
            ("Tandvård", "7510", "#6BCF7F", "Hälsa"),
            ("Läkarvård", "7520", "#81C784", "Hälsa"),
            ("Medicin", "7530", "#A5D6A7", "Hälsa"),
            ("Bonus", "3010", "#66BB6A", "Lön"),
            ("Semesterersättning", "3020", "#81C784", "Lön"),
            ("Buffert", "8100", "#2196F3", "Sparande"),
            ("Månadsspar Fonder", "8200", "#42A5F5", "Sparande"),
            ("ISK", "8300", "#64B5F6", "Sparande"),
            ("Pensionssparande", "8400", "#90CAF9", "Sparande")
        };

        categoriesAdded = false;

        foreach (var (name, accountNumber, color, parentName) in subCategories)
        {
            if (existingCategories.ContainsKey(name))
            {
                continue;
            }

            if (!existingCategories.TryGetValue(parentName, out var parentCategory))
            {
                continue;
            }

            var category = new Category
            {
                Name = name,
                AccountNumber = accountNumber,
                Color = color,
                ParentId = parentCategory.CategoryId,
                TaxRelated = false,
                IsSystemCategory = true,
                OriginalName = name,
                OriginalColor = color,
                OriginalAccountNumber = accountNumber,
                CreatedAt = now
            };

            await context.Categories.AddAsync(category);
            existingCategories[name] = category;
            categoriesAdded = true;
        }

        if (categoriesAdded)
        {
            await context.SaveChangesAsync();
        }
        }

    private static async Task SeedBankSourcesAsync(PrivatekonomyContext context)
    {
        var existingBankSources = await context.BankSources
            .Select(bank => bank.Name)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase);

        var now = DateTime.UtcNow;

        var definitions = new List<(string Name, string Color, string AccountType)>
        {
            ("ICA-banken", "#DC143C", "checking"),
            ("Swedbank", "#FF8C00", "checking"),
            ("SEB", "#0066CC", "checking"),
            ("Nordea", "#00A9CE", "checking"),
            ("Handelsbanken", "#003366", "checking"),
            ("Avanza", "#006400", "investment")
        };

        var banksAdded = false;

        foreach (var (name, color, accountType) in definitions)
        {
            if (existingBankSources.Contains(name))
            {
                continue;
            }

            await context.BankSources.AddAsync(new BankSource
            {
                Name = name,
                Color = color,
                AccountType = accountType,
                Currency = "SEK",
                InitialBalance = 0,
                CreatedAt = now,
                ValidFrom = now
            });
            banksAdded = true;
        }

        if (banksAdded)
        {
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedCategoryRulesAsync(PrivatekonomyContext context)
    {
        var existingRules = await context.CategoryRules
            .Select(rule => rule.Pattern)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase);

        var categoryLookup = await context.Categories
            .Where(category => category.IsSystemCategory)
            .ToDictionaryAsync(category => category.Name, category => category.CategoryId);

        var definitions = new List<(string Pattern, string CategoryName, int Priority, string Description)>
        {
            ("ICA", "Mat & Dryck", 100, "ICA stores"),
            ("Coop", "Mat & Dryck", 100, "Coop stores"),
            ("Willys", "Mat & Dryck", 100, "Willys stores"),
            ("Hemköp", "Mat & Dryck", 100, "Hemköp stores"),
            ("Restaurant", "Mat & Dryck", 90, "Restaurants"),
            ("Restaurang", "Mat & Dryck", 90, "Swedish restaurants"),
            ("Cafe", "Mat & Dryck", 90, "Cafes"),
            ("McDonalds", "Mat & Dryck", 95, "McDonald's"),
            ("Pizza", "Mat & Dryck", 90, "Pizza places"),
            ("Sushi", "Mat & Dryck", 90, "Sushi restaurants"),
            ("SL-kort", "Transport", 100, "Stockholm public transport"),
            ("Bensin", "Transport", 95, "Gas stations"),
            ("Circle K", "Transport", 100, "Circle K gas"),
            ("Preem", "Transport", 100, "Preem gas"),
            ("Parkering", "Transport", 95, "Parking"),
            ("Taxi", "Transport", 95, "Taxi services"),
            ("SJ", "Transport", 100, "Swedish railways"),
            ("Hyra", "Boende", 100, "Rent"),
            ("Vattenfall", "Boende", 100, "Vattenfall electricity"),
            ("Telia", "Boende", 100, "Telia broadband"),
            ("Bredband", "Boende", 95, "Broadband"),
            ("Försäkring", "Boende", 95, "Insurance"),
            ("Spotify", "Nöje", 100, "Spotify subscription"),
            ("Netflix", "Nöje", 100, "Netflix subscription"),
            ("Bio", "Nöje", 90, "Cinema"),
            ("SF", "Nöje", 95, "SF Bio cinema"),
            ("Gym", "Nöje", 90, "Gym memberships"),
            ("Teater", "Nöje", 90, "Theatre"),
            ("Konsert", "Nöje", 90, "Concerts"),
            ("H&M", "Shopping", 100, "H&M stores"),
            ("IKEA", "Shopping", 100, "IKEA"),
            ("Elgiganten", "Shopping", 100, "Elgiganten electronics"),
            ("Clas Ohlson", "Shopping", 100, "Clas Ohlson"),
            ("Stadium", "Shopping", 100, "Stadium sports"),
            ("Apoteket", "Shopping", 95, "Pharmacy"),
            ("Folktandvården", "Hälsa", 100, "Public dental care"),
            ("Apotek", "Hälsa", 95, "Pharmacy (health)"),
            ("Naprapat", "Hälsa", 100, "Naprapathy"),
            ("Vitaminer", "Hälsa", 90, "Vitamins"),
            ("Lön", "Lön", 100, "Salary"),
            ("Bonus", "Lön", 95, "Bonus"),
            ("Semesterersättning", "Lön", 95, "Vacation pay"),
            ("Sparande", "Sparande", 100, "Savings"),
            ("ISK", "Sparande", 95, "Investment savings account")
        };

        var rulesToAdd = new List<CategoryRule>();
        var now = DateTime.UtcNow;

        foreach (var (pattern, categoryName, priority, description) in definitions)
        {
            if (existingRules.Contains(pattern))
            {
                continue;
            }

            if (!categoryLookup.TryGetValue(categoryName, out var categoryId))
            {
                continue;
            }

            rulesToAdd.Add(new CategoryRule
            {
                Pattern = pattern,
                MatchType = PatternMatchType.Contains,
                CategoryId = categoryId,
                Priority = priority,
                IsActive = true,
                Description = description,
                Field = MatchField.Both,
                RuleType = RuleType.System,
                CreatedAt = now
            });
        }

        if (rulesToAdd.Count == 0)
        {
            return;
        }

        await context.CategoryRules.AddRangeAsync(rulesToAdd);
        await context.SaveChangesAsync();
    }

        await context.CategoryRules.AddRangeAsync(rulesToAdd);
        await context.SaveChangesAsync();
}
