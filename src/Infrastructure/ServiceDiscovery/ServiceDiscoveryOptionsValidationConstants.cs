namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Contains constant values used in ServiceDiscoveryOptionsValidation.
/// </summary>
internal static class ServiceDiscoveryOptionsValidationConstants
{
    // ServiceDiscoveryOptions validation messages
    public const string CacheTtlMustBePositive = "ServiceDiscovery.CacheTtl must be positive, but was {0}s.";
    public const string RefreshIntervalMustBePositive = "ServiceDiscovery.RefreshInterval must be positive, but was {0}s.";
    public const string ResolutionTimeoutMustBePositive = "ServiceDiscovery.ResolutionTimeout must be positive, but was {0}s.";

    // DnsDiscoveryOptions validation messages
    public const string DnsSearchDomainMustNotBeNullOrWhiteSpace = "ServiceDiscovery.Dns.SearchDomain must not be null or whitespace.";
    public const string DnsServerAddressMustNotBeNullOrWhiteSpace = "ServiceDiscovery.Dns.DnsServerAddress must not be null or whitespace.";
    public const string DnsServerAddressMustBeValidIpAddress = "ServiceDiscovery.Dns.DnsServerAddress must be a valid IP address, but was '{0}'.";
    public const string DnsServerPortMustBeInRange = "ServiceDiscovery.Dns.DnsServerPort must be between 1 and 65535, but was {0}.";
    public const string DefaultPortMustBeInRange = "ServiceDiscovery.Dns.DefaultPort must be between 1 and 65535, but was {0}.";
    public const string DefaultSchemeMustNotBeNullOrWhiteSpace = "ServiceDiscovery.Dns.DefaultScheme must not be null or whitespace.";
    public const string DefaultSchemeMustBeHttpOrHttps = "ServiceDiscovery.Dns.DefaultScheme must be 'http' or 'https', but was '{0}'.";
    public const string MaxRetriesMustBeNonNegative = "ServiceDiscovery.Dns.MaxRetries must be non-negative, but was {0}.";
    public const string SocketTimeoutMustBePositive = "ServiceDiscovery.Dns.SocketTimeout must be positive, but was {0}s.";

    // RegistryDiscoveryOptions validation messages
    public const string RegistryAgentEndpointMustNotBeNullOrWhiteSpace = "ServiceDiscovery.Registry.AgentEndpoint must not be null or whitespace.";
    public const string RegistryAgentEndpointMustBeValidAbsoluteUri = "ServiceDiscovery.Registry.AgentEndpoint must be a valid absolute URI, but was '{0}'.";
    public const string RegistryAgentEndpointMustUseHttpOrHttpsScheme = "ServiceDiscovery.Registry.AgentEndpoint must use 'http://' or 'https://' scheme.";
    public const string RegistryHeartbeatIntervalMustBePositive = "ServiceDiscovery.Registry.HeartbeatInterval must be positive, but was {0}s.";

    // SelfRegistrationOptions validation messages
    public const string SelfRegistrationServiceNameMustNotBeNullOrWhiteSpace = "ServiceDiscovery.SelfRegistration.ServiceName must not be null or whitespace when self-registration is enabled.";
    public const string SelfRegistrationAdvertisePortMustBeInRange = "ServiceDiscovery.SelfRegistration.AdvertisePort must be between 1 and 65535, but was {0}.";
    public const string SelfRegistrationAdvertiseSchemeMustNotBeNullOrWhiteSpace = "ServiceDiscovery.SelfRegistration.AdvertiseScheme must not be null or whitespace when self-registration is enabled.";
    public const string SelfRegistrationAdvertiseSchemeMustBeHttpOrHttps = "ServiceDiscovery.SelfRegistration.AdvertiseScheme must be 'http' or 'https', but was '{0}'.";
    public const string SelfRegistrationHealthCheckPathMustNotBeNullOrWhiteSpace = "ServiceDiscovery.SelfRegistration.HealthCheckPath must not be null or whitespace when self-registration is enabled.";
    public const string SelfRegistrationHealthCheckPathMustStartWithSlash = "ServiceDiscovery.SelfRegistration.HealthCheckPath must start with '/', but was '{0}'.";

    // EnsureValid validation messages
    public const string ServiceDiscoveryOptionsInvalid = "ServiceDiscoveryOptions is invalid. Problems: {0}";
    public const string DnsDiscoveryOptionsInvalid = "DnsDiscoveryOptions is invalid. Problems: {0}";
    public const string RegistryDiscoveryOptionsInvalid = "RegistryDiscoveryOptions is invalid. Problems: {0}";
    public const string SelfRegistrationOptionsInvalid = "SelfRegistrationOptions is invalid. Problems: {0>";

    // Scheme constants
    public const string HttpScheme = "http";
    public const string HttpsScheme = "https";

    // Port range constants
    public const int MinPortValue = 1;
    public const int MaxPortValue = 65535;

    // Retry count constants
    public const int MinRetriesValue = 0;
}