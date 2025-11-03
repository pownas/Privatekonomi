# Net Worth Dashboard Widget - Visual Documentation

## Overview
This document provides visual documentation for the Net Worth widget added to the Dashboard in Issue #3.

## Widget Location
The Net Worth widget is displayed on the main Dashboard (`Home.razor`) page, positioned after the summary cards (Total Income, Total Expenses, Net Result, Transaction Count) and before the chart filters.

## Widget Layout

### Desktop View (≥768px)
```
┌────────────────────────────────────────────────────────────────────┐
│ Nettoförmögenhet                                                   │
├──────────────────────────────┬─────────────────────────────────────┤
│                              │                                     │
│  1 234 567 kr               │   Net Worth Trend (12 months)      │
│  +5.23% ↑                   │                                     │
│                              │   [Line Chart]                      │
│  Tillgångar: 2 000 000 kr   │   Shows historical net worth        │
│  Skulder: 765 433 kr        │   from last 12 months              │
│                              │                                     │
└──────────────────────────────┴─────────────────────────────────────┘
```

### Mobile View (<768px)
```
┌────────────────────────────┐
│ Nettoförmögenhet           │
├────────────────────────────┤
│                            │
│  1 234 567 kr             │
│  +5.23% ↑                 │
│                            │
│  Tillgångar: 2 000 000    │
│  Skulder: 765 433         │
│                            │
├────────────────────────────┤
│                            │
│  Net Worth Trend Chart     │
│  [12-month line chart]     │
│                            │
└────────────────────────────┘
```

## Visual Elements

### 1. Net Worth Value Display
**Element:** Large heading (Typo.h4)
**Content:** Current net worth in Swedish currency format (SEK)
**Color Logic:**
- **Green** (`Color.Success`) - When net worth is positive (≥ 0)
- **Red** (`Color.Error`) - When net worth is negative (< 0)

**Examples:**
- Positive: `1 234 567 kr` (green)
- Negative: `-45 890 kr` (red)
- Zero: `0 kr` (green)

### 2. Percentage Change Indicator
**Element:** Body text (Typo.body2) with icon
**Content:** Month-over-month percentage change with trend icon
**Format:** `+0.00%` or `-0.00%` or `0%`

**Color Logic:**
- **Green** (`Color.Success`) - When change is positive (> 0)
- **Red** (`Color.Error`) - When change is negative (< 0)
- **Default** - When change is zero (= 0)

**Icon Logic:**
- `TrendingUp` (↑) - When change is positive
- `TrendingDown` (↓) - When change is negative
- `Remove` (−) - When change is zero

**Examples:**
- Increase: `+5.23% ↑` (green)
- Decrease: `-2.15% ↓` (red)
- No change: `0% −` (default color)

### 3. Asset and Liability Breakdown
**Display:** Two rows in a grid
**Format:** Swedish currency format (SEK)

**Tillgångar (Assets):**
- Label: "Tillgångar:"
- Color: Green (`Color.Success`)
- Example: `Tillgångar: 2 000 000 kr`

**Skulder (Liabilities):**
- Label: "Skulder:"
- Color: Red (`Color.Error`)
- Example: `Skulder: 765 433 kr`

### 4. Historical Trend Chart
**Chart Type:** Line chart (MudBlazor `ChartType.Line`)
**Data Points:** 12 months of historical net worth data
**Dimensions:** 
- Width: 100% (responsive)
- Height: 250px

**X-Axis:**
- Labels: Swedish month abbreviations + year
- Format: "MMM yyyy" (e.g., "jan 2024", "feb 2024")
- Culture: Swedish ("sv-SE")

**Y-Axis:**
- Format: Currency (C0)
- Increments: Automatic based on data range

**Series:**
- Single line series: "Nettoförmögenhet"
- Color: Default MudBlazor primary color
- Data: Net worth value for each month

**Empty State:**
When no historical data is available:
```
Ingen historisk data tillgänglig ännu.
```
(Displayed in secondary color, body2 typography)

## Color Scheme

### Net Worth Value Colors
| Condition | Color | MudBlazor Color |
|-----------|-------|-----------------|
| Positive (≥ 0) | Green | `Color.Success` |
| Negative (< 0) | Red | `Color.Error` |

### Percentage Change Colors
| Condition | Color | MudBlazor Color | Icon |
|-----------|-------|-----------------|------|
| Increase (> 0) | Green | `Color.Success` | TrendingUp ↑ |
| Decrease (< 0) | Red | `Color.Error` | TrendingDown ↓ |
| No Change (= 0) | Default | `Color.Default` | Remove − |

### Breakdown Colors
| Item | Color | MudBlazor Color |
|------|-------|-----------------|
| Assets | Green | `Color.Success` |
| Liabilities | Red | `Color.Error` |

## Example Scenarios

