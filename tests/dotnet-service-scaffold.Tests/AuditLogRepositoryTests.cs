using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotnetServiceScaffold.Tests;

public class AuditLogRepositoryTests
{
    private readonly ServiceScaffoldDbContext _context;
    private readonly Mock<ILogger<AuditLogRepository>> _loggerMock;
    private readonly AuditLogRepository _repository;

    public AuditLogRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ServiceScaffoldDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var loggerMock = new Mock<ILogger<ServiceScaffoldDbContext>>();
        _context = new ServiceScaffoldDbContext(options, loggerMock.Object);
        _loggerMock = new Mock<ILogger<AuditLogRepository>>();
        _repository = new AuditLogRepository(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsLogsForUser()
    {
        var userId = Guid.NewGuid();
        _context.AuditLogs.Add(new AuditLog { UserId = userId, ActionName = "Login", EntityType = "User", CreatedAt = DateTime.UtcNow });
        _context.AuditLogs.Add(new AuditLog { UserId = Guid.NewGuid(), ActionName = "Logout", EntityType = "User", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _repository.GetByUserIdAsync(userId);

        result.Should().HaveCount(1);
        result.First().UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByEntityAsync_ReturnsLogsForEntity()
    {
        var entityId = Guid.NewGuid();
        var entityType = "User";
        _context.AuditLogs.Add(new AuditLog { EntityId = entityId, EntityType = entityType, ActionName = "Update", CreatedAt = DateTime.UtcNow });
        _context.AuditLogs.Add(new AuditLog { EntityId = Guid.NewGuid(), EntityType = "Product", ActionName = "Create", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEntityAsync(entityType, entityId);

        result.Should().HaveCount(1);
        result.First().EntityId.Should().Be(entityId);
    }

    [Fact]
    public async Task GetRecentLogsAsync_ReturnsMostRecentLogs()
    {
        for (int i = 0; i < 5; i++)
        {
            _context.AuditLogs.Add(new AuditLog { ActionName = "Action", EntityType = "Entity", CreatedAt = DateTime.UtcNow.AddMinutes(-i) });
        }
        await _context.SaveChangesAsync();

        var result = await _repository.GetRecentLogsAsync(2);

        result.Should().HaveCount(2);
        result.First().CreatedAt.Should().BeAfter(result.Last().CreatedAt);
    }

    [Fact]
    public async Task GetFailedActionsAsync_ReturnsOnlyFailures()
    {
        _context.AuditLogs.Add(new AuditLog { Status = "Failure", ActionName = "Action", EntityType = "Entity", CreatedAt = DateTime.UtcNow });
        _context.AuditLogs.Add(new AuditLog { Status = "Success", ActionName = "Action", EntityType = "Entity", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _repository.GetFailedActionsAsync();

        result.Should().OnlyContain(a => a.Status == "Failure");
    }

    [Fact]
    public async Task DeleteOldLogsAsync_RemovesLogsOlderThanThreshold()
    {
        var oldDate = DateTime.UtcNow.AddDays(-100);
        var recentDate = DateTime.UtcNow.AddDays(-10);
        _context.AuditLogs.Add(new AuditLog { ActionName = "Action", EntityType = "Entity", CreatedAt = oldDate });
        _context.AuditLogs.Add(new AuditLog { ActionName = "Action", EntityType = "Entity", CreatedAt = recentDate });
        await _context.SaveChangesAsync();

        await _repository.DeleteOldLogsAsync(90);

        _context.AuditLogs.Should().HaveCount(1);
        _context.AuditLogs.First().CreatedAt.Should().Be(recentDate);
    }
}
