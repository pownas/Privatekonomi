# Multi-Category Split Feature

## Overview

This feature allows users to split a single transaction across 2-4 categories, distributing the amount either by percentage or exact amounts. This is useful for transactions that span multiple expense categories.

## User Interface

### Mode Selection
The edit transaction dialog now has two modes:
1. **En kategori** (Single category) - Original behavior
2. **Dela på flera kategorier (2-4)** (Split across multiple) - New feature

### Split Methods
When in multi-category mode, users can choose how to split:
1. **Dela via procent** (Split by percentage)
   - Input: Percentage for each category
   - Validation: Must total 100%
   - Auto-calculation: Amounts calculated from percentages

2. **Dela via exakta belopp** (Split by exact amounts)
   - Input: Exact amount for each category
   - Validation: Must total transaction amount
   - Auto-calculation: Percentages calculated from amounts

## Features

### Dynamic Category Management
- **Add categories**: Up to 4 categories total
- **Remove categories**: Delete button on each row (except first)
- **Category selection**: Dropdown with color indicators
- **Real-time validation**: Shows current total and warnings

### Visual Feedback
- **Color indicators**: Each category shows its color as a circle
- **Info box**: Displays running total and validation status
- **Warning messages**: Alerts when totals don't match requirements
- **Disabled save**: Cannot save until validation passes

## Use Cases

### Example 1: Grocery Shopping
**Transaction:** 1000 kr at ICA Maxi

Split by percentage:
- Mat (Food): 60% = 600 kr
- Hushåll (Household): 30% = 300 kr
- Djur (Pets): 10% = 100 kr

### Example 2: Restaurant Bill
**Transaction:** 850 kr at restaurant

Split by exact amounts:
- Mat (Food): 700 kr = 82.35%
- Nöje (Entertainment): 150 kr = 17.65%

### Example 3: Mixed Shopping
**Transaction:** 2500 kr at department store

Split by percentage:
- Kläder (Clothes): 50% = 1250 kr
- Hushåll (Household): 30% = 750 kr
- Presenter (Gifts): 20% = 500 kr

## Technical Implementation

### Data Model
Uses existing `TransactionCategory` model:
```csharp
public class TransactionCategory
{
    public int TransactionCategoryId { get; set; }
    public int TransactionId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; } = 100;
    
    public Transaction Transaction { get; set; }
    public Category Category { get; set; }
}
```

### Key Components

**CategorySplit class** (internal to dialog):
```csharp
private class CategorySplit
{
    public int? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; } = 25;
}
```

**Enums:**
```csharp
private enum SplitMode
{
    Single,     // One category
    Multiple    // 2-4 categories
}

private enum AmountSplitType
{
    Percentage,  // Split by %
    Amount      // Split by exact amounts
}
```

### Validation Logic

**Percentage mode:**
```csharp
var totalPercentage = validSplits.Sum(s => s.Percentage);
if (Math.Abs(totalPercentage - 100) > 0.01m)
{
    Snackbar.Add("Totalen måste vara 100%", Severity.Warning);
    return;
}
```

**Amount mode:**
```csharp
var totalAmount = validSplits.Sum(s => s.Amount);
if (Math.Abs(totalAmount - _transaction.Amount) > 0.01m)
{
    Snackbar.Add("Totalen måste matcha transaktionsbeloppet", Severity.Warning);
    return;
}
```

### Save Logic

**Percentage to Amount conversion:**
```csharp
newCategories = validSplits.Select(s => new TransactionCategory
{
    TransactionId = _transaction.TransactionId,
    CategoryId = s.CategoryId!.Value,
    Amount = _transaction.Amount * (s.Percentage / 100),
    Percentage = s.Percentage
}).ToList();
```

**Amount to Percentage conversion:**
```csharp
newCategories = validSplits.Select(s => new TransactionCategory
{
    TransactionId = _transaction.TransactionId,
    CategoryId = s.CategoryId!.Value,
    Amount = s.Amount,
    Percentage = (s.Amount / _transaction.Amount) * 100
}).ToList();
```

## User Workflow

1. **Open Edit Dialog**
   - Click edit button on any transaction
   - Dialog opens with current transaction data

