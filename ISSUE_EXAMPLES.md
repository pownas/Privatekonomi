# Exempel på GitHub Issues från Förbättringsförslagen

Detta dokument innehåller färdiga GitHub Issue-beskrivningar baserade på **FÖRBÄTTRINGSFÖRSLAG_2025.md**. Kopiera och klistra in dessa direkt i GitHub Issues.

---

## Issue #1: Implementera Personaliserad Dashboard med Widget-system

**Labels:** `feature`, `dashboard`, `ux`, `high-priority`  
**Assignees:** (lägg till efter behov)  
**Milestone:** Fas 2 - Högt Värde  
**Estimat:** 7-10 dagar

### Beskrivning

Låt användare skapa egna dashboards genom att dra och släppa widgets för en mer personaliserad upplevelse.

### Bakgrund

- ✅ Dashboard finns men är statisk
- ❌ Användare kan inte anpassa layout
- 🎯 Personalisering förbättrar användarupplevelsen avsevärt

### Funktionalitet

**Core Features:**
- [x] Drag-and-drop för att ordna widgets
- [x] Välj bland 15+ olika widgets:
  - Nettoförmögenhet
  - Kassaflöde
  - Sparmål progress
  - Lån översikt
  - Investeringar
  - Budgetföljning
  - Kommande räkningar
  - Månadens utgifter
  - Trend-grafer
  - etc.
- [x] Spara flera dashboard-layouter (Hem, Investeringar, Budget)
- [x] Dela widgets mellan familjemedlemmar i hushåll
- [x] Responsiv layout som anpassar sig automatiskt

**User Stories:**
```
Som användare vill jag kunna:
1. Lägga till widgets från en widget-katalog
2. Dra och släppa widgets för att arrangera dem
3. Ta bort widgets jag inte använder
4. Spara min anpassade layout
5. Skapa flera dashboard-vyer för olika syften
```

### Teknisk Implementation

**Föreslagna verktyg:**
- **GridStack.js** - Drag and drop grid system
  - URL: https://gridstackjs.com/
  - Blazor wrapper finns tillgänglig
- **Alternativ: Muuri** - Responsive layout engine
  - URL: https://muuri.dev/

**Datamodell:**
```csharp
// DashboardLayout.cs
public class DashboardLayout
{
    public int LayoutId { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; } // "Hem", "Investeringar", etc.
    public bool IsDefault { get; set; }
    public List<WidgetConfiguration> Widgets { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// WidgetConfiguration.cs
public class WidgetConfiguration
{
    public int WidgetConfigId { get; set; }
    public int LayoutId { get; set; }
    public WidgetType Type { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Settings { get; set; } // JSON för widget-specifika inställningar
}

public enum WidgetType
{
    NetWorth,
    CashFlow,
    Goals,
    Loans,
    Investments,
    BudgetOverview,
    UpcomingBills,
    MonthlyExpenses,
    CategoryPieChart,
    TrendChart,
    QuickActions,
    MonthSummary
}
```

**Service:**
```csharp
public interface IDashboardLayoutService
{
    Task<IEnumerable<DashboardLayout>> GetUserLayoutsAsync(string userId);
    Task<DashboardLayout> GetDefaultLayoutAsync(string userId);
    Task<DashboardLayout> CreateLayoutAsync(DashboardLayout layout);
    Task UpdateLayoutAsync(DashboardLayout layout);
    Task DeleteLayoutAsync(int layoutId);
    Task SetDefaultLayoutAsync(int layoutId, string userId);
}
```

**UI Components:**

Skapa återanvändbara widget-komponenter:
```
src/Privatekonomi.Web/Components/
├── Widgets/
│   ├── WidgetBase.razor (base component)
│   ├── NetWorthWidget.razor
│   ├── CashFlowWidget.razor
│   ├── GoalsWidget.razor
│   ├── LoansWidget.razor
│   ├── InvestmentsWidget.razor
│   └── ...
└── Pages/
    └── CustomizableDashboard.razor
```

### Åtgärder

