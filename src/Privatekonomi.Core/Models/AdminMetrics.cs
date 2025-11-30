namespace Privatekonomi.Core.Models;

/// <summary>
/// Comprehensive metrics for admin dashboard
/// </summary>
public class AdminMetrics
{
    public UserMetrics UserMetrics { get; set; } = new();
    public EngagementMetrics EngagementMetrics { get; set; } = new();
    public PerformanceMetrics PerformanceMetrics { get; set; } = new();
    public SecurityMetrics SecurityMetrics { get; set; } = new();
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// User-related metrics
/// </summary>
public class UserMetrics
{
    // Monthly Active Users
    public int MAU { get; set; }
    public int PreviousMAU { get; set; }
    public decimal MAUGrowthPercent { get; set; }
    
    // Daily Active Users
    public int DAU { get; set; }
    public decimal DAUMAURatio { get; set; }
    
    // Retention
    public decimal RetentionRate30Day { get; set; }
    
    // Churn
    public decimal MonthlyChurnRate { get; set; }
    
    // Total users
    public int TotalUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
}

/// <summary>
/// Engagement-related metrics
/// </summary>
public class EngagementMetrics
{
    // Transactions per user
    public decimal TransactionsPerUser { get; set; }
    
    // Average session duration (in seconds)
    public double AverageSessionDuration { get; set; }
    
    // Feature adoption (% of users using new features)
    public decimal FeatureAdoptionRate { get; set; }
    
    // Net Promoter Score (mock for now)
    public decimal NPS { get; set; }
    
    // Total transactions this month
    public int TotalTransactionsThisMonth { get; set; }
    
    // Active features count
    public int ActiveFeaturesCount { get; set; }
}

/// <summary>
/// Performance-related metrics
/// </summary>
public class PerformanceMetrics
{
    // Uptime percentage
    public decimal UptimePercent { get; set; }
    
    // Average page load time (in seconds)
    public double AveragePageLoadTime { get; set; }
    
    // Lighthouse score (mock for now, would need real implementation)
    public int LighthouseScore { get; set; }
    
    // Crash rate
    public decimal CrashRate { get; set; }
    
    // Error count this month
    public int ErrorCount { get; set; }
}

/// <summary>
/// Security-related metrics
/// </summary>
public class SecurityMetrics
{
    // 2FA adoption rate
    public decimal TwoFactorAdoptionRate { get; set; }
    
    // Failed login attempts percentage
    public decimal FailedLoginAttemptsPercent { get; set; }
    
    // Security incidents count
    public int SecurityIncidentsCount { get; set; }
    
    // GDPR compliance (should always be 100%)
    public decimal GDPRCompliancePercent { get; set; }
    
    // Total login attempts this month
    public int TotalLoginAttempts { get; set; }
    public int FailedLoginAttempts { get; set; }
}

/// <summary>
/// Historical metrics for time period filtering
/// </summary>
public class MetricsSnapshot
{
    public int MetricsSnapshotId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public MetricsPeriodType PeriodType { get; set; }
    public string MetricsJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum MetricsPeriodType
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}
