# BAS 2026 Upgrade Summary

## Overview

Privatekonomi has been updated to reference BAS 2026 (baskontoplanen för 2026) instead of BAS 2025. This document summarizes the changes made and explains the impact on the application.

## Background

BAS (Bas Account Schema) is the Swedish standard chart of accounts used primarily for business accounting. BAS releases updates annually, and BAS 2026 was released in December 2025 with approximately 255 changes compared to BAS 2025.

## BAS 2026 Changes

The main changes in BAS 2026 are focused on business accounting:

### Business-Specific Changes (Not applicable to Privatekonomi)
- **Kontoklass 4 restructuring**: Better separation between "Handelsvaror" (trade goods) and "Råvaror och förnödenheter" (raw materials and supplies)
- **New VAT accounts**: For reverse charge VAT (omvänd moms) - accounts 2614, 2624, 2634
- **New shareholder loan accounts**: 1360, 1369, 1685 for loans to/from shareholders and related parties
- **Periodization funds**: New accounts 2126, 2127, 2136, 2137 for tax year-specific periodization
- **SCB reporting**: Many new accounts to facilitate reporting to Statistics Sweden (SCB)
- **Updated accounting rules**: Following Bokföringsnämnden (BFN) updates

### Impact on Privatekonomi

**No structural changes required** for Privatekonomi's personal finance categories because:

1. **Privatekonomi uses simplified ranges**: The application uses accounts in the 3000-8999 range:
   - 3000-3999: Income (Intäkter)
   - 4000-4999: Housing (Boende) - simplified for personal use
   - 5000-5999: Food & Consumption (Mat & Förbrukning)
   - 6000-6999: Transport & Other (Transport & Övrigt)
   - 7000-7999: Entertainment & Health (Nöje & Hälsa)
   - 8000-8999: Savings (Sparande)

2. **Business-focused changes don't apply**: The BAS 2026 changes are primarily in:
   - Kontoklass 1-2 (assets, equity, liabilities) - not used directly in Privatekonomi
   - Kontoklass 4 (business purchases) - simplified in Privatekonomi for housing costs
   - VAT and tax accounts - not relevant for personal finance

3. **Core structure remains**: The logic and structure of personal finance categories remain unchanged

## Changes Made in Privatekonomi

### Documentation Updates

1. **Renamed main documentation file**:
   - `docs/KONTOPLAN_BAS_2025.md` → `docs/KONTOPLAN_BAS_2026.md`

2. **Updated references** in:
   - `README.md`: Changed "BAS 2025" → "BAS 2026"
   - `docs/KONTOPLAN_BAS_2026.md`: Updated title and content
   - `docs/KONTOPLAN_SNABBREFERENS.md`: Updated quick reference
   - `docs/IMPLEMENTATION_SUMMARY_KONTOPLAN.md`: Updated implementation summary

3. **Added BAS 2026 context**:
   - Explained which changes in BAS 2026 are relevant vs. not relevant
   - Added references to official BAS 2026 documentation
   - Clarified that the personal finance structure is unaffected

### Code Updates

1. **Model comment** (`src/Privatekonomi.Core/Models/Category.cs`):
   - Updated comment to reference "BAS 2026-inspired"

2. **Database context** (`src/Privatekonomi.Core/Data/PrivatekonomyContext.cs`):
   - Updated seeding comments to reference BAS 2026
   - Added note about BAS 2026 changes being business-specific

### No Changes to Account Numbers

All existing account numbers remain the same:
- Income: 3000, 3010, 3020
- Housing: 4000, 4100, 4200, 4300, 4400
- Food: 5000, 5100, 5200, 5300
- Shopping: 5500, 5510, 5520, 5550
- Transport: 6000, 6100, 6200, 6500
- Entertainment: 7000, 7100, 7300, 7400
- Health: 7500, 7510, 7520, 7530
- Savings: 8000, 8100, 8200, 8300, 8400
- Other: 6900

## Testing

All tests pass successfully:
- ✅ **671 unit tests passed** (Core and API tests)
- ✅ **3 tests skipped** (known issues, not related to this change)
- ✅ **Build successful** (0 errors, 17 pre-existing warnings)
- ✅ **No breaking changes**

## User Impact

**Users will experience no functional changes**. This update:
- ✅ Maintains full backward compatibility
- ✅ Keeps all account numbers the same
- ✅ Updates documentation to be current with the latest BAS standard
- ✅ Adds clarity about which BAS changes apply to personal finance

## Future Considerations

While BAS 2026 doesn't require changes now, future BAS updates should be reviewed to determine if:
1. New personal finance-relevant accounts are added
2. Income or expense account ranges are restructured
3. New features could benefit personal finance users

## References

- [BAS 2026: Ändringar i kontoplanen](https://www.bas.se/2025/12/04/andringar-i-kontoplanen-2026/)
- [BAS: Jämför kontoplaner](https://www.bas.se/kontoplaner/jamfor-kontoplaner/)
- [Privatekonomi: Kontoplan BAS 2026](KONTOPLAN_BAS_2026.md)
- [Privatekonomi: Snabbreferens](KONTOPLAN_SNABBREFERENS.md)

## Conclusion

The update to BAS 2026 is primarily a documentation update to ensure Privatekonomi references the latest Swedish accounting standard. The personal finance category structure remains unchanged, and all functionality continues to work exactly as before.

**Date**: December 2025  
**Version**: Updated to BAS 2026 reference  
**Status**: Completed and tested
