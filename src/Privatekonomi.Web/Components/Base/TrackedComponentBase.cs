using Microsoft.AspNetCore.Components;
using Privatekonomi.Web.Telemetry;
using System.Diagnostics;

namespace Privatekonomi.Web.Components.Base;

/// <summary>
/// Basklass för Blazor-komponenter med inbyggd telemetri-spårning.
/// Använd denna för komponenter där du vill spåra rendering och interaktioner.
/// </summary>
public abstract class TrackedComponentBase : ComponentBase
{
    private Activity? _renderActivity;

    /// <summary>
    /// Komponentens namn (används för telemetri)
    /// </summary>
    protected virtual string ComponentName => GetType().Name;

    /// <summary>
    /// Logger som kan användas i ärvande komponenter
    /// </summary>
    [Inject]
    protected ILogger<TrackedComponentBase> Logger { get; set; } = default!;

    protected override void OnInitialized()
    {
        using var activity = BlazorActivitySource.Source.StartActivity($"{ComponentName}.Initialize");
        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        using var activity = BlazorActivitySource.Source.StartActivity($"{ComponentName}.InitializeAsync");
        await base.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        using var activity = BlazorActivitySource.Source.StartActivity($"{ComponentName}.ParametersSet");
        base.OnParametersSet();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        _renderActivity?.Dispose();
        _renderActivity = null;
        base.OnAfterRender(firstRender);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _renderActivity?.Dispose();
        _renderActivity = null;
        await base.OnAfterRenderAsync(firstRender);
    }

    /// <summary>
    /// Spårar ett användarklick
    /// </summary>
    protected Activity? TrackClick(string elementId, string? action = null)
    {
        Logger.LogDebug("User clicked {ElementId} in {ComponentName} (action: {Action})", 
            elementId, ComponentName, action ?? "none");
        return BlazorActivitySource.StartUserClick(ComponentName, elementId, action);
    }

    /// <summary>
    /// Spårar en blockerad klick (t.ex. disabled knapp)
    /// </summary>
    protected void TrackBlockedClick(string elementId, string reason)
    {
        Logger.LogWarning("Click blocked on {ElementId} in {ComponentName}: {Reason}", 
            elementId, ComponentName, reason);
        BlazorActivitySource.RecordDisabledClick(ComponentName, elementId, reason);
    }

    /// <summary>
    /// Spårar en formulärinlämning
    /// </summary>
    protected Activity? TrackFormSubmit(string formName)
    {
        Logger.LogDebug("Form submitted: {FormName} in {ComponentName}", formName, ComponentName);
        return BlazorActivitySource.StartFormSubmit(ComponentName, formName);
    }

    /// <summary>
    /// Spårar en validering som blockerar en action
    /// </summary>
    protected void TrackValidationBlock(string fieldName, params string[] errors)
    {
        Logger.LogWarning("Validation blocked {FieldName} in {ComponentName}: {Errors}", 
            fieldName, ComponentName, string.Join(", ", errors));
        BlazorActivitySource.RecordValidationBlock(ComponentName, fieldName, errors);
    }
}
