#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// ASP.NET Core middleware that reads (or generates) a correlation ID for each
/// incoming request, stores it in <see cref="ILogContextService"/>, and echoes
/// it back in the response header so clients and downstream services can correlate calls.
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
    /// </summary>
    /// <param name="context">Current HTTP context.</param>
    /// <param name="logContext">Scoped logging context service.</param>
    public async Task InvokeAsync(HttpContext context, ILogContextService logContext)
    {
        var headerName = string.IsNullOrWhiteSpace(_options.CorrelationIdHeader)
            ? "X-Correlation-Id"
            : _options.CorrelationIdHeader;

        var correlationId = context.Request.Headers.TryGetValue(headerName, out var headerValue) &&
                            !string.IsNullOrWhiteSpace(headerValue)
            ? headerValue.ToString()
            : context.TraceIdentifier;

        logContext.CorrelationId = correlationId;

        if (_options.EnrichWithRequestContext)
        {
            logContext.AddProperty("RequestPath", context.Request.Path.Value);
            logContext.AddProperty("RequestMethod", context.Request.Method);
        }

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(headerName))
            {
                context.Response.Headers[headerName] = correlationId;
            }

            return Task.CompletedTask;
        });

        using (logContext.PushProperties())
        {
            await _next(context);
        }
    }
}
