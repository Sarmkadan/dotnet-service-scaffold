#nullable enable

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Provides validation helpers for <see cref="ServiceDiscoveryOptions"/> and related configuration classes.
/// </summary>
public static class ServiceDiscoveryOptionsValidation
{
    /// <summary>
    /// Validates the provided <see cref="ServiceDiscoveryOptions"/> instance.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <returns>An immutable list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ServiceDiscoveryOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate ServiceDiscoveryOptions properties
        if (value.CacheTtl <= TimeSpan.Zero)
        {
            problems.Add(string.Format(ServiceDiscoveryOptionsValidationConstants.CacheTtlMustBePositive, value.CacheTtl.TotalSeconds));
        }

        if (value.RefreshInterval <= TimeSpan.Zero)
        {
            problems.Add(string.Format(ServiceDiscoveryOptionsValidationConstants.RefreshIntervalMustBePositive, value.RefreshInterval.TotalSeconds));
        }

        if (value.ResolutionTimeout <= TimeSpan.Zero)
        {
            problems.Add(string.Format(ServiceDiscoveryOptionsValidationConstants.ResolutionTimeoutMustBePositive, value.ResolutionTimeout.TotalSeconds));
        }

        // Validate nested DnsDiscoveryOptions
        problems.AddRange(value.Dns.Validate());

        // Validate nested RegistryDiscoveryOptions
        problems.AddRange(value.Registry.Validate());

