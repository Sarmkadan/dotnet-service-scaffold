// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotnetServiceScaffold.Infrastructure.HealthChecks;

/// <summary>
/// Health check that reports Degraded when total memory exceeds a threshold
/// and Unhealthy when it exceeds twice that threshold.
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private readonly long _degradedThresholdBytes;
    private readonly long _unhealthyThresholdBytes;

    /// <param name="degradedThresholdBytes">
    /// Memory threshold in bytes above which the check reports Degraded. Defaults to 512 MB.
    /// </param>
    public MemoryHealthCheck(long degradedThresholdBytes = 512 * 1024 * 1024)
    {
        _degradedThresholdBytes = degradedThresholdBytes;
        _unhealthyThresholdBytes = degradedThresholdBytes * 2;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Memory operations are generally fast, but enforce timeout for consistency
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(2));

        try
        {
            // Calculate memory usage with timeout
            var totalMemoryBytes = GC.GetTotalMemory(false);

            var data = new Dictionary<string, object>
            {
                { "totalMemoryBytes", totalMemoryBytes },
                { "totalMemoryMB", totalMemoryBytes / (1024 * 1024) }
            };

            if (totalMemoryBytes > _unhealthyThresholdBytes)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Memory usage above unhealthy threshold: {totalMemoryBytes / (1024 * 1024)} MB",
                    data: data));
            }

            if (totalMemoryBytes > _degradedThresholdBytes)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Memory usage above degraded threshold: {totalMemoryBytes / (1024 * 1024)} MB",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                $"Memory usage is within limits: {totalMemoryBytes / (1024 * 1024)} MB",
                data));
        }
        catch (OperationCanceledException)
        {
            var data = new Dictionary<string, object>
            {
                { "timeout", true },
                { "thresholdSeconds", 2 }
            };
            return Task.FromResult(HealthCheckResult.Degraded(
                "Timeout while checking memory usage.",
                data: data));
        }
        catch (Exception ex)
        {
            var data = new Dictionary<string, object>
            {
                { "error", ex.Message }
            };
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Error while checking memory usage.",
                ex, data));
        }
    }
}
