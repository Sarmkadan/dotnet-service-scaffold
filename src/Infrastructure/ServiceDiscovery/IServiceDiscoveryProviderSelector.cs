#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Strategy interface for selecting the appropriate <see cref="IServiceDiscoveryProvider"/>
/// based on runtime configuration and available providers.
/// </summary>
/// <remarks>
/// This interface enables the <see cref="ServiceDiscoveryService"/> to be provider-agnostic while still
/// supporting different backend implementations (DNS, Registry, InMemory, etc.).
/// </remarks>
public interface IServiceDiscoveryProviderSelector
{
    /// <summary>
    /// Gets the active provider instance based on the current configuration.
    /// </summary>
    /// <returns>The selected <see cref="IServiceDiscoveryProvider"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no suitable provider can be determined.</exception>
    IServiceDiscoveryProvider GetProvider();

    /// <summary>
    /// Gets all registered providers that are available for selection.
    /// </summary>
    /// <returns>Collection of available provider instances.</returns>
    IEnumerable<IServiceDiscoveryProvider> GetAllProviders();
}