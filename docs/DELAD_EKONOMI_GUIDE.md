# Delad Ekonomi - Användarguide

## Översikt

Funktionen **Delad Ekonomi** gör det enkelt för hushåll att hantera gemensamma budgetar och skulder mellan medlemmar. Detta är perfekt för par, samboende, kollektiv eller familjer som vill få koll på sin delade ekonomi.

## Funktioner

### 1. Gemensamma Budgetar

#### Vad är en gemensam budget?

En gemensam budget är en budget som delas mellan flera medlemmar i ett hushåll, där varje medlem bidrar med en specifik andel (procentsats) av budgeten.

#### Skapa en gemensam budget

1. Navigera till **Hushåll** > Välj ditt hushåll > Klicka på **Delad Ekonomi**
2. Under fliken **Gemensamma Budgetar**, klicka på **Skapa gemensam budget**
3. Fyll i följande information:
   - **Namn**: T.ex. "Hushållsbudget December 2024"
   - **Beskrivning**: Valfri beskrivning av budgeten
   - **Startdatum**: När budgeten börjar gälla
   - **Slutdatum**: När budgeten slutar gälla
   - **Period**: Månadsvis eller Årsvis
   - **Fördelning**: Ange procentandel för varje medlem (måste summera till 100%)

**Exempel:**
- Partner A bidrar med 60%
- Partner B bidrar med 40%
- Total: 100%

4. Klicka på **Skapa** för att spara budgeten

#### Fördelar med gemensamma budgetar

- ✅ Tydlig fördelning av kostnader baserat på inkomst eller överenskommelse
- ✅ Enkel överblick över vem som bidrar med vad
- ✅ Perfekt för par med olika inkomstnivåer
- ✅ Automatisk koppling till hushållet

### 2. Skuldbalansering (Settle Up)

#### Vad är skuldbalansering?

När medlemmar i ett hushåll betalar för gemensamma utgifter uppstår skulder som behöver balanseras. T.ex. om Partner A betalar för mat åt båda, ska Partner B betala tillbaka sin andel.

#### Registrera en skuld

1. Under fliken **Skuldbalansering**, klicka på **Registrera skuld**
2. Fyll i följande:
   - **Gäldenär**: Vem som ska betala (den som är skyldig pengar)
   - **Borgenär**: Vem som ska få betalt (den som har lagt ut pengar)
   - **Belopp**: Summan i kronor
   - **Beskrivning**: T.ex. "Mat från ICA", "Hyra december", "Gemensam middag"

3. Klicka på **Registrera**

#### Medlemsbalanser

Systemet visar automatiskt en översikt över varje medlems saldo:

- **Grön bakgrund och positivt belopp**: Ska få tillbaka pengar
- **Röd bakgrund och negativt belopp**: Ska betala pengar
- **Grå bakgrund och 0 kr**: Balanserad

**Exempel:**
- Anna: +150 kr (ska få tillbaka)
- Björn: -150 kr (ska betala)

#### Markera skuld som betald

När en skuld har betalats:

1. Hitta skulden i listan **Pågående skulder**
2. Klicka på gröna checkmarken ✓
3. Lägg till en valfri notering, t.ex. "Betald via Swish"
4. Klicka på **Markera som betald**

Skulden flyttas nu till **Avslutade skulder** och balanserna uppdateras automatiskt.

#### Avbryt en skuld

Om en skuld registrerades av misstag:

1. Hitta skulden i listan **Pågående skulder**
2. Klicka på röda krysset ✗
3. Skulden markeras som avbruten

### 3. Optimal Balansering

#### Vad är optimal balansering?

När det finns många skulder mellan flera medlemmar kan det bli komplicerat att räkna ut vem som ska betala vem. Funktionen **Beräkna optimal balansering** använder en algoritm för att minimera antalet transaktioner som behövs.

**Exempel:**

