#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotnetServiceScaffold.Shared.Utilities;
using Serilog;

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Client for sending webhook payloads to external endpoints with security features.
/// Implements:
/// - SSRF protection for webhook URLs
/// - HMAC-SHA256 payload signing for authenticity verification
/// - Retry logic, timeout handling, and logging for debugging webhook delivery issues.
/// </summary>
public class WebhookClient : IWebhookClient
{
    private readonly ICustomHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookClient> _logger;
    private const int MaxRetries = 3;
    private const int InitialRetryDelayMs = 1000;
    private const string SignatureHeaderName = "X-Signature";
    private const string SignatureAlgorithm = "HMAC-SHA256";

    public WebhookClient(ICustomHttpClientFactory httpClientFactory, ILogger<WebhookClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Sends a webhook payload to the specified URL with automatic retry on failure.
    /// </summary>
    /// <param name="webhookUrl">The destination URL for the webhook. Must be HTTPS and not localhost/internal.</param>
    /// <param name="payload">The payload object to send.</param>
    /// <param name="eventType">Optional event type identifier.</param>
    /// <param name="webhookSecret">Optional secret for HMAC-SHA256 payload signing. If provided, signature header will be added.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if delivery was successful, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if webhookUrl or payload is null.</exception>
    /// <exception cref="ArgumentException">Thrown if webhookUrl is invalid or blocked by SSRF protection.</exception>
    public async Task<bool> SendWebhookAsync(string webhookUrl, object payload, string? eventType = null, string? webhookSecret = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(webhookUrl);
        ArgumentNullException.ThrowIfNull(payload);

        ValidateWebhookUrl(webhookUrl);

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
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add webhook-specific headers
                content.Headers.Add("X-Webhook-Id", webhookId);
                if (!string.IsNullOrEmpty(eventType))
                    content.Headers.Add("X-Event-Type", eventType);

                // Add HMAC-SHA256 signature if secret provided
                if (!string.IsNullOrEmpty(webhookSecret))
                {
                    var signature = ComputeHmacSignature(json, webhookSecret);
                    content.Headers.Add(SignatureHeaderName, $"{SignatureAlgorithm}={signature}");
                }

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
}