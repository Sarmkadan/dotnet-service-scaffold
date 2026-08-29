#nullable enable
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

/// <summary>
/// Tests for the AuditService class.
/// </summary>
public class AuditServiceTests : IAuditServiceTests
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly AuditService _auditService;

    /// <summary>
    /// Initializes a new instance of the AuditServiceTests class.
    /// </summary>
    public AuditServiceTests()
    {
        _auditLogRepository = Substitute.For<IAuditLogRepository>();
        _auditService = new AuditService(_auditLogRepository);
    }

    /// <summary>
    /// Tests that the LogAuditAsync method adds an audit log to the repository.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
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

    /// <summary>
    /// Tests that the LogAuditAsync method sets the CreatedAt timestamp.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
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

    /// <summary>
    /// Tests that the GetAuditLogsForUserAsync method returns logs for a user.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve logs for.</param>
    /// <returns>A task that represents the asynchronous operation and returns a list of audit logs.</returns>
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

    /// <summary>
    /// Tests that the GetAuditLogsForUserAsync method returns an empty list when there are no logs for a user.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve logs for.</param>
    /// <returns>A task that represents the asynchronous operation and returns a list of audit logs.</returns>
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

    /// <summary>
    /// Tests that the GetAuditLogsForEntityAsync method returns logs for an entity.
    /// </summary>
    /// <param name="entityType">The type of the entity to retrieve logs for.</param>
    /// <param name="entityId">The ID of the entity to retrieve logs for.</param>
    /// <returns>A task that represents the asynchronous operation and returns a list of audit logs.</returns>
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

    /// <summary>
    /// Tests that the GetAuditLogsForEntityAsync method returns an empty list when there are no logs for an entity.
    /// </summary>
    /// <param name="entityType">The type of the entity to retrieve logs for.</param>
    /// <param name="entityId">The ID of the entity to retrieve logs for.</param>
    /// <returns>A task that represents the asynchronous operation and returns a list of audit logs.</returns>
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
