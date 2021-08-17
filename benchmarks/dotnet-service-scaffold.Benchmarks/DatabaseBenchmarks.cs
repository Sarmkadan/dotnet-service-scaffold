#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

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

    [GlobalSetup]
    public async Task Setup()
    {
        // Create in-memory SQLite database for benchmarking
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ServiceScaffoldDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ServiceScaffoldDbContext(options, NullLogger<ServiceScaffoldDbContext>.Instance);
        await _dbContext.Database.EnsureCreatedAsync();
        await _dbContext.InitializeDatabaseAsync();

        // Create test data
        _testUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "benchmark@example.com",
            FullName = "Benchmark User",
            PasswordHash = "$2a$11$somehashedpassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _testService = new ServiceRegistration
        {
            Id = Guid.NewGuid(),
            ServiceName = "BenchmarkService",
            Description = "Service for benchmarking database operations",
            HealthCheckUrl = "http://localhost:8080/health",
            Version = "1.0.0",
            Endpoint = "http://localhost:8080",
            Status = ServiceStatus.Healthy,
            IsEnabled = true,
            OwnerId = _testUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Seed initial data
        _dbContext.Users.Add(_testUser);
        _dbContext.ServiceRegistrations.Add(_testService);
        await _dbContext.SaveChangesAsync();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        _connection?.Dispose();
    }

    [Benchmark(Description = "User Create - insert new user")]
    public async Task CreateUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "new_benchmark@example.com",
            FullName = "New Benchmark User",
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
        user.FullName = "Updated Benchmark User";
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
            Id = Guid.NewGuid(),
            ServiceName = "NewBenchmarkService",
            Description = "New service for benchmarking",
            HealthCheckUrl = "http://localhost:9090/health",
            Version = "1.0.0",
            Endpoint = "http://localhost:9090",
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

    [Benchmark(Description = "Bulk Create - insert 100 users in batch")]
    public async Task BulkCreateUsers()
    {
        var users = Enumerable.Range(0, 100).Select(i => new User
        {
            Id = Guid.NewGuid(),
            Email = $"bulk_{i}@example.com",
            FullName = $"Bulk User {i}",
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
                Id = Guid.NewGuid(),
                Email = $"tx_{i}@example.com",
                FullName = $"Transaction User {i}",
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