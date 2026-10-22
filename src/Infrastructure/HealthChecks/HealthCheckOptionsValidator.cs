#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.HealthChecks;

/// <summary>
/// Validates <see cref="HealthCheckOptions"/> at startup. Every rule is evaluated and all
/// failures are reported together, so one startup error lists every problem in the section.
/// </summary>
public sealed class HealthCheckOptionsValidator : IValidateOptions<HealthCheckOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, HealthCheckOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (options.Interval <= TimeSpan.Zero)
        {
            failures.Add($"HealthCheck:Interval must be greater than zero, but was {options.Interval}.");
        }

        if (options.Timeout >= options.Interval)
        {
            failures.Add(
                $"HealthCheck:Timeout ({options.Timeout}) must be less than HealthCheck:Interval ({options.Interval}), " +
                "so that a health check finishes before the next one starts.");
        }

        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            failures.Add("HealthCheck:Endpoint must not be empty.");
        }
        else if (options.Endpoint[0] != HealthCheckOptionsConstants.EndpointPrefix)
        {
            failures.Add($"HealthCheck:Endpoint must start with '{HealthCheckOptionsConstants.EndpointPrefix}', but was '{options.Endpoint}'.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
