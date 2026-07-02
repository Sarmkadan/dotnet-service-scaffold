#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Infrastructure.Caching;
using DotnetServiceScaffold.Infrastructure.Metrics;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetServiceScaffold.Benchmarks;

/// <summary>
/// Benchmarks for service management operations. Tests the main application services
/// that handle service registration, health checks, and service lifecycle management.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class ServiceOperationsBenchmarks
{
    private IServiceManagementService _serviceManagementService = default!;
    private IHealthCheckService _healthCheckService = default!;
    private IMetricsService _metricsService = default!;
    private InMemoryCacheService _cacheService = default!;
    private string _testServiceId = string.Empty;

    [GlobalSetup]
    public async Task Setup()
    {
        _cacheService = new InMemoryCacheService(NullLogger<InMemoryCacheService>.Instance);
        _metricsService = new MetricsService(NullLogger<MetricsService>.Instance);

        // Create services with dependencies
        _serviceManagementService = new ServiceManagementService(
            _cacheService,
            _metricsService,
            NullLogger<ServiceManagementService>.Instance
        );

        _healthCheckService = new HealthCheckService(
            new HttpClient(),
            _cacheService,
            _metricsService,
            NullLogger<HealthCheckService>.Instance
        );

        // Register a test service
        var serviceRegistration = new ServiceRegistrationDto
        {
            Name = "BenchmarkService",
            Description = "Service for benchmarking service operations",
            HealthCheckUrl = "http://localhost:8080/health",
            OwnerId = "user-benchmark",
            IsEnabled = true
        };

        var result = await _serviceManagementService.RegisterServiceAsync(serviceRegistration);
        _testServiceId = result.Data.Id;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _cacheService.Dispose();
    }

    [Benchmark(Description = "Service Registration - register new service")]
    public async Task RegisterService()
    {
        var serviceRegistration = new ServiceRegistrationDto
        {
            Name = "NewBenchmarkService",
            Description = "New service for benchmarking",
            HealthCheckUrl = "http://localhost:9090/health",
            OwnerId = "user-benchmark-2",
            IsEnabled = true
        };

        await _serviceManagementService.RegisterServiceAsync(serviceRegistration);
    }

    [Benchmark(Description = "Service Get - retrieve service by ID")]
    public async Task GetService()
    {
        await _serviceManagementService.GetServiceAsync(_testServiceId);
    }

    [Benchmark(Description = "Service List - get all registered services")]
    public async Task ListServices()
    {
        await _serviceManagementService.GetAllServicesAsync();
    }

    [Benchmark(Description = "Service Update - update service properties")]
    public async Task UpdateService()
    {
        var updateDto = new ServiceUpdateDto
        {
            Name = "UpdatedBenchmarkService",
            Description = "Updated description",
            IsEnabled = false
        };

        await _serviceManagementService.UpdateServiceAsync(_testServiceId, updateDto);
    }

    [Benchmark(Description = "Service Enable/Disable - toggle service status")]
    public async Task ToggleServiceStatus()
    {
        await _serviceManagementService.DisableServiceAsync(_testServiceId);
        await _serviceManagementService.EnableServiceAsync(_testServiceId);
    }

    [Benchmark(Description = "Health Check - perform health check on service")]
    public async Task PerformHealthCheck()
    {
        await _healthCheckService.CheckServiceHealthAsync(_testServiceId);
    }

    [Benchmark(Description = "Health Check History - get health check history")]
    public async Task GetHealthCheckHistory()
    {
        await _healthCheckService.GetHealthCheckHistoryAsync(_testServiceId, 7);
    }

    [Benchmark(Description = "Service Metrics - get service metrics")]
    public async Task GetServiceMetrics()
    {
        await _serviceManagementService.GetServiceMetricsAsync(_testServiceId);
    }

    [Benchmark(Description = "Service Success Rate - calculate service success rate")]
    public async Task GetServiceSuccessRate()
    {
        await _serviceManagementService.GetServiceSuccessRateAsync(_testServiceId, 30);
    }

    [Benchmark(Description = "Concurrent Service Operations - 50 parallel registrations")]
    public async Task ConcurrentServiceRegistrations()
    {
        var tasks = Enumerable.Range(0, 50).Select(async i =>
        {
            var serviceRegistration = new ServiceRegistrationDto
            {
                Name = $"ConcurrentService_{i}",
                Description = "Concurrent service registration",
                HealthCheckUrl = "http://localhost:8080/health",
                OwnerId = $"user-concurrent-{i}",
                IsEnabled = true
            };
            await _serviceManagementService.RegisterServiceAsync(serviceRegistration);
        });

        await Task.WhenAll(tasks);
    }

    [Benchmark(Description = "Service Search - search services by name pattern")]
    public async Task SearchServices()
    {
        await _serviceManagementService.SearchServicesAsync("Benchmark%");
    }

    [Benchmark(Description = "Service Cache Hit Rate - measure cache effectiveness")]
    public async Task MeasureCacheHitRate()
    {
        // Warm up cache
        await _serviceManagementService.GetServiceAsync(_testServiceId);

        // Measure cache hits
        for (int i = 0; i < 100; i++)
        {
            await _serviceManagementService.GetServiceAsync(_testServiceId);
        }
    }
}

public record ServiceRegistrationDto(string Name, string Description, string HealthCheckUrl, string OwnerId, bool IsEnabled);
public record ServiceUpdateDto(string Name, string Description, bool IsEnabled);