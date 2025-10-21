# Automatisk Kategorisering

## Översikt

Privatekonomi-applikationen har nu stöd för **automatisk kategorisering** av transaktioner baserat på regelbaserade mönster. Detta gör det möjligt att automatiskt tilldela kategorier till transaktioner när de skapas, baserat på beskrivning eller betalningsmottagare.

## Funktioner

### 1. Regelbaserad Kategorisering

Systemet använder kategoriseringsregler för att automatiskt tilldela kategorier till transaktioner. Varje regel definierar:

- **Mönster**: Text att matcha mot (t.ex. "ICA", "Spotify", "SL-kort")
- **Matchningstyp**: Hur mönstret ska matchas
  - **Innehåller** (`Contains`): Mönstret kan förekomma var som helst i texten
  - **Exakt matchning** (`Exact`): Texten måste matcha mönstret exakt
  - **Börjar med** (`StartsWith`): Texten måste börja med mönstret
  - **Slutar med** (`EndsWith`): Texten måste sluta med mönstret
  - **Regex** (`Regex`): Mönstret är ett reguljärt uttryck
- **Kategori**: Vilken kategori som ska tilldelas vid matchning
- **Fält att matcha**: Vilket fält som ska matchas (beskrivning, betalningsmottagare, eller båda)
- **Prioritet**: Högre prioritet = regeln utvärderas först (vid flera matchande regler)
- **Status**: Aktiv eller inaktiv
- **Skiftlägeskänslig**: Om matchningen ska vara case-sensitive

### 2. Förladdade Regler

Systemet kommer med 44 förkonfigurerade regler som täcker vanliga svenska transaktioner:

#### Mat & Dryck (Kategori 1)
- ICA, Coop, Willys, Hemköp
- Restaurang, Café, McDonalds, Pizza, Sushi

#### Transport (Kategori 2)
- SL-kort (kollektivtrafik)
- Bensin, Circle K, Preem
- Parkering, Taxi, SJ (tåg)

#### Boende (Kategori 3)
- Hyra
- Vattenfall (el)
- Telia (bredband)
- Försäkring

#### Nöje (Kategori 4)
- Spotify, Netflix
- Bio, SF, Teater, Konsert
- Gym

#### Shopping (Kategori 5)
- H&M, IKEA, Elgiganten
- Clas Ohlson, Stadium, Apoteket

#### Hälsa (Kategori 6)
- Folktandvården
- Apotek, Naprapat
- Vitaminer

#### Lön (Kategori 7)
- Lön, Bonus, Semesterersättning

#### Sparande (Kategori 8)
- Sparande, ISK

### 3. Hantera Regler via UI

Användare kan hantera kategoriseringsregler via webbgränssnittet:

1. Navigera till **Kategoriseringsregler** i menyn
2. Visa befintliga regler med information om:
   - Mönster och beskrivning
   - Matchningstyp och fält
   - Tilldelad kategori
   - Prioritet och status
3. Skapa nya regler genom att klicka på **Ny Regel**
4. Redigera befintliga regler genom att klicka på redigera-ikonen
5. Ta bort regler genom att klicka på radera-ikonen
6. Aktivera/inaktivera regler utan att ta bort dem

### 4. API för Kategoriseringsregler

För integration och automatisering finns följande API-endpoints:

#### Hämta alla regler
```http
GET /api/categoryrules
```

#### Hämta aktiva regler
```http
GET /api/categoryrules/active
```

#### Skapa ny regel
```http
POST /api/categoryrules
Content-Type: application/json

{
  "pattern": "ICA",
  "matchType": "Contains",
  "categoryId": 1,
  "priority": 100,
  "isActive": true,
  "field": "Both",
  "caseSensitive": false,
  "description": "ICA stores"
}
```

#### Uppdatera regel
```http
PUT /api/categoryrules/{id}
Content-Type: application/json

{
  "categoryRuleId": 1,
  "pattern": "ICA",
  ...
}
```

#### Ta bort regel
```http
DELETE /api/categoryrules/{id}
```

#### Testa regel
```http
POST /api/categoryrules/test
Content-Type: application/json

{
  "description": "ICA Maxi Stockholm",
  "payee": "ICA"
}
```

