# Investeringar och Pension - Användarguide

## Översikt

Privatekonomi stödjer nu omfattande hantering av investeringar och pension med följande funktioner:

## 📊 Pensionshantering

### Funktioner
- **Pensionstyper:**
  - Tjänstepension (AMF, Alecta, etc.)
  - Privat pensionssparande
  - Allmän pension (AP7)
  - Pensionsförsäkringar

- **Spårning:**
  - Nuvarande värde
  - Totala inbetalningar
  - Månatliga inbetalningar
  - Förväntad månatlig pension vid pensionering
  - Pensionsålder
  - Avkastning i kronor och procent

### Rekommendation: MinPension.se
För en komplett översikt över din pension rekommenderas att hämta uppgifter från [minpension.se](https://www.minpension.se), som samlar information från:
- Allmän pension (Pensionsmyndigheten)
- Tjänstepension (från arbetsgivare)
- Privat pensionssparande

## 💰 Investeringshantering

### Typer av investeringar som stöds
1. **Aktier** - Individuella aktier
2. **Fonder** - Aktiefonder, blandfonder, räntefonder
3. **ETF** - Börshandlade fonder (Exchange Traded Funds)
4. **Certifikat** - Strukturerade produkter
5. **Krypto** - Kryptovalutor (Bitcoin, Ethereum, etc.)
6. **P2P-lån** - Peer-to-peer lending (Lendify, Savelend, etc.)

### Kontotyper (Svenska konton)
- **ISK** (Investeringssparkonto) - Schablonbeskattning
- **KF** (Kapitalförsäkring) - Schablonbeskattning
- **AF** (Aktie- och fondkonto) - Vinstskatt vid försäljning
- **Depå** - Vanligt värdepappersdepå

### Funktioner
- Import från Avanza
- Automatisk kursuppdatering via Yahoo Finance API
- ISIN-nummer för identifiering
- Valutastöd
- Schablonbeskattning för ISK/KF
- Vinst/förlust-beräkning

## 📈 Utdelningar (Dividends)

### Spåra utdelningar från dina investeringar
- Registrera utdelningsbetalningar per investering
- Spåra ex-dividend datum
- Belopp per aktie och totalt belopp
- Källskatt (tax withheld)
- DRIP-stöd (Dividend Reinvestment Plan)

### Statistik
- Total utdelning per år
- Utdelning per investering
- Direktavkastning

## 🔄 Investeringstransaktioner

### Håll koll på köp och försäljningar
- Registrera köp- och säljtransaktioner
- Antal aktier/andelar
- Pris per aktie vid transaktion
- Courtage och avgifter
- Valutaväxling

### Användningsområden
- Kostnadsbas för skatteberäkning
- Historik över portföljförändringar
- Underlag för K4-deklaration

## 🎯 Portföljallokering

### Målallokering
- Definiera målfördelning per tillgångsslag
- Aktier, obligationer, fastigheter, råvaror, kontanter
- Min/max-gränser för rebalansering
- Aktiv/inaktiv strategi

### Framtida funktioner
- Visualisering av nuvarande vs målallokering
- Rebalanseringsrekommendationer
- Riskanalys baserat på allokering
- Faktorexponering

## 📊 Aggregering och Statistik

### Portföljöversikt
Investeringar kan aggregeras på flera sätt:

1. **Per kontotyp:**
   - ISK
   - KF
   - Depå
   - Pension

2. **Per investeringstyp:**
   - Aktier
   - Fonder
   - ETF
   - Krypto
   - P2P-lån

3. **Per bank/leverantör:**
   - Avanza
   - Nordnet
   - Länsförsäkringar
   - etc.

### Nyckeltal
- Totalt värde
- Total kostnad
- Orealiserad vinst/förlust
- Avkastning i procent
- Utdelningar per år

## 🔮 Framtida funktioner (Planerade)

### Avkastningsmått
- **XIRR** (Extended Internal Rate of Return) - Tidsvägd avkastning med hänsyn till in- och utbetalningar
- **TWR** (Time-Weighted Return) - Tidsvägt avkastning utan påverkan av in/utbetalningar
- **Jämförelse mot index** - OMXS30, S&P 500, etc.

### Skattelots och K4
- FIFO/LIFO för kostnadsbasberäkning
- Automatisk K4-generering
- Vyer per skatteår
- Realiserade vs orealiserade vinster

### Risk och allokering
- Volatilitetsberäkning
- Sharpe ratio
- Beta mot index
- Faktorexponering (värde, tillväxt, momentum, etc.)

## 💡 Tips och Best Practices

### För pension
1. Hämta uppgifter från minpension.se regelbundet (minst 1 gång per år)
2. Uppdatera månatliga inbetalningar när de ändras
3. Kontrollera att din förväntade pension stämmer med din plan

### För investeringar
1. Uppdatera kurser regelbundet (använd automatisk uppdatering)
2. Registrera utdelningar när de betalas ut
3. Håll ISIN-nummer uppdaterade för korrekt identifiering
4. Använd rätt kontotyp (ISK/KF/Depå) för korrekt skatteberäkning

### För aggregering
1. Använd konsekventa kategoriseringar (Aktie, Fond, ETF, etc.)
2. Sätt upp målallokering baserat på din risktolerans
3. Rebalansera regelbundet (t.ex. årligen)

## 📚 Relaterad dokumentation
- [STOCK_PRICE_API_GUIDE.md](../wiki/STOCK_PRICE_API_GUIDE.md) - Automatisk kursuppdatering
- [AVANZA_IMPORT_GUIDE.md](../wiki/AVANZA_IMPORT_GUIDE.md) - Import från Avanza
- [SWEDISH_INTEGRATIONS_IMPLEMENTATION.md](../wiki/SWEDISH_INTEGRATIONS_IMPLEMENTATION.md) - ISK/KF schablonbeskattning

## 🔒 Säkerhet och integritet
- All data lagras lokalt eller i din egen databas
- Ingen data delas med tredje part
- Stöd för kryptering av känslig information
- Användarbaserad åtkomstkontroll
