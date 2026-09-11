#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Exceptions;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Service for managing health checks and service monitoring.
/// </summary>
public class HealthCheckService : IHealthCheckService
{
    private readonly IHealthCheckRepository _healthCheckRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly HttpClient _httpClient;
    private readonly ILogger<HealthCheckService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckService"/> class.
    /// </summary>
    /// <param name="healthCheckRepository">The health check repository.</param>
    /// <param name="serviceRepository">The service repository.</param>
    /// <param name="httpClient">The HTTP client used to perform health checks.</param>
    /// <param name="logger">The logger.</param>
    public HealthCheckService(
        IHealthCheckRepository healthCheckRepository,
        IServiceRepository serviceRepository,
        HttpClient httpClient,
        ILogger<HealthCheckService> logger)
    {
        ArgumentNullException.ThrowIfNull(healthCheckRepository);
        ArgumentNullException.ThrowIfNull(serviceRepository);
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(logger);
        _healthCheckRepository = healthCheckRepository;
        _serviceRepository = serviceRepository;
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Performs a health check for the specified service.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service to check.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the health check result.</returns>
    /// <exception cref="ServiceNotFoundException">Thrown when the service with the given ID is not found.</exception>
    /// <exception cref="ServiceScaffoldException">Thrown when the service is disabled.</exception>
    public async Task<HealthCheckResult> PerformHealthCheckAsync(Guid serviceId)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service is null)
            throw new ServiceNotFoundException(serviceId);

