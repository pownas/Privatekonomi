# Algoritmer och Formler för Skuld- och Lånesimulering

Denna dokumentation beskriver de algoritmer och matematiska formler som används i Privatekonomi för skuld- och lånesimulering.

## Översikt

Systemet implementerar två beprövade metoder för skuldåterbetalning:
- **Snöbollsmetoden (Snowball)**: Betala av minsta skulden först
- **Lavinmetoden (Avalanche)**: Betala av skulden med högst ränta först

## Grundläggande Beräkningar

### Månadsränta

Månadsräntan beräknas från årsräntan:

```
Månadsränta = (Årsränta / 100) / 12
```

**Exempel:**
- Årsränta: 5%
- Månadsränta: (5 / 100) / 12 = 0.00416667 (≈ 0.417%)

### Räntekostnad per Månad

För en given skuld beräknas månadens räntekostnad:

```
Räntekostnad = Aktuellt Saldo × Månadsränta
```

**Exempel:**
- Saldo: 100 000 kr
- Månadsränta: 0.00416667
- Räntekostnad: 100 000 × 0.00416667 = 416.67 kr

### Amortering per Månad

Amorteringen är den del av betalningen som minskar skulden:

```
Amortering = Total Betalning - Räntekostnad
```

**Exempel:**
- Total betalning: 2 000 kr
- Räntekostnad: 416.67 kr
- Amortering: 2 000 - 416.67 = 1 583.33 kr

### Nytt Saldo

Efter betalning beräknas det nya saldot:

```
Nytt Saldo = Gammalt Saldo - Amortering
```

## Amorteringsplan (Amortization Schedule)

En amorteringsplan visar hur ett lån betalas av över tid, månad för månad.

### Algoritm

```csharp
Saldo = Lånebelopp
Månadsränta = Årsränta / 100 / 12
Månadsnummer = 1

while (Saldo > 0 och Månadsnummer <= MaxMånader):
    Räntekostnad = Saldo × Månadsränta
    
    if ExtraBetalning > 0:
        TotalBetalning = RegularBetalning + ExtraBetalning
    else:
        TotalBetalning = RegularBetalning
    
    Amortering = min(TotalBetalning - Räntekostnad, Saldo)
    
    Nytt Saldo = Saldo - Amortering
    Total Ränta += Räntekostnad
    
    // Spara denna månads data i schemat
    Schemat.Add(ny Post med alla värden)
    
    Saldo = Nytt Saldo
    Månadsnummer++
```

### Formel för Total Kostnad

Den totala kostnaden för ett lån:

```
Total Kostnad = Lånebelopp + Total Ränta
```

## Snöbollsmetoden (Snowball Method)

### Princip

Betala av minsta skulden först för psykologiska vinster och momentum. När en skuld är betald, lägg den frigjorda betalningen på nästa minsta skuld.

### Algoritm

1. **Sortering**: Sortera alla skulder efter belopp (minsta först)
2. **Fokus**: Välj skulden med minsta beloppet som fokusskuld
3. **Minimibetalningar**: Betala minimumbelopp på alla skulder
4. **Extra betalning**: Lägg all extra betalning på fokusskulden
5. **När betald**: När fokusskulden är betald, lägg den frigjorda betalningen till extra betalningar
6. **Upprepa**: Välj nästa minsta skuld som ny fokusskuld

### Pseudokod

```
Totalt Tillgängligt = AnvändarensInmatning
Skulder = Sortera(AllaSkulder, Efter: Belopp, Stigande)

Extra = Totalt Tillgängligt - Summan(AllaSkulder.Minimumbetalning)

för varje Månad:
    Fokusskuld = Skulder.Första()
    
    för varje Skuld i Skulder:
        Ränta = Skuld.Saldo × Skuld.Månadsränta
        
        if Skuld == Fokusskuld:
            Betalning = Skuld.Minimumbetalning + Extra + Ränta
        else:
            Betalning = Skuld.Minimumbetalning + Ränta
        
        Amortering = min(Betalning - Ränta, Skuld.Saldo)
        Skuld.Saldo -= Amortering
        
        if Skuld.Saldo <= 0:
            Extra += Skuld.Minimumbetalning
            Ta bort Skuld från Skulder
```

