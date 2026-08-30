#nullable enable

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Contains constant values used for HTTP utility validation.
/// </summary>
internal static class HttpUtilityValidationConstants
{
    // Length constants
    public const int MaxBasicAuthLength = 256;
    public const int MaxBearerTokenLength = 4096;
    public const int MaxPathLength = 2048;
    public const int MaxQueryParameterKeyLength = 1024;
    public const int MaxQueryParameterValueLength = 4096;
    public const int MaxContentTypeLength = 256;
    public const int MaxHeaderLength = 8192;
    public const int MaxQueryParameterCount = 100;
    public const int MaxRetryAttempt = 20;

    // Range constants
    public const int MinStatusCode = 100;
    public const int MaxStatusCode = 599;
    public const int MinPortNumber = 0;
    public const int MaxPortNumber = 65535;
    public const int MinRetryAttempt = 1;

    // String constants for validation messages
    public const string UsernameExceedsMaxLength = "Username exceeds maximum length of {0} characters.";
    public const string UsernameContainsColon = "Username contains colon character which is not allowed in Basic authentication.";
    public const string UsernameContainsNull = "Username contains null character which is not allowed.";
    public const string UsernameContainsNonAscii = "Username contains non-ASCII characters which may cause interoperability issues.";
    public const string PasswordExceedsMaxLength = "Password exceeds maximum length of {0} characters.";
    public const string PasswordContainsNull = "Password contains null character which is not allowed.";
    public const string BearerTokenExceedsMaxLength = "Bearer token exceeds maximum length of {0} characters.";
    public const string BearerTokenContainsNull = "Bearer token contains null character which is not allowed.";
    public const string BearerTokenContainsNonAscii = "Bearer token contains non-ASCII characters which may cause interoperability issues.";
    public const string StatusCodeOutOfRange = "Status code must be between {0} and {1} inclusive.";
    public const string BaseUrlInvalidScheme = "Base URL must use http or https scheme.";
    public const string BaseUrlMissingHost = "Base URL must contain a valid host.";
    public const string BaseUrlInvalidPort = "Base URL contains invalid port number.";
    public const string BaseUrlInvalidFormat = "Base URL is not a valid URI format.";
    public const string PathExceedsMaxLength = "Path exceeds maximum length of {0} characters.";
    public const string PathContainsRelativeTraversal = "Path contains relative traversal which is not allowed.";
    public const string PathContainsNull = "Path contains null character which is not allowed.";
    public const string QueryParameterKeyExceedsMaxLength = "Query parameter key exceeds maximum length of {0} characters.";
    public const string QueryParameterValueExceedsMaxLength = "Query parameter value exceeds maximum length of {0} characters.";
    public const string QueryParametersContainNull = "Query parameters contain null character which is not allowed.";
    public const string QueryParametersExceedMaxCount = "Query parameters exceed maximum count of {0}.";
    public const string ContentTypeExceedsMaxLength = "Content-Type header exceeds maximum length of {0} characters.";
    public const string ContentTypeContainsNull = "Content-Type header contains null character which is not allowed.";
    public const string HeaderExceedsMaxLength = "Header exceeds maximum length of {0} characters.";
    public const string HeaderContainsNull = "Header contains null character which is not allowed.";
    public const string RetryAttemptTooLow = "Retry attempt must be {0} or greater.";
    public const string RetryAttemptTooHigh = "Retry attempt exceeds maximum of {0}.";
}