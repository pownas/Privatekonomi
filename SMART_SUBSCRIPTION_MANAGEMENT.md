# Smart Avtalshantering (Smart Subscription Management)

## Översikt
Smart Avtalshantering är en avancerad funktion för att spåra, hantera och optimera alla dina prenumerationer och återkommande avtal.

## Funktioner

### 1. Automatisk Upptäckt av Prenumerationer
Systemet analyserar dina transaktioner och identifierar automatiskt återkommande betalningar som kan vara prenumerationer.

**Hur det fungerar:**
- Analyserar transaktioner från de senaste 12 månaderna
- Identifierar betalningar som förekommer regelbundet (månadsvis, kvartalsvis, årligen)
- Kontrollerar att beloppen är konsekventa (inom 10% variation)
- Känner igen kända prenumerationstjänster (Netflix, Spotify, HBO, etc.)
- Kräver minst 3 transaktioner för att upptäcka ett mönster

**Användning:**
1. Klicka på knappen "Upptäck Abonnemang"
2. Systemet visar alla upptäckta prenumerationer
3. Välj vilka du vill lägga till
4. De markeras automatiskt som auto-upptäckta

### 2. Uppsägningsfrister och Påminnelser
Håll koll på när du måste säga upp prenumerationer för att undvika automatisk förnyelse.

**Fält:**
- **Uppsägningsfrist**: Datum då uppsägningen senast måste göras
- **Uppsägningstid (dagar)**: Antal dagar innan förnyelse som uppsägning krävs

**Varningar:**
- Systemet visar en varning när uppsägningsfristen närmar sig (inom 30 dagar)
- Alerts visas högst upp på sidan med antal dagar kvar

### 3. Oanvänd Prenumeration-Detektion
Identifiera prenumerationer som du inte använder och potentiellt kan spara pengar på.

**Hur det fungerar:**
- Spårar när du senast använde varje prenumeration
- Markerar prenumerationer som inte använts på 45+ dagar
- Visar antal dagar sedan senast använd

**Användning:**
- Uppdatera "Senast använd" fältet när du använder en tjänst
- Klicka på ✓-knappen i varningen för att snabbt markera som använd
- System visar antal oanvända prenumerationer högst upp

### 4. Delad Prenumeration-Spårning
Håll koll på familjeabonnemang och vilka som delar kostnaderna.

**Fält:**
- **Delad med**: Kommaseparerad lista över personer (t.ex. "Anna, Per, Lisa")

**Visning:**
- Prenumerationer som delas visas med en "Delad" badge
- Hovra över badgen för att se vilka som delar

### 5. Prisjämförelser och Historik
Spåra prisändringar över tid (befintlig funktion, utökad).

**Funktioner:**
- Automatisk loggning av prisändringar
- Historik över alla prisförändringar
- Notifikationer vid prisändringar

## Användargränssnitt

### Huvudvy
- **📋 Smart Avtalshantering** header
- **Upptäck Abonnemang** - Button för automatisk upptäckt
- **Nytt Abonnemang** - Button för manuell registrering
- Sammanfattningskort:
  - Antal aktiva abonnemang
  - Total månadskostnad
  - Total årskostnad

### Alerts (när tillämpligt)
1. **Uppsägningsfrister**: Gul varning med lista över abonnemang med kommande frister
2. **Oanvända prenumerationer**: Blå info-ruta med lista över oanvända tjänster

### Abonnemangslista
Kolumner:
- **Namn**: Med "Auto" badge för auto-upptäckta
- **Pris**: Visas i SEK
- **Frekvens**: Månadsvis/Årligen/Kvartalsvis
- **Nästa faktura**: Datum för nästa debitering
- **Status**: 
  - Varningssymbol för oanvända (med antal dagar)
  - Larmsymbol för kommande uppsägningsfrist (med antal dagar kvar)
  - Kategori-badge annars
- **Delad med**: "Delad" badge om familjeabonnemang
- **Länkar**: Uppsägnings- och hanteringslänkar
- **Åtgärder**: Redigera och ta bort

## Formulär
Nya fält i abonnemangsformuläret:

1. **Uppsägningsfrist (valfritt)**: Datumväljare
2. **Uppsägningstid (dagar)**: Numerisk input
3. **Senast använd (valfritt)**: Datumväljare
4. **Delad med (valfritt)**: Textfält för namn

## Teknisk Implementation

### Backend
**Nya fält i `Subscription` modell:**
```csharp
public DateTime? CancellationDeadline { get; set; }
public int? CancellationNoticeDays { get; set; }
public DateTime? LastUsedDate { get; set; }
public string? SharedWith { get; set; }
public bool AutoDetected { get; set; }
public int? DetectedFromTransactionId { get; set; }
```

