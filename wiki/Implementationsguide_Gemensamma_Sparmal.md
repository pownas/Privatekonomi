# Implementationsguide: Gemensamma Sparmål

## Översikt
Detta dokument beskriver implementationen av funktionen för gemensamma sparmål (shared savings goals) i Privatekonomi-applikationen.

## Implementerade komponenter

### 1. Datamodeller (Core/Models)

#### SharedGoal.cs
Huvudmodellen för gemensamma sparmål med följande egenskaper:
- `SharedGoalId`: Unikt ID
- `Name`: Namn på sparmålet
- `Description`: Beskrivning
- `TargetAmount`: Målbelopp
- `CurrentAmount`: Aktuellt sparat belopp
- `TargetDate`: Målsättningsdatum
- `Priority`: Prioritet (1-5)
- `Status`: Status (Active, Completed, Archived)
- `CreatedByUserId`: Skapare av målet
- Navigation properties för deltagare, förslag, transaktioner och notifieringar

#### SharedGoalParticipant.cs
Deltagare i ett gemensamt sparmål:
- `Role`: Owner eller Participant
- `InvitationStatus`: Pending, Accepted eller Rejected
- `JoinedAt`: När användaren gick med
- `InvitedByUserId`: Vem som bjöd in

#### SharedGoalProposal.cs
Förslag för ändringar:
- `ProposalType`: ChangeTargetAmount, ChangeTargetDate, ChangeName, etc.
- `CurrentValue`: Nuvarande värde
- `ProposedValue`: Föreslaget värde
- `Status`: Pending, Approved, Rejected, Withdrawn

#### SharedGoalProposalVote.cs
Röster på förslag:
- `Vote`: Approve eller Reject
- `Comment`: Valfri kommentar
- `VotedAt`: När rösten lades

#### SharedGoalTransaction.cs
Transaktionshistorik:
- `Amount`: Belopp
- `Type`: Deposit eller Withdrawal
- `Description`: Beskrivning
- `TransactionDate`: När transaktionen gjordes

#### SharedGoalNotification.cs
Notifieringar till deltagare:
- `Type`: Olika typer av händelser (invitation, proposal, transaction, etc.)
- `Message`: Meddelande
- `IsRead`: Läst eller ej
- `CreatedAt`: När notifieringen skapades

### 2. Databaskonfiguration

Uppdaterad `PrivatekonomyContext.cs` med:
- DbSet för alla nya modeller
- Entity Framework konfiguration för:
  - Foreign keys och relations
  - Index för prestanda
  - Cascade delete beteende
  - Unique constraints (t.ex. en användare per sparmål)

### 3. Service Layer

#### ISharedGoalService.cs
Interface med följande metodkategorier:

**CRUD operationer:**
- `GetAllSharedGoalsAsync()`: Hämta alla användarens gemensamma mål
- `GetSharedGoalByIdAsync(id)`: Hämta specifikt mål
- `CreateSharedGoalAsync(sharedGoal)`: Skapa nytt mål
- `UpdateSharedGoalAsync(sharedGoal)`: Uppdatera mål
- `DeleteSharedGoalAsync(id)`: Ta bort mål

**Deltagarhantering:**
- `GetParticipantsAsync(sharedGoalId)`: Lista deltagare
- `InviteParticipantAsync(sharedGoalId, userEmail)`: Bjud in
- `AcceptInvitationAsync(participantId)`: Acceptera inbjudan
- `RejectInvitationAsync(participantId)`: Avvisa inbjudan
- `RemoveParticipantAsync(sharedGoalId, userId)`: Ta bort deltagare
- `LeaveSharedGoalAsync(sharedGoalId)`: Lämna sparmål
- `TransferOwnershipAsync(sharedGoalId, newOwnerUserId)`: Överför ägarskap
- `IsParticipantAsync(sharedGoalId, userId)`: Kontrollera deltagande
- `IsOwnerAsync(sharedGoalId, userId)`: Kontrollera ägarskap