Före optimering:
- Anna ska betala Björn 100 kr
- Anna ska betala Cecilia 50 kr
- Björn ska betala Cecilia 30 kr

Efter optimering:
- Anna ska betala Björn 70 kr
- Anna ska betala Cecilia 80 kr

(Färre transaktioner, samma slutresultat!)

#### Använda optimal balansering

1. Klicka på **Beräkna optimal balansering**
2. Systemet analyserar alla pågående skulder
3. Nya optimerade skulder skapas automatiskt
4. De gamla skulderna förblir i systemet för historik

**OBS:** Denna funktion är bäst lämpad när alla har betalat sina utgifter och ni vill "jämna ut" kontot mellan alla medlemmar.

## Användningsfall & Exempel

### Användningsfall 1: Par med olika inkomster

**Situation:** Lisa tjänar 40 000 kr/månad, Johan tjänar 30 000 kr/månad. De vill dela utgifterna proportionellt.

**Lösning:**
1. Skapa hushåll "Lisa & Johan"
2. Lägg till medlemmar: Lisa och Johan
3. Skapa gemensam budget:
   - Namn: "Månadbudget"
   - Lisa: 57% (40000/(40000+30000) × 100)
   - Johan: 43%
4. Registrera skulder när någon betalar för gemensamma utgifter

### Användningsfall 2: Kollektiv med lika fördelning

**Situation:** 4 studenter i ett kollektiv vill dela alla utgifter lika.

**Lösning:**
1. Skapa hushåll "Kollektivet"
2. Lägg till 4 medlemmar
3. Skapa gemensam budget:
   - Varje medlem: 25%
4. När någon handlar mat eller betalar räkningar, registrera skuld
5. Använd "Beräkna optimal balansering" vid månadsskiftet

### Användningsfall 3: Föräldrar med barn

**Situation:** Föräldrar som vill hålla reda på vem som betalar för barnens aktiviteter.

**Lösning:**
1. Skapa hushåll "Familjen Andersson"
2. Lägg till båda föräldrarna som medlemmar
3. Registrera skulder för barnens utgifter (fotbollsavgift, musikskola, osv.)
4. Balansera regelbundet

## Tips & Bästa Praxis

### 💡 Tänk på detta

1. **Kom överens i förväg**: Diskutera och kom överens om fördelningen innan ni börjar använda funktionen
2. **Registrera löpande**: Registrera skulder direkt när någon betalar för något gemensamt
3. **Balansera regelbundet**: Gör upp varje månad eller vecka för att undvika stora summor
4. **Använd beskrivningar**: Skriv alltid vad skulden gäller så det är lätt att komma ihåg
5. **Kontrollera saldot**: Kolla regelbundet på medlemsbalanserna för att se vem som är före/efter

### ⚠️ Vanliga misstag att undvika

- **Glöm inte att markera som betald**: När en skuld har betalats, glöm inte att markera den som betald i systemet
- **Dubbelregistrering**: Registrera inte samma utgift flera gånger
- **Fel person**: Dubbelkolla att gäldenär och borgenär är rätt innan du sparar

## Integrationer

### Koppling till befintliga funktioner

- **Budgetar**: Gemensamma budgetar syns även under normal budgetvy
- **Utgifter**: Skuldbalansering kompletterar funktionen för delade utgifter
- **Rapporter**: Använd rapportfunktionen för att se historiska balanser

## Säkerhet & Integritet

- Alla data är isolerade per hushåll
- Endast medlemmar i hushållet kan se dess skulder och budgetar
- All data krypteras och lagras säkert

## Support & Frågor

Om du har frågor eller stöter på problem, se:
- [README.md](../README.md) för allmän information
- [Hushållsbudget Snabbguide](HUSHALLSBUDGET_SNABBGUIDE.md) för budgettips
- GitHub Issues för buggrapporter och funktionsförfrågningar

---

**Version:** 1.0  
**Senast uppdaterad:** 2024-11-08
