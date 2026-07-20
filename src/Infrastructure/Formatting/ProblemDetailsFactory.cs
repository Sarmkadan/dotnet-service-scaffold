#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Factory for creating RFC 7807 Problem Details responses.
/// Implements the Problem Details format as specified in https://datatracker.ietf.org/doc/html/rfc7807
/// </summary>
public static class ProblemDetailsFactory
{
    /// <summary>
    /// Creates a Problem Details response object according to RFC 7807.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="title">A short, human-readable summary of the problem type.</param>
    /// <param name="detail">A human-readable explanation specific to this occurrence of the problem.</param>
    /// <param name="type">A URI reference that identifies the problem type.</param>
    /// <param name="instance">A URI reference that identifies the specific occurrence of the problem.</param>
    /// <param name="errorCode">An application-specific error code.</param>
    /// <param name="extensions">Additional problem-specific key/value pairs.</param>
    /// <returns>A Problem Details object ready for serialization.</returns>
    public static ProblemDetails CreateProblemDetails(
        HttpContext context,
        int statusCode,
        string? title = null,
        string? detail = null,
        string? type = null,
        string? instance = null,
        string? errorCode = null,
        Dictionary<string, object?>? extensions = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problemDetails = new ProblemDetails
        {
            Type = type ?? "about:blank",
            Title = title ?? GetDefaultTitle(statusCode),
            Status = statusCode,
            Detail = detail,
            Instance = instance ?? context.Request.Path.ToString()
        };

        // Add trace ID from Activity.Current or HttpContext.TraceIdentifier
        var activity = Activity.Current ?? context.Features.Get<Activity>();
        if (activity?.Id != null)
        {
            problemDetails.Extensions["traceId"] = activity.Id;
        }
        else if (context.TraceIdentifier != null)
        {
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        }

        // Add error code if provided
        if (!string.IsNullOrEmpty(errorCode))
        {
            problemDetails.Extensions["errorCode"] = errorCode;
        }

        // Add additional extensions
        if (extensions != null)
        {
            foreach (var extension in extensions)
            {
                problemDetails.Extensions[extension.Key] = extension.Value;
            }
        }

        // Add timestamp
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        return problemDetails;
    }

    /// <summary>
    /// Creates a Problem Details response from an exception.
    /// </summary>
    public static ProblemDetails CreateProblemDetails(
        HttpContext context,
        Exception exception,
        int statusCode,
        string? type = null,
        string? instance = null,
        Dictionary<string, object?>? extensions = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(exception);

        // Get error code from domain exception if available
        var errorCode = exception is DotnetServiceScaffold.Domain.Exceptions.ServiceScaffoldException serviceEx
            ? serviceEx.ErrorCode
            : null;

        return CreateProblemDetails(
            context,
            statusCode,
            title: GetDefaultTitle(statusCode),
            detail: exception.Message,
            type: type,
            instance: instance,
            errorCode: errorCode,
            extensions: extensions
        );
    }

    /// <summary>
    /// Gets the default title for a status code.
    /// </summary>
    private static string GetDefaultTitle(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            405 => "Method Not Allowed",
            406 => "Not Acceptable",
            408 => "Request Timeout",
            409 => "Conflict",
            415 => "Unsupported Media Type",
            422 => "Unprocessable Entity",
            429 => "Too Many Requests",
            500 => "Internal Server Error",
            501 => "Not Implemented",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            504 => "Gateway Timeout",
            _ => "Error"
        };
    }
}

/// <summary>
/// Represents a Problem Details object as defined in RFC 7807.
/// </summary>
public class ProblemDetails
{
    /// <summary>
    /// A URI reference that identifies the problem type.
    /// When dereferenced, it should provide human-readable documentation for the problem type.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// A short, human-readable summary of the problem type.
    /// It shouldn't change from occurrence to occurrence of the problem, except for purposes of localization.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The HTTP status code generated by the origin server for this occurrence of the problem.
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// A human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// A URI reference that identifies the specific occurrence of the problem.
    /// It may or may not yield further information if dereferenced.
    /// </summary>
    public string? Instance { get; set; }

    /// <summary>
    /// Additional members that can be used to carry additional information about the problem.
    /// </summary>
    public Dictionary<string, object?> Extensions { get; set; } = new();
}