**Nya metoder i `ISubscriptionService`:**
```csharp
Task<List<Subscription>> GetUnusedSubscriptionsAsync(string userId, int daysUnused = 45);
Task<List<Subscription>> GetSubscriptionsWithUpcomingCancellationDeadlineAsync(string userId, int daysAhead = 30);
Task UpdateLastUsedDateAsync(int subscriptionId, DateTime? lastUsedDate);
Task<List<Subscription>> DetectSubscriptionsFromTransactionsAsync(string userId);
Task<Subscription?> CreateSubscriptionFromTransactionAsync(int transactionId, string userId);
```

### Automatisk Upptäckt Algoritm
1. Hämtar alla transaktioner från senaste året
2. Grupperar efter payee/beskrivning
3. Kräver minst 3 förekomster
4. Kontrollerar beloppskonsekvens (±10%)
5. Analyserar intervall mellan transaktioner
6. Identifierar frekvens (månadsvis 25-35 dagar, kvartalsvis 85-95 dagar, årligen 350-380 dagar)
7. Filtrerar mot kända prenumerationstjänster
8. Returnerar förslag för användaren att bekräfta

### Tester
9 enhetstester i `SubscriptionServiceTests.cs`:
- ✅ GetUnusedSubscriptionsAsync_ReturnsSubscriptionsNotUsedRecently
- ✅ GetSubscriptionsWithUpcomingCancellationDeadlineAsync_ReturnsOnlyUpcomingDeadlines
- ✅ UpdateLastUsedDateAsync_UpdatesDateSuccessfully
- ✅ DetectSubscriptionsFromTransactionsAsync_DetectsRecurringPatterns
- ✅ DetectSubscriptionsFromTransactionsAsync_IgnoresInconsistentAmounts
- ✅ DetectSubscriptionsFromTransactionsAsync_DoesNotDuplicateExistingSubscriptions
- ✅ CreateSubscriptionFromTransactionAsync_CreatesSubscriptionSuccessfully
- ✅ CreateSubscriptionFromTransactionAsync_ReturnsNullForNonexistentTransaction
- ✅ GetMonthlySubscriptionCostAsync_CalculatesCorrectly

## Användningsexempel

### Scenario 1: Upptäck Befintliga Prenumerationer
1. Gå till Abonnemang-sidan
2. Klicka "Upptäck Abonnemang"
3. Systemet visar alla upptäckta prenumerationer (Netflix, Spotify, etc.)
4. Välj de du vill lägga till
5. Klicka "Lägg till valda"
6. Prenumerationerna läggs till med "Auto" badge

### Scenario 2: Hantera Uppsägningsfrister
1. Lägg till/redigera ett abonnemang
2. Fyll i "Uppsägningsfrist" (t.ex. 2025-12-15)
3. Fyll i "Uppsägningstid (dagar)" (t.ex. 30)
4. Spara
5. När fristen närmar sig (inom 30 dagar) visas en varning högst upp

### Scenario 3: Spåra Familjeabonnemang
1. Lägg till/redigera ett abonnemang (t.ex. Netflix)
2. Fyll i "Delad med" (t.ex. "Anna, Per, Lisa")
3. Spara
4. Abonnemanget visas med "Delad" badge
5. Hovra över badgen för att se vilka som delar

### Scenario 4: Identifiera Oanvända Tjänster
1. Systemet spårar automatiskt när prenumerationer inte används
2. Efter 45 dagar utan användning visas en varning
3. Se listan över oanvända prenumerationer högst upp
4. Klicka ✓ för att markera som använd, eller
5. Överväg att säga upp för att spara pengar

## Besparingsmöjligheter
Med Smart Avtalshantering kan du:
- 💰 Identifiera oanvända prenumerationer (genomsnittlig besparing: 500-1000 kr/mån)
- ⏰ Undvika automatiska förnyelelser du inte vill ha
- 👨‍👩‍👧‍👦 Optimera familjeabonnemang
- 📊 Se tydlig översikt över totala abonnemangskostnader
- 🔍 Upptäcka dolda prenumerationer automatiskt

## Framtida Förbättringar
Potentiella tillägg i framtiden:
- AI-baserade besparingsförslag
- Integration med prisjämförelsesajter
- Automatiska uppsägningar via API
- Delningskostnadsberäkning för familjeabonnemang
- Push-notifikationer för uppsägningsfrister
- Kategorivisning av prenumerationstyper
