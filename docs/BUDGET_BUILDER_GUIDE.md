# Budgetbyggare - Användarguide

## Översikt

Budgetbyggaren är ett smart verktyg som hjälper dig skapa en budget baserad på beprövade fördelningsmodeller som 50/30/20-regeln. Verktyget ger dig förslag på hur du ska fördela din inkomst och låter dig justera och finjustera efter dina behov.

## Funktioner

### Tillgängliga fördelningsmodeller

#### 50/30/20-regeln
Den klassiska och populära budgetmodellen:
- **50% Behov (Needs)**: Boende, mat, transport, försäkringar, hälsa
- **30% Önskemål (Wants)**: Nöje, shopping, restaurang, hobbies
- **20% Sparande (Savings)**: Sparande, investeringar, pension

#### Svenska Familjehushåll
Baserad på Länsförsäkringar's mall för svenska familjer:
- **30% Boende**: Hyra/lån, el, försäkringar
- **15% Sparande**: Behandlas som fast månadskostnad
- **15% Mat (butik)**: Dagligvaruhandel
- **5% Mat (restaurang)**: Utemat, lunch
- **8% Transport**: Bil, kollektivtrafik
- **5% Barn/Fritidsaktiviteter**: Hobbys, sport
- **4% Nöje**: Underhållning, streaming
- **4% Shopping**: Kläder, presenter
- **3% Hälsa**: Träning, friskvård
- **6% Buffert**: Oförutsedda utgifter

#### Svenska Singelhushåll
Anpassad för ensamstående:
- **28% Boende**: Lägre för en person
- **20% Sparande**: Högre sparkvot för singlar
- **12% Mat (butik)**: Lägre för en person
- **6% Mat (restaurang)**: Något högre socialt
- **7% Transport**
- **5% Nöje**
- **6% Shopping**
- **4% Hälsa**
- **9.5% Buffert**: Större buffert

#### 80/20-regeln
Enkel modell som prioriterar sparande:
- **80% Utgifter**: Alla levnadskostnader
- **20% Sparande**: Direkt till sparande

#### 70/20/10-regeln
Inkluderar välgörenhet eller extra amorteringar:
- **70% Behov och önskemål**
- **20% Sparande**
- **10% Välgörenhet/Amortering**

#### Zero-based budgeting
Varje krona tilldelas ett specifikt syfte. Perfekt för full kontroll.

#### Kuvertbudget
Strikta gränser per kategori. När pengarna är slut, inget mer spenderande.

## Hur du använder Budgetbyggaren

### Steg 1: Välj modell och ange inkomst

1. Navigera till **Budget** → **Budgetbyggare** i menyn
2. Ange din månadsinkomst (netto)
3. Välj eventuellt vilket hushåll budgeten ska gälla för
4. Välj en fördelningsmodell genom att klicka på ett kort
5. Klicka på **Skapa förslag**

### Steg 2: Justera förslaget

Efter att förslaget skapats kan du:

#### Se sammanfattning
- Total inkomst
- Summa per kategori (Behov, Önskemål, Sparande)
- Procent per kategori

#### Överföra mellan kategorier
1. Expandera "Överför mellan kategorier"
2. Välj källkategori (Från)
3. Välj målkategori (Till)
4. Ange belopp
5. Klicka **Överför**

#### Justera enskilda kategorier
- Ändra belopp direkt i tabellen
- Systemet uppdaterar procentsatser automatiskt
- Klicka på återställningsikonen för att återgå till ursprungligt förslag

#### Kontrollera balans
Systemet visar:
- 🟢 Grönt: Perfekt fördelat (inkomst = utgifter)
- 🟡 Gult: Pengar kvar att fördela
- 🔴 Rött: Överfördelat (mer än inkomsten)

### Steg 3: Granska effekter och skapa budget

1. Se detaljerad jämförelse mellan ursprungligt förslag och dina justeringar
2. Granska förändringar per kategori
3. Ange datum för budgetperioden
4. Välj om det ska vara månads- eller årsbudget
5. Klicka **Skapa Budget**

## Tips för en bra budget

### Prioritera sparande
- Se sparande som en fast månadskostnad
- "Betala dig själv först" - spara i början av månaden
- Sikta på minst 10-20% av inkomsten

### Dela upp matkostnader
- Separera mat i butik från restaurangmat
- Ger bättre kontroll över "lyxkonsumtion"

### Använd buffert
- Ha alltid 5-10% för oväntade utgifter
- Bygger ekonomisk trygghet

### Gör årskostnader månatliga
- Dela upp årliga kostnader (försäkringar, gymkort) per månad
- Lägg undan pengar varje månad för stora årliga utgifter

## API-dokumentation

### Endpoints

#### Hämta förslag
```
GET /api/budget-suggestions
```
Returnerar alla budget-förslag för inloggad användare.

#### Skapa förslag
```
POST /api/budget-suggestions
Body: {
  "totalIncome": 30000,
  "distributionModel": 0,  // 0 = FiftyThirtyTwenty
  "name": "Mitt förslag",
  "householdId": null
}
```

#### Hämta tillgängliga modeller
```
GET /api/budget-suggestions/models
```
Returnerar alla tillgängliga fördelningsmodeller med beskrivningar.

#### Justera post
```
PUT /api/budget-suggestions/{id}/items/{categoryId}
Body: {
  "newAmount": 5000,
  "reason": "Justerad för högre hyra"
}
```

#### Överför mellan kategorier
```
POST /api/budget-suggestions/{id}/transfer
Body: {
  "fromCategoryId": 1,
  "toCategoryId": 2,
  "amount": 500,
  "reason": "Mer till sparande"
}
```

#### Beräkna effekter
```
GET /api/budget-suggestions/{id}/effects
```
Returnerar detaljerad jämförelse mellan ursprungligt och justerat förslag.

#### Godkänn och skapa budget
```
POST /api/budget-suggestions/{id}/accept
Body: {
  "startDate": "2025-01-01",
  "endDate": "2025-01-31",
  "period": 0  // 0 = Monthly
}
```

### Datamodeller

#### BudgetDistributionModel
```
FiftyThirtyTwenty = 0    // 50/30/20-regeln
ZeroBased = 1            // Zero-based budgeting
Envelope = 2             // Kuvertbudget
SwedishFamily = 3        // Svenska Familjehushåll
SwedishSingle = 4        // Svenska Singelhushåll
EightyTwenty = 5         // 80/20-regeln
SeventyTwentyTen = 6     // 70/20/10-regeln
```

#### BudgetAllocationCategory
```
Needs = 0     // Behov (boende, mat, transport)
Wants = 1     // Önskemål (nöje, shopping)
Savings = 2   // Sparande
Giving = 3    // Välgörenhet (för 70/20/10)
```

## Relaterad dokumentation

- [Budget-guide (BUDGET_GUIDE.md)](./BUDGET_GUIDE.md)
- [Konsumentverket Jämförelse](./KONSUMENTVERKET_JAMFORELSE.md)
- [Svenska Budgetmallar](./IMPLEMENTATION_SUMMARY_SWEDISH_BUDGETS.md)
