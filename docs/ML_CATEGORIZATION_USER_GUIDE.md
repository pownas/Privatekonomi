# Smart Kategorisering med Maskininlärning - Användarguide

> Lär systemet att automatiskt kategorisera dina transaktioner

## Vad är Smart Kategorisering?

Smart kategorisering använder maskininlärning (ML) för att **lära sig** hur du brukar kategorisera dina transaktioner. Ju fler transaktioner du kategoriserar manuellt, desto bättre blir systemet på att automatiskt välja rätt kategori för nya transaktioner.

### Fördelar

✅ **Personlig** - Lär sig dina specifika mönster, inte generiska regler  
✅ **Automatisk** - Kategoriserar nya transaktioner automatiskt  
✅ **Förbättras över tid** - Blir bättre ju mer du använder det  
✅ **Säker** - Din data delas aldrig med andra användare  

---

## Hur fungerar det?

### Steg 1: Samla data 📊

För att systemet ska kunna lära sig behöver du först kategorisera minst **50 transaktioner** manuellt.

**Tips för bästa resultat:**
- Kategorisera så många transaktioner som möjligt
- Var konsekvent i din kategorisering
- Ha minst 5-10 exempel per kategori

### Steg 2: Automatisk träning 🤖

När du har tillräckligt många kategoriserade transaktioner:
1. Systemet tränar automatiskt en ML-modell för dig
2. Modellen lär sig från dina kategoriseringsmönster
3. Detta sker i bakgrunden - inget du behöver göra!

### Steg 3: Automatisk kategorisering ✨

När nya transaktioner kommer in:
1. Systemet försöker först gissa rätt kategori med ML
2. Om systemet är säkert (≥70% konfidens) används ML-kategorin
3. Om systemet är osäkert används regelbaserad kategorisering som backup

### Steg 4: Korrigering och förbättring 🔄

Om systemet gissar fel:
1. Ändra till rätt kategori som vanligt
2. Systemet lär sig från din korrigering
3. Nästa gång blir gissningen bättre!

---

## Exempel

### Scenario 1: Matinköp

**Tidigare transaktioner du kategoriserat:**
```
ICA Maxi Stockholm - 285 kr        → Mat & Dryck
Coop Supermarket - 198 kr          → Mat & Dryck  
ICA Kvantum Uppsala - 342 kr       → Mat & Dryck
Hemköp Järfälla - 156 kr          → Mat & Dryck
```

**Ny transaktion:**
```
ICA Supermarket Göteborg - 267 kr
```

**ML föreslår:** Mat & Dryck (Konfidens: 92%)  
**Resultat:** ✅ Kategoriseras automatiskt som "Mat & Dryck"

### Scenario 2: Osäker transaktion

**Tidigare transaktioner:**
```
Gekås Ullared - 450 kr            → Kläder
Gekås Ullared - 1,250 kr          → Boende (möbler)
Gekås Ullared - 89 kr             → Mat & Dryck
```

**Ny transaktion:**
```
Gekås Ullared - 850 kr
```

**ML föreslår:** Kläder (Konfidens: 45%)  
**Resultat:** ⚠️ Låg konfidens - använder regelbaserad kategorisering istället

---

## Vanliga frågor

### Hur många transaktioner behöver jag kategorisera?

**Minimum:** 50 transaktioner totalt  
**Rekommenderat:** 200+ för bäst resultat  
**Per kategori:** Minst 5 exempel

### Vad händer om jag har färre än 50 transaktioner?

Systemet använder regelbaserad kategorisering tills du har tillräckligt med data för ML.

### Kan jag lita på ML-kategorierna?

Ja! Systemet visar bara ML-kategorier när det är säkert (≥70% konfidens). Vid osäkerhet används regelbaserad kategorisering som backup.

### Vad händer om ML gissar fel?

Bara ändra till rätt kategori! Systemet lär sig från din korrigering och blir bättre nästa gång.

### Delar systemet min data med andra?

**Nej!** Din ML-modell tränas endast på dina egna transaktioner. Ingen data delas mellan användare.

### Hur ofta omtränas modellen?

För närvarande sker omträning manuellt. I framtida versioner planeras automatisk omträning när du har kategoriserat många nya transaktioner.

### Kan jag stänga av ML-kategorisering?

För närvarande är ML-kategorisering alltid aktiv (med automatisk fallback). I framtida versioner kan du eventuellt välja att bara använda regelbaserad kategorisering.

