using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Data;

public static class ProdDataSeeder
{
    public static async Task SeedProductionDataAsync(PrivatekonomyContext context)
    {
        await SeedCategoriesAsync(context);
        await SeedBankSourcesAsync(context);
        await SeedCategoryRulesAsync(context);
    }

    private static async Task SeedCategoriesAsync(PrivatekonomyContext context)
    {
        if (await context.Categories.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var categories = new List<Category>
        {
            new Category { CategoryId = 1, Name = "Mat & Dryck", AccountNumber = "5000", Color = "#FF6B6B", TaxRelated = false, IsSystemCategory = true, OriginalName = "Mat & Dryck", OriginalColor = "#FF6B6B", OriginalAccountNumber = "5000", CreatedAt = now },
            new Category { CategoryId = 2, Name = "Transport", AccountNumber = "6000", Color = "#4ECDC4", TaxRelated = false, IsSystemCategory = true, OriginalName = "Transport", OriginalColor = "#4ECDC4", OriginalAccountNumber = "6000", CreatedAt = now },
            new Category { CategoryId = 3, Name = "Boende", AccountNumber = "4000", Color = "#45B7D1", TaxRelated = false, IsSystemCategory = true, OriginalName = "Boende", OriginalColor = "#45B7D1", OriginalAccountNumber = "4000", CreatedAt = now },
            new Category { CategoryId = 4, Name = "Nöje", AccountNumber = "7000", Color = "#FFA07A", TaxRelated = false, IsSystemCategory = true, OriginalName = "Nöje", OriginalColor = "#FFA07A", OriginalAccountNumber = "7000", CreatedAt = now },
            new Category { CategoryId = 5, Name = "Shopping", AccountNumber = "5500", Color = "#98D8C8", TaxRelated = false, IsSystemCategory = true, OriginalName = "Shopping", OriginalColor = "#98D8C8", OriginalAccountNumber = "5500", CreatedAt = now },
            new Category { CategoryId = 6, Name = "Hälsa", AccountNumber = "7500", Color = "#6BCF7F", TaxRelated = false, IsSystemCategory = true, OriginalName = "Hälsa", OriginalColor = "#6BCF7F", OriginalAccountNumber = "7500", CreatedAt = now },
            new Category { CategoryId = 7, Name = "Lön", AccountNumber = "3000", Color = "#4CAF50", TaxRelated = false, IsSystemCategory = true, OriginalName = "Lön", OriginalColor = "#4CAF50", OriginalAccountNumber = "3000", CreatedAt = now },
            new Category { CategoryId = 8, Name = "Sparande", AccountNumber = "8000", Color = "#2196F3", TaxRelated = false, IsSystemCategory = true, OriginalName = "Sparande", OriginalColor = "#2196F3", OriginalAccountNumber = "8000", CreatedAt = now },
            new Category { CategoryId = 9, Name = "Övrigt", AccountNumber = "6900", Color = "#9E9E9E", TaxRelated = false, IsSystemCategory = true, OriginalName = "Övrigt", OriginalColor = "#9E9E9E", OriginalAccountNumber = "6900", CreatedAt = now },
            new Category { CategoryId = 10, Name = "Livsmedel", AccountNumber = "5100", Color = "#FF6B6B", ParentId = 1, TaxRelated = false, IsSystemCategory = true, OriginalName = "Livsmedel", OriginalColor = "#FF6B6B", OriginalAccountNumber = "5100", CreatedAt = now },
            new Category { CategoryId = 11, Name = "Restaurang", AccountNumber = "5200", Color = "#FF5252", ParentId = 1, TaxRelated = false, IsSystemCategory = true, OriginalName = "Restaurang", OriginalColor = "#FF5252", OriginalAccountNumber = "5200", CreatedAt = now },
            new Category { CategoryId = 12, Name = "Café", AccountNumber = "5300", Color = "#FF8A80", ParentId = 1, TaxRelated = false, IsSystemCategory = true, OriginalName = "Café", OriginalColor = "#FF8A80", OriginalAccountNumber = "5300", CreatedAt = now },
            new Category { CategoryId = 13, Name = "Kollektivtrafik", AccountNumber = "6100", Color = "#4ECDC4", ParentId = 2, TaxRelated = false, IsSystemCategory = true, OriginalName = "Kollektivtrafik", OriginalColor = "#4ECDC4", OriginalAccountNumber = "6100", CreatedAt = now },
            new Category { CategoryId = 14, Name = "Bensin", AccountNumber = "6200", Color = "#26A69A", ParentId = 2, TaxRelated = false, IsSystemCategory = true, OriginalName = "Bensin", OriginalColor = "#26A69A", OriginalAccountNumber = "6200", CreatedAt = now },
            new Category { CategoryId = 15, Name = "Parkering", AccountNumber = "6500", Color = "#80CBC4", ParentId = 2, TaxRelated = false, IsSystemCategory = true, OriginalName = "Parkering", OriginalColor = "#80CBC4", OriginalAccountNumber = "6500", CreatedAt = now },
            new Category { CategoryId = 16, Name = "Hyra/Avgift", AccountNumber = "4100", Color = "#45B7D1", ParentId = 3, TaxRelated = false, IsSystemCategory = true, OriginalName = "Hyra/Avgift", OriginalColor = "#45B7D1", OriginalAccountNumber = "4100", CreatedAt = now },
            new Category { CategoryId = 17, Name = "El", AccountNumber = "4200", Color = "#29B6F6", ParentId = 3, TaxRelated = false, IsSystemCategory = true, OriginalName = "El", OriginalColor = "#29B6F6", OriginalAccountNumber = "4200", CreatedAt = now },
            new Category { CategoryId = 18, Name = "Bredband", AccountNumber = "4300", Color = "#81D4FA", ParentId = 3, TaxRelated = false, IsSystemCategory = true, OriginalName = "Bredband", OriginalColor = "#81D4FA", OriginalAccountNumber = "4300", CreatedAt = now },
            new Category { CategoryId = 19, Name = "Hemförsäkring", AccountNumber = "4400", Color = "#4FC3F7", ParentId = 3, TaxRelated = false, IsSystemCategory = true, OriginalName = "Hemförsäkring", OriginalColor = "#4FC3F7", OriginalAccountNumber = "4400", CreatedAt = now },
            new Category { CategoryId = 20, Name = "Streaming", AccountNumber = "7100", Color = "#FFA07A", ParentId = 4, TaxRelated = false, IsSystemCategory = true, OriginalName = "Streaming", OriginalColor = "#FFA07A", OriginalAccountNumber = "7100", CreatedAt = now },
            new Category { CategoryId = 21, Name = "Gym", AccountNumber = "7300", Color = "#FF8A65", ParentId = 4, TaxRelated = false, IsSystemCategory = true, OriginalName = "Gym", OriginalColor = "#FF8A65", OriginalAccountNumber = "7300", CreatedAt = now },
            new Category { CategoryId = 22, Name = "Resor", AccountNumber = "7400", Color = "#FFAB91", ParentId = 4, TaxRelated = false, IsSystemCategory = true, OriginalName = "Resor", OriginalColor = "#FFAB91", OriginalAccountNumber = "7400", CreatedAt = now },
            new Category { CategoryId = 23, Name = "Kläder", AccountNumber = "5510", Color = "#98D8C8", ParentId = 5, TaxRelated = false, IsSystemCategory = true, OriginalName = "Kläder", OriginalColor = "#98D8C8", OriginalAccountNumber = "5510", CreatedAt = now },
            new Category { CategoryId = 24, Name = "Hygienartiklar", AccountNumber = "5520", Color = "#80CBC4", ParentId = 5, TaxRelated = false, IsSystemCategory = true, OriginalName = "Hygienartiklar", OriginalColor = "#80CBC4", OriginalAccountNumber = "5520", CreatedAt = now },
            new Category { CategoryId = 25, Name = "Elektronik", AccountNumber = "5550", Color = "#B2DFDB", ParentId = 5, TaxRelated = false, IsSystemCategory = true, OriginalName = "Elektronik", OriginalColor = "#B2DFDB", OriginalAccountNumber = "5550", CreatedAt = now },
            new Category { CategoryId = 26, Name = "Tandvård", AccountNumber = "7510", Color = "#6BCF7F", ParentId = 6, TaxRelated = false, IsSystemCategory = true, OriginalName = "Tandvård", OriginalColor = "#6BCF7F", OriginalAccountNumber = "7510", CreatedAt = now },
            new Category { CategoryId = 27, Name = "Läkarvård", AccountNumber = "7520", Color = "#81C784", ParentId = 6, TaxRelated = false, IsSystemCategory = true, OriginalName = "Läkarvård", OriginalColor = "#81C784", OriginalAccountNumber = "7520", CreatedAt = now },
            new Category { CategoryId = 28, Name = "Medicin", AccountNumber = "7530", Color = "#A5D6A7", ParentId = 6, TaxRelated = false, IsSystemCategory = true, OriginalName = "Medicin", OriginalColor = "#A5D6A7", OriginalAccountNumber = "7530", CreatedAt = now },
            new Category { CategoryId = 29, Name = "Bonus", AccountNumber = "3010", Color = "#66BB6A", ParentId = 7, TaxRelated = false, IsSystemCategory = true, OriginalName = "Bonus", OriginalColor = "#66BB6A", OriginalAccountNumber = "3010", CreatedAt = now },
            new Category { CategoryId = 30, Name = "Semesterersättning", AccountNumber = "3020", Color = "#81C784", ParentId = 7, TaxRelated = false, IsSystemCategory = true, OriginalName = "Semesterersättning", OriginalColor = "#81C784", OriginalAccountNumber = "3020", CreatedAt = now },
            new Category { CategoryId = 31, Name = "Buffert", AccountNumber = "8100", Color = "#2196F3", ParentId = 8, TaxRelated = false, IsSystemCategory = true, OriginalName = "Buffert", OriginalColor = "#2196F3", OriginalAccountNumber = "8100", CreatedAt = now },
            new Category { CategoryId = 32, Name = "Månadsspar Fonder", AccountNumber = "8200", Color = "#42A5F5", ParentId = 8, TaxRelated = false, IsSystemCategory = true, OriginalName = "Månadsspar Fonder", OriginalColor = "#42A5F5", OriginalAccountNumber = "8200", CreatedAt = now },
            new Category { CategoryId = 33, Name = "ISK", AccountNumber = "8300", Color = "#64B5F6", ParentId = 8, TaxRelated = false, IsSystemCategory = true, OriginalName = "ISK", OriginalColor = "#64B5F6", OriginalAccountNumber = "8300", CreatedAt = now },
            new Category { CategoryId = 34, Name = "Pensionssparande", AccountNumber = "8400", Color = "#90CAF9", ParentId = 8, TaxRelated = false, IsSystemCategory = true, OriginalName = "Pensionssparande", OriginalColor = "#90CAF9", OriginalAccountNumber = "8400", CreatedAt = now }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedBankSourcesAsync(PrivatekonomyContext context)
    {
        if (await context.BankSources.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var bankSources = new List<BankSource>
        {
            new BankSource { BankSourceId = 1, Name = "ICA-banken", Color = "#DC143C", AccountType = "checking", Currency = "SEK", InitialBalance = 0, CreatedAt = now, ValidFrom = now, ValidTo = null },
            new BankSource { BankSourceId = 2, Name = "Swedbank", Color = "#FF8C00", AccountType = "checking", Currency = "SEK", InitialBalance = 0, CreatedAt = now, ValidFrom = now, ValidTo = null },
            new BankSource { BankSourceId = 3, Name = "SEB", Color = "#0066CC", AccountType = "checking", Currency = "SEK", InitialBalance = 0, CreatedAt = now, ValidFrom = now, ValidTo = null },
            new BankSource { BankSourceId = 4, Name = "Nordea", Color = "#00A9CE", AccountType = "checking", Currency = "SEK", InitialBalance = 0, CreatedAt = now, ValidFrom = now, ValidTo = null },
            new BankSource { BankSourceId = 5, Name = "Handelsbanken", Color = "#003366", AccountType = "checking", Currency = "SEK", InitialBalance = 0, CreatedAt = now, ValidFrom = now, ValidTo = null },
            new BankSource { BankSourceId = 6, Name = "Avanza", Color = "#006400", AccountType = "investment", Currency = "SEK", InitialBalance = 0, CreatedAt = now, ValidFrom = now, ValidTo = null }
        };

        await context.BankSources.AddRangeAsync(bankSources);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCategoryRulesAsync(PrivatekonomyContext context)
    {
        if (await context.CategoryRules.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var categoryRules = new List<CategoryRule>
        {
            // Mat & Dryck (Category 1)
            new CategoryRule { CategoryRuleId = 1, Pattern = "ICA", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 100, IsActive = true, Description = "ICA stores", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 2, Pattern = "Coop", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 100, IsActive = true, Description = "Coop stores", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 3, Pattern = "Willys", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 100, IsActive = true, Description = "Willys stores", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 4, Pattern = "Hemköp", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 100, IsActive = true, Description = "Hemköp stores", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 5, Pattern = "Restaurant", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 90, IsActive = true, Description = "Restaurants", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 6, Pattern = "Restaurang", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 90, IsActive = true, Description = "Swedish restaurants", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 7, Pattern = "Cafe", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 90, IsActive = true, Description = "Cafes", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 8, Pattern = "McDonalds", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 95, IsActive = true, Description = "McDonald's", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 9, Pattern = "Pizza", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 90, IsActive = true, Description = "Pizza places", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 10, Pattern = "Sushi", MatchType = PatternMatchType.Contains, CategoryId = 1, Priority = 90, IsActive = true, Description = "Sushi restaurants", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },

            // Transport (Category 2)
            new CategoryRule { CategoryRuleId = 11, Pattern = "SL-kort", MatchType = PatternMatchType.Contains, CategoryId = 2, Priority = 100, IsActive = true, Description = "Stockholm public transport", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 12, Pattern = "Bensin", MatchType = PatternMatchType.Contains, CategoryId = 2, Priority = 95, IsActive = true, Description = "Gas stations", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 13, Pattern = "Circle K", MatchType = PatternMatchType.Contains, CategoryId = 2, Priority = 100, IsActive = true, Description = "Circle K gas", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 14, Pattern = "Preem", MatchType = PatternMatchType.Contains, CategoryId = 2, Priority = 100, IsActive = true, Description = "Preem gas", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 15, Pattern = "Parkering", MatchType = PatternMatchType.Contains, CategoryId = 2, Priority = 95, IsActive = true, Description = "Parking", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 16, Pattern = "Taxi", MatchType = PatternMatchType.Contains, CategoryId = 2, Priority = 95, IsActive = true, Description = "Taxi services", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 17, Pattern = "SJ", MatchType = PatternMatchType.Contains, CategoryId = 2, Priority = 100, IsActive = true, Description = "Swedish railways", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },

            // Boende (Category 3)
            new CategoryRule { CategoryRuleId = 18, Pattern = "Hyra", MatchType = PatternMatchType.Contains, CategoryId = 3, Priority = 100, IsActive = true, Description = "Rent", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 19, Pattern = "Vattenfall", MatchType = PatternMatchType.Contains, CategoryId = 3, Priority = 100, IsActive = true, Description = "Vattenfall electricity", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 20, Pattern = "Telia", MatchType = PatternMatchType.Contains, CategoryId = 3, Priority = 100, IsActive = true, Description = "Telia broadband", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 21, Pattern = "Bredband", MatchType = PatternMatchType.Contains, CategoryId = 3, Priority = 95, IsActive = true, Description = "Broadband", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 22, Pattern = "Försäkring", MatchType = PatternMatchType.Contains, CategoryId = 3, Priority = 95, IsActive = true, Description = "Insurance", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },

            // Nöje (Category 4)
            new CategoryRule { CategoryRuleId = 23, Pattern = "Spotify", MatchType = PatternMatchType.Contains, CategoryId = 4, Priority = 100, IsActive = true, Description = "Spotify subscription", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 24, Pattern = "Netflix", MatchType = PatternMatchType.Contains, CategoryId = 4, Priority = 100, IsActive = true, Description = "Netflix subscription", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 25, Pattern = "Bio", MatchType = PatternMatchType.Contains, CategoryId = 4, Priority = 90, IsActive = true, Description = "Cinema", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 26, Pattern = "SF", MatchType = PatternMatchType.StartsWith, CategoryId = 4, Priority = 95, IsActive = true, Description = "SF Bio cinema", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 27, Pattern = "Gym", MatchType = PatternMatchType.Contains, CategoryId = 4, Priority = 90, IsActive = true, Description = "Gym memberships", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 28, Pattern = "Teater", MatchType = PatternMatchType.Contains, CategoryId = 4, Priority = 90, IsActive = true, Description = "Theatre", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 29, Pattern = "Konsert", MatchType = PatternMatchType.Contains, CategoryId = 4, Priority = 90, IsActive = true, Description = "Concerts", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },

            // Shopping (Category 5)
            new CategoryRule { CategoryRuleId = 30, Pattern = "H&M", MatchType = PatternMatchType.Contains, CategoryId = 5, Priority = 100, IsActive = true, Description = "H&M stores", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 31, Pattern = "IKEA", MatchType = PatternMatchType.Contains, CategoryId = 5, Priority = 100, IsActive = true, Description = "IKEA", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 32, Pattern = "Elgiganten", MatchType = PatternMatchType.Contains, CategoryId = 5, Priority = 100, IsActive = true, Description = "Elgiganten electronics", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 33, Pattern = "Clas Ohlson", MatchType = PatternMatchType.Contains, CategoryId = 5, Priority = 100, IsActive = true, Description = "Clas Ohlson", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 34, Pattern = "Stadium", MatchType = PatternMatchType.Contains, CategoryId = 5, Priority = 100, IsActive = true, Description = "Stadium sports", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 35, Pattern = "Apoteket", MatchType = PatternMatchType.Contains, CategoryId = 5, Priority = 95, IsActive = true, Description = "Pharmacy", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },

            // Hälsa (Category 6)
            new CategoryRule { CategoryRuleId = 36, Pattern = "Folktandvården", MatchType = PatternMatchType.Contains, CategoryId = 6, Priority = 100, IsActive = true, Description = "Public dental care", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 37, Pattern = "Apotek", MatchType = PatternMatchType.Contains, CategoryId = 6, Priority = 95, IsActive = true, Description = "Pharmacy (health)", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 38, Pattern = "Naprapat", MatchType = PatternMatchType.Contains, CategoryId = 6, Priority = 100, IsActive = true, Description = "Naprapathy", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 39, Pattern = "Vitaminer", MatchType = PatternMatchType.Contains, CategoryId = 6, Priority = 90, IsActive = true, Description = "Vitamins", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },

            // Lön (Category 7)
            new CategoryRule { CategoryRuleId = 40, Pattern = "Lön", MatchType = PatternMatchType.Contains, CategoryId = 7, Priority = 100, IsActive = true, Description = "Salary", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 41, Pattern = "Bonus", MatchType = PatternMatchType.Contains, CategoryId = 7, Priority = 95, IsActive = true, Description = "Bonus", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 42, Pattern = "Semesterersättning", MatchType = PatternMatchType.Contains, CategoryId = 7, Priority = 95, IsActive = true, Description = "Vacation pay", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },

            // Sparande (Category 8)
            new CategoryRule { CategoryRuleId = 43, Pattern = "Sparande", MatchType = PatternMatchType.Contains, CategoryId = 8, Priority = 100, IsActive = true, Description = "Savings", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now },
            new CategoryRule { CategoryRuleId = 44, Pattern = "ISK", MatchType = PatternMatchType.Contains, CategoryId = 8, Priority = 95, IsActive = true, Description = "Investment savings account", Field = MatchField.Both, RuleType = RuleType.System, CreatedAt = now }
        };

        await context.CategoryRules.AddRangeAsync(categoryRules);
        await context.SaveChangesAsync();
    }
}
