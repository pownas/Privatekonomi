# Budget-funktionalitet - Användarguide

## Översikt

Budget-funktionaliteten i Privatekonomi hjälper dig att planera och följa upp din ekonomi genom att sätta upp budgetar för olika kategorier och sedan jämföra faktiskt utfall mot planerade belopp.

Denna guide baseras på bästa praxis från Länsförsäkringar och Konsumentverket för att hjälpa dig skapa en lyckad hushållsbudget.

![Budgethantering](https://github.com/user-attachments/assets/dbd0d556-e37a-43df-99fb-a99f09ffdd40)
*Översikt över budgetar med aktiva och avslutade budgetar*

## Hur gör man en lyckad hushållsbudget?

Att ta kontroll över din privatekonomi behöver inte vara så jobbigt som du kanske tror. Här är de viktigaste stegen:

### 1. Ta med dina fasta månadskostnader

- Gör en klassisk uppställning med alla fasta månadskostnader som exempelvis din elräkning, hemförsäkring och dina streamingtjänster.
- **Viktigt:** Se sparandet som en månadskostnad, och ha med det i budgeten. Sparandet ska alltid dras i början av månaden så att det inte bortprioriteras.

### 2. Dela upp rörliga månadskostnader som matinköp

- När det gäller rörliga kostnader som matinköp är det bäst att dela upp matkostnaderna i två olika delar: en för maten du handlar i butik och en för maten du handlar på restaurang. Kanske inser du att det har blivit lite väl många uteluncher?
- Många fasta kostnader är saker som du måste ha, så det är oftast de rörliga kostnaderna som du får laborera med om budgeten inte går ihop.

### 3. Gör varje månad kostnadsneutral

- Få en bättre överblick genom att göra varje månad kostnadsneutral, alltså att du delar upp årskostnader månadsvis.
- Ett exempel är en fritidsaktivitet till barnen som kostar 2 400 kronor per år. Då delar du den kostnaden med tolv, så att du lägger undan 200 kronor i månaden för att vara säker på att du kan betala fakturan.
- Det kan också handla om ett årskort på gymmet, eller vilken annan årskostnad som helst.
- I Privatekonomi använder du **periodväljaren** för varje kategori för att automatiskt dela upp årskostnader.

## Funktioner

### Skapa Budget

1. Navigera till **Budget** i menyn
2. Klicka på **Ny Budget**
3. Fyll i följande information:
   - **Budgetnamn**: Ett beskrivande namn för din budget (t.ex. "Oktober 2025")
   - **Period**: Välj mellan Månadsbudget eller Årsbudget
   - **Startdatum**: När budgeten börjar gälla (format: ÅÅÅÅ-MM-DD)
   - **Slutdatum**: När budgeten slutar gälla (format: ÅÅÅÅ-MM-DD)
   - **Beskrivning**: Valfri beskrivning av budgeten

4. **Välj budgetmall** (Rekommenderat!):
   - **Svenska Familjehushåll**: Baserad på Länsförsäkringar's mall för familjer med barn. Inkluderar 15% sparande, separata kostnader för mat i butik vs restaurang, och rimliga fördelningar för en svensk familj.
   - **Svenska Singelhushåll**: För ensamstående med högre sparkvot (20%) och lägre fasta kostnader.
   - **50/30/20-regeln**: Klassisk budgetmodell med 50% behov, 30% önskemål, 20% sparande.
   - **Zero-based budgeting**: Varje krona tilldelas ett syfte.
   - **Kuvertbudget**: Strikta gränser per kategori.
   - **Anpassad**: Tom mall där du själv fyller i alla belopp.

5. Ange din totala månadsinkomst och klicka **Använd mall** för att automatiskt fördela budgeten.

6. Justera budgetposterna efter behov:
   - För varje kategori, välj **period** (månad, kvartal, halvår, eller helår)
   - Ange belopp för den valda perioden
   - Systemet beräknar automatiskt månadskostnaden

7. Klicka på **Skapa** för att spara budgeten

### Visa Budgetar

På Budget-sidan visas alla dina budgetar med:
- **Budgetnamn** och period
- **Status**: 
  - **Aktiv** - Budgeten är aktiv just nu (dagens datum ligger mellan start- och slutdatum)
  - **Kommande** - Budgeten startar i framtiden
  - **Avslutad** - Budgeten har passerat sitt slutdatum

### Budgetdetaljer

Klicka på en budget för att expandera och se detaljerad information:

#### Sammanfattning
- **Total Planerad**: Summan av alla planerade budgetposter
- **Faktiskt Utfall**: Summan av faktiska utgifter under budgetperioden
- **Kvar att Spendera**: Skillnaden mellan planerat och faktiskt (positivt värde = du har pengar kvar)

#### Budgetposter per kategori

En tabell som visar varje kategori med:
- **Kategori**: Kategorinamn med färgkodning
- **Planerat**: Det belopp du planerat för kategorin
- **Faktiskt**: Faktiskt utfall baserat på dina transaktioner
- **Differens**: Skillnad mellan planerat och faktiskt
  - 🟢 Grönt = Under budget
  - 🔴 Rött = Över budget
- **Progress**: Visuell progress bar som visar hur stor del av budgeten som använts
  - 0-75% = Grön (bra)
  - 75-100% = Orange (varning)
  - >100% = Röd (över budget)

### Ta bort Budget

1. Klicka på papperskorgen-ikonen bredvid budgetens namn
2. Bekräfta att du vill ta bort budgeten

## Integration med Transaktioner

Budget-funktionaliteten integreras automatiskt med dina befintliga transaktioner:
- Transaktioner som faller inom budgetens datumintervall räknas in i det faktiska utfallet
- Endast utgifter (negativa transaktioner) räknas mot budgeten
- Transaktioner med split-kategorisering fördelas korrekt på respektive budgetpost

## Tips och Bästa Praxis

### Grundläggande budgettips

1. **Realistiska budgetar**: Börja med att titta på dina historiska utgifter för att sätta realistiska budgetar
2. **Månadsbudgetar**: Använd månadsbudgetar för bättre kontroll och uppföljning
3. **Regelbunden uppföljning**: Följ upp din budget minst en gång i veckan
4. **Justera efter behov**: Var inte rädd för att justera budgeten baserat på faktiskt utfall
5. **Säkerhetsmarginal**: Lägg till 5-10% säkerhetsmarginal på varje kategori

### Svenska hushållsbudget-tips (från Länsförsäkringar)

1. **Sparande först**: Se sparandet som en fast månadskostnad som betalas först varje månad, inte som något du sparar "om det blir pengar över". Lägg undan minst 10-15% av inkomsten för familjer, 15-20% för singelhushåll.

2. **Dela upp matkostnader**: 
   - Ha en budgetpost för mat i butik (ca 12-15% av inkomsten för familjer)
   - Ha en separat post för restaurang/utemat (ca 5-6% av inkomsten)
   - Detta hjälper dig se hur mycket som går till "lyxkonsumtion" vs basutgifter

3. **Årskostnader månadsvis**:
   - Barnens fritidsaktiviteter (2 400 kr/år = 200 kr/månad)
   - Årskort på gym (5 000 kr/år ≈ 417 kr/månad)
   - Försäkringar som betalas årligen
   - Använd periodväljaren i budgeten för att automatiskt dela upp dessa

4. **Fasta vs rörliga kostnader**:
   - **Fasta**: Hyra/boende (30%), försäkringar (2-3%), el (1.5-2%), sparande (15-20%)
   - **Rörliga**: Mat (12-15%), transport (7-8%), nöje (4-5%), shopping (4-6%)
   - De rörliga kostnaderna är det du kan justera om budgeten inte går ihop

5. **Buffert för oförutsett**: Lägg alltid in en buffert på 5-10% för oväntade utgifter

### Jämför med Konsumentverket

Använd [Konsumentverkets budgetkalkyl](https://budgetkalkylen.konsumentverket.se/budgetkalkylen-berakna/?bkName=Min+budgetkalkyl#content) för att se vad som är rimliga kostnader för din hushållstyp och var du bor i Sverige. Privatekonomi's svenska budgetmallar baseras på liknande fördelningar.

## Teknisk Information

### API Endpoints

Budget-funktionaliteten exponerar följande REST API endpoints:

- `GET /api/budgets` - Hämta alla budgetar
- `GET /api/budgets/{id}` - Hämta en specifik budget
- `GET /api/budgets/active` - Hämta aktiva budgetar
- `GET /api/budgets/{id}/actual-amounts` - Hämta faktiska belopp per kategori för en budget
- `POST /api/budgets` - Skapa ny budget
- `PUT /api/budgets/{id}` - Uppdatera budget
- `DELETE /api/budgets/{id}` - Ta bort budget

### Datamodell

#### Budget
```csharp
public class Budget
{
    public int BudgetId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public BudgetPeriod Period { get; set; }
    public ICollection<BudgetCategory> BudgetCategories { get; set; }
}
```

#### BudgetCategory
```csharp
public class BudgetCategory
{
    public int BudgetCategoryId { get; set; }
    public int BudgetId { get; set; }
    public int CategoryId { get; set; }
    public decimal PlannedAmount { get; set; }
    public int RecurrencePeriodMonths { get; set; } = 1;  // Nytt! För årskostnader
    public decimal MonthlyAmount => RecurrencePeriodMonths > 0 ? PlannedAmount / RecurrencePeriodMonths : PlannedAmount;
    public Budget Budget { get; set; }
    public Category Category { get; set; }
}
```

#### BudgetPeriod
```csharp
public enum BudgetPeriod
{
    Monthly,  // Månadsbudget
    Yearly    // Årsbudget
}
```

#### BudgetTemplateType
```csharp
public enum BudgetTemplateType
{
    Custom = 0,              // Anpassad budget
    ZeroBased = 1,          // Zero-based budgeting
    FiftyThirtyTwenty = 2,  // 50/30/20-regeln
    Envelope = 3,           // Kuvertbudget
    SwedishFamily = 4,      // Svenska familjehushåll (Länsförsäkringar-stil) - NYA!
    SwedishSingle = 5       // Svenska singelhushåll (Länsförsäkringar-stil) - NYA!
}
```

## Budgetmallar i detalj

### Svenska Familjehushåll (Rekommenderad för familjer)

Denna mall baseras på Länsförsäkringar's riktlinjer för svenska familjer:

- **Boende**: 30% (hyra/lån, el, försäkringar)
- **Sparande**: 15% (behandlas som fast månadskostnad, prioriteras!)
- **Mat (butik)**: 15% (dagligvaruhandel)
- **Mat (restaurang)**: 5% (utemat, lunch på jobbet)
- **Transport**: 8% (bil, kollektivtrafik, bensin)
- **Barn/Fritidsaktiviteter**: 5% (hobbys, sport, aktiviteter)
- **Nöje/Streaming**: 4% (underhållning, prenumerationer)
- **Shopping/Kläder**: 4% (kläder, skor, presenter)
- **Hälsa/Gym**: 3% (träning, friskvård)
- **Buffert/Övrigt**: 6% (oförutsedda utgifter)

**Total**: 100% av månadsinkomsten

**Särskilda funktioner**:
- Sparande prioriteras som en fast månadskostnad
- Separation av mat i butik vs restaurang
- Inkluderar budgetposter för barn och fritidsaktiviteter

### Svenska Singelhushåll (Rekommenderad för ensamstående)

Denna mall är anpassad för ensamstående med lägre fasta kostnader och högre sparkvot:

- **Boende**: 28% (lägre för en person)
- **Sparande**: 20% (högre sparkvot möjlig för singlar)
- **Mat (butik)**: 12% (lägre för en person)
- **Mat (restaurang)**: 6% (något högre andel socialt)
- **Transport**: 7%
- **Nöje/Streaming**: 5%
- **Shopping/Kläder**: 6%
- **Hälsa/Gym**: 4%
- **Buffert/Övrigt**: 9.5%

**Total**: 100% av månadsinkomsten

**Särskilda funktioner**:
- Högre sparkvot (20%) för att bygga ekonomisk trygghet snabbare
- Justerade kategorier för en persons behov
- Mer utrymme för personlig utveckling och sociala aktiviteter

#### BudgetPeriod
```csharp
public enum BudgetPeriod
{
    Monthly,  // Månadsbudget
    Yearly    // Årsbudget
}
```

## Framtida Förbättringar

Planerade förbättringar för budget-funktionaliteten:
- [ ] Kopiera budget från föregående period
- [ ] Budgetmallar för vanliga scenarion
- [ ] Notifieringar när budget närmar sig gränsen
- [ ] Diagram och grafer för budgetvisualisering
- [ ] Export av budgetrapporter till Excel/PDF
- [ ] Budgetjämförelse mellan perioder
- [ ] Automatiska budgetförslag baserat på historik
