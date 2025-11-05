# Test Data för Påminnelser med Snooze

Detta dokument beskriver testdata som skapats för att demonstrera påminnelse-funktioner med snooze, eskalering och uppföljning.

## Översikt

Testdata innehåller **9 räkningar** med tillhörande **9 påminnelser** och **11 notifikationer** som visar olika scenarier:

- Normal påminnelse
- Snoozad påminnelse (1 gång)
- Flera snooze-tillfällen (2-3 gånger)
- Kritisk eskalering (nivå 3)
- Olika eskaleringsnivåer (0-3)
- Slutförd påminnelse
- Försenad räkning

## Bills (Räkningar)

### Bill 1: Elräkning
- **Belopp:** 1,450 kr
- **Förfallodatum:** +10 dagar från nu
- **Status:** Pending
- **Påminnelser:** 2 (normal + eskalerad nivå 1)

### Bill 2: Hemförsäkring
- **Belopp:** 349 kr
- **Förfallodatum:** +5 dagar från nu
- **Status:** Pending
- **Påminnelser:** 1 (nyligen skickad)

### Bill 3: Tandvårdsräkning
- **Belopp:** 2,800 kr
- **Förfallodatum:** +25 dagar från nu
- **Status:** Pending
- **Påminnelser:** 0 (ingen påminnelse än)

### Bill 4: Mobilabonnemang
- **Belopp:** 349 kr
- **Förfallodatum:** -5 dagar (BETALD)
- **Status:** Paid
- **Påminnelser:** 1 (slutförd)

### Bill 5: Bilförsäkring
- **Belopp:** 580 kr
- **Förfallodatum:** -10 dagar (BETALD)
- **Status:** Paid
- **Påminnelser:** 0

### Bill 6: Hyra ⚠️
- **Belopp:** 8,500 kr
- **Förfallodatum:** +2 dagar från nu
- **Status:** Pending
- **Påminnelser:** 2 (snoozad + eskalerad nivå 2)

### Bill 7: Netflix Premium
- **Belopp:** 139 kr
- **Förfallodatum:** +10 dagar från nu
- **Status:** Pending
- **Påminnelser:** 1 (snoozad 2 gånger)

### Bill 8: Interneträkning 🚨
- **Belopp:** 399 kr
- **Förfallodatum:** -8 dagar (FÖRSENAD!)
- **Status:** Overdue
- **Påminnelser:** 1 (kritisk eskalering nivå 3)

### Bill 9: Spotify Family
- **Belopp:** 179 kr
- **Förfallodatum:** +12 dagar från nu
- **Status:** Pending
- **Påminnelser:** 1 (snoozad 3 gånger - varning!)

## Bill Reminders (Påminnelser)

### Reminder 1: Elräkning - Normal
- **BillId:** 1
- **Skickad:** -3 dagar sedan
- **Snooze:** Ingen
- **Status:** Aktiv, Ej slutförd
- **Eskalering:** Nivå 0 (Normal)

### Reminder 2: Hyra - Snoozad 1 gång
- **BillId:** 6
- **Skickad:** -5 dagar sedan
- **Snooze:** Till +1 dag från nu
- **Snooze Count:** 1
- **Status:** Aktiv, Snoozad
- **Eskalering:** Nivå 0

### Reminder 3: Netflix - Snoozad 2 gånger
- **BillId:** 7
- **Skickad:** -10 dagar sedan
- **Snooze:** Till +6 timmar från nu
- **Snooze Count:** 2
- **Status:** Aktiv, Snoozad
- **Eskalering:** Nivå 0

### Reminder 4: Internet - Kritisk Eskalering 🚨
- **BillId:** 8
- **Skickad:** -15 dagar sedan
- **Snooze:** Ingen (men snoozad 2 gånger tidigare)
- **Snooze Count:** 2
- **Status:** Aktiv, KRITISK
- **Eskalering:** Nivå 3 (Kritisk)
- **Senaste uppföljning:** -12 timmar sedan
- **Meddelande:** "⚠️ BRÅDSKANDE: Interneträkning på 399 kr förföll för 8 dagar sedan. Åtgärd krävs omedelbart!"

