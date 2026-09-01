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

    [StringLength(ServiceMetricConstants.MaximumNotesLength)]
    public string? Notes { get; set; }

    public double Uptime { get; set; }

    /// <summary>
    /// Determines if the service metrics indicate a problem.
    /// </summary>
    public bool HasAnomalies()
    {
        return CpuUsagePercent > ServiceMetricConstants.AnomalousCpuUsagePercent ||
               MemoryUsagePercent > ServiceMetricConstants.AnomalousMemoryUsagePercent ||
               DiskUsagePercent > ServiceMetricConstants.AnomalousDiskUsagePercent ||
               AverageResponseTimeMs > ServiceMetricConstants.AnomalousAverageResponseTimeMs;
    }

    /// <summary>
    /// Calculates the error rate as a percentage.
    /// </summary>
    public decimal GetErrorRate()
    {
        if (TotalRequests == ServiceMetricConstants.NoRequests)
            return ServiceMetricConstants.ZeroErrorRate;

        return (decimal)ErrorCount / TotalRequests * ServiceMetricConstants.PercentageMultiplier;
    }

    /// <summary>
    /// Gets a severity rating based on resource usage.
    /// </summary>
    public string GetSeverityRating()
    {
        if (CpuUsagePercent > ServiceMetricConstants.CriticalCpuUsagePercent ||
            MemoryUsagePercent > ServiceMetricConstants.CriticalMemoryUsagePercent)
            return ServiceMetricConstants.CriticalSeverity;

        if (CpuUsagePercent > ServiceMetricConstants.WarningCpuUsagePercent ||
            MemoryUsagePercent > ServiceMetricConstants.WarningMemoryUsagePercent)
            return ServiceMetricConstants.WarningSeverity;

        if (DiskUsagePercent > ServiceMetricConstants.WarningDiskUsagePercent)
            return ServiceMetricConstants.WarningSeverity;

        return ServiceMetricConstants.NormalSeverity;
    }

    /// <summary>
    /// Formats metrics as a readable string for logging or display.
    /// </summary>
    public string FormatMetrics()
    {
        return string.Format(
            ServiceMetricConstants.MetricsDisplayFormat,
            CpuUsagePercent,
            MemoryUsagePercent,
            MemoryUsageBytes / ServiceMetricConstants.BytesPerMegabyte,
            DiskUsagePercent,
            RequestsPerSecond,
            AverageResponseTimeMs,
            ErrorCount,
            TotalRequests,
            Uptime);
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
    public override string ToString() => $"ServiceMetric {{ Id = {Id}, ServiceId = {ServiceId}, Service = {Service}, CpuUsagePercent = {CpuUsagePercent}, MemoryUsagePercent = {MemoryUsagePercent}, MemoryUsageBytes = {MemoryUsageBytes} }}";
}
