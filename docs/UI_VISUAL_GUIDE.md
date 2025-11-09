# UI Screenshots and Visual Guide

## Debt Simulation Features - Visual Overview

This document provides a visual representation of the new debt simulation features implemented in the Loans page.

## Page Structure

The Loans page (`/loans`) now has **4 tabs**:

1. **Översikt** (Overview) - Existing tab showing all loans
2. **Amorteringsplan** (Amortization Schedule) - Enhanced with export button
3. **Avbetalningsstrategier** (Repayment Strategies) - Enhanced with simulation tool and export buttons
4. **Detaljerad Simulering** (Detailed Simulation) - **NEW TAB**

---

## Tab 1: Översikt (Overview)

*Existing functionality - unchanged*

Shows:
- Summary cards: Total Debt, Monthly Cost, Number of Loans, Debt-Free Date
- Table of all loans with edit/delete buttons
- "Nytt Lån/Kredit" button to add new loans

---

## Tab 2: Amorteringsplan (Amortization Schedule)

### Enhanced with Export Button

```
┌─────────────────────────────────────────────────────────────┐
│ Amorteringsplan för [Loan Name]     [Export CSV Button] ▼  │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Betalning | Datum   | Ing. Saldo | Betalning | Ränta | Amortering | Utg. Saldo | Tot. Ränta │
│  ───────────────────────────────────────────────────────────────────────────────────────────── │
│     1      | 2024-01 |  100,000   |   2,000   |  417  |   1,583    |   98,417   |    417     │
│     2      | 2024-02 |   98,417   |   2,000   |  410  |   1,590    |   96,827   |    827     │
│     3      | 2024-03 |   96,827   |   2,000   |  404  |   1,596    |   95,231   |  1,231     │
│    ...     |   ...   |    ...     |    ...    |  ...  |    ...     |    ...     |    ...     │
│                                                              │
│  Visar första 60 av 120 betalningar                         │
└─────────────────────────────────────────────────────────────┘
```

**New Feature:**
- **Export Button**: Downloads CSV file with complete amortization schedule
- Filename: `amorteringsplan_[LoanName]_[Date].csv`

---

## Tab 3: Avbetalningsstrategier (Repayment Strategies)

### Enhanced with Interactive Simulation Tool

```
┌─────────────────────────────────────────────────────────────┐
│ Jämför avbetalningsstrategier                               │
├─────────────────────────────────────────────────────────────┤
│ ℹ️ Info Alert: Snöboll-metoden betalar av minsta skulden   │
│   först. Lavin-metoden betalar av högsta räntan först.     │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ 💡 Simuleringsverktyg                                        │
├─────────────────────────────────────────────────────────────┤
│ Justera din tillgängliga månadsbetalning för att se hur    │
│ det påverkar avbetalningen av dina skulder.                │
│                                                              │
│ Tillgänglig månadsbetalning (kr): [  5,000.00  ]           │
│                                                              │
│ [ Kör simulering ]                                          │
│                                                              │
│ Minsta månatliga betalning: 2,500 kr                       │
└─────────────────────────────────────────────────────────────┘

┌──────────────────────────────┬──────────────────────────────┐
│  ⛷️ Snöboll-metoden        📥 │  🏔️ Lavin-metoden        📥 │
├──────────────────────────────┼──────────────────────────────┤
│ Betala av minsta skulden    │ Betala av högsta räntan      │
│ först för psykologiska       │ först för att minimera       │
│ vinster och momentum         │ räntekostnader               │
│ ─────────────────────────    │ ─────────────────────────    │
│ Skuldfri: 2026-03-15         │ Skuldfri: 2026-02-20         │
│ Total ränta: 12,450 kr       │ Total ränta: 11,800 kr       │
│ Antal månader: 24            │ Antal månader: 23            │
│                              │                               │
│                              │ ✅ Sparar 650 kr i ränta!     │
└──────────────────────────────┴──────────────────────────────┘
```

**New Features:**
1. **Simulation Tool Panel**:
   - Adjustable monthly payment input (numeric field)
   - "Kör simulering" button to recalculate
   - Shows minimum required payment
   - Validates input

2. **Export Buttons** (📥 on each card):
   - Export button on Snowball strategy card
   - Export button on Avalanche strategy card
   - Downloads CSV files with strategy details

---

## Tab 4: Detaljerad Simulering (Detailed Simulation) - **NEW**

### Complete Month-by-Month Breakdown