### Reminder 5: Elräkning - Eskalering Nivå 1
- **BillId:** 1
- **Skickad:** -1 dag sedan
- **Snooze:** Ingen
- **Status:** Aktiv
- **Eskalering:** Nivå 1 (1 dag)
- **Senaste uppföljning:** -1 dag sedan

### Reminder 6: Hyra - Eskalering Nivå 2 ⚠️
- **BillId:** 6
- **Skickad:** -3 dagar sedan
- **Snooze:** Ingen
- **Status:** Aktiv
- **Eskalering:** Nivå 2 (3 dagar)
- **Senaste uppföljning:** -3 dagar sedan
- **Meddelande:** "⚠️ BRÅDSKANDE: Hyra på 8,500 kr förfaller om 5 dagar"

### Reminder 7: Mobilabonnemang - Slutförd ✓
- **BillId:** 4
- **Skickad:** -8 dagar sedan
- **Snooze:** Ingen
- **Status:** SLUTFÖRD
- **Slutförd datum:** -3 dagar sedan
- **Eskalering:** Nivå 0

### Reminder 8: Spotify - Återkommande Snooze Varning ⚠️
- **BillId:** 9
- **Skickad:** -12 dagar sedan
- **Snooze:** Till +2 dagar från nu
- **Snooze Count:** 3 (VARNING - återkommande snooze!)
- **Status:** Aktiv, Snoozad
- **Eskalering:** Nivå 1
- **Senaste uppföljning:** -2 dagar sedan
- **Meddelande:** "Spotify Family på 179 kr förfaller snart (Snoozad 3 gånger)"

### Reminder 9: Hemförsäkring - Nyligen skickad
- **BillId:** 2
- **Skickad:** -6 timmar sedan
- **Snooze:** Ingen
- **Status:** Aktiv, Nyskickad
- **Eskalering:** Nivå 0

## Notifications (Notifikationer)

### Notification 1: Elräkning - Normal
- **Typ:** BillDue
- **Prioritet:** Normal
- **Läst:** Nej
- **Snooze:** Ingen
- **BillReminderId:** 1

### Notification 2: Hyra - Snoozad
- **Typ:** BillDue
- **Prioritet:** High
- **Läst:** Nej
- **Snooze:** Till +1 dag (1 snooze)
- **BillReminderId:** 2
- **Visuell indikation:** 💤 ikon + halvtransparent

### Notification 3: Netflix - Snoozad 2x
- **Typ:** BillDue
- **Prioritet:** Normal
- **Läst:** Nej
- **Snooze:** Till +6h (2 snooze)
- **BillReminderId:** 3
- **Visuell indikation:** "💤 Snoozad till [datum] (2 snooze)"

### Notification 4: Internet - Kritisk 🚨
- **Typ:** BillOverdue
- **Prioritet:** Critical
- **Läst:** Nej
- **Snooze:** Ingen (men 2 tidigare)
- **BillReminderId:** 4
- **Titel:** "⚠️ BRÅDSKANDE: Betala Interneträkning"
- **Meddelande:** Inkluderar "(Snoozad 2 gånger)"

### Notification 5: Hyra - Eskalerad ⚠️
- **Typ:** BillDue
- **Prioritet:** High
- **Läst:** Nej
- **Snooze:** Ingen
- **BillReminderId:** 6
- **Titel:** "⚠️ BRÅDSKANDE: Hyra"

### Notification 6: Mobilabonnemang - Läst ✓
- **Typ:** BillDue
- **Prioritet:** Normal
- **Läst:** Ja (-3 dagar sedan)
- **Snooze:** Ingen
- **BillReminderId:** 7

### Notification 7: Spotify - Varning Återkommande Snooze ⚠️
- **Typ:** BillDue
- **Prioritet:** High
- **Läst:** Nej
- **Snooze:** Till +2d (3 snooze!)
- **BillReminderId:** 8
- **Meddelande:** "Räkning på 179 kr förfaller 2025-11-17 (Snoozad 3 gånger)"
- **Flaggad:** För eskalering

### Notification 8: Hemförsäkring - Nylig
- **Typ:** BillDue
- **Prioritet:** Normal
- **Läst:** Nej
- **Snooze:** Ingen
- **BillReminderId:** 9

### Notification 9-11: Andra typer
- Goal Achievement (oläst)
- Subscription Renewal (läst)
- Budget Warning (oläst)

