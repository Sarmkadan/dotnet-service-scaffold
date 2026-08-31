#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Configuration repository for application and service settings.
/// </summary>
public class ConfigurationRepository : Repository<ServiceConfiguration>, IConfigurationRepository
{
    public ConfigurationRepository(ServiceScaffoldDbContext context, ILogger<ConfigurationRepository> logger) : base(context, logger)
    {
    }

    public async Task<ServiceConfiguration?> GetByKeyAsync(string key, Guid? serviceId = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        cancellationToken.ThrowIfCancellationRequested();
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Key == key && c.ServiceId == serviceId, cancellationToken);
    }

    public async Task<IEnumerable<ServiceConfiguration>> GetByServiceIdAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _dbSet
            .Where(c => c.ServiceId == serviceId)
            .OrderBy(c => c.Key)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> KeyExistsAsync(string key, Guid? serviceId = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        cancellationToken.ThrowIfCancellationRequested();
        return await _dbSet
            .AnyAsync(c => c.Key == key && c.ServiceId == serviceId, cancellationToken);
    }

    public async Task DeleteByKeyAsync(string key, Guid? serviceId = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        var config = await GetByKeyAsync(key, serviceId, CancellationToken.None);
        if (config is not null)
        {
            _dbSet.Remove(config);
            await SaveChangesAsync();
        }
    }
}
