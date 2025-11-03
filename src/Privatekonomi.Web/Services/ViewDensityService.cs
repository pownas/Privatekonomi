namespace Privatekonomi.Web.Services;

/// <summary>
/// Service for managing view density (compact vs. spacious) preference
/// </summary>
public class ViewDensityService
{
    public event Action? OnViewDensityChanged;
    
    private ViewDensity _viewDensity = ViewDensity.Spacious;
    
    /// <summary>
    /// Gets or sets the current view density mode
    /// </summary>
    public ViewDensity ViewDensity
    {
        get => _viewDensity;
        set
        {
            if (_viewDensity != value)
            {
                _viewDensity = value;
                OnViewDensityChanged?.Invoke();
            }
        }
    }
    
    /// <summary>
    /// Gets whether the current mode is compact
    /// </summary>
    public bool IsCompact => _viewDensity == ViewDensity.Compact;
    
    /// <summary>
    /// Gets whether the current mode is spacious
    /// </summary>
    public bool IsSpacious => _viewDensity == ViewDensity.Spacious;
    
    /// <summary>
    /// Toggles between compact and spacious modes
    /// </summary>
    public void ToggleViewDensity()
    {
        ViewDensity = _viewDensity == ViewDensity.Compact 
            ? ViewDensity.Spacious 
            : ViewDensity.Compact;
    }
}

/// <summary>
/// Enum representing view density modes
/// </summary>
public enum ViewDensity
{
    /// <summary>
    /// Compact mode with less padding and margins for higher content density
    /// </summary>
    Compact,
    
    /// <summary>
    /// Spacious mode with more padding and margins for better readability
    /// </summary>
    Spacious
}
