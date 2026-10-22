#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.Metrics;

/// <summary>
/// Validates <see cref="MetricsOptions"/> at startup. Every rule is evaluated and all
/// failures are reported together. <c>Port</c> and <c>Prefix</c> are checked even when
/// <c>Enabled</c> is <see langword="false"/>, so a bad value is caught before it is enabled.
/// </summary>
public sealed class MetricsOptionsValidator : IValidateOptions<MetricsOptions>
{
    private static readonly Regex PrefixRegex = new(MetricsOptionsConstants.PrefixPattern, RegexOptions.CultureInvariant);

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, MetricsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (options.Port < MetricsOptionsConstants.MinPort || options.Port > MetricsOptionsConstants.MaxPort)
        {
            failures.Add(
                $"Metrics:Port must be between {MetricsOptionsConstants.MinPort} and {MetricsOptionsConstants.MaxPort}, but was {options.Port}.");
        }

        if (string.IsNullOrEmpty(options.Prefix) || !PrefixRegex.IsMatch(options.Prefix))
        {
            failures.Add(
                $"Metrics:Prefix must match '{MetricsOptionsConstants.PrefixPattern}' " +
                $"(lowercase letters, digits and underscores, not starting with a digit), but was '{options.Prefix}'.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
