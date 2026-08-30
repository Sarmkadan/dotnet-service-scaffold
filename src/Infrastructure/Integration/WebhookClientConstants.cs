#nullable enable

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Constants for the WebhookClient class.
/// </summary>
internal static class WebhookClientConstants
{
    /// <summary>Maximum number of retry attempts for webhook delivery.</summary>
    public const int MaxRetries = 3;

    /// <summary>Initial delay in milliseconds between retry attempts.</summary>
    public const int InitialRetryDelayMs = 1000;

    /// <summary>HTTP header name for the HMAC-SHA256 signature.</summary>
    public const string SignatureHeaderName = "X-Signature";

    /// <summary>Algorithm used for the HMAC signature.</summary>
    public const string SignatureAlgorithm = "HMAC-SHA256";

    /// <summary>HTTP header name for the webhook identifier.</summary>
    public const string WebhookIdHeader = "X-Webhook-Id";

    /// <summary>HTTP header name for the event type.</summary>
    public const string EventTypeHeader = "X-Event-Type";

    /// <summary>Content type for JSON payloads.</summary>
    public const string JsonContentType = "application/json";

    /// <summary>Error message when a request is cancelled.</summary>
    public const string CancelledErrorMessage = "cancelled";

    /// <summary>Error message when a request times out.</summary>
    public const string TimedOutErrorMessage = "request timed out";

    /// <summary>Metric name for webhook delivery latency.</summary>
    public const string WebhookDeliveryLatencyMetric = "webhook.delivery.latency_ms";

    /// <summary>Metric name for webhook delivery attempts.</summary>
    public const string WebhookDeliveryAttemptsMetric = "webhook.delivery.attempts";

    /// <summary>Metric name for webhook dead-lettered deliveries.</summary>
    public const string WebhookDeliveryDeadLetteredMetric = "webhook.delivery.dead_lettered";

    /// <summary>Metric tag key for attempt number.</summary>
    public const string MetricTagAttempt = "attempt";

    /// <summary>Metric tag key for outcome (success/failure).</summary>
    public const string MetricTagOutcome = "outcome";

    /// <summary>Metric tag key for status code.</summary>
    public const string MetricTagStatusCode = "status_code";

    /// <summary>Default value for unknown event type in logging.</summary>
    public const string UnknownEventType = "unknown";

    /// <summary>Default value for unknown error message in logging.</summary>
    public const string UnknownErrorMessage = "unknown error";
}