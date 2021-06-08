// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotnetServiceScaffold.Domain.Enums;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Records the results of a health check performed on a service.
/// </summary>
public class HealthCheckResult
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Service))]
    public Guid ServiceId { get; set; }

    public ServiceRegistration? Service { get; set; }

    public HealthStatus Status { get; set; }

    public int? HttpStatusCode { get; set; }

    public long ResponseTimeMs { get; set; }

    [StringLength(1000)]
    public string? ErrorMessage { get; set; }

    [StringLength(4000)]
    public string? ResponseBody { get; set; }

    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    public string? CheckMethod { get; set; }

    [StringLength(255)]
    public string? CheckEndpoint { get; set; }

    public decimal? CpuUsagePercent { get; set; }

    public decimal? MemoryUsagePercent { get; set; }

    public long? DiskUsageBytes { get; set; }

    /// <summary>
    /// Determines if this health check result indicates a healthy service.
    /// </summary>
    public bool IsHealthy()
    {
        return Status == HealthStatus.Healthy &&
               HttpStatusCode >= 200 &&
               HttpStatusCode < 300;
    }

    /// <summary>
    /// Determines if the response time is within acceptable parameters.
    /// </summary>
    public bool IsResponseTimeAcceptable(long thresholdMs = 5000)
    {
        return ResponseTimeMs <= thresholdMs;
    }

    /// <summary>
    /// Checks if system resources are within acceptable ranges.
    /// </summary>
    public bool AreResourcesHealthy(decimal cpuThreshold = 90, decimal memoryThreshold = 85)
    {
        if (CpuUsagePercent.HasValue && CpuUsagePercent > cpuThreshold)
            return false;

        if (MemoryUsagePercent.HasValue && MemoryUsagePercent > memoryThreshold)
            return false;

        return true;
    }

    /// <summary>
    /// Gets a human-readable summary of the health check result.
    /// </summary>
    public string GetSummary()
    {
        var parts = new List<string> { $"Status: {Status}" };

        if (HttpStatusCode.HasValue)
            parts.Add($"HTTP {HttpStatusCode}");

        parts.Add($"Response Time: {ResponseTimeMs}ms");

        if (CpuUsagePercent.HasValue)
            parts.Add($"CPU: {CpuUsagePercent:F1}%");

        if (MemoryUsagePercent.HasValue)
            parts.Add($"Memory: {MemoryUsagePercent:F1}%");

        if (!string.IsNullOrEmpty(ErrorMessage))
            parts.Add($"Error: {ErrorMessage}");

        return string.Join(" | ", parts);
    }
}
