using System;

internal static class BasicServiceSetupExampleConstants
{
    public const string DefaultBaseUrl = "http://localhost:5000";
    public const string ApiKeyHeader = "X-API-Key";
    public const string JsonContentType = "application/json";
    public const string RegisterEndpoint = "/api/service/register";
    public const string ListServicesEndpoint = "/api/service";
    public const int ListServicesLimit = 50;
    public const string EnableServiceEndpoint = "/api/service/{0}/enable";
    public const string DisableServiceEndpoint = "/api/service/{0}/disable";
    public const string GetServiceDetailsEndpoint = "/api/service/{0}";
    public const string JsonDataProperty = "data";
    public const string JsonIdProperty = "id";
    public const string JsonNameProperty = "name";
    public const string JsonDescriptionProperty = "description";
    public const string JsonStatusProperty = "status";
    public const string JsonHealthCheckUrlProperty = "healthCheckUrl";
    public const string JsonSuccessRateProperty = "successRate";
    public const string JsonLastCheckedAtProperty = "lastCheckedAt";
    public const string JsonIsEnabledProperty = "isEnabled";
}