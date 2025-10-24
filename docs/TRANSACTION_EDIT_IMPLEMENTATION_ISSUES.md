# Issue Templates för Transaktionsredigering - Implementation

Dessa templates kan användas för att skapa GitHub Issues som följer kravspecifikationen.

## 🏗️ Epic: Transaktionsredigering med Kategorival och Multi-kategori Uppdelning

**Beskrivning**: Implementera fullständig redigering av transaktioner inklusive kategoribyte, multi-kategori split och förbättrad export.

**Acceptance Criteria**:
- [ ] Användare kan redigera alla tillåtna fält på transaktioner
- [ ] Kategoribyte fungerar med validering och audit-logg
- [ ] Multi-kategori split med belopp/procent-validering
- [ ] Förbättrad CSV/JSON export med kategorier
- [ ] Optimistic locking för concurrent updates
- [ ] Komplett audit trail för alla ändringar

**Related Spec**: `docs/TRANSACTION_EDIT_SPEC.md`

---

## 🎯 Story 1: Backend API - Transaction Update Endpoint

**Title**: Förbättra PUT /api/transactions/{id} för fullständig redigering

**As a** användare  
**I want to** kunna uppdatera alla redigerbara fält på en transaktion  
**So that** jag kan korrigera och förtydliga mina transaktionsuppgifter  

### Acceptance Criteria
- [ ] PUT endpoint accepterar alla redigerbara fält (amount, date, description, payee, notes, tags, categories)
- [ ] Optimistic locking med UpdatedAt/ETag kontroll
- [ ] Input validering (belopp > 0, datum giltigt, beskrivning obligatorisk)
- [ ] Audit-logg skapas för alla ändringar med before/after values
- [ ] 409 Conflict vid concurrent modification
- [ ] 403 Forbidden för låsta transaktioner

### Technical Tasks
- [ ] Utöka TransactionController.UpdateTransaction med alla fält
- [ ] Implementera optimistic locking kontroll
- [ ] Lägg till audit-logg i AuditLogService för transaktionsändringar
- [ ] Enhetstest för validering och edge cases
- [ ] Integrationstester för API endpoint

### Definition of Done
- [ ] Endpoint implementerat enligt OpenAPI spec
- [ ] Unit tests >80% coverage
- [ ] Integration tests för happy path och error cases
- [ ] Audit logging fungerar och är testade
- [ ] API dokumentation uppdaterad

---

## 🎯 Story 2: Multi-kategori Split Backend

**Title**: Implementera multi-kategori split för transaktioner

**As a** användare  
**I want to** kunna dela en transaktion över flera kategorier  
**So that** jag kan korrekt kategorisera transaktioner som täcker flera utgiftsområden  

### Acceptance Criteria
- [ ] En transaktion kan ha flera TransactionCategory-poster
- [ ] Summan av category amounts = transaction amount (validering)
- [ ] Procentsatser beräknas automatiskt
- [ ] Split-ändringar loggas i audit trail
- [ ] Original kategori ersätts/uppdateras korrekt

### Technical Tasks
- [ ] Utöka PUT /api/transactions/{id}/categories endpoint
- [ ] Implementera validering för split-summor
- [ ] Beräkna procentsatser automatiskt baserat på belopp
- [ ] Hantera avrundning (sista kategorin får resten)
- [ ] Audit logging för kategoriändringar med detaljer

### Definition of Done
- [ ] Split validering fungerar korrekt
- [ ] Procent-beräkning är exakt
- [ ] Audit trail visar tydlig före/efter information
- [ ] Enhetstester för edge cases (avrundning, tomma listor)

---

## 🎯 Story 3: Frontend - Förbättrad EditTransactionDialog

**Title**: Utöka redigeringsmodal med alla fält och validering

**As a** användare  
**I want to** ha en intuitiv modal för att redigera transaktioner  
**So that** jag snabbt kan göra ändringar direkt från transaktionslistan  

