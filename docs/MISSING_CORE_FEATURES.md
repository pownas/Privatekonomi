# Saknade Kärnfunktioner - Analys och Implementationsplan

## Sammanfattning
Detta dokument analyserar de kärnfunktioner som efterfrågas i issue och identifierar vad som saknas i den nuvarande implementationen av Privatekonomi-applikationen.

## Funktionsöversikt

### 1. Bankkopplingar och Import ✅ 
**Status: Mestadels implementerad**

#### Implementerat:
- ✅ PSD2/Open Banking (Swedbank, ICA Banken, Avanza)
- ✅ CSV-import (ICA-banken, Swedbank, Avanza)
- ✅ Dubbletthantering vid import
- ✅ Automatisk synkronisering via API

#### Saknas:
- ❌ QIF-format import
- ❌ OFX-format import (nämns men ej implementerat)
- ⚠️ Offline manuell registrering (finns men inte framhävd i UI)
- ❌ Import från fler banker (Nordea, SEB, Handelsbanken)

#### Prioritet: Låg
De viktigaste importfunktionerna är implementerade. QIF/OFX kan läggas till vid behov.

---

### 2. Transaktionshantering ⚠️
**Status: Delvis implementerad**

#### Implementerat:
- ✅ Kategorier med färgkodning
- ✅ Taggar (fält finns i modellen)
- ✅ Split-kategorisering (dela upp transaktioner)
- ✅ Datum, belopp, beskrivning
- ✅ Bank/källa
- ✅ Automatisk kategorisering

#### Saknas:
- ❌ Noteringar/kommentarer per transaktion
- ❌ Bilagor/kvitton (fil-uppladdning)
- ❌ Slå ihop transaktioner (merge)
- ❌ Avancerad dubblettidentifiering (utanför import)
- ❌ Redigera befintliga transaktioner (UI)
- ❌ Recurring/återkommande transaktioner (modell finns, ej implementerad)

#### Prioritet: Medel-Hög
- **Bilagor**: Viktigt för kvittohantering
- **Noteringar**: Enkelt att implementera, stort värde
- **Slå ihop transaktioner**: Mindre vanligt use case
- **Redigera transaktioner**: Viktigt för användarupplevelse

---

### 3. Budget ⚠️
**Status: Grundläggande implementerad**

#### Implementerat:
- ✅ Kategoribaserad budget
- ✅ Månadsbudget och årsbudget
- ✅ Jämförelse planerat vs faktiskt
- ✅ Progress-visualisering
- ✅ Aktiva/kommande/avslutade budgetar

#### Saknas:
- ❌ Zero-based budgeting (varje krona tilldelas)
- ❌ 50/30/20-budgetmall (behov/önskemål/sparande)
- ❌ Kuvert/envelope budgeting
- ❌ Månadsrullning (överför oanvänt till nästa månad)
- ❌ Mål per kategori inom budget
- ❌ Kopiera budget från föregående period
- ❌ Budgetmallar

#### Prioritet: Hög
Budget är en kärnfunktion. Följande bör implementeras:
1. **Budgetmallar** (50/30/20, zero-based) - Höga prioritet
2. **Månadsrullning** - Höga prioritet för användare som vill rulla över
3. **Mål per kategori** - Medel prioritet
4. **Kuvert-budgeting** - Kan vänta, mer nisch

---

### 4. Mål & Buffert ⚠️
**Status: Grundläggande implementerad**

#### Implementerat:
- ✅ Sparmål med namn och beskrivning
- ✅ Målbelopp och nuvarande belopp
- ✅ Tidsgräns (target date)
- ✅ Prioritering (1-5)
- ✅ Progress-beräkning

#### Saknas:
- ❌ Automatisk "sweeping" (flytta överskott till sparande)
- ❌ Målstolpar/milestones (t.ex. 25%, 50%, 75%)
- ❌ Notifikationer vid milestones
- ❌ Flera sparkonton per mål
- ❌ Historik över insättningar

#### Prioritet: Medel
- **Målstolpar**: Enkelt att lägga till, motiverande
- **Automatisk sweeping**: Kräver mer logik, kan automatiseras med background job
- **Notifikationer**: Se sektion 6

---

### 5. Rapporter & Dashboards ⚠️
**Status: Grundläggande implementerad**

#### Implementerat:
- ✅ Totala inkomster/utgifter
- ✅ Nettoresultat
- ✅ Antal transaktioner
- ✅ Cirkeldiagram för utgiftsfördelning
- ✅ Stapeldiagram för utgifter per månad

