#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using Serilog;

namespace DotnetServiceScaffold.Presentation.Middleware;

/// <summary>
/// Middleware that logs HTTP requests and responses with timing information.
/// Provides observability into API usage patterns and performance. Excludes health check
/// endpoints from verbose logging to reduce noise in logs.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invoke the middleware. Logs request details, measures response time, and logs response status.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for health check endpoints to reduce noise
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/status"))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        // Capture response body for logging
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            var requestId = context.TraceIdentifier;
            var method = context.Request.Method;
            var path = context.Request.Path;
            var queryString = context.Request.QueryString.Value;

            _logger.LogInformation(
                "Incoming HTTP {Method} {Path}{QueryString} | RequestId: {RequestId}",
                method, path, queryString, requestId);

            try
            {
                await _next(context);

                stopwatch.Stop();

                _logger.LogInformation(
                    "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMilliseconds}ms | RequestId: {RequestId}",
                    method, path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds, requestId);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "HTTP {Method} {Path} failed after {ElapsedMilliseconds}ms | RequestId: {RequestId}",
                    method, path, stopwatch.ElapsedMilliseconds, requestId);
                throw;
            }
            finally
            {
                // Copy response body back to original stream
                responseBody.Position = 0; // Fix: reset position before copying to avoid empty response
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }
}
