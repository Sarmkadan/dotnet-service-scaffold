#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Interface for external API clients that only perform read operations (GET requests).
/// Implements the Interface Segregation Principle by separating read capabilities from write operations.
/// </summary>
public interface IExternalReadClient
{
    /// <summary>
    /// Performs a GET request and returns the deserialized response.
    /// </summary>
    /// <typeparam name="T">Response type to deserialize to</typeparam>
    /// <param name="url">Request URL</param>
    /// <param name="headers">Optional request headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response or null if not found</returns>
    /// <exception cref="ArgumentException"><paramref name="url"/> is null or empty</exception>
    /// <exception cref="HttpRequestException">Request failed</exception>
    Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
}