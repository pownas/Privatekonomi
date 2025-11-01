namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a customizable dashboard layout for a user.
/// Users can create multiple layouts for different purposes (e.g., "Home", "Investments", "Budget").
/// </summary>
public class DashboardLayout
{
    public int LayoutId { get; set; }
    
    /// <summary>
    /// User who owns this layout
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the layout (e.g., "Hem", "Investeringar", "Budget")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this is the default layout to show on dashboard
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Widget configurations for this layout
    /// </summary>
    public List<WidgetConfiguration> Widgets { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public ApplicationUser? User { get; set; }
}