### Acceptance Criteria
- [ ] Modal innehåller alla redigerbara fält
- [ ] Realtidsvalidering med felmeddelanden
- [ ] Kategoriväljare med sök och hierarki-visning
- [ ] Split-panel expanderar när "Split"-knapp klickas
- [ ] Responsiv design för mobil och desktop
- [ ] Laddningsstatus och error handling

### Technical Tasks
- [ ] Utöka EditTransactionDialog.razor med nya fält
- [ ] Implementera kategoriväljare-komponent med sök
- [ ] Skapa split-panel-komponent med add/remove funktionalitet
- [ ] Lägg till realtidsvalidering med MudBlazor validators
- [ ] Responsiv CSS för mobil-anpassning
- [ ] Error handling och user feedback (toasts, alerts)

### Definition of Done
- [ ] Modal fungerar enligt wireframes
- [ ] Alla validationsregler implementerade
- [ ] Responsiv design testad på olika skärmstorlekar
- [ ] Accessibility (ARIA-labels, keyboard navigation)
- [ ] E2E-tester för användningsfall

---

## 🎯 Story 4: Split-funktionalitet Frontend

**Title**: Implementera multi-kategori split UI

**As a** användare  
**I want to** kunna dela transaktioner grafiskt  
**So that** jag kan enkelt fördela utgifter över flera kategorier  

### Acceptance Criteria
- [ ] Split-panel med rader för varje kategori
- [ ] "Lägg till kategori" och "ta bort"-knappar
- [ ] Realtidsvalidering av summor med visuell feedback
- [ ] Både belopp och procent kan anges
- [ ] Bekräftelsedialog innan sparande av split

### Technical Tasks
- [ ] Skapa SplitTransactionComponent.razor
- [ ] Implementera add/remove kategori-rader
- [ ] Realtidsberäkning av summor och procent
- [ ] Visuell indikator för över/under-summering
- [ ] Bekräftelsedialog med sammandrag
- [ ] Hantera avrundning på frontend

### Definition of Done
- [ ] Split UI matchar wireframes
- [ ] Real-time validering fungerar smidigt
- [ ] Procent ↔ belopp konvertering är korrekt
- [ ] Användarupplevelsetest genomfört

---

## 🎯 Story 5: Förbättrad Export med Filter

**Title**: Utöka CSV/JSON export med kategorier och filter

**As a** användare  
**I want to** kunna exportera transaktioner med kategoridetaljer  
**So that** jag kan analysera mina utgifter i externa verktyg  

### Acceptance Criteria
- [ ] CSV innehåller kategorinamn och split-information
- [ ] JSON export med fullständig transaktionsdata
- [ ] Filter: datumintervall, kategorier, belopp
- [ ] Streaming för stora dataset (>10k poster)
- [ ] UTF-8 encoding och svenska tecken fungerar

### Technical Tasks
- [ ] Utöka ExportController med filter-parametrar
- [ ] Lägg till kategoridata i export-format
- [ ] Implementera streaming för stora exports
- [ ] Skapa frontend export-dialog med filter-UI
- [ ] Hantera split-transaktioner i export

### Definition of Done
- [ ] Export-format enligt specification
- [ ] Filter-funktionalitet testad
- [ ] Performance-test för 50k+ transaktioner
- [ ] Export-dialog i frontend

---

## 🎯 Story 6: Audit Trail och Historik

**Title**: Implementera komplett audit trail för transaktionsändringar

**As a** systemadministratör  
**I want to** kunna spåra alla ändringar av transaktioner  
**So that** vi har fullständig revision och användare kan se ändringshistorik  

### Acceptance Criteria
- [ ] Alla transaktionsändringar loggas med före/efter-värden
- [ ] Audit trail inkluderar användar-ID, tidstämpel och IP-adress
- [ ] Historik-vy i frontend visar tidigare versioner
- [ ] Kategoriändringar och splits loggas detaljerat