#### Saknas:
- ❌ Kassaflödesanalys (inkomster/utgifter över tid)
- ❌ Trend-analys (stigande/fallande utgifter)
- ❌ Säsongsanalys (seasonality)
- ❌ Nettoförmögenhet (tillgångar - skulder)
- ❌ Heatmaps (t.ex. utgifter per dag/veckodag)
- ❌ Topp-handlare (mest pengar spenderat var)
- ❌ Kategori-jämförelse mellan månader
- ❌ Budget vs faktiskt över tid
- ❌ Genomsnittlig utgift per kategori
- ❌ Prognos baserat på historik

#### Prioritet: Medel-Hög
Rapporter är viktiga för insikt i ekonomin:
1. **Kassaflödesanalys** - Hög prioritet
2. **Nettoförmögenhet** - Hög prioritet (kräver tillgångar/skulder)
3. **Trend-analys** - Medel prioritet
4. **Topp-handlare** - Medel prioritet
5. **Heatmaps** - Låg prioritet (nice-to-have)

---

### 6. Aviseringar/Notifikationer ❌
**Status: Ej implementerad**

#### Implementerat:
- ❌ Ingen notifikationsfunktionalitet

#### Saknas:
- ❌ Låg balans-varning
- ❌ Ovanligt stor transaktion (outlier detection)
- ❌ Kommande räkningar (baserat på recurring)
- ❌ Budgetöverdrag
- ❌ Mål uppnått
- ❌ Synkroniseringsfel från bank
- ❌ E-post/push-notifikationer
- ❌ In-app notifikationer

#### Prioritet: Medel
Notifikationer förbättrar användarupplevelsen avsevärt:
1. **Budgetöverdrag** - Hög prioritet
2. **Kommande räkningar** - Medel prioritet
3. **Låg balans** - Medel prioritet
4. **Ovanliga transaktioner** - Låg prioritet (kräver ML/statistik)

#### Teknisk implementation:
- Skapa `Notification` modell
- Skapa `NotificationService`
- Implementera SignalR för real-time notifikationer
- Lägg till notifikations-center i UI
- Konfigurera e-post för kritiska notifikationer

---

### 7. Export & Backup ⚠️
**Status: Minimal implementerad**

#### Implementerat:
- ✅ Investering export till CSV
- ⚠️ Manuell backup via in-memory databas

#### Saknas:
- ❌ Transaktioner export till CSV
- ❌ Transaktioner export till JSON
- ❌ Budget export
- ❌ Full databackup (alla entiteter)
- ❌ Schemalagda/automatiska backups
- ❌ Import av backup
- ❌ Dataportabilitet mellan installationer
- ❌ Export till Excel (XLSX)

#### Prioritet: Medel-Hög
Data export är viktig för:
- Backup och återställning
- Dataportabilitet
- Externa analyser
- GDPR-compliance (rätt till data)

#### Implementationsförslag:
1. **Transaktioner export** - Hög prioritet
2. **Full backup/restore** - Hög prioritet
3. **Schemalagda backups** - Medel prioritet
4. **Excel export** - Låg prioritet

---

## Prioriterad Implementationsplan

### Fas 1: Kritiska saknade funktioner (MVP)
**Estimat: 2-3 veckor**

1. **Transaktionsnoteringar** ⭐⭐⭐
   - Lägg till `Notes` string-fält till Transaction
   - Uppdatera UI för att visa/redigera noteringar
   - Enkelt, stort värde

2. **Transaktioner export till CSV** ⭐⭐⭐
   - Implementera `TransactionExportService`
   - Lägg till "Exportera" knapp på Transactions-sidan
   - Viktigt för dataportabilitet

3. **Budgetmallar (50/30/20, zero-based)** ⭐⭐⭐
   - Skapa `BudgetTemplate` enum/modell
   - Implementera budget-skapande från mall
   - Hjälper användare komma igång

4. **Kassaflödesrapport** ⭐⭐⭐
   - Lägg till linjediagram på Dashboard
   - Visa inkomster/utgifter över tid
   - Kärnfunktion för översikt

### Fas 2: Viktiga förbättringar
**Estimat: 3-4 veckor**

5. **Notifikationssystem** ⭐⭐
   - Skapa Notification-modell och service
   - Implementera budgetöverdrag-notifikationer
   - Lägg till notifikations-center i UI
   - SignalR för real-time updates

6. **Bilagor/Kvitton** ⭐⭐
   - Skapa `TransactionAttachment` modell
   - Implementera fil-uppladdning
   - Visa bilagor i transaktionsdetaljer
   - Lagring: lokalt eller cloud (Azure Blob)

