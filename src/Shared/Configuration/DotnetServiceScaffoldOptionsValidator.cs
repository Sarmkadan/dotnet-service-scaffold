#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Shared.Configuration;

/// <summary>
/// Cross-field validation for <see cref="DotnetServiceScaffoldOptions"/>. Single-property rules
/// are DataAnnotations on the class; this validator covers rules that compare several properties.
/// </summary>
public sealed class DotnetServiceScaffoldOptionsValidator : IValidateOptions<DotnetServiceScaffoldOptions>
{
    private static readonly string[] AllowedMetricsProtectionModes =
    [
        DotnetServiceScaffoldOptionsConstants.MetricsProtectionModeDisabled,
        DotnetServiceScaffoldOptionsConstants.MetricsProtectionModeApiKey,
        DotnetServiceScaffoldOptionsConstants.MetricsProtectionModeLocalhostOnly
    ];

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, DotnetServiceScaffoldOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (options.HealthCheckTimeout >= options.HealthCheckInterval)
        {
            failures.Add(
                $"HealthCheckTimeout ({options.HealthCheckTimeout}s) must be less than HealthCheckInterval ({options.HealthCheckInterval}s), " +
                "so that a health check finishes before the next one starts.");
        }

        if (options.DefaultPageSize > options.MaxPageSize)
        {
            failures.Add(
                $"DefaultPageSize ({options.DefaultPageSize}) must not exceed MaxPageSize ({options.MaxPageSize}).");
        }

        ValidateMetricsProtection(options, failures);

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    private static void ValidateMetricsProtection(DotnetServiceScaffoldOptions options, List<string> failures)
    {
        var mode = AllowedMetricsProtectionModes.FirstOrDefault(
            allowed => allowed.Equals(options.MetricsProtectionMode, StringComparison.OrdinalIgnoreCase));

        if (mode is null)
        {
            failures.Add(
                $"MetricsProtectionMode must be one of {string.Join(", ", AllowedMetricsProtectionModes)}, but was '{options.MetricsProtectionMode}'.");
            return;
        }

        if (mode == DotnetServiceScaffoldOptionsConstants.MetricsProtectionModeApiKey &&
            string.IsNullOrWhiteSpace(options.MetricsApiKey))
        {
            failures.Add("MetricsApiKey is required when MetricsProtectionMode is 'ApiKey'.");
        }
    }
}
