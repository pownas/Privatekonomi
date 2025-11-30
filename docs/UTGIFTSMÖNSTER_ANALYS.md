# Utgiftsmönster-analys

## Översikt

Utgiftsmönster-analysen är en omfattande rapport som hjälper användare att förstå sina utgiftsvanor, identifiera trender och upptäcka möjligheter till besparingar. Rapporten kombinerar flera analystekniker för att ge djupgående insikter om din ekonomi.

## Tillgång till rapporten

Rapporten finns tillgänglig via:
- **Navigeringsmeny**: Ekonomi → Utgiftsmönster-analys
- **Direkt URL**: `/economy/reports/spending-pattern`

## Funktioner

### 1. Periodval

Välj vilken tidsperiod du vill analysera:

- **Senaste månaden**: Analyserar dina utgifter från de senaste 30 dagarna
- **Senaste kvartalet**: Analyserar 3 månader av utgiftsdata
- **Senaste året**: Analyserar 12 månaders utgiftsmönster
- **Anpassad period**: Välj dina egna start- och slutdatum för flexibel analys

### 2. Sammanfattningskort

Högst upp i rapporten visas fyra sammanfattningskort:

- **Total utgifter**: Det totala beloppet du har spenderat under perioden
- **Genomsnitt/månad**: Genomsnittlig utgift per månad (användbart för budgetplanering)
- **Kategorier**: Antal kategorier där du har utgifter
- **Avvikelser**: Antal upptäckta ovanliga utgiftsmönster

### 3. Rekommendationer

Systemet genererar automatiskt rekommendationer baserat på din utgiftsdata:

#### Typer av rekommendationer:

**Budgetlarm (BudgetAlert)**
- Identifierar kategorier som utgör en stor del av dina totala utgifter (>20%)
- Föreslår att du ser över möjligheter att minska kostnader i dessa områden

**Trendvarningar (TrendWarning)**
- Upptäcker kategorier där utgifterna ökar kraftigt (>15%)
- Visar potentiell besparing om trenden bryts
- Flaggar ovanligt höga utgifter jämfört med historiken

**Besparingsmöjligheter (SavingsOpportunity)**
- Uppmärksammar många okategoriserade transaktioner
- Ger positiv återkoppling när utgifter minskar
- Visar faktiska besparingar vid minskande trender

#### Prioritetsnivåer:

- 🔴 **Hög (High)**: Kräver omedelbar uppmärksamhet
- 🟡 **Medium**: Bör ses över inom kort
- 🔵 **Låg (Low)**: Informativ, ingen akut åtgärd krävs

### 4. Utgiftsfördelning per kategori

En detaljerad tabell som visar:

- **Kategori**: Kategorins namn med tillhörande färg
- **Belopp**: Total summa spenderad i kategorin
- **Andel**: Procentuell andel av totala utgifter
- **Antal**: Antal transaktioner i kategorin
- **Snitt/trans**: Genomsnittligt belopp per transaktion
- **Förändring**: Procentuell förändring jämfört med föregående period
  - ↗️ Röd pil upp = ökande utgifter
  - ↘️ Grön pil ner = minskande utgifter

### 5. Topp 5 utgiftskategorier

En visuell framställning av dina fem största utgiftskategorier:

