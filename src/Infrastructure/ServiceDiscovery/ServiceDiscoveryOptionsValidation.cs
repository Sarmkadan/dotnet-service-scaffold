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
            problems.Add($"ServiceDiscovery.CacheTtl must be positive, but was {value.CacheTtl.TotalSeconds}s.");
        }

        if (value.RefreshInterval <= TimeSpan.Zero)
        {
            problems.Add($"ServiceDiscovery.RefreshInterval must be positive, but was {value.RefreshInterval.TotalSeconds}s.");
        }

        if (value.ResolutionTimeout <= TimeSpan.Zero)
        {
            problems.Add($"ServiceDiscovery.ResolutionTimeout must be positive, but was {value.ResolutionTimeout.TotalSeconds}s.");
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
            problems.Add("ServiceDiscovery.Dns.SearchDomain must not be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(value.DnsServerAddress))
        {
            problems.Add("ServiceDiscovery.Dns.DnsServerAddress must not be null or whitespace.");
        }
        else if (!IsValidIpAddress(value.DnsServerAddress))
        {
            problems.Add($"ServiceDiscovery.Dns.DnsServerAddress must be a valid IP address, but was '{value.DnsServerAddress}'.");
        }

        if (value.DnsServerPort is < 1 or > 65535)
        {
            problems.Add($"ServiceDiscovery.Dns.DnsServerPort must be between 1 and 65535, but was {value.DnsServerPort}.");
        }

        if (value.DefaultPort is < 1 or > 65535)
        {
            problems.Add($"ServiceDiscovery.Dns.DefaultPort must be between 1 and 65535, but was {value.DefaultPort}.");
        }

        if (string.IsNullOrWhiteSpace(value.DefaultScheme))
        {
            problems.Add("ServiceDiscovery.Dns.DefaultScheme must not be null or whitespace.");
        }
        else if (!value.DefaultScheme.Equals("http", StringComparison.OrdinalIgnoreCase) &&
                 !value.DefaultScheme.Equals("https", StringComparison.OrdinalIgnoreCase))
        {
            problems.Add($"ServiceDiscovery.Dns.DefaultScheme must be 'http' or 'https', but was '{value.DefaultScheme}'.");
        }

        if (value.MaxRetries < 0)
        {
            problems.Add($"ServiceDiscovery.Dns.MaxRetries must be non-negative, but was {value.MaxRetries}.");
        }

        if (value.SocketTimeout <= TimeSpan.Zero)
        {
            problems.Add($"ServiceDiscovery.Dns.SocketTimeout must be positive, but was {value.SocketTimeout.TotalSeconds}s.");
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
            problems.Add("ServiceDiscovery.Registry.AgentEndpoint must not be null or whitespace.");
        }
        else if (!Uri.TryCreate(value.AgentEndpoint, UriKind.Absolute, out _))
        {
            problems.Add($"ServiceDiscovery.Registry.AgentEndpoint must be a valid absolute URI, but was '{value.AgentEndpoint}'.");
        }
        else if (!value.AgentEndpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                 !value.AgentEndpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            problems.Add("ServiceDiscovery.Registry.AgentEndpoint must use 'http://' or 'https://' scheme.");
        }

        if (value.HeartbeatInterval <= TimeSpan.Zero)
        {
            problems.Add($"ServiceDiscovery.Registry.HeartbeatInterval must be positive, but was {value.HeartbeatInterval.TotalSeconds}s.");
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
                problems.Add("ServiceDiscovery.SelfRegistration.ServiceName must not be null or whitespace when self-registration is enabled.");
            }

            if (value.AdvertisePort is < 1 or > 65535)
            {
                problems.Add($"ServiceDiscovery.SelfRegistration.AdvertisePort must be between 1 and 65535, but was {value.AdvertisePort}.");
            }

            if (string.IsNullOrWhiteSpace(value.AdvertiseScheme))
            {
                problems.Add("ServiceDiscovery.SelfRegistration.AdvertiseScheme must not be null or whitespace when self-registration is enabled.");
            }
            else if (!value.AdvertiseScheme.Equals("http", StringComparison.OrdinalIgnoreCase) &&
                     !value.AdvertiseScheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                problems.Add($"ServiceDiscovery.SelfRegistration.AdvertiseScheme must be 'http' or 'https', but was '{value.AdvertiseScheme}'.");
            }

            if (string.IsNullOrWhiteSpace(value.HealthCheckPath))
            {
                problems.Add("ServiceDiscovery.SelfRegistration.HealthCheckPath must not be null or whitespace when self-registration is enabled.");
            }
            else if (!value.HealthCheckPath.StartsWith('/'))
            {
                problems.Add($"ServiceDiscovery.SelfRegistration.HealthCheckPath must start with '/', but was '{value.HealthCheckPath}'.");
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
                $"ServiceDiscoveryOptions is invalid. Problems: {string.Join(" ", problems)}");
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
                $"DnsDiscoveryOptions is invalid. Problems: {string.Join(" ", problems)}");
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
                $"RegistryDiscoveryOptions is invalid. Problems: {string.Join(" ", problems)}");
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
                $"SelfRegistrationOptions is invalid. Problems: {string.Join(" ", problems)}");
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