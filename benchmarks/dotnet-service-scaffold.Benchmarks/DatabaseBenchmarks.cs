#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DotnetServiceScaffold.Benchmarks;

/// <summary>
/// Benchmarks for database operations. Tests CRUD operations and query performance
/// for the most common database interactions in the service scaffold.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class DatabaseBenchmarks : IDisposable
{
    private ServiceScaffoldDbContext _dbContext = default!;
    private SqliteConnection _connection = default!;
    private User _testUser = default!;
    private ServiceRegistration _testService = default!;
    private HealthCheckResult _testHealthCheck = default!;

    [GlobalSetup]
    public async Task Setup()
    {
        // Create in-memory SQLite database for benchmarking
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ServiceScaffoldDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ServiceScaffoldDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        await _dbContext.InitializeDatabaseAsync();

        // Create test data
        _testUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = "benchmark_user",
            Email = "benchmark@example.com",
            PasswordHash = "$2a$11$somehashedpassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _testService = new ServiceRegistration
        {
            Id = Guid.NewGuid().ToString(),
            Name = "BenchmarkService",
            Description = "Service for benchmarking database operations",
            HealthCheckUrl = "http://localhost:8080/health",
            Status = ServiceStatus.Healthy,
            IsEnabled = true,
            OwnerId = _testUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _testHealthCheck = new HealthCheckResult
        {
            Id = Guid.NewGuid().ToString(),
            ServiceId = _testService.Id,
            Status = HealthStatus.Healthy,
            ResponseTime = 42,
            StatusCode = 200,
            Message = "Healthy",
            CheckedAt = DateTime.UtcNow
        };

        // Seed initial data
        _dbContext.Users.Add(_testUser);
        _dbContext.ServiceRegistrations.Add(_testService);
        _dbContext.HealthCheckResults.Add(_testHealthCheck);
        await _dbContext.SaveChangesAsync();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Benchmark(Description = "User Create - insert new user")]
    public async Task CreateUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = "new_benchmark_user",
            Email = "new_benchmark@example.com",
            PasswordHash = "$2a$11$anotherhashedpassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
    }

    [Benchmark(Description = "User Read - find user by email")]
    public async Task ReadUserByEmail()
    {
        await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == "benchmark@example.com");
    }

    [Benchmark(Description = "User Update - update user record")]
    public async Task UpdateUser()
    {
        var user = await _dbContext.Users
            .FirstAsync(u => u.Email == "benchmark@example.com");
        user.Username = "updated_benchmark_user";
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    [Benchmark(Description = "User Delete - remove user record")]
    public async Task DeleteUser()
    {
        var user = await _dbContext.Users
            .FirstAsync(u => u.Email == "benchmark@example.com");
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
    }

    [Benchmark(Description = "Service Create - insert new service")]
    public async Task CreateService()
    {
        var service = new ServiceRegistration
        {
            Id = Guid.NewGuid().ToString(),
            Name = "NewBenchmarkService",
            Description = "New service for benchmarking",
            HealthCheckUrl = "http://localhost:9090/health",
            Status = ServiceStatus.Healthy,
            IsEnabled = true,
            OwnerId = _testUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.ServiceRegistrations.Add(service);
        await _dbContext.SaveChangesAsync();
    }

    [Benchmark(Description = "Service List - get all services")]
    public async Task ListServices()
    {
        await _dbContext.ServiceRegistrations.ToListAsync();
    }

    [Benchmark(Description = "HealthCheck Create - insert health check result")]
    public async Task CreateHealthCheck()
    {
        var healthCheck = new HealthCheckResult
        {
            Id = Guid.NewGuid().ToString(),
            ServiceId = _testService.Id,
            Status = HealthStatus.Healthy,
            ResponseTime = 100,
            StatusCode = 200,
            Message = "Healthy",
            CheckedAt = DateTime.UtcNow
        };

        _dbContext.HealthCheckResults.Add(healthCheck);
        await _dbContext.SaveChangesAsync();
    }

    [Benchmark(Description = "HealthCheck Query - get recent health checks for service")]
    public async Task QueryHealthChecks()
    {
        await _dbContext.HealthCheckResults
            .Where(h => h.ServiceId == _testService.Id)
            .OrderByDescending(h => h.CheckedAt)
            .Take(10)
            .ToListAsync();
    }

    [Benchmark(Description = "Service Metrics Create - insert service metric")]
    public async Task CreateServiceMetric()
    {
        var metric = new ServiceMetric
        {
            Id = Guid.NewGuid().ToString(),
            ServiceId = _testService.Id,
            CpuUsage = 45.2m,
            MemoryUsage = 512,
            DiskUsage = 2048,
            AverageResponseTime = 125,
            RequestsPerMinute = 450,
            ErrorRate = 0.2m,
            RecordedAt = DateTime.UtcNow
        };

        _dbContext.ServiceMetrics.Add(metric);
        await _dbContext.SaveChangesAsync();
    }

    [Benchmark(Description = "AuditLog Create - insert audit log entry")]
    public async Task CreateAuditLog()
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            UserId = _testUser.Id,
            Action = "ServiceRegistered",
            EntityType = "Service",
            EntityId = _testService.Id,
            Changes = "{\"name\":\"BenchmarkService\",\"status\":\"Healthy\"}",
            Timestamp = DateTime.UtcNow,
            IpAddress = "192.168.1.100"
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync();
    }

    [Benchmark(Description = "Bulk Create - insert 100 users in batch")]
    public async Task BulkCreateUsers()
    {
        var users = Enumerable.Range(0, 100).Select(i => new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = $"bulk_user_{i}",
            Email = $"bulk_{i}@example.com",
            PasswordHash = "$2a$11$hashedpassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        _dbContext.Users.AddRange(users);
        await _dbContext.SaveChangesAsync();
    }

    [Benchmark(Description = "Transaction - commit 50 operations in transaction")]
    public async Task TransactionCommit()
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        for (int i = 0; i < 50; i++)
        {
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = $"tx_user_{i}",
                Email = $"tx_{i}@example.com",
                PasswordHash = "$2a$11$hashedpassword",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Users.Add(user);
        }

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}

public enum ServiceStatus
{
    Unknown,
    Healthy,
    Unhealthy,
    Degraded
}

public enum HealthStatus
{
    Unknown,
    Healthy,
    Unhealthy,
    Warning
}