**Fas 1: Grundläggande Infrastructure (3 dagar)**
- [ ] Installera GridStack.js (eller Blazor wrapper)
- [ ] Skapa `DashboardLayout` och `WidgetConfiguration` modeller
- [ ] Skapa `DashboardLayoutService`
- [ ] Lägg till DbSet i `PrivatekonomyContext`
- [ ] Skapa migration

**Fas 2: Widget-komponenter (3 dagar)**
- [ ] Skapa `WidgetBase` component
- [ ] Implementera 5 grundläggande widgets:
  - [ ] NetWorthWidget
  - [ ] CashFlowWidget
  - [ ] GoalsWidget
  - [ ] BudgetOverviewWidget
  - [ ] QuickActionsWidget
- [ ] Widget-konfiguration (storlek, färg, etc.)

**Fas 3: Drag & Drop UI (2 dagar)**
- [ ] Integrera GridStack.js
- [ ] Implementera drag-and-drop funktionalitet
- [ ] Spara layout automatiskt
- [ ] Lägg till "Anpassa"-läge toggle

**Fas 4: Layout Management (2 dagar)**
- [ ] Skapa ny layout
- [ ] Radera layout
- [ ] Byt mellan layouts
- [ ] Sätt default layout
- [ ] Dela layout med hushållsmedlemmar (opt)

### Berörd Kod

- `src/Privatekonomi.Core/Models/DashboardLayout.cs` (ny)
- `src/Privatekonomi.Core/Models/WidgetConfiguration.cs` (ny)
- `src/Privatekonomi.Core/Services/DashboardLayoutService.cs` (ny)
- `src/Privatekonomi.Web/Components/Widgets/*` (nya)
- `src/Privatekonomi.Web/Components/Pages/Home.razor` (modifiera)
- `src/Privatekonomi.Core/Data/PrivatekonomyContext.cs` (uppdatera)

### Testning

- [ ] Enhetstester för `DashboardLayoutService`
- [ ] Testa drag-and-drop i olika browsers
- [ ] Testa responsiv layout på mobil
- [ ] Testa delning mellan användare
- [ ] Performance-test med många widgets

### Screenshots / Wireframes

_(Lägg till mockups här när färdiga)_

### Relaterade Issues