**Förslagshantering:**
- `GetProposalsAsync(sharedGoalId)`: Lista förslag
- `CreateProposalAsync(proposal)`: Skapa förslag
- `VoteOnProposalAsync(proposalId, vote, comment)`: Rösta
- `WithdrawProposalAsync(proposalId)`: Dra tillbaka förslag
- `CheckAndApplyProposalAsync(proposalId)`: Kontrollera och verkställ

**Transaktionshantering:**
- `GetTransactionsAsync(sharedGoalId)`: Hämta historik
- `CreateTransactionAsync(transaction)`: Skapa transaktion
- `UpdateGoalAmountAsync(sharedGoalId, amount, type, description)`: Uppdatera belopp

**Notifieringar:**
- `GetNotificationsAsync()`: Hämta alla notifieringar
- `GetUnreadNotificationsAsync()`: Hämta olästa
- `MarkNotificationAsReadAsync(notificationId)`: Markera som läst
- `DeleteNotificationAsync(notificationId)`: Ta bort
- `CreateNotificationAsync(...)`: Skapa notifiering

**Statistik:**
- `GetTotalProgressAsync(sharedGoalId)`: Beräkna framsteg
- `GetActiveSharedGoalsAsync()`: Hämta aktiva mål

#### SharedGoalService.cs
Fullständig implementation med:

**Säkerhet:**
- Användarautentisering via `ICurrentUserService`
- Verifiering av åtkomsträttigheter för varje operation
- Rollbaserad åtkomstkontroll (Owner vs Participant)

**Business Logic:**
- Automatisk notifieringsgenerering vid händelser
- Förslagsgodkännande kräver enhällighet
- Automatisk målkomplettering vid 100% framsteg
- Arkivering av mål när sista deltagaren lämnar
- Ägarskapsöverföring vid lämning

**Prestanda:**
- Eager loading av relaterad data (Include)
- Indexering i databas för snabba sökningar

### 4. Frontend Komponenter

#### SharedGoals.razor
Översiktssida med:
- **Tabbar:** Aktiva, Avslutade, Arkiverade mål
- **Väntande inbjudningar:** Kort för att acceptera/avvisa
- **Notifieringsindikator:** Antal olästa notifieringar
- **Målkort:** Visar namn, framsteg, antal deltagare, prioritet
- **Navigering:** Klick på kort öppnar detaljvy
- **Skapa-knapp:** Öppnar dialog för nytt mål

Funktioner:
- Laddar alla användarens gemensamma mål
- Filtrerar på status
- Acceptera/avvisa inbjudningar direkt från sidan

#### SharedGoalDetails.razor
Detaljvy med tabbar:

**Översikt-flik:**
- **Statistikkort:** Målbelopp, Sparat, Framsteg, Återstår
- **Uppdatera belopp:** Formulär för insättning/uttag
- **Information:** Prioritet, datum, status

**Deltagare-flik:**
- **Tabell:** Visar alla deltagare med roller och status
- **Åtgärder (Owner):** Ta bort deltagare
- **Bjud in-knapp (Owner):** Öppnar inbjudningsdia log

**Historik-flik:**
- **Transaktionstabell:** Datum, användare, typ, belopp, kommentar
- Sorterad i omvänd kronologisk ordning

**Förslag-flik:**
- **Skapa förslag-knapp:** Ny dialog
- **Förslagstabell:** Typ, föreslagen av, värden, status, röster
- **Röstning:** Tumme upp/ner för pendande förslag
- **Rösträknare:** X av Y deltagare har röstat

Funktioner:
- Breadcrumb-navigation
- Lämna-knapp (med bekräftelse)
- Realtidsuppdatering efter ändringar
- Färgkodade framstegsindikatorer

#### CreateSharedGoalDialog.razor
Dialog för att skapa nytt gemensamt sparmål:
- **Fält:** Namn, Beskrivning, Målbelopp, Startbelopp, Datum, Prioritet
- **Validering:** Namn och målbelopp krävs
- **Returnerar:** SharedGoal-objekt vid skapande

### 5. Navigering

Uppdaterad `NavMenu.razor` med:
- Nytt menyalternativ "Gemensamma Sparmål"
- Ikon: Groups
- Placering: Efter "Sparmål"

