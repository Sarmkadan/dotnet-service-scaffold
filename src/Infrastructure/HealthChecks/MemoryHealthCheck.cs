// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Runtime;

namespace DotnetServiceScaffold.Infrastructure.HealthChecks;

/// <summary>
/// Health check that monitors memory usage using container-aware limits via GC.GetGCMemoryInfo().
/// Reports Healthy when memory is below the configured threshold percentage,
/// Degraded when approaching the limit, and Unhealthy when exceeding it.
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private readonly double _healthyThresholdPercent;
    private readonly double _degradedThresholdPercent;
    private readonly double _unhealthyThresholdPercent;

    /// <summary>
    /// Health check that monitors memory usage with container-aware limits.
    /// </summary>
    /// <param name="healthyThresholdPercent">
    /// Percentage of available memory below which the check reports Healthy.
    /// Defaults to 70% (70). Must be between 1 and 99.
    /// </param>
    /// <param name="degradedThresholdPercent">
    /// Percentage of available memory above which the check reports Degraded.
    /// Defaults to 85% (85). Must be greater than healthyThresholdPercent.
    /// </param>
    /// <param name="unhealthyThresholdPercent">
    /// Percentage of available memory above which the check reports Unhealthy.
    /// Defaults to 95% (95). Must be greater than degradedThresholdPercent.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any threshold is outside valid range or thresholds are not in ascending order.
    /// </exception>
    public MemoryHealthCheck(
        double healthyThresholdPercent = MemoryHealthCheckConstants.DefaultHealthyThresholdPercent,
        double degradedThresholdPercent = MemoryHealthCheckConstants.DefaultDegradedThresholdPercent,
        double unhealthyThresholdPercent = MemoryHealthCheckConstants.DefaultUnhealthyThresholdPercent)
    {
        if (healthyThresholdPercent is < MemoryHealthCheckConstants.MinThresholdPercent or > MemoryHealthCheckConstants.MaxThresholdPercent)
        {
            throw new ArgumentOutOfRangeException(
                nameof(healthyThresholdPercent),
                $"Healthy threshold must be between {MemoryHealthCheckConstants.MinThresholdPercent} and {MemoryHealthCheckConstants.MaxThresholdPercent}, got {healthyThresholdPercent}.");
        }

        if (degradedThresholdPercent <= healthyThresholdPercent)
        {
            throw new ArgumentOutOfRangeException(
                nameof(degradedThresholdPercent),
                $"Degraded threshold must be greater than healthy threshold ({healthyThresholdPercent}), got {degradedThresholdPercent}.");
        }

        if (degradedThresholdPercent > MemoryHealthCheckConstants.MaxThresholdPercent)
        {
            throw new ArgumentOutOfRangeException(
                nameof(degradedThresholdPercent),
                $"Degraded threshold must be between {healthyThresholdPercent + 1} and {MemoryHealthCheckConstants.MaxThresholdPercent}, got {degradedThresholdPercent}.");
        }

        if (unhealthyThresholdPercent <= degradedThresholdPercent)
        {
            throw new ArgumentOutOfRangeException(
                nameof(unhealthyThresholdPercent),
                $"Unhealthy threshold must be greater than degraded threshold ({degradedThresholdPercent}), got {unhealthyThresholdPercent}.");
        }

        if (unhealthyThresholdPercent > MemoryHealthCheckConstants.MaxThresholdPercent)
        {
            throw new ArgumentOutOfRangeException(
                nameof(unhealthyThresholdPercent),
                $"Unhealthy threshold must be between {degradedThresholdPercent + 1} and {MemoryHealthCheckConstants.MaxThresholdPercent}, got {unhealthyThresholdPercent}.");
        }

        _healthyThresholdPercent = healthyThresholdPercent;
        _degradedThresholdPercent = degradedThresholdPercent;
        _unhealthyThresholdPercent = unhealthyThresholdPercent;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Memory operations are generally fast, but enforce timeout for consistency
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(MemoryHealthCheckConstants.TimeoutSeconds));

        try
        {
            // Use GC.GetGCMemoryInfo() which respects container/cgroup memory limits
            // TotalAvailableMemoryBytes returns the memory limit as configured by the container
            var gcInfo = GC.GetGCMemoryInfo();
            var totalAvailableMemoryBytes = gcInfo.TotalAvailableMemoryBytes;
            var memoryLoadBytes = gcInfo.MemoryLoadBytes;
            var heapSizeBytes = gcInfo.HeapSizeBytes;

            // Calculate percentage of available memory currently in use
            // If TotalAvailableMemoryBytes is 0 (unlimited), use HeapSizeBytes as fallback
            var memoryUsagePercent = totalAvailableMemoryBytes > 0
                ? (double)memoryLoadBytes / totalAvailableMemoryBytes * 100
                : (double)heapSizeBytes / Math.Max(1, GC.GetTotalMemory(false)) * 100;

            var data = new Dictionary<string, object>
            {
                ["memoryLoadBytes"] = memoryLoadBytes,
                ["memoryLoadMB"] = memoryLoadBytes / MemoryHealthCheckConstants.BytesToMegabytesFactor,
                ["totalAvailableMemoryBytes"] = totalAvailableMemoryBytes,
                ["totalAvailableMemoryMB"] = totalAvailableMemoryBytes / MemoryHealthCheckConstants.BytesToMegabytesFactor,
                ["heapSizeBytes"] = heapSizeBytes,
                ["heapSizeMB"] = heapSizeBytes / MemoryHealthCheckConstants.BytesToMegabytesFactor,
                ["memoryUsagePercent"] = Math.Round(memoryUsagePercent, 2),
                ["healthyThresholdPercent"] = _healthyThresholdPercent,
                ["degradedThresholdPercent"] = _degradedThresholdPercent,
                ["unhealthyThresholdPercent"] = _unhealthyThresholdPercent
            };

            if (memoryUsagePercent >= _unhealthyThresholdPercent)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Memory usage at {memoryUsagePercent}% exceeds unhealthy threshold of {_unhealthyThresholdPercent}%",
                    data: data));
            }

            if (memoryUsagePercent >= _degradedThresholdPercent)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Memory usage at {memoryUsagePercent}% exceeds degraded threshold of {_degradedThresholdPercent}%",
                    data: data));
            }

            if (memoryUsagePercent >= _healthyThresholdPercent)
            {
                return Task.FromResult(HealthCheckResult.Healthy(
                    $"Memory usage at {memoryUsagePercent}% is within healthy range (up to {_healthyThresholdPercent}%)",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                $"Memory usage at {memoryUsagePercent}% is below healthy threshold of {_healthyThresholdPercent}%",
                data: data));
        }
        catch (OperationCanceledException)
        {
            var data = new Dictionary<string, object>
            {
                ["timeout"] = true,
                ["thresholdSeconds"] = MemoryHealthCheckConstants.TimeoutSeconds
            };
            return Task.FromResult(HealthCheckResult.Degraded(
                "Timeout while checking memory usage.",
                data: data));
        }
        catch (Exception ex)
        {
            var data = new Dictionary<string, object>
            {
                ["error"] = ex.Message
            };
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Error while checking memory usage.",
                ex,
                data));
        }
    }
}
