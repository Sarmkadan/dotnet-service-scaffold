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
    /// <param name="exception">The exception to process.</param>
    /// <returns>The concatenated messages from the exception and all inner exceptions, separated by " -> ".</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static string GetFullMessage(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

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
    /// <param name="exception">The exception to process.</param>
    /// <returns>A formatted string containing all stack traces from the exception chain.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static string GetFullStackTrace(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

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
    /// <typeparam name="TException">The exception type to check for.</typeparam>
    /// <param name="exception">The exception to check.</param>
    /// <returns><see langword="true"/> if the exception or any inner exception matches the specified type; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static bool Is<TException>(this Exception exception) where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            TException => true,
            _ => exception.InnerException is not null && exception.InnerException.Is<TException>()
        };
    }

    /// <summary>
    /// Finds the first exception of a specific type in the exception chain.
    /// </summary>
    /// <typeparam name="TException">The exception type to find.</typeparam>
    /// <param name="exception">The root exception to search.</param>
    /// <returns>The first exception of type <typeparamref name="TException"/> found, or <see langword="null"/> if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static TException? FindInnerException<TException>(this Exception exception) where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(exception);

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
    /// <param name="exception">The exception to process.</param>
    /// <returns>A user-friendly error message appropriate for the exception type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static string GetSafeMessage(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

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
    /// <param name="exception">The exception to process.</param>
    /// <returns>An appropriate HTTP status code for the exception type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static int GetHttpStatusCode(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

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
    /// <param name="exception">The exception to evaluate.</param>
    /// <returns><see langword="true"/> if the exception indicates a transient failure that may succeed on retry; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static bool IsRetryable(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

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
    /// <param name="exception">The exception to convert.</param>
    /// <param name="errorId">Optional error identifier. If not provided, a new GUID will be generated.</param>
    /// <returns>An anonymous object containing error details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static object ToErrorObject(this Exception exception, Guid? errorId = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

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
    /// <param name="exception">The exception to log.</param>
    /// <param name="context">Optional context label for the error.</param>
    /// <returns>A formatted log message containing exception details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static string ToLogMessage(this Exception exception, string context = "Error")
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(context);

        return string.Join(Environment.NewLine,
            context,
            $"Type: {exception.GetType().FullName}",
            $"Message: {exception.Message}",
            $"FullMessage: {exception.GetFullMessage()}",
            $"StackTrace: {exception.StackTrace}");
    }
}