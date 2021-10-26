#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.ServiceMesh;

/// <summary>
/// Extension methods for <see cref="ServiceMeshOptions"/> that provide convenient
/// ways to configure and work with service mesh options programmatically.
/// </summary>
public static class ServiceMeshOptionsExtensions
{
    /// <summary>
    /// Configures the service mesh options with a delegate for fluent configuration.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="configure">The delegate that configures the options.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <returns>The configured options for method chaining.</returns>
    public static ServiceMeshOptions Configure(this ServiceMeshOptions options, Action<ServiceMeshOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(configure);

        configure(options);
        return options;
    }

    /// <summary>
    /// Creates a configured instance of <see cref="ServiceMeshOptions"/> with the specified settings.
    /// Useful for testing or when configuring options without dependency injection.
    /// </summary>
    /// <param name="configure">The delegate that configures the options.</param>
    /// <returns>A new instance of <see cref="ServiceMeshOptions"/> configured with the delegate.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public static ServiceMeshOptions CreateConfigured(Action<ServiceMeshOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new ServiceMeshOptions();
        configure(options);
        return options;
    }

    /// <summary>
    /// Determines whether the service mesh is configured to be enabled and ready for use.
    /// This combines both the <see cref="ServiceMeshOptions.Enabled"/> flag and a successful
    /// readiness check against the admin endpoint.
    /// </summary>
    /// <param name="options">The service mesh options to check.</param>
    /// <param name="sidecarProxyService">The sidecar proxy service to perform the readiness check.</param>
    /// <returns>True if the service mesh is enabled and ready; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sidecarProxyService"/> is <see langword="null"/>.</exception>
    public static async Task<bool> IsReadyAsync(
        this ServiceMeshOptions options,
        ISidecarProxyService sidecarProxyService,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(sidecarProxyService);

        if (!options.Enabled)
        {
            return false;
        }

        return await sidecarProxyService.IsServiceMeshEnabledAsync(cancellationToken);
    }
}