        if (!service.IsEnabled)
        {
            _logger.LogWarning("Health check requested for disabled service {ServiceId}", serviceId);
            throw new ServiceScaffoldException($"Service {service.ServiceName} is disabled", "SERVICE_DISABLED");
        }

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(service.TimeoutSeconds));

            var response = await _httpClient.GetAsync(service.HealthCheckUrl, cts.Token);
            stopwatch.Stop();

            var result = new HealthCheckResult
            {
                ServiceId = serviceId,
                CheckedAt = DateTime.UtcNow,
                HttpStatusCode = (int)response.StatusCode,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Status = response.IsSuccessStatusCode ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                CheckMethod = "HTTP GET",
                CheckEndpoint = service.HealthCheckUrl
            };

            if (response.IsSuccessStatusCode)
            {
                service.RecordSuccessfulHealthCheck();
                _logger.LogInformation("Health check passed for service {ServiceId}", serviceId);
            }
            else
            {
                result.ErrorMessage = $"HTTP {response.StatusCode}";
                service.RecordFailedHealthCheck();
                _logger.LogWarning("Health check failed for service {ServiceId}: HTTP {StatusCode}", serviceId, response.StatusCode);
            }

            await _healthCheckRepository.AddAsync(result);
            await _serviceRepository.UpdateAsync(service);

            return result;
        }
        catch (OperationCanceledException)
        {
            var result = new HealthCheckResult
            {
                ServiceId = serviceId,
                CheckedAt = DateTime.UtcNow,
                Status = HealthStatus.Timeout,
                ErrorMessage = $"Health check timeout after {service.TimeoutSeconds} seconds",
                ResponseTimeMs = service.TimeoutSeconds * 1000
            };

            service.RecordFailedHealthCheck();
            await _healthCheckRepository.AddAsync(result);
            await _serviceRepository.UpdateAsync(service);

            _logger.LogWarning("Health check timeout for service {ServiceId}", serviceId);
            return result;
        }
        catch (HttpRequestException ex)
        {
            var result = new HealthCheckResult
            {
                ServiceId = serviceId,
                CheckedAt = DateTime.UtcNow,
                Status = HealthStatus.Error,
                ErrorMessage = ex.Message,
                ResponseTimeMs = 0
            };

            service.RecordFailedHealthCheck();
            await _healthCheckRepository.AddAsync(result);
            await _serviceRepository.UpdateAsync(service);

            _logger.LogError(ex, "Health check error for service {ServiceId}", serviceId);
            return result;
        }
    }

    /// <summary>
    /// Gets the health check history for the specified service.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service.</param>
    /// <param name="count">The maximum number of health check results to return. Default is 20.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the health check history.</returns>
    /// <exception cref="ServiceNotFoundException">Thrown when the service with the given ID is not found.</exception>
    public async Task<IEnumerable<HealthCheckResult>> GetServiceHealthHistoryAsync(Guid serviceId, int count = 20)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service is null)
            throw new ServiceNotFoundException(serviceId);

        return await _healthCheckRepository.GetRecentResultsAsync(serviceId, count);
    }

    /// <summary>
    /// Gets the success rate of health checks for the specified service within the specified time frame.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service.</param>
    /// <param name="minutesBack">The number of minutes in the past to consider for health checks. Default is 60.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the success rate as a percentage.</returns>
    /// <exception cref="ServiceNotFoundException">Thrown when the service with the given ID is not found.</exception>
    public async Task<decimal> GetServiceSuccessRateAsync(Guid serviceId, int minutesBack = 60)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service is null)
            throw new ServiceNotFoundException(serviceId);

        var threshold = DateTime.UtcNow.AddMinutes(-minutesBack);
        var results = await _healthCheckRepository.GetByServiceIdAsync(serviceId);

        var recentResults = results
            .Where(r => r.CheckedAt >= threshold)
            .ToList();

        if (recentResults.Count == 0)
            return 100m;

        var healthyCount = recentResults.Count(r => r.IsHealthy());
        return (decimal)healthyCount / recentResults.Count * 100;
    }

    /// <summary>
    /// Gets the current health status of the specified service.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the health status as a string.</returns>
    /// <exception cref="ServiceNotFoundException">Thrown when the service with the given ID is not found.</exception>
    public async Task<string> GetServiceHealthStatusAsync(Guid serviceId)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service is null)
            throw new ServiceNotFoundException(serviceId);

        if (!service.IsEnabled)
            return "Disabled";

        return service.Status.ToString();
    }

    /// <summary>
    /// Gets the failed health checks for the specified service within the specified time frame.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service.</param>
    /// <param name="hoursBack">The number of hours in the past to consider for failed health checks. Default is 24.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the failed health checks.</returns>
    /// <exception cref="ServiceNotFoundException">Thrown when the service with the given ID is not found.</exception>
    public async Task<IEnumerable<HealthCheckResult>> GetFailedChecksAsync(Guid serviceId, int hoursBack = 24)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service is null)
            throw new ServiceNotFoundException(serviceId);

        return await _healthCheckRepository.GetFailedResultsAsync(serviceId, hoursBack);
    }

    /// <summary>
    /// Cleans up old health check results for all services.
    /// </summary>
    /// <param name="daysToKeep">The number of days of health check results to keep. Older results will be deleted. Default is 30.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task CleanupOldResultsAsync(int daysToKeep = 30)
    {
        var services = await _serviceRepository.GetAllAsync();

        foreach (var service in services)
        {
            await _healthCheckRepository.DeleteOldResultsAsync(service.Id, daysToKeep);
        }

        _logger.LogInformation("Cleaned up health check results older than {DaysToKeep} days", daysToKeep);
    }

    /// <summary>
    /// Creates a health check result manually.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service.</param>
    /// <param name="statusCode">The HTTP status code of the health check.</param>
    /// <param name="responseTimeMs">The response time in milliseconds.</param>
    /// <param name="errorMessage">The error message if the health check failed, otherwise null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created health check result.</returns>
    /// <exception cref="ServiceNotFoundException">Thrown when the service with the given ID is not found.</exception>
    public async Task<HealthCheckResult> CreateHealthCheckResultAsync(
        Guid serviceId,
        int statusCode,
        long responseTimeMs,
        string? errorMessage = null)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service is null)
            throw new ServiceNotFoundException(serviceId);

        var result = new HealthCheckResult
        {
            ServiceId = serviceId,
            CheckedAt = DateTime.UtcNow,
            HttpStatusCode = statusCode,
            ResponseTimeMs = responseTimeMs,
            Status = statusCode >= 200 && statusCode < 300 ? HealthStatus.Healthy : HealthStatus.Unhealthy,
            ErrorMessage = errorMessage
        };

        if (result.IsHealthy())
            service.RecordSuccessfulHealthCheck();
        else
            service.RecordFailedHealthCheck();

        await _healthCheckRepository.AddAsync(result);
        await _serviceRepository.UpdateAsync(service);

        return result;
    }
}