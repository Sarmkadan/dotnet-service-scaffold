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
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="url"/> is null or empty</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxRetries"/> or <paramref name="timeoutSeconds"/> is less than or equal to 0</exception>
    /// <exception cref="HttpRequestException">Request failed after all retry attempts</exception>
    public static async Task<T?> GetWithRetryAsync<T>(
        this ExternalApiClient client,
        string url,
        int maxRetries = ExternalApiClientExtensionsConstants.DefaultMaxRetries,
        int timeoutSeconds = ExternalApiClientExtensionsConstants.DefaultTimeoutSeconds,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ValidationUtility.ValidateNotNullOrEmpty(url, nameof(url));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxRetries, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeoutSeconds, 0);

        var retryCount = 0;
        Exception? lastException = null;

        while (retryCount <= maxRetries)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

                return await client.GetAsync<T>(url, headers, linkedCts.Token).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastException = ex;
                retryCount++;

                if (retryCount > maxRetries)
                    break;

                // Exponential backoff with jitter to avoid thundering herd
                var delaySeconds = Math.Pow(2, retryCount - 1);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken).ConfigureAwait(false);
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
    /// <exception cref="ArgumentNullException"><paramref name="client"/> or <paramref name="payload"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="url"/> is null or empty</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxRetries"/> or <paramref name="timeoutSeconds"/> is less than or equal to 0</exception>
    /// <exception cref="HttpRequestException">Request failed after all retry attempts</exception>
    public static async Task<T?> PostWithRetryAsync<T>(
        this ExternalApiClient client,
        string url,
        object payload,
        int maxRetries = ExternalApiClientExtensionsConstants.DefaultMaxRetries,
        int timeoutSeconds = ExternalApiClientExtensionsConstants.DefaultTimeoutSeconds,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ValidationUtility.ValidateNotNullOrEmpty(url, nameof(url));
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxRetries, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeoutSeconds, 0);

        var retryCount = 0;
        Exception? lastException = null;

        while (retryCount <= maxRetries)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

                return await client.PostAsync<T>(url, payload, headers, linkedCts.Token).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastException = ex;
                retryCount++;

                if (retryCount > maxRetries)
                    break;

                // Exponential backoff with jitter to avoid thundering herd
                var delaySeconds = Math.Pow(2, retryCount - 1);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken).ConfigureAwait(false);
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
    /// <exception cref="ArgumentNullException"><paramref name="client"/> or <paramref name="payload"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="url"/> is null or empty</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxRetries"/> or <paramref name="timeoutSeconds"/> is less than or equal to 0</exception>
    /// <exception cref="HttpRequestException">Request failed after all retry attempts</exception>
    public static async Task<T?> PutWithRetryAsync<T>(
        this ExternalApiClient client,
        string url,
        object payload,
        int maxRetries = ExternalApiClientExtensionsConstants.DefaultMaxRetries,
        int timeoutSeconds = ExternalApiClientExtensionsConstants.DefaultTimeoutSeconds,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ValidationUtility.ValidateNotNullOrEmpty(url, nameof(url));
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxRetries, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeoutSeconds, 0);

        var retryCount = 0;
        Exception? lastException = null;

        while (retryCount <= maxRetries)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

                return await client.PutAsync<T>(url, payload, headers, linkedCts.Token).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastException = ex;
                retryCount++;

                if (retryCount > maxRetries)
                    break;

                // Exponential backoff with jitter to avoid thundering herd
                var delaySeconds = Math.Pow(2, retryCount - 1);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken).ConfigureAwait(false);
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
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="url"/> is null or empty</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxRetries"/> or <paramref name="timeoutSeconds"/> is less than or equal to 0</exception>
    /// <exception cref="HttpRequestException">Request failed after all retry attempts</exception>
    public static async Task<bool> DeleteWithRetryAsync(
        this ExternalApiClient client,
        string url,
        int maxRetries = ExternalApiClientExtensionsConstants.DefaultMaxRetries,
        int timeoutSeconds = ExternalApiClientExtensionsConstants.DefaultTimeoutSeconds,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ValidationUtility.ValidateNotNullOrEmpty(url, nameof(url));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxRetries, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeoutSeconds, 0);

        var retryCount = 0;
        Exception? lastException = null;

        while (retryCount <= maxRetries)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

                return await client.DeleteAsync(url, headers, linkedCts.Token).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastException = ex;
                retryCount++;

                if (retryCount > maxRetries)
                    break;

                // Exponential backoff with jitter to avoid thundering herd
                var delaySeconds = Math.Pow(2, retryCount - 1);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken).ConfigureAwait(false);
            }
        }

        throw new HttpRequestException(
            $"DELETE request to {HttpUtility.MaskSensitiveUrl(url)} failed after {maxRetries} retries",
            lastException);
    }
}