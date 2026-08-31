#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Mail;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Service for sending notifications to users. Abstracts notification delivery
/// (email, SMS, push, webhook) to allow flexible implementations.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Sends a notification to a user.
    /// </summary>
    public async Task<bool> SendNotificationAsync(
        Guid userId,
        string subject,
        string message,
        NotificationType type = NotificationType.Email,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(message);

        cancellationToken.ThrowIfCancellationRequested();

        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning(
                "Notification was not sent because user ID, subject, or message is invalid");
            return false;
        }

        _logger.LogWarning(
            "{NotificationType} notification delivery to user {UserId} is not implemented",
            type, userId);

        return false;
    }

    /// <summary>
    /// Sends a notification to an email address.
    /// </summary>
    public async Task<bool> SendEmailAsync(
        string emailAddress,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(emailAddress);
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(htmlBody);

        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(emailAddress)
            || !MailAddress.TryCreate(emailAddress, out _)
            || string.IsNullOrWhiteSpace(subject)
            || string.IsNullOrWhiteSpace(htmlBody))
        {
            _logger.LogWarning(
                "Email was not sent because the email address, subject, or body is invalid");
            return false;
        }

        _logger.LogWarning(
            "Email delivery to {EmailAddress} is not implemented",
            emailAddress);

        return false;
    }

    /// <summary>
    /// Sends a bulk notification to multiple users.
    /// </summary>
    public async Task<int> SendBulkNotificationAsync(
        IEnumerable<Guid> userIds,
        string subject,
        string message,
        NotificationType type = NotificationType.Email,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userIds);
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(message);

        cancellationToken.ThrowIfCancellationRequested();

        if (userIds is null || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning(
                "Bulk notification was not sent because user IDs, subject, or message is invalid");
            return 0;
        }

        var userList = userIds.ToList();
        var successCount = 0;

        if (userList.Count == 0 || userList.Any(userId => userId == Guid.Empty))
        {
            _logger.LogWarning(
                "Bulk notification was not sent because the user ID collection is empty or contains an invalid ID");
            return 0;
        }

        _logger.LogWarning(
            "Bulk {NotificationType} notification delivery to {Count} users is not implemented",
            type, userList.Count);

        foreach (var userId in userList)
        {
            if (await SendNotificationAsync(userId, subject, message, type, cancellationToken))
            {
                successCount++;
            }
        }

        return successCount;
    }

    /// <summary>
    /// Sends an alert notification for critical events.
    /// </summary>
    public async Task<bool> SendAlertAsync(
        string alertType,
        string description,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(alertType);
        ArgumentException.ThrowIfNullOrEmpty(description);

        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(alertType) || string.IsNullOrWhiteSpace(description))
        {
            _logger.LogWarning(
                "Alert was not sent because the alert type or description is invalid");
            return false;
        }

        _logger.LogWarning(
            "Alert delivery for {AlertType} is not implemented. Description: {Description}. Details: {Details}",
            alertType, description, details ?? "N/A");

        return false;
    }
}

/// <summary>
/// Interface for notification service.
/// </summary>
public interface INotificationService
{
    Task<bool> SendNotificationAsync(
        Guid userId,
        string subject,
        string message,
        NotificationType type = NotificationType.Email,
        CancellationToken cancellationToken = default);

    Task<bool> SendEmailAsync(
        string emailAddress,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);

    Task<int> SendBulkNotificationAsync(
        IEnumerable<Guid> userIds,
        string subject,
        string message,
        NotificationType type = NotificationType.Email,
        CancellationToken cancellationToken = default);

    Task<bool> SendAlertAsync(
        string alertType,
        string description,
        string? details = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Enum for notification types.
/// </summary>
public enum NotificationType
{
    Email,
    Sms,
    Push,
    Webhook,
    Slack
}
