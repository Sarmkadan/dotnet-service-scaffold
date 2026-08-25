namespace DotnetServiceScaffold.Infrastructure.Extensions;

/// <summary>
/// Constants used in ServiceCollectionExtensions to avoid magic strings and numbers.
/// </summary>
internal static class ServiceCollectionExtensionsConstants
{
    // Configuration section names
    public const string HttpClientConfigurationSection = "HttpClient";
    public const string ResilienceConfigurationSection = "Resilience";

    // HTTP header names
    public const string UserAgentHeaderName = "User-Agent";
    public const string ConsulTokenHeaderName = "X-Consul-Token";

    // Default User-Agent values
    public const string DefaultUserAgent = "DotnetServiceScaffold/1.0";
    public const string ServiceDiscoveryUserAgentPrefix = "dotnet-service-scaffold/";

    // HttpClient names
    public const string ExternalApiHttpClientName = "external-api";
    public const string WebhookHttpClientName = "webhook";

    // Timeouts and durations (in seconds unless specified)
    public const int WebhookTimeoutSeconds = 10;
    public const int ExternalApiTimeoutSeconds = 30;
    public const int HandlerLifetimeMinutes = 5;
    public const int ServiceDiscoveryAdditionalTimeoutSeconds = 2;

    // Rate limiting
    public const int AnonymousRequestsPerMinute = 60;
    public const int AuthenticatedRequestsPerMinute = 300;
}