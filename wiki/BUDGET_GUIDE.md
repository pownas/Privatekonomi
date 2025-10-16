# Budget-funktionalitet - Användarguide

## Översikt

Budget-funktionaliteten i Privatekonomi hjälper dig att planera och följa upp din ekonomi genom att sätta upp budgetar för olika kategorier och sedan jämföra faktiskt utfall mot planerade belopp.

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
4. Ange planerade belopp för varje kategori:
   - Mat & Dryck
   - Transport
   - Boende
   - Nöje
   - Shopping
   - Hälsa
   - Lön
   - Sparande
   - Övrigt
5. Klicka på **Skapa** för att spara budgeten

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

1. **Realistiska budgetar**: Börja med att titta på dina historiska utgifter för att sätta realistiska budgetar
2. **Månadsbudgetar**: Använd månadsbudgetar för bättre kontroll och uppföljning
3. **Regelbunden uppföljning**: Följ upp din budget minst en gång i veckan
4. **Justera efter behov**: Var inte rädd för att justera budgeten baserat på faktiskt utfall
5. **Säkerhetsmarginal**: Lägg till 5-10% säkerhetsmarginal på varje kategori

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

## Framtida Förbättringar

Planerade förbättringar för budget-funktionaliteten:
- [ ] Kopiera budget från föregående period
- [ ] Budgetmallar för vanliga scenarion
- [ ] Notifieringar när budget närmar sig gränsen
- [ ] Diagram och grafer för budgetvisualisering
- [ ] Export av budgetrapporter till Excel/PDF
- [ ] Budgetjämförelse mellan perioder
- [ ] Automatiska budgetförslag baserat på historik