2. **Select Split Mode**
   - Choose "Dela på flera kategorier (2-4)"
   - Initial splits appear (2 categories with 50/50 default)

3. **Choose Split Method**
   - Select "Dela via procent" or "Dela via exakta belopp"

4. **Configure Categories**
   - Select category from dropdown (with color indicator)
   - Enter percentage or amount
   - Add more categories (up to 4 total)
   - Remove categories if needed

5. **Verify Totals**
   - Check info box for current total
   - Ensure no warning messages
   - Adjust values if needed

6. **Save**
   - Click "Spara" button
   - System validates totals
   - Updates all categories atomically
   - Success message displayed

## Validation Rules

### Category Selection
- Minimum: 1 category must be selected
- Maximum: 4 categories allowed
- Each category must be unique (no duplicates)

### Percentage Mode
- Range: 0% to 100% per category
- Total: Must equal exactly 100%
- Precision: 0.01% tolerance

### Amount Mode
- Range: 0 kr to transaction amount per category
- Total: Must equal transaction amount exactly
- Precision: 0.01 kr tolerance

### General
- At least one valid category required to save
- Empty categories (no category selected) are filtered out
- Both amount and percentage stored for all categories

## Error Handling

### Validation Errors
- **"Välj minst en kategori"** - No categories selected
- **"Totalen måste vara 100%"** - Percentage doesn't sum to 100%
- **"Totalen måste matcha transaktionsbeloppet"** - Amounts don't match total

### UI Warnings
- Warning icon (⚠️) appears in info box when totals are incorrect
- Warning text color to draw attention
- Save button remains enabled but validation prevents saving

### Edge Cases
- Rounding handled with 0.01 precision
- Empty splits filtered before validation
- Existing multi-category splits loaded correctly on edit

## Benefits

### For Users
- **Accurate categorization** of mixed transactions
- **Flexible input** - choose percentage or amount
- **Real-time feedback** - see totals as you type
- **Easy management** - add/remove categories dynamically

### For Reporting
- **Better insights** - transactions properly distributed
- **Accurate budgets** - category spending more precise
- **Tax deductions** - proper allocation for tax purposes
- **Trend analysis** - more accurate category trends

## Future Enhancements (Not Implemented)

Potential improvements:
- Preset split templates (e.g., "Grocery Split", "Bill Split")
- Copy split pattern to other transactions
- Bulk split operations
- Smart suggestions based on transaction description
- Graphical pie chart showing split visualization

## Testing Recommendations

### Manual Testing
- [ ] Create new transaction with 2 categories via percentage
- [ ] Create new transaction with 3 categories via amounts
- [ ] Edit existing single-category transaction to multi-category
- [ ] Edit existing multi-category transaction
- [ ] Verify percentages calculate correctly to amounts
- [ ] Verify amounts calculate correctly to percentages
- [ ] Test validation with incorrect totals
- [ ] Test add/remove category buttons
- [ ] Test with maximum 4 categories
- [ ] Verify saved splits display correctly in transaction list

### Edge Cases to Test
- Very small percentages (0.1%)
- Very large amounts (999999.99 kr)
- Rounding scenarios (33.33% x 3)
- Switching between percentage and amount modes
- Canceling edit without saving

## Compatibility

- **Database**: No schema changes required
- **API**: Uses existing endpoints
- **Export**: Multi-category splits included in CSV/JSON exports
- **Reports**: All existing reports work with split categories
- **Budgets**: Categories accumulate split amounts correctly

## Performance

- **Load time**: Minimal impact (loads categories once)
- **Calculation**: Real-time with no noticeable delay
- **Save time**: Single atomic update operation
- **Memory**: Lightweight (only active split data in memory)

## Accessibility

- **Keyboard navigation**: Full support for all controls
- **Screen readers**: Labels and helpers for all inputs
- **Color contrast**: Meets WCAG guidelines
- **Error messages**: Clear and actionable
- **Help text**: Contextual helpers throughout

---

**Implementation Date:** 2025-10-23  
**Commit:** 1734571  
**File:** EditTransactionDialog.razor
