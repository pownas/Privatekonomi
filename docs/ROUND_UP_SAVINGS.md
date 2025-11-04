# Round-up Sparande

Automatisk sparplanering genom att avrunda transaktioner och spara skillnaden.

## Översikt

Round-up Sparande är en funktion som hjälper dig att spara pengar automatiskt genom att:
- Avrunda varje utgift till närmaste 10 kr (konfigurerbart)
- Spara skillnaden automatiskt i ett valt sparmål
- Dubbla ditt sparande med arbetsgivarmatchning
- Spara en procentandel av varje inkomst automatiskt

## Funktioner

### 1. Grundläggande Round-up
När du gör ett köp på t.ex. 127 kr avrundar systemet till 130 kr och sparar de 3 kr som blir över i ditt valda sparmål.

**Exempel:**
```
💰 Round-up Sparande

Senaste transaktioner:
- ICA:      127 kr → 130 kr (3 kr sparat)
- SL-kort:  245 kr → 250 kr (5 kr sparat)
- Bensin:   587 kr → 590 kr (3 kr sparat)

Total denna månad: 145 kr från round-ups! 🎉
```

### 2. Arbetsgivarmatchning
Aktivera "Matcha min arbetsgivare" för att dubbla ditt sparande. Varje gång du sparar 3 kr via round-up, lägger "arbetsgivaren" till ytterligare 3 kr.

**Konfiguration:**
- **Matchningsprocent**: Standard 100% (dubblar sparandet). Kan sättas till 50% för halv matchning eller 200% för tredubbelt sparande.
- **Månadstak**: Valfritt tak för hur mycket arbetsgivaren matchar per månad.

### 3. Lön-regel
Spara automatiskt en procentandel av varje inkomst. Standard är 10%, men du kan justera detta efter dina behov.

**Exempel:**
- Lön: 25 000 kr → 2 500 kr sparas automatiskt (vid 10%)
- Bonusar och andra inkomster sparas också automatiskt

### 4. Avancerade Filter
- **Minimumbelopp**: Ignorera transaktioner under ett visst belopp
- **Maximumbelopp**: Ignorera stora transaktioner över ett visst belopp
- **Endast utgifter**: Välj om round-up endast ska tillämpas på utgifter (inte inkomster)

## Kom igång

1. **Navigera till Round-up Sparande**
   - Gå till menyn "Sparande" → "Round-up Sparande"

2. **Aktivera funktionen**
   - Klicka på "Aktivera Round-up Sparande"

3. **Välj sparmål**
   - Välj vilket sparmål pengarna ska sparas till
   - Om du inte har något sparmål, skapa ett under "Sparande" → "Sparmål"

4. **Anpassa inställningar**
   - Justera avrundningsbelopp (standard: 10 kr)
   - Aktivera arbetsgivarmatchning om önskat
   - Aktivera lön-regel om du vill spara en % av inkomster
   - Sätt filter för min/max belopp om du vill

5. **Spara inställningar**
   - Klicka på "Spara Inställningar"

## Automatisk aktivering

När Round-up är aktiverat kommer funktionen automatiskt att:
- Bearbeta varje ny transaktion du skapar
- Beräkna avrundningsbeloppet
- Spara beloppet i ditt valda sparmål
- Applicera arbetsgivarmatchning om aktiverat
- Spara procentandel av inkomster om lön-regel är aktiverad

## Statistik och historik

På Round-up sidan kan du se:
- **Total sparat denna månad**: Totalt belopp från alla round-ups
- **Senaste transaktioner**: De 5 senaste round-up transaktionerna
- **Statistik**:
  - Total Round-up (exklusive matchning)
  - Arbetsgivarmatchning
  - Lön-sparande
  - Antal transaktioner

## Teknisk implementation

### Datamodeller

**RoundUpSettings**
- Innehåller användarens inställningar för round-up
- En post per användare

**RoundUpTransaction**
- Loggning av varje round-up som görs
- Kopplas till originaltranskationen
- Innehåller belopp, matchning och totalt sparat

### Services

**IRoundUpService**
- `GetOrCreateSettingsAsync()`: Hämta eller skapa inställningar
- `UpdateSettingsAsync()`: Uppdatera inställningar
- `CalculateRoundUp()`: Beräkna avrundningsbelopp
- `ProcessRoundUpForTransactionAsync()`: Bearbeta round-up för transaktion
- `ProcessSalaryAutoSaveAsync()`: Bearbeta lön-baserat sparande
- `GetStatisticsAsync()`: Hämta statistik

### Integration med transaktioner

Round-up bearbetas automatiskt när en transaktion skapas via `TransactionService.CreateTransactionAsync()`. Tjänsten:
1. Skapar transaktionen
2. Kontrollerar om round-up är aktiverat
3. Bearbetar round-up eller lön-sparande baserat på transaktionstyp
4. Uppdaterar sparmålets saldo

## Exempel på användning

### Scenario 1: Grundläggande round-up
```
Användaren köper mat för 347 kr
→ Avrundar till 350 kr
→ Sparar 3 kr i sparmålet
```

### Scenario 2: Med arbetsgivarmatchning (100%)
```
Användaren köper mat för 347 kr
→ Avrundar till 350 kr (3 kr sparas)
→ Arbetsgivaren matchar med 3 kr
→ Totalt sparat: 6 kr
```

### Scenario 3: Lön-sparande (10%)
```
Användaren får lön på 25 000 kr
→ 10% sparas automatiskt
→ 2 500 kr läggs till i sparmålet
```

## Testning

Enhetstester finns i `tests/Privatekonomi.Core.Tests/RoundUpServiceTests.cs`:
- 15 tester som täcker alla huvudfunktioner
- Testar beräkningar, arbetsgivarmatchning, lön-sparande och statistik

## Framtida förbättringar

Potentiella förbättringar som kan göras:
- Möjlighet att välja olika sparmål för olika typer av sparande
- Schemalägga när arbetsgivarmatchning ska ske
- Visualisera sparande över tid med diagram
- Exportera round-up historik
- Notifikationer när milstolpar nås
- Jämföra med andra användare (anonymt)
