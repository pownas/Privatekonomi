# Återkommande uppgifter och Kanban-tavla för Hushåll

## Översikt

Denna funktionalitet utökar "Att göra"-uppgifter i hushållsvyn med två kraftfulla funktioner:

1. **Återkommande uppgifter** - Möjlighet att skapa uppgifter som automatiskt återkommer på önskat intervall
2. **Kanban-tavla** - Organisera uppgifter i ett visuellt workflow med tre kolumner (Att göra, Pågår, Klart)

## Återkommande uppgifter

### Funktionalitet

När du skapar eller redigerar en uppgift kan du nu markera den som återkommande. Detta innebär att när uppgiften markeras som klar, skapas automatiskt en ny instans av uppgiften med ett nytt förfallodatum baserat på det valda återkommande mönstret.

### Återkommande mönster

Följande återkommande mönster stöds:

- **Dagligen** - Uppgiften återkommer varje dag
- **Veckovis** - Uppgiften återkommer varje vecka
- **Varannan vecka** - Uppgiften återkommer varannan vecka
- **Månadsvis** - Uppgiften återkommer varje månad
- **Kvartalsvis** - Uppgiften återkommer varje kvartal (3 månader)
- **Årligen** - Uppgiften återkommer varje år

### Intervall

För varje återkommande mönster kan du ange ett intervall. Till exempel:
- Intervall 1 med "Veckovis" = varje vecka
- Intervall 2 med "Veckovis" = varannan vecka
- Intervall 3 med "Månadsvis" = var tredje månad

### Hur det fungerar

1. Skapa en uppgift och markera "Gör uppgiften återkommande"
2. Välj återkommande mönster (t.ex. "Veckovis")
3. Ange intervall (vanligtvis 1)
4. När uppgiften flyttas till "Klart" eller markeras som genomförd, skapas automatiskt en ny uppgift med samma information men med ett nytt förfallodatum

### Exempel på användning

**Städning varje vecka:**
- Titel: "Städa badrummet"
- Kategori: Städning
- Återkommande: Ja
- Mönster: Veckovis
- Intervall: 1

**Månatlig budgetgenomgång:**
- Titel: "Granska hushållets budget"
- Kategori: Allmänt
- Återkommande: Ja
- Mönster: Månadsvis
- Intervall: 1

## Kanban-tavla

### Översikt

Kanban-vyn organiserar dina uppgifter i tre kolumner baserat på deras status:

1. **Att göra (ToDo)** - Uppgifter som ännu inte påbörjats
2. **Pågår (InProgress)** - Uppgifter som är under arbete
3. **Klart (Done)** - Genomförda uppgifter

### Visa Kanban-tavlan

1. Gå till hushållets "Att göra"-flik
2. Aktivera "Kanban-vy" med växelknappen
3. Välj eventuellt en specifik kategori för att filtrera uppgifter

### Flytta uppgifter mellan kolumner

Varje uppgift har knappar för att flytta den mellan statusar:

- **→ (Pil framåt)** - Flytta från "Att göra" till "Pågår"
- **✓ (Checkmark)** - Flytta från "Pågår" till "Klart"
- **← (Pil bakåt)** - Flytta från "Pågår" till "Att göra"
- **↶ (Ångra)** - Flytta från "Klart" tillbaka till "Pågår"

### Kategorivisning

Du kan filtrera uppgifter efter kategori:

- **Alla kategorier** - Visar separata kanban-tavlor för varje kategori som har uppgifter
- **Specifik kategori** - Visar endast en kanban-tavla för den valda kategorin

Detta gör det enkelt att fokusera på specifika typer av uppgifter, till exempel:
- Alla städuppgifter
- Alla inköp
- Alla underhållsuppgifter

### Uppgiftsinformation i Kanban-vyn

Varje uppgiftskort visar:

- **Titel** - Uppgiftens namn
- **Beskrivning** - Kortversion av beskrivningen (trunkerad)
- **Förfallodatum** - Om satt, med röd markering om försenat
- **Prioritet** - "Hög" prioritet markeras med röd badge
- **Återkommande** - Visas med repeat-ikon om uppgiften är återkommande
- **Tilldelad till** - Vilken hushållsmedlem som är tilldelad
- **Genomförd av** - Visas för genomförda uppgifter (endast i "Klart"-kolumnen)

## Mobilvyn

Både kanban-tavlan och återkommande funktioner är optimerade för mobil:

- Kanban-kolumnerna staplas vertikalt på mindre skärmar
- Uppgiftskort är touch-vänliga
- Dialoger för att lägga till/redigera uppgifter är responsiva

## Traditionell listvy

Om du föredrar den traditionella listvyn:

1. Avaktivera "Kanban-vy" med växelknappen
2. Du får då den ursprungliga listvyn med sökfunktion och filter för genomförda uppgifter

I listvyn visas även återkommande uppgifter med en grön "Återkommande"-badge.

## Tekniska detaljer

### Databas

Nya fält i `HouseholdTask`-tabellen:

- `Status` (INTEGER) - 0=ToDo, 1=InProgress, 2=Done
- `IsRecurring` (BOOLEAN) - Om uppgiften är återkommande
- `RecurrencePattern` (INTEGER?) - Återkommande mönster
- `RecurrenceInterval` (INTEGER?) - Intervall för återkommande
- `NextDueDate` (TEXT?) - Nästa förfallodatum för återkommande uppgift
- `ParentTaskId` (INTEGER?) - Referens till ursprunglig uppgift för genererade återkommanden

### API

Nya metoder i `IHouseholdService`:

- `GetTasksByStatusAsync` - Hämta uppgifter per status
- `GetTasksGroupedByStatusAsync` - Hämta uppgifter grupperade efter status och eventuellt kategori
- `UpdateTaskStatusAsync` - Uppdatera status för en uppgift (hanterar automatiskt recurring tasks)
- `CreateNextRecurrenceAsync` - Skapa nästa förekomst av en återkommande uppgift
- `ProcessRecurringTasksAsync` - Processa alla återkommande uppgifter för ett hushåll

## Tips och tricks

### Effektiv användning av återkommande uppgifter

1. Använd återkommande uppgifter för rutinmässiga göromål
2. Sätt realistiska förfallodatum
3. Tilldela uppgifter till specifika hushållsmedlemmar

### Effektiv användning av Kanban-tavlan

1. Flytta uppgifter till "Pågår" när du börjar arbeta på dem
2. Håll "Pågår"-kolumnen begränsad (max 2-3 uppgifter per person)
3. Använd kategorifilter för att fokusera på specifika områden
4. Granska regelbundet "Klart"-kolumnen för motivation

### Kombinera funktionerna

Återkommande uppgifter fungerar perfekt med kanban-vyn:

1. Skapa återkommande städuppgifter
2. Använd kanban för att organisera arbetsflödet
3. När en uppgift flyttas till "Klart", skapas nästa förekomst automatiskt i "Att göra"
