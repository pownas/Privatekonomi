# Periodisering av Kostnader (Cost Periodization)

## Översikt

Funktionen för periodisering av kostnader gör det möjligt att hantera kostnader som inte sker varje månad i budgetkalkylen. Detta gör budgetplaneringen mer överskådlig och korrekt genom att fördela årskostnader, halvårskostnader, kvartalskostnader eller varannanmånadskostnader till månadsvisa poster.

## Användning

### Skapa en budget med periodiserade kostnader

1. Navigera till **Budgethantering**
2. Klicka på **Ny Budget**
3. Fyll i grundläggande budgetinformation (namn, period, datum)
4. För varje kategori kan du nu:
   - Välja **period** från rullgardinsmenyn:
     - Månad (standard)
     - Varannan månad
     - Kvartal (3 månader)
     - Halvår (6 månader)
     - Helår (12 månader)
   - Ange **belopp** för den valda perioden

5. Systemet beräknar automatiskt **månadskostnaden**
6. Spara budgeten

### Exempel

#### Exempel 1: Gymkort (Årskostnad)
- **Period:** Helår (12 månader)
- **Belopp:** 1 800 kr/år
- **Månadskostnad:** 150 kr/månad (1 800 ÷ 12)

#### Exempel 2: Danskurs (Halvårskostnad)
- **Period:** Halvår (6 månader)
- **Belopp:** 850 kr/halvår
- **Månadskostnad:** 141,67 kr/månad (850 ÷ 6)

#### Exempel 3: Avfall och Vatten (Varannan månad)
- **Period:** Varannan månad (2 månader)
- **Belopp:** 1 300 kr/varannan månad
- **Månadskostnad:** 650 kr/månad (1 300 ÷ 2)

#### Exempel 4: Fritidsaktivitet (Årskostnad)
- **Period:** Helår (12 månader)
- **Belopp:** 2 400 kr/år
- **Månadskostnad:** 200 kr/månad (2 400 ÷ 12)

## Visa Periodiserade Kostnader

När du visar en budget kommer periodiserade poster att tydligt markeras:
- **Period-kolumn** visar vilken period posten avser (med en färgad chip för periodiserade poster)
- **Periodbelopp-kolumn** visar det totala beloppet för perioden
- **Månadskostnad-kolumn** visar den beräknade månadskostnaden (visas i fetstil för tydlighet)

Budgetsammanfattningen visar den totala månadskostnaden för alla poster, vilket gör det enkelt att se hur mycket du behöver budgetera varje månad.

## Teknisk Information

### Datamodell

Budgetkategorier har följande egenskaper:
- `PlannedAmount`: Det totala beloppet för perioden
- `RecurrencePeriodMonths`: Antal månader i perioden (1, 2, 3, 6 eller 12)
- `MonthlyAmount`: Beräknad månadskostnad (PlannedAmount ÷ RecurrencePeriodMonths)

### Beräkning

Månadskostnaden beräknas enligt följande formel:

```
Månadskostnad = Periodbelopp ÷ Antal månader i perioden
```

Till exempel:
- 1 800 kr/år ÷ 12 månader = 150 kr/månad
- 850 kr/halvår ÷ 6 månader = 141,67 kr/månad
- 1 300 kr/varannan månad ÷ 2 månader = 650 kr/månad

## Fördelar

1. **Enklare budgetplanering**: Alla kostnader visas som månadskostnader, vilket gör det lättare att planera månadsbudgeten
2. **Mer korrekt budgetkalkyl**: Årskostnader fördelas jämnt över året istället för att belasta en enskild månad
3. **Bättre översikt**: Tydlig markering av periodiserade poster gör det enkelt att se vilka kostnader som är årliga, halvårliga, etc.
4. **Flexibilitet**: Stöd för flera olika perioder (månad, varannan månad, kvartal, halvår, år)

## Begränsningar

- Periodiseringen är manuell - systemet identifierar inte automatiskt periodiska kostnader från transaktioner
- Användaren kan ändra period och belopp när som helst genom att redigera budgeten
- Periodiserade kostnader kan inte ha delperioder (t.ex. 5 månader eller 18 månader)
