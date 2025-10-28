# EditTransactionDialog Documentation

## Quick Links

- 📖 [User Guide](EDIT_TRANSACTION_DIALOG_GUIDE.md) - Complete feature documentation
- ⌨️ [Keyboard Navigation Guide](KEYBOARD_NAVIGATION_GUIDE.md) - Shortcuts and accessibility
- 📊 [Change Summary](EDIT_TRANSACTION_CHANGES.md) - Before/after comparison
- 🎨 [Visual Improvements](VISUAL_IMPROVEMENTS_SUMMARY.md) - UI improvements with diagrams

## What's New in EditTransactionDialog v2.0

### 🆕 New Fields
1. **IsIncome Toggle** - Easily switch between Income and Expense
2. **Payee Field** - Record who received or sent the payment
3. **Payment Method** - Select from common Swedish payment methods
4. **Currency Field** - Default SEK, but customizable for foreign transactions

### ✨ Enhanced Features
1. **Smart Category Search** - MudAutocomplete with real-time filtering
2. **Category Hierarchy** - See parent categories in search results
3. **Form Validation** - Real-time validation with helpful error messages
4. **Loading States** - Visual feedback during save operations
5. **Accessibility** - Full keyboard navigation and ARIA support

### 🎯 Key Improvements
- ⚡ Faster category selection with autocomplete
- ✅ Clear validation feedback before saving
- 🔍 Better visibility of category relationships
- ♿ WCAG 2.1 compliant accessibility
- 📱 Improved mobile experience
- 💾 Loading indicators for better UX

## For Users

### Getting Started
1. Click "Edit" button on any transaction in the transaction list
2. The EditTransactionDialog opens with all transaction details
3. Modify any fields as needed
4. Use autocomplete to quickly find categories
5. For split transactions, click "Dela på flera kategorier"
6. Validation happens in real-time
7. Click "Spara" to save changes

### Quick Tips
- 🔍 Start typing in category field to filter options
- ⌨️ Use Tab to navigate between fields quickly
- ✅ Required fields are marked with *
- ⚠️ Validation warnings appear immediately
- 💡 Helper text is shown below each field

### Common Tasks

#### Edit Basic Information
1. Open the dialog
2. Change description, date, or amount
3. Click Save
4. Done! ✅

#### Change Category
1. Open the dialog
2. Click in the category autocomplete field
3. Start typing to search (e.g., "mat")
4. Press Enter or click to select
5. Click Save

#### Split Transaction Across Categories
1. Open the dialog
2. Select "Dela på flera kategorier"
3. Choose "Dela via procent" or "Dela via exakta belopp"
4. For each category:
   - Search and select category
   - Enter percentage or amount
5. Verify total equals 100% or full amount
6. Click Save

## For Developers

### Architecture
- **Component**: Blazor Razor component
- **Validation**: MudBlazor MudForm
- **Category Search**: MudAutocomplete with local filtering
- **Layout**: Responsive MudGrid (xs/md breakpoints)
- **State Management**: Component-level state

### Key Technologies
- MudBlazor 8.13.0
- .NET 9.0
- Blazor Server (InteractiveServer mode)

### Validation Rules
- Description: Required
- Date: Required
- Amount: Required, Min 0.01
- Split Categories (Percentage): Sum must equal 100%
- Split Categories (Amount): Sum must equal transaction amount

### Accessibility
- All fields have aria-label attributes
- Full keyboard navigation support
- Screen reader compatible
- High contrast focus indicators
- Helper text for all inputs

### Responsive Breakpoints
- Desktop: ≥960px (two-column layout)
- Mobile: <960px (single-column layout)

## Documentation Structure

```
docs/
├── README_EDIT_TRANSACTION.md (this file)
├── EDIT_TRANSACTION_DIALOG_GUIDE.md
│   └── Complete user guide with examples
├── KEYBOARD_NAVIGATION_GUIDE.md
│   └── Keyboard shortcuts and accessibility
├── EDIT_TRANSACTION_CHANGES.md
│   └── Detailed before/after comparison
└── VISUAL_IMPROVEMENTS_SUMMARY.md
    └── UI improvements with ASCII diagrams
```

## Version History

### v2.0 (2024-10-28)
- ✅ MudAutocomplete for category search
- ✅ MudForm validation
- ✅ New fields: IsIncome, Payee, PaymentMethod, Currency
- ✅ Comprehensive ARIA labels
- ✅ Loading states
- ✅ Category hierarchy display

### v1.0 (Initial)
- Basic transaction editing
- Split functionality
- Responsive design

## Support

For issues or questions:
1. Check the User Guide for usage help
2. Review the Keyboard Navigation Guide for shortcuts
3. See the Change Summary for implementation details
4. Consult the Visual Improvements Summary for UI reference

## Related Documentation

- [Transaction Model](../src/Privatekonomi.Core/Models/Transaction.cs)
- [Category Model](../src/Privatekonomi.Core/Models/Category.cs)
- [Transaction Service](../src/Privatekonomi.Core/Services/ITransactionService.cs)

## Contributing

When making changes to EditTransactionDialog:
1. Maintain accessibility (ARIA labels, keyboard nav)
2. Keep validation comprehensive
3. Preserve responsive design
4. Update documentation
5. Test on multiple screen sizes
6. Verify keyboard navigation still works

## License

Part of the Privatekonomi project. See main LICENSE file for details.
