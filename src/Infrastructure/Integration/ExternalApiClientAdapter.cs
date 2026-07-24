#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Adapter class that implements the deprecated IExternalApiClient interface by delegating
/// to separate IExternalReadClient and IExternalWriteClient implementations.
/// This maintains backward compatibility during the transition period.
/// </summary>
[Obsolete("This adapter is deprecated. Use IExternalReadClient and IExternalWriteClient directly.")]
public class ExternalApiClientAdapter : IExternalApiClient
{
    private readonly IExternalReadClient _readClient;
    private readonly IExternalWriteClient _writeClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalApiClientAdapter"/> class.
    /// </summary>
    /// <param name="readClient">The read client implementation</param>
    /// <param name="writeClient">The write client implementation</param>
    /// <exception cref="ArgumentNullException">Thrown if either client is null</exception>
    public ExternalApiClientAdapter(IExternalReadClient readClient, IExternalWriteClient writeClient)
    {
        ArgumentNullException.ThrowIfNull(readClient);
        ArgumentNullException.ThrowIfNull(writeClient);

        _readClient = readClient;
        _writeClient = writeClient;
    }

    /// <inheritdoc/>
    public Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        => _readClient.GetAsync<T>(url, headers, cancellationToken);

    /// <inheritdoc/>
    public Task<T?> PostAsync<T>(string url, object payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        => _writeClient.PostAsync<T>(url, payload, headers, cancellationToken);

    /// <inheritdoc/>
    public Task<T?> PutAsync<T>(string url, object payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        => _writeClient.PutAsync<T>(url, payload, headers, cancellationToken);

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        => _writeClient.DeleteAsync(url, headers, cancellationToken);
}