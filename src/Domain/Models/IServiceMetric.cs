#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Interface for service metric operations.
/// </summary>
public interface IServiceMetric
{
    Guid Id { get; set; }
    Guid ServiceId { get; set; }
    ServiceRegistration? Service { get; set; }
    decimal CpuUsagePercent { get; set; }
    decimal MemoryUsagePercent { get; set; }
    long MemoryUsageBytes { get; set; }
    decimal DiskUsagePercent { get; set; }
    long DiskUsageBytes { get; set; }
    int ActiveConnections { get; set; }
    long RequestsPerSecond { get; set; }
    decimal AverageResponseTimeMs { get; set; }
    long TotalRequests { get; set; }
    int ErrorCount { get; set; }
    DateTime RecordedAt { get; set; }
    string? Notes { get; set; }
    double Uptime { get; set; }
    bool HasAnomalies();
    decimal GetErrorRate();
    string GetSeverityRating();
    string FormatMetrics();
}