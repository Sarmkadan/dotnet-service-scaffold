#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Repository interface for ServiceConfiguration entity operations.
/// </summary>
public interface IConfigurationRepository : IRepository<ServiceConfiguration>
{
    Task<ServiceConfiguration?> GetByKeyAsync(string key, Guid? serviceId = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<ServiceConfiguration>> GetByServiceIdAsync(Guid serviceId, CancellationToken cancellationToken = default);

    Task<bool> KeyExistsAsync(string key, Guid? serviceId = null, CancellationToken cancellationToken = default);

    Task DeleteByKeyAsync(string key, Guid? serviceId = null);
}