### 6. Dependency Injection

Registrering i `Program.cs`:
```csharp
builder.Services.AddScoped<ISharedGoalService, SharedGoalService>();
```

## Användningsflöden

### Skapa nytt gemensamt sparmål
1. Användare navigerar till `/sharedgoals`
2. Klickar på "Nytt Gemensamt Sparmål"
3. Fyller i dialog med måldetaljer
4. Klickar "Skapa"
5. Systemet skapar mål och lägger till användaren som Owner
6. Navigering till detaljvy

### Bjuda in deltagare
1. Owner öppnar sparmålets detaljvy
2. Klickar "Bjud In"
3. Anger e-postadress
4. Systemet:
   - Söker efter användare
   - Skapar participant med status Pending
   - Skickar notifiering till inbjuden användare
5. Inbjuden användare:
   - Ser inbjudan på översiktssidan
   - Kan acceptera eller avvisa

### Skapa och rösta på förslag
1. Deltagare öppnar förslag-fliken
2. Klickar "Nytt Förslag"
3. Väljer typ och anger nytt värde
4. Systemet skapar förslag och notifierar alla
5. Andra deltagare:
   - Ser förslag i listan
   - Röstar Godkänn eller Avslå
6. När alla röstat:
   - Om alla godkänt: Ändring verkställs automatiskt
   - Om någon avvisat: Förslag markeras rejected

### Uppdatera sparbelopp
1. Deltagare öppnar översikt-fliken
2. Anger belopp och kommentar
3. Klickar "Sätt In" eller "Ta Ut"
4. Systemet:
   - Skapar transaktion
   - Uppdaterar CurrentAmount
   - Om stort belopp (≥1000 kr): Notifierar andra
   - Om 100% uppnått: Markerar som Completed

## Säkerhetsfunktioner

### Åtkomstkontroll
- Endast deltagare kan se sparmål
- Endast Owner kan:
  - Bjuda in nya deltagare
  - Ta bort deltagare
  - Ta bort hela sparmålet
- Alla deltagare kan:
  - Skapa förslag
  - Rösta på förslag
  - Uppdatera sparbelopp
  - Lämna sparmålet

### Validering
- E-postadress måste finnas i systemet för inbjudan
- Användare kan inte bjudas in två gånger
- Förslag kan bara dras tillbaka av upphovsperson
- Owner kan inte tas bort utan ägarskapsöverföring först

### Audit Trail
- Alla transaktioner loggas med användare och tidstämpel
- Förslag och röster sparas
- Notifieringar ger historik över händelser

## Prestanda

### Optimeringar
- **Eager Loading:** Include() används för att minimera databasanrop
- **Indexering:** Foreign keys och ofta sökta fält indexeras
- **Batch Operations:** Notifieringar skapas effektivt
- **Caching:** Användardata cachas i sessionen

### Skalbarhet
- Databasdesign stöder tusentals mål och användare
- Notifieringssystem kan enkelt bytas till SignalR för realtid
- Paginering kan läggas till för stora datamängder

## Framtida Förbättringar

### Version 1.1
- [ ] Komplett inbjudningsdialog med validering
- [ ] Komplett förslagsdialog med dropdown för typ
- [ ] Notifieringscenter-sida (`/sharedgoals/notifications`)
- [ ] Realtidsuppdateringar med SignalR
- [ ] E-postnotifieringar

### Version 1.2
- [ ] API endpoints för mobilapp
- [ ] Automatiska månatliga insättningar
- [ ] Mål-mallar (semester, bil, boende)
- [ ] Exportera till PDF/Excel
- [ ] Grafer och visualiseringar

### Version 2.0
- [ ] Gamification (badges, achievements)
- [ ] AI-baserade sparrekommendationer
- [ ] Integration med bankapi för automatisk sparande
- [ ] Mobilapp (iOS/Android)

## Testning

### Manuella tester att utföra

1. **Skapa sparmål:**
   - Skapa nytt gemensamt sparmål
   - Verifiera att du blir Owner
   - Kontrollera att målet visas i Aktiva

