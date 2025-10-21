# Gemensamma Sparmål - Implementation Summary

## Översikt

En komplett implementering av funktionalitet för gemensamma sparmål (shared savings goals) i Privatekonomi-applikationen. Denna funktion möjliggör för flera användare att samarbeta kring ekonomiska mål.

## Snabbstart

### För användare
Läs [Användarguiden](Anvandardguide_Gemensamma_Sparmal.md) för detaljerade instruktioner om hur du använder funktionen.

### För utvecklare
Se [Implementationsguiden](Implementationsguide_Gemensamma_Sparmal.md) för teknisk dokumentation.

### För produktägare
Granska [Kravspecifikationen](Kravspecifikation_Gemensamma_Sparmal.md) för fullständiga funktionella och icke-funktionella krav.

## Huvudfunktioner

### 👥 Multi-user Collaboration
- Skapa gemensamma sparmål med flera deltagare
- Tydliga roller: Owner och Participant
- Inbjudningssystem med accept/reject

### 💰 Sparmålshantering
- Sätt målbelopp och datum
- Spåra framsteg i realtid
- Prioritera mål (1-5)
- Automatisk komplettering vid 100%

### 📊 Transaktionshistorik
- Fullständig logg av insättningar och uttag
- Visa vem, när och hur mycket
- Kommentarer på transaktioner
- Notifieringar vid stora belopp (≥1000 kr)

### 🗳️ Förslagssystem
- Demokratiska ändringar via förslag
- Omröstning med enhällighet
- Stöd för olika typer av ändringar:
  - Målbelopp
  - Målsättningsdatum
  - Namn och beskrivning
  - Prioritet

### 🔔 Notifieringar
- 13 olika typer av händelser
- Visuell notifieringsräknare
- Historik över alla notifieringar
- Markera som läst/oläst

### 🔒 Säkerhet
- Användarautentisering krävs
- Rollbaserad åtkomstkontroll
- Dataisolering mellan sparmål
- Audit trail för alla ändringar

## Arkitektur

```
┌─────────────────────────────────────────┐
│         Frontend (Blazor Server)        │
│  ┌────────────────────────────────────┐ │
│  │  SharedGoals.razor                 │ │  Översiktssida
│  │  - Aktiva/Avslutade/Arkiverade    │ │
│  │  - Väntande inbjudningar          │ │
│  └────────────────────────────────────┘ │
│  ┌────────────────────────────────────┐ │
│  │  SharedGoalDetails.razor           │ │  Detaljvy
│  │  - Översikt/Deltagare/Historik    │ │
│  │  - Förslag och röstning           │ │
│  └────────────────────────────────────┘ │
│  ┌────────────────────────────────────┐ │
│  │  CreateSharedGoalDialog.razor      │ │  Dialog
│  └────────────────────────────────────┘ │
└─────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────┐
│        Service Layer (Core)             │
│  ┌────────────────────────────────────┐ │
│  │  SharedGoalService                 │ │  Business Logic
│  │  - CRUD operations                 │ │
│  │  - Participant management          │ │
│  │  - Proposal & voting               │ │
│  │  - Notifications                   │ │
│  │  - Security & validation           │ │
│  └────────────────────────────────────┘ │
└─────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────┐
│      Data Layer (EF Core)               │
│  ┌────────────────────────────────────┐ │
│  │  SharedGoal                        │ │  Modeller
│  │  SharedGoalParticipant             │ │
│  │  SharedGoalProposal                │ │
│  │  SharedGoalProposalVote            │ │
│  │  SharedGoalTransaction             │ │
│  │  SharedGoalNotification            │ │
│  └────────────────────────────────────┘ │
└─────────────────────────────────────────┘
                    │
                    ▼
          ┌──────────────────┐
          │  InMemory DB     │
          │  (Development)   │
          └──────────────────┘
```

## Teknisk Stack

- **Backend:** ASP.NET Core 9.0, C#
- **Frontend:** Blazor Server, MudBlazor
- **Databas:** Entity Framework Core (InMemory för utveckling)
- **Autentisering:** ASP.NET Core Identity
- **UI Framework:** MudBlazor (Material Design)

## Kodstatistik

- **6** nya datamodeller
- **2** nya service-filer (interface + implementation)
- **3** nya frontend-komponenter
- **1** uppdaterad DbContext
- **30+** service-metoder
- **3** omfattande dokumentationsfiler

## Användarflöden

### 1. Skapa gemensamt sparmål
```
Användare → SharedGoals.razor
         → Klicka "Nytt Gemensamt Sparmål"
         → CreateSharedGoalDialog
         → Fyll i formulär
         → SharedGoalService.CreateSharedGoalAsync()
         → Blir Owner automatiskt
         → Navigera till detaljer
```

### 2. Bjud in deltagare
```
Owner → SharedGoalDetails.razor
      → Klicka "Bjud In"
      → Ange e-post
      → SharedGoalService.InviteParticipantAsync()
      → Skapa Participant (status: Pending)
      → Notifiering till inbjuden
```