        // Validate nested SelfRegistrationOptions
        problems.AddRange(value.SelfRegistration.Validate());

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="DnsDiscoveryOptions"/> instance.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <returns>An immutable list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this DnsDiscoveryOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.SearchDomain))
        {
            problems.Add(ServiceDiscoveryOptionsValidationConstants.DnsSearchDomainMustNotBeNullOrWhiteSpace);
        }

        if (string.IsNullOrWhiteSpace(value.DnsServerAddress))
        {
            problems.Add(ServiceDiscoveryOptionsValidationConstants.DnsServerAddressMustNotBeNullOrWhiteSpace);
        }
        else if (!IsValidIpAddress(value.DnsServerAddress))
        {
            problems.Add(string.Format(ServiceDiscoveryOptionsValidationConstants.DnsServerAddressMustBeValidIpAddress, value.DnsServerAddress));
        }

        if (value.DnsServerPort is < ServiceDiscoveryOptionsValidationConstants.MinPortValue or > ServiceDiscoveryOptionsValidationConstants.MaxPortValue)
        {
            problems.Add(string.Format(ServiceDiscoveryOptionsValidationConstants.DnsServerPortMustBeInRange, value.DnsServerPort));
        }

        if (value.DefaultPort is < ServiceDiscoveryOptionsValidationConstants.MinPortValue or > ServiceDiscoveryOptionsValidationConstants.MaxPortValue)
        {
            problems.Add(string.Format(ServiceDiscoveryOptionsValidationConstants.DefaultPortMustBeInRange, value.DefaultPort));
        }

        if (string.IsNullOrWhiteSpace(value.DefaultScheme))
        {
            problems.Add(ServiceDiscoveryOptionsValidationConstants.DefaultSchemeMustNotBeNullOrWhiteSpace);
        }
        else if (!value.DefaultScheme.Equals(ServiceDiscoveryOptionsValidationConstants.HttpScheme, StringComparison.OrdinalIgnoreCase) &&
                 !value.DefaultScheme.Equals(ServiceDiscoveryOptionsValidationConstants.HttpsScheme, StringComparison.OrdinalIgnoreCase))
        {
            problems.Add(string.Format(ServiceDiscoveryOptionsValidationConstants.DefaultSchemeMustBeHttpOrHttps, value.DefaultScheme));
        }

        if (value.MaxRetries < ServiceDiscoveryOptionsValidationConstants.MinRetriesValue)
        {
            problems.Add(string.Format(ServiceDiscoveryOptionsValidationConstants.MaxRetriesMustBeNonNegative, value.MaxRetries));
        }

        if (value.SocketTimeout <= TimeSpan.Zero)
        {
            problems.Add(string.Format(ServiceDiscoveryOptionsValidationConstants.SocketTimeoutMustBePositive, value.SocketTimeout.TotalSeconds));
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="RegistryDiscoveryOptions"/> instance.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <returns>An immutable list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this RegistryDiscoveryOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.AgentEndpoint))
        {
            problems.Add(ServiceDiscoveryOptionsValidationConstants.RegistryAgentEndpointMustNotBeNullOrWhiteSpace);
        }
        else if (!Uri.TryCreate(value.AgentEndpoint, UriKind.Absolute, out _))
        {
            problems.Add(string.Format(ServiceDiscoveryOptionsValidationConstants.RegistryAgentEndpointMustBeValidAbsoluteUri, value.AgentEndpoint));
        }
        else if (!value.AgentEndpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                 !value.AgentEndpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            problems.Add(ServiceDiscoveryOptionsValidationConstants.RegistryAgentEndpointMustUseHttpOrHttpsScheme);
        }

        if (value.HeartbeatInterval <= TimeSpan.Zero)
        {
            problems.Add(string.Format(ServiceDiscoveryOptionsValidationConstants.RegistryHeartbeatIntervalMustBePositive, value.HeartbeatInterval.TotalSeconds));
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="SelfRegistrationOptions"/> instance.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <returns>An immutable list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this SelfRegistrationOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.Enabled)
        {
            if (string.IsNullOrWhiteSpace(value.ServiceName))
            {
                problems.Add(ServiceDiscoveryOptionsValidationConstants.SelfRegistrationServiceNameMustNotBeNullOrWhiteSpace);
            }

            if (value.AdvertisePort is < ServiceDiscoveryOptionsValidationConstants.MinPortValue or > ServiceDiscoveryOptionsValidationConstants.MaxPortValue)
            {
                problems.Add(string.Format(ServiceDiscoveryOptionsValidationConstants.SelfRegistrationAdvertisePortMustBeInRange, value.AdvertisePort));
            }

            if (string.IsNullOrWhiteSpace(value.AdvertiseScheme))
            {
                problems.Add(ServiceDiscoveryOptionsValidationConstants.SelfRegistrationAdvertiseSchemeMustNotBeNullOrWhiteSpace);
            }
            else if (!value.AdvertiseScheme.Equals(ServiceDiscoveryOptionsValidationConstants.HttpScheme, StringComparison.OrdinalIgnoreCase) &&
                     !value.AdvertiseScheme.Equals(ServiceDiscoveryOptionsValidationConstants.HttpsScheme, StringComparison.OrdinalIgnoreCase))
            {
                problems.Add(string.Format(ServiceDiscoveryOptionsValidationConstants.SelfRegistrationAdvertiseSchemeMustBeHttpOrHttps, value.AdvertiseScheme));
            }

            if (string.IsNullOrWhiteSpace(value.HealthCheckPath))
            {
                problems.Add(ServiceDiscoveryOptionsValidationConstants.SelfRegistrationHealthCheckPathMustNotBeNullOrWhiteSpace);
            }
            else if (!value.HealthCheckPath.StartsWith('/'))
            {
                problems.Add(string.Format(ServiceDiscoveryOptionsValidationConstants.SelfRegistrationHealthCheckPathMustStartWithSlash, value.HealthCheckPath));
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the provided <see cref="ServiceDiscoveryOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this ServiceDiscoveryOptions value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the provided <see cref="ServiceDiscoveryOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of all validation problems.</exception>
    public static void EnsureValid(this ServiceDiscoveryOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                string.Format(ServiceDiscoveryOptionsValidationConstants.ServiceDiscoveryOptionsInvalid, string.Join(" ", problems)));
        }
    }

    /// <summary>
    /// Determines whether the provided <see cref="DnsDiscoveryOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this DnsDiscoveryOptions value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the provided <see cref="DnsDiscoveryOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of all validation problems.</exception>
    public static void EnsureValid(this DnsDiscoveryOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                string.Format(ServiceDiscoveryOptionsValidationConstants.DnsDiscoveryOptionsInvalid, string.Join(" ", problems)));
        }
    }

    /// <summary>
    /// Determines whether the provided <see cref="RegistryDiscoveryOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this RegistryDiscoveryOptions value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the provided <see cref="RegistryDiscoveryOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of all validation problems.</exception>
    public static void EnsureValid(this RegistryDiscoveryOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                string.Format(ServiceDiscoveryOptionsValidationConstants.RegistryDiscoveryOptionsInvalid, string.Join(" ", problems)));
        }
    }

    /// <summary>
    /// Determines whether the provided <see cref="SelfRegistrationOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this SelfRegistrationOptions value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the provided <see cref="SelfRegistrationOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of all validation problems.</exception>
    public static void EnsureValid(this SelfRegistrationOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                string.Format(ServiceDiscoveryOptionsValidationConstants.SelfRegistrationOptionsInvalid, string.Join(" ", problems)));
        }
    }

    /// <summary>
    /// Determines whether the provided string is a valid IPv4 or IPv6 address.
    /// </summary>
    /// <param name="address">The address string to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="address"/> is <see langword="null"/>.</exception>
    private static bool IsValidIpAddress(string address)
    {
        ArgumentNullException.ThrowIfNull(address);
        return System.Net.IPAddress.TryParse(address, out _);
    }
}