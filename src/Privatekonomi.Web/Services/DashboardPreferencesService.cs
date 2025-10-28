using System.Text.Json;

namespace Privatekonomi.Web.Services;

public class DashboardPreferencesService
{
    public const string StorageKey = "dashboard_preferences";
    
    public event Action? OnPreferencesChanged;
    
    public DashboardPreferences Preferences { get; private set; } = new();
    
    public void LoadPreferences(string? json)
    {
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                var preferences = JsonSerializer.Deserialize<DashboardPreferences>(json);
                if (preferences != null)
                {
                    Preferences = preferences;
                }
            }
            catch
            {
                // Use default preferences if deserialization fails
                Preferences = new DashboardPreferences();
            }
        }
    }
    
    public string SavePreferences()
    {
        return JsonSerializer.Serialize(Preferences);
    }
    
    public void UpdatePreferences(DashboardPreferences preferences)
    {
        Preferences = preferences;
        OnPreferencesChanged?.Invoke();
    }
}

public class DashboardPreferences
{
    public bool ShowTotalCards { get; set; } = true;
    public bool ShowCashFlowChart { get; set; } = true;
    public bool ShowExpensePieChart { get; set; } = true;
    public bool ShowIncomePieChart { get; set; } = true;
    public bool ShowCategoryBarChart { get; set; } = true;
    public bool ShowActiveBudgets { get; set; } = true;
    public bool ShowUnmappedTransactions { get; set; } = true;
    public bool ShowRecentTransactions { get; set; } = true;
    public int RecentTransactionsCount { get; set; } = 10;
    public int DefaultPeriodMonths { get; set; } = 12;
    
    public DashboardPreferences Clone()
    {
        return new DashboardPreferences
        {
            ShowTotalCards = this.ShowTotalCards,
            ShowCashFlowChart = this.ShowCashFlowChart,
            ShowExpensePieChart = this.ShowExpensePieChart,
            ShowIncomePieChart = this.ShowIncomePieChart,
            ShowCategoryBarChart = this.ShowCategoryBarChart,
            ShowActiveBudgets = this.ShowActiveBudgets,
            ShowUnmappedTransactions = this.ShowUnmappedTransactions,
            ShowRecentTransactions = this.ShowRecentTransactions,
            RecentTransactionsCount = this.RecentTransactionsCount,
            DefaultPeriodMonths = this.DefaultPeriodMonths
        };
    }
}
