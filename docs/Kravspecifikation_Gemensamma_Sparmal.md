# Kravspecifikation: Gemensamma Sparmål

## 1. Översikt

### 1.1 Syfte
Implementera en funktion som möjliggör att flera användare kan skapa och följa gemensamma sparmål tillsammans. Detta möjliggör familjer, par eller vänner att samarbeta kring ekonomiska mål som semester, boende eller större inköp.

### 1.2 Omfattning
Funktionen bygger på den befintliga sparmålsfunktionaliteten och utökar den med följande:
- Möjlighet att bjuda in andra användare till ett sparmål
- Delad synlighet av sparmålets status och transaktioner
- Förslag och godkännanden för ändringar av sparmålet
- Notifieringssystem för viktiga händelser
- Hantering av deltagare (inbjudan, borttagning, lämna)

## 2. Funktionella Krav

### 2.1 Skapa Gemensamt Sparmål (FR-001)
**Beskrivning:** En användare ska kunna skapa ett nytt gemensamt sparmål.

**Krav:**
- Användaren anger namn, beskrivning, målbelopp och målsättningsdatum
- Användaren kan välja prioritet (1-5)
- Användaren kan välja vilket sparkonto målet ska kopplas till (valfritt)
- Vid skapande blir användaren automatiskt "ägare" av målet
- Systemet genererar ett unikt ID för målet
- Sparmålet sparas i databasen med status "Active"

**Acceptanskriterier:**
- Ett nytt gemensamt sparmål kan skapas via användargränssnittet
- Alla obligatoriska fält måste fyllas i
- Skaparen blir automatiskt deltagare med rollen "Owner"

### 2.2 Bjuda In Deltagare (FR-002)
**Beskrivning:** Ägaren eller deltagare med rätt behörighet ska kunna bjuda in andra användare.

**Krav:**
- Endast ägare kan bjuda in nya deltagare
- Inbjudan sker via användarens e-postadress
- Inbjudna användare måste ha ett konto i systemet
- Systemet skickar en notifiering till den inbjudna användaren
- Inbjudningar har statusen "Pending" tills de accepteras eller avvisas
- Inbjudningar kan dras tillbaka av ägaren

**Acceptanskriterier:**
- Ägare kan skicka inbjudningar till registrerade användare
- Inbjudna användare får en notifiering
- Inbjudningar kan accepteras eller avvisas
- Accepterad inbjudan lägger till användaren som deltagare

### 2.3 Hantera Deltagare (FR-003)
**Beskrivning:** Systemet ska hantera olika roller och rättigheter för deltagare.

**Roller:**
- **Owner:** Skapare av målet, kan bjuda in/ta bort deltagare, föreslå ändringar, lämna (överföra ägarskap först)
- **Participant:** Kan se status, föreslå ändringar, lämna sparmålet

**Krav:**
- Varje deltagare har en roll (Owner eller Participant)
- Deltagare kan se alla andra deltagare
- Ägare kan ta bort deltagare (kräver bekräftelse)
- Deltagare kan lämna målet frivilligt
- Om ägaren lämnar måste ägarskapet överföras till en annan deltagare först

**Acceptanskriterier:**
- Olika roller har olika behörigheter
- Deltagarlista visas för alla deltagare
- Endast ägare kan ta bort andra deltagare

### 2.4 Visa Sparmålsstatus (FR-004)
**Beskrivning:** Alla deltagare ska kunna se aktuell status för det gemensamma sparmålet.

**Krav:**
- Visa målbelopp och aktuellt belopp
- Visa framsteg i procent
- Visa målsättningsdatum och tid kvar
- Visa alla deltagare
- Visa historik av insättningar/uttag
- Visa vem som gjorde senaste insättningen

**Acceptanskriterier:**
- Status uppdateras i realtid när ändringar görs
- Alla deltagare ser samma information
- Historik är synlig för alla deltagare

### 2.5 Föreslå Ändringar (FR-005)
**Beskrivning:** Deltagare ska kunna föreslå ändringar av sparmålets parametrar.

**Ändringar som kan föreslås:**
- Nytt målbelopp
- Nytt målsättningsdatum
- Nytt namn eller beskrivning
- Ändrad prioritet

**Krav:**
- Varje deltagare kan skapa ett förslag
- Förslag måste godkännas av alla deltagare för att träda i kraft
- Deltagare får notifieringar om nya förslag
- Deltagare kan rösta "Ja" eller "Nej" på förslag
- Förslag genomförs automatiskt när alla har godkänt
- Förslag kan dras tillbaka av den som föreslog det