### Fördelar

- **Psykologisk motivation**: Ser snabba resultat när små skulder försvinner
- **Momentum**: Bygger självförtroende och disciplin
- **Enklare att hålla fast vid**: Konkreta milstolpar

### Nackdelar

- **Kan vara dyrare**: Om stora skulder har höga räntor betalas mer i total ränta

## Lavinmetoden (Avalanche Method)

### Princip

Betala av skulden med högst ränta först för att minimera total räntekostnad. Matematiskt mest optimal metod.

### Algoritm

Identisk med Snöbollsmetoden men med annan sorteringsordning:

1. **Sortering**: Sortera alla skulder efter ränta (högsta först)
2. **Fokus**: Välj skulden med högsta räntan som fokusskuld
3. **Resten**: Samma som Snöbollsmetoden

### Pseudokod

```
Totalt Tillgängligt = AnvändarensInmatning
Skulder = Sortera(AllaSkulder, Efter: Ränta, Fallande)  // Skillnaden här!

// Resten är identisk med Snöbollsmetoden
Extra = Totalt Tillgängligt - Summan(AllaSkulder.Minimumbetalning)
...
```

### Fördelar

- **Lägsta total kostnad**: Minimerar räntekostnader
- **Matematiskt optimal**: Sparar mest pengar
- **Snabbare skuldfri**: Oftast kortare tid än Snöbollsmetoden

### Nackdelar

- **Långsammare "vinster"**: Kan ta längre tid att betala av första skulden
- **Kräver disciplin**: Mindre psykologiska belöningar tidigt

## Jämförelse av Strategier

### Exempel-scenario

**Skulder:**
1. Kreditkort: 5 000 kr @ 18% ränta, 200 kr/mån minimumbetalning
2. Privatlån: 20 000 kr @ 8% ränta, 500 kr/mån minimumbetalning
3. Studielån: 50 000 kr @ 3% ränta, 1 000 kr/mån minimumbetalning

**Tillgänglig månadsbetalning:** 2 500 kr

### Snöboll-ordning:
1. Kreditkort (5 000 kr) - betald först trots högsta räntan
2. Privatlån (20 000 kr)
3. Studielån (50 000 kr)

**Resultat:**
- Tid till skuldfri: ~36 månader
- Total ränta: ~7 200 kr

### Lavin-ordning:
1. Kreditkort (18%) - betald först pga högsta räntan
2. Privatlån (8%)
3. Studielån (3%)

**Resultat:**
- Tid till skuldfri: ~35 månader
- Total ränta: ~6 800 kr
- **Besparing:** 400 kr jämfört med Snöboll

## Detaljerad Strategi-simulering

### MonthlyStrategyPayment

För varje månad i simuleringen sparas:

```
MonthlyStrategyPayment {
    Månadsnummer: int
    Betalningsdatum: DateTime
    Total Betalning: decimal (summa för alla lån)
    Total Amortering: decimal (summa för alla lån)
    Total Ränta: decimal (summa för alla lån)
    Kvarvarande Total Skuld: decimal
    
    LånBetalningar: List<LoanPaymentDetail>
}
```

### LoanPaymentDetail

För varje lån i varje månad:

```
LoanPaymentDetail {
    Lån-ID: int
    Lånnamn: string
    Ingående Saldo: decimal
    Betalning: decimal (för detta lån denna månad)
    Amortering: decimal (för detta lån denna månad)
    Ränta: decimal (för detta lån denna månad)
    Utgående Saldo: decimal
    Är Fokus Lån: bool (får extra betalning denna månad)
    Är Betald: bool (betald av denna månad)
}
```

## Extrabetalningsanalys

### Algoritm

För att analysera effekten av extra betalningar:

1. Generera amorteringsplan **utan** extra betalning
2. Generera amorteringsplan **med** extra betalning
3. Jämför resultat:

