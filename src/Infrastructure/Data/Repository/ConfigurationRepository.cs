// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Configuration repository for application and service settings.
/// </summary>
public class ConfigurationRepository : Repository<ServiceConfiguration>, IConfigurationRepository
{
    public ConfigurationRepository(ServiceScaffoldDbContext context) : base(context)
    {
    }

    public async Task<ServiceConfiguration?> GetByKeyAsync(string key, Guid? serviceId = null)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Key == key && c.ServiceId == serviceId);
    }

    public async Task<IEnumerable<ServiceConfiguration>> GetByServiceIdAsync(Guid serviceId)
    {
        return await _dbSet
            .Where(c => c.ServiceId == serviceId)
            .OrderBy(c => c.Key)
            .ToListAsync();
    }

    public async Task<bool> KeyExistsAsync(string key, Guid? serviceId = null)
    {
        return await _dbSet
            .AnyAsync(c => c.Key == key && c.ServiceId == serviceId);
    }

    public async Task DeleteByKeyAsync(string key, Guid? serviceId = null)
    {
        var config = await GetByKeyAsync(key, serviceId);
        if (config != null)
        {
            _dbSet.Remove(config);
            await SaveChangesAsync();
        }
    }
}
