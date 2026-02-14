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
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Success() => new(true);

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    public static Result Failure(string errorMessage, string? errorCode = null) =>
        new(false, errorMessage, errorCode);

    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    public static Result Failure(Exception exception) =>
        new(false, exception.Message, exception.GetType().Name);
}

/// <summary>
/// Generic result wrapper for operations that return a value or fail.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? ErrorMessage { get; private set; }
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Success(T value) => new(true, value);

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    public static Result<T> Failure(string errorMessage, string? errorCode = null) =>
        new(false, default, errorMessage, errorCode);

    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    public static Result<T> Failure(Exception exception) =>
        new(false, default, exception.Message, exception.GetType().Name);

    /// <summary>
    /// Converts a Result to a Result&lt;T&gt; if it was successful, otherwise fails with the error.
    /// </summary>
    public static Result<T> FromResult(Result result, T value)
    {
        return result.IsSuccess
            ? Success(value)
            : Failure(result.ErrorMessage ?? "Unknown error", result.ErrorCode);
    }

    /// <summary>
    /// Maps the result value to a different type.
    /// </summary>
    public Result<TNext> Map<TNext>(Func<T?, TNext> mapper)
    {
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
    public void IfSuccess(Action<T?> action)
    {
        if (IsSuccess)
            action(Value);
    }

    /// <summary>
    /// Executes an action if the result failed.
    /// </summary>
    public void IfFailure(Action<string?, string?> action)
    {
        if (!IsSuccess)
            action(ErrorMessage, ErrorCode);
    }
}
