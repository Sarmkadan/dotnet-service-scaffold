#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetServiceScaffold.Infrastructure.HealthChecks;

/// <summary>
/// Base class for health checks that ensures consistent exception handling.
/// Prevents leakage of exception details into health check responses while
/// still logging the full exception details for diagnostics.
/// </summary>
public abstract class SafeHealthCheckBase : IHealthCheck
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SafeHealthCheckBase"/> class.
    /// </summary>
    /// <param name="logger">The logger to use for logging exception details.</param>
    protected SafeHealthCheckBase(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the health check logic with safe exception handling.
    /// </summary>
    /// <param name="context">The context in which the health check is performed.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the health check.</param>
    /// <returns>A task containing the health status.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            return await CheckHealthInternalAsync(context, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Re-share cancellation tokens
            throw;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Health check timed out.");
            return HealthCheckResult.Degraded("Health check timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed.");
            return HealthCheckResult.Unhealthy("Health check failed.");
        }
    }

    /// <summary>
    /// Implements the actual health check logic. Derived classes must implement this method.
    /// </summary>
    /// <param name="context">The context in which the health check is performed.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the health check.</param>
    /// <returns>A task containing the health status.</returns>
    protected abstract Task<HealthCheckResult> CheckHealthInternalAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default);
}