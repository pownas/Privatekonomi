# Återkommande Uppgifter och Kanban - Flödesdiagram

## Översikt av Kanban Workflow

```mermaid
stateDiagram-v2
    [*] --> ToDo: Ny uppgift skapas
    ToDo --> InProgress: Börja arbeta (→)
    InProgress --> ToDo: Flytta tillbaka (←)
    InProgress --> Done: Slutför uppgift (✓)
    Done --> InProgress: Ångra (↶)
    Done --> [*]: Uppgift klar
    
    note right of Done
        Om återkommande:
        Skapa ny uppgift i ToDo
    end note
```

## Återkommande Uppgifter - Dataflöde

```mermaid
sequenceDiagram
    participant User
    participant UI as HouseholdDetails
    participant Service as HouseholdService
    participant DB as Database
    
    User->>UI: Skapa uppgift (IsRecurring=true)
    UI->>Service: CreateTaskAsync(task)
    Service->>DB: INSERT HouseholdTask
    DB-->>Service: Task saved
    Service-->>UI: Task created
    UI-->>User: Uppgift visas i ToDo-kolumn
    
    Note over User,DB: Senare: Användare slutför uppgift
    
    User->>UI: Flytta till "Klart" (✓)
    UI->>Service: UpdateTaskStatusAsync(taskId, Done)
    Service->>Service: Check if IsRecurring
    Service->>Service: CreateNextRecurrenceAsync(taskId)
    Service->>Service: CalculateNextDueDate()
    Service->>DB: INSERT new task (NextDueDate)
    Service->>DB: UPDATE original task (CompletedDate)
    DB-->>Service: Tasks updated
    Service-->>UI: Status updated + new task created
    UI-->>User: Uppgift flyttad till Klart<br/>Ny uppgift i ToDo
```

## Beräkning av Nästa Förfallodatum

```mermaid
flowchart TD
    A[Ursprungligt DueDate] --> B{RecurrencePattern?}
    B -->|Daily| C[+interval dagar]
    B -->|Weekly| D[+interval*7 dagar]
    B -->|BiWeekly| E[+interval*14 dagar]
    B -->|Monthly| F[+interval månader]
    B -->|Quarterly| G[+interval*3 månader]
    B -->|Yearly| H[+interval år]
    
    C --> I[Nytt DueDate]
    D --> I
    E --> I
    F --> I
    G --> I
    H --> I
    
    I --> J[Skapa ny uppgift med nytt DueDate]
    J --> K[Status = ToDo]
    K --> L[ParentTaskId = originalTaskId]
```

## Kanban Board Komponentstruktur

```mermaid
graph TB
    A[HouseholdDetails.razor] --> B{Kanban-vy aktiverad?}
    B -->|Ja| C{Kategori vald?}
    B -->|Nej| D[Traditionell listvy]
    
    C -->|Ja| E[En KanbanBoard för vald kategori]
    C -->|Nej| F[KanbanBoard för varje kategori med uppgifter]
    
    E --> G[KanbanBoard.razor]
    F --> G
    
    G --> H[ToDo-kolumn]
    G --> I[InProgress-kolumn]
    G --> J[Done-kolumn]
    
    H --> K[Uppgiftskort 1]
    H --> L[Uppgiftskort 2]
    I --> M[Uppgiftskort 3]
    J --> N[Uppgiftskort 4]
    
    K --> O[Åtgärdsknappar:<br/>Edit, Move→, Delete]
    M --> P[Åtgärdsknappar:<br/>←Move, Edit, Done✓, Delete]
    N --> Q[Åtgärdsknappar:<br/>Undo↶, Delete]
```

## Användarinteraktion - Kanban

```mermaid
sequenceDiagram
    participant U as Användare
    participant KB as KanbanBoard
    participant HS as HouseholdService
    participant DB as Database
    
    U->>KB: Klicka "→" på uppgift i ToDo
    KB->>HS: UpdateTaskStatusAsync(taskId, InProgress)
    HS->>DB: UPDATE Status = InProgress
    DB-->>HS: OK
    HS-->>KB: Status uppdaterad
    KB->>KB: OnTasksChanged callback
    KB->>HS: GetTasksGroupedByStatusAsync()
    HS->>DB: SELECT ... GROUP BY Status
    DB-->>HS: Grupperade uppgifter
    HS-->>KB: Uppdaterad data
    KB-->>U: UI uppdateras, uppgift visas i InProgress-kolumn
```

