namespace Privatekonomi.Core.Services.Notifications;

/// <summary>
/// Service for sending email notifications
/// </summary>
public interface IEmailNotificationService
{
    /// <summary>
    /// Send an email notification
    /// </summary>
    Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
}

/// <summary>
/// Service for sending SMS notifications
/// </summary>
public interface ISmsNotificationService
{
    /// <summary>
    /// Send an SMS notification
    /// </summary>
    Task<bool> SendSmsAsync(string phoneNumber, string message);
}

/// <summary>
/// Service for sending push notifications (PWA)
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Send a push notification
    /// </summary>
    Task<bool> SendPushAsync(string userId, string title, string message, string? actionUrl = null);
}

/// <summary>
/// Service for sending Slack notifications
/// </summary>
public interface ISlackNotificationService
{
    /// <summary>
    /// Send a Slack notification
    /// </summary>
    Task<bool> SendSlackMessageAsync(string webhookUrl, string message);
}

/// <summary>
/// Service for sending Microsoft Teams notifications
/// </summary>
public interface ITeamsNotificationService
{
    /// <summary>
    /// Send a Teams notification
    /// </summary>
    Task<bool> SendTeamsMessageAsync(string webhookUrl, string message);
}