### Scenario 1: Positive Net Worth with Increase
```
┌────────────────────────────────────┐
│ Nettoförmögenhet                   │
├────────────────────────────────────┤
│                                    │
│ 1 234 567 kr        (Green)       │
│ +5.23% ↑            (Green)       │
│                                    │
│ Tillgångar: 2 000 000 kr (Green)  │
│ Skulder: 765 433 kr      (Red)    │
│                                    │
│ [Upward trending line chart]       │
└────────────────────────────────────┘
```

### Scenario 2: Negative Net Worth with Decrease
```
┌────────────────────────────────────┐
│ Nettoförmögenhet                   │
├────────────────────────────────────┤
│                                    │
│ -45 890 kr          (Red)         │
│ -12.50% ↓           (Red)         │
│                                    │
│ Tillgångar: 500 000 kr  (Green)   │
│ Skulder: 545 890 kr     (Red)     │
│                                    │
│ [Downward trending line chart]     │
└────────────────────────────────────┘
```

### Scenario 3: Positive Net Worth with No Change
```
┌────────────────────────────────────┐
│ Nettoförmögenhet                   │
├────────────────────────────────────┤
│                                    │
│ 850 000 kr          (Green)       │
│ 0% −                (Default)      │
│                                    │
│ Tillgångar: 1 500 000 kr (Green)  │
│ Skulder: 650 000 kr      (Red)    │
│                                    │
│ [Flat line chart]                  │
└────────────────────────────────────┘
```

### Scenario 4: No Historical Data
```
┌────────────────────────────────────┐
│ Nettoförmögenhet                   │
├────────────────────────────────────┤
│                                    │
│ 1 000 000 kr        (Green)       │
│ 0% −                (Default)      │
│                                    │
│ Tillgångar: 1 200 000 kr (Green)  │
│ Skulder: 200 000 kr      (Red)    │
│                                    │
│ Ingen historisk data               │
│ tillgänglig ännu.                  │
└────────────────────────────────────┘
```

## Chart Details

### Historical Trend Line Chart

**Month Labels Example:**
```
jan 2024  feb 2024  mar 2024  apr 2024  maj 2024  jun 2024
jul 2024  aug 2024  sep 2024  okt 2024  nov 2024  dec 2024
```

**Chart Visualization:**
The chart shows a continuous line connecting monthly net worth values, allowing users to:
- See trends over time (increasing, decreasing, or stable)
- Identify significant changes in specific months
- Compare current position with historical performance

**Data Source:**
- Primary: `NetWorthSnapshot` records from database
- Fallback: Current values when historical snapshots are not available

## Integration with Dashboard

The Net Worth widget appears in the following context on the Dashboard:

```
[Dashboard Header with buttons]
    ↓
[Summary Cards Row]
- Total Income | Total Expenses | Net Result | Transaction Count
    ↓
[Net Worth Widget] ← NEW WIDGET
    ↓
[Chart Filters]
- Time period selection (6/12/24 months)
    ↓
[Expense/Income Pie Charts]
    ↓
[Cash Flow Chart]
    ↓
[Category Bar Chart]
    ↓
[Active Budgets]
    ↓
[Unmapped Transactions]
    ↓
[Recent Transactions]
```

## Responsive Behavior

### Desktop (md and larger)
- Left column (4/12): Net worth value and breakdown
- Right column (8/12): Historical trend chart
- Elements side-by-side for efficient use of screen space

### Mobile (xs)
- Full width (12/12) layout
- Value and breakdown stacked at top
- Chart displayed below in full width
- Maintains readability on smaller screens

## Currency Formatting

All monetary values use Swedish locale formatting:
- **Format:** `"C0"` (currency with 0 decimal places)
- **Culture:** `new CultureInfo("sv-SE")`
- **Examples:**
  - 1234567 → "1 234 567 kr"
  - -45890 → "-45 890 kr"
  - 0 → "0 kr"

## Accessibility

- **Semantic HTML:** Uses MudBlazor card components with proper structure
- **Color Coding:** Supplemented with icons for better accessibility
- **Text Labels:** Clear Swedish labels for all elements
- **Chart:** MudBlazor chart component with built-in accessibility features

## Technical Implementation Reference

**File:** `src/Privatekonomi.Web/Components/Pages/Home.razor`
**Lines:** Approximately 100-160 (Net Worth widget section)

**Key Methods:**
- `LoadNetWorthData()` - Fetches and prepares data
- `GetNetWorthColor()` - Determines net worth value color
- `GetChangeColor()` - Determines percentage change color
- `GetChangeIcon()` - Selects appropriate trend icon

**Data Binding:**
- `_netWorthReport` - Main data object
- `_netWorthSeries` - Chart data series
- `_netWorthLabels` - Chart X-axis labels

## Summary

The Net Worth widget provides users with an at-a-glance view of their financial position:
- **Current value** with clear visual indicators
- **Trend direction** showing if finances are improving or declining
- **Asset/Liability breakdown** for context
- **12-month historical chart** for long-term perspective

The implementation follows MudBlazor design patterns and Swedish localization standards, integrating seamlessly with the existing Dashboard layout.
