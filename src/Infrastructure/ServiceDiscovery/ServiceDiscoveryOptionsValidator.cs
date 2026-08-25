#nullable enable

using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Validates <see cref="ServiceDiscoveryOptions"/> at startup: endpoints must be
/// populated, ports within range, and timeouts positive and internally consistent.
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

        ValidateEndpoints(options, failures);
        ValidatePorts(options, failures);
        ValidateTimeouts(options, failures);

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    private static void ValidateEndpoints(ServiceDiscoveryOptions options, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(options.Dns.SearchDomain))
        {
            failures.Add($"{nameof(ServiceDiscoveryOptions.Dns)}.{nameof(DnsDiscoveryOptions.SearchDomain)} must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(options.Dns.DnsServerAddress))
        {
            failures.Add($"{nameof(ServiceDiscoveryOptions.Dns)}.{nameof(DnsDiscoveryOptions.DnsServerAddress)} must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(options.Registry.AgentEndpoint))
        {
            failures.Add($"{nameof(ServiceDiscoveryOptions.Registry)}.{nameof(RegistryDiscoveryOptions.AgentEndpoint)} must not be empty.");
        }

        if (options.SelfRegistration.Enabled && string.IsNullOrWhiteSpace(options.SelfRegistration.HealthCheckPath))
        {
            failures.Add(
                $"{nameof(ServiceDiscoveryOptions.SelfRegistration)}.{nameof(SelfRegistrationOptions.HealthCheckPath)} must not be empty when self-registration is enabled.");
        }
    }

    private static void ValidatePorts(ServiceDiscoveryOptions options, List<string> failures)
    {
        ValidatePortInRange(options.Dns.DnsServerPort, $"{nameof(ServiceDiscoveryOptions.Dns)}.{nameof(DnsDiscoveryOptions.DnsServerPort)}", failures);
        ValidatePortInRange(options.Dns.DefaultPort, $"{nameof(ServiceDiscoveryOptions.Dns)}.{nameof(DnsDiscoveryOptions.DefaultPort)}", failures);

        if (options.SelfRegistration.Enabled)
        {
            ValidatePortInRange(
                options.SelfRegistration.AdvertisePort,
                $"{nameof(ServiceDiscoveryOptions.SelfRegistration)}.{nameof(SelfRegistrationOptions.AdvertisePort)}",
                failures);
        }

        if (options.Dns.MaxRetries < 0)
        {
            failures.Add($"{nameof(ServiceDiscoveryOptions.Dns)}.{nameof(DnsDiscoveryOptions.MaxRetries)} must not be negative.");
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

    private static void ValidatePortInRange(int port, string optionName, List<string> failures)
    {
        if (port is < 1 or > 65535)
        {
            failures.Add($"{optionName} must be between 1 and 65535, but was '{port}'.");
        }
    }
}
