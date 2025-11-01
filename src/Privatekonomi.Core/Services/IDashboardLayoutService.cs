using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for managing customizable dashboard layouts
/// </summary>
public interface IDashboardLayoutService
{
    /// <summary>
    /// Gets all dashboard layouts for the current user
    /// </summary>
    Task<IEnumerable<DashboardLayout>> GetUserLayoutsAsync(string userId);
    
    /// <summary>
    /// Gets the default layout for the current user
    /// </summary>
    Task<DashboardLayout?> GetDefaultLayoutAsync(string userId);
    
    /// <summary>
    /// Gets a specific layout by ID
    /// </summary>
    Task<DashboardLayout?> GetLayoutByIdAsync(int layoutId);
    
    /// <summary>
    /// Creates a new dashboard layout
    /// </summary>
    Task<DashboardLayout> CreateLayoutAsync(DashboardLayout layout);
    
    /// <summary>
    /// Updates an existing dashboard layout
    /// </summary>
    Task<DashboardLayout> UpdateLayoutAsync(DashboardLayout layout);
    
    /// <summary>
    /// Deletes a dashboard layout
    /// </summary>
    Task DeleteLayoutAsync(int layoutId);
    
    /// <summary>
    /// Sets a layout as the default for a user
    /// </summary>
    Task SetDefaultLayoutAsync(int layoutId, string userId);
    
    /// <summary>
    /// Creates a default layout for a new user
    /// </summary>
    Task<DashboardLayout> CreateDefaultLayoutForUserAsync(string userId);
}
