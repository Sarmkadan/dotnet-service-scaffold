// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using NSubstitute;
using Xunit;
using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;

namespace DotnetServiceScaffold.Tests.Application.Services;

public class AuditServiceTests
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly AuditService _auditService;

    public AuditServiceTests()
    {
        _auditLogRepository = Substitute.For<IAuditLogRepository>();
        _auditService = new AuditService(_auditLogRepository);
    }

    [Fact]
    public async Task LogAuditAsync_ShouldAddAuditLogToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityType = "User";
        var entityId = Guid.NewGuid();
        var action = "Create";
        var details = "User created successfully";

        _auditLogRepository.AddAuditLogAsync(Arg.Any<AuditLog>()).Returns(Task.CompletedTask);

        // Act
        await _auditService.LogAuditAsync(userId, entityType, entityId, action, details);

        // Assert
        await _auditLogRepository.Received(1).AddAuditLogAsync(
            Arg.Is<AuditLog>(log =>
                log.UserId == userId &&
                log.EntityType == entityType &&
                log.EntityId == entityId &&
                log.Action == action &&
                log.Details == details));
    }

    [Fact]
    public async Task LogAuditAsync_ShouldSetCreatedAtTimestamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityType = "Service";
        var entityId = Guid.NewGuid();
        var action = "Update";
        var details = "Service updated";

        _auditLogRepository.AddAuditLogAsync(Arg.Any<AuditLog>()).Returns(Task.CompletedTask);

        // Act
        await _auditService.LogAuditAsync(userId, entityType, entityId, action, details);

        // Assert
        await _auditLogRepository.Received(1).AddAuditLogAsync(
            Arg.Is<AuditLog>(log => log.CreatedAt != default));
    }

    [Fact]
    public async Task GetAuditLogsForUserAsync_ShouldReturnLogsForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var logs = new List<AuditLog>
        {
            new AuditLog { UserId = userId, EntityType = "User", Action = "Login" },
            new AuditLog { UserId = userId, EntityType = "Service", Action = "View" }
        };
        _auditLogRepository.GetAuditLogsByUserIdAsync(userId).Returns(logs);

        // Act
        var result = await _auditService.GetAuditLogsForUserAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(logs);
        await _auditLogRepository.Received(1).GetAuditLogsByUserIdAsync(userId);
    }

    [Fact]
    public async Task GetAuditLogsForUserAsync_ShouldReturnEmpty_WhenNoLogsForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _auditLogRepository.GetAuditLogsByUserIdAsync(userId).Returns(new List<AuditLog>());

        // Act
        var result = await _auditService.GetAuditLogsForUserAsync(userId);

        // Assert
        result.Should().BeEmpty();
        await _auditLogRepository.Received(1).GetAuditLogsByUserIdAsync(userId);
    }

    [Fact]
    public async Task GetAuditLogsForEntityAsync_ShouldReturnLogsForEntity()
    {
        // Arrange
        var entityType = "Service";
        var entityId = Guid.NewGuid();
        var logs = new List<AuditLog>
        {
            new AuditLog { EntityType = entityType, EntityId = entityId, Action = "Create" },
            new AuditLog { EntityType = entityType, EntityId = entityId, Action = "Delete" }
        };
        _auditLogRepository.GetAuditLogsByEntityAsync(entityType, entityId).Returns(logs);

        // Act
        var result = await _auditService.GetAuditLogsForEntityAsync(entityType, entityId);

        // Assert
        result.Should().BeEquivalentTo(logs);
        await _auditLogRepository.Received(1).GetAuditLogsByEntityAsync(entityType, entityId);
    }

    [Fact]
    public async Task GetAuditLogsForEntityAsync_ShouldReturnEmpty_WhenNoLogsForEntity()
    {
        // Arrange
        var entityType = "Service";
        var entityId = Guid.NewGuid();
        _auditLogRepository.GetAuditLogsByEntityAsync(entityType, entityId).Returns(new List<AuditLog>());

        // Act
        var result = await _auditService.GetAuditLogsForEntityAsync(entityType, entityId);

        // Assert
        result.Should().BeEmpty();
        await _auditLogRepository.Received(1).GetAuditLogsByEntityAsync(entityType, entityId);
    }
}
