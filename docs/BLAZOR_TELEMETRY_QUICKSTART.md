# Blazor Telemetri - Snabbstart

## 🎯 Vad har vi implementerat?

Vi har lagt till **komplett telemetri-spårning** för Blazor-interaktioner i Privatekonomi. Detta hjälper dig att:

✅ **Spåra alla användarklick** med exakta timestamps och duration  
✅ **Diagnostisera varför knappar inte går att klicka på** (disabled, validering, etc.)  
✅ **Se fel i realtid** med full stack trace i Aspire Dashboard  
✅ **Mäta prestanda** för alla operationer  

## 📦 Vad har lagts till?

### Nya filer:
1. **`src/Privatekonomi.Web/Telemetry/BlazorActivitySource.cs`**  
   - ActivitySource för OpenTelemetry-spårning
   - Helper-metoder: `StartUserClick()`, `StartFormSubmit()`, `RecordDisabledClick()`, etc.

2. **`src/Privatekonomi.Web/Components/Base/TrackedComponentBase.cs`**  
   - Basklass för komponenter med inbyggd telemetri
   - Automatisk spårning av lifecycle (OnInitialized, OnParametersSet, etc.)
   - Helper-metoder: `TrackClick()`, `TrackBlockedClick()`, `TrackFormSubmit()`, `TrackValidationBlock()`

3. **`src/Privatekonomi.Web/Components/Pages/TelemetryExample.razor`**  
   - Live demo-sida på `/telemetry-example`
   - Visar alla typer av spårning i praktiken

4. **`docs/BLAZOR_TELEMETRY_GUIDE.md`**  
   - Fullständig guide med exempel och best practices

### Modifierade filer:
- **`src/Privatekonomi.ServiceDefaults/Extensions.cs`** - Registrering av `Privatekonomi.Web.Blazor` ActivitySource
- **`src/Privatekonomi.Web/appsettings.Development.json`** - Debug-logging för Blazor Server Circuit
- **`src/Privatekonomi.Web/Components/Layout/NavMenu.razor`** - Utvecklarmeny med länk till test-sida

## 🚀 Kom igång på 30 sekunder

### 1. Starta applikationen med Aspire

```bash
cd src/Privatekonomi.AppHost
dotnet run
```

### 2. Öppna Aspire Dashboard

Klicka på länken som visas i konsolen (vanligtvis `https://localhost:17033`)

### 3. Testa telemetri

1. Logga in på webbapplikationen (`https://localhost:5274`)
2. Navigera till **Utvecklare → Telemetri Test** (endast synlig i development mode)
3. Klicka på de olika knapparna
4. Gå tillbaka till Aspire Dashboard → **Traces**
5. Filtrera på resource: `privatekonomi-web`
6. Se dina klick i realtid! 🎉

## 💻 Använd i din komponent

### Enklaste sättet - Ärv från TrackedComponentBase:

```razor
@page "/my-page"
@using Privatekonomi.Web.Components.Base
@inherits TrackedComponentBase
@rendermode InteractiveServer

<MudButton OnClick="SaveData">Spara</MudButton>

@code {
    private async Task SaveData()
    {
        using var activity = TrackClick("save-btn", "save-transaction");
        
        try
        {
            await _service.SaveAsync(data);
            Logger.LogInformation("Saved successfully");
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            Logger.LogError(ex, "Save failed");
        }
    }
}
```

### Spåra blockerade klick:

```csharp
<MudButton Disabled="@(!_canSave)" OnClick="TrySave">Spara</MudButton>

@code {
    private void TrySave()
    {
        if (!_canSave)
        {
            TrackBlockedClick("save-btn", "User lacks permission");
            return;
        }
        // ... save logic
    }
}
```

### Spåra valideringsfel:

```csharp
private async Task SubmitForm()
{
    using var activity = TrackFormSubmit("my-form");
    
    if (string.IsNullOrEmpty(_email))
    {
        TrackValidationBlock("email", "Email is required");
        return;
    }
    
    // ... submit logic
}
```

## 🔍 Hitta traces i Aspire Dashboard

1. **Öppna Aspire Dashboard** (https://localhost:17033)
2. **Klicka på "Traces"** i vänstermenyn
3. **Filtrera på resource**: `privatekonomi-web`
4. **Sök efter**:
   - `User.Click` - Normal klick
   - `User.ClickBlocked` - Blockerade klick (se varför!)
   - `Form.Submit` - Formulär
   - `Validation.Block` - Valideringsfel
5. **Klicka på en trace** för att se:
   - Duration (hur lång tid tog det?)
   - Tags (komponent, element-ID, action, felmeddelande)
   - Error details (stack trace om något gick fel)

## 🐛 Felsökning "Kan inte klicka"

### Problem: Knappen reagerar inte

**Lösning:**
1. Öppna Aspire Dashboard → Traces
2. Leta efter `User.ClickBlocked` events
3. Kolla `blocked.reason` tag - där står varför!

### Problem: Formulär validerar inte

**Lösning:**
1. Sök efter `Validation.Block` i traces
2. Titta på `validation.errors` tag
3. Se exakt vilket fält som failade

### Problem: SignalR-anslutning bruten

**Lösning:**
1. Aspire Dashboard → Logs → privatekonomi-web
2. Filtrera på "Circuit" eller "SignalR"
3. Se om det är reconnection-försök

## 📊 Vad loggas?

### För varje användarklick:
- **Komponentnamn** - Vilken komponent
- **Element-ID** - Vilket element
- **Action** - Vilken åtgärd (t.ex. "save", "delete")
- **Duration** - Hur lång tid det tog
- **Status** - Success/Error
- **Exception details** - Om något gick fel

### För blockerade klick:
- **Blocked reason** - Varför klicket blockerades
- **Element-ID** - Vilken knapp
- **Timestamp** - När försöket gjordes

### För valideringsfel:
- **Field name** - Vilket fält
- **Validation errors** - Lista med felmeddelanden
- **Komponent** - Var felet uppstod

## ⚡ Prestanda

- **Development**: ~1-2ms overhead per trace (försumbart)
- **Production**: Ej aktiverat som standard
- **Sampling**: Kan konfigureras för att bara spåra X% av requests

## 📖 Läs mer

- **Fullständig guide**: [docs/BLAZOR_TELEMETRY_GUIDE.md](BLAZOR_TELEMETRY_GUIDE.md)
- **Aspire guide**: [docs/ASPIRE_GUIDE.md](ASPIRE_GUIDE.md)
- **Test-sida**: `/telemetry-example` i appen

## ✨ Best Practices

✅ **Gör detta:**
- Ärv från `TrackedComponentBase` för komplexa komponenter
- Använd `using` statements för activities
- Logga varför knappar är disabled
- Fånga exceptions med `activity.RecordException(ex)`

❌ **Undvik detta:**
- Spåra varje liten komponent (overhead)
- Glömma dispose activities
- Logga känslig data i tags

## 🎉 Färdigt!

Nu kan du spåra **varje klick**, se **varför saker är blockerade**, och **felsöka UI-problem** direkt i Aspire Dashboard!

**Prova det nu:**
1. Starta appen (`cd src/Privatekonomi.AppHost && dotnet run`)
2. Gå till `/telemetry-example`
3. Klicka runt
4. Se traces i Aspire Dashboard

Lycka till! 🚀
