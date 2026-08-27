#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using DotnetServiceScaffold.Domain.Enums;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Defines the contract for a service registration.
/// </summary>
public interface IServiceRegistration
{
    Guid Id { get; set; }
    string ServiceName { get; set; }
    string? Description { get; set; }
    string HealthCheckUrl { get; set; }
    string Version { get; set; }
    string Endpoint { get; set; }
    ServiceStatus Status { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    DateTime? LastHealthCheckAt { get; set; }
    Guid OwnerId { get; set; }
    User? Owner { get; set; }
    int HealthCheckIntervalSeconds { get; set; }
    int TimeoutSeconds { get; set; }
    bool IsEnabled { get; set; }
    int ConsecutiveFailures { get; set; }
    int TotalRequests { get; set; }
    int SuccessfulRequests { get; set; }
    string? SystemdServiceName { get; set; }
    ICollection<HealthCheckResult> HealthCheckResults { get; set; }
}