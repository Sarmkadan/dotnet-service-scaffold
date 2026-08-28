#nullable enable

using System;

namespace DotnetServiceScaffold.Tests;

/// <summary>
/// Interface for HttpClientFactoryTests
/// </summary>
public interface IHttpClientFactoryTests : IDisposable
{
    void CreateClient_WithDefaultName_ReturnsConfiguredHttpClient();
    void CreateClient_WithCustomName_ReturnsConfiguredHttpClient();
    void CreateClient_WhenUserAgentHeaderAlreadyExists_DoesNotOverride();
    void CreateAuthenticatedClient_WithValidApiKey_AddsApiKeyHeader();
    void CreateAuthenticatedClient_WithCustomName_UsesCustomName();
    void CreateBearerClient_WithValidToken_AddsAuthorizationHeader();
    void CreateBearerClient_WithCustomName_UsesCustomName();
    void CreateClientWithBaseUrl_WithValidUrl_SetsBaseAddress();
    void CreateClientWithBaseUrl_WithCustomName_UsesCustomName();
    void CreateClientWithBaseUrl_WithTrailingSlash_HandlesCorrectly();
    void CreateBearerClient_WithNullToken_ThrowsArgumentException();
    void CreateBearerClient_WithEmptyToken_ThrowsArgumentException();
    void CreateBearerClient_WithWhitespaceToken_ThrowsArgumentException();
    void CreateClientWithBaseUrl_WithNullBaseUrl_ThrowsArgumentNullException();
    void CreateClientWithBaseUrl_WithEmptyBaseUrl_ThrowsUriFormatException();
    void CreateClientWithBaseUrl_WithInvalidUrl_ThrowsUriFormatException();
    void CreateClient_WithMultipleCalls_ReturnsIndependentClients();
    void CreateAuthenticatedClient_AfterCreateClient_HasBothHeaders();
}