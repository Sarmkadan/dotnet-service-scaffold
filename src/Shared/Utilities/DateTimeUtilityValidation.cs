#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Provides validation helpers for date/time values and operations.
/// Validates DateTime values and parameters used with <see cref="DateTimeUtility"/> methods.
/// </summary>
public static class DateTimeUtilityValidation
{
    /// <summary>
    /// Validates a DateTime value to ensure it is not a default/unspecified date.
    /// </summary>
    /// <param name="value">The DateTime value to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails (should not happen as this is a validation method).</exception>
    public static IReadOnlyList<string> ValidateDateTime(DateTime value)
    {
        var errors = new List<string>();

        if (value == default)
        {
            errors.Add("DateTime value cannot be default (DateTime.MinValue).");
        }

        if (value.Kind == DateTimeKind.Unspecified)
        {
            errors.Add("DateTime value must have a specified kind (UTC or Local).");
        }

        return errors.AsReadOnly();
    }


    /// <summary>
    /// Validates a nullable DateTime value.
    /// </summary>
    /// <param name="value">The nullable DateTime value to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid or null.</returns>
    public static IReadOnlyList<string> ValidateDateTime(DateTime? value)
    {
        if (value is null)
        {
            return Array.Empty<string>();
        }

        return ValidateDateTime(value.Value);
    }

    /// <summary>
    /// Validates a string value for ISO 8601 duration format.
    /// </summary>
    /// <param name="value">The ISO 8601 duration string to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid or null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateDuration(string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            return new[] { "Duration string cannot be null or whitespace." };
        }

        try
        {
            // Try to parse the duration to validate the format
            _ = DateTimeUtility.ParseIsoDuration(value);
            return Array.Empty<string>();
        }
        catch (FormatException)
        {
            return new[] { $"Duration string '{value}' is not a valid ISO 8601 duration format. Expected format: PnYnMnDTnHnMnS (e.g., P3DT4H5M6S)." };
        }
        catch (OverflowException)
        {
            return new[] { $"Duration string '{value}' contains values that are too large." };
        }
    }

    /// <summary>
    /// Validates a birth date for age calculation.
    /// Ensures the date is not in the future and is a valid DateTime.
    /// </summary>
    /// <param name="birthDate">The birth date to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentException"><paramref name="birthDate"/> results in validation errors.</exception>
    public static IReadOnlyList<string> ValidateBirthDate(DateTime birthDate)
    {
        var errors = new List<string>();

        errors.AddRange(ValidateDateTime((DateTime?)birthDate));

        if (birthDate > DateTime.UtcNow)
        {
            errors.Add("Birth date cannot be in the future.");
        }

        if (DateTimeUtility.CalculateAge(birthDate) < 0)
        {
            errors.Add("Birth date results in a negative age, which is not valid.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a reference date for comparison operations.
    /// </summary>
    /// <param name="referenceDate">The reference date to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid or null.</returns>
    public static IReadOnlyList<string> ValidateReferenceDate(DateTime? referenceDate)
    {
        if (referenceDate is null)
        {
            return Array.Empty<string>();
        }

        return ValidateDateTime(referenceDate.Value);
    }

    /// <summary>
    /// Determines whether a DateTime value is valid (not default, has specified kind).
    /// </summary>
    /// <param name="value">The DateTime value to check.</param>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public static bool IsValidDateTime(DateTime value) => ValidateDateTime(value).Count == 0;

    /// <summary>
    /// Determines whether a nullable DateTime value is valid.
    /// </summary>
    /// <param name="value">The nullable DateTime value to check.</param>
    /// <returns>True if the value is valid or null; otherwise, false.</returns>
    public static bool IsValidDateTime(DateTime? value) => ValidateDateTime(value).Count == 0;

    /// <summary>
    /// Determines whether a string duration value is valid.
    /// </summary>
    /// <param name="value">The duration string to check.</param>
    /// <returns>True if the string is valid or null; otherwise, false.</returns>
    public static bool IsValidDuration(string? value) => ValidateDuration(value).Count == 0;

    /// <summary>
    /// Determines whether a birth date is valid for age calculation.
    /// </summary>
    /// <param name="birthDate">The birth date to check.</param>
    /// <returns>True if the birth date is valid; otherwise, false.</returns>
    public static bool IsValidBirthDate(DateTime birthDate) => ValidateBirthDate(birthDate).Count == 0;

    /// <summary>
    /// Ensures that a DateTime value is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="value">The DateTime value to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the value is not valid.</exception>
    public static void EnsureValidDateTime(DateTime value)
    {
        var errors = ValidateDateTime(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"DateTime value is not valid. Validation errors:\n - {
                string.Join("\n - ", errors)
            }");
        }
    }

    /// <summary>
    /// Ensures that a nullable DateTime value is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="value">The nullable DateTime value to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the value is not valid.</exception>
    public static void EnsureValidDateTime(DateTime? value)
    {
        var errors = ValidateDateTime(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Nullable DateTime value is not valid. Validation errors:\n - {
                string.Join("\n - ", errors)
            }");
        }
    }

    /// <summary>
    /// Ensures that a string duration value is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="value">The duration string to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the value is not valid.</exception>
    public static void EnsureValidDuration(string? value)
    {
        var errors = ValidateDuration(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Duration string is not valid. Validation errors:\n - {
                string.Join("\n - ", errors)
            }");
        }
    }

    /// <summary>
    /// Ensures that a birth date is valid for age calculation, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="birthDate">The birth date to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the value is not valid.</exception>
    public static void EnsureValidBirthDate(DateTime birthDate)
    {
        var errors = ValidateBirthDate(birthDate);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Birth date is not valid. Validation errors:\n - {
                string.Join("\n - ", errors)
            }");
        }
    }
}