# UI Screenshots och Exempel

## Påminnelser med Snooze - Användarvy

### Notifikationslista

Nedan följer en textbeskrivning av UI:t för påminnelser med snooze-funktionalitet:

```
╔════════════════════════════════════════════════════════════════════╗
║ 🔔 Notifikationer                                                  ║
╠════════════════════════════════════════════════════════════════════╣
║                                                                    ║
║ Olästa notifikationer (3)     [Visa alla] [✓ Markera alla lästa] ║
║                                                                    ║
║ ┌──────────────────────────────────────────────────────────────┐  ║
║ │ ⚠️  ⚠️ BRÅDSKANDE: Betala Elräkning          [Kritisk]      │  ║
║ │                                                               │  ║
║ │    Räkningen på 1,500 kr förföll för 8 dagar sedan.          │  ║
║ │    Åtgärd krävs omedelbart! (Snoozad 2 gånger)               │  ║
║ │                                                               │  ║
║ │    2025-10-27 14:30                                     🗑️   │  ║
║ │                                                               │  ║
║ │    [✓ Markera som betald] [💤 Snooze ▼] [📝 Skapa trans...]  │  ║
║ │                           ├─ ⏱️ 1 timme                       │  ║
║ │                           ├─ 📅 1 dag                         │  ║
║ │                           └─ 📆 1 vecka                       │  ║
║ └──────────────────────────────────────────────────────────────┘  ║
║                                                                    ║
║ ┌──────────────────────────────────────────────────────────────┐  ║
║ │ 📋  Påminnelse: Hyra                                         │  ║
║ │                                                               │  ║
║ │    Räkning på 8,500 kr förfaller 2025-11-05 (om 1 dag)       │  ║
║ │                                                               │  ║
║ │    2025-11-03 09:00                                     🗑️   │  ║
║ │                                                               │  ║
║ │    [✓ Markera som betald] [💤 Snooze ▼] [📝 Skapa trans...]  │  ║
║ └──────────────────────────────────────────────────────────────┘  ║
║                                                                    ║
║ ┌──────────────────────────────────────────────────────────────┐  ║
║ │ 📋  Påminnelse: Netflix-prenumeration          (SNOOZAD)     │  ║
║ │                                                               │  ║
║ │    Räkning på 139 kr förfaller 2025-11-07                    │  ║
║ │                                                               │  ║
║ │    2025-11-02 18:45                                     🗑️   │  ║
║ │    💤 Snoozad till 2025-11-05 10:00 (1 snooz)                │  ║
║ │                                                               │  ║
║ │    [✓ Markera som betald] [💤 Snooze ▼] [📝 Skapa trans...]  │  ║
║ └──────────────────────────────────────────────────────────────┘  ║
║                                                                    ║
║ ┌──────────────────────────────────────────────────────────────┐  ║
║ │ 💰  Sparmål uppnått                                     (Läst)│  ║
║ │                                                               │  ║
║ │    Grattis! Du har nått ditt sparmål "Semester 2025"         │  ║
║ │    på 25,000 kr                                               │  ║
║ │                                                               │  ║
║ │    2025-11-01 12:30 - Läst 2025-11-01 12:35           🗑️   │  ║
║ └──────────────────────────────────────────────────────────────┘  ║
║                                                                    ║
║ ┌─────────────────────────────────────────────────────────────┐   ║
║ │ ℹ️ Funktioner                                               │   ║
║ │                                                             │   ║
║ │ • Snooze: Skjut upp påminnelse (1h, 1d, 1v)                 │   ║
║ │ • Markera som betald: Slutför påminnelse och markera       │   ║
║ │   räkning som betald                                        │   ║
║ │ • Skapa transaktion: Registrera betalning direkt            │   ║
║ │ • Eskalering: Automatisk uppföljning vid 1, 3 och 7 dagar   │   ║
║ │ • Snooze-detektion: Varning vid 3+ snooze-tillfällen        │   ║
║ └─────────────────────────────────────────────────────────────┘   ║
╚════════════════════════════════════════════════════════════════════╝
```