7. **Månadsrullning för budget** ⭐⭐
   - Lägg till `RolloverUnspent` boolean på Budget
   - Implementera logik för att rulla över oanvänt belopp
   - UI för att aktivera/inaktivera

8. **Nettoförmögenhet** ⭐⭐
   - Skapa NetWorth-rapport (tillgångar - skulder)
   - Använd befintlig Investment och Loan data
   - Lägg till på Dashboard

### Fas 3: Avancerade funktioner
**Estimat: 4-5 veckor**

9. **Målstolpar för sparmål** ⭐
   - Lägg till milestones till Goal
   - Visa progress med milestones
   - Notifikationer vid milestone

10. **Trend & Säsongsanalys** ⭐
    - Implementera statistisk analys
    - Visa trends på Dashboard
    - Identifiera säsongsmönster

11. **Topp-handlare rapport** ⭐
    - Gruppera transaktioner per payee/description
    - Visa topp 10 handlare
    - Lägg till på Dashboard eller ny Reports-sida

12. **Schemalagda backups** ⭐
    - Implementera background service
    - Exportera full databas till JSON
    - Konfigurerbar frekvens

### Fas 4: Nice-to-have
**Estimat: 2-3 veckor**

13. **QIF/OFX import**
14. **Heatmaps**
15. **Kuvert-budgeting**
16. **Automatisk sweeping**
17. **Slå ihop transaktioner**

---

## Tekniska Överväganden

### Nya Modeller som behövs:
```csharp
// Notifikationer
public class Notification
{
    public int NotificationId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; } // Info, Warning, Error
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; } // "Transaction", "Budget", etc.
}

// Bilagor
public class TransactionAttachment
{
    public int AttachmentId { get; set; }
    public int TransactionId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; } // eller BlobUrl
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public Transaction Transaction { get; set; }
}

// Budgetmallar
public enum BudgetTemplateType
{
    Custom,
    ZeroBased,
    FiftyThirtyTwenty,
    Envelope
}

// Målstolpar
public class GoalMilestone
{
    public int MilestoneId { get; set; }
    public int GoalId { get; set; }
    public decimal TargetAmount { get; set; }
    public string? Description { get; set; }
    public bool IsReached { get; set; }
    public DateTime? ReachedAt { get; set; }
    public Goal Goal { get; set; }
}
```

### Nya Services som behövs:
```csharp
public interface INotificationService
{
    Task<IEnumerable<Notification>> GetUnreadNotificationsAsync();
    Task CreateNotificationAsync(Notification notification);
    Task MarkAsReadAsync(int notificationId);
    Task DeleteNotificationAsync(int notificationId);
}

public interface IExportService
{
    Task<byte[]> ExportTransactionsToCsvAsync(DateTime? fromDate, DateTime? toDate);
    Task<byte[]> ExportTransactionsToJsonAsync(DateTime? fromDate, DateTime? toDate);
    Task<byte[]> ExportFullBackupAsync();
}

public interface IReportService
{
    Task<CashFlowReport> GetCashFlowReportAsync(DateTime fromDate, DateTime toDate);
    Task<NetWorthReport> GetNetWorthReportAsync();
    Task<TrendAnalysis> GetTrendAnalysisAsync(int categoryId, int months);
    Task<IEnumerable<TopMerchant>> GetTopMerchantsAsync(int limit);
}

public interface IAttachmentService
{
    Task<TransactionAttachment> UploadAttachmentAsync(int transactionId, Stream fileStream, string fileName, string contentType);
    Task<Stream> DownloadAttachmentAsync(int attachmentId);
    Task DeleteAttachmentAsync(int attachmentId);
}
```

---

## Sammanfattning

Av de 7 kategorierna av kärnfunktioner:
- **3 är väl implementerade** (Bankkopplingar, grundläggande transaktioner, grundläggande budget)
- **4 behöver betydande förbättringar** (Avancerad transaktionshantering, mål, rapporter, export)
- **1 saknas helt** (Notifikationer)

### Rekommendation:
Fokusera på **Fas 1 och 2** för att få en komplett MVP med de viktigaste kärnfunktionerna. Detta ger:
- Bättre transaktionshantering (noteringar, export)
- Bättre budgetfunktionalitet (mallar, rullning)
- Viktig rapportering (kassaflöde, nettoförmögenhet)
- Notifikationssystem för bättre användarupplevelse
- Bilagor för kvittohantering

**Total estimat för komplett kärnfunktionalitet: 9-12 veckor**

Detta dokument bör uppdateras när funktioner implementeras.
