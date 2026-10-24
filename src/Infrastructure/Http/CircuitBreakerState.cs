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
/// Thread-safe circuit-breaker state for a single upstream target. Trips to
/// <see cref="CircuitBreakerPhase.Open"/> after a configured number of consecutive transient
/// failures, and after a cool-down period admits exactly one probe request while
/// <see cref="CircuitBreakerPhase.HalfOpen"/>.
/// </summary>
public sealed class CircuitBreakerState
{
    private readonly object _sync = new();
    private readonly int _failureThreshold;
    private readonly TimeSpan _breakDuration;

    private int _consecutiveFailures;
    private CircuitBreakerPhase _phase = CircuitBreakerPhase.Closed;
    private DateTimeOffset _openedAt;
    private bool _probeInFlight;

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
    /// Determines whether a new attempt is currently permitted. When the break duration has
    /// elapsed, the first caller is granted the half-open probe; all other callers are rejected
    /// until that probe reports an outcome or is released.
    /// </summary>
    /// <param name="isProbe">
    /// <see langword="true"/> when the granted attempt is the half-open probe and must be
    /// concluded with <see cref="RecordSuccess"/>, <see cref="RecordFailure"/>, or <see cref="ReleaseProbe"/>.
    /// </param>
    /// <returns><see langword="true"/> if the caller may proceed with an attempt.</returns>
    public bool TryEnter(out bool isProbe)
    {
        lock (_sync)
        {
            isProbe = false;

            switch (_phase)
            {
                case CircuitBreakerPhase.Closed:
                    return true;

                case CircuitBreakerPhase.Open when DateTimeOffset.UtcNow - _openedAt < _breakDuration:
                    return false;

                case CircuitBreakerPhase.Open:
                    _phase = CircuitBreakerPhase.HalfOpen;
                    break;

                case CircuitBreakerPhase.HalfOpen when _probeInFlight:
                    return false;
            }

            _probeInFlight = true;
            isProbe = true;
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
            _probeInFlight = false;
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

    /// <summary>
    /// Releases the half-open probe slot after a probe attempt ended without a verdict
    /// (for example, the caller cancelled it), so that another request may probe the target.
    /// Has no effect when the circuit is not half-open.
    /// </summary>
    public void ReleaseProbe()
    {
        lock (_sync)
        {
            if (_phase == CircuitBreakerPhase.HalfOpen)
                _probeInFlight = false;
        }
    }

    private void Trip()
    {
        _phase = CircuitBreakerPhase.Open;
        _openedAt = DateTimeOffset.UtcNow;
        _probeInFlight = false;
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