---

## Tips för bästa resultat

### 1. Var konsekvent 🎯

Kategorisera liknande transaktioner på samma sätt:
- ✅ Alla matinköp → "Mat & Dryck"
- ❌ Vissa matinköp → "Mat & Dryck", andra → "Shopping"

### 2. Använd tydliga kategorier 📂

Undvik att ha för många liknande kategorier:
- ✅ "Mat & Dryck" (en kategori)
- ❌ "Mat", "Dryck", "Livsmedel", "Matvaror" (fyra kategorier för samma sak)

### 3. Kategorisera regelbundet 📅

Kategorisera nya transaktioner så snart de kommer in:
- Färsk data ger bättre ML-resultat
- Systemet lär sig snabbare
- Du glömmer inte vad transaktionen var

### 4. Ha tålamod ⏳

ML behöver tid och data för att bli bra:
- Första 50 transaktionerna: Regelbaserad kategorisering
- 50-200 transaktioner: ML börjar fungera, kan göra misstag
- 200+ transaktioner: ML blir riktigt bra!

### 5. Korrigera misstag 🔧

Om ML gissar fel:
1. Ändra till rätt kategori
2. Systemet lär sig
3. Nästa liknande transaktion kategoriseras korrekt

---

## Säkerhet och integritet

### Din data är säker 🔒

- **Lokalt** - Modeller sparas lokalt på servern
- **Isolerat** - Din modell innehåller bara din data
- **Privat** - Ingen annan användare kan se din modell
- **Skyddad** - Modeller raderas om du raderar ditt konto

### GDPR-rättigheter

Du har rätt att:
- **Se din data** - Se vilka ML-modeller som finns för dig
- **Radera din data** - Ta bort ML-modeller och feedback
- **Exportera din data** - Få ut dina ML-data i läsbart format

Kontakta support för att utöva dessa rättigheter.

---

## Felsökning

### Problem: "Kategori föreslås inte automatiskt"

**Möjliga orsaker:**
- Du har färre än 50 kategoriserade transaktioner
- Transaktionen liknar ingen tidigare transaktion
- ML-modellen inte är tränad än

**Lösning:** Fortsätt kategorisera fler transaktioner manuellt

### Problem: "ML gissar ofta fel"

**Möjliga orsaker:**
- För få exempel per kategori
- Inkonsekvent kategorisering
- För liknande kategorier

**Lösning:**
- Kategorisera fler transaktioner
- Var mer konsekvent
- Överväg att slå ihop liknande kategorier

### Problem: "Samma transaktion får olika kategorier"

**Orsak:** Detta kan hända om transaktionen verkligen är oklar

**Exempel:**
```
"Gekås Ullared - 500 kr"
Kan vara: Kläder, Möbler, Mat, etc.
```

**Lösning:** Var konsekvent i din egen kategorisering så lär sig ML rätt mönster

---

## Framtida funktioner

Vi planerar att lägga till:

### Kommande i nästa version
- 🔔 **Notifikationer** när modellen behöver omtränas
- 📊 **Statistik** över ML-prestanda (hur ofta ML gissar rätt)
- ⚙️ **Inställningar** för att välja ML-konfidensnivå
- 🔄 **Automatisk omträning** när många nya transaktioner kategoriserats

### På längre sikt
- 🎨 **Visualisering** av ML-mönster
- 💡 **Förslag** på kategorier att slå ihop
- 🌍 **Multi-språk** stöd för transaktionsbeskrivningar
- 📱 **Push-notiser** för osäkra transaktioner

---

## Behöver du hjälp?

### Dokumentation
- [Teknisk dokumentation](ML_CATEGORIZATION.md) (för utvecklare)
- [Snabbreferens](ML_CATEGORIZATION_QUICK_REFERENCE.md) (för utvecklare)

### Support
- Skapa ett issue på GitHub
- Kontakta support via e-post
- Läs FAQ-sektionen ovan

---

## Sammanfattning

Smart kategorisering med ML gör livet enklare:

1. **Kategorisera 50+ transaktioner** manuellt först
2. **ML tränas automatiskt** i bakgrunden
3. **Nya transaktioner kategoriseras** automatiskt
4. **Korrigera vid behov** - systemet lär sig!

Ju mer du använder systemet, desto bättre blir det! 🚀

---

**Version:** 1.0  
**Senast uppdaterad:** 2025-10-30  
**Skapad av:** Privatekonomi Development Team
