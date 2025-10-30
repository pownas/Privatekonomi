# Net Worth Widget - Visual Examples

## Desktop Layout (Full Screen)

### Example 1: Positive Net Worth with Upward Trend

```
╔═══════════════════════════════════════════════════════════════════════════╗
║ Nettoförmögenhet                                                          ║
╠═════════════════════════════════╦═════════════════════════════════════════╣
║                                 ║                                         ║
║   1 234 567 kr                 ║    Nettoförmögenhet                    ║
║   (large, green text)           ║                                         ║
║                                 ║    1.4M ┤                     ●         ║
║   +5.23% ↑                     ║        │                 ●   ╱          ║
║   (small, green text with icon) ║    1.2M ┤             ●   ╱            ║
║                                 ║        │         ●   ╱                  ║
║                                 ║    1.0M ┤     ●   ╱                     ║
║   Tillgångar: 2 000 000 kr     ║        │ ●   ╱                          ║
║   (green text)                  ║    800k ┼─────────────────────────      ║
║                                 ║        └──┬──┬──┬──┬──┬──┬──┬──┬──┬──  ║
║   Skulder: 765 433 kr          ║          jan feb mar apr maj jun jul   ║
║   (red text)                    ║          2024                           ║
║                                 ║                                         ║
╚═════════════════════════════════╩═════════════════════════════════════════╝
```

### Example 2: Negative Net Worth with Downward Trend

```
╔═══════════════════════════════════════════════════════════════════════════╗
║ Nettoförmögenhet                                                          ║
╠═════════════════════════════════╦═════════════════════════════════════════╣
║                                 ║                                         ║
║   -45 890 kr                   ║    Nettoförmögenhet                    ║
║   (large, red text)             ║                                         ║
║                                 ║      0  ┤                               ║
║   -12.50% ↓                    ║        │ ●                              ║
║   (small, red text with icon)   ║   -20k ┤   ╲                            ║
║                                 ║        │     ╲   ●                      ║
║                                 ║   -40k ┤       ╲   ╲                    ║
║   Tillgångar: 500 000 kr       ║        │         ╲   ●   ●              ║
║   (green text)                  ║   -60k ┼─────────────────────────      ║
║                                 ║        └──┬──┬──┬──┬──┬──┬──┬──┬──┬──  ║
║   Skulder: 545 890 kr          ║          jan feb mar apr maj jun jul   ║
║   (red text)                    ║          2024                           ║
║                                 ║                                         ║
╚═════════════════════════════════╩═════════════════════════════════════════╝
```

### Example 3: Stable Net Worth (No Change)

```
╔═══════════════════════════════════════════════════════════════════════════╗
║ Nettoförmögenhet                                                          ║
╠═════════════════════════════════╦═════════════════════════════════════════╣
║                                 ║                                         ║
║   850 000 kr                   ║    Nettoförmögenhet                    ║
║   (large, green text)           ║                                         ║
║                                 ║    900k ┤                               ║
║   0% −                         ║        │                               ║
║   (small, default text)         ║    850k ┤●──●──●──●──●──●──●──●──●──●  ║
║                                 ║        │                               ║
║                                 ║    800k ┤                               ║
║   Tillgångar: 1 500 000 kr     ║        │                               ║
║   (green text)                  ║    750k ┼─────────────────────────      ║
║                                 ║        └──┬──┬──┬──┬──┬──┬──┬──┬──┬──  ║
║   Skulder: 650 000 kr          ║          jan feb mar apr maj jun jul   ║
║   (red text)                    ║          2024                           ║
║                                 ║                                         ║
╚═════════════════════════════════╩═════════════════════════════════════════╝
```

## Mobile Layout (Stacked)

### Example: Positive Net Worth

```
╔══════════════════════════════════╗
║ Nettoförmögenhet                 ║
╠══════════════════════════════════╣
║                                  ║
║   1 234 567 kr                  ║
║   (large, green text)            ║
║                                  ║
║   +5.23% ↑                      ║
║   (small, green text with icon)  ║
║                                  ║
║                                  ║
║   Tillgångar: 2 000 000 kr      ║
║   (green text)                   ║
║                                  ║
║   Skulder: 765 433 kr           ║
║   (red text)                     ║
║                                  ║
╠══════════════════════════════════╣
║                                  ║
║  Nettoförmögenhet               ║
║                                  ║
║  1.4M ┤                     ●    ║
║       │                 ●   ╱    ║
║  1.2M ┤             ●   ╱        ║
║       │         ●   ╱            ║
║  1.0M ┤     ●   ╱                ║
║       │ ●   ╱                    ║
║  800k ┼──────────────────        ║
║       └┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─     ║
║        j f m a m j j a s o      ║
║        2024                      ║
║                                  ║
╚══════════════════════════════════╝
```

## Color Key

