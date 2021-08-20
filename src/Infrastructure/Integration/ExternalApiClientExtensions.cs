#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using DotnetServiceScaffold.Shared.Utilities;
using Serilog;

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Extension methods for <see cref="ExternalApiClient"/> providing additional convenience methods
/// for common API operations like retry logic, timeout handling, and bulk operations.
/// </summary>
public static class ExternalApiClientExtensions
{
    /// <summary>
    /// Performs a GET request with retry logic and timeout handling.
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="client">The API client instance</param>
    /// <param name="url">Request URL</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3)</param>
    /// <param name="timeoutSeconds">Request timeout in seconds (default: 30)</param>
    /// <param name="headers">Optional request headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response or null if not found</returns>
    public static async Task<T?> GetWithRetryAsync<T>(
        this ExternalApiClient client,
        string url,
        int maxRetries = 3,
        int timeoutSeconds = 30,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client));
        ValidationUtility.ValidateNotNullOrEmpty(url, nameof(url));
        if (maxRetries <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "maxRetries must be greater than 0");
        if (timeoutSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(timeoutSeconds), "timeoutSeconds must be greater than 0");

        var retryCount = 0;
        var lastException = (Exception?)null;

        while (retryCount <= maxRetries)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

                return await client.GetAsync<T>(url, headers, linkedCts.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastException = ex;
                retryCount++;

                if (retryCount > maxRetries)
                    break;

                // Exponential backoff
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount - 1)), cancellationToken);
            }
        }

        throw new HttpRequestException(
            $"GET request to {HttpUtility.MaskSensitiveUrl(url)} failed after {maxRetries} retries",
            lastException);
    }

    /// <summary>
    /// Performs a POST request with retry logic and timeout handling.
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="client">The API client instance</param>
    /// <param name="url">Request URL</param>
    /// <param name="payload">Request payload</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3)</param>
    /// <param name="timeoutSeconds">Request timeout in seconds (default: 30)</param>
    /// <param name="headers">Optional request headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response or null</returns>
    public static async Task<T?> PostWithRetryAsync<T>(
        this ExternalApiClient client,
        string url,
        object payload,
        int maxRetries = 3,
        int timeoutSeconds = 30,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client));
        ValidationUtility.ValidateNotNullOrEmpty(url, nameof(url));
        if (payload is null)
            throw new ArgumentNullException(nameof(payload));
        if (maxRetries <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "maxRetries must be greater than 0");
        if (timeoutSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(timeoutSeconds), "timeoutSeconds must be greater than 0");

        var retryCount = 0;
        var lastException = (Exception?)null;

        while (retryCount <= maxRetries)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

                return await client.PostAsync<T>(url, payload, headers, linkedCts.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastException = ex;
                retryCount++;

                if (retryCount > maxRetries)
                    break;

                // Exponential backoff
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount - 1)), cancellationToken);
            }
        }

        throw new HttpRequestException(
            $"POST request to {HttpUtility.MaskSensitiveUrl(url)} failed after {maxRetries} retries",
            lastException);
    }

    /// <summary>
    /// Performs a PUT request with retry logic and timeout handling.
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="client">The API client instance</param>
    /// <param name="url">Request URL</param>
    /// <param name="payload">Request payload</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3)</param>
    /// <param name="timeoutSeconds">Request timeout in seconds (default: 30)</param>
    /// <param name="headers">Optional request headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response or null</returns>
    public static async Task<T?> PutWithRetryAsync<T>(
        this ExternalApiClient client,
        string url,
        object payload,
        int maxRetries = 3,
        int timeoutSeconds = 30,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client));
        ValidationUtility.ValidateNotNullOrEmpty(url, nameof(url));
        if (payload is null)
            throw new ArgumentNullException(nameof(payload));
        if (maxRetries <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "maxRetries must be greater than 0");
        if (timeoutSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(timeoutSeconds), "timeoutSeconds must be greater than 0");

        var retryCount = 0;
        var lastException = (Exception?)null;

        while (retryCount <= maxRetries)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

                return await client.PutAsync<T>(url, payload, headers, linkedCts.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastException = ex;
                retryCount++;

                if (retryCount > maxRetries)
                    break;

                // Exponential backoff
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount - 1)), cancellationToken);
            }
        }

        throw new HttpRequestException(
            $"PUT request to {HttpUtility.MaskSensitiveUrl(url)} failed after {maxRetries} retries",
            lastException);
    }

    /// <summary>
    /// Performs a DELETE request with retry logic.
    /// </summary>
    /// <param name="client">The API client instance</param>
    /// <param name="url">Request URL</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3)</param>
    /// <param name="timeoutSeconds">Request timeout in seconds (default: 30)</param>
    /// <param name="headers">Optional request headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    public static async Task<bool> DeleteWithRetryAsync(
        this ExternalApiClient client,
        string url,
        int maxRetries = 3,
        int timeoutSeconds = 30,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client));
        ValidationUtility.ValidateNotNullOrEmpty(url, nameof(url));
        if (maxRetries <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "maxRetries must be greater than 0");
        if (timeoutSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(timeoutSeconds), "timeoutSeconds must be greater than 0");

        var retryCount = 0;
        var lastException = (Exception?)null;

        while (retryCount <= maxRetries)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

                return await client.DeleteAsync(url, headers, linkedCts.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastException = ex;
                retryCount++;

                if (retryCount > maxRetries)
                    break;

                // Exponential backoff
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount - 1)), cancellationToken);
            }
        }

        throw new HttpRequestException(
            $"DELETE request to {HttpUtility.MaskSensitiveUrl(url)} failed after {maxRetries} retries",
            lastException);
    }
}
