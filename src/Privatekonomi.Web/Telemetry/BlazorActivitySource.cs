using System.Diagnostics;

namespace Privatekonomi.Web.Telemetry;

/// <summary>
/// Activity source for spårning av Blazor-interaktioner och komponenthändelser.
/// Används för att diagnostisera användarinteraktioner och hitta blockeringar i UI.
/// </summary>
public static class BlazorActivitySource
{
    /// <summary>
    /// ActivitySource namn som registreras i OpenTelemetry
    /// </summary>
    public const string SourceName = "Privatekonomi.Web.Blazor";

    /// <summary>
    /// ActivitySource för Blazor-events
    /// </summary>
    public static readonly ActivitySource Source = new(SourceName, "1.0.0");

    /// <summary>
    /// Skapar en activity för komponentrendering
    /// </summary>
    public static Activity? StartComponentRender(string componentName)
    {
        var activity = Source.StartActivity($"Component.Render", ActivityKind.Internal);
        activity?.SetTag("component.name", componentName);
        return activity;
    }

    /// <summary>
    /// Skapar en activity för användarklick
    /// </summary>
    public static Activity? StartUserClick(string componentName, string elementId, string? action = null)
    {
        var activity = Source.StartActivity($"User.Click", ActivityKind.Internal);
        activity?.SetTag("component.name", componentName);
        activity?.SetTag("element.id", elementId);
        if (!string.IsNullOrEmpty(action))
        {
            activity?.SetTag("action", action);
        }
        return activity;
    }

    /// <summary>
    /// Skapar en activity för formulärinlämning
    /// </summary>
    public static Activity? StartFormSubmit(string componentName, string formName)
    {
        var activity = Source.StartActivity($"Form.Submit", ActivityKind.Internal);
        activity?.SetTag("component.name", componentName);
        activity?.SetTag("form.name", formName);
        return activity;
    }

    /// <summary>
    /// Loggar ett fel i en activity
    /// </summary>
    public static void RecordException(this Activity? activity, Exception exception)
    {
        if (activity == null) return;

        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.SetTag("error.type", exception.GetType().Name);
        activity.SetTag("error.message", exception.Message);
        activity.SetTag("error.stacktrace", exception.StackTrace);
    }

    /// <summary>
    /// Loggar att en knapp är disabled
    /// </summary>
    public static void RecordDisabledClick(string componentName, string elementId, string reason)
    {
        using var activity = Source.StartActivity($"User.ClickBlocked", ActivityKind.Internal);
        activity?.SetTag("component.name", componentName);
        activity?.SetTag("element.id", elementId);
        activity?.SetTag("blocked.reason", reason);
        activity?.SetTag("event.blocked", true);
    }

    /// <summary>
    /// Loggar validering som blockerar en action
    /// </summary>
    public static void RecordValidationBlock(string componentName, string fieldName, string[] errors)
    {
        using var activity = Source.StartActivity($"Validation.Block", ActivityKind.Internal);
        activity?.SetTag("component.name", componentName);
        activity?.SetTag("field.name", fieldName);
        activity?.SetTag("validation.errors", string.Join(", ", errors));
        activity?.SetTag("event.blocked", true);
    }
}
