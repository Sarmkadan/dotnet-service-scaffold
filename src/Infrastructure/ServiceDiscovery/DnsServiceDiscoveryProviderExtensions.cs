using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.ServiceDiscovery;
using DotnetServiceScaffold.Shared.Models;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Provides extension methods for <see cref="DnsServiceDiscoveryProvider"/>.
/// </summary>
public static class DnsServiceDiscoveryProviderExtensions
{
    /// <summary>
    /// Attempts to retrieve the first available service record from the provider.
    /// </summary>
    /// <param name="provider">The <see cref="DnsServiceDiscoveryProvider"/> instance.</param>
    /// <param name="serviceName">The name of the service to resolve.</param>
    /// <param name="ct">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="Result{T}"/> containing the first <see cref="ServiceDiscoveryRecord"/> if found, or an error.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="provider"/> or <paramref name="serviceName"/> is null.</exception>
    public static async Task<Result<ServiceDiscoveryRecord?>> GetFirstRecordAsync(
        this DnsServiceDiscoveryProvider provider,
        string serviceName,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);

        var result = await provider.ResolveAsync(serviceName, ct);
        if (!result.IsSuccess)
        {
            return Result<ServiceDiscoveryRecord?>.Failure(result.ErrorMessage!, result.ErrorCode);
        }

        return Result<ServiceDiscoveryRecord?>.Success(result.Value!.FirstOrDefault());
    }

    /// <summary>
    /// Waits for the provider to become available within the specified timeout.
    /// </summary>
    /// <param name="provider">The <see cref="DnsServiceDiscoveryProvider"/> instance.</param>
    /// <param name="timeout">The maximum time to wait for availability.</param>
    /// <param name="pollInterval">The interval between availability checks.</param>
    /// <param name="ct">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that completes with <see langword="true"/> if available, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="provider"/> is null.</exception>
    public static async Task<bool> WaitForAvailabilityAsync(
        this DnsServiceDiscoveryProvider provider,
        TimeSpan timeout,
        TimeSpan pollInterval,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(provider);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        try
        {
            while (!cts.IsCancellationRequested)
            {
                if (await provider.IsAvailableAsync(cts.Token))
                {
                    return true;
                }

                await Task.Delay(pollInterval, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Timeout or cancellation reached
        }

        return false;
    }
}
