#nullable enable

using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.Http;

/// <summary>
/// Provides one <see cref="CircuitBreakerState"/> per upstream origin (scheme, host, and port),
/// so that repeated failures against one host do not short-circuit requests to unrelated hosts.
/// </summary>
/// <remarks>
/// Breakers are created on first use from the current <see cref="ResilienceOptions"/> value.
/// Changes to the circuit-breaker settings therefore apply to origins first seen after the change.
/// The number of breakers grows with the number of distinct origins called, which is bounded
/// by the set of upstream services the application talks to.
/// </remarks>
public sealed class CircuitBreakerRegistry
{
    private readonly IOptionsMonitor<ResilienceOptions> _optionsMonitor;
    private readonly ConcurrentDictionary<string, CircuitBreakerState> _breakers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerRegistry"/> class.
    /// </summary>
    /// <param name="optionsMonitor">Resilience configuration used to create new breakers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="optionsMonitor"/> is <see langword="null"/>.</exception>
    public CircuitBreakerRegistry(IOptionsMonitor<ResilienceOptions> optionsMonitor)
    {
        ArgumentNullException.ThrowIfNull(optionsMonitor);

        _optionsMonitor = optionsMonitor;
    }

    /// <summary>
    /// Gets the circuit breaker responsible for the origin of the given request URI, creating it if needed.
    /// </summary>
    /// <param name="requestUri">An absolute URI whose origin identifies the upstream target.</param>
    /// <returns>The circuit breaker shared by all requests to that origin.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="requestUri"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="requestUri"/> is not an absolute URI.</exception>
    public CircuitBreakerState GetFor(Uri requestUri)
    {
        ArgumentNullException.ThrowIfNull(requestUri);
        if (!requestUri.IsAbsoluteUri)
            throw new ArgumentException("Circuit breakers can only be resolved for absolute URIs.", nameof(requestUri));

        var origin = requestUri.GetLeftPart(UriPartial.Authority);
        return _breakers.GetOrAdd(origin, _ => CreateBreaker());
    }

    private CircuitBreakerState CreateBreaker()
    {
        var options = _optionsMonitor.CurrentValue;
        return new CircuitBreakerState(
            options.CircuitBreakerFailureThreshold,
            TimeSpan.FromSeconds(options.CircuitBreakerBreakDurationSeconds));
    }
}
