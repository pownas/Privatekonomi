# Transaktion: Redigering och kategoriändring

Datum: 2025-10-24
Författare: [Automatisk utkast]

## Syfte
Möjliggöra för användare att smidigt, säkert och spårbart redigera enskilda transaktioner i systemet, inklusive att byta den kategori som transaktionen är kopplad till. Funktionen ska även erbjuda export av transaktioner i CSV och JSON.

## Omfattning
- Redigering av enskilda transaktioner (belopp, valuta, datum, beskrivning, motpart, kategori, tags, konto, eventuella metadata).
- Byte av kategori med sök/auto-suggest och validering.
- Audit-logg av alla ändringar (vem, när, vad ändrades, tidigare värden).
- Begränsningar för låsta eller synkroniserade transaktioner (t.ex. importerade från bank, koblat till household-transaktioner eller låsta av administratör).
- Export av transaktioner till CSV och JSON.


## Antaganden
- Systemet har existerande transaktionsmodell med åtminstone fälten: id, accountId, amount, currency, date, description, counterparty, categoryId, createdAt, updatedAt.
- Det finns någon form av audit-logg eller möjligheten att skapa en sådan (migrering kan behövas).
- Behörighetsmodell finns (användare kan ha olika rättigheter att redigera transaktioner och skapa/ändra kategorier).
- Kopplingar som household/household-transaction, budget, reconciliations måste hanteras.


## Kontraktsöversikt (API)
Föreslagna endpoints:
- PATCH /api/transactions/{transactionId}
  - Syfte: Partiell uppdatering av en transaktion.
  - Payload (exempel):
    {
      "amount": 123.45,
      "date": "2025-10-24",
      "description": "Lunch",
      "categoryId": "guid-or-int",
      "counterparty": "Café X",
      "metadata": { ... }
    }
  - Validering:
    - amount: tal, icke-noll
    - date: giltigt datum
    - categoryId: måste finnas i kategori-tabellen och vara aktiv (om kategori saknas -> 400 eller förslag att skapa ny beroende på behörighet)
  - Response:
    - 200 + uppdaterad transaktion
    - 400 vid valideringsfel
    - 403 om användaren saknar permission
    - 409 om transaktionen är låst/konflikt pga extern synkning

- GET /api/transactions/export?format=csv|json&from=YYYY-MM-DD&to=YYYY-MM-DD&accountId=&categoryId=
  - Streamad export, paginerad backend, men klient får fil.
  - Innehåller standardfält + categoryName + categoryPath + audit-version?


## Datakontrakt / Schema
- Transaction: id, accountId, amount, currency, date, description, counterparty, categoryId, status, externalId, createdAt, updatedAt
- Category: id, name, parentId, isActive, path
- AuditLog/TransactionHistory: id, transactionId, changedByUserId, changeTime, changes (JSON diff), reason (optional)


## Funktionella krav
1. Grundläggande edit
   - R1.1: Användare ska kunna redigera fälten: amount, date, description, counterparty, account (migrering/ändring av konto kan kräva additional checks), metadata, tags.
   - R1.2: Alla ändringar ska sparas atomiskt.
   - R1.3: Uppdatering ska valideras i API: format, typ och affärsregler (t.ex. belopp inte noll om transaktionstyp kräver).

2. Kategorieändring
   - R2.1: Användare ska kunna byta kategori på en transaktion.
   - R2.2: Kategori-väljaren ska erbjuda sök, autokomplettering och visa kategori-hierarki (path) samt göra det enkelt att se om en kategori är budget- eller neutral.
   - R2.3: Om kategori inte finns och användaren har rättighet, erbjud "skapa ny kategori" inline (eller länka till kategori-sida).
   - R2.4: När kategori ändras, loggas det i audit-loggen med tidigare och nya kategori.

