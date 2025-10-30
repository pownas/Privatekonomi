# Life Timeline Planner - UI Visual Description

## Page Layout

```
┌─────────────────────────────────────────────────────────────────┐
│ 🗓️ Livslinjeplanering                    [+ Ny Milstolpe]      │
│ Långsiktig ekonomisk planering över hela livet                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│ ┌─ Scenarioplanering ──────────────────── [+ Nytt Scenario] ─┐ │
│ │                                                             │ │
│ │ Aktivt Scenario: [Realistisk ▼]                           │ │
│ │                                                             │ │
│ │ ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐│ │
│ │ │Månadsspar  │ │Förväntad   │ │Pensionsål. │ │Prognos     ││ │
│ │ │4,500 kr    │ │avkastning  │ │65 år       │ │pension     ││ │
│ │ │            │ │7.0%        │ │            │ │25M kr      ││ │
│ │ └────────────┘ └────────────┘ └────────────┘ └────────────┘│ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ ┌─ Tidslinjen ───────────────────────────────────────────────┐ │
│ │                                                             │ │
│ │ ┌───────────────────────────────────────────────────────┐  │ │
│ │ │ 🏠 Köpa bostad             [Prioritet 1] [✏️] [🗑️]    │  │ │
│ │ │ Kontantinsats för lägenhet                             │  │ │
│ │ │                                                         │  │ │
│ │ │ Datum: 2030-06-01 (5.5 år)  Kostnad: 1,500,000 kr     │  │ │
│ │ │ Sparat: 200,000 kr   Kräver/mån: 21,667 kr            │  │ │
│ │ │                                                         │  │ │
│ │ │ ████████░░░░░░░░░░  13%                               │  │ │
│ │ └───────────────────────────────────────────────────────┘  │ │
│ │                                                             │ │
│ │ ┌───────────────────────────────────────────────────────┐  │ │
│ │ │ 👶 Barn                     [Prioritet 2] [✏️] [🗑️]    │  │ │
│ │ │ Ekonomisk förberedelse                                 │  │ │
│ │ │                                                         │  │ │
│ │ │ Datum: 2032-03-15 (7.4 år)  Kostnad: 300,000 kr       │  │ │
│ │ │ Sparat: 50,000 kr    Kräver/mån: 2,816 kr             │  │ │
│ │ │                                                         │  │ │
│ │ │ ████████████████░░░  17%                              │  │ │
│ │ └───────────────────────────────────────────────────────┘  │ │
│ │                                                             │ │
│ │ ┌───────────────────────────────────────────────────────┐  │ │
│ │ │ 👴 Pension               [Prioritet 3] [Genomförd]     │  │ │
│ │ │ Första milstolpen till pensionen                       │  │ │
│ │ │                                                         │  │ │
│ │ │ Datum: 2055-12-31 (30.2 år) Kostnad: 5,000,000 kr     │  │ │
│ │ │ Sparat: 800,000 kr   Kräver/mån: 11,602 kr            │  │ │
│ │ │                                                         │  │ │
│ │ │ ███████████░░░░░░░░  16%                              │  │ │
│ │ └───────────────────────────────────────────────────────┘  │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Color Scheme

### Priority Colors
- **Prioritet 1** (Högst): 🔴 Red
- **Prioritet 2** (Hög): 🟡 Orange/Warning
- **Prioritet 3** (Normal): 🔵 Blue/Primary
- **Prioritet 4** (Låg): 🟢 Teal/Info
- **Prioritet 5** (Lägst): ⚪ Default/Gray

### Milestone Type Icons and Colors
- **🏠 Köpa bostad** (HousePurchase): Blue
- **👶 Barn** (Child): Pink/Secondary
- **👴 Pension** (Retirement): Green/Success
- **🎓 Utbildning** (Education): Cyan/Info
- **💼 Karriärbyte** (Career): Orange/Warning
- **🎯 Annat** (Other): Gray/Default

### Progress Bar Colors
- **100%**: Green (Success) - Goal reached
- **75-99%**: Cyan (Info) - Almost there
- **50-74%**: Blue (Primary) - On track
- **25-49%**: Orange (Warning) - Needs attention
- **0-24%**: Red (Error) - Behind schedule

## Form Examples

### Milestone Form
```
┌─ Lägg till Ny Milstolpe ──────────────────────────┐
│                                                    │
│ Namn:         [Köpa villa_____________________]   │
│                                                    │
│ Beskrivning:  [Kontantinsats och flyttkostnader  │
│               ___________________________________] │
│                                                    │
│ Typ:          [Köpa bostad ▼]                     │
│               • Köpa bostad                        │
│               • Barn                               │
│               • Pension                            │
│               • Utbildning                         │
│               • Karriärbyte                        │
│               • Annat                              │
│                                                    │
│ Planerat datum: [📅 2030-06-01]                   │
│                                                    │
│ Beräknad kostnad: [1,500,000] kr                  │
│                                                    │
│ Redan sparat:     [200,000] kr                    │
│                                                    │
│ Prioritet:    [3 - Normal ▼]                      │
│                                                    │
│ [Lägg till]  [Avbryt]                             │
└────────────────────────────────────────────────────┘
```

### Scenario Form
```
┌─ Skapa Nytt Scenario ─────────────────────────────┐
│                                                    │
│ Scenarionamn: [Realistisk___________________]     │
│                                                    │
│ Beskrivning:  [Baserat på historisk avkastning   │
│               ___________________________________] │
│                                                    │
│ Månadssparande: [4,500] kr                        │
│                                                    │
│ Förväntad årsavkastning: [7.0] %                  │
│                          ├───●───────┤ 0-20%      │
│                                                    │
│ Pensionsålder: [65]                               │
│                ├───────●─────┤ 55-75              │
│                                                    │
│ Inflationstakt: [2.0] %                           │
│                 ├──●─────────┤ 0-10%              │
│                                                    │
│ [Skapa Scenario]  [Avbryt]                        │
└────────────────────────────────────────────────────┘
```

## Mobile Responsive View

On smaller screens, the layout adjusts:
- Scenario cards stack vertically (1 column)
- Milestone cards become full-width
- Edit/Delete buttons move to a dropdown menu
- Forms become full-screen overlays

## Empty State

When no milestones exist:
```
┌─────────────────────────────────────────┐
│                                         │
│              📅 Timeline Icon           │
│                                         │
│        Inga milstolpar än               │
│                                         │
│ Skapa milstolpar för att planera din   │
│ ekonomiska framtid. Lägg till viktiga  │
│ händelser som köp av bostad, barn,     │
│ eller pension.                          │
│                                         │
└─────────────────────────────────────────┘
```

## Interaction Flow

1. **User arrives at page**
   - Sees existing scenarios (if any)
   - Sees timeline with milestones sorted by date

2. **Create new scenario**
   - Click "Nytt Scenario"
   - Fill in form
   - System auto-activates if first scenario
   - Scenario appears in dropdown

3. **Select active scenario**
   - Choose from dropdown
   - Cards update with scenario details
   - Projected wealth recalculated

4. **Create new milestone**
   - Click "Ny Milstolpe"
   - Fill in details
   - System calculates required monthly savings
   - Milestone appears in timeline

5. **View progress**
   - See progress bars for each milestone
   - View years remaining
   - Check required monthly savings

6. **Edit milestone**
   - Click edit button
   - Update details
   - Progress recalculates automatically

This visual description helps understand the user experience without running the application.
