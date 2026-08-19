#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using DotnetServiceScaffold.Presentation.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotnetServiceScaffold.Tests;

public class AuditLogControllerTests
{
    private readonly Mock<IAuditLogRepository> _mockRepository;
    private readonly Mock<ILogger<AuditLogController>> _mockLogger;
    private readonly AuditLogController _controller;

    public AuditLogControllerTests()
    {
        _mockRepository = new Mock<IAuditLogRepository>();
        _mockLogger = new Mock<ILogger<AuditLogController>>();
        _controller = new AuditLogController(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ListAuditLogs_ReturnsOk_WithPaginatedData()
    {
        var mockLogs = new List<AuditLog> { new AuditLog { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, ActionName = "Test", EntityType = "TestType" } };
        var pagedResult = new PagedResult<AuditLog> { Items = mockLogs, Page = 1, PageSize = 50, TotalCount = 1, TotalPages = 1 };
        
        _mockRepository.Setup(r => r.GetFilteredAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AuditLog, bool>>?>(), 1, 50))
            .ReturnsAsync(pagedResult);

        var result = await _controller.ListAuditLogs(null, null, null, null, 1, 50);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeOfType<PagedAuditLogResponse>();
    }

    [Fact]
    public async Task GetAuditLog_ReturnsNotFound_WhenLogDoesNotExist()
    {
        var logId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<AuditLog>());

        var result = await _controller.GetAuditLog(logId);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetAuditLog_ReturnsOk_WhenLogExists()
    {
        var logId = Guid.NewGuid();
        var log = new AuditLog { Id = logId, CreatedAt = DateTime.UtcNow, ActionName = "Test", EntityType = "TestType" };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<AuditLog> { log });

        var result = await _controller.GetAuditLog(logId);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeOfType<AuditLogDto>();
    }

    [Fact]
    public async Task GetUserAuditLogs_ReturnsOk_WithUserLogs()
    {
        var userId = Guid.NewGuid();
        var mockLogs = new List<AuditLog> { new AuditLog { Id = Guid.NewGuid(), UserId = userId, CreatedAt = DateTime.UtcNow, ActionName = "Test", EntityType = "TestType" } };
        var pagedResult = new PagedResult<AuditLog> { Items = mockLogs, Page = 1, PageSize = 1000, TotalCount = 1, TotalPages = 1 };
        
        _mockRepository.Setup(r => r.GetByUserIdPagedAsync(userId, 1, 1000))
            .ReturnsAsync(pagedResult);

        var result = await _controller.GetUserAuditLogs(userId, 30);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeOfType<List<AuditLogDto>>();
    }
}
