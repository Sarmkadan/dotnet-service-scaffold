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
    /// <typeparam name="T">The type of the source result value.</typeparam>
    /// <typeparam name="TNext">The type of the next result value.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="func">The function to apply to the successful result value.</param>
    /// <returns>A new result from applying the function, or the original failure result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="func"/> is <see langword="null"/>.</exception>
    public static Result<TNext> Then<T, TNext>(this Result<T> result, Func<T, Result<TNext>> func)
    {
        ArgumentNullException.ThrowIfNull(func);

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
    /// <typeparam name="T">The type of the source result value.</typeparam>
    /// <typeparam name="TNext">The type of the next result value.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="func">The async function to apply to the successful result value.</param>
    /// <returns>A task that represents the combined result operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="func"/> is <see langword="null"/>.</exception>
    public static async Task<Result<TNext>> ThenAsync<T, TNext>(this Result<T> result, Func<T, Task<Result<TNext>>> func)
    {
        ArgumentNullException.ThrowIfNull(func);

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
    /// <typeparam name="T">The type of the resulting generic result.</typeparam>
    /// <param name="result">The source result to convert.</param>
    /// <param name="defaultValue">The default value to use if conversion is needed.</param>
    /// <returns>A generic result containing the default value on success, or the original error.</returns>
    public static Result<T> ToGeneric<T>(this Result result, T defaultValue = default!)
    {
        return result.IsSuccess
            ? Result<T>.Success(defaultValue)
            : Result<T>.Failure(result.ErrorMessage ?? "Unknown error", result.ErrorCode);
    }

    /// <summary>
    /// Combines multiple results - succeeds only if all results are successful.
    /// </summary>
    /// <param name="first">The first result in the sequence.</param>
    /// <param name="others">Additional results to combine.</param>
    /// <returns>A successful result if all inputs are successful; otherwise the first failure.</returns>
    public static Result Combine(this Result first, params Result[] others)
    {
        ArgumentNullException.ThrowIfNull(others);

        if (!first.IsSuccess)
            return first;

        foreach (var result in others)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (!result.IsSuccess)
                return result;
        }

        return Result.Success();
    }

    /// <summary>
    /// Combines multiple generic results - succeeds only if all results are successful.
    /// </summary>
    /// <typeparam name="T">The type of the result values.</typeparam>
    /// <param name="first">The first result in the sequence.</param>
    /// <param name="others">Additional results to combine.</param>
    /// <returns>An array of all successful values if all inputs are successful; otherwise the first failure.</returns>
    public static Result<T[]> Combine<T>(this Result<T> first, params Result<T>[] others)
    {
        ArgumentNullException.ThrowIfNull(others);

        if (!first.IsSuccess)
            return Result<T[]>.Failure(first.ErrorMessage ?? "Unknown error", first.ErrorCode);

        var results = new T[others.Length + 1];
        results[0] = first.Value!;

        for (int i = 0; i < others.Length; i++)
        {
            ArgumentNullException.ThrowIfNull(others[i]);

            if (!others[i].IsSuccess)
                return Result<T[]>.Failure(others[i].ErrorMessage ?? "Unknown error", others[i].ErrorCode);

            results[i + 1] = others[i].Value!;
        }

        return Result<T[]>.Success(results);
    }

    /// <summary>
    /// Executes an action if the result is successful and returns the original result for chaining.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="action">The action to execute on the successful value.</param>
    /// <returns>The original result for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
    public static Result<T> Also<T>(this Result<T> result, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsSuccess)
            action(result.Value!);

        return result;
    }

    /// <summary>
    /// Executes an action if the result failed and returns the original result for chaining.
    /// </summary>
    /// <param name="result">The source result.</param>
    /// <param name="action">The action to execute with error details.</param>
    /// <returns>The original result for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
    public static Result Also(this Result result, Action<string?, string?> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (!result.IsSuccess)
            action(result.ErrorMessage, result.ErrorCode);

        return result;
    }

    /// <summary>
    /// Tries to get the value or returns a default if the result failed.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="defaultValue">The value to return if the result failed.</param>
    /// <returns>The result value if successful; otherwise the default value.</returns>
    public static T GetValueOrDefault<T>(this Result<T> result, T defaultValue = default!)
    {
        return result.IsSuccess ? result.Value! : defaultValue;
    }

    /// <summary>
    /// Tries to get the value or throws an exception if the result failed.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The source result.</param>
    /// <returns>The result value if successful.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is not successful.</exception>
    public static T GetValueOrThrow<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? result.Value!
            : throw new InvalidOperationException(result.ErrorMessage ?? "Operation failed with unknown error");
    }

    /// <summary>
    /// Tries to get the error details as a tuple.
    /// </summary>
    /// <param name="result">The source result.</param>
    /// <returns>A tuple containing the error message and error code.</returns>
    public static (string? ErrorMessage, string? ErrorCode) GetError(this Result result)
    {
        return (result.ErrorMessage, result.ErrorCode);
    }

    /// <summary>
    /// Tries to get the error details as a tuple.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The source result.</param>
    /// <returns>A tuple containing the error message and error code.</returns>
    public static (string? ErrorMessage, string? ErrorCode) GetError<T>(this Result<T> result)
    {
        return (result.ErrorMessage, result.ErrorCode);
    }

    /// <summary>
    /// Creates a result from a boolean condition.
    /// </summary>
    /// <param name="condition">The boolean condition to evaluate.</param>
    /// <param name="errorMessage">The error message to use if the condition is false.</param>
    /// <param name="errorCode">Optional error code for the failure case.</param>
    /// <returns>A successful result if the condition is true; otherwise a failure result.</returns>
    public static Result FromCondition(bool condition, string errorMessage, string? errorCode = null)
    {
        return condition
            ? Result.Success()
            : Result.Failure(errorMessage, errorCode);
    }

    /// <summary>
    /// Creates a result from a boolean condition with a value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="condition">The boolean condition to evaluate.</param>
    /// <param name="valueFactory">The factory function to create the value if the condition is true.</param>
    /// <param name="errorMessage">The error message to use if the condition is false.</param>
    /// <param name="errorCode">Optional error code for the failure case.</param>
    /// <returns>A successful result with the value if the condition is true; otherwise a failure result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="valueFactory"/> is <see langword="null"/>.</exception>
    public static Result<T> FromCondition<T>(bool condition, Func<T> valueFactory, string errorMessage, string? errorCode = null)
    {
        ArgumentNullException.ThrowIfNull(valueFactory);

        return condition
            ? Result<T>.Success(valueFactory())
            : Result<T>.Failure(errorMessage, errorCode);
    }
}