- Relaterad till: Nettoförmögenhet-widget (#3)
- Bygger på: Dashboard-anpassning (#existing)
- Förutsätter: Widget-komponenter

### Dokumentation

Efter implementation, uppdatera:
- [ ] README.md - Lägg till widget-funktion i features
- [ ] Skapa `docs/DASHBOARD_WIDGETS.md` - Guide för widgets
- [ ] Användarguide med screenshots

---

## Issue #2: Implementera ML-baserad Smart Kategorisering

**Labels:** `feature`, `ml`, `transactions`, `ai`, `high-priority`  
**Assignees:** (lägg till efter behov)  
**Milestone:** Fas 1 - Kritiska Förbättringar  
**Estimat:** 7-10 dagar

### Beskrivning

Förbättra automatisk kategorisering med maskininlärning som lär sig från användarens egna kategoriseringsmönster.

### Bakgrund

- ✅ Regelbaserad kategorisering finns redan
- ⚠️ 44+ förkonfigurerade regler, men inte personliga
- 🎯 ML kan lära sig användarens unika mönster
- 🎯 Förbättras automatiskt över tid

### Problem som Löses

1. **Regelbaserad begränsning**: Nuvarande system kan inte lära sig nya mönster
2. **Generiska regler**: Samma regler för alla användare
3. **Manuell uppdatering**: Användare måste manuellt skapa nya regler
4. **Låg precision**: Svårt att hantera edge cases

### Funktionalitet

**Core Features:**
- [x] Träna ML-modell på användarens egna data
- [x] Lär sig från användarens manuella kategoriseringar
- [x] Föreslå kategorier med konfidenspoäng (0-100%)
- [x] Markera som "osäker" om konfidensen är låg (<70%)
- [x] Kontinuerlig förbättring genom feedback-loop
- [x] Export av träningsdata för analys
- [x] Fallback till regelbaserad om ML ej tillämplig

**Användningsfall:**
```
Scenario 1: Ny transaktion från okänd källa
Input: "Pizzeria Milano - 285 kr"
ML Output:
  - Kategori: Mat & Dryck (Restaurang)
  - Konfidenspoäng: 92%
  - Förslag: ✓ Acceptera automatiskt

Scenario 2: Osäker transaktion
Input: "Gekås Ullared - 1,250 kr"
ML Output:
  - Kategori: Shopping (45%), Boende (30%), Mat (25%)
  - Konfidenspoäng: 45% (OSÄKER)
  - Förslag: ? Fråga användare
```

### Teknisk Implementation

**ML Framework:**
- **ML.NET** - Microsoft's ML framework för .NET
  - URL: https://dotnet.microsoft.com/apps/machinelearning-ai/ml-dotnet
  - Stöd för: Classification, Regression, Clustering
  - Integreras smidigt med befintlig .NET kod

**Algoritm:**
Föreslagna algoritmer i prioritetsordning:
1. **SDCA (Stochastic Dual Coordinate Ascent)** - Snabb multi-class classification
2. **LightGBM** - Gradient boosting för högre precision
3. **Naive Bayes** - Baseline, enkel och snabb

**Features för modellen:**
```csharp
public class TransactionFeatures
{
    // Text features (TF-IDF)
    [VectorType(1000)]
    public float[] DescriptionTfIdf { get; set; }
    
    // Numeric features
    public float Amount { get; set; }
    public float AmountLog { get; set; } // Log-transform för bättre distribution
    public float DayOfWeek { get; set; }
    public float DayOfMonth { get; set; }
    public float Hour { get; set; } // Om tidstämpel finns
    
    // Categorical features (one-hot encoded)
    public bool IsWeekend { get; set; }
    public bool IsMonthStart { get; set; }
    public bool IsMonthEnd { get; set; }
    
    // Label (target)
    [ColumnName("Label")]
    public string Category { get; set; }
}
```

**Model Training Pipeline:**
```csharp
public interface ITransactionMLService
{
    // Training
    Task TrainModelAsync(string userId);
    Task<ModelMetrics> EvaluateModelAsync(string userId);
    
    // Prediction
    Task<CategoryPrediction> PredictCategoryAsync(Transaction transaction);
    Task<List<CategoryPrediction>> PredictBatchAsync(List<Transaction> transactions);
    
    // Model management
    Task SaveModelAsync(string userId, string modelPath);
    Task LoadModelAsync(string userId);
    Task<bool> IsModelTrainedAsync(string userId);
    
    // Feedback loop
    Task UpdateModelWithFeedbackAsync(string userId, Transaction transaction, string correctCategory);
}

public class CategoryPrediction
{
    public string Category { get; set; }
    public float Confidence { get; set; } // 0-1
    public bool IsUncertain => Confidence < 0.7f;
    public Dictionary<string, float> AlternativeCategories { get; set; } // Top 3
}
```

**Datamodell:**
```csharp
// MLModel.cs
public class MLModel
{
    public int ModelId { get; set; }
    public string UserId { get; set; }
    public string ModelPath { get; set; } // Path to saved model file
    public DateTime TrainedAt { get; set; }
    public int TrainingRecordsCount { get; set; }
    public float Accuracy { get; set; }
    public float Precision { get; set; }
    public float Recall { get; set; }
    public string Metrics { get; set; } // JSON med detaljerad metrics
}

// UserFeedback.cs
public class UserFeedback
{
    public int FeedbackId { get; set; }
    public string UserId { get; set; }
    public int TransactionId { get; set; }
    public string PredictedCategory { get; set; }
    public float PredictedConfidence { get; set; }
    public string ActualCategory { get; set; } // Vad användaren valde
    public bool WasCorrectionNeeded { get; set; }
    public DateTime FeedbackDate { get; set; }
}
```

### Åtgärder

**Fas 1: ML Infrastructure (3 dagar)**
- [ ] Installera ML.NET NuGet package
- [ ] Skapa `ITransactionMLService` interface
- [ ] Implementera `TransactionMLService`
- [ ] Skapa modeller: `MLModel`, `UserFeedback`
- [ ] Setup model storage (lokal fil eller Azure Blob)

**Fas 2: Feature Engineering (2 dagar)**
- [ ] Implementera text preprocessing (TF-IDF)
- [ ] Extrahera numeric features från transaktioner
- [ ] Skapa feature transformation pipeline
- [ ] Testa feature quality

**Fas 3: Model Training (2 dagar)**
- [ ] Implementera training pipeline
- [ ] Cross-validation (80/20 split)
- [ ] Hyperparameter tuning
- [ ] Model evaluation metrics
- [ ] Save/Load model functionality

**Fas 4: Prediction Integration (2 dagar)**
- [ ] Integrera ML-predictions i TransactionService
- [ ] Fallback-logik till regelbaserad
- [ ] Confidence threshold handling
- [ ] Batch prediction för import

**Fas 5: UI & Feedback Loop (2 dagar)**
- [ ] Visa ML-förslag i UI med konfidenspoäng
- [ ] "Osäker"-indikator för låg confidence
- [ ] Feedback-knapp ("Rätt kategori", "Fel kategori")
- [ ] Auto-reträning varje natt (background service)

**Fas 6: Monitoring & Analytics (1 dag)**
- [ ] Model metrics dashboard
- [ ] Accuracy över tid
- [ ] Användnings-statistik
- [ ] Export träningsdata för analys

### Berörd Kod

- `src/Privatekonomi.Core/ML/TransactionMLService.cs` (ny)
- `src/Privatekonomi.Core/ML/ITransactionMLService.cs` (ny)
- `src/Privatekonomi.Core/Models/MLModel.cs` (ny)
- `src/Privatekonomi.Core/Models/UserFeedback.cs` (ny)
- `src/Privatekonomi.Core/Services/TransactionService.cs` (uppdatera)
- `src/Privatekonomi.Web/Components/Pages/Transactions.razor` (uppdatera)
- `src/Privatekonomi.Web/Components/Dialogs/EditTransactionDialog.razor` (uppdatera)

### Training Requirements

**Minimum krav:**
- Minst 50 kategoriserade transaktioner per användare
- Minst 5 exempel per kategori
- Om ej tillräcklig data: fallback till regelbaserad

**Optimal:**
- 200+ transaktioner för bättre precision
- Jämn fördelning över kategorier
- Historik över minst 3 månader

### Testning

- [ ] Enhetstester för `TransactionMLService`
- [ ] Testa med små dataset (<50 trans)
- [ ] Testa med stora dataset (>1000 trans)
- [ ] Test accuracy vs regelbaserad
- [ ] Performance-test (prediction latency)
- [ ] Test feedback loop
- [ ] Cross-user test (modeller ska inte läcka)

### Metrics & Success Criteria

**Target Metrics:**
- Accuracy: >85% (vs regelbaserad ~75%)
- Precision: >80%
- Recall: >80%
- Prediction latency: <100ms per transaktion
- Confidence calibration: 90% av "högkonfidens" ska vara korrekta

### Säkerhet & Privacy

- [ ] Modeller är user-specific (ingen delning mellan användare)
- [ ] Träningsdata lagras säkert
- [ ] GDPR: Rätt att radera modell och träningsdata
- [ ] Opt-in för ML (användare kan välja regelbaserad)

### Dokumentation

Efter implementation:
- [ ] README.md - Lägg till ML-kategorisering
- [ ] Skapa `docs/ML_CATEGORIZATION.md`
- [ ] API-dokumentation för `ITransactionMLService`
- [ ] Användarguide: "Hur ML-kategorisering fungerar"

### Framtida Förbättringar

- [ ] Deep Learning (LSTM/Transformer) för ännu bättre text-förståelse
- [ ] Transfer learning från andra användare (anonymiserat)
- [ ] Multi-label prediction (split categories)
- [ ] Anomali-detektion för ovanliga transaktioner

---

## Issue #3: Implementera PWA med Offline-stöd

**Labels:** `feature`, `mobile`, `pwa`, `offline`, `high-priority`  
**Assignees:** (lägg till efter behov)  
**Milestone:** Fas 1 - Kritiska Förbättringar  
**Estimat:** 8-10 dagar

### Beskrivning

Konvertera Privatekonomi till en installierbar Progressive Web App (PWA) med offline-funktionalitet för bättre mobilupplevelse.

### Bakgrund

- ❌ Applikationen fungerar bara online
- ❌ Kan inte installeras som app
- ❌ Ingen offline-funktionalitet
- 🎯 PWA ger app-liknande upplevelse utan App Store
- 🎯 Offline-support kritiskt för mobilanvändning

### Funktionalitet

**Core Features:**
- [x] Installierbar på mobil och desktop
- [x] Service Worker för caching
- [x] Offline-läge för läsning av data
- [x] Kö för transaktioner som skapas offline
- [x] Background sync när online igen
- [x] Push-notifikationer (via PWA)
- [x] App-ikon och splash screen
- [x] Offline-indikator i UI

### Teknisk Implementation

**PWA Components:**

1. **Web App Manifest** (`wwwroot/manifest.json`):
```json
{
  "name": "Privatekonomi",
  "short_name": "Privatekonomi",
  "description": "Hantera din privatekonomi enkelt",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#ffffff",
  "theme_color": "#594ae2",
  "orientation": "portrait-primary",
  "icons": [
    {
      "src": "/images/icon-72x72.png",
      "sizes": "72x72",
      "type": "image/png",
      "purpose": "any maskable"
    },
    {
      "src": "/images/icon-96x96.png",
      "sizes": "96x96",
      "type": "image/png"
    },
    {
      "src": "/images/icon-128x128.png",
      "sizes": "128x128",
      "type": "image/png"
    },
    {
      "src": "/images/icon-144x144.png",
      "sizes": "144x144",
      "type": "image/png"
    },
    {
      "src": "/images/icon-152x152.png",
      "sizes": "152x152",
      "type": "image/png"
    },
    {
      "src": "/images/icon-192x192.png",
      "sizes": "192x192",
      "type": "image/png"
    },
    {
      "src": "/images/icon-384x384.png",
      "sizes": "384x384",
      "type": "image/png"
    },
    {
      "src": "/images/icon-512x512.png",
      "sizes": "512x512",
      "type": "image/png"
    }
  ],
  "shortcuts": [
    {
      "name": "Ny Transaktion",
      "short_name": "Ny Trans",
      "description": "Skapa ny transaktion",
      "url": "/transactions/new",
      "icons": [{ "src": "/images/add-icon.png", "sizes": "96x96" }]
    },
    {
      "name": "Dashboard",
      "url": "/",
      "icons": [{ "src": "/images/home-icon.png", "sizes": "96x96" }]
    }
  ]
}
```

2. **Service Worker** (`wwwroot/service-worker.js`):
```javascript
const CACHE_NAME = 'privatekonomi-v1';
const urlsToCache = [
  '/',
  '/css/app.css',
  '/js/app.js',
  '/manifest.json',
  '/_framework/blazor.server.js'
];

// Install
self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => cache.addAll(urlsToCache))
  );
});

// Fetch - Network first, fallback to cache
self.addEventListener('fetch', event => {
  event.respondWith(
    fetch(event.request)
      .then(response => {
        // Clone and cache successful responses
        const responseClone = response.clone();
        caches.open(CACHE_NAME).then(cache => {
          cache.put(event.request, responseClone);
        });
        return response;
      })
      .catch(() => caches.match(event.request))
  );
});

// Background Sync
self.addEventListener('sync', event => {
  if (event.tag === 'sync-transactions') {
    event.waitUntil(syncTransactions());
  }
});

async function syncTransactions() {
  // Sync offline queue with server
  const db = await openDB();
  const transactions = await db.getAll('pending-transactions');
  
  for (const transaction of transactions) {
    try {
      await fetch('/api/transactions', {
        method: 'POST',
        body: JSON.stringify(transaction),
        headers: { 'Content-Type': 'application/json' }
      });
      await db.delete('pending-transactions', transaction.id);
    } catch (err) {
      console.error('Sync failed:', err);
    }
  }
}

// Push Notifications
self.addEventListener('push', event => {
  const data = event.data.json();
  const options = {
    body: data.message,
    icon: '/images/icon-192x192.png',
    badge: '/images/badge.png',
    vibrate: [200, 100, 200],
    data: {
      url: data.url
    }
  };
  
  event.waitUntil(
    self.registration.showNotification(data.title, options)
  );
});

self.addEventListener('notificationclick', event => {
  event.notification.close();
  event.waitUntil(
    clients.openWindow(event.notification.data.url)
  );
});
```

3. **IndexedDB för Offline Storage:**
```javascript
// wwwroot/js/offline-db.js
const DB_NAME = 'PrivatekonomyDB';
const DB_VERSION = 1;

function openDB() {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open(DB_NAME, DB_VERSION);
    
    request.onerror = () => reject(request.error);
    request.onsuccess = () => resolve(request.result);
    
    request.onupgradeneeded = event => {
      const db = event.target.result;
      
      // Stores
      if (!db.objectStoreNames.contains('transactions')) {
        const transactionStore = db.createObjectStore('transactions', { keyPath: 'transactionId' });
        transactionStore.createIndex('date', 'date', { unique: false });
        transactionStore.createIndex('userId', 'userId', { unique: false });
      }
      
      if (!db.objectStoreNames.contains('pending-transactions')) {
        db.createObjectStore('pending-transactions', { keyPath: 'localId', autoIncrement: true });
      }
      
      if (!db.objectStoreNames.contains('categories')) {
        db.createObjectStore('categories', { keyPath: 'categoryId' });
      }
      
      if (!db.objectStoreNames.contains('budgets')) {
        db.createObjectStore('budgets', { keyPath: 'budgetId' });
      }
    };
  });
}

// Blazor JSInterop
window.offlineDb = {
  async saveTransaction(transaction) {
    const db = await openDB();
    const tx = db.transaction('pending-transactions', 'readwrite');
    const store = tx.objectStore('pending-transactions');
    const id = await store.add(transaction);
    
    // Register background sync
    if ('serviceWorker' in navigator && 'sync' in self.registration) {
      await self.registration.sync.register('sync-transactions');
    }
    
    return id;
  },
  
  async getPendingTransactions() {
    const db = await openDB();
    const tx = db.transaction('pending-transactions', 'readonly');
    const store = tx.objectStore('pending-transactions');
    return await store.getAll();
  },
  
  async clearPendingTransactions() {
    const db = await openDB();
    const tx = db.transaction('pending-transactions', 'readwrite');
    const store = tx.objectStore('pending-transactions');
    return await store.clear();
  }
};
```

4. **Blazor Integration:**
```csharp
// Services/OfflineService.cs
public interface IOfflineService
{
    Task<bool> IsOnlineAsync();
    Task SaveForLaterAsync<T>(string key, T data);
    Task<T> GetOfflineDataAsync<T>(string key);
    Task SyncPendingChangesAsync();
    event EventHandler<bool> OnlineStatusChanged;
}

public class OfflineService : IOfflineService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ITransactionService _transactionService;
    private bool _isOnline = true;

    public event EventHandler<bool> OnlineStatusChanged;

    public OfflineService(IJSRuntime jsRuntime, ITransactionService transactionService)
    {
        _jsRuntime = jsRuntime;
        _transactionService = transactionService;
    }

    public async Task<bool> IsOnlineAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("navigator.onLine");
        }
        catch
        {
            return false;
        }
    }

    public async Task SaveForLaterAsync<T>(string key, T data)
    {
        await _jsRuntime.InvokeVoidAsync("offlineDb.saveTransaction", data);
    }

    public async Task<T> GetOfflineDataAsync<T>(string key)
    {
        return await _jsRuntime.InvokeAsync<T>("offlineDb.getPendingTransactions");
    }

    public async Task SyncPendingChangesAsync()
    {
        var pending = await _jsRuntime.InvokeAsync<List<Transaction>>("offlineDb.getPendingTransactions");
        
        foreach (var transaction in pending)
        {
            try
            {
                await _transactionService.CreateTransactionAsync(transaction);
            }
            catch (Exception ex)
            {
                // Log error but continue
                Console.WriteLine($"Failed to sync transaction: {ex.Message}");
            }
        }
        
        await _jsRuntime.InvokeVoidAsync("offlineDb.clearPendingTransactions");
    }
}
```

### Åtgärder

**Fas 1: PWA Manifest & Icons (1 dag)**
- [ ] Skapa `manifest.json`
- [ ] Generera app-ikoner (72x72 till 512x512)
- [ ] Skapa splash screens
- [ ] Lägg till manifest i `_Host.cshtml`

**Fas 2: Service Worker (2 dagar)**
- [ ] Skapa `service-worker.js`
- [ ] Implementera caching-strategier
- [ ] Registrera service worker i app
- [ ] Testa offline-läge

**Fas 3: IndexedDB Setup (2 dagar)**
- [ ] Implementera `offline-db.js`
- [ ] Skapa object stores
- [ ] Testa read/write

**Fas 4: Offline Queue (2 dagar)**
- [ ] Skapa `OfflineService`
- [ ] Implementera offline-kö för transaktioner
- [ ] Background sync registration
- [ ] Sync-logik

**Fas 5: UI Updates (1 dag)**
- [ ] Offline-indikator banner
- [ ] Pending-transactions badge
- [ ] Sync status icon
- [ ] Toast vid sync completion

**Fas 6: Push Notifications (2 dagar)**
- [ ] Implementera push subscription
- [ ] Server-side push API
- [ ] Notification permissions UI
- [ ] Test notifications

### Berörd Kod

- `src/Privatekonomi.Web/wwwroot/manifest.json` (ny)
- `src/Privatekonomi.Web/wwwroot/service-worker.js` (ny)
- `src/Privatekonomi.Web/wwwroot/js/offline-db.js` (ny)
- `src/Privatekonomi.Web/Services/OfflineService.cs` (ny)
- `src/Privatekonomi.Web/Pages/_Host.cshtml` (uppdatera)
- `src/Privatekonomi.Web/Components/Layout/MainLayout.razor` (offline-banner)

### Testning

- [ ] Test installation på Android
- [ ] Test installation på iOS
- [ ] Test installation på Desktop (Chrome, Edge)
- [ ] Test offline-läge
- [ ] Test background sync
- [ ] Test push notifications
- [ ] Lighthouse PWA audit (>90 score)

### Success Criteria

- ✅ Installierbar på alla plattformar
- ✅ Lighthouse PWA score >90
- ✅ Offline-läsning fungerar
- ✅ Offline-skapande med sync fungerar
- ✅ Push notifications fungerar
- ✅ App-ikon syns på hemskärm

### Dokumentation

- [ ] README.md - PWA-funktioner
- [ ] `docs/PWA_GUIDE.md` - Installationsguide
- [ ] Användarguide: "Installera som app"
- [ ] Användarguide: "Arbeta offline"

---

## Hur man Använder dessa Examples

1. **Välj ett issue** från exemplen ovan
2. **Gå till GitHub** → Issues → New Issue
3. **Kopiera hela innehållet** för det issue du valt
4. **Klistra in** i issue-beskrivningen
5. **Lägg till labels** som anges i template
6. **Assigna** till utvecklare
7. **Sätt milestone** om tillämpligt
8. **Submit** issue

## Nästa Steg

Fler issue-exempel finns i **FÖRBÄTTRINGSFÖRSLAG_2025.md**. Använd dessa templates som inspiration för att skapa dina egna issues baserat på förslagen i dokumentet.

---

**Senast uppdaterad:** 2025-10-28  
**Antal examples:** 3  
**Relaterade dokument:** FÖRBÄTTRINGSFÖRSLAG_2025.md, FUNKTIONSANALYS.md