## Interaktiva Element

### Snooze-meny
När användaren klickar på "💤 Snooze ▼" visas en dropdown med:
- ⏱️ 1 timme
- 📅 1 dag
- 📆 1 vecka

### Quick Actions för Räkningar
Varje räkningspåminnelse har tre åtgärdsknappar:
1. **Markera som betald** (grön) - Slutför påminnelsen
2. **Snooze** (outlined) - Skjut upp med dropdown
3. **Skapa transaktion** (outlined) - Gå till transaktionsskapande

### Visuella Indikatorer

**Prioritetsnivåer:**
- 🔴 Kritisk (Critical): Röd ikon, "⚠️ BRÅDSKANDE" i titeln
- 🟠 Hög (High): Orange ikon
- 🔵 Normal (Normal): Blå ikon
- ⚪ Låg (Low): Grå ikon

**Status:**
- Oläst: Fet stil på titeln
- Läst: Normal stil, lite transparent
- Snoozad: Halvtransparent, 💤-ikon med tidpunkt
- Snooze-räknare: "(X snooz)" om > 0

### Eskaleringsindikatorer

**Nivå 1 (1 dag sedan påminnelse):**
```
📋 Påminnelse: Betala Elräkning
Påminnelse om räkning på 1,500 kr som förföll 2025-10-30
[High Priority]
```

**Nivå 2 (3 dagar sedan påminnelse):**
```
⚠️ BRÅDSKANDE: Betala Elräkning
Påminnelse om räkning på 1,500 kr som förföll 2025-10-28
[High Priority]
```

**Nivå 3 (7+ dagar sedan påminnelse):**
```
⚠️ BRÅDSKANDE: Betala Elräkning
Räkningen på 1,500 kr förföll för 8 dagar sedan. 
Åtgärd krävs omedelbart!
[Critical Priority]
```

## Användarflöden

### Flöde 1: Snooze en påminnelse
1. Användaren ser notifikation "Påminnelse: Hyra"
2. Klickar på "💤 Snooze ▼"
3. Väljer "📅 1 dag"
4. Notifikationen blir halvtransparent
5. Text läggs till: "💤 Snoozad till [datum] (1 snooz)"
6. Notifikationen döljs från "Olästa" tills snooze går ut

### Flöde 2: Markera som betald
1. Användaren ser notifikation "Påminnelse: Elräkning"
2. Klickar på "✓ Markera som betald"
3. Notifikationen markeras som läst
4. Relaterad räkning uppdateras till status "Paid"
5. Påminnelsen döljs från olästa notifikationer

### Flöde 3: Skapa transaktion från påminnelse
1. Användaren ser notifikation med räkningsinformation
2. Klickar på "📝 Skapa transaktion"
3. Navigeras till transaktionssidan
4. Formuläret är förifyllt med:
   - Belopp från räkningen
   - Mottagare/payee
   - Kategori
   - Förfallodatum som transaktionsdatum
5. Användaren kan justera och spara

### Flöde 4: Återkommande snooze detekteras
1. Användaren snooze:ar samma påminnelse 3 gånger
2. Texten "(3 snooz)" visas i meta-information
3. Vid nästa uppföljning eskaleras påminnelsen automatiskt
4. System loggar varning om återkommande snooze-mönster

## Responsiv Design

### Desktop (> 768px)
- Fullbredd notifikationer med alla knappar synliga
- Snooze-dropdown till höger om huvudknappen

### Tablet (768px - 1024px)
- Stacked layout för knappar
- Snooze-meny som overlay

### Mobil (< 768px)
- Kompakt vy med ikoner
- Swipe för quick actions
- Snooze som fullskärms-modal

## Accessibility

Alla interaktiva element har:
- Tydliga aria-labels
- Keyboard navigation support
- Fokusindikatorer
- Screen reader-vänliga beskrivningar
- Tillräcklig kontrast (WCAG 2.1 AA)

Exempel:
```html
<button aria-label="Snooze påminnelse för Elräkning">
  💤 Snooze ▼
</button>
```
