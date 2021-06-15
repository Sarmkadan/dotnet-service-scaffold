#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using DotnetServiceScaffold.Shared.Utilities;
using Serilog;

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Client for sending webhook payloads to external endpoints. Implements retry logic,
/// timeout handling, and logging for debugging webhook delivery issues.
/// </summary>
public class WebhookClient : IWebhookClient
{
    private readonly ICustomHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookClient> _logger;
    private const int MaxRetries = 3;
    private const int InitialRetryDelayMs = 1000;

    public WebhookClient(ICustomHttpClientFactory httpClientFactory, ILogger<WebhookClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Sends a webhook payload to the specified URL with automatic retry on failure.
    /// Returns true if delivery was successful, false otherwise.
    /// </summary>
    public async Task<bool> SendWebhookAsync(string webhookUrl, object payload, string? eventType = null, CancellationToken cancellationToken = default)
    {
        ValidationUtility.ValidateNotNullOrEmpty(webhookUrl, nameof(webhookUrl));
        if (!ValidationUtility.IsValidUrl(webhookUrl))
            throw new ArgumentException("Invalid webhook URL format", nameof(webhookUrl));

        var webhookId = Guid.NewGuid().ToString();
        _logger.LogInformation(
            "Sending webhook {WebhookId} to {Url} for event type {EventType}",
            webhookId, HttpUtility.MaskSensitiveUrl(webhookUrl), eventType ?? "unknown");

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("webhook");
                client.Timeout = TimeSpan.FromSeconds(10);

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // Add webhook-specific headers
                content.Headers.Add("X-Webhook-Id", webhookId);
                if (!string.IsNullOrEmpty(eventType))
                    content.Headers.Add("X-Event-Type", eventType);

                var response = await client.PostAsync(webhookUrl, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Webhook {WebhookId} delivered successfully with status {StatusCode}",
                        webhookId, response.StatusCode);
                    return true;
                }

                if (!HttpUtility.IsRetryableStatusCode((int)response.StatusCode))
                {
                    _logger.LogWarning(
                        "Webhook {WebhookId} failed with non-retryable status {StatusCode}",
                        webhookId, response.StatusCode);
                    return false;
                }

                _logger.LogWarning(
                    "Webhook {WebhookId} failed with status {StatusCode}, will retry (attempt {Attempt}/{MaxRetries})",
                    webhookId, response.StatusCode, attempt + 1, MaxRetries);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex,
                    "Webhook {WebhookId} HTTP error on attempt {Attempt}/{MaxRetries}",
                    webhookId, attempt + 1, MaxRetries);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Webhook {WebhookId} was cancelled", webhookId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Webhook {WebhookId} unexpected error on attempt {Attempt}/{MaxRetries}",
                    webhookId, attempt + 1, MaxRetries);
            }

            // Wait before retry with exponential backoff
            if (attempt < MaxRetries - 1)
            {
                var delayMs = InitialRetryDelayMs * (int)Math.Pow(2, attempt);
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        _logger.LogError("Webhook {WebhookId} failed after {MaxRetries} attempts", webhookId, MaxRetries);
        return false;
    }
}

/// <summary>
/// Interface for webhook client.
/// </summary>
public interface IWebhookClient
{
    Task<bool> SendWebhookAsync(string webhookUrl, object payload, string? eventType = null, CancellationToken cancellationToken = default);
}
