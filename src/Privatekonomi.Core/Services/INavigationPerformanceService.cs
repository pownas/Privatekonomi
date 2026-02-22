namespace Privatekonomi.Core.Services;

public interface INavigationPerformanceService
{
    void StartNavigation(string targetUrl, string sourceName);
    void CompleteNavigation(string targetUrl, string pageName);
    void CancelNavigation(string targetUrl);
    NavigationMetric? GetActiveNavigation(string targetUrl);
    IReadOnlyList<NavigationMetric> GetAllActiveNavigations();
}
