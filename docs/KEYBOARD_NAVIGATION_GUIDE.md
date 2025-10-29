# Tangentbordsnavigering - EditTransactionDialog

## Översikt
EditTransactionDialog stödjer fullständig tangentbordsnavigering för snabbare och mer tillgänglig användning.

## Navigering mellan Fält

### Grundläggande Navigation
| Tangent | Funktion |
|---------|----------|
| `Tab` | Flytta till nästa fält/knapp |
| `Shift + Tab` | Flytta till föregående fält/knapp |
| `Enter` | Aktivera knapp eller bekräfta val i dropdown |
| `Escape` | Stäng dropdown/autocomplete eller avbryt dialog |

## Specifika Fält

### Textfält (Beskrivning, Mottagare, Taggar, Noteringar)
| Tangent | Funktion |
|---------|----------|
| `Tab` | Gå till nästa fält |
| `Shift + Tab` | Gå till föregående fält |
| Standard textredigeringstangenter | Fungerar normalt (Ctrl+A, Ctrl+C, Ctrl+V, etc.) |

### Numeriska Fält (Belopp, Procent)
| Tangent | Funktion |
|---------|----------|
| `Upp-pil` | Öka värde med 1 |
| `Ner-pil` | Minska värde med 1 |
| `Page Up` | Öka värde med 10 |
| `Page Down` | Minska värde med 10 |

### Datumväljare
| Tangent | Funktion |
|---------|----------|
| `Space` eller `Enter` | Öppna kalendern |
| `Upp/Ner-pilar` | Navigera mellan veckor i kalendern |
| `Vänster/Höger-pilar` | Navigera mellan dagar |
| `Page Up/Down` | Byta månad |
| `Home` | Gå till första dagen i månaden |
| `End` | Gå till sista dagen i månaden |
| `Enter` | Välj markerat datum |
| `Escape` | Stäng kalender utan att välja |

### Autocomplete (Kategorival)
| Tangent | Funktion |
|---------|----------|
| Börja skriva | Filtrera kategorier automatiskt |
| `Upp-pil` | Navigera till föregående alternativ i listan |
| `Ner-pil` | Navigera till nästa alternativ i listan |
| `Enter` | Välj markerat alternativ |
| `Escape` | Stäng dropdown utan att välja |
| `Tab` | Stäng dropdown och gå till nästa fält |

### Dropdown (Hushåll, Betalningsmetod)
| Tangent | Funktion |
|---------|----------|
| `Space` eller `Enter` | Öppna dropdown |
| `Upp/Ner-pilar` | Navigera i listan |
| `Enter` | Välj alternativ |
| `Escape` | Stäng utan att välja |

### Radio-knappar (Kategorival, Delningsmetod)
| Tangent | Funktion |
|---------|----------|
| `Upp/Vänster-pil` | Välj föregående alternativ |
| `Ner/Höger-pil` | Välj nästa alternativ |
| `Space` | Välj markerat alternativ |

### Switch (Inkomst/Utgift)
| Tangent | Funktion |
|---------|----------|
| `Space` | Växla mellan Inkomst och Utgift |
| `Enter` | Växla mellan Inkomst och Utgift |

### Knappar
| Tangent | Funktion |
|---------|----------|
| `Space` eller `Enter` | Aktivera knapp |
| `Tab` | Gå till nästa knapp/fält |

## Snabbkommandon

### När Dialogen är Öppen
| Tangent | Funktion |
|---------|----------|
| `Escape` | Avbryt och stäng dialogen (samma som "Avbryt"-knappen) |
| `Tab` genom alla fält + `Enter` på "Spara" | Spara ändringar |

## Arbetsflöde: Tangentbordsanvändning

### Exempel 1: Redigera Grundläggande Information
```
1. Tab (x1) → Fokus på Beskrivning
2. Ändra beskrivning
3. Tab (x1) → Fokus på Datum
4. Enter → Öppna kalender
5. Använd pilar för att välja datum
6. Enter → Bekräfta datum
7. Tab (x1) → Fokus på Belopp
8. Ange belopp
9. Tab (flera gånger) → Navigera till "Spara"
10. Enter → Spara ändringar
```

### Exempel 2: Välja Kategori med Autocomplete
```
1. Tab till Kategori-autocomplete
2. Börja skriva kategorinamn (t.ex. "mat")
3. Ner-pil → Navigera till önskat alternativ
4. Enter → Välj kategori
5. Tab → Gå till nästa fält
```

### Exempel 3: Skapa Split-Transaktion
```
1. Tab till "Kategorier"
2. Ner-pil → Välj "Dela på flera kategorier"
3. Tab → Fokus på delningsmetod
4. Ner-pil → Välj "Dela via procent"
5. Tab → Fokus på första kategorin
6. Börja skriva kategorinamn
7. Ner-pil + Enter → Välj kategori
8. Tab → Fokus på procentfält
9. Ange procent
10. Tab → Gå till nästa kategori
11. Upprepa steg 6-9 för varje kategori
12. Tab till "Spara"
13. Enter → Spara
```

## Tillgänglighetstips

### För Skärmläsaranvändare
- Alla fält har beskrivande etiketter som läses upp
- Hjälptexter ger ytterligare kontext
- Valideringsfel läses upp när de uppstår
- Fokusordning är logisk och följer visuell layout

### För Motoriska Funktionshinder
- Stora knappmål (minst 44x44 pixlar)
- Tydlig fokusstil visar vilket fält som är aktivt
- Inga tidsbegränsade interaktioner
- Kräver inte precision för att träffa knappar

### För Synnedsättning
- Hög kontrast på alla texter
- Fokusindikator är tydligt synlig
- Textstorlek kan förstoras utan förlust av funktionalitet
- Färg används aldrig som enda indikator

## Felsökning

### Problem: Tab fungerar inte som förväntat
**Lösning**: Kontrollera att du inte är fokuserad på ett flerradigt textfält (Noteringar) där Tab kan användas för indentering.

### Problem: Enter öppnar inte dropdown
**Lösning**: Använd Space istället, eller klicka på fältet först för att ge det fokus.

### Problem: Autocomplete filtrerar inte
**Lösning**: Se till att fältet har fokus och börja skriva. Filtreringen sker automatiskt.

## Browser-Kompatibilitet

Tangentbordsnavigering fungerar i:
- ✅ Chrome/Edge 90+
- ✅ Firefox 88+
- ✅ Safari 14+
- ✅ Opera 76+

## Ytterligare Resurser

- [MudBlazor Accessibility Docs](https://mudblazor.com/getting-started/accessibility)
- [WCAG 2.1 Keyboard Guidelines](https://www.w3.org/WAI/WCAG21/Understanding/keyboard.html)
- [ARIA Authoring Practices](https://www.w3.org/WAI/ARIA/apg/)
