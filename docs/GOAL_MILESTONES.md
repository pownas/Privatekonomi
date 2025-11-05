# Målstolpar för Sparmål (Goal Milestones)

## Översikt

Målstolpar är delmål som automatiskt skapas för varje sparmål för att hjälpa dig att hålla motivationen uppe och fira framsteg längs vägen. När du når en målstolpe får du en notifikation och kan se din historia över uppnådda milstolpar.

## Funktioner

### Automatiska Målstolpar

När du skapar ett nytt sparmål skapas automatiskt fyra målstolpar:
- **25%** - Första kvartilen
- **50%** - Halvvägs!
- **75%** - Trekvartsdelen klar
- **100%** - Målet uppnått!

### Anpassade Målstolpar

Du kan också lägga till egna anpassade målstolpar för specifika belopp eller procentsatser som är viktiga för dig.

### Notifikationer

När du når en målstolpe får du automatiskt en notifikation som firar din framgång! 🎉

### Visualisering

I sparmålsvyn kan du:
- Se hur många målstolpar du uppnått (t.ex. "2/4")
- Expandera målet för att se alla målstolpar
- Se när varje målstolpe uppnåddes
- Se hur mycket som återstår till nästa målstolpe

## Användning

### Visa Målstolpar

1. Navigera till **Sparmål** (`/goals`)
2. Du ser automatiskt en kolumn "Milestones" som visar antal uppnådda/totala milestones
3. Klicka på en rad för att expandera och se alla målstolpar för det målet

### Målstolpe-Information

För varje målstolpe visas:
- ✓ Grön bock om målstolpen är uppnådd
- ○ Tom cirkel om målstolpen inte är uppnådd än
- Procentsats och belopp (t.ex. "25% (15,000 kr)")
- Beskrivning (för automatiska: "25% av målet uppnått")
- **Om uppnådd**: Datum och tid när målstolpen nåddes
- **Om ej uppnådd**: Kvarstående belopp till målstolpen

## Teknisk Implementation

### Datamodell

```csharp
public class GoalMilestone
{
    public int GoalMilestoneId { get; set; }
    public int GoalId { get; set; }
    public decimal TargetAmount { get; set; }
    public int Percentage { get; set; }
    public string? Description { get; set; }
    public bool IsReached { get; set; }
    public DateTime? ReachedAt { get; set; }
    public bool IsAutomatic { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Service-Metoder

#### IGoalMilestoneService

- `GetMilestonesByGoalIdAsync(int goalId)` - Hämta alla milestones för ett mål
- `CreateAutomaticMilestonesAsync(int goalId)` - Skapa automatiska milestones (25%, 50%, 75%, 100%)
- `CreateCustomMilestoneAsync(GoalMilestone milestone)` - Skapa en anpassad milestone
- `CheckAndUpdateMilestonesAsync(int goalId, decimal currentAmount)` - Kontrollera och uppdatera milestones baserat på nuvarande belopp
- `GetReachedMilestonesAsync(int goalId)` - Hämta alla uppnådda milestones (historik)
- `DeleteMilestoneAsync(int milestoneId)` - Ta bort en milestone
- `GetMilestoneByIdAsync(int milestoneId)` - Hämta specifik milestone

### Automatisk Kontroll

Varje gång ett sparmåls progress uppdateras genom `GoalService.UpdateGoalProgressAsync()`:
1. Kontrolleras automatiskt om några nya milestones har uppnåtts
2. De markeras som uppnådda med aktuell tidstämpel
3. Notifikationer skickas för varje nådd milestone

## Exempel

### Exempel: Buffert-sparmål

```
Buffert - 45,000 kr av 60,000 kr (75%)
────────────────────────────────────
✓ 25% (15,000 kr) - 2024-10-15 14:30
✓ 50% (30,000 kr) - 2024-11-20 09:15
✓ 75% (45,000 kr) - 2025-01-05 16:45
○ 100% (60,000 kr) - Återstår: 15,000 kr
```

## Integration

Målstolpar integreras automatiskt med:
- **GoalService**: Milestones skapas vid nytt mål, kontrolleras vid uppdatering
- **NotificationService**: Skickar notifikationer när milestones uppnås
- **Goals.razor**: Visar milestones i UI med expanderbar rad

## Testning

Fullständig testsvit finns i `GoalMilestoneServiceTests.cs` med 10 test som validerar:
- Skapande av automatiska milestones
- Anpassade milestones
- Kontroll och uppdatering av uppnådda milestones
- Notifikationer
- Historik
- CRUD-operationer

## Framtida Förbättringar

Potentiella förbättringar för framtiden:
- Badge/achievement-system för uppnådda milestones
- Visualisering av milestone-progress i en tidslinje
- Statistik över genomsnittlig tid mellan milestones
- Påminnelser när du närmar dig nästa milestone
- Möjlighet att ändra beskrivning på automatiska milestones
- Export av milestone-historik
