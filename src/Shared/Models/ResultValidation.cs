#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotnetServiceScaffold.Shared.Models;

/// <summary>
/// Provides validation helpers for <see cref="Result"/> and <see cref="Result{T}"/> types.
/// Validates business rules, null/empty values, and domain constraints.
/// </summary>
public static class ResultValidation
{
    /// <summary>
    /// Validates a <see cref="Result"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The result to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Result? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (!value.IsSuccess)
        {
            if (string.IsNullOrWhiteSpace(value.ErrorMessage))
            {
                problems.Add("Failed result must have a non-empty ErrorMessage.");
            }

            if (value.ErrorCode is null)
            {
                problems.Add("Failed result must have a non-null ErrorCode.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="Result{T}"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The result to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate<T>(this Result<T>? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (!value.IsSuccess)
        {
            if (string.IsNullOrWhiteSpace(value.ErrorMessage))
            {
                problems.Add("Failed result must have a non-empty ErrorMessage.");
            }

            if (value.ErrorCode is null)
            {
                problems.Add("Failed result must have a non-null ErrorCode.");
            }
        }
        else
        {
            // Validate the actual value when result is successful
            if (value.Value is null)
            {
                // Null values are allowed for reference types in successful results
                // No validation error for null values in successful results
            }
            else if (value.Value is string str && string.IsNullOrWhiteSpace(str))
            {
                problems.Add("Successful result with string value must not be empty or whitespace.");
            }
            else if (value.Value is IFormattable && !typeof(T).IsValueType)
            {
                // For numeric types, dates, etc. - check for default values
                // Skip value types as they may legitimately have default values
                if (EqualityComparer<T>.Default.Equals(value.Value, default!))
                {
                    problems.Add($"Successful result must not contain default value of type {typeof(T).Name}.");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="Result"/> instance is valid.
    /// </summary>
    /// <param name="value">The result to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this Result? value) => Validate(value).Count == 0;

    /// <summary>
    /// Determines whether a <see cref="Result{T}"/> instance is valid.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The result to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid<T>(this Result<T>? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that a <see cref="Result"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The result to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the result is invalid, containing the validation problems.</exception>
    public static void EnsureValid(this Result? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Result validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that a <see cref="Result{T}"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The result to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the result is invalid, containing the validation problems.</exception>
    public static void EnsureValid<T>(this Result<T>? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Result<T> validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}