### Technical Tasks
- [ ] Utöka AuditLogService för transaktions-specifik loggning
- [ ] Skapa TransactionHistoryComponent.razor
- [ ] API endpoint för att hämta audit trail för transaktion
- [ ] Formatera diff-visning av ändringar

### Definition of Done
- [ ] Komplett audit trail för alla ändringstyper
- [ ] Historik-UI visar tydliga före/efter-värden
- [ ] Audit data kan exporteras för compliance

---

## 🎯 Story 7: E2E Testning och Performance

**Title**: Komplett testning av transaktionsredigering

**As a** utvecklingsteam  
**I want to** ha fullständig testtäckning  
**So that** vi kan vara säkra på att funktionaliteten fungerar korrekt  

### Acceptance Criteria
- [ ] E2E-tester för alla användarscenarier
- [ ] Performance-tester för export och stora datasets
- [ ] Cross-browser kompatibilitet
- [ ] Mobile responsiveness validerat

### Technical Tasks
- [ ] Playwright tester för redigering, split och export
- [ ] Performance benchmarks för concurrent updates
- [ ] Browser-tester (Chrome, Firefox, Safari, Edge)
- [ ] Mobile E2E-tester

### Definition of Done
- [ ] >95% E2E test pass rate
- [ ] Performance mål uppnådda (API <500ms, export streaming)
- [ ] Cross-browser kompatibilitet verifierad

---

## 📋 Checklista för Implementation

### Backend (API & Database)
- [ ] TransactionsController uppdateringar
- [ ] Optimistic locking implementation
- [ ] AuditLogService utökningar
- [ ] Export API förbättringar
- [ ] Unit tests (>80% coverage)
- [ ] Integration tests
- [ ] API dokumentation

### Frontend (UI Components)
- [ ] EditTransactionDialog förbättringar
- [ ] SplitTransactionComponent
- [ ] CategoryPickerComponent
- [ ] TransactionHistoryComponent
- [ ] Export-dialog
- [ ] Responsiv CSS
- [ ] Accessibility (WCAG 2.1)

### Testing & Quality
- [ ] Unit tests backend
- [ ] Unit tests frontend (Blazor)
- [ ] Integration tests
- [ ] E2E tests (Playwright)
- [ ] Performance tests
- [ ] Cross-browser testing
- [ ] Mobile testing

### Documentation
- [ ] API dokumentation uppdaterad
- [ ] Användarguide för redigering
- [ ] Utvecklardokumentation
- [ ] Deployment guide

### Deployment & Monitoring
- [ ] Database migrations
- [ ] Configuration updates
- [ ] Monitoring/logging för nya endpoints
- [ ] Feature flags för gradvis rollout
- [ ] Rollback plan

---

## 🔧 Development Environment Setup

För att arbeta med transaktionsredigering:

```bash
# 1. Branching
git checkout -b feature/transaction-editing
git checkout -b feature/transaction-splitting
git checkout -b feature/export-improvements

# 2. Database migrations (om behövs)
dotnet ef migrations add AddTransactionAuditFields
dotnet ef database update

# 3. Frontend development
cd src/Privatekonomi.Web
dotnet watch run

# 4. Testing
dotnet test
cd tests/playwright && npm test

# 5. API testing
cd tests && ./api-tests.sh
```

## 📈 Success Metrics

- **User Experience**: Task completion rate >95% för transaktionsredigering
- **Performance**: API response time <300ms för updates
- **Audit**: 100% av ändringar loggas korrekt
- **Export**: Stödjer export av >100k transaktioner
- **Error Rate**: <1% fel vid concurrent updates
- **Accessibility**: WCAG 2.1 AA compliance

---

**Specs**: Se `docs/TRANSACTION_EDIT_SPEC.md` för fullständiga krav och wireframes.
**Wireframes**: `docs/wireframes_transaction_edit.svg`, `docs/wireframes_mobile_transaction_edit.svg`