#### Tillämpa regler på transaktioner
```http
POST /api/categoryrules/apply
Content-Type: application/json

[1, 2, 3, 4, 5]
```

### 5. Automatisk Kategorisering vid Import

När transaktioner skapas (manuellt eller via CSV/API-import) tillämpas automatiskt följande logik:

1. **Regel-baserad kategorisering**: Systemet söker efter den första matchande regeln (baserat på prioritet)
2. **Likhetskategorisering**: Om ingen regel matchar, söker systemet efter liknande tidigare transaktioner och föreslår den vanligaste kategorin

Detta säkerställer att nya transaktioner automatiskt får relevanta kategorier tilldelade.

## Teknisk Implementation

### Modeller

**CategoryRule** (`Privatekonomi.Core.Models.CategoryRule`)
- Innehåller alla egenskaper för en kategoriseringsregel
- Inkluderar enum för `PatternMatchType` och `MatchField`

### Services

**ICategoryRuleService** / **CategoryRuleService** (`Privatekonomi.Core.Services`)
- `GetAllRulesAsync()`: Hämta alla regler
- `GetActiveRulesAsync()`: Hämta endast aktiva regler
- `FindMatchingRuleAsync()`: Hitta matchande regel för beskrivning/betalningsmottagare
- `ApplyCategoryRulesAsync()`: Tillämpa regler och returnera kategori-ID
- `ApplyRulesToTransactionsAsync()`: Tillämpa regler på flera transaktioner

**TransactionService** (uppdaterad)
- Integrerar automatisk kategorisering i `CreateTransactionAsync()`
- Använder både regel-baserad och likhetsbaserad kategorisering

### Databas

**PrivatekonomyContext** (uppdaterad)
- Lagt till `DbSet<CategoryRule> CategoryRules`
- Konfigurerat index för snabbare frågor (Priority, IsActive)

## Exempel på Användning

### Exempel 1: Skapa regel för alla ICA-inköp

```csharp
var rule = new CategoryRule
{
    Pattern = "ICA",
    MatchType = PatternMatchType.Contains,
    CategoryId = 1, // Mat & Dryck
    Priority = 100,
    IsActive = true,
    Field = MatchField.Both,
    Description = "Alla ICA-inköp kategoriseras som Mat & Dryck"
};

await categoryRuleService.CreateRuleAsync(rule);
```

### Exempel 2: Skapa regel med regex för specifika bensinmackar

```csharp
var rule = new CategoryRule
{
    Pattern = "(Circle K|Preem|OKQ8)",
    MatchType = PatternMatchType.Regex,
    CategoryId = 2, // Transport
    Priority = 95,
    IsActive = true,
    Field = MatchField.Description,
    Description = "Bensinmackar kategoriseras som Transport"
};

await categoryRuleService.CreateRuleAsync(rule);
```

### Exempel 3: Testa om en beskrivning matchar en regel

```csharp
var matchingRule = await categoryRuleService.FindMatchingRuleAsync(
    "ICA Maxi Stockholm", 
    "ICA");

if (matchingRule != null)
{
    Console.WriteLine($"Matchade regel: {matchingRule.Pattern}");
    Console.WriteLine($"Kategori: {matchingRule.Category.Name}");
}
```

## Framtida Förbättringar

Följande förbättringar kan implementeras för att utöka funktionaliteten:

1. **ML-baserad kategorisering**: Träna en maskininlärningsmodell på historiska transaktioner för smartare kategorisering
2. **Anomali-detektion**: Identifiera ovanliga transaktioner som kan vara bedrägeri
3. **Automatisk reglergenerering**: Föreslå nya regler baserat på användarens manuella kategoriseringar
4. **Regelgrupper**: Organisera regler i grupper för enklare hantering
5. **Import/export av regler**: Dela regler mellan användare eller applikationsinstanser
6. **Statistik**: Visa hur ofta varje regel används och hur effektiv den är

## Support och Bidrag

För frågor, bugrapporter eller funktionsförslag, öppna en issue på GitHub-repositoriet.

För att bidra med nya förladdade regler, uppdatera `TestDataSeeder.SeedCategoryRules()` metoden i `Privatekonomi.Core.Data.TestDataSeeder.cs`.
