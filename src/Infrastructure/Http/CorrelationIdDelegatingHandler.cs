#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.Http;

/// <summary>
/// HTTP message handler that automatically adds the correlation ID to outgoing requests.
/// Reads the correlation ID from <see cref="ILogContextService"/> which is populated by <see cref="CorrelationIdMiddleware"/>.
/// </summary>
public sealed class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CorrelationIdDelegatingHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationIdDelegatingHandler"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider for resolving scoped services.</param>
    /// <param name="logger">Logger instance.</param>
    public CorrelationIdDelegatingHandler(IServiceProvider serviceProvider, ILogger<CorrelationIdDelegatingHandler> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sends an HTTP request with the correlation ID added to the headers.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response message.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            // Use a scope to resolve the log context service
            using var scope = _serviceProvider.CreateScope();
            var logContext = scope.ServiceProvider.GetRequiredService<ILogContextService>();

            // Get correlation ID from log context
            var correlationId = logContext.CorrelationId;
            var traceParent = logContext.TraceParent;

            if (!string.IsNullOrEmpty(correlationId))
            {
                // Add correlation ID header
                request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
                _logger.LogDebug("Added X-Correlation-Id header: {CorrelationId}", correlationId);
            }

            if (!string.IsNullOrEmpty(traceParent))
            {
                // Add W3C traceparent header for distributed tracing
                request.Headers.TryAddWithoutValidation("traceparent", traceParent);
                _logger.LogDebug("Added traceparent header: {TraceParent}", traceParent);
            }

            // Add additional tracing headers
            request.Headers.TryAddWithoutValidation("X-Request-Id", Guid.NewGuid().ToString("N"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to add correlation ID to request headers");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
