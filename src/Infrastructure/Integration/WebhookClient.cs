#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data;
using DotnetServiceScaffold.Infrastructure.Metrics;
using DotnetServiceScaffold.Infrastructure.Integration;
using DotnetServiceScaffold.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Metadata captured for a single webhook delivery attempt, used both for the
/// per-request metrics surfaced through <see cref="IMetricsService"/> and for
/// the attempt history persisted on dead-lettered deliveries.
/// </summary>
/// <param name="AttemptNumber">1-based index of the attempt.</param>
/// <param name="StatusCode">HTTP status code returned, or null if no response was received.</param>
/// <param name="LatencyMs">Round-trip latency of the attempt, in milliseconds.</param>
/// <param name="ErrorMessage">Error message captured for the attempt, if any.</param>
/// <param name="AttemptedAt">Timestamp the attempt was made.</param>
public sealed record WebhookAttemptRecord(int AttemptNumber, int? StatusCode, long LatencyMs, string? ErrorMessage, DateTime AttemptedAt);

/// <summary>
/// Describes the outcome of a webhook delivery attempt sequence, giving callers a clear
/// result object (status, attempts, error) instead of forcing them to infer failures from
/// a boolean or catch exceptions.
/// </summary>
/// <param name="Delivered">True if the endpoint accepted the payload.</param>
/// <param name="StatusCode">HTTP status code of the final attempt, or null if no response was received.</param>
/// <param name="AttemptCount">Total number of attempts made.</param>
/// <param name="ErrorMessage">Error message from the final attempt, if any.</param>
/// <param name="Attempts">History of every attempt made during delivery.</param>
/// <param name="Cancelled">True when delivery stopped because the caller's cancellation token fired.</param>
public sealed record WebhookDeliveryResult(
    bool Delivered,
    int? StatusCode,
    int AttemptCount,
    string? ErrorMessage,
    IReadOnlyList<WebhookAttemptRecord> Attempts,
    bool Cancelled)
{
    /// <summary>Creates a result for a successfully delivered webhook.</summary>
    public static WebhookDeliveryResult Success(int statusCode, IReadOnlyList<WebhookAttemptRecord> attempts) =>
        new(true, statusCode, attempts.Count, null, attempts, Cancelled: false);

    /// <summary>Creates a result for a webhook that was not delivered.</summary>
    public static WebhookDeliveryResult Failure(int? statusCode, string? errorMessage, IReadOnlyList<WebhookAttemptRecord> attempts, bool cancelled = false) =>
        new(false, statusCode, attempts.Count, errorMessage, attempts, cancelled);
}

