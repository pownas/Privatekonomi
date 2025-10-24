## Transaktionsredigering — kravspecifikation

Datum: 2025-10-24

Syfte
- Möjliggöra för användare att redigera enskilda transaktioner i systemet, inklusive att byta kategori kopplad till transaktionen. Funktionaliteten ska vara säker, spårbar och tillgänglig enligt WCAG 2.1 AA.

Mål
- Smidig och säker redigering av transaktioner (belopp, datum, beskrivning etc).
- Möjlighet att byta kategori och att ändringen reflekteras i budget/rapporter.
- Bevara dataintegritet och historik (audit-logg).
- Export av transaktioner till CSV och JSON.

Översikt av nuvarande funktion
- Nuvarande vy visar transaktioner (lista/tabell) med begränsade redigeringsmöjligheter. (Se UI-skärmbild i bilaga/sprintticket.)

1) Funktionella krav

1.1 Grundläggande redigering
- Användaren ska kunna redigera följande fält på en transaktion:
  - belopp (decimal, eventuell valuta)
  - datum
  - beskrivning/notes
  - motpart/partner (text)
  - kategori
  - eventuella tags

1.2 Kategoribyte
- Användaren ska kunna byta kategori via en sökbar dropdown/lista.
- Kategoriväljaren ska visa nivå (om hierarkiska kategorier finns), färg/ikon och senaste använd frekvens.
- Systemet måste validera att ny kategori finns och är aktiv innan ändring sparas.
- Ett byte av kategori ska kunna trigga uppdatering av aggregerade data (budget, rapporter) och eventuella notifieringar.

1.3 Behörighet & begränsningar
- Endast användare med skriv-behörighet för konto/transaktion får redigera.
- Vissa transaktioner kan vara låsta (t.ex. markerade som "reconcilied", importerade från bank med låsflagga eller äldre än konfigurerbart antal dagar). Låsta transaktioner kan inte redigeras utan admin-override.
- Systemet ska erbjuda en tydlig förklaring i UI varför en transaktion är låst.

1.4 Spårbarhet (audit logg)
- Varje ändring ska loggas: transaction_id, changed_by, changed_at, changes (fält före->efter som JSON), ändringsorsak (valfritt textfält).
- Auditposter ska vara skrivskyddade och tillgängliga via API och UI för administratörer.

1.5 Datavalidering
- Belopp måste vara numeriskt (inte 0 om policy så kräver).
- Datum måste vara ett giltigt datum inom rimligt intervall (t.ex. +/- 10 år från idag), eller enligt kontots konfigurerade periodrestriktioner.
- Vid kategoribyte måste ny kategori inte vara markerad som inaktiv eller raderad.

1.6 Concurrency
- Implementera optimistic concurrency: PUT/UPDATE ska kräva en version-tagg (t.ex. updatedAt eller rowversion). Vid konflikt returneras 409 med databasens senaste värde.

2) Specifika krav för kategoribyte

2.1 UI/UX
- Kategoriändring ska vara snabb — använd sökbar autosuggest med keyboard support.
- Visa visuella signaler om bytet påverkar budget (t.ex. "Den här ändringen påverkar Budget: Hemma -500 kr/månad").
- Bekräftelsemodal om ändringen påverkar tidigare periodrapporter eller är reversibel.

2.2 Semantik och följder
- Bytet uppdaterar transaktionens kategori_id och skapar audit-logg.
- Aggrerade värden som summeringar per kategori ska uppdateras i realtid eller enligt batch-jobb (specificera implementation i teknisk översikt).
- Om transaktionen ingår i en matchad household-transaction måste relaterade poster uppdateras eller flaggas för manuell granskning.

3) Eventuella begränsningar
- Låsta transaktioner (se 1.3) — kräver särskilt godkännande eller admin-override.
- Historik måste bevaras; ingen ändring får radera ursprungliga värden.
- Ändring av transaktioner äldre än X dagar kan vara begränsat av bokföringspolicy (konfigurera X per org).

4) UX / Designkrav för redigeringsflödet

4.1 Rekommenderade flöden (prioriterade)
- Sidopanel (recommended desktop): klick på "Redigera" öppnar en höger-sidopanel med formulär för fält. Fördelen: behåller kontext, bra för snabb redigering.
- Fullskärmsmodal (mobile): på mindre skärmar öppnas en fullskärmsmodal.
- Inline-edit (kompletterande): snabba ändringar (belopp, kategori) direkt i tabellen för power-users.

4.2 Tillgänglighet
- Alla fält ska vara keyboard-navigerbara och etiketter ska följa WCAG 2.1 AA.
- Kontrastkrav för färger, stora klickytor och meningsfulla ARIA-attribut för category-sök.

4.3 Visuell feedback
- Spara-knapp med spinner och state (sparar, sparad, fel).
- Visa toast/alert vid lyckad ändring och länk till "Visa ändringshistorik".
- Markera fält som ändrats (t.ex. med svag bakgrundsfärg) före spar.

