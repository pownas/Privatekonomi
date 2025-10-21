# Användarguide: Löneutveckling

## Översikt
Löneutvecklingsfunktionen i Privatekonomi hjälper dig att spåra och analysera din lön över tid. Med denna funktion kan du:
- Registrera din lön för olika perioder
- Visualisera din löneutveckling i graf
- Se statistik över genomsnittslön och lönetillväxt
- Hålla koll på jobbbyten och löneförhöjningar

## Komma igång

### Navigera till Löneutveckling
1. Logga in i Privatekonomi
2. Klicka på **Löneutveckling** i vänstermenyn (ikon: trending-up)

### Din första lönepost

När du öppnar Löneutveckling för första gången ser du en tom vy med budskapet:
> "Ingen lönehistorik ännu"
> "Börja spåra din löneutveckling genom att lägga till din första lönepost."

**Lägg till din första lön:**
1. Klicka på knappen **"Ny Lönepost"** (överst till höger)
2. Fyll i formuläret:
   - **Månadslön (kr)**: Din bruttolön per månad (före skatt)
   - **Period**: Välj månad och år när denna lön gällde
   - **Befattning**: Din jobbtitel (t.ex. "Utvecklare", "Säljare")
   - **Arbetsgivare**: Namnet på ditt företag
   - **Anställningstyp**: Välj från dropdown (Heltid, Deltid, Timavlönad, Konsult, Praktik)
   - **Tjänstegrad (%)**: Din arbetsgrad i procent (100% = heltid, 50% = halvtid)
   - **Anteckningar**: Fritext för att notera varför lönen ändrades (t.ex. "Lönerevision", "Nytt jobb", "Befordran")
   - **Detta är min nuvarande lön**: Markera om detta är din nuvarande lön
3. Klicka **"Lägg till"**

## Huvudfunktioner

### Statistikkort
Efter att du lagt till löneposter visas fyra statistikkort överst:

1. **Nuvarande Lön**: Din senaste lön markerad som "nuvarande"
2. **Genomsnitt (12 mån)**: Genomsnittslön för de senaste 12 månaderna
3. **Tillväxt (12 mån)**: Procentuell ökning/minskning senaste 12 månaderna
4. **Antal Poster**: Totalt antal löneposter du registrerat

### Graf - Löneutveckling över tid
Grafen visar din löneutveckling som ett linjediagram:
- **X-axel**: Tidsperiod (år-månad)
- **Y-axel**: Lönebelopp i kronor
- **Linje**: Visar hur din lön förändrats över tid

Grafen använder mjuka kurvor (natural spline interpolation) för att visa trenden tydligt.

### Lönehistorik-tabell
Tabellen visar alla dina löneposter med följande kolumner:

| Kolumn | Beskrivning |
|--------|-------------|
| Period | När lönen gällde (ÅÅÅÅ-MM) |
| Månadslön | Belopp i kronor |
| Befattning | Din jobbtitel |
| Arbetsgivare | Företagsnamn |
| Anställningstyp | Heltid, Deltid, etc. |
| Tjänstegrad | Arbetsgrad i procent |
| Nuvarande | "Ja" om detta är din nuvarande lön |
| Åtgärder | Edit- och Delete-knappar |

**Sökfunktion:**
- Använd sökfältet ovanför tabellen för att filtrera löneposter
- Sökningen fungerar på: Period, Befattning, Arbetsgivare, Anteckningar

**Paginering:**
- Standard: 10 poster per sida
- Kan ändras till 25, 50 eller 100 poster per sida

## Hantera löneposter

### Redigera en lönepost
1. Klicka på Edit-ikonen (penna) på raden du vill redigera
2. Formuläret öppnas med befintliga värden
3. Ändra de fält du vill uppdatera
4. Klicka **"Uppdatera"**

**Tips:** Du kan ändra period, lön eller markera en annan post som "nuvarande".

### Ta bort en lönepost
1. Klicka på Delete-ikonen (papperskorg) på raden du vill ta bort
2. Bekräfta i dialogrutan som visas
3. Posten tas bort och statistiken uppdateras automatiskt

**Varning:** Borttagning kan inte ångras!

### Markera nuvarande lön
Du kan endast ha EN post markerad som "nuvarande lön" åt gången:

1. När du lägger till eller redigerar en post, markera "Detta är min nuvarande lön"
2. Om du redan har en post markerad som nuvarande, avmarkeras den automatiskt
3. Detta säkerställer att statistikkortet "Nuvarande Lön" alltid visar rätt värde

## Användningsscenarier

### Scenario 1: Nytt jobb
Du har bytt jobb och fått högre lön:

1. Klicka "Ny Lönepost"
2. Fyll i:
   - **Månadslön**: Din nya lön
   - **Period**: Månad/år när du började
   - **Befattning**: Din nya jobbtitel
   - **Arbetsgivare**: Det nya företaget
   - **Anställningstyp**: T.ex. "Heltid"
   - **Tjänstegrad**: 100
   - **Anteckningar**: "Nytt jobb på Tech AB"
   - **Nuvarande**: ✓ Markera
