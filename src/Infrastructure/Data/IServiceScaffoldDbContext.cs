#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DotnetServiceScaffold.Infrastructure.Data
{
    /// <summary>
    /// Interface for ServiceScaffoldDbContext to enable dependency injection and mocking.
    /// </summary>
    public interface IServiceScaffoldDbContext
    {
        DbSet<User> Users { get; set; }
        DbSet<ServiceRegistration> ServiceRegistrations { get; set; }
        DbSet<HealthCheckResult> HealthCheckResults { get; set; }
        DbSet<ServiceMetric> ServiceMetrics { get; set; }
        DbSet<ServiceEvent> ServiceEvents { get; set; }
        DbSet<ApiKey> ApiKeys { get; set; }
        DbSet<AuditLog> AuditLogs { get; set; }
        DbSet<ServiceConfiguration> ServiceConfigurations { get; set; }

        Task InitializeDatabaseAsync();
    }
}