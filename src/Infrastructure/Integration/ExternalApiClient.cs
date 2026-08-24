#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using DotnetServiceScaffold.Shared.Utilities;
using Serilog;

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Generic HTTP client for calling external APIs. Handles JSON serialization,
/// error responses, and provides a clean interface for common HTTP operations.
/// Implements both IExternalReadClient and IExternalWriteClient interfaces.
/// </summary>
public class ExternalApiClient : IExternalReadClient, IExternalWriteClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiClient> _logger;

    public ExternalApiClient(HttpClient httpClient, ILogger<ExternalApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Performs a GET request and returns the deserialized response.
    /// </summary>
    public async Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        ValidationUtility.ValidateNotNullOrEmpty(url, nameof(url));

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        AddHeaders(request, headers);

        try
        {
            _logger.LogDebug("GET request to {Url}", HttpUtility.MaskSensitiveUrl(url));

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return await HandleResponse<T>(response, url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform GET request to {Url}", HttpUtility.MaskSensitiveUrl(url));
            throw;
        }
    }

    /// <summary>
    /// Performs a POST request and returns the deserialized response.
    /// </summary>
    public async Task<T?> PostAsync<T>(string url, object payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        ValidationUtility.ValidateNotNullOrEmpty(url, nameof(url));

        using var request = new HttpRequestMessage(HttpMethod.Post, url);

        var json = JsonSerializer.Serialize(payload);
        request.Content = new StringContent(json, System.Text.Encoding.UTF8, ExternalApiClientConstants.ContentTypeJson);

        AddHeaders(request, headers);

        try
        {
            _logger.LogDebug("POST request to {Url}", HttpUtility.MaskSensitiveUrl(url));

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return await HandleResponse<T>(response, url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform POST request to {Url}", HttpUtility.MaskSensitiveUrl(url));
            throw;
        }
    }

    /// <summary>
    /// Performs a PUT request and returns the deserialized response.
    /// </summary>
    public async Task<T?> PutAsync<T>(string url, object payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        ValidationUtility.ValidateNotNullOrEmpty(url, nameof(url));

        using var request = new HttpRequestMessage(HttpMethod.Put, url);

        var json = JsonSerializer.Serialize(payload);
        request.Content = new StringContent(json, System.Text.Encoding.UTF8, ExternalApiClientConstants.ContentTypeJson);

        AddHeaders(request, headers);

        try
        {
            _logger.LogDebug("PUT request to {Url}", HttpUtility.MaskSensitiveUrl(url));

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return await HandleResponse<T>(response, url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform PUT request to {Url}", HttpUtility.MaskSensitiveUrl(url));
            throw;
        }
    }

    /// <summary>
    /// Performs a DELETE request.
    /// </summary>
    public async Task<bool> DeleteAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        ValidationUtility.ValidateNotNullOrEmpty(url, nameof(url));

        using var request = new HttpRequestMessage(HttpMethod.Delete, url);

        AddHeaders(request, headers);

        try
        {
            _logger.LogDebug("DELETE request to {Url}", HttpUtility.MaskSensitiveUrl(url));

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform DELETE request to {Url}", HttpUtility.MaskSensitiveUrl(url));
            throw;
        }
    }

    /// <summary>
    /// Handles HTTP responses by checking status codes and deserializing JSON.
    /// </summary>
    private async Task<T?> HandleResponse<T>(HttpResponseMessage response, string url)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "HTTP request to {Url} failed with status {StatusCode}: {Response}",
                HttpUtility.MaskSensitiveUrl(url), response.StatusCode, content);

            throw new HttpRequestException(
                $"Request failed with status {response.StatusCode}: {content}",
                null,
                response.StatusCode);
        }

        if (string.IsNullOrEmpty(content))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(content);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize response from {Url}", HttpUtility.MaskSensitiveUrl(url));
            throw;
        }
    }

    /// <summary>
    /// Adds custom headers to the HTTP request.
    /// </summary>
    private void AddHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
    {
        if (headers is null)
            return;

        foreach (var header in headers)
        {
            request.Headers.Add(header.Key, header.Value);
        }
    }
}
