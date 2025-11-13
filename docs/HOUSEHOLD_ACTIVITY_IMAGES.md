# Bilduppladdning för Hushållsaktiviteter

## Översikt

Från och med denna version kan du ladda upp och koppla 1-10 bilder till aktiviteter på hushållets tidslinje. Detta gör det enklare att dokumentera och dela viktiga händelser, projekt och minnen i hushållet.

## Funktioner

### Ladda upp bilder

När du lägger till en ny aktivitet på hushållets tidslinje kan du nu:

1. Klicka på "Lägg till aktivitet"-knappen
2. Fyll i aktivitetsinformation (titel, beskrivning, typ, datum)
3. Rulla ner till "Bilder (valfritt, max 10)"-sektionen
4. Klicka på uppladdningsområdet eller dra och släpp bilder

### Begränsningar och säkerhet

- **Antal bilder**: 1-10 bilder per aktivitet
- **Filstorlek**: Max 5 MB per bild
- **Format som stöds**: JPEG, JPG, PNG, WebP, GIF
- Bilder valideras automatiskt på klientsidan

### Visa bilder

- Bilderna visas som thumbnails (miniatyrbilder) i tidslinjevyn
- Max 4 thumbnails visas direkt, med en indikator om det finns fler bilder
- Klicka på en thumbnail för att visa bilden i fullstorlek
- Ett bildräknare-chip visar totalt antal bilder för varje aktivitet

### Ta bort bilder

För närvarande tas bilder bort automatiskt när du raderar en aktivitet. I framtida versioner kommer du kunna redigera aktiviteter och ta bort enskilda bilder.

## Användningsexempel

### Exempel 1: Dokumentera renoveringsprojekt
Ladda upp bilder på ett renoveringsprojekt (före, under och efter) för att hålla koll på framstegen och dela med andra hushållsmedlemmar.

### Exempel 2: Dela händelser
Ta bilder av speciella händelser som födelsedagsfirande, familjemiddagar eller andra viktiga tillfällen i hushållet.

### Exempel 3: Underhållsdokumentation
Dokumentera underhållsarbete med bilder, t.ex. trädgårdsarbete, reparationer eller städningsprojekt.

## Tekniska detaljer

### Datalagring
- Bilder lagras i `/wwwroot/uploads/household-activities/` på servern
- Metadata om bilderna (filnamn, storlek, MIME-typ) sparas i databasen
- Bilder är kopplade till specifika aktiviteter med en one-to-many-relation

### Säkerhet
- Filvalidering på klientsidan (storlek och format)
- Unika filnamn genereras för varje uppladdad bild (GUID-baserade)
- Användarisolering genom hushålls-ID och autentisering
- Cascade delete: bilder raderas automatiskt när aktivitet raderas

## Framtida förbättringar

Planerade förbättringar inkluderar:
- Möjlighet att redigera aktiviteter och ta bort enskilda bilder
- Bildkarusell för att visa alla bilder
- Bildtexter och beskrivningar
- Sortera/arrangera bildordning
- Bildkomprimering för snabbare laddning

## Support

Om du stöter på problem med bilduppladdning:
1. Kontrollera att bilden är under 5 MB
2. Kontrollera att filformatet är JPEG, PNG, WebP eller GIF
3. Se till att du inte försöker ladda upp fler än 10 bilder per aktivitet
