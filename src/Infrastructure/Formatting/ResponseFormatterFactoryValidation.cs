#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Provides validation helpers for <see cref="ResponseFormatterFactory"/> instances.
/// Validates the state and configuration of response formatter factories.
/// </summary>
public static class ResponseFormatterFactoryValidation
{
    /// <summary>
    /// Validates the specified response formatter factory.
    /// </summary>
    /// <param name="value">The response formatter factory to validate.</param>
    /// <returns>A list of validation problems; empty if the factory is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ResponseFormatterFactory value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate formatters dictionary
        if (value.GetSupportedMediaTypes().Count() == 0)
        {
            problems.Add("ResponseFormatterFactory has no registered formatters.");
        }

        // Validate default formatter
        try
        {
            var defaultFormatter = value.GetFormatter(null);
            if (defaultFormatter is null)
            {
                problems.Add("ResponseFormatterFactory default formatter is null.");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ResponseFormatterFactory default formatter retrieval failed: {ex.Message}");
        }

        // Validate individual formatters
        var formatters = value.GetSupportedMediaTypes().ToList();
        foreach (var mediaType in formatters)
        {
            if (string.IsNullOrWhiteSpace(mediaType))
            {
                problems.Add("ResponseFormatterFactory contains formatter with null or whitespace media type.");
                continue;
            }

            var formatter = value.GetFormatter(mediaType);
            if (formatter is null)
            {
                problems.Add($"ResponseFormatterFactory formatter for media type '{mediaType}' is null.");
            }

            if (!value.IsMediaTypeSupported(mediaType))
            {
                problems.Add($"ResponseFormatterFactory reports media type '{mediaType}' as not supported despite being registered.");
            }
        }

        // Check for duplicate media type handling
        var duplicateCheck = formatters
            .GroupBy(mt => mt, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .ToList();

        if (duplicateCheck.Count > 0)
        {
            problems.Add(
                $"ResponseFormatterFactory contains duplicate media type registrations: {string.Join(", ", duplicateCheck.Select(g => $"'{g.Key}'"))}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified response formatter factory is valid.
    /// </summary>
    /// <param name="value">The response formatter factory to check.</param>
    /// <returns><see langword="true"/> if the factory is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ResponseFormatterFactory value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified response formatter factory is valid.
    /// </summary>
    /// <param name="value">The response formatter factory to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the factory has validation problems.</exception>
    public static void EnsureValid(this ResponseFormatterFactory value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ResponseFormatterFactory is invalid. Problems:\n- {string.Join("\n- ", problems)}");
        }
    }
}