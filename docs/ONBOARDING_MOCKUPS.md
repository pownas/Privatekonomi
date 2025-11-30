# Onboarding Flow - Screen Mockups

This document provides visual mockups (low-fi) of each onboarding screen.

---

## Screen 1: Välkommen (Welcome)

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                    [💰 Large Icon]                          │
│                                                             │
│              Välkommen till Privatekonomi!                  │
│                                                             │
│          Vi hjälper dig att få full koll på din ekonomi     │
│                                                             │
│  ─────────────────────────────────────────────────────────  │
│                                                             │
│  För att komma igång snabbt kommer vi att:                 │
│                                                             │
│  🏦 Koppla dina bankkonton för automatisk import           │
│  📜 Importera dina transaktioner (12-18 månader)           │
│  🏷️  Kategorisera dina utgifter automatiskt                │
│  📊 Föreslå en budget baserad på 50/30/20-regeln          │
│  📈 Visa din ekonomiska översikt                           │
│                                                             │
│  ℹ️  Din integritet är viktig för oss. All data lagras     │
│     säkert och krypterat. Du har full kontroll.            │
│                                                             │
│  Detta tar ungefär 5 minuter att slutföra.                 │
│                                                             │
│              [      Kom igång      ]                        │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Features:**
- Central icon (savings/money)
- Welcome message
- List of what will happen
- Privacy assurance
- Time estimate
- Single CTA button

---

## Screen 2: Välj bank (Bank Selection)

```
┌─────────────────────────────────────────────────────────────┐
│  🏦 Välj din bank                                           │
│  Anslut dina bankkonton för automatisk transaktionsimport  │
│  ─────────────────────────────────────────────────────────  │
│                                                             │
│  [🔍 Sök bank..............................]                │
│                                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐                 │
│  │   🏦     │  │   🏦     │  │   🏦     │                 │
│  │  SEB     │  │ Swedbank │  │ Nordea   │                 │
│  │  PSD2    │  │  PSD2    │  │  PSD2    │                 │
│  └──────────┘  └──────────┘  └──────────┘                 │
│                                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐                 │
│  │   🏦     │  │   🏦     │  │   🏦     │                 │
│  │Handelsb. │  │   ICA    │  │  Avanza  │                 │
│  │  PSD2    │  │  PSD2    │  │  PSD2    │                 │
│  └──────────┘  └──────────┘  └──────────┘                 │
│                                                             │
│  ℹ️  Kom snart: Vi arbetar på stöd för fler svenska        │
│     banker. Du kan hoppa över detta steg.                  │
│                                                             │
│  [Hoppa över]                    [   Fortsätt   ]          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Features:**
- Search bar for filtering
- Grid of bank cards (clickable)
- Each card shows bank name and API type
- Info message about upcoming features
- Skip and Continue buttons

---

## Screen 3: Samtycke (Consent)

```
┌─────────────────────────────────────────────────────────────┐
│  🔒 Samtycke och dataskydd                                  │
│  Innan vi fortsätter vill vi informera dig om data         │
│  ─────────────────────────────────────────────────────────  │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ 🔒 Så skyddar vi din data                             │ │
│  │                                                       │ │
│  │ ✅ Kryptering: All data krypteras                     │ │
│  │ ✅ Isolering: Din data är helt isolerad               │ │
│  │ ✅ Lokalt: Du kan köra appen lokalt                   │ │
│  │ ✅ GDPR: Vi följer EU:s dataskyddsförordning          │ │
│  │ ✅ Kontroll: Du äger din data                         │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                             │
│  ℹ️  PSD2 och bankkoppling: Vi använder PSD2-standarden    │
│     som är reglerad av EU. Vi lagrar aldrig dina           │
│     inloggningsuppgifter till banken.                      │
│                                                             │
│  [▼] Läs mer om vår integritetspolicy                      │
│                                                             │
│  ☐ Jag har läst och godkänner integritetspolicyn och       │
│    samtycker till att mina transaktioner behandlas         │
│                                                             │
│                          [Jag samtycker, fortsätt]         │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Features:**
- Security icon
- List of data protection measures
- PSD2 information
- Expandable privacy policy
- Required consent checkbox
- Continue button (disabled until checked)

---

## Screen 4: Importera transaktioner (Transaction Import)

