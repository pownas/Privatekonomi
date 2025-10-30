# Livslinjeplanering (Life Timeline Planner)

## Översikt

Livslinjeplanering är en funktion för långsiktig ekonomisk planering som hjälper användare att visualisera och planera för viktiga ekonomiska milstolpar genom hela livet, från nuvarande ålder till pension.

## Funktioner

### 1. Milstolpar (Milestones)

Användare kan skapa och hantera viktiga livshändelser med ekonomisk påverkan:

- **Typer av milstolpar:**
  - Köpa bostad (HousePurchase)
  - Barn (Child)
  - Pension (Retirement)
  - Utbildning (Education)
  - Karriärbyte (Career)
  - Annat (Other)

- **För varje milstolpe kan du ange:**
  - Namn och beskrivning
  - Planerat datum
  - Beräknad kostnad
  - Redan sparat belopp
  - Prioritet (1-5)
  - Anteckningar

- **Automatiska beräkningar:**
  - Framsteg i procent (baserat på sparat belopp vs kostnad)
  - Kräver månadssparande för att nå målet

### 2. Scenarioplanering

Skapa och jämför olika ekonomiska scenarios för framtiden:

- **Scenarioparametrar:**
  - Månadssparande (kr)
  - Förväntad årsavkastning (%)
  - Pensionsålder
  - Inflationstakt (%)
  - Löneutvecklingstakt (%)

- **Beräkningar:**
  - Projicerad pensionsförmögenhet
  - Förväntad månatlig pension

- **Aktiva scenario:**
  - Ett scenario kan markeras som aktivt
  - Används för beräkningar och prognoser

### 3. Tidslinje-visualisering

Visuell representation av milstolpar:
- Sorterade kronologiskt från idag till framtiden
- Ikoner för olika typer av milstolpar
- Färgkodade prioritetsnivåer
- Framstegsindikatorer för varje milstolpe
- Information om tid kvar och kräver månadssparande

## Användning

### Skapa en ny milstolpe

1. Navigera till "Sparande > Livslinjeplanering" i menyn
2. Klicka på "Ny Milstolpe"
3. Fyll i information:
   - Namn (t.ex. "Köpa villa")
   - Beskrivning
   - Typ av milstolpe
   - Planerat datum
   - Beräknad kostnad
   - Redan sparat belopp (om någon)
   - Prioritet
4. Klicka "Lägg till"

### Skapa ett scenario

1. I scenarioplanering-sektionen, klicka "Nytt Scenario"
2. Ange scenarionamn (t.ex. "Optimistisk", "Pessimistisk", "Realistisk")
3. Fyll i parametrar:
   - Månadssparande
   - Förväntad avkastning
   - Pensionsålder
   - Inflationstakt
4. Klicka "Skapa Scenario"
5. Välj scenario som aktivt från dropdown-menyn

## Teknisk implementation

### Modeller

**LifeTimelineMilestone:**
- ID, namn, beskrivning
- Typ av milstolpe
- Planerat datum
- Beräknad kostnad och redan sparat
- Prioritet (1-5)
- Status (genomförd eller ej)
- User ownership (UserId)

**LifeTimelineScenario:**
- ID, namn, beskrivning
- Månadssparande och avkastning
- Pensionsålder
- Inflation och löneutvecklingstakt
- Projicerad förmögenhet
- Aktiv/baseline-flaggor
- User ownership (UserId)

### Service

**LifeTimelinePlannerService** tillhandahåller:
- CRUD-operationer för milstolpar och scenarios
- Beräkning av kräver månadssparande
- Projektion av pensionsförmögenhet med sammansatt ränta
- Aggregering och statistik

### UI-komponenter

**LifeTimelinePlanner.razor:**
- Huvudsida med scenarioval
- Formulär för att skapa milstolpar och scenarios
- Tidslinje-visualisering med MudBlazor-komponenter
- Responsiv design med mobil-support

## Exempel på användning

### Scenario: Köpa bostad om 5 år

1. Skapa milstolpe:
   - Namn: "Köpa lägenhet"
   - Typ: Köpa bostad
   - Datum: 2030-06-01
   - Kostnad: 1 500 000 kr
   - Redan sparat: 200 000 kr
   - Prioritet: 1 (högst)

2. Systemet beräknar:
   - Kvarvarande belopp: 1 300 000 kr
   - Månader kvar: 60
   - Kräver månadssparande: ~21 667 kr/månad

3. Skapa scenario "Realistisk":
   - Månadssparande: 20 000 kr
   - Förväntad avkastning: 6%
   - Pensionsålder: 65

4. Systemet projicerar:
   - Total förmögenhet vid pension
   - Möjliga justeringar för att nå alla mål

## Framtida utveckling

Potentiella förbättringar:
- Integration med faktiska sparkonton (från Goals)
- Automatisk uppföljning av framsteg
- Notifikationer när milstolpar närmar sig
- Mer avancerade scenarios med ekonomiska chocker
- Visualisering av flera scenarios samtidigt
- Export av planering till PDF/Excel
- Integration med pensionsprognoser från Pensionsmyndigheten
- Livförsäkring-behovsanalys baserat på milstolpar
- Arv och gåvor-planering

## Säkerhet och integritet

- Alla data är användarspecifika (UserId)
- Ingen data delas mellan användare
- Standard EF Core säkerhet och datavalidering
- GDPR-kompatibel datahantering
