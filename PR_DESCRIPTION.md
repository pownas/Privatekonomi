# PR: Navigation Performance Tracking

## Översikt

Detta PR implementerar ett system för att spåra användarens navigationsprestanda från klick i menyn till att sidan är helt renderad. Systemet integreras med .NET Aspire för telemetri och ger utvecklare verktyg för att mäta och optimera sidladdningstider.

## Ändringar

### Nya filer

1. **`src/Privatekonomi.Core/Services/NavigationPerformanceService.cs`**
   - Service för att hantera navigationsmetriker
   - Använder `ActivitySource` för distributed tracing
   - Spårar start, completion och duration av navigationer

2. **`src/Privatekonomi.Core/Services/INavigationPerformanceService.cs`**
   - Interface för NavigationPerformanceService
   - Metoder: `StartNavigation`, `CompleteNavigation`, `CancelNavigation`, `GetActiveNavigation`, `GetAllActiveNavigations`

3. **`src/Privatekonomi.Web/Components/Shared/PerformanceTrackedPageBase.cs`**
   - Basklass för sidor som ska rapportera navigation completion
   - Ärver från `TrackedComponentBase` för full telemetri-support
   - Automatiskt anropar `CompleteNavigation` i `OnAfterRender`

4. **`src/Privatekonomi.Web/Components/Pages/NavigationPerformance.razor`**
   - Dashboard för realtidsvisning av navigationsmetriker
   - Visar aktiva navigeringar med progress indicators
   - Endast synlig i Development-läge
   - Inkluderar implementationsguide för utvecklare

5. **`docs/NAVIGATION_PERFORMANCE_TRACKING.md`**
   - Komplett dokumentation av funktionen
   - Användningsexempel
   - Arkitekturdiagram
   - Prestandamål och felsökningsguide

### Modifierade filer

1. **`src/Privatekonomi.Web/Program.cs`**
   - Registrerar `INavigationPerformanceService` i DI-containern

2. **`src/Privatekonomi.Web/Components/Layout/NavMenu.razor`**
   - Lägger till `OnClick`-händelser på alla `MudNavLink`-komponenter
   - Anropar `StartNavigation` när användaren klickar
   - Ny menypost för `/navigation-performance` under Utvecklare-gruppen

3. **`src/Privatekonomi.Web/Components/Pages/TelemetryExample.razor`**
   - Dokumentation har uppdaterats för att visa integration med navigation tracking

## Funktionalitet

### Hur det fungerar

1. **Klick i menyn**: När användaren klickar på en länk i `NavMenu`, startas en `Activity` via `NavigationPerformanceService.StartNavigation()`
2. **Navigation**: Blazor Server navigerar till målsidan
3. **Sidan renderas**: När sidan är färdigrenderad (`OnAfterRender` med `firstRender=true`) anropas `CompleteNavigation()`
4. **Telemetri**: Activity stoppas och duration beräknas automatiskt

### Spårade metriker

- `navigation.target_url`: URL till målsidan
- `navigation.source`: Varifrån navigationen initierades (t.ex. "NavMenu:Transaktioner")
- `navigation.page_name`: Namnet på målsidan
- `navigation.duration_ms`: Tid från klick till render i millisekunder
- `navigation.id`: Unikt ID för varje navigation

## Användning

### För nya sidor

```razor
@page "/min-sida"
@inherits Privatekonomi.Web.Components.Shared.PerformanceTrackedPageBase

<PageTitle>Min Sida</PageTitle>

<!-- Din sidmarkup här -->

@code {
    protected override string PageName => "Min Sida";
}
```

### För befintliga sidor som redan har en basklass

```razor
@inject INavigationPerformanceService NavigationPerformanceService
@inject NavigationManager NavigationManager

@code {
    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        
        if (firstRender)
        {
            var currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            NavigationPerformanceService.CompleteNavigation(currentUrl, "Sidnamn");
        }
    }
}
```

## Visualisering

### I applikationen

Navigera till `/navigation-performance` (endast i Development):
- Realtidsvy av aktiva navigeringar
- Information om senaste navigeringar
- Implementationsguide

### I Aspire Dashboard

1. Öppna Aspire Dashboard (https://localhost:17033)
2. Gå till **Traces**
3. Filtrera på `privatekonomi-web`
4. Sök efter traces med namnet `Navigation` eller `NavigationCompleted`

## Prestandamål

- **Utmärkt**: < 100 ms (grön)
- **Bra**: 100-300 ms (blå)
- **Acceptabel**: 300-500 ms (gul)
- **Långsam**: > 500 ms (röd)

## Testing

För att testa:

1. Starta applikationen med Aspire (`cd src/Privatekonomi.AppHost && dotnet run`)
2. Öppna Aspire Dashboard
3. Klicka på olika menyalternativ i applikationen
4. Se traces i Aspire Dashboard under "Traces"
5. Besök `/navigation-performance` för realtidsvy (endast Development)

## Screenshot

![Navigation Performance Dashboard](/docs/images/navigation-performance-tracking.png)
*(Lägg till screenshot här när PR granskas)*

## Framtida förbättringar

- [ ] Persistent storage av metriker för långtidsanalys
- [ ] Percentil-statistik (P50, P95, P99)
- [ ] Alerting för ovanligt långsamma navigationer
- [ ] Client-side performance metrics (FCP, LCP, TTI)
- [ ] User segmentation för olika användargrupper

## Påverkan

- ✅ Ingen påverkan på befintlig funktionalitet
- ✅ Opt-in för nya sidor (genom att ärva från `PerformanceTrackedPageBase`)
- ✅ Dashboard endast synlig i Development
- ✅ Minimal overhead (endast telemetri när Aspire körs)

## Checklista

- [x] Kod kompilerar utan fel
- [x] Nya klasser och metoder har XML-kommentarer
- [x] Service registrerad i DI-container
- [x] Dokumentation skapad (`docs/NAVIGATION_PERFORMANCE_TRACKING.md`)
- [x] Exempel-implementationer finns
- [x] Integrerad med befintlig telemetri-infrastruktur
- [ ] Screenshot tillagd (lägg till vid granskning)
- [ ] Testat manuellt med Aspire Dashboard

## Relaterade Issues

Ingen specifik issue, men detta är del av den övergripande observability-strategin för applikationen.

## Merge-strategi

Squash and merge rekommenderas för att hålla historiken ren.
