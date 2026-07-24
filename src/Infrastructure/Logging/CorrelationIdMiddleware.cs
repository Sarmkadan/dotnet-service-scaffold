#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// ASP.NET Core middleware that reads (or generates) a correlation ID for each
/// incoming request, stores it in <see cref="ILogContextService"/> with proper W3C trace context integration,
/// and echoes it back in the response header so clients and downstream services can correlate calls.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly StructuredLoggingOptions _options;

    public CorrelationIdMiddleware(RequestDelegate next, IOptions<StructuredLoggingOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    /// <summary>
    /// Processes the HTTP request by attaching a correlation ID to the log context.
    /// Integrates with W3C trace context if available.
    /// </summary>
    /// <param name="context">Current HTTP context.</param>
    /// <param name="logContext">Scoped logging context service.</param>
    public async Task InvokeAsync(HttpContext context, ILogContextService logContext)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logContext);

        var headerName = string.IsNullOrWhiteSpace(_options.CorrelationIdHeader)
            ? "X-Correlation-Id"
            : _options.CorrelationIdHeader;

        // Try to read correlation ID from header
        var correlationId = context.Request.Headers.TryGetValue(headerName, out var headerValue) &&
                            !string.IsNullOrWhiteSpace(headerValue)
            ? headerValue.ToString()
            : null;

        // Initialize W3C trace context if available
        var activity = Activity.Current;
        if (activity is not null)
        {
            // Store Activity properties in the log context
            logContext.ActivityId = activity.Id;
            logContext.TraceParent = activity.IdFormat switch
            {
                ActivityIdFormat.W3C => $"00-{activity.TraceId:D32}-{activity.SpanId:D16}-00",
                _ => $"00-{activity.TraceId:D32}-{activity.SpanId:D16}-01"
            };

            // If no correlation ID from header, use the W3C trace ID
            if (correlationId is null && activity.TraceId != default)
            {
                correlationId = activity.TraceId.ToHexString();
            }
        }

        // Initialize or ensure correlation ID is set
        if (correlationId is null)
        {
            correlationId = logContext.InitializeCorrelationId();
        }
        else
        {
            logContext.CorrelationId = correlationId;
        }

        // Add request context properties if enabled
        if (_options.EnrichWithRequestContext)
        {
            logContext.AddProperty("RequestPath", context.Request.Path.Value);
            logContext.AddProperty("RequestMethod", context.Request.Method);
        }

        // Set correlation ID in response header for downstream services
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(headerName))
            {
                context.Response.Headers[headerName] = correlationId;
            }

            return Task.CompletedTask;
        });

        // Push properties to Serilog context for the duration of this request
        using (logContext.PushProperties())
        {
            await _next(context);
        }
    }
}
