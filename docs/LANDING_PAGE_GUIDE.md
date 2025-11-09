# Landningssida för Ekonomiappen.se (Beta)

## Översikt

Landningssidan är en visuellt tilltalande sida som presenterar applikationen för potentiella användare under betafasen. Sidan är tillgänglig på `/landing` och visar en översikt av systemets funktioner samt ett "kommer snart"-meddelande.

## Funktioner

### Hero-sektion
- Stor rubrik med applikationsnamn och ikon
- Tagline: "Din kompletta privatekonomi i en app"
- Kort beskrivning av applikationen
- Prominent "BETA - Kommer snart" badge

### Funktionsöversikt
Sidan presenterar 6 huvudfunktioner i kortformat:
1. **Dashboard** - Översikt med anpassningsbara widgets
2. **Transaktionshantering** - Registrera och kategorisera transaktioner
3. **Budgethantering** - Svenska mallar och visualisering
4. **Sparmål** - Målstolpar och gemensamma sparmål
5. **Investeringar & Pension** - Hantera aktier, fonder, ETF och pensioner
6. **Kontohantering** - Stöd för BAS-kontoplan

### Sverige-specifika funktioner
- ROT/RUT-avdrag
- K4 Kapitalvinstrapport
- ISK/KF Schablonbeskattning
- SIE-export för bokföring

### Teknologi-stack
Visar de teknologier som används:
- .NET 9
- Blazor Server
- MudBlazor
- Entity Framework
- .NET Aspire
- PWA

### Call-to-Action
- "Anmäl intresse" knapp (för närvarande inaktiverad)
- Länk till GitHub-repository

## Design

### Färgschema
- **Primärfärg**: #6366F1 (Indigo blue)
- **Gradient bakgrund**: Linear gradient från #667eea till #764ba2
- **Textfärger**: Vit på gradient, standard på vita bakgrunder
- **Accentfärger**: Använder MudBlazor's färgpaletter

### Responsiv design
Sidan är fullt responsiv och optimerad för:
- **Desktop** (> 768px): Tre kolumner för funktionskort
- **Tablet** (480px - 768px): Två kolumner för funktionskort
- **Mobil** (< 480px): En kolumn, anpassade textstorlekar

### Interaktivitet
- Hover-effekter på funktionskort (lyfts upp och får skugga)
- Hover-effekter på Sverige-specifika funktioner (skalas upp)
- Smooth transitions för alla animationer

## Användning

### Åtkomst
Navigera till `/landing` i din webbläsare när applikationen körs:
```
http://localhost:5274/landing
```

### Integration i routing
Sidan är automatiskt tillgänglig via Blazor's routing-system tack vare `@page "/landing"` direktivet.

### Uppdatera landningssidan för produktion
När applikationen går ur beta:

1. **Ta bort BETA-badge**: Redigera `Landing.razor` och kommentera ut eller ta bort:
```razor
<MudChip T="string" Icon="@Icons.Material.Filled.Info" Color="Color.Primary" Size="Size.Large" Class="beta-chip">
    BETA - Kommer snart
</MudChip>
```

2. **Aktivera "Anmäl intresse"-knappen**: Ta bort `Disabled="true"` attributet:
```razor
<MudButton Variant="Variant.Filled" 
          Color="Color.Primary" 
          Size="Size.Large"
          StartIcon="@Icons.Material.Filled.Notifications"
          Href="/Account/Register">  <!-- Lägg till länk till registrering -->
    Anmäl intresse
</MudButton>
```

3. **Uppdatera CTA-texten**:
```razor
<MudText Typo="Typo.body1" Align="Align.Center" Color="Color.Secondary" Class="mb-4">
    Börja använda Ekonomiappen idag och få koll på din privatekonomi!
</MudText>
```

## Anpassning

### Ändra färgschema
Redigera `/wwwroot/css/landing.css`:

```css
.landing-page {
    background: linear-gradient(135deg, #din-färg-1 0%, #din-färg-2 100%);
}

.cta-paper {
    background: linear-gradient(135deg, #din-färg-1 0%, #din-färg-2 100%);
}
```

### Lägg till fler funktioner
I `Landing.razor`, lägg till ett nytt `MudItem` under `features-grid`:

```razor
<MudItem xs="12" sm="6" md="4">
    <MudCard Elevation="2" Class="feature-card">
        <MudCardContent>
            <div class="feature-icon">
                <MudIcon Icon="@Icons.Material.Filled.YourIcon" Size="Size.Large" Color="Color.Primary" />
            </div>
            <MudText Typo="Typo.h6" GutterBottom="true">Din funktionsrubrik</MudText>
            <MudText Typo="Typo.body2" Color="Color.Secondary">
                Din funktionsbeskrivning
            </MudText>
        </MudCardContent>
    </MudCard>
</MudItem>
```

### Ändra texter
Alla texter finns direkt i `Landing.razor` och kan enkelt redigeras för att matcha dina behov.

## Tekniska detaljer

### Filer
- **Komponent**: `/src/Privatekonomi.Web/Components/Pages/Landing.razor`
- **Styling**: `/src/Privatekonomi.Web/wwwroot/css/landing.css`

### Beroenden
- MudBlazor (för UI-komponenter)
- Blazor Server (för rendering)
- Ingen backend-logik krävs

### Prestanda
- Statisk sida utan API-anrop
- Snabb laddningstid
- Minimala beroenden

## Framtida förbättringar

### Förslag på tillägg
- [ ] Animationer vid scroll
- [ ] Video- eller bildspel som visar applikationen
- [ ] Testimonials från användare
- [ ] FAQ-sektion
- [ ] E-postformulär för anmälan
- [ ] Internationalisering (flera språk)
- [ ] Mörklägesstöd
- [ ] Analytics-integration för att spåra besökare

### SEO-optimering
För att förbättra SEO:
1. Lägg till meta-taggar för beskrivning och keywords
2. Implementera Open Graph tags
3. Lägg till structured data (JSON-LD)
4. Optimera bilder och laddningstid

## Support

För frågor eller problem med landningssidan, öppna en issue på GitHub:
https://github.com/pownas/Privatekonomi/issues

## Licens

Samma licens som huvudprojektet.
