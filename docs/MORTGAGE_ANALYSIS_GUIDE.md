# Bolåneanalys - Användarguide

## Översikt

Privatekonomi innehåller ett avancerat analysverktyg för bolån som hjälper dig att:
- Följa svenska amorteringskrav
- Analysera ränterisker
- Planera för räntebindningar
- Förstå din totala månadskostnad

## Svenska Amorteringskrav

Enligt Finansinspektionens regler finns det amorteringskrav för bolån baserat på belåningsgrad (LTV - Loan to Value):

### Regler

| Belåningsgrad (LTV) | Amorteringskrav | Beskrivning |
|---------------------|-----------------|-------------|
| Under 50% | Inget krav | Du behöver inte amortera på ditt bolån |
| 50-70% | 1% per år | Du måste amortera minst 1% av det ursprungliga lånebeloppet per år |
| Över 70% | 2% per år | Du måste amortera minst 2% av det ursprungliga lånebeloppet per år |

### Exempel

**Exempel 1: Belåningsgrad 75%**
- Fastighetsvärde: 4 000 000 kr
- Lånebelopp: 3 000 000 kr
- Belåningsgrad: 75%
- Amorteringskrav: 2% per år = 60 000 kr/år = 5 000 kr/månad

**Exempel 2: Belåningsgrad 60%**
- Fastighetsvärde: 4 000 000 kr
- Lånebelopp: 2 400 000 kr
- Belåningsgrad: 60%
- Amorteringskrav: 1% per år = 24 000 kr/år = 2 000 kr/månad

**Exempel 3: Belåningsgrad 45%**
- Fastighetsvärde: 4 000 000 kr
- Lånebelopp: 1 800 000 kr
- Belåningsgrad: 45%
- Amorteringskrav: Inget krav (men rekommenderas ändå)

## Räntebindning

### Vad är räntebindning?

Räntebindning innebär att din ränta är fast under en viss period, vanligtvis 1-10 år. Efter bindningsperioden måste du förnya eller byta till en ny ränta.

### Vanliga bindningsperioder

- **3 månader**: Rörlig ränta eller mycket kort bindning
- **12 månader**: Kortsiktig bindning
- **24 månader**: Medellång bindning
- **36 månader**: 3 års bindning (populärt val)
- **60 månader**: 5 års bindning
- **120 månader**: 10 års bindning (långsiktig)

### Fördelar med räntebindning

✅ **Trygghet**: Du vet exakt vad din månadskostnad blir
✅ **Budgetsäkerhet**: Lättare att planera ekonomin
✅ **Skydd mot räntehöjningar**: Om räntan stiger är du skyddad under bindningstiden

### Nackdelar med räntebindning

❌ **Ingen fördel vid räntesänkning**: Om räntan sjunker får du inte lägre ränta
❌ **Ofta högre ränta**: Bunden ränta är vanligtvis något högre än rörlig
❌ **Svårt att byta**: Kräver ofta nya förhandlingar med banken

## Ränteriskanalys

### Vad är ränterisk?

Ränterisk är risken att din månadskostnad ökar när räntan går upp. Detta är särskilt viktigt om du har:
- Rörlig ränta
- Kort bindningsperiod som snart löper ut
- Hög belåningsgrad

### Riskbedömning i systemet

Systemet bedömer din ränterisk i tre nivåer:

#### 🟢 Låg risk
- Lång räntebindning (mer än 3 år kvar)
- Låg belåningsgrad (under 70%)
- God ekonomisk buffert

#### 🟡 Måttlig risk
- Medellång räntebindning (1-3 år kvar)
- Medelhög belåningsgrad (50-70%)
- Viss ekonomisk buffert

#### 🔴 Hög risk
- Rörlig ränta eller kort bindningsperiod (under 1 år kvar)
- Hög belåningsgrad (över 70%)
- Liten ekonomisk buffert

### Räntescenarier

Systemet visar hur din månadskostnad påverkas av olika räntehöjningar:

| Scenario | Beskrivning |
|----------|-------------|
| +0% | Nuvarande läge (ingen förändring) |
| +1% | Försiktig höjning - måttlig påverkan |
| +2% | Betydande höjning - stor påverkan |
| +3% | Kraftig höjning - mycket stor påverkan |

**Exempel**: Om du har ett bolån på 3 000 000 kr med 4% ränta:

| Scenario | Ränta | Månadskostnad (endast ränta) | Ökning |
|----------|-------|------------------------------|--------|
| Nuvarande | 4.0% | 10 000 kr | - |
| +1% | 5.0% | 12 500 kr | +2 500 kr |
| +2% | 6.0% | 15 000 kr | +5 000 kr |
| +3% | 7.0% | 17 500 kr | +7 500 kr |

## Använda Bolåneanalys-funktionen

### Steg 1: Lägg till bolån

1. Gå till **Lån & Krediter**
2. Klicka på **Nytt Lån/Kredit**
3. Välj typ: **Bolån**
4. Fyll i grundläggande information:
   - Namn (t.ex. "Bolån villa")
   - Belopp
   - Ränta (i procent)
   - Amortering per månad

### Steg 2: Lägg till bolånespecifik information

5. Fyll i **Bolånespecifik information**:
   - **Fastighetsadress**: Din fastighets adress (valfritt)
   - **Fastighetsvärde**: Aktuellt värde på fastigheten (viktigt för LTV-beräkning!)
   - **Bank/Långivare**: Vilken bank som tillhandahåller lånet (valfritt)
   - **Bunden ränta**: Markera om du har fast ränta
   - **Bindningsperiod**: Hur många månader bindningen gäller
   - **Datum när bindning löper ut**: När du behöver förnya

### Steg 3: Se analysen

