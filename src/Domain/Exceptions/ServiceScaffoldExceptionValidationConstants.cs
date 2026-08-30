#nullable enable

namespace DotnetServiceScaffold.Domain.Exceptions;

/// <summary>
/// Constants for validation messages and limits in ServiceScaffoldException validation.
/// </summary>
internal static class ServiceScaffoldExceptionValidationConstants
{
    // ErrorCode validation
    public const int ErrorCodeMaxLength = 50;
    public const string ErrorCodeMustNotBeNullOrWhitespace = "ErrorCode must not be null or whitespace.";
    public const string ErrorCodeMustBeMaxLength = "ErrorCode must be {0} characters or less.";

    // Message validation
    public const int MessageMaxLength = 1000;
    public const string MessageMustNotBeNullOrWhitespace = "Message must not be null or whitespace.";
    public const string MessageMustBeMaxLength = "Message must be {0} characters or less.";

    // Errors collection validation
    public const string ErrorsCollectionMustNotBeNull = "Errors collection must not be null.";
    public const string ErrorsCollectionMustContainAtLeastOneError = "Errors collection must contain at least one error.";
    public const string ErrorsItemMustNotBeNullOrWhitespace = "Errors[{0}] must not be null or whitespace.";
    public const int ErrorsItemMaxLength = 500;
    public const string ErrorsItemMustBeMaxLength = "Errors[{0}] must be {1} characters or less.";

    // Exception validation prefixes
    public const string ServiceScaffoldExceptionInvalidFormat = "ServiceScaffoldException is invalid. Problems: {0}";
    public const string ServiceValidationExceptionInvalidFormat = "ServiceValidationException is invalid. Problems: {0}";
    public const string HealthCheckExceptionInvalidFormat = "HealthCheckException is invalid. Problems: {0}";
    public const string UnauthorizedExceptionInvalidFormat = "UnauthorizedException is invalid. Problems: {0}";
    public const string InvalidApiKeyExceptionInvalidFormat = "InvalidApiKeyException is invalid. Problems: {0}";
    public const string DataAccessExceptionInvalidFormat = "DataAccessException is invalid. Problems: {0}";
    public const string ConfigurationExceptionInvalidFormat = "ConfigurationException is invalid. Problems: {0}";
    public const string ResourceExhaustedExceptionInvalidFormat = "ResourceExhaustedException is invalid. Problems: {0}";
}