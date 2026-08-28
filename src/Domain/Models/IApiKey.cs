#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================


namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Interface for API authentication key.
/// </summary>
public interface IApiKey
{
    Guid Id { get; set; }
    Guid UserId { get; set; }
    User? User { get; set; }
    string Name { get; set; }
    string KeyHash { get; set; }
    string KeyPrefix { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime? ExpiresAt { get; set; }
    DateTime? LastUsedAt { get; set; }
    bool IsActive { get; set; }
    string? AllowedIps { get; set; }
    string? AllowedScopes { get; set; }
    long ApiCallsCount { get; set; }
    string? Description { get; set; }
    bool IsValid();
    bool IsExpired();
    int? GetDaysUntilExpiration();
    bool IsIpAllowed(string sourceIp);
    bool HasScope(string requestedScope);
    void RecordUsage();
}