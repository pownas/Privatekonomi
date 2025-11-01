using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for managing customizable dashboard layouts
/// </summary>
public class DashboardLayoutService : IDashboardLayoutService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;

    public DashboardLayoutService(PrivatekonomyContext context, ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<DashboardLayout>> GetUserLayoutsAsync(string userId)
    {
        return await _context.DashboardLayouts
            .Include(l => l.Widgets)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.IsDefault)
            .ThenBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<DashboardLayout?> GetDefaultLayoutAsync(string userId)
    {
        var defaultLayout = await _context.DashboardLayouts
            .Include(l => l.Widgets)
            .Where(l => l.UserId == userId && l.IsDefault)
            .FirstOrDefaultAsync();

        // If no default layout exists, create one
        if (defaultLayout == null)
        {
            defaultLayout = await CreateDefaultLayoutForUserAsync(userId);
        }

        return defaultLayout;
    }

    public async Task<DashboardLayout?> GetLayoutByIdAsync(int layoutId)
    {
        return await _context.DashboardLayouts
            .Include(l => l.Widgets)
            .FirstOrDefaultAsync(l => l.LayoutId == layoutId);
    }

    public async Task<DashboardLayout> CreateLayoutAsync(DashboardLayout layout)
    {
        layout.CreatedAt = DateTime.UtcNow;
        layout.UpdatedAt = DateTime.UtcNow;

        // Set user ID for new layouts
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            layout.UserId = _currentUserService.UserId;
        }

        // If this is set as default, unset other defaults
        if (layout.IsDefault)
        {
            await UnsetOtherDefaultsAsync(layout.UserId);
        }

        _context.DashboardLayouts.Add(layout);
        await _context.SaveChangesAsync();
        return layout;
    }

    public async Task<DashboardLayout> UpdateLayoutAsync(DashboardLayout layout)
    {
        layout.UpdatedAt = DateTime.UtcNow;

        // If setting as default, unset other defaults
        if (layout.IsDefault)
        {
            await UnsetOtherDefaultsAsync(layout.UserId);
        }

        _context.Entry(layout).State = EntityState.Modified;
        
        // Handle widget updates
        var existingWidgets = await _context.WidgetConfigurations
            .Where(w => w.LayoutId == layout.LayoutId)
            .ToListAsync();

        // Remove widgets that are no longer in the layout
        var widgetsToRemove = existingWidgets
            .Where(ew => !layout.Widgets.Any(w => w.WidgetConfigId == ew.WidgetConfigId))
            .ToList();
        _context.WidgetConfigurations.RemoveRange(widgetsToRemove);

        // Add or update widgets
        foreach (var widget in layout.Widgets)
        {
            if (widget.WidgetConfigId == 0)
            {
                _context.WidgetConfigurations.Add(widget);
            }
            else
            {
                _context.Entry(widget).State = EntityState.Modified;
            }
        }

        await _context.SaveChangesAsync();
        return layout;
    }

    public async Task DeleteLayoutAsync(int layoutId)
    {
        var layout = await _context.DashboardLayouts
            .Include(l => l.Widgets)
            .FirstOrDefaultAsync(l => l.LayoutId == layoutId);

        if (layout != null)
        {
            // If deleting default layout, set another as default
            if (layout.IsDefault)
            {
                var nextLayout = await _context.DashboardLayouts
                    .Where(l => l.UserId == layout.UserId && l.LayoutId != layoutId)
                    .FirstOrDefaultAsync();

                if (nextLayout != null)
                {
                    nextLayout.IsDefault = true;
                    _context.Entry(nextLayout).State = EntityState.Modified;
                }
            }

            _context.DashboardLayouts.Remove(layout);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SetDefaultLayoutAsync(int layoutId, string userId)
    {
        // Unset all defaults for user
        await UnsetOtherDefaultsAsync(userId);

        // Set the specified layout as default
        var layout = await _context.DashboardLayouts
            .FirstOrDefaultAsync(l => l.LayoutId == layoutId && l.UserId == userId);

        if (layout != null)
        {
            layout.IsDefault = true;
            layout.UpdatedAt = DateTime.UtcNow;
            _context.Entry(layout).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<DashboardLayout> CreateDefaultLayoutForUserAsync(string userId)
    {
        var defaultLayout = new DashboardLayout
        {
            UserId = userId,
            Name = "Hem",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Widgets = new List<WidgetConfiguration>
            {
                // Row 0: Net Worth widget (full width)
                new WidgetConfiguration
                {
                    Type = WidgetType.NetWorth,
                    Row = 0,
                    Column = 0,
                    Width = 12,
                    Height = 2
                },
                // Row 1: Period Comparison (full width)
                new WidgetConfiguration
                {
                    Type = WidgetType.PeriodComparison,
                    Row = 1,
                    Column = 0,
                    Width = 12,
                    Height = 1
                },
                // Row 2: Cash Flow (left half)
                new WidgetConfiguration
                {
                    Type = WidgetType.CashFlow,
                    Row = 2,
                    Column = 0,
                    Width = 6,
                    Height = 2
                },
                // Row 2: Budget Overview (right half)
                new WidgetConfiguration
                {
                    Type = WidgetType.BudgetOverview,
                    Row = 2,
                    Column = 6,
                    Width = 6,
                    Height = 2
                },
                // Row 3: Recent Transactions (full width)
                new WidgetConfiguration
                {
                    Type = WidgetType.RecentTransactions,
                    Row = 3,
                    Column = 0,
                    Width = 12,
                    Height = 2
                }
            }
        };

        _context.DashboardLayouts.Add(defaultLayout);
        await _context.SaveChangesAsync();
        return defaultLayout;
    }

    private async Task UnsetOtherDefaultsAsync(string userId)
    {
        var otherDefaults = await _context.DashboardLayouts
            .Where(l => l.UserId == userId && l.IsDefault)
            .ToListAsync();

        foreach (var layout in otherDefaults)
        {
            layout.IsDefault = false;
            _context.Entry(layout).State = EntityState.Modified;
        }
    }
}
