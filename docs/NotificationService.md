# NotificationService

The `NotificationService` class provides an abstraction for sending various types of notifications—such as generic notifications, emails, alerts, and bulk messages—within an application. It encapsulates the underlying delivery logic (e.g., SMTP, push, or in‑memory transport) and exposes asynchronous methods that return success indicators or counts. This service is intended to be registered as a transient or scoped dependency and used by higher‑level components like controllers, background jobs, or domain services.

## API

### `NotificationService()`

Initializes a new instance of the `NotificationService`. The constructor may accept optional dependencies (e.g., an `IEmailSender`, `ILogger<NotificationService>`) depending on the hosting environment; the default constructor is provided for scenarios where dependencies are resolved externally.

### `Task<bool> SendNotificationAsync(string recipient, string subject, string body)`

Sends a generic notification to the specified recipient.

- **Parameters**  
  - `recipient` – The target address or identifier (e.g., email, device token). Must not be null or empty.  
  - `subject` – The subject line of the notification. Must not be null or empty.  
  - `body` – The message body. Must not be null or empty.

- **Returns**  
  `true` if the notification was successfully dispatched; otherwise `false`.

- **Throws**  
  - `ArgumentNullException` if any parameter is `null`.  
  - `ArgumentException` if any parameter is an empty string or whitespace.  
  - `InvalidOperationException` if the underlying transport is not configured.

### `Task<bool> SendEmailAsync(string to, string subject, string body)`

Sends an email message to the specified address.

- **Parameters**  
  - `to` – The recipient email address. Must not be null or empty.  
  - `subject` – The email subject. Must not be null or empty.  
  - `body` – The email body (plain text or HTML, depending on configuration). Must not be null or empty.

- **Returns**  
  `true` if the email was sent successfully; otherwise `false`.

- **Throws**  
  - `ArgumentNullException` if any parameter is `null`.  
  - `ArgumentException` if any parameter is an empty string or whitespace, or if `to` is not a valid email format.  
  - `InvalidOperationException` if the email sender is not configured.

### `Task<int> SendBulkNotificationAsync(IEnumerable<string> recipients, string subject, string body)`

Sends the same notification to multiple recipients.

- **Parameters**  
  - `recipients` – A collection of recipient addresses or identifiers. Must not be null or empty.  
  - `subject` – The subject line. Must not be null or empty.  
  - `body` – The message body. Must not be null or empty.

- **Returns**  
  The number of recipients for which the notification was successfully sent. A return value of zero indicates that no messages were delivered.

- **Throws**  
  - `ArgumentNullException` if `recipients` is `null`, or if `subject` or `body` is `null`.  
  - `ArgumentException` if `recipients` is empty, or if `subject` or `body` is empty or whitespace.  
  - `InvalidOperationException` if the underlying transport is not configured.

### `Task<bool> SendAlertAsync(string recipient, string alertType, string message)`

Sends an alert (e.g., a high‑priority push notification or system alert) to the specified recipient.

- **Parameters**  
  - `recipient` – The target address or identifier. Must not be null or empty.  
  - `alertType` – A string identifying the alert category (e.g., "critical", "warning"). Must not be null or empty.  
  - `message` – The alert content. Must not be null or empty.

- **Returns**  
  `true` if the alert was dispatched successfully; otherwise `false`.

- **Throws**  
  - `ArgumentNullException` if any parameter is `null`.  
  - `ArgumentException` if any parameter is an empty string or whitespace.  
  - `InvalidOperationException` if the alert channel is not configured.

## Usage

### Example 1: Sending a single email notification

```csharp
public class UserRegistrationHandler
{
    private readonly NotificationService _notificationService;

    public UserRegistrationHandler(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task HandleRegistrationAsync(string email, string userName)
    {
        bool sent = await _notificationService.SendEmailAsync(
            email,
            "Welcome to the platform",
            $"Hello {userName}, thank you for registering.");

        if (!sent)
        {
            // Log failure, queue for retry, etc.
        }
    }
}
```

### Example 2: Sending a bulk alert to multiple recipients

```csharp
public class SystemMonitor
{
    private readonly NotificationService _notificationService;

    public SystemMonitor(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task NotifyCriticalIssueAsync(IEnumerable<string> adminEmails, string issueDescription)
    {
        int successCount = await _notificationService.SendBulkNotificationAsync(
            adminEmails,
            "Critical system alert",
            issueDescription);

        if (successCount < adminEmails.Count())
        {
            // Some deliveries failed – implement fallback logic.
        }
    }
}
```

## Notes

- **Thread safety** – `NotificationService` is not guaranteed to be thread‑safe. If the same instance is used concurrently from multiple threads, external synchronization (e.g., a lock or a dedicated channel) should be applied. For most scenarios, registering the service as scoped (per‑request) or transient avoids concurrency issues.
- **Null and empty parameters** – All methods throw `ArgumentNullException` or `ArgumentException` for null, empty, or whitespace parameters. Always validate inputs before calling these methods.
- **Bulk delivery** – `SendBulkNotificationAsync` processes recipients sequentially or in batches depending on the underlying implementation. For very large collections, consider splitting the work into smaller chunks to avoid timeouts or resource exhaustion.
- **Return values** – A `false` return or a count lower than expected does not necessarily indicate a permanent failure; transient errors (e.g., network timeouts) may cause partial delivery. Implement retry logic with exponential backoff for critical notifications.
- **Configuration** – The service relies on external configuration (e.g., SMTP server, API keys). If the required configuration is missing, `InvalidOperationException` is thrown. Ensure configuration is validated at application startup.
- **Disposal** – `NotificationService` does not implement `IDisposable`. Any disposable dependencies (e.g., HTTP clients) should be managed by the dependency injection container or by the caller.
