#nullable enable

using System.Net;
using Microsoft.Extensions.Options;
using Serilog;

namespace DotnetServiceScaffold.Infrastructure.Http;

/// <summary>
/// Delegating handler that wraps outbound HTTP calls with a resilience pipeline:
/// a per-attempt timeout, exponential backoff with jitter for transient failures
/// (5xx responses, HTTP 408, and network-level <see cref="HttpRequestException"/>s),
/// and a circuit breaker per upstream origin that short-circuits requests once that target is
/// consistently unavailable.
/// </summary>
/// <remarks>
/// The current <see cref="ResilienceOptions"/> value is read for each request, allowing
/// updated resilience settings to be applied without recreating the handler.
/// </remarks>
public class ResilientHttpMessageHandler : DelegatingHandler
{
    private readonly IOptionsMonitor<ResilienceOptions> _optionsMonitor;
    private readonly CircuitBreakerRegistry _circuitBreakers;
    private readonly ILogger<ResilientHttpMessageHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResilientHttpMessageHandler"/> class.
    /// </summary>
    /// <param name="optionsMonitor">Live-reloadable resilience configuration.</param>
    /// <param name="circuitBreakers">Registry providing the circuit breaker for each upstream origin.</param>
    /// <param name="logger">Logger used to record retry and circuit-breaker events.</param>
    /// <exception cref="ArgumentNullException">
    /// Any of <paramref name="optionsMonitor"/>, <paramref name="circuitBreakers"/>, or
    /// <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    public ResilientHttpMessageHandler(
        IOptionsMonitor<ResilienceOptions> optionsMonitor,
        CircuitBreakerRegistry circuitBreakers,
        ILogger<ResilientHttpMessageHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(optionsMonitor);
        ArgumentNullException.ThrowIfNull(circuitBreakers);
        ArgumentNullException.ThrowIfNull(logger);

        _optionsMonitor = optionsMonitor;
        _circuitBreakers = circuitBreakers;
        _logger = logger;
    }

    /// <summary>
    /// Sends the request, applying the retry, timeout, and circuit-breaker policy.
    /// </summary>
    /// <param name="request">The outbound HTTP request.</param>
    /// <param name="cancellationToken">Cancellation token for the overall operation.</param>
    /// <returns>The final <see cref="HttpResponseMessage"/> once a successful or non-retryable outcome is reached.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> or its <see cref="HttpRequestMessage.RequestUri"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="request"/> does not have an absolute <see cref="HttpRequestMessage.RequestUri"/>.</exception>
    /// <exception cref="BrokenCircuitException">The circuit breaker for the request's origin is currently open.</exception>
    /// <exception cref="HttpRequestException">All retry attempts were exhausted with a transient failure.</exception>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.RequestUri);

        var options = _optionsMonitor.CurrentValue;
        var circuitBreaker = _circuitBreakers.GetFor(request.RequestUri);
        var attempt = 0;
        Exception? lastException = null;
        HttpResponseMessage? lastResponse = null;

        while (attempt <= options.RetryCount)
        {
            attempt++;

            if (!circuitBreaker.TryEnter(out var isProbe))
            {
                throw new BrokenCircuitException(
                    $"Circuit breaker is open for {request.RequestUri.GetLeftPart(UriPartial.Authority)}; request rejected without attempting the call.");
            }

            using var attemptCts = new CancellationTokenSource(TimeSpan.FromSeconds(options.PerAttemptTimeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, attemptCts.Token);
            var verdictRecorded = false;

            try
            {
                var cloned = await CloneRequestAsync(request).ConfigureAwait(false);
                var response = await base.SendAsync(cloned, linkedCts.Token).ConfigureAwait(false);

                if (IsTransientStatusCode(response.StatusCode))
                {
                    circuitBreaker.RecordFailure();
                    verdictRecorded = true;
                    lastResponse?.Dispose();
                    lastResponse = response;

                    if (attempt > options.RetryCount)
                        return response;

                    await DelayBeforeRetryAsync(options, attempt, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                circuitBreaker.RecordSuccess();
                verdictRecorded = true;
                return response;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Per-attempt timeout fired, not the caller's own cancellation token.
                circuitBreaker.RecordFailure();
                verdictRecorded = true;
                lastException = new TimeoutException(
                    $"Attempt {attempt} to {request.RequestUri} exceeded the per-attempt timeout of {options.PerAttemptTimeoutSeconds}s.");

                if (attempt > options.RetryCount)
                    break;

                _logger.LogWarning(
                    "Attempt {Attempt} to {RequestUri} timed out after {TimeoutSeconds}s; retrying.",
                    attempt, request.RequestUri, options.PerAttemptTimeoutSeconds);

                await DelayBeforeRetryAsync(options, attempt, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                circuitBreaker.RecordFailure();
                verdictRecorded = true;
                lastException = ex;

                if (attempt > options.RetryCount)
                    break;

                _logger.LogWarning(
                    ex,
                    "Attempt {Attempt} to {RequestUri} failed transiently; retrying.",
                    attempt, request.RequestUri);

                await DelayBeforeRetryAsync(options, attempt, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                // A half-open probe that ended without a verdict (caller cancelled, or an unexpected
                // exception) must free the probe slot, otherwise the circuit would stay blocked.
                if (isProbe && !verdictRecorded)
                    circuitBreaker.ReleaseProbe();
            }
        }

        if (lastResponse is not null)
            return lastResponse;

        throw new HttpRequestException(
            $"Request to {request.RequestUri} failed after {attempt} attempt(s).",
            lastException);
    }

    /// <summary>
    /// Determines whether the given status code should be treated as a transient failure
    /// worth retrying (server errors, request timeout, and too many requests).
    /// </summary>
    private static bool IsTransientStatusCode(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests
            || (int)statusCode >= 500;

    /// <summary>
    /// Computes an exponential backoff delay with full jitter and waits before the next attempt.
    /// </summary>
    private static Task DelayBeforeRetryAsync(ResilienceOptions options, int attempt, CancellationToken cancellationToken)
    {
        var exponential = options.BaseDelayMilliseconds * Math.Pow(2, attempt - 1);
        var capped = Math.Min(exponential, options.MaxDelayMilliseconds);
        var jittered = Random.Shared.NextDouble() * capped;
        return Task.Delay(TimeSpan.FromMilliseconds(jittered), cancellationToken);
    }

    /// <summary>
    /// Clones a request message so it can be safely re-sent on retry, since
    /// <see cref="HttpRequestMessage"/> instances cannot be reused after being sent.
    /// </summary>
    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        if (request.Content is not null)
        {
            var buffer = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var content = new ByteArrayContent(buffer);
            foreach (var header in request.Content.Headers)
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);

            clone.Content = content;
        }

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        foreach (var option in request.Options)
            ((IDictionary<string, object?>)clone.Options)[option.Key] = option.Value;

        return clone;
    }
}
