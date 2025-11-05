# Budget Alert Feature - Visual Guide

## Overview
This document demonstrates the real-time budget alert system implementation with screenshots and examples.

## 1. Budget Page - Normal State (No Alerts)

![Budget Page Overview](https://github.com/user-attachments/assets/2f9ce6eb-b204-494c-8bdb-efff51c16217)

**Current State:** The budget page shows the standard view with active and completed budgets. When no budget thresholds are exceeded, the alert section is hidden.

## 2. Budget Alert Display (When Activated)

When a budget reaches 75%, 90%, or 100% of its limit, the alert system activates automatically.

### Alert Section Structure

The budget alert section appears at the top of the Budgets page with the following components:

```
┌─────────────────────────────────────────────────────────────────┐
│ 🚨 Aktiva Budgetvarningar (2)                                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│ ⚠️ BUDGETVARNING: Mat & Dryck                                   │
│                                                                 │
│ Du har använt 6,750 kr av 7,500 kr (90%)                       │
│ ██████████████████████████░░ 90%                                │
│                                                                 │
│ Återstående: 750 kr för 8 dagar                                │
│                                                                 │
│ ⚡ Prognos: Budget överskrids om 4 dagar i nuvarande takt      │
│   (94 kr/dag)                                                  │
│                                                                 │
│ [Visa detaljer] [Justera budget]                     [Stäng X] │
└─────────────────────────────────────────────────────────────────┘
```

### Alert Color Coding

**Green (75% - Safe Zone)**
- Progress bar: Green
- Icon: Information (ℹ️)
- Priority: Normal

**Yellow (75-89% - Warning Zone)**
- Progress bar: Yellow/Warning
- Icon: Warning (⚠️)
- Priority: Normal
- Message: "Du närmar dig budgetgränsen"

**Orange (90-99% - Critical Zone)**
- Progress bar: Orange/Warning
- Icon: Alert (⚠️)
- Priority: High
- Message: "Varning! Budget nästan överskriden"

**Red (100%+ - Exceeded)**
- Progress bar: Red/Error
- Icon: Critical (🚨)
- Priority: Critical
- Message: "VARNING! Budget överskriden"

## 3. Budget Alert Components

### BudgetAlertCard Component

Each alert card displays:

1. **Category Name** - Which budget category triggered the alert
2. **Usage Statistics**:
   - Amount spent vs. planned amount
   - Percentage used
   - Visual progress bar with color coding
3. **Remaining Information**:
   - Amount left in budget
   - Days remaining in budget period
4. **Forecast** (if applicable):
   - Predicted days until budget exceeded
   - Daily spending rate
5. **Action Buttons**:
   - "Visa detaljer" - View detailed transactions
   - "Justera budget" - Adjust budget limits
   - Close/Acknowledge button

### Example Alert Cards

#### 75% Threshold Alert
```
┌─────────────────────────────────────────┐
│ ℹ️ Budgetvarning: Transport             │
│                                         │
│ Använt: 3,750 kr av 5,000 kr (75%)     │
│ ██████████████████░░░░░ 75%             │
│                                         │
│ Återstående: 1,250 kr för 15 dagar     │
└─────────────────────────────────────────┘
```

#### 90% Threshold Alert with Forecast
```
┌─────────────────────────────────────────┐
│ ⚠️ BUDGETVARNING: Mat & Dryck           │
│                                         │
│ Använt: 6,750 kr av 7,500 kr (90%)     │
│ ██████████████████████████░░ 90%        │
│                                         │
│ Återstående: 750 kr för 8 dagar        │
│                                         │
│ ⚡ Prognos: Budget överskrids om 4 dagar│
│   i nuvarande takt (94 kr/dag)         │
└─────────────────────────────────────────┘
```

#### 100% Exceeded Alert
```
┌─────────────────────────────────────────┐
│ 🚨 BUDGETVARNING: Shopping              │
│                                         │
│ Använt: 10,500 kr av 10,000 kr (105%)  │
│ ████████████████████████████ 105%       │
│                                         │
│ Överskridit: 500 kr                    │
│ Dagar kvar: 12 dagar                   │
│                                         │
│ ❄️ Budget freeze aktiverad             │
└─────────────────────────────────────────┘
```

## 4. Real-Time Features

### SignalR Integration

The budget alert system uses SignalR for real-time updates:

1. **Automatic Checks** - Background service runs every 30 minutes
2. **Instant Notifications** - Alerts appear immediately when thresholds are crossed
3. **Live Updates** - Alert status updates in real-time across all open browser tabs
4. **User Groups** - Each user receives only their own alerts via SignalR groups

### Notification Channels

Alerts are delivered through multiple channels:

1. **In-App** - Alert cards on Budgets page (always shown)
2. **Email** - Notification email sent to user
3. **Push Notifications** - (When PWA is configured)
4. **Weekly Digest** - Summary email of all alerts

## 5. Budget Freeze Feature

When a budget is exceeded and "Budget Freeze" is enabled in settings:

```
┌─────────────────────────────────────────┐
│ ❄️ BUDGET FRYST: Mat & Dryck            │
│                                         │
│ Denna budget har överskridit gränsen    │
│ och är nu tillfälligt fryst.           │
│                                         │
│ Nya transaktioner i denna kategori      │
│ kommer att flaggas för granskning.      │
│                                         │
│ [Ta bort frysning]                     │
└─────────────────────────────────────────┘
```

**Freeze Indicators:**
- ❄️ Icon on budget category
- Warning message on transaction page
- Highlighted in budget list

## 6. Technical Implementation

### Backend Services

**BudgetAlertService:**
- Calculates budget usage percentages
- Determines daily spending rates
- Generates forecasts
- Creates alerts at thresholds
- Manages freeze status

**BudgetAlertBackgroundService:**
- Runs every 30 minutes
- Checks all active budgets
- Creates alerts when thresholds crossed
- Sends SignalR notifications

**BudgetAlertHub (SignalR):**
- Real-time client-server communication
- User-specific alert groups
- Methods: GetActiveAlerts, AcknowledgeAlert, CheckBudget

### Calculations

```csharp
// Daily Rate Calculation
dailyRate = totalSpent / daysElapsed

// Forecast Calculation  
daysUntilExceeded = remainingBudget / dailyRate

// Usage Percentage
usagePercentage = (spent / planned) * 100
```

## 7. User Settings

Users can customize alert behavior in Settings > Notifications:

**Alert Thresholds:**
- ☑ Enable alerts at 75%
- ☑ Enable alerts at 90%
- ☑ Enable alerts at 100%
- Custom thresholds (e.g., 80%, 95%)

**Alert Channels:**
- ☑ In-app notifications
- ☑ Email notifications
- ☐ Push notifications (PWA)
- ☐ SMS notifications

**Budget Freeze:**
- ☑ Enable automatic freeze when exceeded
- Days before auto-freeze: 0 (immediate)

**Forecast Settings:**
- ☑ Enable forecast warnings
- Forecast warning days: 7 days ahead

## 8. Usage Examples

### Scenario 1: Early Warning (75%)
**User:** Lisa
**Category:** Mat & Dryck
**Budget:** 5,000 kr/månad
**Spent:** 3,750 kr (75%)
**Days Remaining:** 20 days

**Alert:** "Du har använt 3,750 kr av 5,000 kr (75%). Återstående: 1,250 kr för 20 dagar."

**Action:** Lisa adjusts spending to stay within budget.

---

### Scenario 2: Critical Warning with Forecast (90%)
**User:** Erik
**Category:** Shopping
**Budget:** 3,000 kr/månad
**Spent:** 2,700 kr (90%)
**Days Remaining:** 8 days
**Daily Rate:** 180 kr/dag

**Forecast:** Budget exceeds in ~2 days at current rate

**Alert:** "⚠️ VARNING! Du har använt 2,700 kr av 3,000 kr (90%). Prognos: Budget överskrids om 2 dagar i nuvarande takt (180 kr/dag)."

**Action:** Erik receives email notification and stops discretionary spending.

---

### Scenario 3: Budget Exceeded with Freeze (105%)
**User:** Maria
**Category:** Transport
**Budget:** 2,000 kr/månad
**Spent:** 2,100 kr (105%)
**Freeze:** Enabled

**Alert:** "🚨 Budget överskriden! Transport-budgeten har överskridits med 100 kr. Budget freeze aktiverad."

**Action:** Maria receives critical notification. New transport expenses are flagged for review.

## 9. Benefits

✅ **Proactive Monitoring** - Catch budget issues before they become problems
✅ **Data-Driven Forecasts** - Predict future spending based on current trends
✅ **Real-Time Updates** - Instant alerts when thresholds are crossed
✅ **Multi-Channel Notifications** - Reach users through their preferred channels
✅ **Budget Protection** - Freeze feature prevents overspending
✅ **User Control** - Customizable thresholds and notification preferences

## 10. Testing

To test the budget alert feature:

1. **Create a budget** with low thresholds (e.g., 1,000 kr)
2. **Add transactions** totaling 750 kr (75% threshold)
3. **Observe** alert appears on Budgets page
4. **Add more transactions** to reach 900 kr (90% threshold)
5. **Check** email/notifications for alert
6. **Exceed budget** (1,050 kr) to trigger freeze
7. **Verify** freeze indicator and restrictions

## Summary

The Budget Alert system provides comprehensive, real-time monitoring of budget status with intelligent forecasting and multi-channel notifications. The visual indicators, color-coded progress bars, and detailed forecast information help users maintain financial discipline and avoid budget overruns.
