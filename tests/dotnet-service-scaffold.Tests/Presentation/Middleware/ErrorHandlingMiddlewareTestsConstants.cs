#nullable enable

internal static class ErrorHandlingMiddlewareTestsConstants
{
    public const string DevelopmentEnvironment = "Development";
    public const string ProductionEnvironment = "Production";
    public const string GenericErrorMessage = "An error occurred processing your request.";
    public const string ProductionErrorMessage = "An error occurred processing your request. Please contact support with the error ID.";
    public const string BadRequestMessage = "Bad request error";
    public const string ArgumentNullMessage = "Argument null error";
    public const string ArgumentMessage = "Invalid argument";
    public const string InvalidOperationMessage = "Operation not allowed";
    public const string KeyNotFoundMessage = "Resource not found";
    public const string SensitiveErrorDetails = "Sensitive error details";
}