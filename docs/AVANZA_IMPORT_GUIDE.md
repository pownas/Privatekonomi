# Guide: Importera investeringar från Avanza

**Version:** 1.0  
**Datum:** 2025-10-17

## Översikt

Denna guide beskriver hur du importerar dina investeringar från Avanza Bank till Privatekonomi-applikationen.

![Importera från Avanza](https://github.com/user-attachments/assets/e352caaf-230e-4032-baf0-b850667760f0)
*Importvyn där du väljer bank och laddar upp CSV-fil*

## Exportera från Avanza

### Steg 1: Logga in på Avanza

1. Gå till [www.avanza.se](https://www.avanza.se)
2. Logga in med dina uppgifter

### Steg 2: Exportera innehav

Avanza tillhandahåller två olika CSV-exportformat. Du kan använda vilket som helst:

#### Alternativ 1: Mitt innehav fördelat per konto

1. Navigera till **Översikt** → **Innehav**
2. Klicka på **Exportera** eller **Ladda ner**
3. Välj **Mitt innehav fördelat per konto**
4. Välj format: **CSV**
5. Spara filen på din dator

**Detta format innehåller:**
- Kontonummer
- Investeringsnamn
- Kortnamn
- Volym (antal)
- Marknadsvärde
- GAV (Genomsnittligt Anskaffningsvärde)
- Valuta
- Land
- ISIN
- Marknad
- Typ

#### Alternativ 2: Mitt sammanställda innehav

1. Navigera till **Översikt** → **Innehav**
2. Klicka på **Exportera** eller **Ladda ner**
3. Välj **Mitt sammanställda innehav**
4. Välj format: **CSV**
5. Spara filen på din dator

**Detta format innehåller:**
- Samma information som Alternativ 1, men utan kontonummer
- Visar sammanställt innehav över alla dina konton

## Importera till Privatekonomi

### Steg 1: Navigera till import-sidan

1. Öppna Privatekonomi i din webbläsare
2. Gå till **Aktier & Fonder**
3. Klicka på knappen **Importera**

### Steg 2: Välj bank

1. På import-sidan väljer du **Avanza** som bank
2. Klicka på **Nästa**

### Steg 3: Ladda upp CSV-fil

1. Dra och släpp din exporterade CSV-fil i uppladdningsområdet
   - **eller**
2. Klicka på **Välj fil från dator** och navigera till filen

**Observera:**
- Max filstorlek: 10 MB
- Tillåtet format: .csv
- Båda Avanza-formaten stöds automatiskt

### Steg 4: Importera

1. Klicka på **Importera**
2. Systemet läser och bearbetar filen
3. Ett resultat visas med:
   - Antal nya investeringar
   - Antal uppdaterade investeringar
   - Eventuella fel

### Steg 5: Verifiera import

1. Klicka på **Gå till investeringar**
2. Kontrollera att dina investeringar visas korrekt
3. Verifiera att bank- och kontoinformation stämmer

## Funktioner i investeringslistan

### Filtrering

Efter import kan du:
- **Filtrera per bank:** Använd dropdown-menyn "Bank" för att visa endast investeringar från Avanza
- **Filtrera per konto:** Använd dropdown-menyn "Konto" för att visa innehav från specifika konton
- **Sök:** Använd sökfältet för att hitta specifika investeringar

### Visning

Investeringslistan visar:
- **Namn:** Fullständigt namn på investering
- **Kortnamn:** Ticker-symbol (t.ex. TELIA, AZA)
- **Typ:** Aktie, Fond eller Certifikat (färgkodad)
- **Bank:** Visas som färgkodad badge
- **Konto:** Kontonummer (om tillgängligt)
- **Antal:** Antal innehav
- **Köpkurs:** Genomsnittligt anskaffningsvärde (GAV)
- **Aktuell kurs:** Beräknas från marknadsvärde/volym
- **Värde:** Totalt marknadsvärde
- **Vinst/Förlust:** Skillnad mellan värde och kostnad
- **Avkastning %:** Procentuell avkastning

### Exportera

Du kan exportera dina investeringar:
1. Klicka på **Exportera CSV**
2. Filen laddas ner automatiskt
3. Öppna i Excel eller annan kalkylapplikation

Exporterade filen innehåller alla kolumner inklusive beräknade värden.

## Uppdatera befintliga investeringar

När du importerar samma investeringar igen:
- **Dubbletter upptäcks automatiskt** baserat på ISIN och kontonummer
- **Befintliga investeringar uppdateras** med nya värden
- **Nya investeringar läggs till** som inte fanns tidigare

Detta gör det enkelt att hålla dina investeringar uppdaterade!

## Exempel på CSV-format

### Format 1: Mitt innehav fördelat per konto

```csv
Kontonummer|Namn|Kortnamn|Volym|Marknadsvärde|GAV (SEK)|GAV|Valuta|Land|ISIN|Marknad|Typ
1111-2223332|Telia Company|TELIA|5|180,90|54,78|54,78|SEK|SE|SE0000667925|XSTO|STOCK
2222-3333444|Oneflow|ONEF|100|2730,00|44,71|44,71|SEK|SE|SE0017564461|FNSE|STOCK
```

### Format 2: Mitt sammanställda innehav

```csv
Namn|Kortnamn|Volym|Marknadsvärde|GAV (SEK)|GAV|Valuta|Land|ISIN|Marknad|Typ
AFRY|AFRY|7|1164,10|159,59|159,59|SEK|SE|SE0005999836|XSTO|STOCK
Avanza Bank Holding|AZA|6|2248,20|216,25|216,25|SEK|SE|SE0012454072|XSTO|STOCK
```

## Felsökning

### Problem: "Kunde inte identifiera CSV-format"

**Möjliga orsaker:**
- Filen är inte i rätt format
- Filen är skadad
- Filen innehåller fel separator

**Lösning:**
1. Kontrollera att filen kommer från Avanza
2. Öppna filen i en textredigerare och verifiera att den innehåller rätt kolumner
3. Försök exportera filen igen från Avanza

### Problem: "Filen är för stor"

**Lösning:**
- Max filstorlek är 10 MB
- Om du har många innehav, dela upp exporten i flera filer
- Kontakta support om du regelbundet behöver importera större filer

### Problem: Investeringar saknas efter import

**Kontrollera:**
1. Att CSV-filen innehåller alla rader
2. Att inga rader har felaktigt format
3. Importresultatet för eventuella felmeddelanden

### Problem: Fel värden visas

**Möjliga orsaker:**
- CSV-filen använder annan decimalavgränsare (komma vs punkt)
- Valutakonvertering behövs

**Lösning:**
1. Systemet hanterar både komma och punkt som decimalavgränsare
2. För valutakonvertering, kontrollera att "Valuta"-kolumnen stämmer
3. Uppdatera investeringarna manuellt om nödvändigt

## Tips och råd

### Regelbunden uppdatering

För att hålla dina investeringar aktuella:
1. Exportera från Avanza regelbundet (t.ex. en gång i veckan)
2. Importera till Privatekonomi
3. Befintliga investeringar uppdateras automatiskt med nya värden

### Kombinera källor

Du kan importera investeringar från olika banker:
1. Importera från Avanza
2. Lägg till andra banker manuellt eller via import
3. Använd bank-filtret för att se innehav per bank

### Backup

Innan större importer:
1. Exportera dina befintliga investeringar till CSV
2. Spara som backup
3. Gör sedan importen

## Support

Om du stöter på problem:
1. Kontrollera denna guide
2. Verifiera att CSV-filen har rätt format
3. Öppna ett issue på GitHub med:
   - Beskrivning av problemet
   - Ett exempel på CSV-filen (utan känsliga uppgifter)
   - Felmeddelanden du fått

## Versionshistorik

- **1.0** (2025-10-17): Initial version med stöd för båda Avanza CSV-formaten
