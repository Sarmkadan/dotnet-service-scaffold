using Moq;
using Xunit;
using DotnetServiceScaffold.Application.Extensions;
using DotnetServiceScaffold.Application.Services;

namespace DotnetServiceScaffold.Tests;

public class AuditServiceExtensionsTests
{
    private readonly Mock<IAuditService> _mockAuditService;

    public AuditServiceExtensionsTests()
    {
        _mockAuditService = new Mock<IAuditService>();
    }

    [Fact]
    public async Task LogAsync_CallsLogActionAsync_WithCorrectParameters()
    {
        const string message = "Test message";
        await _mockAuditService.Object.LogAsync(message);

        _mockAuditService.Verify(
            s => s.LogActionAsync(null, "MessageLogged", "System", null, message),
            Times.Once);
    }

    [Fact]
    public async Task LogAsync_ThrowsArgumentNullException_WhenAuditServiceIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => AuditServiceExtensions.LogAsync(null!, "Message"));
    }

    [Fact]
    public async Task LogAsync_ThrowsArgumentException_WhenMessageIsEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _mockAuditService.Object.LogAsync(string.Empty));
    }

    [Fact]
    public async Task LogActionAsync_CallsLogActionAsync_WithCorrectParameters()
    {
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        const string action = "TestAction";
        const string entityType = "TestEntity";
        const string description = "TestDescription";

        await _mockAuditService.Object.LogActionAsync(userId, action, entityType, entityId, description);

        _mockAuditService.Verify(
            s => s.LogActionAsync(userId, action, entityType, entityId, description),
            Times.Once);
    }

    [Fact]
    public async Task LogActionAsync_ThrowsArgumentNullException_WhenAuditServiceIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => AuditServiceExtensions.LogActionAsync(null!, Guid.NewGuid(), "Action", "Type", Guid.NewGuid()));
    }

    [Fact]
    public async Task LogFailedActionAsync_CallsLogFailedActionAsync_WithCorrectParameters()
    {
        var userId = Guid.NewGuid();
        const string action = "TestAction";
        const string entityType = "TestEntity";
        const string reason = "TestReason";

        await _mockAuditService.Object.LogFailedActionAsync(userId, action, entityType, reason);

        _mockAuditService.Verify(
            s => s.LogFailedActionAsync(userId, action, entityType, reason),
            Times.Once);
    }

    [Fact]
    public async Task LogFailedActionAsync_ThrowsArgumentNullException_WhenAuditServiceIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => AuditServiceExtensions.LogFailedActionAsync(null!, Guid.NewGuid(), "Action", "Type", "Reason"));
    }
}
