#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using FluentAssertions;
using Xunit;

/// <summary>
/// Integration tests for the AuditLogRepository.
/// </summary>
public class AuditLogRepositoryIntegrationTests : IntegrationTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogRepositoryIntegrationTests"/> class.
    /// </summary>
    public AuditLogRepositoryIntegrationTests()
    {
        _auditLogRepository = new AuditLogRepository(DbContext);
    }

    /// <summary>
    /// Tests that adding an audit log to the repository results in the audit log being added to the database.
    /// </summary>
    [Fact]
    public async Task AddAuditLog_ShouldAddAuditLogToDatabase()
    {
        // Arrange
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = "Login",
            EntityType = "User",
            EntityId = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Details = "User logged in successfully"
        };

        // Act
        await _auditLogRepository.AddAsync(auditLog);
        await DbContext.SaveChangesAsync();

        // Assert
        var retrievedAuditLog = await DbContext.AuditLogs.FindAsync(auditLog.Id);
        retrievedAuditLog.Should().NotBeNull();
        retrievedAuditLog!.Action.Should().Be("Login");
    }

    /// <summary>
    /// Tests that getting an audit log by ID from the repository results in the correct audit log being returned.
    /// </summary>
    [Fact]
    public async Task GetAuditLogById_ShouldReturnCorrectAuditLog()
    {
        // Arrange
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = "Logout",
            EntityType = "User",
            EntityId = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Details = "User logged out"
        };
        await _auditLogRepository.AddAsync(auditLog);
        await DbContext.SaveChangesAsync();

        // Act
        var retrievedAuditLog = await _auditLogRepository.GetByIdAsync(auditLog.Id);

        // Assert
        retrievedAuditLog.Should().NotBeNull();
        retrievedAuditLog!.Action.Should().Be("Logout");
    }

    /// <summary>
    /// Tests that updating an audit log in the repository results in the audit log being updated in the database.
    /// </summary>
    [Fact]
    public async Task UpdateAuditLog_ShouldUpdateAuditLogInDatabase()
    {
        // Arrange
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = "UpdateProfile",
            EntityType = "User",
            EntityId = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Details = "Profile updated"
        };
        await _auditLogRepository.AddAsync(auditLog);
        await DbContext.SaveChangesAsync();

        // Detach the entity
        DbContext.Entry(auditLog).State = EntityState.Detached;

        auditLog.Details = "Profile updated with new email";
        auditLog.Timestamp = DateTime.UtcNow.AddMinutes(1);

        // Act
        _auditLogRepository.Update(auditLog);
        await DbContext.SaveChangesAsync();

        // Assert
        var updatedAuditLog = await DbContext.AuditLogs.FindAsync(auditLog.Id);
        updatedAuditLog.Should().NotBeNull();
        updatedAuditLog!.Details.Should().Be("Profile updated with new email");
    }

    /// <summary>
    /// Tests that deleting an audit log from the repository results in the audit log being removed from the database.
    /// </summary>
    [Fact]
    public async Task DeleteAuditLog_ShouldRemoveAuditLogFromDatabase()
    {
        // Arrange
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = "DeleteData",
            EntityType = "Data",
            EntityId = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Details = "Data deleted"
        };
        await _auditLogRepository.AddAsync(auditLog);
        await DbContext.SaveChangesAsync();

        // Act
        _auditLogRepository.Delete(auditLog);
        await DbContext.SaveChangesAsync();

        // Assert
        var deletedAuditLog = await DbContext.AuditLogs.FindAsync(auditLog.Id);
        deletedAuditLog.Should().BeNull();
    }

    /// <summary>
    /// Tests that getting all audit logs from the repository results in all audit logs being returned.
    /// </summary>
    [Fact]
    public async Task GetAllAuditLogs_ShouldReturnAllAuditLogs()
    {
        // Arrange
        await _auditLogRepository.AddAsync(new AuditLog { Id = Guid.NewGuid(), Action = "Action1", EntityType = "Type1", EntityId = Guid.NewGuid().ToString(), UserId = Guid.NewGuid(), Timestamp = DateTime.UtcNow, Details = "Details1" });
        await _auditLogRepository.AddAsync(new AuditLog { Id = Guid.NewGuid(), Action = "Action2", EntityType = "Type2", EntityId = Guid.NewGuid().ToString(), UserId = Guid.NewGuid(), Timestamp = DateTime.UtcNow, Details = "Details2" });
        await DbContext.SaveChangesAsync();

        // Act
        var auditLogs = await _auditLogRepository.GetAllAsync();

        // Assert
        auditLogs.Should().NotBeNull().And.HaveCount(2);
    }

    /// <summary>
    /// Tests that getting an audit log by a non-existent ID from the repository results in null being returned.
    /// </summary>
    [Fact]
    public async Task GetAuditLogByNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var retrievedAuditLog = await _auditLogRepository.GetByIdAsync(nonExistentId);

        // Assert
        retrievedAuditLog.Should().BeNull();
    }
}