## Databas Schema (Nya fält)

```mermaid
erDiagram
    HouseholdTask {
        int HouseholdTaskId PK
        int HouseholdId FK
        string Title
        string Description
        int Status "NEW: ToDo/InProgress/Done"
        bool IsRecurring "NEW"
        int RecurrencePattern "NEW: Daily/Weekly/Monthly etc"
        int RecurrenceInterval "NEW: 1, 2, 3..."
        datetime NextDueDate "NEW"
        int ParentTaskId "NEW: FK to original task"
        datetime DueDate
        datetime CompletedDate
        int AssignedToMemberId FK
        int CompletedByMemberId FK
    }
    
    HouseholdTask ||--o{ HouseholdTask : "parent-child recurring"
```

## State Management - Uppgiftsstatus

```mermaid
stateDiagram-v2
    state "Uppgift i ToDo" as s1
    state "Uppgift i InProgress" as s2
    state "Uppgift i Done" as s3
    
    state s1 {
        [*] --> NotStarted
        NotStarted --> Assigned: Tilldela medlem
        Assigned --> HighPriority: Sätt hög prioritet
    }
    
    state s2 {
        [*] --> Working
        Working --> NearDeadline: DueDate närmar sig
        NearDeadline --> Overdue: DueDate passerad
    }
    
    state s3 {
        [*] --> Completed
        Completed --> RecurringProcessed: If IsRecurring
        RecurringProcessed --> NextInstanceCreated
    }
    
    s1 --> s2: Start arbete
    s2 --> s3: Slutför
    s2 --> s1: Flytta tillbaka
    s3 --> s2: Ångra slutförande
```

## Kategorifiltrering

```mermaid
flowchart LR
    A[Alla hushållsuppgifter] --> B{Kategorifilter aktivt?}
    B -->|Nej| C[Visa alla kategorier]
    B -->|Ja| D[Filtrera efter vald kategori]
    
    C --> E[Städning KanbanBoard]
    C --> F[Underhåll KanbanBoard]
    C --> G[Inköp KanbanBoard]
    C --> H[Matlagning KanbanBoard]
    
    D --> I[En KanbanBoard för vald kategori]
    
    E --> J[Gruppera per Status]
    F --> J
    G --> J
    H --> J
    I --> J
```

## Exempel: Veckovis Städning

```mermaid
gantt
    title Återkommande Städuppgift - Veckovis
    dateFormat YYYY-MM-DD
    section Vecka 1
    Städa badrum (ursprunglig)    :done, task1, 2025-01-06, 1d
    section Vecka 2
    Städa badrum (förekomst 1)    :active, task2, 2025-01-13, 1d
    section Vecka 3
    Städa badrum (förekomst 2)    :task3, 2025-01-20, 1d
    section Vecka 4
    Städa badrum (förekomst 3)    :task4, 2025-01-27, 1d
```

## Responsiv Layout

```mermaid
graph TB
    subgraph Desktop[Desktop View - Horisontell]
        D1[ToDo-kolumn<br/>33% bredd] 
        D2[InProgress-kolumn<br/>33% bredd]
        D3[Done-kolumn<br/>33% bredd]
    end
    
    subgraph Mobile[Mobile View - Vertikal]
        M1[ToDo-kolumn<br/>100% bredd]
        M2[InProgress-kolumn<br/>100% bredd]
        M3[Done-kolumn<br/>100% bredd]
    end
    
    A[Screen Size] --> B{Width > 960px?}
    B -->|Yes| Desktop
    B -->|No| Mobile
```

## Säkerhet och Dataisolering

```mermaid
flowchart TD
    A[Användare loggar in] --> B[Väljer hushåll]
    B --> C{Medlem i hushåll?}
    C -->|Nej| D[Åtkomst nekad]
    C -->|Ja| E[Hämta uppgifter]
    
    E --> F[Filter: HouseholdId = ValthushållId]
    F --> G[Returnera endast uppgifter för detta hushåll]
    
    G --> H{Uppdatera uppgift?}
    H -->|Ja| I[Validera HouseholdId]
    I --> J{Match?}
    J -->|Nej| K[Åtkomst nekad]
    J -->|Ja| L[Uppdatering tillåten]
```
