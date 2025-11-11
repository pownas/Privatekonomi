# Admin Dashboard - Mätetal och Success Metrics

## Översikt
Admin-dashboarden ger systemadministratörer en central plats för att övervaka plattformens hälsa och prestanda genom omfattande mätetal och success metrics.

## Åtkomst
- **URL**: `/admin/metrics`
- **Behörighet**: Endast systemadministratörer
- **Test-användare**: test@example.com / Test123!

## Funktioner

### 1. Användarstatistik (User Metrics)
Spårar hur användare engagerar sig med plattformen:

- **MAU (Monthly Active Users)**: Antal unika användare som varit aktiva den senaste månaden
  - Mål: +20% tillväxt per kvartal
  - Färgkodad indikator visar framsteg mot mål

- **DAU (Daily Active Users)**: Antal unika användare som varit aktiva idag
  - Visar även DAU/MAU-kvot (mål: >40%)
  - Hjälper identifiera dagligt engagement

- **30-dagars Retention Rate**: Andel användare som kommer tillbaka efter 30 dagar
  - Mål: >60%
  - Viktigt för långsiktig användartillväxt

- **Monthly Churn Rate**: Andel användare som slutar använda plattformen
  - Mål: <5% per månad
  - Tidig varningssignal för användarproblem

### 2. Engagement-statistik
Mäter hur djupt användare engagerar sig:

- **Transaktioner per användare**: Genomsnittligt antal transaktioner per aktiv användare
  - Mål: >30 per månad
  - Indikerar hur mycket användare faktiskt använder plattformen

- **Genomsnittlig sessionstid**: Hur länge användare är aktiva per session
  - Mål: >5 minuter
  - Visar hur engagerande plattformen är

- **Feature Adoption Rate**: Procentandel användare som använder nya funktioner
  - Mål: >50%
  - Hjälper förstå feature-användning

- **NPS (Net Promoter Score)**: Användarnöjdhetsmått
  - Mål: >50
  - Baseras på användarfeedback

### 3. Prestandastatistik
Övervakar teknisk prestanda:

- **Uptime**: Procentandel tid som plattformen är tillgänglig
  - Mål: 99.9%
  - Kritiskt för användartillfredsställelse

- **Genomsnittlig laddningstid**: Hur snabbt sidor laddar
  - Mål: <2s (Desktop), <3s (Mobile)
  - Påverkar användarupplevelsen direkt

- **Lighthouse Score**: Googles kvalitetsmått för webbprestanda
  - Mål: >90
  - Omfattar prestanda, tillgänglighet, SEO, m.m.

- **Crash Rate**: Procentandel sessioner som kraschar
  - Mål: <0.1%
  - Indikator för stabilitet

### 4. Säkerhetsstatistik
Spårar säkerhetsrelaterade mätetal:

- **2FA Adoption**: Procentandel användare med tvåfaktorsautentisering
  - Mål: >70%
  - Viktigt för kontosäkerhet

- **Failed Login Attempts**: Andel misslyckade inloggningsförsök
  - Mål: <1%
  - Kan indikera attacker eller användarproblem

- **Security Incidents**: Antal säkerhetsincidenter
  - Mål: 0
  - Kritisk säkerhetsindikator

- **GDPR Compliance**: Efterlevnad av dataskyddsförordningen
  - Mål: 100%
  - Måste alltid vara 100%

## Tidsperiodfiltrering

Dashboarden stöder olika tidsperioder för historisk analys:

- **Daglig**: Se metrics dag för dag
- **Veckovis**: Aggregerad veckodata
- **Månadsvis**: Månadsöversikt (standard)
- **Kvartalsvis**: Kvartalsöversikt för långsiktig analys
- **Årlig**: Årsöversikt

Välj antal perioder att visa: 6, 12, eller 24 perioder.

## Visualiseringar

### Trenddiagram
Dashboarden inkluderar två huvuddiagram:

1. **MAU Trend**: Visar utvecklingen av månadsaktiva användare över tid
2. **Transaktioner per användare**: Visar engagemanget över tid

Båda diagrammen uppdateras baserat på vald tidsperiod och antal perioder.

### Färgkodade indikatorer
Varje mätetal har en färgkodad indikator:

- 🟢 **Grönt**: Mål uppnått eller överträffat
- 🟡 **Gult**: Närmar sig mål (80-99%)
- 🔴 **Rött**: Under mål

## Användningsfall

### Daglig övervakning
- Kontrollera DAU och systemstatus
- Identifiera ovanliga trender tidigt
- Övervaka säkerhetsincidenter

### Veckoöversikt
- Granska veckovis utveckling
- Analysera användarbeteende
- Planera förbättringar

### Månatlig analys
- Utvärdera månadsresultat mot mål
- Identifiera säsongsmönster
- Rapportera till stakeholders

### Kvartalsplanering
- Utvärdera långsiktig tillväxt
- Jämföra kvartal mot kvartal
- Sätta nya mål

## Teknisk information

### Databeräkning
- Metrics beräknas i realtid från databasen
- Historiska snapshots sparas för snabb hämtning
- Alla beräkningar optimerade för prestanda

### Datakällor
- **Användardata**: ApplicationUser-tabell och LastLoginAt
- **Transaktionsdata**: Transactions-tabell
- **Säkerhetsdata**: AuditLogs-tabell
- **Autentisering**: Identity-systemet

### Cachning
Vissa metrics kan cachas för bättre prestanda, speciellt historiska data som inte ändras.

## Säkerhet

- Sidan kräver systemadministratörsbehörighet
- Endast användare med `IsSystemAdmin = true` har åtkomst
- Redirect till login om obehörig
- Alla queries scopade till korrekt behörighet

## Felsökning

### Metrics visar 0
- Kontrollera att det finns data i databasen
- Verifiera att testdata har seedats korrekt
- Kontrollera att LastLoginAt uppdateras vid inloggning

### Kan inte nå sidan
- Verifiera att användaren är systemadministratör
- Kontrollera att test-användaren har `IsSystemAdmin = true`
- Logga ut och in igen för att uppdatera behörigheter

### Diagram laddas inte
- Kontrollera browser-konsolen för JavaScript-fel
- Verifiera att MudBlazor-komponenter laddas korrekt
- Testa att uppdatera sidan

## Framtida förbättringar

Planerade förbättringar inkluderar:

1. **Export-funktionalitet**: Exportera metrics till Excel/PDF
2. **Email-rapporter**: Automatiska vecko/månadsrapporter
3. **Custom alerts**: Konfigurera anpassade larm när metrics når tröskelvärden
4. **Jämförelser**: Jämför perioder mot varandra
5. **Drill-down**: Klicka på metrics för mer detaljerad information
6. **Real-time updates**: Live-uppdatering av metrics med SignalR
7. **Segmentering**: Filtrera metrics per användargrupp eller funktion

## Support

Vid frågor eller problem, kontakta utvecklingsteamet eller skapa en issue i GitHub-repositoryt.

## Se även

- [DEVELOPER_QUICKSTART.md](DEVELOPER_QUICKSTART.md) - Utvecklarguide
- [README.md](../README.md) - Projektöversikt
- Relevant kod:
  - `src/Privatekonomi.Core/Services/MetricsService.cs`
  - `src/Privatekonomi.Web/Components/Pages/Admin.razor`
  - `src/Privatekonomi.Core/Models/AdminMetrics.cs`