3. Klicka "Lägg till"

Din gamla lön avmarkeras automatiskt som "nuvarande", och grafen uppdateras för att visa löneökningen.

### Scenario 2: Lönerevision
Du har fått lönerevision:

1. Klicka "Ny Lönepost"
2. Fyll i samma arbetsgivare och befattning, men med ny lön
3. Anteckningar: "Årlig lönerevision"
4. Markera som nuvarande
5. Klicka "Lägg till"

### Scenario 3: Deltid till heltid
Du har gått från deltid till heltid:

1. Klicka "Ny Lönepost"
2. Öka både lön och tjänstegrad
3. Ändra anställningstyp från "Deltid" till "Heltid"
4. Anteckningar: "Ökad till heltid"
5. Klicka "Lägg till"

### Scenario 4: Importera historisk data
Du vill lägga till äldre löner:

1. Börja med äldsta lönen
2. Lägg till en lönepost för varje löneändring
3. Arbeta dig framåt i tiden
4. Markera endast den senaste som "nuvarande"

**Tips:** Du behöver inte lägga till varje månad, bara när lönen ändrades!

## Tips och tricks

### Bästa praxis
- **Lägg till vid varje löneändring**: Registrera din lön varje gång den ändras (nytt jobb, lönerevision, befordran)
- **Använd anteckningar**: Dokumentera varför lönen ändrades - detta är värdefullt att titta tillbaka på
- **Markera nuvarande korrekt**: Se till att rätt post är markerad som nuvarande för korrekta statistikberäkningar
- **Period = första månaden**: Använd första månaden då den nya lönen började gälla

### Tolka statistiken
- **Genomsnitt (12 mån)**: Använd detta för att jämföra med statistik eller andra jobberbjudanden
- **Tillväxt (12 mån)**: Positivt tal = löneökning, negativt = löneminskning
- **Grafen**: Visar trender över tid - är din lön på väg uppåt?

### Planering
- **Lönefördel**: Se hur mycket din lön ökat sedan karriärstart
- **Löneutveckling**: Identifiera perioder med stillastående lön
- **Framtida mål**: Använd grafen för att sätta realistiska lönemål

## Vanliga frågor

**F: Måste jag lägga till en post för varje månad?**  
S: Nej! Lägg bara till när lönen faktiskt ändras (nytt jobb, lönerevision, etc.).

**F: Vad händer om jag markerar två poster som "nuvarande"?**  
S: Systemet avmarkerar automatiskt den gamla posten - endast en post kan vara nuvarande.

**F: Kan jag lägga till framtida löner?**  
S: Ja, men vi rekommenderar att vänta tills lönen faktiskt börjar gälla.

**F: Visas min lön för andra användare?**  
S: Nej! All lönedata är privat och isolerad per användare.

**F: Kan jag exportera min lönehistorik?**  
S: Exportfunktionen är planerad för framtida version.

**F: Hur beräknas tillväxten?**  
S: Tillväxt beräknas som: ((Senaste lönen - Äldsta lönen inom perioden) / Äldsta lönen) × 100

**F: Vad händer om jag tar bort en post som är markerad som nuvarande?**  
S: Posten tas bort, och du behöver markera en annan post som nuvarande manuellt.

## Felsökning

### Grafen visas inte
- **Orsak**: Du har inga löneposter eller bara en post
- **Lösning**: Lägg till minst 2 löneposter för olika perioder

### Statistiken visar 0,00 kr
- **Orsak**: Ingen post är markerad som "nuvarande"
- **Lösning**: Redigera din senaste lönepost och markera "Detta är min nuvarande lön"

### Kan inte se mina löneposter
- **Orsak**: Du är inte inloggad eller tittar på fel användare
- **Lösning**: Logga in med rätt användarkonto

### Felmeddelande vid sparande
- **Orsak**: Obligatoriska fält saknas eller ogiltiga värden
- **Lösning**: Kontrollera att Månadslön och Period är ifyllda

## Sekretesspolicy

### Datasäkerhet
- All lönedata lagras säkert i databasen
- Data är krypterad och isolerad per användare
- Ingen annan användare kan se din löneinformation
- API-endpoints kräver autentisering

### GDPR-compliance
- Du har full kontroll över din data
- Du kan redigera eller ta bort poster när som helst
- Data exporteras (planerad funktion) för portabilitet

## Kontakt och support

Har du frågor eller feedback om Löneutvecklingsfunktionen?
- Öppna ett issue på GitHub: [github.com/pownas/Privatekonomi/issues](https://github.com/pownas/Privatekonomi/issues)
- Diskutera i diskussionsforumet

---

**Version:** 1.0  
**Senast uppdaterad:** 2025-10-21