```
┌─────────────────────────────────────────────────────────────┐
│ Månad-för-månad simulering: Avalanche                       │
├─────────────────────────────────────────────────────────────┤
│ ℹ️ Algoritm: Betala av skulden med högst ränta först       │
│    Formel: Månadsränta = (Belopp × Årsränta / 100) / 12    │
└─────────────────────────────────────────────────────────────┘

┌──────────────┬──────────────┬──────────────┬──────────────┐
│ Skuldfri     │ Total ränta  │ Antal mån.   │ Total kostnad│
│ 2026-02-20   │ 11,800 kr    │ 23           │ 86,800 kr    │
└──────────────┴──────────────┴──────────────┴──────────────┘

┌─────────────────────────────────────────────────────────────┐
│ Avbetalningsordning                                          │
├─────────────────────────────────────────────────────────────┤
│ Ord. | Lån           | Belopp   | Ränta  | Betalt    | Mån│
│ ─────┼───────────────┼──────────┼────────┼───────────┼────│
│  1   │ Kreditkort    │  5,000   │ 18.0%  │ 2024-06   │  6 │
│  2   │ Privatlån     │ 20,000   │  8.0%  │ 2025-02   │ 14 │
│  3   │ Studielån     │ 50,000   │  3.0%  │ 2026-02   │ 23 │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ Månatlig betalningsplan                                      │
├─────────────────────────────────────────────────────────────┤
│ Mån | Datum  | Tot. Bet. | Amort. | Ränta | Kvar  | Aktiva│
│ ────┼────────┼───────────┼────────┼───────┼───────┼───────│
│  1  │2024-01 │  2,500    │ 2,200  │  300  │72,800 │[K][P]│
│  2  │2024-02 │  2,500    │ 2,220  │  280  │70,580 │[K][P]│
│  3  │2024-03 │  2,500    │ 2,240  │  260  │68,340 │[K][P]│
│  4  │2024-04 │  2,500    │ 2,260  │  240  │66,080 │[K][P]│
│  5  │2024-05 │  2,500    │ 2,280  │  220  │63,800 │[K][P]│
│  6  │2024-06 │  2,300    │ 2,120  │  180  │61,680 │[✓K][P]│
│  7  │2024-07 │  2,700    │ 2,590  │  110  │59,090 │[P][S] │
│ ... │  ...   │   ...     │  ...   │  ...  │  ...  │  ...  │
│                                                              │
│  Visar första 60 av 23 månader                              │
└─────────────────────────────────────────────────────────────┘

Legend:
[K] = Kreditkort (Active loan)
[P] = Privatlån (Active loan, highlighted if focus loan)
[S] = Studielån (Active loan)
[✓K] = Kreditkort (Paid off this month - shown in green)
```

**New Features:**

1. **Summary Section**:
   - Algorithm description with inline formula
   - Four summary cards with key metrics

2. **Payoff Order Table**:
   - Shows order loans will be paid off
   - Original loan details
   - Payoff dates and durations
   - Total interest per loan

3. **Monthly Payment Schedule**:
   - Month-by-month breakdown
   - Total payments, principal, and interest
   - Remaining total debt
   - Active loan indicators:
     - Regular chips for active loans
     - Highlighted chip for focus loan (receiving extra payments)
     - Green checkmark chips for loans paid off this month
   - First 60 months shown with counter

---

## CSV Export Examples

### Amortization Schedule CSV

```csv
Amorteringsplan för: Kreditkort
Lånebelopp: 5000.00 kr
Ränta: 18.00%
Månatlig amortering: 200.00 kr

Betalning,Datum,Ingående Saldo,Betalning,Ränta,Amortering,Utgående Saldo,Total Ränta
1,2024-01,5000.00,275.00,75.00,200.00,4800.00,75.00
2,2024-02,4800.00,272.00,72.00,200.00,4600.00,147.00
3,2024-03,4600.00,269.00,69.00,200.00,4400.00,216.00
...

Sammanfattning
Antal betalningar,26
Total ränta,945.50 kr
Total kostnad,5945.50 kr
Betalt datum,2026-02
```

### Strategy CSV

```csv
Avbetalningsstrategi: Lavin
Beskrivning: Betala av skulden med högst ränta först för att minimera totala räntekostnader
Skuldfri datum: 2026-02-20
Total ränta: 11,800.00 kr
Total kostnad: 86,800.00 kr
Antal månader: 23

Avbetalningsordning
Ordning,Lån,Belopp,Ränta,Betalt datum,Månader,Total ränta
1,"Kreditkort",5000.00,18.00%,2024-06,6,450.00
2,"Privatlån",20000.00,8.00%,2025-02,14,1280.00
3,"Studielån",50000.00,3.00%,2026-02,23,10070.00
```

---

## User Workflows

### Workflow 1: Export Amortization Schedule

1. Navigate to Loans page (`/loans`)
2. Go to "Amorteringsplan" tab
3. Review the schedule
4. Click "Exportera till CSV" button
5. File downloads automatically
6. Open in Excel or other spreadsheet software

### Workflow 2: Compare Strategies

1. Navigate to Loans page
2. Go to "Avbetalningsstrategier" tab
3. Adjust monthly payment amount
4. Click "Kör simulering"
5. Review Snowball vs Avalanche comparison
6. Note the interest savings
7. Click export button on preferred strategy

### Workflow 3: Detailed Analysis

1. Run simulation (as above)
2. Go to "Detaljerad Simulering" tab
3. Review payoff order
4. Scroll through monthly schedule
5. Identify focus loan (highlighted)
6. See when each loan is paid off

---

## Visual Indicators

### Loan Status Chips

- **Default (Gray)**: Regular active loan
- **Primary (Blue)**: Focus loan receiving extra payments
- **Success (Green with ✓)**: Loan paid off this month

### Color Scheme

Following MudBlazor theme:
- Primary: Blue/Indigo for actions and focus
- Success: Green for positive outcomes
- Info: Light blue for informational content
- Warning: Amber for validation messages
- Error: Red for errors

---

## Responsive Design

All components are responsive and work on:
- Desktop (full width tables)
- Tablet (stacked cards)
- Mobile (vertical lists)

---

## Accessibility

- Proper ARIA labels on all interactive elements
- Keyboard navigation support
- Screen reader compatible
- High contrast support in dark mode

---

This visual guide demonstrates the comprehensive UI enhancements that provide users with powerful tools for debt analysis and planning.