**Acceptanskriterier:**
- Förslag kan skapas av alla deltagare
- Alla deltagare måste godkänna innan ändringar träder i kraft
- Notifieringar skickas vid nya förslag och godkännanden

### 2.6 Uppdatera Sparbelopp (FR-006)
**Beschrivning:** Deltagare ska kunna uppdatera det aktuella sparbeloppet.

**Krav:**
- Deltagare kan öka eller minska det sparade beloppet
- Ändringar loggas med datum, tid och användare
- Alla deltagare notifieras vid insättningar/uttag över ett visst belopp (t.ex. 1000 kr)
- Historik över alla ändringar visas

**Acceptanskriterier:**
- Sparbelopp kan uppdateras av alla deltagare
- Ändringar loggas och är spårbara
- Notifieringar skickas vid större ändringar

### 2.7 Notifieringssystem (FR-007)
**Beskrivning:** Systemet ska skicka notifieringar vid viktiga händelser.

**Händelser som genererar notifieringar:**
- Ny inbjudan mottagen
- Inbjudan accepterad/avvisad
- Nytt förslag skapat
- Förslag godkänt/avvisat av deltagare
- Förslag genomfört
- Stor insättning/uttag gjord
- Deltagare lämnat målet
- Deltagare borttagen
- Sparmål uppnått (100%)

**Krav:**
- Notifieringar visas i användargränssnittet
- Notifieringar kan vara lästa/olästa
- Användare kan se historik över alla notifieringar
- Notifieringar kan tas bort av användaren

**Acceptanskriterier:**
- Notifieringar genereras vid rätt händelser
- Användare ser notifieringar i realtid
- Notifieringar kan markeras som lästa

### 2.8 Lämna Sparmål (FR-008)
**Beskrivning:** Deltagare ska kunna lämna ett gemensamt sparmål.

**Krav:**
- Deltagare kan lämna målet när som helst
- Systemet begär bekräftelse innan användaren lämnar
- Om ägaren lämnar måste ägarskapet överföras först
- Alla deltagare notifieras när någon lämnar
- Om alla deltagare lämnar arkiveras målet automatiskt

**Acceptanskriterier:**
- Deltagare kan lämna via användargränssnittet
- Bekräftelse krävs
- Ägare måste överföra ägarskap först

### 2.9 Ta Bort Deltagare (FR-009)
**Beskrivning:** Ägare ska kunna ta bort deltagare från sparmålet.

**Krav:**
- Endast ägare kan ta bort deltagare
- Systemet begär bekräftelse
- Borttagen deltagare notifieras
- Övriga deltagare notifieras
- Borttagen deltagare förlorar åtkomst till målet

**Acceptanskriterier:**
- Ägare kan ta bort deltagare
- Bekräftelse krävs
- Notifieringar skickas

## 3. Icke-Funktionella Krav

### 3.1 Säkerhet (NFR-001)
**Krav:**
- Endast deltagare har åtkomst till ett gemensamt sparmål
- Användarautentisering krävs för alla operationer
- Rollbaserad åtkomstkontroll för olika funktioner
- Audit logging för alla ändringar

**Acceptanskriterier:**
- Obehöriga användare kan inte se eller ändra sparmål
- Alla ändringar loggas med användare och tidstämpel

### 3.2 Prestanda (NFR-002)
**Krav:**
- Sparmålsstatus ska laddas inom 2 sekunder
- Notifieringar ska levereras inom 5 sekunder
- Systemet ska hantera minst 1000 samtidiga användare
- Systemet ska kunna hantera minst 10 000 gemensamma sparmål

**Acceptanskriterier:**
- Sidladdningstider uppfyller kraven
- System svarar även under hög belastning

### 3.3 Användbarhet (NFR-003)
**Krav:**
- Intuitivt användargränssnitt
- Responsiv design för mobila enheter
- Svenska som standardspråk
- Tydliga felmeddelanden
- Hjälptexter och instruktioner

**Acceptanskriterier:**
- Användare kan utföra huvudfunktioner utan dokumentation
- Gränssnittet fungerar på mobil, tablet och desktop

### 3.4 Skalbarhet (NFR-004)
**Krav:**
- Databasdesign som stöder tillväxt
- Effektiv indexering för snabba sökningar
- Möjlighet att arkivera gamla/inaktiva mål

**Acceptanskriterier:**
- Prestanda försämras inte vid ökad datamängd
- Systemet kan växa med användarbasen

