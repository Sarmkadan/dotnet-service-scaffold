#nullable enable

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Constants for RegistryServiceDiscoveryProvider.
/// </summary>
internal static class RegistryServiceDiscoveryProviderConstants
{
    // Query parameters
    public const string PassingQueryParam = "&passing=true";
    public const string DatacenterQueryParamHealthPrefix = "&dc=";
    public const string DatacenterQueryParamCatalogPrefix = "?dc=";

    // Consul API endpoints (base paths)
    public const string HealthServiceBasePath = "/v1/health/service/";
    public const string RegisterServiceEndpoint = "/v1/agent/service/register";
    public const string DeregisterServiceBasePath = "/v1/agent/service/deregister/";
    public const string StatusLeaderEndpoint = "/v1/status/leader";
    public const string CatalogServicesBasePath = "/v1/catalog/services";

    // Format strings
    public const string ZeroDecimalFormat = "0";

    // Units
    public const string SecondsUnit = "s";
    public const string MinutesUnit = "m";

    // Hardcoded Consul check values
    public const string CheckTimeout = "5s";
    public const string DeregisterCriticalServiceAfter = "1m";
}