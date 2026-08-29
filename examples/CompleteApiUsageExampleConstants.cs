#nullable enable

internal static class CompleteApiUsageExampleConstants
{
    public const string DefaultBaseUrl = "http://localhost:5000";
    public const string ApiKeyHeader = "X-API-Key";
    public const string AuthorizationHeader = "Authorization";
    public const string JsonContentType = "application/json";
    public const string DefaultIpWhitelist = "0.0.0.0/0";

    public const string RegisterUserEndpoint = "/api/user/register";
    public const string LoginEndpoint = "/api/user/login";
    public const string CreateApiKeyEndpoint = "/api/apikey/create";
    public const string RegisterServiceEndpoint = "/api/service/register";
    public const string GetServicesEndpoint = "/api/service";
    public const string HealthCheckEndpointFormat = "/api/healthcheck/{0}/check";
    public const string GetHealthHistoryEndpointFormat = "/api/healthcheck/{0}/history?days={1}&limit={2}";
    public const string GetMetricsServiceEndpointFormat = "/api/metrics/service/{0}";
    public const string GetMetricsEndpoint = "/api/metrics";
    public const string GetAuditLogsEndpointFormat = "/api/auditlog?days={0}&limit={1}";
    public const string EnableServiceEndpointFormat = "/api/service/{0}/enable";
    public const string DisableServiceEndpointFormat = "/api/service/{0}/disable";
    public const string ChangePasswordEndpointFormat = "/api/user/{0}/change-password";

    public const int DefaultServiceListLimit = 50;
    public const int DefaultItemsLimit = 100;
    public const int DefaultHistoryDays = 7;
    public const int DefaultAuditLogDays = 30;
}