### 3.5 Tillgänglighet (NFR-005)
**Krav:**
- 99% uptime
- Graceful degradation vid fel
- Tydliga felmeddelanden till användare

**Acceptanskriterier:**
- Systemet är tillgängligt enligt SLA
- Användare får vettiga felmeddelanden vid problem

## 4. Datamodell

### 4.1 SharedGoal (Gemensamt Sparmål)
```
- SharedGoalId (int, PK)
- Name (string, required, max 200)
- Description (string, max 500)
- TargetAmount (decimal)
- CurrentAmount (decimal)
- TargetDate (DateTime?)
- Priority (int, 1-5)
- Status (enum: Active, Completed, Archived)
- CreatedAt (DateTime)
- UpdatedAt (DateTime?)
- CreatedByUserId (string, FK to ApplicationUser)
```

### 4.2 SharedGoalParticipant (Deltagare)
```
- SharedGoalParticipantId (int, PK)
- SharedGoalId (int, FK)
- UserId (string, FK to ApplicationUser)
- Role (enum: Owner, Participant)
- JoinedAt (DateTime)
- InvitationStatus (enum: Pending, Accepted, Rejected)
- InvitedByUserId (string, FK to ApplicationUser)
- InvitedAt (DateTime)
```

### 4.3 SharedGoalProposal (Förslag)
```
- SharedGoalProposalId (int, PK)
- SharedGoalId (int, FK)
- ProposedByUserId (string, FK to ApplicationUser)
- ProposalType (enum: ChangeTargetAmount, ChangeTargetDate, ChangeName, ChangePriority)
- CurrentValue (string)
- ProposedValue (string)
- Description (string, max 500)
- Status (enum: Pending, Approved, Rejected, Withdrawn)
- CreatedAt (DateTime)
- ResolvedAt (DateTime?)
```

### 4.4 SharedGoalProposalVote (Röst på förslag)
```
- SharedGoalProposalVoteId (int, PK)
- SharedGoalProposalId (int, FK)
- UserId (string, FK to ApplicationUser)
- Vote (enum: Approve, Reject)
- VotedAt (DateTime)
- Comment (string, max 500)
```

### 4.5 SharedGoalTransaction (Transaktioner)
```
- SharedGoalTransactionId (int, PK)
- SharedGoalId (int, FK)
- UserId (string, FK to ApplicationUser)
- Amount (decimal)
- Type (enum: Deposit, Withdrawal)
- Description (string, max 500)
- TransactionDate (DateTime)
- CreatedAt (DateTime)
```

### 4.6 SharedGoalNotification (Notifieringar)
```
- SharedGoalNotificationId (int, PK)
- SharedGoalId (int, FK)
- UserId (string, FK to ApplicationUser)
- Type (enum: Invitation, ProposalCreated, ProposalApproved, ProposalRejected, 
        TransactionMade, ParticipantJoined, ParticipantLeft, ParticipantRemoved, GoalCompleted)
- Message (string, max 500)
- IsRead (bool)
- CreatedAt (DateTime)
- ReadAt (DateTime?)
```

## 5. API Endpoints

### 5.1 Sparmål (Shared Goals)
```
GET    /api/sharedgoals                    - Lista alla användarens gemensamma mål
GET    /api/sharedgoals/{id}               - Hämta specifikt mål
POST   /api/sharedgoals                    - Skapa nytt gemensamt mål
PUT    /api/sharedgoals/{id}               - Uppdatera mål (kräver godkännande)
DELETE /api/sharedgoals/{id}               - Ta bort mål (endast ägare)
PUT    /api/sharedgoals/{id}/amount        - Uppdatera sparat belopp
```

### 5.2 Deltagare (Participants)
```
GET    /api/sharedgoals/{id}/participants         - Lista deltagare
POST   /api/sharedgoals/{id}/participants/invite  - Bjud in användare
PUT    /api/sharedgoals/{id}/participants/{userId}/accept   - Acceptera inbjudan
PUT    /api/sharedgoals/{id}/participants/{userId}/reject   - Avvisa inbjudan
DELETE /api/sharedgoals/{id}/participants/{userId}          - Ta bort/lämna
PUT    /api/sharedgoals/{id}/participants/{userId}/transfer - Överför ägarskap
```

### 5.3 Förslag (Proposals)
```
GET    /api/sharedgoals/{id}/proposals            - Lista förslag
POST   /api/sharedgoals/{id}/proposals            - Skapa förslag
PUT    /api/sharedgoals/{id}/proposals/{proposalId}/vote    - Rösta på förslag
DELETE /api/sharedgoals/{id}/proposals/{proposalId}         - Dra tillbaka förslag
```

