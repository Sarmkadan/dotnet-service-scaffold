#nullable enable

namespace DotnetServiceScaffold.Tests;

/// <summary>
/// Constants used in HttpClientFactoryTests.
/// </summary>
internal static class HttpClientFactoryTestsConstants
{
    public const string DefaultClientName = "default";
    public const string AuthenticatedClientName = "authenticated";
    public const string BearerClientName = "bearer";
    public const string CustomClientName = "custom-client";
    public const string ExistingClientName = "existing";
    public const string CustomAuthClientName = "custom-auth";
    public const string CustomBearerClientName = "custom-bearer";
    public const string CustomBaseClientName = "custom-base";
    public const string UserAgentHeaderName = "User-Agent";
    public const string UserAgentValue = "DotnetServiceScaffold/1.0";
    public const string ExistingAgentValue = "Existing-Agent/1.0";
    public const string TestApiKey = "test-api-key-12345";
    public const string TestApiKeyBase = "test-api-key";
    public const string TestKey = "test-key";
    public const string TestToken = "test-token";
    public const string TestBearerToken = "test-token-abc123";
    public const string EmptyString = "";
    public const string WhitespaceString = "   ";
    public const string InvalidUrlString = "not-a-valid-url";
    public const string TestClientName1 = "client1";
    public const string TestClientName2 = "client2";
    public const string ApiKeyHeaderName = "X-Api-Key";
    public const string AuthorizationHeaderName = "Authorization";
    public const string BearerScheme = "Bearer";
    public const string ExampleBaseUrl = "https://api.example.com";
    public const string ExampleBaseUrlWithTrailingSlash = "https://api.example.com/";
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
}
