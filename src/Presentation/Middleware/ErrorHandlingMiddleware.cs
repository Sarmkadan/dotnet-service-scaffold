#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using DotnetServiceScaffold.Domain.Exceptions;
using Serilog;

namespace DotnetServiceScaffold.Presentation.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions
/// and returns structured error responses. This prevents stacktraces from leaking
/// to clients and provides consistent error formatting across all endpoints.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invoke the middleware. Wraps the next middleware in a try-catch block to handle
    /// any unhandled exceptions. Returns appropriate HTTP status codes based on exception type.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles exceptions by logging them and returning appropriate HTTP responses.
    /// Maps domain-specific exceptions to appropriate HTTP status codes.
    /// </summary>
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorId = Guid.NewGuid().ToString();
        var timestamp = DateTime.UtcNow;

        var errorResponse = new
        {
            errorId,
            timestamp,
            message = exception.Message,
            type = exception.GetType().Name
        };

        // Log the exception with full context
        _logger.LogError(exception, "Unhandled exception occurred. ErrorId: {ErrorId}", errorId);

        // Map exceptions to HTTP status codes
        response.StatusCode = exception switch
        {
            ServiceScaffoldException => (int)HttpStatusCode.BadRequest,
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.Conflict,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };

        // In production, hide internal details
        if (!context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            return response.WriteAsJsonAsync(new
            {
                errorId,
                timestamp,
                message = "An error occurred processing your request. Please contact support with the error ID.",
                statusCode = response.StatusCode
            });
        }

        return response.WriteAsJsonAsync(errorResponse);
    }
}