### 5.4 Notifieringar (Notifications)
```
GET    /api/sharedgoals/notifications             - Hämta alla notifieringar
GET    /api/sharedgoals/notifications/unread      - Hämta olästa notifieringar
PUT    /api/sharedgoals/notifications/{id}/read   - Markera som läst
DELETE /api/sharedgoals/notifications/{id}        - Ta bort notifiering
```

### 5.5 Transaktioner (Transactions)
```
GET    /api/sharedgoals/{id}/transactions         - Hämta transaktionshistorik
POST   /api/sharedgoals/{id}/transactions         - Skapa transaktion
```

## 6. Användargränssnitt

### 6.1 Gemensamma Sparmål - Översikt
**Sida:** `/sharedgoals`

**Komponenter:**
- Lista över alla gemensamma sparmål
- Filterering (Aktiva, Avslutade, Arkiverade)
- Sökfunktion
- Knapp för att skapa nytt gemensamt mål
- Varje mål visar:
  - Namn och beskrivning
  - Framsteg (progress bar)
  - Antal deltagare
  - Målbelopp och sparat belopp
  - Målsättningsdatum
  - Notifieringsikon om olästa notiser

### 6.2 Gemensamt Sparmål - Detaljvy
**Sida:** `/sharedgoals/{id}`

**Komponenter:**
- Rubrik med namn och beskrivning
- Statistik:
  - Målbelopp
  - Sparat belopp
  - Framsteg (%)
  - Tid kvar till målsättningsdatum
  - Genomsnittlig månatlig insättning behövd
- Deltagare:
  - Lista över alla deltagare med roller
  - Knapp för att bjuda in (om ägare)
  - Möjlighet att lämna/ta bort
- Transaktionshistorik:
  - Lista över alla insättningar/uttag
  - Vem, när och belopp
- Förslag:
  - Aktiva förslag som väntar på godkännande
  - Möjlighet att rösta
  - Historik över tidigare förslag
- Åtgärder:
  - Uppdatera sparbelopp
  - Skapa nytt förslag
  - Hantera deltagare

### 6.3 Dialog - Skapa Gemensamt Sparmål
**Fält:**
- Namn (required)
- Beskrivning
- Målbelopp (required)
- Startbelopp
- Målsättningsdatum
- Prioritet (1-5)
- Sparkonto (dropdown med användares bankkonton)

**Knappar:**
- Skapa
- Avbryt

### 6.4 Dialog - Bjud In Deltagare
**Fält:**
- E-postadress (autocomplete från användare)
- Personligt meddelande (optional)

**Knappar:**
- Skicka inbjudan
- Avbryt

### 6.5 Dialog - Skapa Förslag
**Fält:**
- Typ av förslag (dropdown)
- Nuvarande värde (read-only)
- Föreslaget värde (required)
- Motivering (optional)

**Knappar:**
- Skapa förslag
- Avbryt

### 6.6 Dialog - Uppdatera Sparbelopp
**Fält:**
- Nuvarande belopp (read-only)
- Ändring (+ eller -)
- Nytt belopp (calculated)
- Kommentar (optional)

**Knappar:**
- Spara
- Avbryt

### 6.7 Notifieringscenter
**Komponenter:**
- Ikon i navigation med antal olästa
- Dropdown/panel med lista över notifieringar
- Markera alla som lästa
- Länk till fullständig notifieringshistorik

## 7. Användningsfall (Use Cases)

### UC-001: Skapa Gemensamt Sparmål
**Aktör:** Användare (blir Ägare)

**Förutsättningar:**
- Användare är inloggad

**Huvudflöde:**
1. Användare navigerar till Gemensamma Sparmål
2. Användare klickar på "Skapa Nytt Gemensamt Sparmål"
3. Systemet visar formulär
4. Användare fyller i namn, beskrivning, målbelopp, datum och prioritet
5. Användare klickar på "Skapa"
6. Systemet skapar sparmålet och lägger till användaren som ägare
7. Systemet visar bekräftelse och navigerar till detaljvyn

**Alternativt flöde:**
- Om obligatoriska fält saknas visar systemet felmeddelande

### UC-002: Bjuda In Deltagare
**Aktör:** Ägare

**Förutsättningar:**
- Användare är ägare av ett gemensamt sparmål

