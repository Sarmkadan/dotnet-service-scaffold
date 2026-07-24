#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Interface for external API clients that perform write operations (POST, PUT, DELETE requests).
/// Implements the Interface Segregation Principle by separating write capabilities from read operations.
/// </summary>
public interface IExternalWriteClient
{
    /// <summary>
    /// Performs a POST request and returns the deserialized response.
    /// </summary>
    /// <typeparam name="T">Response type to deserialize to</typeparam>
    /// <param name="url">Request URL</param>
    /// <param name="payload">Request payload to serialize as JSON</param>
    /// <param name="headers">Optional request headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response or null</returns>
    /// <exception cref="ArgumentException"><paramref name="url"/> is null or empty</exception>
    /// <exception cref="ArgumentNullException"><paramref name="payload"/> is null</exception>
    /// <exception cref="HttpRequestException">Request failed</exception>
    Task<T?> PostAsync<T>(string url, object payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a PUT request and returns the deserialized response.
    /// </summary>
    /// <typeparam name="T">Response type to deserialize to</typeparam>
    /// <param name="url">Request URL</param>
    /// <param name="payload">Request payload to serialize as JSON</param>
    /// <param name="headers">Optional request headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response or null</returns>
    /// <exception cref="ArgumentException"><paramref name="url"/> is null or empty</exception>
    /// <exception cref="ArgumentNullException"><paramref name="payload"/> is null</exception>
    /// <exception cref="HttpRequestException">Request failed</exception>
    Task<T?> PutAsync<T>(string url, object payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a DELETE request.
    /// </summary>
    /// <param name="url">Request URL</param>
    /// <param name="headers">Optional request headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    /// <exception cref="ArgumentException"><paramref name="url"/> is null or empty</exception>
    /// <exception cref="HttpRequestException">Request failed</exception>
    Task<bool> DeleteAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
}