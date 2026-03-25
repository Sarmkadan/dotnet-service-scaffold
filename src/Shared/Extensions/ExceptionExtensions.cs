#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Shared.Extensions;

/// <summary>
/// Extension methods for Exception types to simplify error handling and debugging.
/// Provides utilities for extracting information from exceptions and formatting error messages.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Gets the complete exception message including inner exception messages.
    /// Useful for logging the full error chain.
    /// </summary>
    public static string GetFullMessage(this Exception exception)
    {
        if (exception is null)
            return string.Empty;

        var messages = new List<string> { exception.Message };

        var innerException = exception.InnerException;
        while (innerException is not null)
        {
            messages.Add(innerException.Message);
            innerException = innerException.InnerException;
        }

        return string.Join(" -> ", messages);
    }

    /// <summary>
    /// Gets the complete stack trace including stack traces from inner exceptions.
    /// </summary>
    public static string GetFullStackTrace(this Exception exception)
    {
        if (exception is null)
            return string.Empty;

        var traces = new List<string>();

        var current = exception;
        int level = 0;

        while (current is not null)
        {
            if (!string.IsNullOrEmpty(current.StackTrace))
            {
                traces.Add($"Level {level}: {current.GetType().Name}");
                traces.Add(current.StackTrace);
                traces.Add(string.Empty);
            }

            current = current.InnerException;
            level++;
        }

        return string.Join(Environment.NewLine, traces);
    }

    /// <summary>
    /// Checks if an exception is of a specific type or has an inner exception of that type.
    /// </summary>
    public static bool Is<TException>(this Exception exception) where TException : Exception
    {
        if (exception is TException)
            return true;

        return exception.InnerException is not null && exception.InnerException.Is<TException>();
    }

    /// <summary>
    /// Finds the first exception of a specific type in the exception chain.
    /// </summary>
    public static TException? FindInnerException<TException>(this Exception exception) where TException : Exception
    {
        var current = exception;

        while (current is not null)
        {
            if (current is TException match)
                return match;

            current = current.InnerException;
        }

        return null;
    }

    /// <summary>
    /// Gets a safe error message for user display that doesn't leak internal details.
    /// </summary>
    public static string GetSafeMessage(this Exception exception)
    {
        if (exception is null)
            return "An unexpected error occurred.";

        return exception switch
        {
            ArgumentNullException => "Required value was not provided.",
            ArgumentException => "Invalid input provided.",
            InvalidOperationException => "The requested operation is not valid in the current state.",
            NotImplementedException => "This feature is not yet implemented.",
            TimeoutException => "The operation timed out. Please try again.",
            HttpRequestException => "Failed to communicate with the external service.",
            _ => "An error occurred processing your request. Please try again later."
        };
    }

    /// <summary>
    /// Gets the HTTP status code that would be appropriate for this exception.
    /// </summary>
    public static int GetHttpStatusCode(this Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => 400,
            ArgumentException => 400,
            KeyNotFoundException => 404,
            InvalidOperationException => 409,
            TimeoutException => 504,
            NotImplementedException => 501,
            HttpRequestException => 502,
            _ => 500
        };
    }

    /// <summary>
    /// Determines if an exception should be retried.
    /// </summary>
    public static bool IsRetryable(this Exception exception)
    {
        return exception switch
        {
            TimeoutException => true,
            HttpRequestException => true,
            IOException => true,
            OperationCanceledException => false,
            _ => false
        };
    }

    /// <summary>
    /// Converts an exception to a formatted error object suitable for API responses.
    /// </summary>
    public static object ToErrorObject(this Exception exception, Guid? errorId = null)
    {
        return new
        {
            errorId = errorId ?? Guid.NewGuid(),
            message = exception.GetSafeMessage(),
            type = exception.GetType().Name,
            statusCode = exception.GetHttpStatusCode()
        };
    }

    /// <summary>
    /// Logs exception details in a structured format.
    /// Returns a formatted string with all relevant exception information.
    /// </summary>
    public static string ToLogMessage(this Exception exception, string context = "Error")
    {
        return $@"
{context}
Type: {exception.GetType().FullName}
Message: {exception.Message}
FullMessage: {exception.GetFullMessage()}
StackTrace: {exception.StackTrace}";
    }
}