4.4 Wireframes (enkla skisser)
- Lista (desktop): [Datum] [Belopp] [Kategori] [Beskrivning] [Åtgärder(Edit)]
- Klick Edit -> höger sidopanel:
  - Header: "Redigera transaktion"
  - Fält: Datum | Belopp | Valuta | Kategori (sökbar) | Beskrivning | Orsak (valfritt) 
  - Footer: Avbryt | Spara

5) API och databas (förslag)

5.1 Endpoints
- GET /api/transactions/{id}
  - Returnerar transaction med audit-metadata
- PUT /api/transactions/{id}
  - Payload: { amount, date, description, categoryId, tags[], version, reason? }
  - 200 OK med uppdaterad resurs eller 409 Conflict (version mismatch)
- GET /api/transactions/{id}/audit
  - Returnerar array av auditposter
- GET /api/transactions/export?format=csv|json&from=2025-01-01&to=2025-10-24&filters=...

5.2 Databasförändringar
- Ny tabell: transaction_edits
  - id (PK), transaction_id (FK), changed_by, changed_at, changes_json, reason, ip_address (optional)
- Möjlig migration: lägg till kolumn last_edited_by, last_edited_at i transactions för snabb vy.

5.3 Transaktionellt beteende
- Uppdateringar ska ske inom DB-transaction; skriv till audit-tabellen i samma transaction för att undvika förlust.

6) Export: CSV och JSON

6.1 Krav
- Exportera samma fält som list- och detaljvy: id, date, amount, currency, description, category_id, category_name, account_id, tags, created_at, updated_at, last_edited_by, last_edited_at.
- CSV ska använda UTF-8 med BOM eller dokumenteras tydligt. Fältdelare: komma (,) eller semikolon beroende på lokal inställning — API kan ha parameter `delimiter`.
- Filnamn: transactions_export_{from}_{to}_{timestamp}.{csv|json}

6.2 API
- GET /api/transactions/export?format=csv|json&from=yyyy-mm-dd&to=yyyy-mm-dd&filters=...
- Stöd för pagination eller streaming för stora dataset. För större export jobb, returnera job-id och möjliggör nedladdning när jobbet är klart.

6.3 Exempel CSV-header
"id","date","amount","currency","description","category_id","category_name","account_id","created_at","updated_at","last_edited_by","last_edited_at"

7) Testfall (urval)

7.1 Happy path — redigera fält
- Givet en transaktion T med kategori A
- När användaren ändrar belopp, datum och beskrivning och sparar
- Så ska transaktionen uppdateras och audit-post skapas med korrekt före/efter

7.2 Kategoribyte — giltig kategori
- Givet transaktion T
- När användaren byter kategori till B och bekräftar
- Så ska transaktionens category_id uppdateras, budget-summeringar uppdateras och audit-logg skapas

7.3 Kategoribyte — ogiltig/inaktiv kategori
- Försök att byta till en inaktiv kategori
- Systemet returnerar validation error (400) och ingen ändring sparas

7.4 Låst transaktion
- Försök att redigera en transaktion med locked=true
- Systemet returnerar 403 eller 423 (locked) och förklarar orsaken i UI

7.5 Concurrent edit
- Två klienter läser T med samma version
- Klient A sparar först (version++). Klient B försöker spara med gammal version
- B får 409 Conflict och uppmanas att läsa senaste data

7.6 Audit-logg test
- Efter varje lyckad ändring verifiera audit-rad innehåller fälten som ändrats, changed_by och changed_at

7.7 Export CSV/JSON
- Kör export för ett intervall, verifiera att fil innehåller korrekta kolumner, enkodning och att alla filter tillämpats

8) Acceptanskriterier
- Användare kan redigera transaktionens fält i UI och spara utan fel (happy path)
- Kategoribyte uppdaterar databas och relevanta summeringar
- Audit-logg skapas och kan visas
- Låsning/behörigheter hindrar otillåtna ändringar
- Export till CSV och JSON fungerar för normal dataset (inkl. enkla filter)
- UI uppfyller WCAG 2.1 AA grundläggande krav

9) Implementationstekniska rekommendationer
- Backend: skapa PUT endpoint med input-validering och optimistic concurrency (ETag/If-Match eller version i payload).
- DB: transaction_edits-tabell + optional last_edited_* på transactions.
- Frontend: återanvänd existerande transaction-form; implementera sidopanel för desktop + modal för mobil. Komponenter ska vara testbara.
- Jobb för att uppdatera aggregerade tabeller vid stora batch-ändringar (async) men små ändringar kan uppdatera realtime.

10) Rollout & risker
- Börja med feature-flag: slå på endast för testgrupper.
- Risk: felaktigt kategoribyte kan påverka budget/rapporter. Lösning: audit, möjlighet att ångra via admin eller fånga via daglig QA-rapport.

11) Nästa steg / review
- Granska denna spec med produktägare, backend, frontend och QA.
- Efter godkännande: skapa tekniska tickets: DB-migration, API-ändring, frontend-komponent, audit-logg, export-implementation, tester.

Bilagor
- UI-skärmbild: referera till ticket eller PR där screenshot finns bifogad.

---
Dokument skapat som utgångspunkt; iterera efter stakeholder-review.
