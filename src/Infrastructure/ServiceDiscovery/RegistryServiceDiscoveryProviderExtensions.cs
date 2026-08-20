using System.Globalization;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.ServiceDiscovery;
using DotnetServiceScaffold.Shared.Models;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Provides extension methods for <see cref="RegistryServiceDiscoveryProvider"/>.
/// </summary>
public static class RegistryServiceDiscoveryProviderExtensions
{
    /// <summary>
    /// Checks if a specific service is currently present in the catalog.
    /// </summary>
    /// <param name="provider">The registry provider instance.</param>
    /// <param name="serviceName">The name of the service to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> containing <see langword="true"/> if the service exists, <see langword="false"/> otherwise, or a failure result if the registry could not be queried.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="provider"/> or <paramref name="serviceName"/> is <see langword="null"/>.</exception>
    public static async Task<Result<bool>> ServiceExistsAsync(
        this RegistryServiceDiscoveryProvider provider,
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);

        var result = await provider.GetAllServiceNamesAsync(cancellationToken);
        if (!result.IsSuccess)
        {
            return Result<bool>.Failure(result.ErrorMessage ?? "Failed to retrieve service list");
        }

        return Result<bool>.Success(result.Value?.Contains(serviceName, StringComparer.OrdinalIgnoreCase) ?? false);
    }

    /// <summary>
    /// Resolves a service and ensures it returns a non-empty list of records if successful.
    /// </summary>
    /// <param name="provider">The registry provider instance.</param>
    /// <param name="serviceName">The name of the service to resolve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> containing a list of records, or a failure if no instances were found or an error occurred.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="provider"/> or <paramref name="serviceName"/> is <see langword="null"/>.</exception>
    public static async Task<Result<IReadOnlyList<ServiceDiscoveryRecord>>> ResolveAnyAsync(
        this RegistryServiceDiscoveryProvider provider,
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);

        var result = await provider.ResolveAsync(serviceName, cancellationToken);
        if (!result.IsSuccess)
        {
            return result;
        }

        if (result.Value.Count == 0)
        {
            return Result<IReadOnlyList<ServiceDiscoveryRecord>>.Failure(
                new InvalidOperationException(string.Create(CultureInfo.InvariantCulture, $"No instances found for service '{serviceName}'")));
        }

        return result;
    }
}
