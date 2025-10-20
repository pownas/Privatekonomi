# Goals Feature Implementation

## Översikt

Sparmålsfunktionaliteten (Goals) har implementerats komplett enligt OpenAPI-specifikationen.

## Vad som implementerades

### 1. Datamodell (`Goal.cs`)

```csharp
public class Goal
{
    public int GoalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime TargetDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public GoalStatus Status { get; set; }
    public string Color { get; set; } = "#2196F3";
}

public enum GoalStatus
{
    Active,
    Completed,
    Cancelled
}
```

### 2. Service-lager

**IGoalService** - Interface med metoder:
- `GetAllGoalsAsync()` - Hämta alla sparmål
- `GetGoalByIdAsync(int id)` - Hämta specifikt sparmål
- `CreateGoalAsync(Goal goal)` - Skapa nytt sparmål
- `UpdateGoalAsync(Goal goal)` - Uppdatera sparmål
- `DeleteGoalAsync(int id)` - Ta bort sparmål
- `GetActiveGoalsAsync()` - Hämta endast aktiva sparmål
- `GetTotalProgress()` - Beräkna total framsteg i procent

**GoalService** - Implementering med:
- CRUD-operationer
- Automatisk sättning av CreatedDate och Status vid skapande
- Progress-beräkning baserad på CurrentAmount/TargetAmount

### 3. API Controller (`GoalsController.cs`)

**Endpoints:**

```
GET    /api/goals              - Lista alla sparmål
GET    /api/goals/{id}         - Hämta specifikt sparmål
GET    /api/goals/active       - Hämta aktiva sparmål
GET    /api/goals/progress     - Hämta total framsteg (i %)
POST   /api/goals              - Skapa nytt sparmål
PUT    /api/goals/{id}         - Uppdatera sparmål
DELETE /api/goals/{id}         - Ta bort sparmål
```

Alla endpoints har:
- Korrekt felhantering
- Logging
- HTTP statuskoder (200, 201, 204, 404, 500)

### 4. Databaskonfiguration

**PrivatekonomyContext** uppdaterad med:
- `DbSet<Goal> Goals` - DbSet för sparmål
- Entity configuration med precision, maxlängd och constraints
- Korrekt mappning av alla fält

### 5. Dependency Injection

**Program.cs** uppdaterad med:
```csharp
builder.Services.AddScoped<IGoalService, GoalService>();
```

### 6. Dokumentation

Uppdaterade filer:
- **API_IMPROVEMENTS.md** - Komplett dokumentation med exempel
- **IMPLEMENTATION_SUMMARY.md** - Status ändrad från "Placeholder" till "Komplett"
- **QUICK_REFERENCE.md** - Snabbreferens med alla endpoints
- **api-tests.sh** - Test för active och progress endpoints

## Användningsexempel

### Skapa sparmål
```bash
POST /api/goals
Content-Type: application/json

{
  "name": "Semesterresa",
  "description": "Resa till Italien sommaren 2025",
  "targetAmount": 50000.00,
  "currentAmount": 0,
  "targetDate": "2025-06-01",
  "color": "#2196F3"
}
```

### Uppdatera framsteg
```bash
PUT /api/goals/1
Content-Type: application/json

{
  "goalId": 1,
  "name": "Semesterresa",
  "description": "Resa till Italien sommaren 2025",
  "targetAmount": 50000.00,
  "currentAmount": 12500.00,
  "targetDate": "2025-06-01",
  "createdDate": "2024-10-19T00:00:00",
  "status": "Active",
  "color": "#2196F3"
}
```

### Hämta framsteg
```bash
GET /api/goals/progress

# Svar:
{
  "progress": 25.0
}
```

## Tester

Test-scriptet inkluderar:
```bash
# Testa alla goals endpoints
GET /api/goals              # 200 OK
GET /api/goals/active       # 200 OK
GET /api/goals/progress     # 200 OK
```

Kör testerna:
```bash
./tests/api-tests.sh
```

## Tekniska detaljer

### Status-hantering
- Nya mål får automatiskt status `Active`
- Status kan ändras via PUT till `Completed` eller `Cancelled`
- Endast `Active` goals inkluderas i progress-beräkningen

### Progress-beräkning
```csharp
progress = (totalCurrentAmount / totalTargetAmount) * 100
```

Returnerar 0 om inga aktiva mål finns eller totalTargetAmount är 0.

### Färger
- Default färg: `#2196F3` (Material Design Blue)
- Färgen används för visualisering i frontend

## Verifiering

✅ Build: Lyckades utan errors  
✅ Alla endpoints fungerar  
✅ Service-lager implementerat  
✅ Dokumentation uppdaterad  
✅ Tester tillagda  
✅ Dependency injection konfigurerad  
✅ Databaskonfiguration korrekt  

## Framtida förbättringar (optional)

- Koppling till transaktioner för automatisk uppdatering av CurrentAmount
- Notifikationer när mål nås
- Milestones/delmål
- Kategori-koppling för automatisk allokering
- Statistik och historik
