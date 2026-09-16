namespace DotnetServiceScaffold.Presentation.Middleware;

/// <summary>
/// Constants for API key authentication middleware.
/// </summary>
internal static class ApiKeyAuthenticationMiddlewareConstants
{
    public const string ApiKeyHeaderName = "X-Api-Key";
    public const string InvalidApiKeyError = "Invalid API key";
    public const string ApiKeyValidationError = "API key validation failed";
    public const string ApiKeyValidationLogMessage = "Error validating API key";
    public const string JsonContentType = "application/json";
    public const string UnauthorizedError = "Unauthorized";
    public const string ApiKeyRequiredMessage = "API key is required. Provide it in the X-Api-Key header.";
    public const string DefaultScheme = "ApiKey";

    public const int StatusCodeUnauthorized = 401;
}