### 3. Acceptera inbjudan
```
Inbjuden → SharedGoals.razor
         → Se väntande inbjudan
         → Klicka "Acceptera"
         → SharedGoalService.AcceptInvitationAsync()
         → Status → Accepted
         → Notifiering till alla
```

### 4. Skapa och rösta på förslag
```
Deltagare → SharedGoalDetails.razor
          → Förslag-fliken
          → Klicka "Nytt Förslag"
          → Välj typ och värde
          → SharedGoalService.CreateProposalAsync()
          → Notifiering till alla

Andra deltagare → Se förslag
                → Rösta (👍/👎)
                → SharedGoalService.VoteOnProposalAsync()
                → Om alla godkänt → Verkställ automatiskt
```

## Databasschemat

Relationerna mellan entiteterna:

```
ApplicationUser
      │
      ├─── CreatedByUser ───┐
      │                     │
      ├─── InvitedByUser    │
      │                     ▼
      └─── User ────── SharedGoalParticipant
                            │
                            │  SharedGoalId
                            ▼
                       SharedGoal ◄─── SharedGoalProposal
                            │                  │
                            │                  ▼
                            │           SharedGoalProposalVote
                            │
                            ├─── SharedGoalTransaction
                            │
                            └─── SharedGoalNotification
```

## API-kontrakt (Intern service layer)

```csharp
// Exempel på service-anrop

// Skapa sparmål
var goal = new SharedGoal 
{ 
    Name = "Sommarsemester", 
    TargetAmount = 50000 
};
var created = await _service.CreateSharedGoalAsync(goal);

// Bjud in
var participant = await _service.InviteParticipantAsync(
    goalId: 1, 
    userEmail: "erik@example.com"
);

// Rösta på förslag
var vote = await _service.VoteOnProposalAsync(
    proposalId: 1, 
    vote: VoteType.Approve,
    comment: "Bra idé!"
);

// Uppdatera belopp
await _service.UpdateGoalAmountAsync(
    sharedGoalId: 1,
    amount: 2000,
    type: TransactionType.Deposit,
    description: "Månadssparande"
);
```

## Säkerhet

### Autentisering
- Alla endpoints kräver inloggad användare
- ICurrentUserService används för att hämta aktuell användare

### Auktorisering
- Endast deltagare kan se sparmål
- Endast Owner kan bjuda in och ta bort
- Validering vid varje operation

### Dataskydd
- Foreign keys förhindrar felaktig dataåtkomst
- Cascade delete hanterar relaterade data
- Unique constraints förhindrar dubletter

## Prestanda

### Optimeringar
- Eager loading med Include() för relaterad data
- Indexering på foreign keys och ofta sökta fält
- Batch-operationer för notifieringar
- Optimistisk låsning för samtidiga uppdateringar

### Skalbarhet
- Stöd för tusentals sparmål och användare
- Paginering kan enkelt läggas till
- Caching-strategi kan implementeras
- SignalR kan läggas till för realtid

## Testning

Se [Implementationsguiden](Implementationsguide_Gemensamma_Sparmal.md) för:
- Manuella testscenarier
- Exempel på enhetstester
- Integrationstester
- Säkerhetstester

## Framtida förbättringar

### Kort sikt (v1.1)
- [ ] Komplett inbjudningsdialog
- [ ] Komplett förslagsdialog
- [ ] Notifieringscenter-sida
- [ ] Real-time med SignalR

### Medellång sikt (v1.2)
- [ ] E-postnotifieringar
- [ ] API endpoints för mobilapp
- [ ] Automatiska insättningar
- [ ] Export till PDF/Excel

### Lång sikt (v2.0)
- [ ] Mobilapp (iOS/Android)
- [ ] Gamification
- [ ] AI-rekommendationer
- [ ] Bankapi-integration

## Kända begränsningar

1. **Inbjudningar:** Endast registrerade användare kan bjudas in
2. **Real-time:** Uppdateringar kräver siduppdatering (SignalR planerad)
3. **E-post:** Inga e-postnotifieringar ännu
4. **Export:** Ingen export-funktion implementerad
5. **Mobile:** Ingen dedikerad mobilapp

## Felsökning

### Vanliga problem

**"User is not authenticated"**
- Kontrollera att ICurrentUserService fungerar
- Verifiera att användaren är inloggad

**"Proposal not approved despite all votes"**
- Kontrollera att alla har status Accepted (inte Pending)
- Verifiera att rösträknaren är korrekt

**Notifieringar visas inte**
- Kontrollera databas för notifieringar
- Verifiera att CreateNotificationAsync anropas

## Bidra

För att bidra till utvecklingen:
1. Läs implementationsguiden
2. Följ kodstandarder
3. Skriv tester för nya funktioner
4. Uppdatera dokumentation

## Licens

Se projektets huvudlicens.

## Kontakt

- GitHub Issues: Rapportera buggar och feature requests
- Pull Requests: Bidra med kod
- Diskussioner: Ställ frågor och dela idéer

---

**Version:** 1.0  
**Datum:** 2025-10-21  
**Status:** ✅ Implementerad och dokumenterad
