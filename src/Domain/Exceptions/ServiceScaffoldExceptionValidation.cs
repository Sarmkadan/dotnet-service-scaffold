#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace DotnetServiceScaffold.Domain.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="ServiceScaffoldException"/> and derived exception types.
/// </summary>
public static class ServiceScaffoldExceptionValidation
{
    /// <summary>
    /// Validates a <see cref="ServiceScaffoldException"/> instance.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ServiceScaffoldException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate ErrorCode
        if (string.IsNullOrWhiteSpace(value.ErrorCode))
        {
            problems.Add(ServiceScaffoldExceptionValidationConstants.ErrorCodeMustNotBeNullOrWhitespace);
        }
        else if (value.ErrorCode.Length > ServiceScaffoldExceptionValidationConstants.ErrorCodeMaxLength)
        {
            problems.Add(string.Format(ServiceScaffoldExceptionValidationConstants.ErrorCodeMustBeMaxLength, ServiceScaffoldExceptionValidationConstants.ErrorCodeMaxLength));
        }

        // Validate Message
        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add(ServiceScaffoldExceptionValidationConstants.MessageMustNotBeNullOrWhitespace);
        }
        else if (value.Message.Length > ServiceScaffoldExceptionValidationConstants.MessageMaxLength)
        {
            problems.Add(string.Format(ServiceScaffoldExceptionValidationConstants.MessageMustBeMaxLength, ServiceScaffoldExceptionValidationConstants.MessageMaxLength));
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ServiceScaffoldException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ServiceScaffoldException? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ServiceScaffoldException"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid, containing the validation problems.</exception>
    public static void EnsureValid(this ServiceScaffoldException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                string.Format(ServiceScaffoldExceptionValidationConstants.ServiceScaffoldExceptionInvalidFormat, string.Join(" ", problems)),
                nameof(value));
        }
    }

    /// <summary>
    /// Validates a <see cref="ServiceNotFoundException"/> instance.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ServiceNotFoundException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>(Validate((ServiceScaffoldException)value));

        // ServiceNotFoundException has no additional validation beyond base
        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ServiceNotFoundException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ServiceNotFoundException? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ServiceNotFoundException"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this ServiceNotFoundException? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureValid((ServiceScaffoldException)value);
    }

    /// <summary>
    /// Validates a <see cref="ServiceValidationException"/> instance.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ServiceValidationException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>(Validate((ServiceScaffoldException)value));

        // Validate Errors collection
        if (value.Errors is null)
        {
            problems.Add(ServiceScaffoldExceptionValidationConstants.ErrorsCollectionMustNotBeNull);
        }
        else if (value.Errors.Count == 0)
        {
            problems.Add(ServiceScaffoldExceptionValidationConstants.ErrorsCollectionMustContainAtLeastOneError);
        }
        else
        {
            for (int i = 0; i < value.Errors.Count; i++)
            {
                var error = value.Errors[i];
                if (string.IsNullOrWhiteSpace(error))
                {
                    problems.Add(string.Format(ServiceScaffoldExceptionValidationConstants.ErrorsItemMustNotBeNullOrWhitespace, i));
                }
                else if (error.Length > ServiceScaffoldExceptionValidationConstants.ErrorsItemMaxLength)
                {
                    problems.Add(string.Format(ServiceScaffoldExceptionValidationConstants.ErrorsItemMustBeMaxLength, i, ServiceScaffoldExceptionValidationConstants.ErrorsItemMaxLength));
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ServiceValidationException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ServiceValidationException? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ServiceValidationException"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid, containing the validation problems.</exception>
    public static void EnsureValid(this ServiceValidationException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                string.Format(ServiceScaffoldExceptionValidationConstants.ServiceValidationExceptionInvalidFormat, string.Join(" ", problems)),
                nameof(value));
        }
    }

    /// <summary>
    /// Validates a <see cref="HealthCheckException"/> instance.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this HealthCheckException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>(Validate((ServiceScaffoldException)value));

        // HealthCheckException has no additional validation beyond base
        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="HealthCheckException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this HealthCheckException? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="HealthCheckException"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this HealthCheckException? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureValid((ServiceScaffoldException)value);
    }

    /// <summary>
    /// Validates a <see cref="UnauthorizedException"/> instance.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UnauthorizedException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>(Validate((ServiceScaffoldException)value));

        // UnauthorizedException has no additional validation beyond base
        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="UnauthorizedException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this UnauthorizedException? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="UnauthorizedException"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this UnauthorizedException? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureValid((ServiceScaffoldException)value);
    }

    /// <summary>
    /// Validates a <see cref="InvalidApiKeyException"/> instance.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this InvalidApiKeyException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>(Validate((ServiceScaffoldException)value));

        // InvalidApiKeyException has no additional validation beyond base
        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="InvalidApiKeyException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this InvalidApiKeyException? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="InvalidApiKeyException"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this InvalidApiKeyException? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureValid((ServiceScaffoldException)value);
    }

    /// <summary>
    /// Validates a <see cref="DataAccessException"/> instance.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DataAccessException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>(Validate((ServiceScaffoldException)value));

        // DataAccessException has no additional validation beyond base
        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="DataAccessException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this DataAccessException? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="DataAccessException"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this DataAccessException? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureValid((ServiceScaffoldException)value);
    }

    /// <summary>
    /// Validates a <see cref="ConfigurationException"/> instance.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ConfigurationException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>(Validate((ServiceScaffoldException)value));

        // ConfigurationException has no additional validation beyond base
        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ConfigurationException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ConfigurationException? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ConfigurationException"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this ConfigurationException? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureValid((ServiceScaffoldException)value);
    }

    /// <summary>
    /// Validates a <see cref="ResourceExhaustedException"/> instance.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ResourceExhaustedException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>(Validate((ServiceScaffoldException)value));

        // ResourceExhaustedException has no additional validation beyond base
        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ResourceExhaustedException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ResourceExhaustedException? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ResourceExhaustedException"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this ResourceExhaustedException? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureValid((ServiceScaffoldException)value);
    }
}