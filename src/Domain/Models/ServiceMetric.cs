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
public class ServiceMetric : IServiceMetric, IEquatable<ServiceMetric>
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

    public bool Equals(ServiceMetric? other)
    {
        if (other is null) return false;
        return Id == other.Id &&
               ServiceId == other.ServiceId &&
               EqualityComparer<ServiceRegistration?>.Default.Equals(Service, other.Service) &&
               CpuUsagePercent == other.CpuUsagePercent &&
               MemoryUsagePercent == other.MemoryUsagePercent &&
               MemoryUsageBytes == other.MemoryUsageBytes &&
               DiskUsagePercent == other.DiskUsagePercent &&
               DiskUsageBytes == other.DiskUsageBytes;
    }

    public override bool Equals(object? obj) => Equals(obj as ServiceMetric);
    public override int GetHashCode() => HashCode.Combine(Id, ServiceId, Service, CpuUsagePercent, MemoryUsagePercent, MemoryUsageBytes, DiskUsagePercent, DiskUsageBytes);
    public static bool operator ==(ServiceMetric? left, ServiceMetric? right) => EqualityComparer<ServiceMetric>.Default.Equals(left, right);
    public static bool operator !=(ServiceMetric? left, ServiceMetric? right) => !(left == right);
}