3. Behörighet och begränsningar
   - R3.1: Användare utan edit-rättigheter får inte se eller använda edit-funktioner.
   - R3.2: Vissa transaktioner kan vara låsta: importerade från bank och markerade som "immutable", eller transaktioner med extern sync. För dessa ska API returnera 409 eller 403 med tydligt felmeddelande.
   - R3.3: Om transaktionen är del av en household-linked pair, ska edit kräva särskild hantering (t.ex. båda eller via central process). Ange policy: antingen blockera edit eller synka ändring till den länkade posten.

4. Audit & historik
   - R4.1: Alla ändringar sparas i en historik-tabell som inkluderar fältet som ändrades, tidigare värde och nytt värde, vem och när.
   - R4.2: Användargränssnittet ska visa en enkel "Historik"-vy där man kan se tidigare värden och vem ändrade.

5. Export
   - R5.1: Systemet ska stödja export till CSV och JSON av filtrerade transaktioner.
   - R5.2: Export ska inkludera fält: id, accountId, date, amount, currency, description, counterparty, categoryId, categoryName, categoryPath, createdAt, updatedAt.
   - R5.3: Stora exporter ska köras som stream eller background-jobb med notifikation när filen är klar.


## Icke-funktionella krav
- NFR1: Ändringar ska vara ACID-konsistenta (eller tydligt dokumenterade begränsningar där asynkron synk behövs).
- NFR2: API-svarstid för enkel redigering < 500ms under normal last.
- NFR3: Audit-loggning ska vara robust och inte kunna kringgås.
- NFR4: Exporter ska hantera upp till X miljoner rader via streaming/background (definiera X efter systemkapacitet).


## Begränsningar och policyer
- Låsta/externa transaktioner: markerade som "lockedBySource" eller liknande får ej ändras utan admin eller rollback från källsystem.
- Historik får ej modifieras retroaktivt via UI; endast admins med särskild process kan göra korrigeringar (och dessa måste spåras).


## UX / Designkrav
- Huvudprinciper: enkel, tydlig och reversibel.
- Redigeringsmönster: rekommenderat användarmönster är en modal eller slide-over editor för enskild transaktion (för att behålla kontext i listan). Inline-edit kan erbjudas för snabb ändring av tex. beskrivning/belopp.
- Bekräftelse: Om ändring påverkar kopplade objekt (budget, household, återbetalningar) visa en varning innan commit.
- Undo: Visa en kort "Ångra"-toast efter lyckad ändring (t.ex. 5-10 sek) som kan återställa senaste commit.
- Kategoriexplorering: kategori-väljaren ska visa sök + dropdown med path och färg/ikon. Visa även senaste valda kategorier och "smart suggestion" baserat på beskrivning via client-side suggest.
- Visuella ändringar i transaktionslistan: visa kategori-namn och eventuell flagga ("låst", "sync") tydligt i list- och detail-vyer.

### Enkel ASCII-wireframe (modal)

Listvy:
[ Konto | Datum | Belopp | Beskrivning | Kategori ]  <-- klick på rad öppnar modal

Modal: [Transaktion #12345]
---------------------------------------
| Datum: [ 2025-10-24 ]                |
| Belopp: [ 123.45 ]                  |
| Beskrivning: [ Lunch på Café ]      |
| Motpart: [ Café X ]                 |
| Konto: [ Privatkonto ]              |
| Kategori: [Mat > Lunch] [v]         |
| Tags: [Fika]                        |
|                                     |
| [Avbryt]    [Spara ändringar]       |
---------------------------------------

Kategori-dropdown:
- Sök: "lun" -> visar
  * Mat > Lunch
  * Mat > Restaurang
  * Personal > Lunchersättning


## Testfall (exempel: UI + API + DB)
Organisera tester i nivåer: enhet (service/validering), integration (API), end-to-end (UI) samt export-tester.

1) API: Happy path
- Givet en giltig transactionId
- När PATCH med giltiga fält skickas
- Då ska status 200 returneras och fälten uppdateras i DB och en audit-rad skapas.

