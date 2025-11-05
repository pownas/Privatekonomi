# Progressive Web App (PWA) Guide

Privatekonomi är nu en fullt fungerande Progressive Web App (PWA) som kan installeras på din mobil eller desktop och fungerar även offline!

## 🎯 Vad är en PWA?

En Progressive Web App är en webbapplikation som upplevs som en native app. Den kan:
- **Installeras** på din enhet (mobil, surfplatta, desktop)
- **Fungera offline** - läsa data även utan internetanslutning
- **Synkronisera automatiskt** när du kommer online igen
- **Skicka notifikationer** (om du tillåter det)
- **Starta snabbt** från hemskärmen

## 📱 Installation

### Android

1. Öppna Privatekonomi i Chrome eller Edge
2. En banner kommer att visas som erbjuder installation
3. Tryck på **"Installera"** i bannern
4. Alternativt: Tryck på menyn (⋮) och välj **"Lägg till på startskärmen"** eller **"Installera app"**
5. Appen installeras och en ikon läggs till på din hemskärm

### iOS (iPhone/iPad)

1. Öppna Privatekonomi i Safari
2. Tryck på dela-knappen (□ med pil uppåt)
3. Scrolla ner och välj **"Lägg till på hemskärmen"**
4. Ge appen ett namn (eller behåll "Privatekonomi")
5. Tryck på **"Lägg till"**
6. Appen finns nu på din hemskärm

> **⚠️ iOS-begränsningar:**
> - **Bakgrundssynkronisering fungerar INTE** - du måste ha appen öppen för att synkronisera offline-transaktioner
> - **Push-notifikationer fungerar INTE** i Safari och är mycket begränsade även i installerade PWA:er (iOS 16.4+)
> - Detta är en begränsning från Apple, inte ett fel i appen
> - Läs mer i [iOS-begränsningar guide](PWA_IOS_LIMITATIONS.md)

### Desktop (Windows/Mac/Linux)

1. Öppna Privatekonomi i Chrome, Edge, eller annat stödjt webbläsare
2. Klicka på installations-ikonen i adressfältet (datorskärm med pil)
3. Alternativt: En banner visas som erbjuder installation
4. Klicka på **"Installera"**
5. Appen öppnas i ett eget fönster och läggs till i startmenyn/Launchpad

## 🔌 Offline-funktionalitet

### Vad fungerar offline?

När du är offline kan du:
- ✅ **Visa transaktioner** som har laddats tidigare
- ✅ **Granska budgetar och mål**
- ✅ **Se investeringar och pensioner**
- ✅ **Navigera i appen**
- ✅ **Skapa nya transaktioner** (sparas i kö)

### Vad fungerar inte offline?

- ❌ Hämta nya data från servern
- ❌ Uppdatera aktiekurser
- ❌ Importera från bank via PSD2
- ❌ Dela data med andra användare i realtid

### Offline-kö för transaktioner

När du skapar transaktioner offline:

1. **Transaktionen sparas lokalt** i en kö i din webbläsare (IndexedDB)
2. **Ett meddelande visas** om att transaktionen väntar på synkronisering
3. **Automatisk synkronisering** sker när du kommer online igen
4. **Bekräftelse visas** när transaktionen har synkats till servern

Du ser antalet väntande transaktioner i den gula offline-bannern högst upp på sidan.

## 🔄 Automatisk synkronisering

### Background Sync

Privatekonomi använder Background Sync API för att:
- Automatiskt synkronisera offline-skapade transaktioner när du kommer online
- Försöka igen om synkroniseringen misslyckas
- Synkronisera även när appen är stängd (på vissa plattformar)

### Manual synkronisering

Om automatisk synkronisering misslyckas kan du:
1. Öppna appen när du är online
2. Synkroniseringen startar automatiskt
3. Kontrollera offline-bannern för status

## 🔔 Push-notifikationer

### Aktivera notifikationer

1. Första gången du öppnar appen kommer du att tillfrågas om tillstånd
2. Välj **"Tillåt"** för att få notifikationer
3. Du kan ändra detta senare i webbläsarens inställningar

### Vad kan du få notifikationer om?

- 💰 Budget varningar när du närmar dig gränsen
- 🎯 Sparmål uppnådda
- 📅 Kommande räkningar och betalningar
- 🔄 Synkronisering slutförd
- ℹ️ Viktig information från appen

### Hantera notifikationer

**Android:**
- Inställningar → Appar → Privatekonomi → Notifikationer

**iOS:**
- Inställningar → Notifikationer → Privatekonomi

**Desktop:**
- Webbläsarinställningar → Integritet → Notifikationer

## 🔄 Uppdateringar

### Automatiska uppdateringar

När en ny version av Privatekonomi är tillgänglig:

1. **Ett meddelande visas** automatiskt i appen
2. Klicka på **"Uppdatera nu"** för att installera
3. Appen laddas om med den nya versionen
4. Dina data förblir intakta

### Manual uppdatering

Om du vill kontrollera efter uppdateringar:
1. Ladda om sidan (Ctrl/Cmd + R)
2. Om en uppdatering finns kommer meddelandet att visas

