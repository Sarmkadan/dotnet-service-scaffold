#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;

namespace DotnetServiceScaffold.Shared.Models;

/// <summary>
/// Extension methods for <see cref="Result"/> and <see cref="Result{T}"/> types.
/// Provides fluent API for chaining operations and handling results.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Chains multiple operations on a successful result.
    /// </summary>
    public static Result<TNext> Then<T, TNext>(this Result<T> result, Func<T, Result<TNext>> func)
    {
        if (!result.IsSuccess)
            return Result<TNext>.Failure(result.ErrorMessage ?? "Unknown error", result.ErrorCode);

        try
        {
            return func(result.Value!);
        }
        catch (Exception ex)
        {
            return Result<TNext>.Failure(ex);
        }
    }

    /// <summary>
    /// Chains multiple async operations on a successful result.
    /// </summary>
    public static async Task<Result<TNext>> ThenAsync<T, TNext>(this Result<T> result, Func<T, Task<Result<TNext>>> func)
    {
        if (!result.IsSuccess)
            return Result<TNext>.Failure(result.ErrorMessage ?? "Unknown error", result.ErrorCode);

        try
        {
            return await func(result.Value!);
        }
        catch (Exception ex)
        {
            return Result<TNext>.Failure(ex);
        }
    }

    /// <summary>
    /// Converts a non-generic Result to a generic Result with a default value.
    /// </summary>
    public static Result<T> ToGeneric<T>(this Result result, T defaultValue = default!)
    {
        return result.IsSuccess
            ? Result<T>.Success(defaultValue)
            : Result<T>.Failure(result.ErrorMessage ?? "Unknown error", result.ErrorCode);
    }

    /// <summary>
    /// Combines multiple results - succeeds only if all results are successful.
    /// </summary>
    public static Result Combine(this Result first, params Result[] others)
    {
        if (!first.IsSuccess)
            return first;

        foreach (var result in others)
        {
            if (!result.IsSuccess)
                return result;
        }

        return Result.Success();
    }

    /// <summary>
    /// Combines multiple generic results - succeeds only if all results are successful.
    /// </summary>
    public static Result<T[]> Combine<T>(this Result<T> first, params Result<T>[] others)
    {
        if (!first.IsSuccess)
            return Result<T[]>.Failure(first.ErrorMessage ?? "Unknown error", first.ErrorCode);

        var results = new T[others.Length + 1];
        results[0] = first.Value!;

        for (int i = 0; i < others.Length; i++)
        {
            if (!others[i].IsSuccess)
                return Result<T[]>.Failure(others[i].ErrorMessage ?? "Unknown error", others[i].ErrorCode);

            results[i + 1] = others[i].Value!;
        }

        return Result<T[]>.Success(results);
    }

    /// <summary>
    /// Executes an action if the result is successful and returns the original result for chaining.
    /// </summary>
    public static Result<T> Also<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
            action(result.Value!);

        return result;
    }

    /// <summary>
    /// Executes an action if the result failed and returns the original result for chaining.
    /// </summary>
    public static Result Also(this Result result, Action<string?, string?> action)
    {
        if (!result.IsSuccess)
            action(result.ErrorMessage, result.ErrorCode);

        return result;
    }

    /// <summary>
    /// Tries to get the value or returns a default if the result failed.
    /// </summary>
    public static T GetValueOrDefault<T>(this Result<T> result, T defaultValue = default!)
    {
        return result.IsSuccess ? result.Value! : defaultValue;
    }

    /// <summary>
    /// Tries to get the value or throws an exception if the result failed.
    /// </summary>
    public static T GetValueOrThrow<T>(this Result<T> result)
    {
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                result.ErrorMessage ?? "Operation failed with unknown error");
        }

        return result.Value!;
    }

    /// <summary>
    /// Tries to get the error details as a tuple.
    /// </summary>
    public static (string? ErrorMessage, string? ErrorCode) GetError(this Result result)
    {
        return (result.ErrorMessage, result.ErrorCode);
    }

    /// <summary>
    /// Tries to get the error details as a tuple.
    /// </summary>
    public static (string? ErrorMessage, string? ErrorCode) GetError<T>(this Result<T> result)
    {
        return (result.ErrorMessage, result.ErrorCode);
    }

    /// <summary>
    /// Creates a result from a boolean condition.
    /// </summary>
    public static Result FromCondition(bool condition, string errorMessage, string? errorCode = null)
    {
        return condition
            ? Result.Success()
            : Result.Failure(errorMessage, errorCode);
    }

    /// <summary>
    /// Creates a result from a boolean condition with a value.
    /// </summary>
    public static Result<T> FromCondition<T>(bool condition, Func<T> valueFactory, string errorMessage, string? errorCode = null)
    {
        return condition
            ? Result<T>.Success(valueFactory())
            : Result<T>.Failure(errorMessage, errorCode);
    }
}