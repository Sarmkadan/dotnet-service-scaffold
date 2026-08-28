#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Stores performance and resource metrics for a service.
/// </summary>
public class ServiceMetric : IServiceMetric
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Service))]
    public Guid ServiceId { get; set; }

    public ServiceRegistration? Service { get; set; }

    public decimal CpuUsagePercent { get; set; }

    public decimal MemoryUsagePercent { get; set; }

    public long MemoryUsageBytes { get; set; }

    public decimal DiskUsagePercent { get; set; }

    public long DiskUsageBytes { get; set; }

    public int ActiveConnections { get; set; }

    public long RequestsPerSecond { get; set; }

    public decimal AverageResponseTimeMs { get; set; }

    public long TotalRequests { get; set; }

    public int ErrorCount { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Notes { get; set; }

    public double Uptime { get; set; }

    /// <summary>
    /// Determines if the service metrics indicate a problem.
    /// </summary>
    public bool HasAnomalies()
    {
        return CpuUsagePercent > 85 ||
               MemoryUsagePercent > 80 ||
               DiskUsagePercent > 90 ||
               AverageResponseTimeMs > 5000;
    }

    /// <summary>
    /// Calculates the error rate as a percentage.
    /// </summary>
    public decimal GetErrorRate()
    {
        if (TotalRequests == 0)
            return 0;

        return (decimal)ErrorCount / TotalRequests * 100;
    }

    /// <summary>
    /// Gets a severity rating based on resource usage.
    /// </summary>
    public string GetSeverityRating()
    {
        if (CpuUsagePercent > 90 || MemoryUsagePercent > 85)
            return "Critical";

        if (CpuUsagePercent > 75 || MemoryUsagePercent > 70)
            return "Warning";

        if (DiskUsagePercent > 85)
            return "Warning";

        return "Normal";
    }

    /// <summary>
    /// Formats metrics as a readable string for logging or display.
    /// </summary>
    public string FormatMetrics()
    {
        return $"CPU: {CpuUsagePercent:F1}% | " +
               $"Memory: {MemoryUsagePercent:F1}% ({MemoryUsageBytes / (1024 * 1024)}MB) | " +
               $"Disk: {DiskUsagePercent:F1}% | " +
               $"RPS: {RequestsPerSecond} | " +
               $"Avg Response: {AverageResponseTimeMs:F0}ms | " +
               $"Errors: {ErrorCount}/{TotalRequests} | " +
               $"Uptime: {Uptime:F2}%";
    }
}
