#nullable enable
namespace DotnetServiceScaffold.Tests
{
    internal static class HttpClientFactoryValidationTestsConstants
    {
        // Error messages
        public const string ClientNameNullOrWhitespace = "Client name cannot be null, empty, or whitespace.";
        public const string ClientNameTooLong = "Client name cannot exceed 100 characters.";
        public const string ApiKeyNullOrWhitespace = "API key cannot be null, empty, or whitespace.";
        public const string ApiKeyTooLong = "API key cannot exceed 500 characters.";
        public const string BearerTokenNullOrWhitespace = "Bearer token cannot be null, empty, or whitespace.";
        public const string BearerTokenTooLong = "Bearer token cannot exceed 2000 characters.";
        public const string BaseUrlNullOrWhitespace = "Base URL cannot be null, empty, or whitespace.";
        public const string BaseUrlInvalidUri = "Base URL must be a valid absolute URI.";
        public const string BaseUrlWrongScheme = "Base URL must use http:// or https:// scheme.";

        // Maximum lengths
        public const int MaxClientNameLength = 100;
        public const int MaxApiKeyLength = 500;
        public const int MaxBearerTokenLength = 2000;
    }
}