2) API: Kategoriexistens
- PATCH med categoryId som inte finns -> 400 + tydligt felmeddelande.
- Om användaren saknar rätt att skapa kategori -> 400/403.

3) API: Låst transaktion
- PATCH på transaktion markerad "lockedBySource" -> 409 med orsak.

4) Concurrency
- Två samtidiga PATCH där båda ändrar amount -> antingen optimistic lock (409/ETag) eller sista skrivning vinner beroende på policy. Testa optimistic locking scenario.

5) Audit
- Efter uppdatering, kontrollera att audit-loggen har en rad med: transactionId, changedBy, changeTime, diff innehållande gamalt och nytt value.

6) UI: Modal edit
- Öppna modal, ändra category via sök och spara -> observera att listan uppdateras och toast visas.

7) UI: Undo
- Efter spar, klicka undo i toast -> transaktionen återgår till tidigare värde och en audit-rad med återställning skapas.

8) Export CSV/JSON
- Generera export för ett intervall, kontrollera att filen innehåller korrekta kolumner och antal rader, att categoryName och categoryPath är med.

9) Edge: Household-linked
- För transaktion som är del av household-pair: redigera kategori -> validera policy (antingen blockerat eller synkroniseras). Testa båda varianter beroende på vald policy.

10) Security
- Användare utan edit-permission försöker PATCH -> 403.
- Export kräver rätt permission -> 403 utan rättighet.


## Acceptanskriterier
- AC1: En användare med edit-rätt kan ändra fälten och se förändringen i UI och i databasen.
- AC2: Kategorieändring är möjlig och loggas korrekt i historiken.
- AC3: Låsta/externa transaktioner skyddar mot ändring och ger klart felmeddelande.
- AC4: Export till CSV och JSON inkluderar alla angivna fält och fungerar för normala volymer.
- AC5: Alla ändringar har audit-trail där man kan se tidigare värden, ändringsbrukare och tid.


## Tekniska förslag / Implementation notes
- Backend: implementera PATCH endpoint som tar partial update DTO och kör validering + transaction + skriv audit-logg.
- DB: Lägg till `TransactionAudit`-tabell om inte finns; indexera på transactionId.
- Concurrency: använd optimistic concurrency (rowversion/updatedAt + etag) eller pessimistic lock vid behov.
- UI: återanvänd existerande transaction-detail-komponent; implementera modal med client-side validation.
- Kategorieditor: återanvänd category service; cache top categories; ge snabb sök via server-endpoint.
- Export: implementera streaming response för CSV (text/csv) och JSON (application/json) med filtillverkning i background för stora dataset.


## Risker och mitigering
- Risk: användare råkar ändra fel kategori -> Mitigation: visa breadcrumb, senaste val och require confirmation om känslig kategori.
- Risk: stora exporter slår mot DB -> Mitigation: streama och/eller köra export-jobb asynkront med queue.
- Risk: audit-logg slukas i load -> Mitigation: separera audit storage (append-only table) och ha retention policy.


## Review och sign-off
- Föreslå granskare: Produktägare, Backend lead, Frontend lead, QA.
- Agenda för granskningsmöte: genomgång av krav, UX-wireframes, API-kontrakt, DB-ändringar, testplan.
- Mål: få sign-off för att starta implementation och skriva tickets.


## Nästa steg (tekniska tickets)
1. Backend: Implementera PATCH endpoint + audit-logg (inkl. migration för audit-tabell)
2. Frontend: Modal editor + kategori-sök + Undo-toast
3. Backend: Export endpoint + background-job för stora exporter
4. Tests: Unit/integration + e2e (Playwright) + export-validations
5. Dokumentation: UI copy och hjälptext för redigering


---

Fyll gärna i följande beslutspunkter som påverkar implementation:
- Policy för household-linked transaktioner: blockera/redigera/synka?
- Ska användare kunna skapa nya kategorier inline eller hänvisas till kategori-fliken?
- Acceptabel max-volym för synkron export (för att välja streaming vs background by default).

