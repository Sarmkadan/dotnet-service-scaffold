#nullable enable

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Validates <see cref="ServiceDiscoveryOptions"/> at startup. Property rules on the nested
/// sections are DataAnnotations and are evaluated here, because ValidateDataAnnotations() does not
/// descend into nested objects. This validator adds the conditional and cross-field rules:
/// self-registration requirements, non-negative retries, positive timeouts and the eviction/stale ordering.
/// </summary>
public sealed class ServiceDiscoveryOptionsValidator : IValidateOptions<ServiceDiscoveryOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, ServiceDiscoveryOptions options)
    {
        if (options is null)
        {
            return ValidateOptionsResult.Fail("Service discovery options must not be null.");
        }

        var failures = new List<string>();

        ValidateNestedDataAnnotations(options.Dns, nameof(ServiceDiscoveryOptions.Dns), failures);
        ValidateNestedDataAnnotations(options.Registry, nameof(ServiceDiscoveryOptions.Registry), failures);
        ValidateSelfRegistration(options, failures);
        ValidateTimeouts(options, failures);

        if (options.Dns.MaxRetries < 0)
        {
            failures.Add($"{nameof(ServiceDiscoveryOptions.Dns)}.{nameof(DnsDiscoveryOptions.MaxRetries)} must not be negative.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    private static void ValidateSelfRegistration(ServiceDiscoveryOptions options, List<string> failures)
    {
        if (!options.SelfRegistration.Enabled)
        {
            return;
        }

        var prefix = nameof(ServiceDiscoveryOptions.SelfRegistration);
        ValidateNestedDataAnnotations(options.SelfRegistration, prefix, failures);

        if (string.IsNullOrWhiteSpace(options.SelfRegistration.HealthCheckPath))
        {
            failures.Add($"{prefix}.{nameof(SelfRegistrationOptions.HealthCheckPath)} must not be empty when self-registration is enabled.");
        }
    }

    private static void ValidateTimeouts(ServiceDiscoveryOptions options, List<string> failures)
    {
        ValidatePositiveTimeSpan(options.CacheTtl, nameof(ServiceDiscoveryOptions.CacheTtl), failures);
        ValidatePositiveTimeSpan(options.RefreshInterval, nameof(ServiceDiscoveryOptions.RefreshInterval), failures);
        ValidatePositiveTimeSpan(options.HeartbeatStaleThreshold, nameof(ServiceDiscoveryOptions.HeartbeatStaleThreshold), failures);
        ValidatePositiveTimeSpan(options.EvictionThreshold, nameof(ServiceDiscoveryOptions.EvictionThreshold), failures);
        ValidatePositiveTimeSpan(options.StaleEvictionInterval, nameof(ServiceDiscoveryOptions.StaleEvictionInterval), failures);
        ValidatePositiveTimeSpan(options.ResolutionTimeout, nameof(ServiceDiscoveryOptions.ResolutionTimeout), failures);
        ValidatePositiveTimeSpan(options.Dns.SocketTimeout, $"{nameof(ServiceDiscoveryOptions.Dns)}.{nameof(DnsDiscoveryOptions.SocketTimeout)}", failures);
        ValidatePositiveTimeSpan(options.Registry.HeartbeatInterval, $"{nameof(ServiceDiscoveryOptions.Registry)}.{nameof(RegistryDiscoveryOptions.HeartbeatInterval)}", failures);

        if (options.EvictionThreshold < options.HeartbeatStaleThreshold)
        {
            failures.Add(
                $"{nameof(ServiceDiscoveryOptions.EvictionThreshold)} ({options.EvictionThreshold}) must be greater than or equal to {nameof(ServiceDiscoveryOptions.HeartbeatStaleThreshold)} ({options.HeartbeatStaleThreshold}).");
        }
    }

    private static void ValidatePositiveTimeSpan(TimeSpan value, string optionName, List<string> failures)
    {
        if (value <= TimeSpan.Zero)
        {
            failures.Add($"{optionName} must be greater than zero, but was '{value}'.");
        }
    }

    /// <summary>
    /// Runs the DataAnnotations on a nested section. Each failure is prefixed with the section
    /// name, so a message reads as "Dns.SearchDomain must not be empty.".
    /// </summary>
    private static void ValidateNestedDataAnnotations(object section, string prefix, List<string> failures)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(section, new ValidationContext(section), results, validateAllProperties: true);

        foreach (var result in results)
        {
            failures.Add($"{prefix}.{result.ErrorMessage}");
        }
    }
}
