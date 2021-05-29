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

    public HealthCheckService(
        IHealthCheckRepository healthCheckRepository,
        IServiceRepository serviceRepository,
        HttpClient httpClient,
        ILogger<HealthCheckService> logger)
    {
        _healthCheckRepository = healthCheckRepository;
        _serviceRepository = serviceRepository;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> PerformHealthCheckAsync(Guid serviceId)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service == null)
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

    public async Task<IEnumerable<HealthCheckResult>> GetServiceHealthHistoryAsync(Guid serviceId, int count = 20)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service == null)
            throw new ServiceNotFoundException(serviceId);

        return await _healthCheckRepository.GetRecentResultsAsync(serviceId, count);
    }

    public async Task<decimal> GetServiceSuccessRateAsync(Guid serviceId, int minutesBack = 60)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service == null)
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

    public async Task<string> GetServiceHealthStatusAsync(Guid serviceId)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service == null)
            throw new ServiceNotFoundException(serviceId);

        if (!service.IsEnabled)
            return "Disabled";

        return service.Status.ToString();
    }

    public async Task<IEnumerable<HealthCheckResult>> GetFailedChecksAsync(Guid serviceId, int hoursBack = 24)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service == null)
            throw new ServiceNotFoundException(serviceId);

        return await _healthCheckRepository.GetFailedResultsAsync(serviceId, hoursBack);
    }

    public async Task CleanupOldResultsAsync(int daysToKeep = 30)
    {
        var services = await _serviceRepository.GetAllAsync();

        foreach (var service in services)
        {
            await _healthCheckRepository.DeleteOldResultsAsync(service.Id, daysToKeep);
        }

        _logger.LogInformation("Cleaned up health check results older than {DaysToKeep} days", daysToKeep);
    }

    public async Task<HealthCheckResult> CreateHealthCheckResultAsync(
        Guid serviceId,
        int statusCode,
        long responseTimeMs,
        string? errorMessage = null)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service == null)
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
