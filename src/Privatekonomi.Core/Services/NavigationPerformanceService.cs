using System.Diagnostics;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for tracking navigation performance metrics from menu click to page render
/// </summary>
public class NavigationPerformanceService : INavigationPerformanceService
{
    private static readonly ActivitySource ActivitySource = new("Privatekonomi.Navigation");
    private readonly Dictionary<string, NavigationMetric> _activeNavigations = new();
    private readonly object _lock = new();

    public void StartNavigation(string targetUrl, string sourceName)
    {
        lock (_lock)
        {
            var navigationId = Guid.NewGuid().ToString();
            var metric = new NavigationMetric
            {
                NavigationId = navigationId,
                TargetUrl = targetUrl,
                SourceName = sourceName,
                StartTime = DateTimeOffset.UtcNow,
                Activity = ActivitySource.StartActivity("Navigation", ActivityKind.Internal)
            };

            metric.Activity?.SetTag("navigation.target_url", targetUrl);
            metric.Activity?.SetTag("navigation.source", sourceName);
            metric.Activity?.SetTag("navigation.id", navigationId);

            _activeNavigations[targetUrl] = metric;
        }
    }

    public void CompleteNavigation(string targetUrl, string pageName)
    {
        lock (_lock)
        {
            if (_activeNavigations.TryGetValue(targetUrl, out var metric))
            {
                metric.EndTime = DateTimeOffset.UtcNow;
                metric.PageName = pageName;
                metric.DurationMs = (metric.EndTime.Value - metric.StartTime).TotalMilliseconds;

                metric.Activity?.SetTag("navigation.page_name", pageName);
                metric.Activity?.SetTag("navigation.duration_ms", metric.DurationMs);
                metric.Activity?.SetStatus(ActivityStatusCode.Ok);
                metric.Activity?.Stop();

                // Emit custom event for dashboard/reporting
                using var completionActivity = ActivitySource.StartActivity("NavigationCompleted", ActivityKind.Internal);
                completionActivity?.SetTag("navigation.target_url", targetUrl);
                completionActivity?.SetTag("navigation.page_name", pageName);
                completionActivity?.SetTag("navigation.duration_ms", metric.DurationMs);
                completionActivity?.SetTag("navigation.source", metric.SourceName);

                _activeNavigations.Remove(targetUrl);
            }
        }
    }

    public void CancelNavigation(string targetUrl)
    {
        lock (_lock)
        {
            if (_activeNavigations.TryGetValue(targetUrl, out var metric))
            {
                metric.Activity?.SetStatus(ActivityStatusCode.Error, "Navigation cancelled");
                metric.Activity?.Stop();
                _activeNavigations.Remove(targetUrl);
            }
        }
    }

    public NavigationMetric? GetActiveNavigation(string targetUrl)
    {
        lock (_lock)
        {
            return _activeNavigations.TryGetValue(targetUrl, out var metric) ? metric : null;
        }
    }

    public IReadOnlyList<NavigationMetric> GetAllActiveNavigations()
    {
        lock (_lock)
        {
            return _activeNavigations.Values.ToList();
        }
    }
}

public class NavigationMetric
{
    public string NavigationId { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string? PageName { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public double? DurationMs { get; set; }
    public Activity? Activity { get; set; }
}
