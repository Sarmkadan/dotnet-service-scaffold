#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Service interface for health check operations and monitoring.
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// Performs a health check for a specific service.
    /// </summary>
    /// <param name="serviceId">The ID of the service to check</param>
    /// <returns>The result of the health check</returns>
    Task<HealthCheckResult> PerformHealthCheckAsync(Guid serviceId);

    /// <summary>
    /// Retrieves the health check history for a service.
    /// </summary>
    /// <param name="serviceId">The ID of the service</param>
    /// <param name="count">The number of records to retrieve</param>
    /// <returns>A collection of health check results</returns>
    Task<IEnumerable<HealthCheckResult>> GetServiceHealthHistoryAsync(Guid serviceId, int count = 20);

    Task<decimal> GetServiceSuccessRateAsync(Guid serviceId, int minutesBack = 60);

    Task<string> GetServiceHealthStatusAsync(Guid serviceId);

    Task<IEnumerable<HealthCheckResult>> GetFailedChecksAsync(Guid serviceId, int hoursBack = 24);

    Task CleanupOldResultsAsync(int daysToKeep = 30);

    Task<HealthCheckResult> CreateHealthCheckResultAsync(Guid serviceId, int statusCode, long responseTimeMs, string? errorMessage = null);
}
