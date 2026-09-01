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
        _logger.LogInformation("GetByKeyAsync called with {Key}={Key}, {ServiceId}={ServiceId}", key, serviceId);
        ArgumentException.ThrowIfNullOrEmpty(key);
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Key == key && c.ServiceId == serviceId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in GetByKeyAsync for Key={Key}, ServiceId={ServiceId}", key, serviceId);
            throw;
        }
    }

    public async Task<IEnumerable<ServiceConfiguration>> GetByServiceIdAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetByServiceIdAsync called with {ServiceId}={ServiceId}", serviceId);
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return await _dbSet
                .Where(c => c.ServiceId == serviceId)
                .OrderBy(c => c.Key)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in GetByServiceIdAsync for ServiceId={ServiceId}", serviceId);
            throw;
        }
    }

    public async Task<bool> KeyExistsAsync(string key, Guid? serviceId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("KeyExistsAsync called with {Key}={Key}, {ServiceId}={ServiceId}", key, serviceId);
        ArgumentException.ThrowIfNullOrEmpty(key);
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return await _dbSet
                .AnyAsync(c => c.Key == key && c.ServiceId == serviceId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in KeyExistsAsync for Key={Key}, ServiceId={ServiceId}", key, serviceId);
            throw;
        }
    }

    public async Task DeleteByKeyAsync(string key, Guid? serviceId = null)
    {
        _logger.LogInformation("DeleteByKeyAsync called with {Key}={Key}, {ServiceId}={ServiceId}", key, serviceId);
        ArgumentException.ThrowIfNullOrEmpty(key);
        try
        {
            var config = await GetByKeyAsync(key, serviceId, CancellationToken.None);
            if (config is not null)
            {
                _dbSet.Remove(config);
                await SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in DeleteByKeyAsync for Key={Key}, ServiceId={ServiceId}", key, serviceId);
            throw;
        }
    }
}