## 📊 Cache-strategi

Privatekonomi använder en smart cache-strategi:

### Network First (Nätverk först)
- Försök alltid hämta från servern först
- Om det misslyckas, använd cachad version
- Bra för dynamiskt innehåll som transaktioner

### Cache First (Cache först)
- Använd cachad version om den finns
- Uppdatera cache i bakgrunden
- Bra för statiskt innehåll som CSS, JS, bilder

## 🗄️ Lagring

### IndexedDB
Offline-data sparas i webbläsarens IndexedDB:
- Transaktioner i vänteläge
- Användarinställningar
- Cache-metadata

### Cache Storage
Statiska resurser sparas i Cache Storage:
- HTML-sidor
- CSS-filer
- JavaScript-filer
- Bilder och ikoner

### Rensa data

För att rensa offline-data:
1. Webbläsarinställningar → Integritet
2. Rensa webbplatsdata för privatekonomi.se
3. Eller avinstallera appen

## 🔒 Säkerhet och integritet

- ✅ Alla data krypteras i transit (HTTPS)
- ✅ Lokala data skyddas av webbläsarens säkerhetsmekanismer
- ✅ Ingen data delas mellan användare offline
- ✅ Service Worker körs i en isolerad kontext
- ✅ Cachelagring följer samma-origin policy

## 🐛 Felsökning

### Installationen fungerar inte

**Kontrollera att:**
- Du använder en stödd webbläsare (Chrome, Edge, Safari, Firefox)
- Du är ansluten till en säker anslutning (HTTPS)
- Du inte använder inkognitoläge
- Webbläsaren tillåter PWA-installation

### Offline-läge fungerar inte

**Försök:**
1. Ladda om sidan när du är online
2. Kontrollera att Service Worker är registrerad (DevTools → Application → Service Workers)
3. Rensa cache och ladda om
4. Kontrollera webbläsarens konsollogg för fel

### Synkronisering fungerar inte

**Kontrollera:**
1. Att du är ansluten till internet
2. Att appen är öppen
3. Kontrollera antalet väntande transaktioner i offline-bannern
4. Försök manuell omladdning

**⚠️ På iOS/Safari:**
- Bakgrundssynkronisering fungerar INTE
- Du måste ha appen öppen för att synkronisera
- När du öppnar appen och är online kommer synkning ske automatiskt
- Detta är en Apple-begränsning, inte ett fel

### Notifikationer kommer inte fram

**Kontrollera:**
1. Att du har gett tillstånd för notifikationer
2. Att systemnotifikationer är aktiverade
3. Att "Stör ej"-läge inte är aktivt
4. Webbläsarens notifikationsinställningar

**⚠️ På iOS:**
- Push-notifikationer fungerar INTE i Safari
- Push fungerar begränsat på iOS 16.4+ om appen är installerad
- Överväg att använda email-notifikationer istället
- Detta är en Apple-begränsning, inte ett fel

### iOS-specifika problem

**Problem:** Appen synkroniserar inte automatiskt när jag kommer online

**Lösning:** 
- iOS stödjer inte Background Sync API
- Du måste ha appen öppen för att synkronisera
- Öppna appen när du är online så synkas data automatiskt

**Problem:** Jag får inga push-notifikationer på iPhone

**Lösning:**
- Safari stödjer inte Web Push på iOS (eller mycket begränsat från iOS 16.4+)
- Detta är en plattformsbegränsning från Apple
- Använd email-notifikationer som alternativ

**Mer information:** Se [iOS PWA-begränsningar guide](PWA_IOS_LIMITATIONS.md)

## 📖 Teknisk information

### Service Worker

- **Fil:** `/service-worker.js`
- **Scope:** `/`
- **Cache-strategi:** Network-first med cache-fallback
- **Version:** Automatisk versionshantering

### Manifest

- **Fil:** `/manifest.json`
- **Displayläge:** Standalone
- **Tema:** #594AE2 (Privatekonomi lila)
- **Bakgrund:** #1a1a1f (Mörk)

### Browser-stöd

- ✅ Chrome 90+
- ✅ Edge 90+
- ✅ Safari 15+ (begränsat stöd för push)
- ✅ Firefox 90+
- ✅ Opera 75+

### Lighthouse PWA Score

Privatekonomi uppfyller alla PWA-krav och får 90+ poäng på Lighthouse PWA-audit.

## 🎓 Lär mer

- [MDN: Progressive Web Apps](https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps)
- [web.dev: PWA](https://web.dev/progressive-web-apps/)
- [Service Workers: an Introduction](https://developers.google.com/web/fundamentals/primers/service-workers)
- [PWA Next Steps](PWA_NEXT_STEPS.md) - Deployment och produktionsförberedelser

## ❓ Support

Om du har frågor eller problem med PWA-funktioner:
1. Kontrollera denna guide
2. Läs Felsökning-sektionen
3. För deployment och produktion, se [PWA_NEXT_STEPS.md](PWA_NEXT_STEPS.md)
4. Kontakta support via GitHub Issues