6. Klicka på fliken **Bolåneanalys**
7. Här ser du:
   - 📊 **Låneuppgifter**: Belopp, värde, belåningsgrad, ränta
   - 🔒 **Räntebindning**: Status och hur länge kvar
   - 💰 **Amorteringskrav**: Om du uppfyller kraven
   - ⚠️ **Ränterisk**: Bedömning och scenarier

### Steg 4: Agera på varningar

Om systemet visar varningar:

#### ⚠️ Amorteringskrav inte uppfyllt
- **Åtgärd**: Öka din månatliga amortering
- **Hur**: Redigera lånet och höj "Amortering per månad" eller "Extra amortering"

#### ⚠️ Räntebindning löper snart ut
- **Åtgärd**: Kontakta din bank för att förnya bindningen
- **Tips**: Jämför räntor mellan olika banker
- **Timing**: Börja förhandla 2-3 månader innan bindningen löper ut

#### 🔴 Hög ränterisk
- **Åtgärd 1**: Överväg att binda räntan för längre period
- **Åtgärd 2**: Amortera mer för att sänka belåningsgraden
- **Åtgärd 3**: Bygg upp en ekonomisk buffert

## Tips och råd

### 1. Regelbunden uppföljning

- Uppdatera fastighetsvärdet minst en gång per år
- Kontrollera att amorteringskravet uppfylls
- Se över räntebindningen 3-6 månader innan den löper ut

### 2. Strategisk räntebindning

**Dela upp lånet:**
```
Exempel med 3 miljoner kr:
- 1 miljon kr med 1 års bindning (flexibilitet)
- 1 miljon kr med 3 års bindning (balans)
- 1 miljon kr med 5 års bindning (trygghet)
```

Detta ger dig:
- Flexibilitet att dra nytta av låga räntor
- Trygghet om räntan stiger
- Möjlighet att förhandla om delar regelbundet

### 3. Amortera smartare

**Prioritera så här:**
1. **Först**: Uppfyll minimikravet enligt lag
2. **Sedan**: Extra amortering på lån med högst ränta
3. **Sist**: Bygg buffert för oförutsedda utgifter

### 4. Utnyttja ränteavdrag

- Du får göra ränteavdrag på 30% av räntekostnaden
- Beräknas automatiskt i självdeklarationen
- Sänker din faktiska räntekostnad

**Exempel:**
- Räntekostnad per år: 120 000 kr
- Ränteavdrag (30%): 36 000 kr
- Faktisk kostnad: 84 000 kr

### 5. Bygg buffert för räntehöjningar

**Rekommenderad buffert:**
- Spara motsvarande 2-3% räntehöjning under 6 månader
- För 3 miljoner kr lån: Cirka 30 000 - 45 000 kr i buffert

### 6. Jämför banker regelbundet

- Räntor varierar mellan banker
- Du kan spara tusentals kronor per år
- Använd konkurrens till din fördel när du förhandlar

## Vanliga frågor (FAQ)

### Måste jag följa amorteringskravet?

**Ja**, amorteringskravet är lagstadgat sedan 2016 (förstärkt 2018) och gäller för:
- Nya bolån
- Tilläggslån
- Omförhandlade lån

### Vad händer om jag inte uppfyller amorteringskravet?

Banken är skyldig att se till att du följer reglerna. Om du inte gör det kan:
- Lånet sägas upp
- Du tvingas sälja fastigheten
- Banken får böter av Finansinspektionen

### Kan jag få dispens från amorteringskravet?

I vissa fall kan du få amorteringsfrihet:
- Tillfälliga ekonomiska problem
- Föräldraledighet
- Sjukdom

Kontakta din bank för att diskutera möjligheterna.

### Bör jag binda räntan nu?

Det beror på:
- **Aktuellt ränteläge**: Är räntan låg eller hög historiskt?
- **Din ekonomi**: Har du råd med eventuella höjningar?
- **Din riskprofil**: Vill du ha trygghet eller flexibilitet?

**Tumregel**: Om du inte har råd med 2-3% räntehöjning bör du binda räntan.

### Hur ofta bör jag uppdatera fastighetsvärdet?

- **Minimum**: En gång per år
- **Rekommenderat**: Vid varje taxeringsvärde (från Skatteverket)
- **Extra**: Vid stora marknadsförändringar

### Vad är en bra belåningsgrad?

| Belåningsgrad | Bedömning |
|---------------|-----------|
| Under 50% | Utmärkt - stor buffert |
| 50-70% | Bra - uppfyller lägsta krav |
| 70-85% | Måttlig - följ extra noga |
| Över 85% | Hög - mycket begränsat utrymme |

## Kontaktinformation och resurser

### Myndigheter och organisationer

- **Finansinspektionen**: Regler och tillsyn för bolånemarknaden
- **Konsumentverket**: Rådgivning och information om bolån
- **Skatteverket**: Information om ränteavdrag

### Användbara länkar

- [Finansinspektionens bolåneguide](https://www.fi.se)
- [Konsumentverkets bolånekalkylatorer](https://www.konsumentverket.se)
- [Bolanejämförelser](https://www.compricer.se)

## Sammanfattning

✅ **Använd systemet för att:**
- Kontrollera att du uppfyller amorteringskrav
- Övervaka räntebindningar
- Bedöma din ränterisk
- Planera för framtida kostnader

✅ **Kom ihåg:**
- Uppdatera information regelbundet
- Planera för räntehöjningar
- Förhandla med banken i tid
- Bygg ekonomisk buffert

✅ **Kontakta banken om:**
- Räntebindningen snart löper ut
- Du har svårt att klara amorteringskravet
- Du vill diskutera din bolanesituation

---

*Denna guide uppdaterades senast: 2025-11-10*
