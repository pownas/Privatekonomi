# Cost Periodization Feature - Implementation Summary

## Overview
This feature allows users to enter budget costs with different recurrence periods (bi-monthly, quarterly, semi-annual, or annual) and automatically calculates the monthly cost for better budget planning.

## UI Changes

### Budget Creation/Edit Form

**Before:**
- Single amount field per category labeled "Kategorinamn"
- All costs assumed to be monthly

**After:**
- Period selection dropdown (5 options):
  - Månad (1 month) - default
  - Varannan månad (2 months)
  - Kvartal - 3 mån (3 months)
  - Halvår - 6 mån (6 months)
  - Helår - 12 mån (12 months)
- Amount field label changes based on period:
  - "Belopp/månad" for monthly
  - "Belopp/varannan månad" for bi-monthly
  - "Belopp/kvartal" for quarterly
  - "Belopp/halvår" for semi-annual
  - "Belopp/år" for annual
- Green alert showing calculated monthly cost for periodized items

**Example Budget Entry:**
```
┌─────────────────────────────────────────────────────────┐
│ Kategori: Gym (orange chip)                             │
│ ┌───────────────┐  ┌──────────────────────┐             │
│ │ Period        │  │ Belopp/år            │             │
│ │ Helår (12 mån)│  │ 1 800 kr             │             │
│ └───────────────┘  └──────────────────────┘             │
│ ✓ Månadskostnad: 150,00 kr                              │
└─────────────────────────────────────────────────────────┘
```

### Budget Display Table

**New Columns:**
1. **Kategori** - Category name with color chip
2. **Period** - Shows period type (with blue chip for periodized costs)
3. **Periodbelopp** - Total cost for the period (only shown for periodized items)
4. **Månadskostnad** - Calculated monthly cost (in bold)
5. **Faktiskt** - Actual spending
6. **Differens** - Difference (budget vs actual)
7. **Progress** - Visual progress bar

**Example Display:**
```
┌──────────┬────────────────┬──────────────┬──────────────┬──────────┬───────────┬──────────┐
│ Kategori │ Period         │ Periodbelopp │ Månadskostnad│ Faktiskt │ Differens │ Progress │
├──────────┼────────────────┼──────────────┼──────────────┼──────────┼───────────┼──────────┤
│ Mat      │ Månad          │ -            │ 3 000 kr     │ 2 850 kr │ +150 kr   │ ████ 95% │
│ Gym      │ Helår (blue)   │ 1 800 kr     │ 150 kr       │ 150 kr   │ 0 kr      │ ████ 100%│
│ Danskurs │ Halvår (blue)  │ 850 kr       │ 141,67 kr    │ 141,67kr │ 0 kr      │ ████ 100%│
└──────────┴────────────────┴──────────────┴──────────────┴──────────┴───────────┴──────────┘
```

### Summary Section

**Before:**
- Total Planerad (total planned)
- Shows sum of all PlannedAmount values

**After:**
- Total Månadskostnad (total monthly cost)
- Shows sum of all MonthlyAmount values (calculated from periodized costs)
- This gives a more accurate monthly budget total

## Technical Implementation

### Data Model
```csharp
public class BudgetCategory
{
    public int BudgetCategoryId { get; set; }
    public int BudgetId { get; set; }
    public int CategoryId { get; set; }
    public decimal PlannedAmount { get; set; }
    
    // New property
    public int RecurrencePeriodMonths { get; set; } = 1;
    
    // Computed property (not stored in DB)
    public decimal MonthlyAmount => RecurrencePeriodMonths > 0 
        ? PlannedAmount / RecurrencePeriodMonths 
        : PlannedAmount;
}
```

### Calculation Logic
```
Monthly Cost = Period Amount ÷ Number of Months in Period

Examples:
- 1,800 kr/year ÷ 12 = 150 kr/month
- 850 kr/semi-annual ÷ 6 = 141.67 kr/month
- 1,300 kr/bi-monthly ÷ 2 = 650 kr/month
```

## User Flow

1. User creates/edits a budget
2. For each category:
   - Select period (default: Månad)
   - Enter amount for that period
   - System shows calculated monthly cost if period > 1 month
3. Save budget
4. View budget shows:
   - Period type (with visual indicator)
   - Period amount (if applicable)
   - Monthly cost (in bold)
   - Summary uses monthly costs for total

## Benefits

1. **Clearer Monthly Planning**: All costs shown as monthly amounts
2. **Accurate Budgeting**: Annual costs distributed evenly across months
3. **Visual Clarity**: Periodized costs clearly marked with colored chips
4. **Flexibility**: Support for 5 common period types
5. **Automatic Calculation**: No manual division needed

## Examples from Requirements

All examples from the original issue are implemented:

### Example 1: Gymkort (Annual)
- Input: 1,800 kr/år
- Output: 150 kr/månad
- ✅ Implemented

### Example 2: Danskurs (Semi-annual)
- Input: 850 kr/halvår
- Output: 141.67 kr/månad
- ✅ Implemented

### Example 3: Avfall och Vatten (Bi-monthly)
- Input: 1,300 kr/varannan månad
- Output: 650 kr/månad
- ✅ Implemented

### Example 4: Fritidsaktivitet (Annual)
- Input: 2,400 kr/år
- Output: 200 kr/månad
- ✅ Implemented

## Files Changed

1. `src/Privatekonomi.Core/Models/BudgetCategory.cs` - Added periodization properties
2. `src/Privatekonomi.Core/Data/PrivatekonomyContext.cs` - Database configuration
3. `src/Privatekonomi.Web/Components/Pages/Budgets.razor` - UI enhancements
4. `tests/Privatekonomi.Core.Tests/BudgetCategoryTests.cs` - Unit tests
5. `docs/COST_PERIODIZATION.md` - User documentation

Total: 368 lines added, 26 lines modified

## Test Coverage

8 comprehensive unit tests:
- ✅ Monthly period (1 month)
- ✅ Bi-monthly period (2 months)
- ✅ Quarterly period (3 months)
- ✅ Semi-annual period (6 months)
- ✅ Annual period (12 months)
- ✅ Examples from issue (Gymkort, Danskurs)
- ✅ Edge cases (zero period)
- ✅ Default value validation

All tests passing! 🎉