**Huvudflöde:**
1. Ägare öppnar sparmålets detaljvy
2. Ägare klickar på "Bjud In Deltagare"
3. Systemet visar inbjudningsformulär
4. Ägare anger e-postadress till registrerad användare
5. Ägare klickar på "Skicka Inbjudan"
6. Systemet skapar inbjudan med status "Pending"
7. Systemet skickar notifiering till inbjuden användare
8. Systemet visar bekräftelse

**Alternativt flöde:**
- Om e-postadressen inte finns i systemet visar systemet felmeddelande

### UC-003: Acceptera Inbjudan
**Aktör:** Inbjuden Användare

**Förutsättningar:**
- Användare har fått en inbjudan

**Huvudflöde:**
1. Användare ser notifiering om inbjudan
2. Användare klickar på notifieringen
3. Systemet visar information om sparmålet och inbjudan
4. Användare klickar på "Acceptera"
5. Systemet lägger till användaren som deltagare
6. Systemet uppdaterar inbjudningsstatus till "Accepted"
7. Systemet skickar notifiering till ägare och övriga deltagare
8. Systemet visar sparmålets detaljvy

**Alternativt flöde:**
- Användare klickar på "Avvisa" och inbjudningen avvisas

### UC-004: Skapa Förslag
**Aktör:** Deltagare

**Förutsättningar:**
- Användare är deltagare i ett gemensamt sparmål

**Huvudflöde:**
1. Deltagare öppnar sparmålets detaljvy
2. Deltagare klickar på "Skapa Förslag"
3. Systemet visar förslagsformulär
4. Deltagare väljer typ av förslag (t.ex. "Ändra Målbelopp")
5. Deltagare anger nytt värde och motivering
6. Deltagare klickar på "Skapa Förslag"
7. Systemet skapar förslag med status "Pending"
8. Systemet skickar notifieringar till alla deltagare
9. Systemet visar bekräftelse

### UC-005: Rösta på Förslag
**Aktör:** Deltagare

**Förutsättningar:**
- Det finns ett aktivt förslag för sparmålet
- Användare har inte röstat på förslaget ännu

**Huvudflöde:**
1. Deltagare ser notifiering om nytt förslag eller öppnar sparmålets detaljvy
2. Deltagare läser förslagets detaljer
3. Deltagare klickar på "Godkänn" eller "Avvisa"
4. Systemet registrerar rösten
5. Om alla deltagare har godkänt:
   - Systemet genomför ändringen
   - Systemet uppdaterar förslagsstatus till "Approved"
   - Systemet skickar notifieringar till alla deltagare
6. Om någon deltagare avvisar:
   - Systemet uppdaterar förslagsstatus till "Rejected"
   - Systemet skickar notifieringar till alla deltagare
7. Systemet visar bekräftelse

### UC-006: Uppdatera Sparbelopp
**Aktör:** Deltagare

**Förutsättningar:**
- Användare är deltagare i ett gemensamt sparmål

**Huvudflöde:**
1. Deltagare öppnar sparmålets detaljvy
2. Deltagare klickar på "Uppdatera Belopp"
3. Systemet visar dialog med nuvarande belopp
4. Deltagare anger belopp att lägga till eller ta bort
5. Deltagare anger valfri kommentar
6. Deltagare klickar på "Spara"
7. Systemet uppdaterar CurrentAmount
8. Systemet skapar en transaktion i historiken
9. Om ändringen är större än tröskelvärde (t.ex. 1000 kr):
   - Systemet skickar notifieringar till alla deltagare
10. Systemet visar uppdaterad status

### UC-007: Lämna Sparmål
**Aktör:** Deltagare

**Förutsättningar:**
- Användare är deltagare i ett gemensamt sparmål
- Om användaren är ägare måste det finnas andra deltagare att överföra ägarskap till

**Huvudflöde:**
1. Deltagare öppnar sparmålets detaljvy
2. Deltagare klickar på "Lämna Sparmål"
3. Om användaren är ägare:
   - Systemet visar dialog för att välja ny ägare
   - Deltagare väljer ny ägare från listan
4. Systemet begär bekräftelse
5. Deltagare bekräftar
6. Systemet tar bort deltagaren från sparmålet
7. Om ägare: Systemet överför ägarskap
8. Systemet skickar notifieringar till övriga deltagare
9. Systemet navigerar till översikten över gemensamma sparmål

**Alternativt flöde:**
- Om deltagaren är ensam kvar arkiveras målet istället

## 8. Tekniska Överväganden

