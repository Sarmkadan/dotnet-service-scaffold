#nullable enable

namespace DotnetServiceScaffold.Infrastructure.Http;

/// <summary>
/// Describes the current state of a <see cref="CircuitBreakerState"/> instance.
/// </summary>
public enum CircuitBreakerPhase
{
    /// <summary>Requests flow through normally.</summary>
    Closed,

    /// <summary>Requests are short-circuited without being attempted.</summary>
    Open,

    /// <summary>A single probe request is allowed through to test recovery.</summary>
    HalfOpen
}

/// <summary>
/// Thread-safe circuit-breaker state shared across all attempts made through a resilient
/// HTTP message handler. Trips to <see cref="CircuitBreakerPhase.Open"/> after a configured
/// number of consecutive transient failures and resets after a cool-down period.
/// </summary>
public sealed class CircuitBreakerState
{
    private readonly object _sync = new();
    private readonly int _failureThreshold;
    private readonly TimeSpan _breakDuration;

    private int _consecutiveFailures;
    private CircuitBreakerPhase _phase = CircuitBreakerPhase.Closed;
    private DateTimeOffset _openedAt;

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerState"/> class.
    /// </summary>
    /// <param name="failureThreshold">Consecutive failures required to open the circuit.</param>
    /// <param name="breakDuration">How long the circuit stays open before allowing a probe.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="failureThreshold"/> is less than 1, or <paramref name="breakDuration"/> is not positive.
    /// </exception>
    public CircuitBreakerState(int failureThreshold, TimeSpan breakDuration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(failureThreshold, 1);
        if (breakDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(breakDuration), breakDuration, "Break duration must be positive.");

        _failureThreshold = failureThreshold;
        _breakDuration = breakDuration;
    }

    /// <summary>
    /// Determines whether a new attempt is currently permitted, transitioning the circuit
    /// from <see cref="CircuitBreakerPhase.Open"/> to <see cref="CircuitBreakerPhase.HalfOpen"/>
    /// once the break duration has elapsed.
    /// </summary>
    /// <returns><see langword="true"/> if the caller may proceed with an attempt.</returns>
    public bool TryEnter()
    {
        lock (_sync)
        {
            if (_phase != CircuitBreakerPhase.Open)
                return true;

            if (DateTimeOffset.UtcNow - _openedAt < _breakDuration)
                return false;

            _phase = CircuitBreakerPhase.HalfOpen;
            return true;
        }
    }

    /// <summary>
    /// Records a successful attempt, closing the circuit and resetting the failure count.
    /// </summary>
    public void RecordSuccess()
    {
        lock (_sync)
        {
            _consecutiveFailures = 0;
            _phase = CircuitBreakerPhase.Closed;
        }
    }

    /// <summary>
    /// Records a failed attempt, opening the circuit once the configured failure threshold
    /// is reached (or immediately, if the failure occurred during a half-open probe).
    /// </summary>
    public void RecordFailure()
    {
        lock (_sync)
        {
            if (_phase == CircuitBreakerPhase.HalfOpen)
            {
                Trip();
                return;
            }

            _consecutiveFailures++;
            if (_consecutiveFailures >= _failureThreshold)
                Trip();
        }
    }

    private void Trip()
    {
        _phase = CircuitBreakerPhase.Open;
        _openedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the current phase of the circuit breaker.
    /// </summary>
    public CircuitBreakerPhase Phase
    {
        get
        {
            lock (_sync)
            {
                return _phase;
            }
        }
    }
}
