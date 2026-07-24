#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Default implementation of <see cref="IServiceDiscoveryProviderSelector"/> that selects
/// the appropriate provider based on <see cref="ServiceDiscoveryOptions.Mode"/> configuration.
/// </summary>
/// <remarks>
/// This selector supports the following provider resolution logic:
/// <list type="bullet">
///   <item><see cref="DiscoveryMode.Dns"/> → <see cref="DnsServiceDiscoveryProvider"/></item>
///   <item><see cref="DiscoveryMode.Registry"/> → <see cref="RegistryServiceDiscoveryProvider"/></item>
///   <item><see cref="DiscoveryMode.Hybrid"/> → <see cref="RegistryServiceDiscoveryProvider"/> (primary), <see cref="DnsServiceDiscoveryProvider"/> (fallback)</item>
/// </list>
/// </remarks>
public sealed class ServiceDiscoveryProviderSelector : IServiceDiscoveryProviderSelector
{
    private readonly DnsServiceDiscoveryProvider _dnsProvider;
    private readonly RegistryServiceDiscoveryProvider _registryProvider;
    private readonly InMemoryServiceDiscoveryProvider _inMemoryProvider;
    private readonly ServiceDiscoveryOptions _options;
    private readonly ILogger<ServiceDiscoveryProviderSelector> _logger;

    /// <summary>
    /// Initialises a new <see cref="ServiceDiscoveryProviderSelector"/> with all available providers.
    /// </summary>
    /// <param name="dnsProvider">DNS-based service discovery provider.</param>
    /// <param name="registryProvider">Registry-based service discovery provider.</param>
    /// <param name="inMemoryProvider">In-memory service discovery provider for testing/development.</param>
    /// <param name="options">Service discovery configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public ServiceDiscoveryProviderSelector(
        DnsServiceDiscoveryProvider dnsProvider,
        RegistryServiceDiscoveryProvider registryProvider,
        InMemoryServiceDiscoveryProvider inMemoryProvider,
        IOptions<ServiceDiscoveryOptions> options,
        ILogger<ServiceDiscoveryProviderSelector> logger)
    {
        _dnsProvider = dnsProvider ?? throw new ArgumentNullException(nameof(dnsProvider));
        _registryProvider = registryProvider ?? throw new ArgumentNullException(nameof(registryProvider));
        _inMemoryProvider = inMemoryProvider ?? throw new ArgumentNullException(nameof(inMemoryProvider));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public IServiceDiscoveryProvider GetProvider()
    {
        return _options.Mode switch
        {
            DiscoveryMode.Dns => _dnsProvider,
            DiscoveryMode.Registry => _registryProvider,
            DiscoveryMode.Hybrid => _registryProvider, // Registry is primary in hybrid mode
            _ => throw new InvalidOperationException($"Unknown discovery mode: {_options.Mode}")
        };
    }

    /// <inheritdoc/>
    public IEnumerable<IServiceDiscoveryProvider> GetAllProviders()
    {
        yield return _dnsProvider;
        yield return _registryProvider;
        yield return _inMemoryProvider;
    }

    /// <summary>
    /// Gets the appropriate provider for write operations (registration/deregistration).
    /// </summary>
    /// <returns>The provider that supports write operations.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no writable provider is available.</exception>
    public IServiceDiscoveryProvider GetWritableProvider()
    {
        var provider = GetProvider();

        // DNS provider is read-only, so we need to check if it's the active provider
        if (provider.ProviderName == "DNS" && _options.Mode != DiscoveryMode.Dns)
        {
            // In hybrid mode, registry is primary, so use that instead
            return _registryProvider;
        }

        return provider;
    }
}