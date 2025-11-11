# Implementation Summary: Mål och sparande - Hantering av mål, prioriteringar och simuleringar

## Overview
This implementation adds comprehensive savings goal management features to the Privatekonomi application, including:
1. Auto-savings configuration (fixed amount or round-up)
2. Savings simulation to visualize impact of different monthly savings amounts
3. Priority management with up/down controls
4. Enhanced goal creation and editing forms

## Features Implemented

### 1. Auto-Savings Configuration

#### Model Changes
- Added `AutoSavingsType` enum to `Goal.cs`:
  - `None`: No automatic savings
  - `FixedAmount`: Fixed monthly amount
  - `RoundUp`: Transaction round-up savings
- Added `MonthlyAutoSavingsAmount` decimal field to track fixed monthly savings

#### Database Migration
- Created migration `20251111194000_AddAutoSavingsToGoal.cs`
- Adds two new columns to Goals table:
  - `AutoSavingsType` (INTEGER, default 0)
  - `MonthlyAutoSavingsAmount` (TEXT/decimal, default 0)

### 2. Savings Simulation

#### Service Layer
Enhanced `IGoalService` and `GoalService` with simulation methods:

```csharp
// Calculate when a goal will be completed based on monthly savings
DateTime? CalculateCompletionDate(Goal goal, decimal monthlySavings);

// Calculate number of months to completion
int CalculateMonthsToCompletion(Goal goal, decimal monthlySavings);

// Simulate the impact of changing monthly savings amount
SavingsSimulationResult SimulateSavingsChange(Goal goal, decimal newMonthlySavings);
```

#### SavingsSimulationResult
New DTO class containing:
- Current and new months to completion
- Months difference (positive = earlier, negative = later)
- Current and new completion dates
- Current and new monthly savings amounts
- Remaining amount to save

#### UI Component
Created `SavingsSimulationDialog.razor` with:
- Display of current goal information (target, saved, remaining)
- Current prognosis showing completion date
- Input field for new monthly savings amount
- Real-time calculation of new completion date
- Visual feedback showing time difference:
  - Green background for earlier completion
  - Orange background for later completion
- "Tillämpa nytt sparande" button to save changes

### 3. Priority Management

#### Service Layer
Added `UpdateGoalPrioritiesAsync` method to batch update priorities

#### UI Enhancements
- Added up/down arrow buttons next to priority chips
- Buttons disabled at limits (priority 1-5)
- Immediate visual feedback with snackbar notifications
- Added drag indicator icon for visual affordance

### 4. Enhanced Goal Form

Updated `Goals.razor` form to include:
- Auto-savings type selector (dropdown)
- Monthly amount field (shown for FixedAmount type)
- Informational alert for RoundUp type with link to settings
- All fields properly bound to Goal model

## Code Changes

### Files Modified
1. `src/Privatekonomi.Core/Models/Goal.cs` - Added auto-savings fields
2. `src/Privatekonomi.Core/Services/IGoalService.cs` - Added simulation methods
3. `src/Privatekonomi.Core/Services/GoalService.cs` - Implemented simulation logic
4. `src/Privatekonomi.Web/Components/Pages/Goals.razor` - Enhanced UI

### Files Created
1. `src/Privatekonomi.Core/Migrations/20251111194000_AddAutoSavingsToGoal.cs` - Database migration
2. `src/Privatekonomi.Web/Components/Dialogs/SavingsSimulationDialog.razor` - Simulation dialog
3. `tests/Privatekonomi.Core.Tests/GoalServiceTests.cs` - Unit tests (9 tests)

## Testing

### Unit Tests
Created comprehensive unit tests for simulation logic:
- ✅ UpdateGoalPrioritiesAsync updates multiple goals
- ✅ CalculateCompletionDate with valid monthly savings
- ✅ CalculateCompletionDate with zero savings returns null
- ✅ CalculateCompletionDate with completed goal returns current date
- ✅ CalculateMonthsToCompletion with valid savings
- ✅ CalculateMonthsToCompletion rounds up partial months
- ✅ SimulateSavingsChange with increased savings shows earlier completion
- ✅ SimulateSavingsChange with decreased savings shows later completion
- ✅ SimulateSavingsChange with zero current savings calculates correctly

**Test Results**: 9/9 tests passing

### Build Status
- Build: ✅ Success
- Warnings: 10 (existing warnings, not related to changes)
- Errors: 0

## User Experience

### Workflow: Create Goal with Auto-Savings
1. User clicks "Nytt Sparmål" button
2. Fills in goal name, description, target amount
3. Selects auto-savings type from dropdown
4. If "Fast belopp per månad" selected, enters monthly amount
5. Clicks "Lägg till" to create goal

### Workflow: Simulate Savings
1. User clicks calculator icon (🧮) on goal row
2. Dialog opens showing current goal status and prognosis
3. User adjusts "Nytt månadsbelopp" field
4. Real-time calculation shows:
   - New completion date
   - Time difference (e.g., "2 månader tidigare")
5. User can apply changes or close dialog

### Workflow: Adjust Priority
1. User clicks up/down arrow buttons on goal row
2. Priority changes immediately
3. Snackbar notification confirms change
4. Goals list re-sorts by new priority

## Swedish Text Examples

- "Simulera Sparande" - Simulate Savings
- "Klart om X månader" - Done in X months
- "X månader tidigare" - X months earlier
- "Nytt månadsbelopp" - New monthly amount
- "Tillämpa nytt sparande" - Apply new savings
- "Automatiskt sparande" - Automatic savings
- "Fast belopp per månad" - Fixed amount per month
- "Avrundning (Round-up)" - Rounding (Round-up)

## Technical Notes

### Calculation Logic
The simulation uses simple linear calculation:
```csharp
monthsToCompletion = Math.Ceiling(remainingAmount / monthlySavings)
```

This assumes:
- Constant monthly savings amount
- No interest or growth (conservative estimate)
- Rounds up to account for partial months

### Priority System
- Priority ranges from 1 (highest) to 5 (lowest)
- Goals are ordered by priority, then by target date
- Each goal can be adjusted independently

### Auto-Savings Integration
- RoundUp type connects to existing RoundUpSavings feature
- FixedAmount allows direct entry of monthly savings
- None type for manual-only savings goals

## Future Enhancements (Not Implemented)

1. **Drag-and-Drop Reordering**: Full drag-drop interface for priorities
2. **Compound Interest**: Factor in savings account interest rates
3. **Variable Contributions**: Support for irregular contribution patterns
4. **Goal Dependencies**: Link goals in priority chain
5. **Historical Tracking**: Track actual vs. planned progress

## Compliance

- ✅ Follows existing code patterns
- ✅ Uses Swedish for UI text
- ✅ Uses English for code and comments
- ✅ Maintains nullable reference types
- ✅ Includes comprehensive unit tests
- ✅ Compatible with existing storage providers
- ✅ Minimal changes to existing functionality
