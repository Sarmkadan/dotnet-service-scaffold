#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Persists a webhook delivery that exhausted all retry attempts (or received a
/// non-retryable response) so operators can inspect the failure history and
/// optionally replay the delivery once the downstream endpoint recovers.
/// </summary>
public class WebhookDeadLetter
{
    /// <summary>
    /// Unique identifier of the dead-lettered delivery.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Correlation identifier shared with the log entries emitted during delivery attempts.
    /// </summary>
    [StringLength(64)]
    public string WebhookId { get; set; } = string.Empty;

    /// <summary>
    /// Destination URL the webhook was sent to.
    /// </summary>
    [StringLength(2048)]
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>
    /// The JSON-serialized payload that was (attempted to be) delivered.
    /// </summary>
    public string PayloadJson { get; set; } = string.Empty;

    /// <summary>
    /// Event type identifier associated with the payload, if any.
    /// </summary>
    [StringLength(255)]
    public string? EventType { get; set; }

    /// <summary>
    /// Total number of delivery attempts made before the delivery was dead-lettered.
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// HTTP status code returned by the last delivery attempt, if a response was received.
    /// </summary>
    public int? LastStatusCode { get; set; }

    /// <summary>
    /// Latency in milliseconds of the last delivery attempt.
    /// </summary>
    public long LastLatencyMs { get; set; }

    /// <summary>
    /// Error message captured from the last failed delivery attempt.
    /// </summary>
    [StringLength(2000)]
    public string? LastErrorMessage { get; set; }

    /// <summary>
    /// JSON-serialized list of per-attempt metadata (status code, latency, error) for full auditability.
    /// </summary>
    public string AttemptHistoryJson { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when this delivery was first dead-lettered.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp of the most recent delivery or replay attempt.
    /// </summary>
    public DateTime LastAttemptAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp the delivery was successfully replayed, if it has been resolved.
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Whether the dead-lettered delivery has since succeeded via replay.
    /// </summary>
    public bool IsResolved { get; set; }

    /// <summary>
    /// Marks the dead letter as resolved after a successful replay.
    /// </summary>
    public void MarkResolved()
    {
        IsResolved = true;
        ResolvedAt = DateTime.UtcNow;
    }
}