## Test-scenarios

### Scenario 1: Normal påminnelse
- **Bill:** Elräkning (Bill 1)
- **Reminder:** Reminder 1
- **Notification:** Notification 1
- **Åtgärd:** Visa snooze-dropdown, "Markera som betald", "Skapa transaktion"

### Scenario 2: Snoozad notifikation (en gång)
- **Bill:** Hyra (Bill 6)
- **Reminder:** Reminder 2
- **Notification:** Notification 2
- **Visuellt:** 70% opacitet, 💤 ikon, "(1 snooze)"
- **Dold till:** Snooze går ut (+1 dag)

### Scenario 3: Flera snooze-tillfällen
- **Bill:** Netflix (Bill 7)
- **Reminder:** Reminder 3
- **Notification:** Notification 3
- **Visuellt:** "(2 snooze)"
- **Dold till:** +6 timmar

### Scenario 4: Återkommande snooze - Varning
- **Bill:** Spotify (Bill 9)
- **Reminder:** Reminder 8
- **Notification:** Notification 7
- **Snooze count:** 3 (triggrar varning!)
- **Visuellt:** "(3 snooze)" + prioritet höjd till High
- **Meddelande:** Inkluderar "(Snoozad 3 gånger)"

### Scenario 5: Eskalering Nivå 1 (1 dag)
- **Bill:** Elräkning (Bill 1)
- **Reminder:** Reminder 5
- **Åtgärd:** Uppföljning skickad efter 24h
- **Prioritet:** Ändrad till High

### Scenario 6: Eskalering Nivå 2 (3 dagar) ⚠️
- **Bill:** Hyra (Bill 6)
- **Reminder:** Reminder 6
- **Notification:** Notification 5
- **Titel:** Börjar med "⚠️ BRÅDSKANDE:"
- **Prioritet:** High

### Scenario 7: Eskalering Nivå 3 (7+ dagar) 🚨
- **Bill:** Internet (Bill 8)
- **Reminder:** Reminder 4
- **Notification:** Notification 4
- **Titel:** "⚠️ BRÅDSKANDE: Betala Interneträkning"
- **Meddelande:** "Åtgärd krävs omedelbart!"
- **Prioritet:** Critical
- **Senaste uppföljning:** -12h sedan

### Scenario 8: Slutförd påminnelse
- **Bill:** Mobilabonnemang (Bill 4)
- **Reminder:** Reminder 7
- **Notification:** Notification 6
- **Status:** IsCompleted = true, IsRead = true
- **Visuellt:** Halvtransparent, "Läst" märkning

## Databas-queries för att testa

### Hämta alla aktiva (ej snoozade) notifikationer:
```sql
SELECT * FROM Notifications 
WHERE UserId = 'test-user-id' 
AND (SnoozeUntil IS NULL OR SnoozeUntil <= CURRENT_TIMESTAMP)
AND IsRead = 0
ORDER BY Priority DESC, CreatedAt DESC;
```

### Hämta snoozade notifikationer:
```sql
SELECT * FROM Notifications 
WHERE UserId = 'test-user-id' 
AND SnoozeUntil IS NOT NULL 
AND SnoozeUntil > CURRENT_TIMESTAMP
ORDER BY SnoozeUntil ASC;
```

### Hämta påminnelser som behöver eskaleras:
```sql
SELECT br.*, b.Name, b.Amount, b.DueDate
FROM BillReminders br
INNER JOIN Bills b ON br.BillId = b.BillId
WHERE br.IsSent = 1 
AND br.IsCompleted = 0
AND (br.SnoozeUntil IS NULL OR br.SnoozeUntil <= CURRENT_TIMESTAMP)
AND (br.LastFollowUpDate IS NULL OR br.LastFollowUpDate < DATEADD(hour, -24, CURRENT_TIMESTAMP))
ORDER BY b.DueDate ASC;
```

### Hämta påminnelser med återkommande snooze (3+):
```sql
SELECT * FROM BillReminders 
WHERE SnoozeCount >= 3 
AND IsCompleted = 0
ORDER BY SnoozeCount DESC;
```

## UI Test-fall

