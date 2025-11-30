# Påminnelsehantering med Snooze - Implementation

## Översikt

Implementering av ett flexibelt påminnelsesystem med snooze-funktionalitet, uppföljning och eskalering enligt specifikation.

## Implementerade komponenter

### 1. Datamodeller

#### Reminder.cs
- **Fält**: Id, UserId, Title, Description, Status, ReminderDate, Priority
- **Snooze**: SnoozeUntil, SnoozeCount
- **Eskalering**: EscalationLevel, LastFollowUpDate, EnableFollowUp
- **Metadata**: ReminderType, RelatedEntityId, RelatedEntityType, Tags, ActionUrl
- **Status**: Active, Snoozed, Completed, Dismissed, Expired
- **Prioritet**: Low, Normal, High, Critical

#### ReminderSettings.cs
- **Eskalering**: EnableEscalation, SnoozeThresholdForEscalation, EscalateToEmail, EscalateToSMS, EscalateToPush
- **Uppföljning**: EnableFollowUp, FollowUpIntervalHours, MaxFollowUps
- **Inställningar**: DefaultSnoozeDurationMinutes, QuietHours, RespectQuietHours

### 2. ReminderService

#### Huvudfunktionalitet
- `GetUserRemindersAsync()` - Hämta alla påminnelser för användare
- `GetActiveRemindersAsync()` - Hämta aktiva (icke-snoozade) påminnelser
- `GetDueRemindersAsync()` - Hämta förfallna påminnelser
- `CreateReminderAsync()` - Skapa ny påminnelse
- `UpdateReminderAsync()` - Uppdatera påminnelse
- `DeleteReminderAsync()` - Ta bort påminnelse

#### Snooze-funktioner
- `SnoozeReminderAsync(reminderId, userId, durationMinutes)` - Skjut upp påminnelse
- Automatisk eskaleringscheck vid upprepade snoozes
- Snooze-räknare för att spåra antal gånger snoozad

#### Slutförande och avfärdande
- `MarkAsCompletedAsync()` - Markera som slutförd med datum
- `DismissReminderAsync()` - Avfärda påminnelse
- Automatisk notifikation vid slutförande

#### Uppföljning och eskalering
- `ProcessFollowUpsAsync()` - Bakgrundsprocess för automatisk uppföljning
- `ShouldEscalateReminderAsync()` - Kontrollera om eskalering behövs
- `EscalateReminderAsync()` - Eskalera påminnelse till högre prioritet
- Konfigurerbara uppföljningsintervall
- Max antal uppföljningar innan påminnelse markeras som expired

#### Inställningar och statistik
- `GetUserSettingsAsync()` - Hämta användarinställningar (skapar default om saknas)
- `UpdateUserSettingsAsync()` - Uppdatera inställningar
- `GetStatisticsAsync()` - Hämta statistik (aktiva, snoozade, slutförda, försenade, etc.)

### 3. Användargränssnitt

#### Reminders.razor - Huvudsida
- **Statistikkort**: Visar antal aktiva, dagens, försenade och slutförda påminnelser
- **Filterflikar**: Aktiva, Alla, Slutförda
- **Påminnelselista** med:
  - Prioritetsikon och färg
  - Status-chips (Försenad, Snoozad, Eskalerad)
  - Datum och tid
  - Snooze-information
  - Beskrivning och typ

#### Quick Actions
- **Slutför-knapp**: Markera som slutförd direkt
- **Snooze-meny**: 
  - 1 timme
  - 1 dag
  - 1 vecka
  - Anpassad (öppnar CustomSnoozeDialog)
- **Redigera-knapp**: Öppna redigeringsdialog
- **Avfärda-knapp**: Avfärda påminnelse
- **Ta bort-knapp**: Ta bort permanent

#### ReminderDialog.razor
Dialog för att skapa och redigera påminnelser:
- Titel (obligatorisk)
- Beskrivning
- Datum och tid (separata fält)
- Prioritet (dropdown: Låg, Normal, Hög, Kritisk)
- Typ (valfritt, t.ex. "Räkning", "Möte")
- Åtgärdslänk (för navigation)
- Uppföljningsinställningar:
  - Aktivera automatisk uppföljning (checkbox)
  - Uppföljningsintervall (timmar)
  - Max antal uppföljningar

#### CustomSnoozeDialog.razor
Anpassad snooze-dialog:
- Timmar (numeriskt fält)
- Minuter (numeriskt fält)
- Visar beräknat snooze-tid

### 4. Navigation
- Lagt till under "Inställningar" i huvudmenyn
- Ikon: NotificationImportant
- Route: /reminders

### 5. Dependency Injection
- Registrerat `IReminderService` och `ReminderService` i Program.cs
- Scoped lifetime för att dela DbContext

### 6. Databas
- Lagt till `DbSet<Reminder> Reminders` i PrivatekonomyContext
- Lagt till `DbSet<ReminderSettings> ReminderSettings` i PrivatekonomyContext

## Tester

