#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Infrastructure.Formatting;
using DotnetServiceScaffold.Infrastructure.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.Middleware;

/// <summary>
/// Global exception handler that implements .NET 8+ IExceptionHandler for consistent error responses.
/// Maps exceptions to RFC 7807 ProblemDetails responses with correlation ID tracking.
/// </summary>
/// <remarks>
/// This handler provides a modern alternative to middleware-based exception handling,
/// following .NET 8+ best practices for global error handling.
/// </remarks>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly ILogContextService _logContextService;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        ILogContextService logContextService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(logContextService);

        _logger = logger;
        _logContextService = logContextService;
    }

    /// <summary>
    /// Attempts to handle the exception and produce a ProblemDetails response.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the exception was handled; otherwise false.</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        // Get correlation ID from log context service
        var correlationId = _logContextService.CorrelationId ?? Guid.NewGuid().ToString("N");

        // Log the exception with full context
        _logger.LogError(exception, "Unhandled exception occurred. CorrelationId: {CorrelationId}", correlationId);

        // Determine status code based on exception type
        var statusCode = GetStatusCode(exception);
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        // Create ProblemDetails response
        var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
            httpContext,
            exception,
            statusCode,
            extensions: new Dictionary<string, object?>
            {
                ["correlationId"] = correlationId,
                ["errorId"] = Guid.NewGuid().ToString(),
                ["timestamp"] = DateTime.UtcNow
            }
        );

        // Write the response
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    /// <summary>
    /// Determines the appropriate HTTP status code for a given exception.
    /// </summary>
    /// <param name="exception">The exception to evaluate.</param>
    /// <returns>The HTTP status code.</returns>
    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            DotnetServiceScaffold.Domain.Exceptions.ServiceScaffoldException => StatusCodes.Status400BadRequest,
            ArgumentNullException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status409Conflict,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    /// <summary>
    /// Creates a ProblemDetails response object for the given exception.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="correlationId">The correlation ID for tracing.</param>
    /// <returns>A ProblemDetails object ready for serialization.</returns>
    private ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        Exception exception,
        int statusCode,
        string correlationId)
    {
        // In production, hide internal details
        var isDevelopment = httpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment();

        if (!isDevelopment)
        {
            // Production: sanitized error response
            var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
                httpContext,
                statusCode: statusCode,
                detail: "An error occurred processing your request. Please contact support with the correlation ID.",
                errorCode: "INTERNAL_ERROR"
            );

            // Add correlation ID to extensions
            problemDetails.Extensions["correlationId"] = correlationId;
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

            return problemDetails;
        }

        // Development: include full details
        var devProblemDetails = ProblemDetailsFactory.CreateProblemDetails(
            httpContext,
            exception,
            statusCode: statusCode
        );

        // Add correlation ID and error tracking to extensions
        devProblemDetails.Extensions["correlationId"] = correlationId;
        devProblemDetails.Extensions["errorId"] = Guid.NewGuid().ToString();
        devProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        return devProblemDetails;
    }
}