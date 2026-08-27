#nullable enable
using DotnetServiceScaffold.Domain.Enums;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Defines the contract for a health check result.
/// </summary>
public interface IHealthCheckResult
{
    Guid Id { get; set; }
    Guid ServiceId { get; set; }
    ServiceRegistration? Service { get; set; }
    HealthStatus Status { get; set; }
    int? HttpStatusCode { get; set; }
    long ResponseTimeMs { get; set; }
    string? ErrorMessage { get; set; }
    string? ResponseBody { get; set; }
    DateTime CheckedAt { get; set; }
    string? CheckMethod { get; set; }
    string? CheckEndpoint { get; set; }
    decimal? CpuUsagePercent { get; set; }
    decimal? MemoryUsagePercent { get; set; }
    long? DiskUsageBytes { get; set; }
    bool IsHealthy();
    bool IsResponseTimeAcceptable(long thresholdMs = 5000);
    bool AreResourcesHealthy(decimal cpuThreshold = 90, decimal memoryThreshold = 85);
    string GetSummary();
}