- Rankade i ordning (#1-#5)
- Stapeldiagram som visar relativ storlek
- Exakt belopp för varje kategori
- Snabb överblick över var pengarna går

### 6. Månatlig trend

En tabell som visar utgiftsutveckling månad för månad:

- **Månad**: Månad och år (t.ex. "november 2024")
- **Belopp**: Total utgift för månaden
- **Transaktioner**: Antal genomförda transaktioner
- **Snitt/dag**: Genomsnittlig utgift per dag i månaden

Denna data hjälper dig att:
- Identifiera säsongsmönster i dina utgifter
- Jämföra olika månader
- Planera för framtida utgifter

### 7. Trendanalys

Detaljerad analys av utgiftstrender både totalt och per kategori:

#### Trendtyper:

- **Ökande (Increasing)**: 🔴 Utgifterna ökar över tid
- **Minskande (Decreasing)**: 🟢 Utgifterna minskar över tid
- **Stabil (Stable)**: ⚪ Ingen signifikant förändring (<5%)

#### Vad visas:

- Trendprocent (hur mycket ökning/minskning)
- Beskrivning av trenden på svenska
- Endast signifikanta trender (>10% förändring) visas för kategorier

### 8. Upptäckta avvikelser

Systemet använder statistisk analys för att upptäcka ovanliga utgiftsmönster:

#### Avvikelsetyper:

**Ovanligt hög utgift (UnusuallyHigh)**
- Månad där utgifterna är betydligt högre än normalt
- Kan indikera extraordinära utgifter eller budgetproblem
- Visas i rött

**Ovanligt låg utgift (UnusuallyLow)**
- Månad där utgifterna är betydligt lägre än normalt
- Kan indikera sparsamhet eller ofullständig data
- Visas i blått

#### Vad visas:

- Datum för avvikelsen
- Faktiskt belopp
- Förväntat belopp (baserat på historisk data)
- Beskrivning av avvikelsen

## Analysmetoder

### Kategorifördelning

Beräknar hur dina utgifter fördelas mellan olika kategorier och jämför med föregående period (samma längd bakåt i tiden).

### Trenddetektering

Delar upp perioden i två halvor och jämför genomsnitten för att upptäcka trender. En förändring på >10% flaggas som signifikant trend.

### Anomalidetektering

Använder standardavvikelse för att identifiera månader som avviker mer än 2 standardavvikelser från genomsnittet. Detta hjälper till att upptäcka ovanliga mönster.

### Rekommendationsgenerering

Regelbaserad motor som:
1. Identifierar kategorier med hög andel (>20%)
2. Upptäcker kraftigt ökande trender (>15%)
3. Flaggar många okategoriserade transaktioner (>10%)
4. Ger positiv återkoppling för minskande utgifter

## Användningsområden

### Budgetplanering

Använd månadsgenomsnittet och kategorifördelningen för att sätta realistiska budgetar.

### Kostnadskontroll

Identifiera kategorier där du spenderar mest och utvärdera om det finns möjlighet till besparing.

### Trendövervakning

Håll koll på om dina utgifter ökar eller minskar över tid i olika kategorier.

### Avvikelsehantering

Undersök ovanliga utgiftsmönster och förstå vad som orsakade dem.

### Sparplanering

Använd rekommendationerna för att identifiera konkreta besparingsmöjligheter.

## Tips för bästa resultat

1. **Kategorisera dina transaktioner**: Ju fler transaktioner som är kategoriserade, desto mer exakta blir analyserna.

2. **Använd längre perioder för trender**: Trendanalys fungerar bäst med minst 3-6 månaders data.

3. **Jämför olika perioder**: Byt mellan månad, kvartal och år för att se olika perspektiv.

4. **Följ upp rekommendationer**: Ta rekommendationerna på allvar och implementera förändringar.

5. **Regelbunden granskning**: Kör rapporten månadsvis för att följa upp utvecklingen.

6. **Kombinera med andra rapporter**: Använd tillsammans med Budgetöversikt och Ekonomisk Hälsa för en helhetsbild.

## Begränsningar

- Kräver historisk data för trend- och avvikelseanalys (minst 2-3 månader)
- Endast utgiftstransaktioner inkluderas (inte inkomster)
- Anomalidetektering kräver minst 3 månaders data
- Okategoriserade transaktioner grupperas separat
- Hushållsfiltrering stöds men är valfri

## Framtida utveckling

Planerade förbättringar inkluderar:

- 📊 Grafisk visualisering med diagram (tårtdiagram, stapeldiagram, tidsserier)
- 💾 Export till PDF och Excel
- 🔍 Drill-down till transaktionsnivå direkt från rapporten
- 📈 Prognoser baserade på historiska mönster
- 🎯 Anpassningsbara tröskelvärden för rekommendationer
- 🤖 Maskininlärningsbaserad anomalidetektering
- 📅 Rapporthistorik och jämförelser mellan perioder
- 🔔 Notifikationer vid stora avvikelser

## Teknisk information

### API-anrop

Rapporten använder följande service-metod:

```csharp
Task<SpendingPatternReport> GetSpendingPatternReportAsync(
    DateTime fromDate, 
    DateTime toDate, 
    int? householdId = null
)
```

### Datamodeller

- `SpendingPatternReport`: Huvudrapporten
- `CategorySpending`: Utgiftsdata per kategori
- `SpendingTrend`: Trenddata
- `SpendingAnomaly`: Avvikelsedata
- `SpendingRecommendation`: Rekommendationer
- `MonthlySpendingData`: Månatlig sammanfattning

### Prestanda

- Rapporten cachas inte - genereras vid varje anrop
- Rekommenderad maxperiod: 2 år (för prestanda)
- Typisk svarstid: <1 sekund för 12 månaders data

## Support

För frågor eller problem med Utgiftsmönster-analysen, kontakta supporten eller öppna ett issue på GitHub.