```
┌─────────────────────────────────────────────────────────────┐
│  📜 Importera transaktioner                                 │
│  Importera för de senaste 12-18 månaderna                   │
│  ─────────────────────────────────────────────────────────  │
│                                                             │
│  ℹ️  OBSERVERA: Detta är en simulerad import för demo.     │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ Vad händer vid import?                                │ │
│  │                                                       │ │
│  │ 📥 Hämtar transaktioner från senaste 12-18 mån       │ │
│  │ 🏷️  Kategoriserar automatiskt                         │ │
│  │ 📊 Analyserar spenderingsmönster                      │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                             │
│  [Hoppa över]                    [Börja importera]         │
│                                                             │
│                     -- AFTER IMPORT --                      │
│                                                             │
│  ✅ Import klar! Vi har importerat 156 transaktioner.      │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ Snabb granskning av kategorier                        │ │
│  │                                                       │ │
│  │ Kategori            Antal      Belopp                │ │
│  │ ─────────────────────────────────────────────────    │ │
│  │ Boende               12       -48 000 kr             │ │
│  │ Mat & Livsmedel      45       -15 234 kr             │ │
│  │ Transport            23        -4 568 kr             │ │
│  │ Nöje                 18        -3 456 kr             │ │
│  │ Lön                  12       360 000 kr             │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                             │
│                          [Fortsätt till budget]            │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Features:**
- Shows before and after states
- Loading indicator during import
- Summary table with top categories
- Success message with count
- Note about simulation

---

## Screen 5: Budgetförslag (Budget Proposal)

```
┌─────────────────────────────────────────────────────────────┐
│  📊 Ditt budgetförslag                                      │
│  Baserat på 50/30/20-regeln                                 │
│  ─────────────────────────────────────────────────────────  │
│                                                             │
│  ℹ️  50/30/20-regeln: 50% Behov, 30% Önskemål, 20% Sparande│
│                                                             │
│  Månadsinkomst: 30 000 kr                                   │
│                                                             │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │ Behov (50%)  │ │ Önskemål     │ │ Sparande     │       │
│  │              │ │ (30%)        │ │ (20%)        │       │
│  │  15 000 kr   │ │  9 000 kr    │ │  6 000 kr    │       │
│  └──────────────┘ └──────────────┘ └──────────────┘       │
│                                                             │
│  Detaljerad budgetfördelning                                │
│                                                             │
│  Kategori            Förslag        Justera                 │
│  ────────────────────────────────────────────────────────   │
│  🏠 Boende           6 000 kr    [────●────────]            │
│  🍕 Mat & Livsmedel  5 250 kr    [────●────────]            │
│  🚗 Transport        2 250 kr    [──●──────────]            │
│  🛡️  Försäkringar     1 500 kr    [●─────────────]          │
│  🎉 Nöje             4 500 kr    [──────●───────]            │
│  🛍️  Shopping         2 700 kr    [───●──────────]          │
│  🍔 Restaurang       1 800 kr    [──●──────────]            │
│  💰 Sparande         6 000 kr    [────●────────]            │
│                                                             │
│  ☐ Aktivera budgetvarningar när jag närmar mig gränsen     │
│                                                             │
│  ⚠️  Detta är ett förslag. Du kan justera senare.          │
│                                                             │
│  [Hoppa över]              [Spara och aktivera budget]     │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Features:**
- 50/30/20 breakdown at top
- Detailed category table
- Interactive sliders for each category
- Budget alerts checkbox
- Warning about being a suggestion
- Skip and save options

---

## Screen 6: Klar (Completion)

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                    [✅ Large Icon]                          │
│                                                             │
│                      Du är klar!                            │
│                                                             │
│          Välkommen till Privatekonomi.                      │
│          Här är din månad i korthet:                        │
│  ─────────────────────────────────────────────────────────  │
│                                                             │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │  📈 Inkomster│ │  📉 Utgifter │ │  💰 Sparande │       │
│  │              │ │              │ │              │       │
│  │  30 000 kr   │ │  24 000 kr   │ │   6 000 kr   │       │
│  │  Denna månad │ │  Denna månad │ │  Kvar (20%)  │       │
│  └──────────────┘ └──────────────┘ └──────────────┘       │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ 💡 Nästa steg                                         │ │
│  │                                                       │ │
│  │ 📊 Utforska din dashboard                            │ │
│  │ 📝 Lägg till fler transaktioner                      │ │
│  │ 📈 Sätt upp sparmål                                  │ │
│  │ 🔔 Aktivera notifikationer                           │ │
│  │ 🏦 Koppla fler bankkonton                            │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                             │
│  ✅ Tips: Kolla in anpassningsbara widgets på dashboard!   │
│                                                             │
│              [📊 Gå till Dashboard]                         │
│                                                             │
│  Du kan ändra inställningar när som helst i menyn          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Features:**
- Success checkmark icon
- Monthly summary cards
- Next steps list
- Tip/hint message
- Large CTA to dashboard
- Reassurance about settings

---

## Flow Navigation

```
Register → Onboarding (Step 0) → ... → Completion (Step 5) → Dashboard
              ↓                                                    ↑
              └────────────── Skip available ─────────────────────┘
```

**Navigation Rules:**
- Register page redirects to `/onboarding` after successful registration
- Home page checks `OnboardingCompleted` flag, redirects if false
- Each step can navigate forward only
- Some steps have "Skip" options
- Completion marks `OnboardingCompleted = true` and redirects to `/`

---

## Color Scheme

**Background:**
- Gradient: Purple (#667eea) to Pink (#764ba2)

**Cards:**
- White background with elevation
- MudBlazor default shadows

**Buttons:**
- Primary: MudBlazor Primary color
- Secondary/Skip: MudBlazor Default/Text variant

**Icons:**
- MudBlazor Material Icons
- Primary color for main icons
- Success/Error/Warning colors for status

---

## Responsive Behavior

**Desktop (>960px):**
- Bank cards: 3 columns
- Budget summary: 3 columns
- Completion cards: 3 columns

**Tablet (600-960px):**
- Bank cards: 2 columns
- Budget summary: 3 columns (stacked)
- Completion cards: 2 columns

**Mobile (<600px):**
- All cards: 1 column
- Full-width buttons
- Condensed padding

---

## Accessibility

- All interactive elements have proper ARIA labels
- Keyboard navigation supported
- Focus indicators visible
- Color contrast meets WCAG 2.1 Level AA
- Screen reader compatible
- Swedish language alt text

---

## Technical Routes

| Step | Route | Component |
|------|-------|-----------|
| 0 | `/onboarding` or `/onboarding/0` | `OnboardingWelcome` |
| 1 | `/onboarding/1` | `OnboardingBankSelection` |
| 2 | `/onboarding/2` | `OnboardingConsent` |
| 3 | `/onboarding/3` | `OnboardingTransactionImport` |
| 4 | `/onboarding/4` | `OnboardingBudgetProposal` |
| 5 | `/onboarding/5` | `OnboardingCompletion` |

All routes are protected by authentication. Unauthenticated users are redirected to `/Account/Login`.