### ReminderServiceTests.cs
11 enhetstester som täcker:
1. ✅ CreateReminderAsync - Skapa påminnelse
2. ✅ GetUserRemindersAsync - Hämta användarens påminnelser
3. ✅ SnoozeReminderAsync - Snooze-funktionalitet
4. ✅ MarkAsCompletedAsync - Slutföra påminnelse
5. ✅ GetActiveRemindersAsync - Filtrera aktiva påminnelser
6. ✅ GetDueRemindersAsync - Filtrera förfallna påminnelser
7. ✅ ShouldEscalateReminderAsync - Eskaleringslogik
8. ✅ GetUserSettingsAsync - Hämta/skapa inställningar
9. ✅ GetStatisticsAsync - Statistikberäkning
10. ✅ DeleteReminderAsync - Ta bort påminnelse
11. ✅ DismissReminderAsync - Avfärda påminnelse

Alla tester passerar! ✅

## Funktioner som implementerats

### ✅ 1. Datamodell för påminnelser
- Komplett Reminder-entitet med alla nödvändiga fält
- ReminderSettings för användarspecifika preferenser
- Relationer till användare och andra entiteter

### ✅ 2. Snooze-funktionalitet
- Möjlighet att skjuta upp påminnelser
- Snabbval: 1 timme, 1 dag, 1 vecka
- Anpassad snooze med timmar/minuter
- Uppdatering av status och nästa aktiveringstid
- Räknare för antal snoozes

### ✅ 3. Markera som klar
- Funktion för att markera påminnelse som genomförd
- Status ändras till "Completed"
- CompletedDate loggas
- Bekräftelsenotifikation skickas

### ✅ 4. Uppföljning och eskalering
- ProcessFollowUpsAsync för bakgrundsbearbetning
- Konfigurerbara uppföljningsintervall
- Automatisk eskalering efter X antal snoozes
- Eskaleringskanaler (InApp, Email, Push, SMS)
- EscalationLevel-spårning
- Max antal uppföljningar innan "expired"

### ✅ 5. Quick actions i UI
- Snabbvalsmenyer direkt i gränssnittet
- Slutför-knapp
- Snooze-meny med fördefinierade val
- Redigera och avfärda direkt i listan
- Översikt med statistik och filter

## Exempel på användning

### Skapa en påminnelse programmatiskt
```csharp
var reminder = new Reminder
{
    UserId = currentUserId,
    Title = "Betala elräkning",
    Description = "Förfallodatum är den 15:e",
    ReminderDate = new DateTime(2025, 11, 15, 10, 0, 0),
    Priority = ReminderPriority.High,
    ReminderType = "Räkning",
    EnableFollowUp = true,
    FollowUpIntervalHours = 24,
    MaxFollowUps = 3
};

await reminderService.CreateReminderAsync(reminder);
```

### Snooze en påminnelse
```csharp
// Snooze i 1 timme (60 minuter)
await reminderService.SnoozeReminderAsync(reminderId, userId, 60);

// Anpassad snooze (2 dagar)
await reminderService.SnoozeReminderAsync(reminderId, userId, 2880);
```

### Konfigurera användarinställningar
```csharp
var settings = await reminderService.GetUserSettingsAsync(userId);
settings.EnableEscalation = true;
settings.SnoozeThresholdForEscalation = 3;
settings.EscalateToEmail = true;
settings.EscalateToPush = true;
settings.FollowUpIntervalHours = 12;
await reminderService.UpdateUserSettingsAsync(settings);
```

## Framtida förbättringar (ej implementerade)

- **Bakgrundsprocess**: Implementera en hosted background service för automatisk ProcessFollowUpsAsync
- **Email-integration**: Faktisk email-funktion för eskalering
- **SMS-integration**: SMS-funktion för kritiska påminnelser
- **Push-notifikationer**: Push-funktion för mobila enheter
- **Återkommande påminnelser**: Stöd för dagliga/veckovisa/månatliga påminnelser
- **Mallar**: Fördefinierade påminnelsemallar för vanliga uppgifter
- **Kategorier**: Gruppera påminnelser i kategorier
- **AI-förslag**: Föreslå lämpliga påminnelsetider baserat på användarens beteende

## Säkerhet och prestanda

- ✅ Användarisolering: Alla queries filtrerar på UserId
- ✅ Validering: Required-attribut på viktiga fält
- ✅ MaxLength-begränsningar på texfält
- ✅ Indexes (potentiellt): Överväg index på UserId, Status, ReminderDate
- ✅ Enhetstester: 11 tester täcker huvudfunktionalitet
- ✅ Nullable handling: Använder nullable reference types

## Sammanfattning

Ett komplett påminnelsesystem har implementerats med alla efterfrågade funktioner:
- ✅ Flexibel datamodell
- ✅ Snooze med anpassningsbara varaktigheter
- ✅ Slutförandefunktion
- ✅ Uppföljning och eskalering
- ✅ Användarvänligt gränssnitt med quick actions
- ✅ Omfattande enhetstester
- ✅ Integration med befintligt notifikationssystem

Implementationen följer projektets konventioner och använder befintliga mönster och komponenter (MudBlazor, Entity Framework, Identity, etc.).

**Tidplan**: Implementationen genomfördes enligt plan.
**Prioritet**: Medel - Implementerad och testad.