```
┌─────────────────────────────────────────┐
│ Net Worth Value Colors:                 │
├─────────────────────────────────────────┤
│ ▓▓ Green   - Positive or zero (≥ 0)    │
│ ▒▒ Red     - Negative (< 0)             │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ Percentage Change Colors & Icons:       │
├─────────────────────────────────────────┤
│ ▓▓ Green ↑ - Increase (> 0)            │
│ ▒▒ Red ↓   - Decrease (< 0)            │
│ ░░ Gray −  - No change (= 0)            │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ Breakdown Colors:                        │
├─────────────────────────────────────────┤
│ ▓▓ Green   - Tillgångar (Assets)       │
│ ▒▒ Red     - Skulder (Liabilities)      │
└─────────────────────────────────────────┘
```

## Chart Details

### Line Chart Legend
```
●   Data point (monthly net worth value)
─   Line connecting data points (showing trend)
│   Y-axis (net worth in SEK)
┤   Y-axis tick mark
└   X-axis start
┬   X-axis month marker
```

### X-Axis Labels (Swedish Months)
```
Full month format:  jan feb mar apr maj jun jul aug sep okt nov dec
With year:          jan 2024, feb 2024, mar 2024, etc.
```

### Y-Axis Formatting
```
Format:   Currency (SEK) with thousands separator
Examples: 
  1 500 000 kr  → 1.5M
  800 000 kr    → 800k
  -45 000 kr    → -45k
  0 kr          → 0
```

## Widget States

### State 1: Loading (Initial)
```
╔═══════════════════════════════════╗
║ Nettoförmögenhet                  ║
╠═══════════════════════════════════╣
║                                   ║
║   [Loading spinner/skeleton]      ║
║                                   ║
╚═══════════════════════════════════╝
```

### State 2: No Historical Data
```
╔═══════════════════════════════════════════════════╗
║ Nettoförmögenhet                                  ║
╠═════════════════════╦═════════════════════════════╣
║                     ║                             ║
║  1 000 000 kr      ║  Ingen historisk data      ║
║  0% −              ║  tillgänglig ännu.         ║
║                     ║                             ║
║  Tillgångar: 1.2M  ║  (secondary color text)     ║
║  Skulder: 200k     ║                             ║
║                     ║                             ║
╚═════════════════════╩═════════════════════════════╝
```

### State 3: With Complete Data (Normal)
```
╔═══════════════════════════════════════════════════════════╗
║ Nettoförmögenhet                                          ║
╠═════════════════════╦═════════════════════════════════════╣
║                     ║                                     ║
║  1 234 567 kr      ║  [12-month line chart with data]   ║
║  +5.23% ↑          ║  ● ● ● ● ● ● ● ● ● ● ● ●          ║
║                     ║                                     ║
║  Tillgångar: 2.0M  ║                                     ║
║  Skulder: 765k     ║                                     ║
║                     ║                                     ║
╚═════════════════════╩═════════════════════════════════════╝
```

## Integration Example: Full Dashboard View

```
╔═══════════════════════════════════════════════════════════════════════╗
║ Dashboard                                [Anpassa] [Ny Transaktion]   ║
╠═══════════════════════════════════════════════════════════════════════╣
║                                                                        ║
║ ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐         ║
║ │ Totala     │ │ Totala     │ │ Netto-     │ │ Antal      │         ║
║ │ Inkomster  │ │ Utgifter   │ │ resultat   │ │ Trans-     │         ║
║ │ 45 000 kr  │ │ 32 000 kr  │ │ 13 000 kr  │ │ aktioner   │         ║
║ │            │ │            │ │            │ │ 156        │         ║
║ └────────────┘ └────────────┘ └────────────┘ └────────────┘         ║
║                                                                        ║
║ ╔═══════════════════════════════════════════════════════════════════╗ ║
║ ║ Nettoförmögenhet                                                  ║ ║
║ ╠════════════════════════╦══════════════════════════════════════════╣ ║
║ ║                        ║                                          ║ ║
║ ║  1 234 567 kr         ║  [12-month trend chart]                 ║ ║
║ ║  +5.23% ↑             ║  ● ● ● ● ● ● ● ● ● ● ● ●               ║ ║
║ ║                        ║                                          ║ ║
║ ║  Tillgångar: 2.0M     ║                                          ║ ║
║ ║  Skulder: 765k        ║                                          ║ ║
║ ║                        ║                                          ║ ║
║ ╚════════════════════════╩══════════════════════════════════════════╝ ║
║                                                                        ║
║ [Tidsperiod: 6 månader | 12 månader | 24 månader]                    ║
║                                                                        ║
║ [Additional dashboard widgets below...]                               ║
║                                                                        ║
╚═══════════════════════════════════════════════════════════════════════╝
```

## Summary

The Net Worth widget provides a comprehensive view of financial health with:
- **Prominent display** of current net worth value
- **Visual indicators** (colors and icons) for quick understanding
- **Detailed breakdown** of assets vs liabilities
- **Historical context** via 12-month trend chart
- **Responsive design** adapting to different screen sizes

All text is in Swedish, following the application's localization standards, and all monetary values use Swedish currency formatting (space-separated thousands with "kr" suffix).
