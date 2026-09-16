#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Runtime.CompilerServices;

namespace DotnetServiceScaffold.Shared.Models;

/// <summary>
/// Generic result wrapper for operations that can succeed or fail.
/// Provides a clean way to return operation outcomes with error details.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Gets the error message when the operation failed; otherwise, <see langword="null"/>.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Gets the error code when the operation failed; otherwise, <see langword="null"/>.
    /// </summary>
    public string? ErrorCode { get; private set; }

    protected Result(bool isSuccess, string? errorMessage = null, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Success() => new(true);

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    /// <param name="errorMessage">The message that describes the error.</param>
    /// <param name="errorCode">The optional code that identifies the error.</param>
    /// <returns>A failed result containing the supplied error details.</returns>
    /// <exception cref="ArgumentException"><paramref name="errorMessage"/> is empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="errorMessage"/> is <see langword="null"/>.</exception>
    public static Result Failure(string errorMessage, string? errorCode = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorMessage);
        return new(false, errorMessage, errorCode);
    }

    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    /// <param name="exception">The exception whose message and type name describe the failure.</param>
    /// <returns>A failed result containing details from <paramref name="exception"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    public static Result Failure(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new(false, exception.Message, exception.GetType().Name);
    }
}

/// <summary>
/// Generic result wrapper for operations that return a value or fail.
/// </summary>
/// <typeparam name="T">The type of value returned by a successful operation.</typeparam>
public class Result<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Gets the value produced by a successful operation.
    /// </summary>
    public T? Value { get; private set; }

    /// <summary>
    /// Gets the error message when the operation failed; otherwise, <see langword="null"/>.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Gets the error code when the operation failed; otherwise, <see langword="null"/>.
    /// </summary>
    public string? ErrorCode { get; private set; }

    protected Result(bool isSuccess, T? value = default, string? errorMessage = null, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <param name="value">The value produced by the successful operation.</param>
    /// <returns>A successful result containing <paramref name="value"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Success(T value) => new(true, value);

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    /// <param name="errorMessage">The message that describes the error.</param>
    /// <param name="errorCode">The optional code that identifies the error.</param>
    /// <returns>A failed result containing the supplied error details.</returns>
    /// <exception cref="ArgumentException"><paramref name="errorMessage"/> is empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="errorMessage"/> is <see langword="null"/>.</exception>
    public static Result<T> Failure(string errorMessage, string? errorCode = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorMessage);
        return new(false, default, errorMessage, errorCode);
    }

    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    /// <param name="exception">The exception whose message and type name describe the failure.</param>
    /// <returns>A failed result containing details from <paramref name="exception"/>.</returns>
    public static Result<T> Failure(Exception exception) =>
        new(false, default, exception.Message, exception.GetType().Name);

    /// <summary>
    /// Converts a Result to a Result&lt;T&gt; if it was successful, otherwise fails with the error.
    /// </summary>
    /// <param name="result">The result whose success state and error details are propagated.</param>
    /// <param name="value">The value to include when <paramref name="result"/> is successful.</param>
    /// <returns>
    /// A successful result containing <paramref name="value"/> when <paramref name="result"/> is successful;
    /// otherwise, a failed result containing the original error details.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static Result<T> FromResult(Result result, T value)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess
            ? Success(value)
            : Failure(result.ErrorMessage ?? "Unknown error", result.ErrorCode);
    }

    /// <summary>
    /// Maps the result value to a different type.
    /// </summary>
    /// <typeparam name="TNext">The type of the mapped value.</typeparam>
    /// <param name="mapper">The function that transforms the current value.</param>
    /// <returns>
    /// A successful result containing the mapped value, or a failed result containing either the current
    /// error details or details of an exception thrown by <paramref name="mapper"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="mapper"/> is <see langword="null"/>.</exception>
    public Result<TNext> Map<TNext>(Func<T?, TNext> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        if (!IsSuccess)
            return Result<TNext>.Failure(ErrorMessage ?? "Unknown error", ErrorCode);

        try
        {
            var mappedValue = mapper(Value);
            return Result<TNext>.Success(mappedValue);
        }
        catch (Exception ex)
        {
            return Result<TNext>.Failure(ex);
        }
    }

    /// <summary>
    /// Applies an async operation to the result value.
    /// </summary>
    /// <typeparam name="TNext">The type of the mapped value.</typeparam>
    /// <param name="mapper">The asynchronous function that transforms the current value.</param>
    /// <returns>
    /// A task whose result contains the mapped value, the current error details, or details of an exception
    /// thrown by <paramref name="mapper"/>.
    /// </returns>
    public async Task<Result<TNext>> MapAsync<TNext>(Func<T?, Task<TNext>> mapper)
    {
        if (!IsSuccess)
            return Result<TNext>.Failure(ErrorMessage ?? "Unknown error", ErrorCode);

        try
        {
            var mappedValue = await mapper(Value);
            return Result<TNext>.Success(mappedValue);
        }
        catch (Exception ex)
        {
            return Result<TNext>.Failure(ex);
        }
    }

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    /// <param name="action">The action to execute with the result value.</param>
    public void IfSuccess(Action<T?> action)
    {
        if (IsSuccess)
            action(Value);
    }

    /// <summary>
    /// Executes an action if the result failed.
    /// </summary>
    /// <param name="action">The action to execute with the error message and error code.</param>
    public void IfFailure(Action<string?, string?> action)
    {
        if (!IsSuccess)
            action(ErrorMessage, ErrorCode);
    }
}