### 8.1 Databasdesign
- Indexera foreign keys för bättre prestanda
- Indexera ofta efterfrågade fält (UserId, SharedGoalId, Status)
- Använd cascade delete där det är lämpligt
- Överväg soft delete för sparmål (arkivering)

### 8.2 Notifieringar
- Implementera med SignalR för realtidsnotifieringar
- Fallback till polling för äldre webbläsare
- Lagra notifieringar i databas för historik
- Implementera batch-uppdateringar för att minska belastning

### 8.3 Säkerhet
- Validera att användare har åtkomst till sparmål vid varje operation
- Använd authorization policies i ASP.NET Core
- Sanitera all användarinput
- Logga alla viktiga operationer för audit trail

### 8.4 Prestanda
- Använd eager loading för relaterade data (Include())
- Implementera caching för ofta hämtad data
- Paginering för stora listor
- Optimistisk låsning för samtidiga uppdateringar

### 8.5 Integration med Befintliga Funktioner
- Gemensamma sparmål är separata från individuella sparmål
- Använd samma BankSource för koppling till sparkonton
- Använd befintligt notifieringssystem om sådant finns, annars skapa nytt
- Återanvänd befintliga UI-komponenter där möjligt

## 9. Testplan

### 9.1 Enhetstester
- Testa alla service-metoder
- Testa validering av input
- Testa business logic för förslag och röstning
- Testa notifieringsgenerering

### 9.2 Integrationstester
- Testa API endpoints
- Testa databasoperationer
- Testa autentisering och auktorisering

### 9.3 UI-tester
- Testa skapande av gemensamt sparmål
- Testa inbjudningsflöde
- Testa förslagsflöde
- Testa uppdatering av belopp
- Testa notifieringar
- Testa responsiv design

### 9.4 Manuella tester
- Testa hela användarflödet end-to-end
- Testa med flera användare samtidigt
- Testa felhantering
- Testa prestanda med stor datamängd

## 10. Implementationsplan

### Fas 1: Datamodell och Backend
1. Skapa datamodeller (SharedGoal, SharedGoalParticipant, etc.)
2. Uppdatera DbContext
3. Skapa services (ISharedGoalService, SharedGoalService)
4. Implementera business logic för förslag och röstning
5. Skapa API controllers
6. Enhetstester för services

### Fas 2: Frontend - Grundfunktioner
1. Skapa SharedGoals.razor (översiktssida)
2. Skapa SharedGoalDetails.razor (detaljvy)
3. Implementera formulär för att skapa sparmål
4. Implementera visning av status och framsteg
5. Implementera deltagarlista

### Fas 3: Frontend - Avancerade Funktioner
1. Implementera inbjudningssystem
2. Implementera förslagssystem
3. Implementera röstning på förslag
4. Implementera uppdatering av sparbelopp
5. Implementera transaktionshistorik

### Fas 4: Notifieringar
1. Implementera notifieringsmodell
2. Implementera notifieringsservice
3. Integrera notifieringar i UI
4. Implementera realtidsuppdateringar (SignalR eller polling)

### Fas 5: Test och Dokumentation
1. Integrationstester
2. UI-tester
3. Manuell testning
4. Användarguide
5. API-dokumentation

## 11. Framtida Utökningar

### 11.1 Version 2.0
- Automatiska månatliga insättningar
- Integration med bankapi för automatisk sparande
- Mål-mallar (semester, bil, boende, etc.)
- Visualisering av historiska trender
- Export av data till Excel/PDF

### 11.2 Version 3.0
- Gamification (badges, achievements)
- Delning på sociala medier (frivilligt)
- Jämförelse med liknande mål (anonymiserat)
- AI-baserade rekommendationer för sparmål
- Mobilapp

## 12. Bilagor

### 12.1 Ordlista
- **Sparmål:** Ett finansiellt mål som användare sparar pengar för
- **Gemensamt Sparmål:** Ett sparmål som delas mellan flera användare
- **Deltagare:** En användare som är med i ett gemensamt sparmål
- **Ägare:** Den användare som skapade det gemensamma sparmålet
- **Förslag:** En föreslagen ändring av sparmålets parametrar
- **Notifiering:** Ett meddelande till användare om en händelse

### 12.2 Referenser
- ASP.NET Core Identity: https://docs.microsoft.com/aspnet/core/security/authentication/identity
- Entity Framework Core: https://docs.microsoft.com/ef/core/
- MudBlazor: https://mudblazor.com/
- SignalR: https://docs.microsoft.com/aspnet/core/signalr/introduction

### 12.3 Versionshistorik
- v1.0 (2025-10-21): Initial kravspecifikation