### Test 1: Visa notifikationslista
**Förväntat resultat:**
- 8 notifikationer för räkningspåminnelser
- 3 övriga notifikationer (Goal, Subscription, Budget)
- Snoozade notifikationer visas med 70% opacitet
- Kritisk notifikation (Internet) överst med röd markering
- Snooze-räknare visas på Netflix (2) och Spotify (3)

### Test 2: Snooze-funktionalitet
**Åtgärd:** Klicka på "💤 Snooze" dropdown
**Förväntat resultat:**
- Dropdown visar 3 alternativ: "⏱️ 1 timme", "📅 1 dag", "📆 1 vecka"
- Efter val: Notifikation blir halvtransparent
- Snooze-räknare ökar
- Snackbar: "Påminnelse snoozad"

### Test 3: Markera som betald
**Åtgärd:** Klicka "✓ Markera som betald"
**Förväntat resultat:**
- Notifikation markeras som läst
- Reminder markeras som IsCompleted = true
- Bill status ändras till "Paid"
- Snackbar: "Påminnelse markerad som betald"

### Test 4: Skapa transaktion
**Åtgärd:** Klicka "📝 Skapa transaktion"
**Förväntat resultat:**
- Navigation till /economy/transactions/new
- Formulär förifyllt med:
  - Belopp från räkning
  - Payee
  - Kategori
  - Förfallodatum som transaktionsdatum

### Test 5: Eskalerad notifikation
**Förväntat:** 
- Internet-notifikationen visas med:
  - Röd border (Critical priority)
  - "⚠️ BRÅDSKANDE" i titel
  - "(Snoozad 2 gånger)" i meddelande
  - Högst upp i listan

## Tekniska Detaljer

### Filer ändrade:
- `src/Privatekonomi.Core/Data/TestDataSeeder.cs`

### Nya metoder:
- `SeedBillReminders(context, userId)` - Skapar 9 testpåminnelser

### Uppdaterade metoder:
- `SeedBills(context, userId)` - Lagt till 4 nya räkningar (6-9)
- `SeedNotifications(context, userId)` - Helt omskriven med 11 notifikationer

### Datamodeller involverade:
- `Bill` (9 instanser)
- `BillReminder` (9 instanser)
- `Notification` (11 instanser)

## Användarflöde för manuell testning

1. **Starta applikationen**
   ```bash
   cd src/Privatekonomi.Web
   dotnet run
   ```

2. **Logga in**
   - Email: `test@example.com`
   - Password: `Test123!`

3. **Navigera till Notifikationer**
   - Gå till `/settings/notifications`
   - Alternativt klicka "Inställningar" → "Notifikationer" i menyn

4. **Observera olika tillstånd**
   - Normal påminnelse (Elräkning)
   - Snoozad (Hyra - 1 snooze)
   - Flera snooze (Netflix - 2 snooze)
   - Varning (Spotify - 3 snooze)
   - Kritisk (Internet - försenad)

5. **Testa snooze-funktionen**
   - Klicka snooze-dropdown på valfri notifikation
   - Välj duration (1h/1d/1v)
   - Verifiera att notifikationen blir halvtransparent
   - Verifiera att snooze-räknaren ökar

6. **Testa "Markera som betald"**
   - Klicka på någon aktiv påminnelse
   - Verifiera att notifikationen försvinner
   - Kontrollera att räkningen markeras som betald i `/economy/bills`

7. **Testa eskalering**
   - Observera Internet-notifikationen (kritisk)
   - Notera "⚠️ BRÅDSKANDE" i titel
   - Notera "(Snoozad 2 gånger)" i meddelande

## Sammanfattning

Testdata innehåller nu:
- ✅ 9 räkningar i olika tillstånd (pending, paid, overdue)
- ✅ 9 påminnelser med varierande snooze och eskalering
- ✅ 11 notifikationer inklusive 8 för räkningspåminnelser
- ✅ Alla eskaleringsnivåer (0-3) representerade
- ✅ Olika snooze-scenarion (0, 1, 2, 3+ snooze)
- ✅ Återkommande snooze-varning vid 3+ tillfällen
- ✅ Slutförd/läst påminnelse
- ✅ Kritisk försenad räkning

Detta ger en komplett demonstration av snooze och uppföljningsfunktionaliteten!
