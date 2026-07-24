#nullable enable

using System.ComponentModel.DataAnnotations;

namespace DotnetServiceScaffold.Infrastructure.Http;

/// <summary>
/// Options controlling the retry, circuit-breaker, and per-attempt timeout policy
/// applied to outbound HTTP calls made by <see cref="ExternalApiClient"/>-style clients.
/// </summary>
public class ResilienceOptions
{
    /// <summary>
    /// The maximum number of retry attempts performed after the initial request fails
    /// with a transient error.
    /// </summary>
    [Range(0, 10)]
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// The base delay used for the exponential backoff calculation, in milliseconds.
    /// The actual delay for attempt <c>n</c> is <c>BaseDelayMilliseconds * 2^(n-1)</c>,
    /// capped at <see cref="MaxDelayMilliseconds"/> and randomized with jitter.
    /// </summary>
    [Range(1, 60_000)]
    public int BaseDelayMilliseconds { get; set; } = 200;

    /// <summary>
    /// The maximum delay between retry attempts, in milliseconds, regardless of the
    /// exponential backoff calculation.
    /// </summary>
    [Range(1, 300_000)]
    public int MaxDelayMilliseconds { get; set; } = 30_000;

    /// <summary>
    /// The timeout applied to each individual attempt, in seconds. If an attempt does
    /// not complete within this window it is treated as a transient failure and retried.
    /// </summary>
    [Range(1, 300)]
    public int PerAttemptTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// The number of consecutive transient failures required to trip the circuit breaker
    /// into the open state.
    /// </summary>
    [Range(1, 100)]
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>
    /// The amount of time the circuit breaker stays open before allowing a single
    /// probe request through, in seconds.
    /// </summary>
    [Range(1, 3_600)]
    public int CircuitBreakerBreakDurationSeconds { get; set; } = 30;
}