2. **Inbjudningar:**
   - Logga in som användare A
   - Skapa sparmål och bjud in användare B
   - Logga in som användare B
   - Verifiera inbjudan visas
   - Acceptera inbjudan
   - Verifiera att B blir Participant

3. **Insättningar:**
   - Sätt in belopp som deltagare A
   - Verifiera transaktion i historik
   - Verifiera uppdaterat belopp
   - Logga in som B och verifiera synlighet

4. **Förslag:**
   - Skapa förslag som A
   - Verifiera notifiering till B
   - Rösta som A och B
   - Verifiera att ändring verkställs när alla röstat

5. **Lämna/ta bort:**
   - Testa lämna som Participant
   - Testa ta bort deltagare som Owner
   - Testa ägarskapsöverföring

### Enhetstest att implementera

```csharp
// SharedGoalServiceTests.cs

[Fact]
public async Task CreateSharedGoal_ShouldAddCreatorAsOwner()
{
    // Arrange
    var service = CreateService();
    var goal = new SharedGoal { Name = "Test", TargetAmount = 1000 };
    
    // Act
    var result = await service.CreateSharedGoalAsync(goal);
    
    // Assert
    Assert.NotNull(result);
    var participants = await service.GetParticipantsAsync(result.SharedGoalId);
    Assert.Single(participants);
    Assert.Equal(ParticipantRole.Owner, participants.First().Role);
}

[Fact]
public async Task InviteParticipant_ShouldCreatePendingInvitation()
{
    // Test implementation
}

[Fact]
public async Task VoteOnProposal_AllApprove_ShouldApplyChange()
{
    // Test implementation
}

// ... more tests
```

## Felsökning

### Vanliga problem

**Problem:** "User is not authenticated"
- **Lösning:** Kontrollera att `ICurrentUserService` är korrekt konfigurerad och användaren är inloggad

**Problem:** "Invitation not found"
- **Lösning:** Verifiera att participantId är korrekt och tillhör inloggad användare

**Problem:** Förslag verkställs inte trots alla röster
- **Lösning:** Kontrollera att alla deltagare har status Accepted (inte Pending)

**Problem:** Notifieringar visas inte
- **Lösning:** Kör `GetUnreadNotificationsAsync()` och verifiera data i databasen

## Teknisk dokumentation

### Databasschemat

```sql
-- SharedGoals
CREATE TABLE SharedGoals (
    SharedGoalId INT PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    TargetAmount DECIMAL(18,2) NOT NULL,
    CurrentAmount DECIMAL(18,2) NOT NULL,
    TargetDate DATETIME2,
    Priority INT NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2,
    CreatedByUserId NVARCHAR(450) NOT NULL,
    FOREIGN KEY (CreatedByUserId) REFERENCES AspNetUsers(Id)
);

-- SharedGoalParticipants
CREATE TABLE SharedGoalParticipants (
    SharedGoalParticipantId INT PRIMARY KEY,
    SharedGoalId INT NOT NULL,
    UserId NVARCHAR(450) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    InvitationStatus NVARCHAR(50) NOT NULL,
    JoinedAt DATETIME2 NOT NULL,
    InvitedByUserId NVARCHAR(450),
    InvitedAt DATETIME2,
    FOREIGN KEY (SharedGoalId) REFERENCES SharedGoals(SharedGoalId) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
    UNIQUE (SharedGoalId, UserId)
);

-- ... more tables
```

### API kontrakt (om endpoints skapas)

```http
POST /api/sharedgoals
Content-Type: application/json

{
  "name": "Sommarsemester 2026",
  "description": "Resa till Italien",
  "targetAmount": 50000,
  "currentAmount": 0,
  "targetDate": "2026-06-01",
  "priority": 2
}

Response: 201 Created
{
  "sharedGoalId": 1,
  "name": "Sommarsemester 2026",
  ...
}
```

## Kontakt och Support

För frågor om implementationen, kontakta utvecklingsteamet eller öppna ett issue på GitHub.

## Versionshistorik

- **v1.0 (2025-10-21):** Initial implementation
  - Datamodeller
  - Service layer
  - Frontend komponenter
  - Grundläggande funktionalitet