/// <summary>
/// Client for sending webhook payloads to external endpoints with security features.
/// Implements:
/// - SSRF protection for webhook URLs
/// - HMAC-SHA256 payload signing for authenticity verification
/// - Retry logic, timeout handling, and logging for debugging webhook delivery issues.
/// </summary>
/// <remarks>
/// Failed deliveries are recorded as dead letters so that they can be inspected and replayed.
/// A single instance uses the injected <see cref="HttpClient"/> for every delivery.
/// </remarks>
public class WebhookClient : IWebhookClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookClient> _logger;
    private readonly ServiceScaffoldDbContext _dbContext;
    private readonly IMetricsService _metricsService;
    // Constants are now in WebhookClientConstants

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to deliver webhook requests.</param>
    /// <param name="logger">Logger used for delivery diagnostics.</param>
    /// <param name="dbContext">Database context used to persist dead-lettered deliveries.</param>
    /// <param name="metricsService">Metrics sink used to surface delivery-attempt metadata.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="httpClient"/>, <paramref name="logger"/>,
    /// <paramref name="dbContext"/>, or <paramref name="metricsService"/> is <see langword="null"/>.
    /// </exception>
    public WebhookClient(HttpClient httpClient, ILogger<WebhookClient> logger, ServiceScaffoldDbContext dbContext, IMetricsService metricsService)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(metricsService);

        _httpClient = httpClient;
        _logger = logger;
        _dbContext = dbContext;
        _metricsService = metricsService;
    }

    /// <summary>
    /// Sends a webhook payload to the specified URL with automatic retry on failure.
    /// </summary>
    /// <param name="webhookUrl">The HTTP or HTTPS destination URL. Local, private, link-local, and metadata-service addresses are rejected.</param>
    /// <param name="payload">The object to serialize as JSON and send in the request body.</param>
    /// <param name="eventType">An optional event type to include in the webhook headers and metrics.</param>
    /// <param name="webhookSecret">An optional secret used to add an HMAC-SHA256 signature header.</param>
    /// <param name="cancellationToken">A token that can cancel the delivery and retry delay.</param>
    /// <returns><see langword="true"/> when the endpoint returns a successful HTTP status code; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="webhookUrl"/> or <paramref name="payload"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="webhookUrl"/> is empty, malformed, uses an unsupported scheme, or targets a blocked address.</exception>
    public async Task<bool> SendWebhookAsync(string webhookUrl, object payload, string? eventType = null, string? webhookSecret = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(webhookUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(webhookUrl);
        ArgumentNullException.ThrowIfNull(payload);

        var result = await DeliverAsync(webhookUrl, payload, eventType, webhookSecret, cancellationToken);
        return result.Delivered;
    }

    /// <summary>
    /// Sends a webhook payload to the specified URL with automatic retry on failure and returns
    /// a detailed delivery result describing every attempt instead of reducing the outcome to a boolean.
    /// </summary>
    /// <param name="webhookUrl">The HTTP or HTTPS destination URL. Local, private, link-local, and metadata-service addresses are rejected.</param>
    /// <param name="payload">The object to serialize as JSON and send in the request body.</param>
    /// <param name="eventType">An optional event type to include in the webhook headers and metrics.</param>
    /// <param name="webhookSecret">An optional secret used to add an HMAC-SHA256 signature header.</param>
    /// <param name="cancellationToken">A token that can cancel the delivery and retry delay.</param>
    /// <returns>A result containing the final status, error details, cancellation state, and history of all delivery attempts.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="webhookUrl"/> or <paramref name="payload"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="webhookUrl"/> is empty, malformed, uses an unsupported scheme, or targets a blocked address.</exception>
    public async Task<WebhookDeliveryResult> DeliverAsync(string webhookUrl, object payload, string? eventType = null, string? webhookSecret = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(webhookUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(webhookUrl);
        ArgumentNullException.ThrowIfNull(payload);

        ValidateWebhookUrl(webhookUrl);

        var webhookId = Guid.NewGuid().ToString();
        var json = JsonSerializer.Serialize(payload);
        var result = await SendAttemptsAsync(webhookId, webhookUrl, json, eventType, webhookSecret, firstAttemptNumber: 1, cancellationToken);

        if (!result.Delivered && !result.Cancelled)
        {
            await PersistDeadLetterAsync(webhookId, webhookUrl, json, eventType, result, cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Performs up to <see cref="WebhookClientConstants.MaxRetries"/> POST attempts with exponential backoff
    /// and records latency, status code, and outcome metrics for each attempt. Does not persist dead letters;
    /// callers decide how a failed sequence is recorded.
    /// </summary>
    /// <param name="webhookId">Correlation identifier sent in the <c>X-Webhook-Id</c> header and logs.</param>
    /// <param name="webhookUrl">The destination URL, already validated.</param>
    /// <param name="json">The serialized JSON request body, sent unchanged on every attempt.</param>
    /// <param name="eventType">An optional event type to include in the webhook headers and metrics.</param>
    /// <param name="webhookSecret">An optional secret used to add an HMAC-SHA256 signature header.</param>
    /// <param name="firstAttemptNumber">The 1-based number of the first attempt in this sequence, so replays continue the original numbering.</param>
    /// <param name="cancellationToken">A token that can cancel the delivery and retry delay.</param>
    /// <returns>A result describing the outcome and every attempt made in this sequence.</returns>
    private async Task<WebhookDeliveryResult> SendAttemptsAsync(string webhookId, string webhookUrl, string json, string? eventType, string? webhookSecret, int firstAttemptNumber, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Sending webhook {WebhookId} to {Url} for event type {EventType}",
            webhookId, HttpUtility.MaskSensitiveUrl(webhookUrl), eventType ?? WebhookClientConstants.UnknownEventType);

        var attempts = new List<WebhookAttemptRecord>(WebhookClientConstants.MaxRetries);
        var metricTags = new Dictionary<string, string> { ["event_type"] = eventType ?? WebhookClientConstants.UnknownEventType };

        for (int attempt = 0; attempt < WebhookClientConstants.MaxRetries; attempt++)
        {
            var attemptNumber = firstAttemptNumber + attempt;
            var stopwatch = Stopwatch.StartNew();
            int? statusCode = null;
            string? errorMessage = null;
            var retryable = true;
            HttpResponseMessage? response = null;

            try
            {
                var content = new StringContent(json, Encoding.UTF8, WebhookClientConstants.JsonContentType);

                // Add webhook-specific headers
                content.Headers.Add(WebhookClientConstants.WebhookIdHeader, webhookId);
                if (!string.IsNullOrEmpty(eventType))
                    content.Headers.Add(WebhookClientConstants.EventTypeHeader, eventType);

                // Add HMAC-SHA256 signature if secret provided
                if (!string.IsNullOrEmpty(webhookSecret))
                {
                    var signature = ComputeHmacSignature(json, webhookSecret);
                    content.Headers.Add(WebhookClientConstants.SignatureHeaderName, $"{WebhookClientConstants.SignatureAlgorithm}={signature}");
                }

                // _httpClient already has timeout configured
                response = await _httpClient.PostAsync(webhookUrl, content, cancellationToken);
                stopwatch.Stop();
                statusCode = (int)response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    attempts.Add(new WebhookAttemptRecord(attemptNumber, statusCode, stopwatch.ElapsedMilliseconds, null, DateTime.UtcNow));
                    RecordAttemptMetrics(attemptNumber, statusCode, stopwatch.ElapsedMilliseconds, success: true, metricTags);
                    _logger.LogInformation(
                        "Webhook {WebhookId} delivered successfully with status {StatusCode}",
                        webhookId, response.StatusCode);
                    return WebhookDeliveryResult.Success(statusCode.Value, attempts);
                }

                retryable = HttpUtility.IsRetryableStatusCode(statusCode.Value);
                errorMessage = $"Non-success status code {statusCode}";

                if (!retryable)
                {
                    _logger.LogWarning(
                        "Webhook {WebhookId} failed with non-retryable status {StatusCode}",
                        webhookId, response.StatusCode);
                }
                else
                {
                    _logger.LogWarning(
                        "Webhook {WebhookId} failed with status {StatusCode}, will retry (attempt {Attempt}/{MaxRetries})",
                        webhookId, response.StatusCode, attemptNumber, WebhookClientConstants.MaxRetries);
                }
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                errorMessage = ex.Message;
                _logger.LogWarning(ex,
                    "Webhook {WebhookId} HTTP error on attempt {Attempt}/{MaxRetries}",
                    webhookId, attemptNumber, WebhookClientConstants.MaxRetries);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Caller requested cancellation - do not retry or dead-letter.
                stopwatch.Stop();
                attempts.Add(new WebhookAttemptRecord(attemptNumber, statusCode, stopwatch.ElapsedMilliseconds, WebhookClientConstants.CancelledErrorMessage, DateTime.UtcNow));
                RecordAttemptMetrics(attemptNumber, statusCode, stopwatch.ElapsedMilliseconds, success: false, metricTags);
                _logger.LogWarning("Webhook {WebhookId} was cancelled", webhookId);
                return WebhookDeliveryResult.Failure(statusCode, "cancelled", attempts, cancelled: true);
            }
            catch (OperationCanceledException ex)
            {
                // HttpClient timeouts surface as TaskCanceledException while the caller's token is still live.
                stopwatch.Stop();
                errorMessage = "request timed out";
                _logger.LogWarning(ex,
                    "Webhook {WebhookId} timed out on attempt {Attempt}/{MaxRetries}",
                    webhookId, attemptNumber, WebhookClientConstants.MaxRetries);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                errorMessage = ex.Message;
                _logger.LogError(ex,
                    "Webhook {WebhookId} unexpected error on attempt {Attempt}/{MaxRetries}",
                    webhookId, attemptNumber, WebhookClientConstants.MaxRetries);

                // Unexpected exceptions are not retried: log and return a failure result
                // instead of silently swallowing the error and masking the underlying fault.
                attempts.Add(new WebhookAttemptRecord(attemptNumber, statusCode, stopwatch.ElapsedMilliseconds, errorMessage, DateTime.UtcNow));
                RecordAttemptMetrics(attemptNumber, statusCode, stopwatch.ElapsedMilliseconds, success: false, metricTags);
                return WebhookDeliveryResult.Failure(statusCode, errorMessage, attempts);
            }
            finally
            {
                // Ensure the response is disposed on every path (success, failure, exception).
                response?.Dispose();
            }

            attempts.Add(new WebhookAttemptRecord(attemptNumber, statusCode, stopwatch.ElapsedMilliseconds, errorMessage, DateTime.UtcNow));
            RecordAttemptMetrics(attemptNumber, statusCode, stopwatch.ElapsedMilliseconds, success: false, metricTags);

            if (!retryable)
            {
                return WebhookDeliveryResult.Failure(statusCode, errorMessage, attempts);
            }

            // Wait before retry with exponential backoff
            if (attempt < WebhookClientConstants.MaxRetries - 1)
            {
                var delayMs = WebhookClientConstants.InitialRetryDelayMs * (int)Math.Pow(2, attempt);
                try
                {
                    await Task.Delay(delayMs, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Webhook {WebhookId} was cancelled during retry backoff", webhookId);
                    return WebhookDeliveryResult.Failure(statusCode, WebhookClientConstants.CancelledErrorMessage, attempts, cancelled: true);
                }
            }
        }

        _logger.LogError("Webhook {WebhookId} failed after {MaxRetries} attempts", webhookId, WebhookClientConstants.MaxRetries);
        var lastAttempt = attempts[^1];
        return WebhookDeliveryResult.Failure(lastAttempt.StatusCode, lastAttempt.ErrorMessage, attempts);
    }

    /// <summary>
    /// Replays a previously dead-lettered webhook delivery. The original payload, event type, and
    /// <c>X-Webhook-Id</c> are reused so receivers can de-duplicate replays. On success the dead letter
    /// is marked resolved; on failure the existing record's attempt history and last-failure metadata are updated.
    /// </summary>
    /// <param name="deadLetterId">The identifier of the dead-lettered delivery to replay.</param>
    /// <param name="webhookSecret">An optional secret used to sign the replayed payload with HMAC-SHA256.</param>
    /// <param name="cancellationToken">A token that can cancel the lookup, delivery, or persistence operation.</param>
    /// <returns><see langword="true"/> when the replay succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when no dead-lettered delivery has the specified <paramref name="deadLetterId"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the dead letter has already been resolved, so replaying it would send a duplicate.</exception>
    public async Task<bool> ReplayDeadLetterAsync(Guid deadLetterId, string? webhookSecret = null, CancellationToken cancellationToken = default)
    {
        var deadLetter = await _dbContext.WebhookDeadLetters.FindAsync(new object?[] { deadLetterId }, cancellationToken)
            ?? throw new ArgumentException($"No dead-lettered webhook found with id {deadLetterId}", nameof(deadLetterId));

        if (deadLetter.IsResolved)
        {
            throw new InvalidOperationException($"Dead-lettered webhook {deadLetterId} is already resolved and will not be replayed.");
        }

        ValidateWebhookUrl(deadLetter.WebhookUrl);

        var result = await SendAttemptsAsync(
            deadLetter.WebhookId,
            deadLetter.WebhookUrl,
            deadLetter.PayloadJson,
            deadLetter.EventType,
            webhookSecret,
            firstAttemptNumber: deadLetter.AttemptCount + 1,
            cancellationToken);

        deadLetter.LastAttemptAt = DateTime.UtcNow;

        if (result.Delivered)
        {
            deadLetter.MarkResolved();
        }
        else
        {
            var history = JsonSerializer.Deserialize<List<WebhookAttemptRecord>>(deadLetter.AttemptHistoryJson) ?? [];
            history.AddRange(result.Attempts);
            var lastAttempt = result.Attempts[^1];

            deadLetter.AttemptCount = history.Count;
            deadLetter.LastStatusCode = lastAttempt.StatusCode;
            deadLetter.LastLatencyMs = lastAttempt.LatencyMs;
            deadLetter.LastErrorMessage = lastAttempt.ErrorMessage;
            deadLetter.AttemptHistoryJson = JsonSerializer.Serialize(history);

            _logger.LogWarning(
                "Replay of dead letter {DeadLetterId} failed: {ErrorMessage}",
                deadLetterId, result.ErrorMessage ?? WebhookClientConstants.UnknownErrorMessage);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return result.Delivered;
    }

    /// <summary>
    /// Lists dead-lettered deliveries, newest first, so operators can inspect failures before replaying them.
    /// </summary>
    /// <param name="includeResolved">When <see langword="true"/>, deliveries that were later replayed successfully are included.</param>
    /// <param name="limit">The maximum number of dead letters to return.</param>
    /// <param name="cancellationToken">A token that can cancel the query.</param>
    /// <returns>The matching dead letters, ordered by creation time descending.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="limit"/> is zero or negative.</exception>
    public async Task<IReadOnlyList<WebhookDeadLetter>> GetDeadLettersAsync(bool includeResolved = false, int limit = WebhookClientConstants.DefaultDeadLetterPageSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        var query = _dbContext.WebhookDeadLetters.AsNoTracking();
        if (!includeResolved)
        {
            query = query.Where(d => !d.IsResolved);
        }

        return await query
            .OrderByDescending(d => d.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Records latency, status code, and outcome metrics for a single delivery attempt.
    /// </summary>
    private void RecordAttemptMetrics(int attemptNumber, int? statusCode, long latencyMs, bool success, IDictionary<string, string> tags)
    {
        var attemptTags = new Dictionary<string, string>(tags)
        {
            [WebhookClientConstants.MetricTagAttempt] = attemptNumber.ToString(),
            [WebhookClientConstants.MetricTagOutcome] = success ? "success" : "failure",
            [WebhookClientConstants.MetricTagStatusCode] = statusCode?.ToString() ?? "none",
        };

        _metricsService.RecordTiming(WebhookClientConstants.WebhookDeliveryLatencyMetric, latencyMs, attemptTags);
        _metricsService.IncrementCounter(WebhookClientConstants.WebhookDeliveryAttemptsMetric, tags: attemptTags);
    }

    /// <summary>
    /// Persists a permanently-failed delivery to the dead-letter table for operator inspection and replay.
    /// </summary>
    private async Task PersistDeadLetterAsync(string webhookId, string webhookUrl, string payloadJson, string? eventType, WebhookDeliveryResult result, CancellationToken cancellationToken)
    {
        var lastAttempt = result.Attempts[^1];
        var deadLetter = new WebhookDeadLetter
        {
            Id = Guid.NewGuid(),
            WebhookId = webhookId,
            WebhookUrl = webhookUrl,
            PayloadJson = payloadJson,
            EventType = eventType,
            AttemptCount = result.AttemptCount,
            LastStatusCode = lastAttempt.StatusCode,
            LastLatencyMs = lastAttempt.LatencyMs,
            LastErrorMessage = lastAttempt.ErrorMessage,
            AttemptHistoryJson = JsonSerializer.Serialize(result.Attempts),
        };

        _dbContext.WebhookDeadLetters.Add(deadLetter);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            _metricsService.IncrementCounter(
                WebhookClientConstants.WebhookDeliveryDeadLetteredMetric,
                tags: new Dictionary<string, string> { ["event_type"] = eventType ?? WebhookClientConstants.UnknownEventType });
            _logger.LogWarning(
                "Webhook {WebhookId} dead-lettered as {DeadLetterId} after {AttemptCount} attempts",
                webhookId, deadLetter.Id, deadLetter.AttemptCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist dead letter for webhook {WebhookId}", webhookId);
        }
    }

    /// <summary>
    /// Validates a webhook URL to prevent SSRF attacks.
    /// </summary>
    /// <param name="webhookUrl">The URL to validate.</param>
    /// <exception cref="ArgumentException">Thrown if URL is blocked by SSRF protection.</exception>
    private static void ValidateWebhookUrl(string webhookUrl)
    {
        try
        {
            var uri = new Uri(webhookUrl);

            // SSRF Protection: Only allow http/https schemes
            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                throw new ArgumentException(
                    $"Webhook URL scheme '{uri.Scheme}' is not allowed. Only http and https are permitted.",
                    nameof(webhookUrl));
            }

            // SSRF Protection: Block localhost addresses
            if (IsLocalhostAddress(uri.Host))
            {
                throw new ArgumentException(
                    "Webhook URL points to localhost which is not allowed for security reasons.",
                    nameof(webhookUrl));
            }

            // SSRF Protection: Block link-local addresses (RFC 3927)
            if (IsLinkLocalAddress(uri.Host))
            {
                throw new ArgumentException(
                    "Webhook URL points to link-local address which is not allowed.",
                    nameof(webhookUrl));
            }

            // SSRF Protection: Block private IP ranges (RFC 1918)
            if (IsPrivateIpAddress(uri.Host))
            {
                throw new ArgumentException(
                    "Webhook URL points to private IP address which is not allowed.",
                    nameof(webhookUrl));
            }

            // SSRF Protection: Block common metadata service endpoints
            if (IsMetadataServiceEndpoint(uri))
            {
                throw new ArgumentException(
                    "Webhook URL points to metadata service endpoint which is not allowed.",
                    nameof(webhookUrl));
            }

            // SSRF Protection: Block IPv6 loopback
            if (uri.Host.Equals("::1", StringComparison.OrdinalIgnoreCase) ||
                uri.Host.Equals("0:0:0:0:0:0:0:1", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Webhook URL points to IPv6 loopback address which is not allowed.",
                    nameof(webhookUrl));
            }

            // SSRF Protection: Block 0.0.0.0
            if (uri.Host.Equals("0.0.0.0", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Webhook URL points to 0.0.0.0 which is not allowed.",
                    nameof(webhookUrl));
            }
        }
        catch (UriFormatException ex)
        {
            throw new ArgumentException("Invalid webhook URL format", nameof(webhookUrl), ex);
        }
    }

    /// <summary>
    /// Computes HMAC-SHA256 signature for the payload.
    /// </summary>
    /// <param name="payloadJson">The JSON payload to sign.</param>
    /// <param name="secret">The secret key for HMAC signing.</param>
    /// <returns>Base64-encoded HMAC-SHA256 signature.</returns>
    private static string ComputeHmacSignature(string payloadJson, string secret)
    {
        ArgumentException.ThrowIfNullOrEmpty(secret);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadJson));
        return Convert.ToBase64String(signatureBytes);
    }

    /// <summary>
    /// Checks if a hostname represents localhost.
    /// </summary>
    private static bool IsLocalhostAddress(string host)
    {
        var normalizedHost = host.Trim().ToLowerInvariant();
        return normalizedHost == "localhost" ||
               normalizedHost == "127.0.0.1" ||
               normalizedHost == "::1" ||
               normalizedHost.StartsWith("127.") ||
               normalizedHost.StartsWith("0.");
    }

    /// <summary>
    /// Checks if a hostname is a link-local address (RFC 3927).
    /// </summary>
    private static bool IsLinkLocalAddress(string host)
    {
        // Link-local IPv4: 169.254.0.0/16
        // Link-local IPv6: fe80::/10

        if (IPAddress.TryParse(host, out var ipAddress))
        {
            // IPv4 link-local
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                var bytes = ipAddress.GetAddressBytes();
                return bytes[0] == 169 && bytes[1] == 254;
            }

            // IPv6 link-local
            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var bytes = ipAddress.GetAddressBytes();
                return bytes[0] == 0xfe && (bytes[1] & 0xc0) == 0x80; // fe80::/10
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a hostname is a private IP address (RFC 1918).
    /// </summary>
    private static bool IsPrivateIpAddress(string host)
    {
        if (IPAddress.TryParse(host, out var ipAddress))
        {
            var bytes = ipAddress.GetAddressBytes();

            // IPv4 private ranges:
            // 10.0.0.0/8
            // 172.16.0.0/12
            // 192.168.0.0/16
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                if (bytes[0] == 10) return true; // 10.0.0.0/8
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true; // 172.16.0.0/12
                if (bytes[0] == 192 && bytes[1] == 168) return true; // 192.168.0.0/16
                if (bytes[0] == 127) return true; // loopback
                if (bytes[0] == 169 && bytes[1] == 254) return true; // link-local
            }

            // IPv6 unique local addresses: fc00::/7
            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return (bytes[0] & 0xfe) == 0xfc; // fc00::/7
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a URI points to a common metadata service endpoint.
    /// </summary>
    private static bool IsMetadataServiceEndpoint(Uri uri)
    {
        // Common metadata service endpoints to block
        var blockedHosts = new[] {
            "169.254.169.254", // AWS, Azure, GCP metadata service
            "10.0.0.1",        // Some cloud metadata services
            "10.128.0.1",      // GCP metadata service
            "169.254.123.1",   // Azure internal load balancer
            "fd00:ec2::254",  // IPv6 metadata service
        };

        var normalizedHost = uri.Host.Trim().ToLowerInvariant();
        return blockedHosts.Contains(normalizedHost) ||
               normalizedHost.EndsWith(".metadata.google.internal") ||
               normalizedHost.EndsWith(".internal");
    }
}

/// <summary>
/// Interface for webhook client.
/// </summary>
public interface IWebhookClient
{
    /// <summary>
    /// Sends a webhook payload to the specified URL with automatic retry on failure.
    /// </summary>
    /// <param name="webhookUrl">The destination URL for the webhook.</param>
    /// <param name="payload">The payload object to send.</param>
    /// <param name="eventType">Optional event type identifier.</param>
    /// <param name="webhookSecret">Optional secret for HMAC-SHA256 payload signing.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if delivery was successful, false otherwise.</returns>
    Task<bool> SendWebhookAsync(string webhookUrl, object payload, string? eventType = null, string? webhookSecret = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replays a previously dead-lettered webhook delivery.
    /// </summary>
    /// <param name="deadLetterId">Identifier of the dead-lettered delivery to replay.</param>
    /// <param name="webhookSecret">Optional secret used to re-sign the payload for this replay.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the replayed delivery succeeded, false otherwise.</returns>
    Task<bool> ReplayDeadLetterAsync(Guid deadLetterId, string? webhookSecret = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists dead-lettered webhook deliveries, newest first, for operator inspection.
    /// </summary>
    /// <param name="includeResolved">Whether deliveries already replayed successfully are included.</param>
    /// <param name="limit">The maximum number of dead letters to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching dead letters, ordered by creation time descending.</returns>
    Task<IReadOnlyList<WebhookDeadLetter>> GetDeadLettersAsync(bool includeResolved = false, int limit = WebhookClientConstants.DefaultDeadLetterPageSize, CancellationToken cancellationToken = default);
}
