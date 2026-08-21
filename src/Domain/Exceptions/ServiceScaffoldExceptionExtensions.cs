#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetServiceScaffold.Domain.Exceptions;

/// <summary>
/// Extension methods for <see cref="ServiceScaffoldException"/> and its derived types.
/// </summary>
public static class ServiceScaffoldExceptionExtensions
{
    /// <summary>
    /// Determines whether the exception represents a critical failure.
    /// </summary>
    /// <param name="exception">The exception to evaluate.</param>
    /// <returns>
    /// <see langword="true"/> if the exception's <see cref="ServiceScaffoldException.ErrorCode"/>
    /// is one of the known critical codes; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
    public static bool IsCritical(this ServiceScaffoldException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        // Known critical error codes – adjust as the platform evolves.
        return exception.ErrorCode switch
        {
            "DATA_ACCESS_ERROR" => true,
            "RESOURCE_EXHAUSTED" => true,
            "HEALTH_CHECK_FAILED" => true,
            _ => false,
        };
    }

    /// <summary>
    /// Retrieves a read‑only collection of error messages associated with the exception.
    /// </summary>
    /// <param name="exception">The exception from which to extract messages.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> containing one or more messages.
    /// For <see cref="ServiceValidationException"/> the collection contains all validation errors;
    /// otherwise it contains the exception's <see cref="Exception.Message"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> GetErrorMessages(this ServiceScaffoldException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            ServiceValidationException validationException => validationException.Errors.AsReadOnly(),
            _ => new[] { exception.Message }
        };
    }

    /// <summary>
    /// Sets (or replaces) the <see cref="ServiceScaffoldException.ErrorCode"/> on the exception
    /// and returns the same instance to enable fluent usage.
    /// </summary>
    /// <param name="exception">The exception to modify.</param>
    /// <param name="errorCode">The new error code to assign.</param>
    /// <returns>The modified <paramref name="exception"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errorCode"/> is <c>null</c> or empty.</exception>
    public static ServiceScaffoldException SetErrorCode(this ServiceScaffoldException exception, string errorCode)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(errorCode);

        exception.ErrorCode = errorCode;
        return exception;
    }
}