```
MonthsSaved = Antal Betalningar (Original) - Antal Betalningar (Med Extra)
InterestSavings = Total Ränta (Original) - Total Ränta (Med Extra)
TimeSaved = Betalt Datum (Original) - Betalt Datum (Med Extra)
TotalExtraPayments = ExtraBetalning × Antal Betalningar (Med Extra)
NetSavings = InterestSavings - TotalExtraPayments
```

### Exempel

**Original:**
- Lånebelopp: 100 000 kr @ 5%
- Betalning: 2 000 kr/mån
- Antal månader: 56
- Total ränta: 11 200 kr

**Med 500 kr extra:**
- Betalning: 2 500 kr/mån
- Antal månader: 43
- Total ränta: 8 400 kr

**Resultat:**
- Månader sparade: 13
- Räntebesparing: 2 800 kr
- Total extra betalning: 21 500 kr
- Nettobesparing: 2 800 kr (plus 13 månader tidigare skuldfri)

## CSV Export

### Amorteringsplan Format

```csv
Amorteringsplan för: [Lånnamn]
Lånebelopp: [Belopp] kr
Ränta: [Ränta]%
Månatlig amortering: [Amortering] kr

Betalning,Datum,Ingående Saldo,Betalning,Ränta,Amortering,Utgående Saldo,Total Ränta
1,2024-01,100000.00,2000.00,416.67,1583.33,98416.67,416.67
2,2024-02,98416.67,2000.00,410.07,1589.93,96826.74,826.74
...

Sammanfattning
Antal betalningar,[Antal]
Total ränta,[Belopp] kr
Total kostnad,[Belopp] kr
Betalt datum,[Datum]
```

### Strategi Format

```csv
Avbetalningsstrategi: [Snöboll/Lavin]
Beskrivning: [Beskrivning]
Skuldfri datum: [Datum]
Total ränta: [Belopp] kr
Total kostnad: [Belopp] kr
Antal månader: [Antal]

Avbetalningsordning
Ordning,Lån,Belopp,Ränta,Betalt datum,Månader,Total ränta
1,"Lån 1",5000.00,18.00%,2024-06,6,450.00
2,"Lån 2",20000.00,8.00%,2025-02,14,1280.00
...
```

## Implementation Detaljer

### Säkerhetsåtgärder

1. **Max iterations**: Begränsa simuleringar till max 600 månader (50 år)
2. **Avrundning**: Använd tolerans på 0.01 kr för att hantera avrundningsfel
3. **Validering**: Kontrollera att tillgänglig betalning >= minimumbetalningar

### Prestanda

- Amorteringsplaner beräknas i O(n) där n = antal månader
- Strategier beräknas i O(m × n) där m = antal lån, n = antal månader
- CSV export är minneseffektiv med StringBuilder

## Referenser

### Akademiska Källor

- **Debt Snowball vs Avalanche**: Northwestern Mutual Research
- **Consumer Debt Repayment**: Harvard Business School Study
- **Behavioral Economics**: Daniel Kahneman, "Thinking, Fast and Slow"

### Finansiella Standarder

- **Amortization Calculations**: Standard mortgage amortization formulas
- **APR Calculations**: Swedish Consumer Credit Act (Konsumentkreditlagen)
- **Interest Rate Calculations**: Riksbanken guidelines

## Sammanfattning

### Nyckelformler

```
Månadsränta = (Årsränta / 100) / 12
Räntekostnad = Saldo × Månadsränta
Amortering = Betalning - Räntekostnad
Nytt Saldo = Gammalt Saldo - Amortering
Total Kostnad = Lånebelopp + Σ(Räntekostnader)
```

### Algoritmer

1. **Amorteringsplan**: Iterativ beräkning månad-för-månad tills saldo = 0
2. **Snöbollsmetoden**: Sortera efter belopp, fokusera på minsta
3. **Lavinmetoden**: Sortera efter ränta, fokusera på högsta
4. **Simulering**: Applicera strategi över tid med frigjorda betalningar

### Användningsområden

- Planera skuldåterbetalning
- Jämföra återbetalningsstrategier
- Analysera effekten av extra betalningar
- Exportera data för vidare analys
- Fatta informerade ekonomiska beslut

---

**Senast uppdaterad:** 2024
**Version:** 1.0
**Författare:** Privatekonomi Development Team
