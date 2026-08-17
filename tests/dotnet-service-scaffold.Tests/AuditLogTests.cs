#nullable enable
using System;
using DotnetServiceScaffold.Domain.Models;
using Xunit;
using FluentAssertions;

namespace DotnetServiceScaffold.Tests;

public class AuditLogTests
{
    private AuditLog CreateValidAuditLog()
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            User = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = "hash"
            },
            ActionName = "TestAction",
            EntityType = "TestEntity",
            EntityId = Guid.NewGuid(),
            OldValues = "{}",
            NewValues = "{}",
            Status = "Success",
            IpAddress = "127.0.0.1",
            UserAgent = "TestAgent",
            Description = "Test description"
        };
    }

    [Fact]
    public void Properties_AreInitializedCorrectly_WhenSetViaObjectInitializer()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var createdAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User",
            PasswordHash = "hash"
        };

        var auditLog = new AuditLog
        {
            Id = id,
            UserId = userId,
            User = user,
            ActionName = "TestAction",
            EntityType = "TestEntity",
            EntityId = entityId,
            OldValues = "old",
            NewValues = "new",
            Status = "Pending",
            IpAddress = "192.168.1.1",
            UserAgent = "TestAgent",
            CreatedAt = createdAt,
            Description = "Test description"
        };

        auditLog.Id.Should().Be(id);
        auditLog.UserId.Should().Be(userId);
        auditLog.User.Should().Be(user);
        auditLog.ActionName.Should().Be("TestAction");
        auditLog.EntityType.Should().Be("TestEntity");
        auditLog.EntityId.Should().Be(entityId);
        auditLog.OldValues.Should().Be("old");
        auditLog.NewValues.Should().Be("new");
        auditLog.Status.Should().Be("Pending");
        auditLog.IpAddress.Should().Be("192.168.1.1");
        auditLog.UserAgent.Should().Be("TestAgent");
        auditLog.CreatedAt.Should().Be(createdAt);
        auditLog.Description.Should().Be("Test description");
    }

    [Fact]
    public void GetSummary_ReturnsExpectedString_WhenUserIsPresent()
    {
        var auditLog = CreateValidAuditLog();
        auditLog.ActionName = "Create";
        auditLog.EntityType = "User";
        auditLog.EntityId = Guid.NewGuid();

        var summary = auditLog.GetSummary();

        summary.Should().Contain(auditLog.User!.FullName);
        summary.Should().Contain("Create");
        summary.Should().Contain("User");
        summary.Should().Contain(auditLog.EntityId.ToString());
        summary.Should().Contain(auditLog.CreatedAt.ToString("O"));
    }

    [Fact]
    public void GetSummary_ReturnsExpectedString_WhenUserIsNull()
    {
        var auditLog = CreateValidAuditLog();
        auditLog.User = null;
        auditLog.ActionName = "Update";
        auditLog.EntityType = "Product";
        auditLog.EntityId = Guid.NewGuid();

        var summary = auditLog.GetSummary();

        summary.Should().Contain("System");
        summary.Should().Contain("Update");
        summary.Should().Contain("Product");
        summary.Should().Contain(auditLog.EntityId.ToString());
        summary.Should().Contain(auditLog.CreatedAt.ToString("O"));
    }

    [Fact]
    public void WasSuccessful_ReturnsTrue_WhenStatusIsSuccessOrNull()
    {
        var auditLog = CreateValidAuditLog();

        auditLog.Status = "Success";
        auditLog.WasSuccessful().Should().BeTrue();

        auditLog.Status = null;
        auditLog.WasSuccessful().Should().BeTrue();
    }

    [Fact]
    public void WasSuccessful_ReturnsFalse_WhenStatusIsNotSuccessOrNull()
    {
        var auditLog = CreateValidAuditLog();

        auditLog.Status = "Failed";
        auditLog.WasSuccessful().Should().BeFalse();

        auditLog.Status = "Error";
        auditLog.WasSuccessful().Should().BeFalse();

        auditLog.Status = "";
        auditLog.WasSuccessful().Should().BeFalse();
    }

    [Fact]
    public void GetActionDescription_ReturnsCorrectDescription_ForKnownActions()
    {
        var auditLog = CreateValidAuditLog();

        auditLog.ActionName = "Create";
        auditLog.GetActionDescription().Should().Be("Created");

        auditLog.ActionName = "Update";
        auditLog.GetActionDescription().Should().Be("Updated");

        auditLog.ActionName = "Delete";
        auditLog.GetActionDescription().Should().Be("Deleted");

        auditLog.ActionName = "Restore";
        auditLog.GetActionDescription().Should().Be("Restored");

        auditLog.ActionName = "Login";
        auditLog.GetActionDescription().Should().Be("Logged in");

        auditLog.ActionName = "Logout";
        auditLog.GetActionDescription().Should().Be("Logged out");

        auditLog.ActionName = "Export";
        auditLog.GetActionDescription().Should().Be("Exported");

        auditLog.ActionName = "Import";
        auditLog.GetActionDescription().Should().Be("Imported");
    }

    [Fact]
    public void GetActionDescription_ReturnsSameString_ForUnknownAction()
    {
        var auditLog = CreateValidAuditLog();
        auditLog.ActionName = "UnknownAction";

        var description = auditLog.GetActionDescription();

        description.Should().Be("UnknownAction");
    }

    [Fact]
    public void Properties_CanBeSetToNull_ForNullableReferenceTypes()
    {
        var auditLog = CreateValidAuditLog();
        auditLog.User = null;
        auditLog.UserAgent = null;
        auditLog.Description = null;

        auditLog.User.Should().BeNull();
        auditLog.UserAgent.Should().BeNull();
        auditLog.Description.Should().BeNull();
    }

    [Fact]
    public void Properties_CanBeSetToEmptyString_ForStringProperties()
    {
        var auditLog = CreateValidAuditLog();
        auditLog.ActionName = "";
        auditLog.EntityType = "";
        auditLog.OldValues = "";
        auditLog.NewValues = "";
        auditLog.Status = "";
        auditLog.IpAddress = "";
        auditLog.UserAgent = "";
        auditLog.Description = "";

        auditLog.ActionName.Should().BeEmpty();
        auditLog.EntityType.Should().BeEmpty();
        auditLog.OldValues.Should().BeEmpty();
        auditLog.NewValues.Should().BeEmpty();
        auditLog.Status.Should().BeEmpty();
        auditLog.IpAddress.Should().BeEmpty();
        auditLog.UserAgent.Should().BeEmpty();
        auditLog.Description.Should().BeEmpty();
    }
}