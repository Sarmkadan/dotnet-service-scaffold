#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Threading.Tasks;

namespace DotnetServiceScaffold.Tests.Application.Services;

/// <summary>
/// Interface for AuditServiceTests.
/// </summary>
public interface IAuditServiceTests
{
    Task LogAuditAsync_ShouldAddAuditLogToRepository();
    Task LogAuditAsync_ShouldSetCreatedAtTimestamp();
    Task GetAuditLogsForUserAsync_ShouldReturnLogsForUser();
    Task GetAuditLogsForUserAsync_ShouldReturnEmpty_WhenNoLogsForUser();
    Task GetAuditLogsForEntityAsync_ShouldReturnLogsForEntity();
    Task GetAuditLogsForEntityAsync_ShouldReturnEmpty_WhenNoLogsForEntity();
}