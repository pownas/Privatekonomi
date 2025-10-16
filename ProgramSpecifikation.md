# Programspecifikation för Privatekonomi-applikation

## 1. Sammanfattning
En privatekonomi-applikation med syfte att hjälpa användare att få koll och kontroll över sin ekonomi. Applikationen gör det möjligt att registrera inkomster och utgifter, kategorisera poster, samt automatiskt kategorisera nya poster baserat på tidigare inmatningar. Den stödjer dessutom att dela upp enskilda poster i upp till fem olika kategorier. Applikationen utvecklas med en **.NET-backend** och en **Blazor-baserad frontend**.

---

## 2. Funktionalitet

### 2.1. Grundläggande funktioner
1. **Registrera poster**:
   - Användaren kan lägga till inkomster och utgifter.
   - Varje post innehåller:
     - Datum
     - Belopp
     - Beskrivning
     - Kategori (eller flera kategorier)

2. **Automatisk kategorisering**:
   - Nya poster kategoriseras automatiskt baserat på tidigare poster med samma eller liknande beskrivning.

3. **Splitta poster**:
   - En post kan delas upp i upp till **5 olika kategorier** med procentuell eller beloppsbaserad fördelning.

4. **Rapporter och översikt**:
   - Visa en sammanställning av användarens ekonomi:
     - Diagram och grafer.
     - Uppdelning efter kategori.
     - Filtrering efter datum.

---

## **3. Användargränssnitt**

### **3.1. Frontend**
Frontend byggs med **Blazor WebAssembly** eller **Blazor Server** och ska vara responsiv och användarvänlig.

#### **Vykomponenter**:
1. **Dashboard**:
   - Översikt över senaste poster och total ekonomi.
   - Diagram som visar kategorifördelning och trender.

2. **Registrera ny post**:
   - Formulär för att lägga till nya poster med fält för belopp, beskrivning och kategori.

3. **Lista över poster**:
   - Tabell med alla poster.
   - Möjlighet att redigera eller ta bort poster.

4. **Rapporter**:
   - Detaljerade rapporter med filtreringsmöjligheter.

---

## **4. Backend**

### **4.1. Backend-struktur**
Backend byggs med **.NET Core/ASP.NET Core** och erbjuder ett REST API som hanterar frontendens kommunikation med databasen.

#### **Ansvarsområden**:
1. **Databaslagring**:
   - CRUD-operationer för poster och kategorier.
2. **Affärslogik**:
   - Automatisk kategorisering av nya poster.
   - Hantering av split-kategorisering.
3. **Autentisering** (valfritt):
   - Möjlighet till fleranvändarstöd med OAuth eller andra metoder.

---

## **5. Databasdesign**

### **5.1. Tabeller**
1. **Transactions** (Poster):
   - `TransactionId` (PK)
   - `UserId` (FK, om fleranvändarstöd implementeras)
   - `Amount` (Belopp)
   - `Description` (Beskrivning)
   - `Date` (Datum)
   - `CategoryId` (FK)

2. **Categories** (Kategorier):
   - `CategoryId` (PK)
   - `Name` (Kategorinamn)

3. **TransactionCategories** (Splittade kategorier):
   - `TransactionId` (FK)
   - `CategoryId` (FK)
   - `Percentage` eller `Amount`

4. **Users** (valfritt):
   - `UserId` (PK)
   - `Username`
   - `PasswordHash`

---

## **6. Teknisk stack**

### **6.1. Verktyg och teknologier**
- **Frontend**: Blazor WebAssembly eller Blazor Server
- **Backend**: .NET Core/ASP.NET Core
- **Databas**: SQL Server
- **UI-komponentbibliotek**: MudBlazor eller Radzen
- **Hosting**: Azure, AWS eller annan molnplattform

---

## **7. Utvecklingsplan**

### **7.1. Fas 1 – Grundläggande funktioner**
- Backend: Bygg ett API för att hantera poster och kategorier.
- Frontend: Implementera vyer för att registrera och visa poster.
- Databas: Design och implementera databasen.

### **7.2. Fas 2 – Automatisering**
- Implementera automatisk kategorisering av poster.
- Lägg till stöd för split-kategorisering.

### **7.3. Fas 3 – Rapporter och visualisering**
- Bygg vyer för rapporter och grafer.
- Lägg till filtreringsfunktionalitet.

### **7.4. Fas 4 – Testning och deployment**
- Testa applikationen (enhetstester och integrationstester).
- Deploya applikationen till produktionsmiljö.

---

## **8. Framtida funktioner**
- Integration med bank-API:er för automatisk import av transaktioner.
- Mobilapplikation med samma funktionalitet.
- Budgeteringsverktyg och målsparande.

---

## **9. Ikoner och UI-design**
- Fokusera på ett intuitivt gränssnitt med tydliga ikoner och färger för olika kategorier, t.ex.:
  - Grön för inkomster.
  - Röd för utgifter.
  - Blå för sparande.
