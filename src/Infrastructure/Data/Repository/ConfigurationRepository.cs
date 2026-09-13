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
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationRepository"/> class.
    /// </summary>
    /// <param name="context">The database context used to access configuration records.</param>
    /// <param name="logger">The logger used to record repository operations.</param>
    public ConfigurationRepository(ServiceScaffoldDbContext context, ILogger<ConfigurationRepository> logger) : base(context, logger)
    {
    }

    /// <summary>
    /// Retrieves the configuration with the specified key and service scope.
    /// </summary>
    /// <param name="key">The configuration key to locate.</param>
    /// <param name="serviceId">
    /// The identifier of the associated service, or <see langword="null"/> for a global configuration.
    /// </param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A task whose result is the matching configuration, or <see langword="null"/> if no match exists.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="key"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> is canceled.</exception>
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

    /// <summary>
    /// Retrieves all configurations associated with the specified service.
    /// </summary>
    /// <param name="serviceId">The identifier of the service whose configurations are retrieved.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task whose result contains the configurations ordered by key.</returns>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> is canceled.</exception>
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

    /// <summary>
    /// Determines whether a configuration exists with the specified key and service scope.
    /// </summary>
    /// <param name="key">The configuration key to locate.</param>
    /// <param name="serviceId">
    /// The identifier of the associated service, or <see langword="null"/> for a global configuration.
    /// </param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A task whose result is <see langword="true"/> if the configuration exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="key"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> is canceled.</exception>
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

    /// <summary>
    /// Deletes the configuration with the specified key and service scope if it exists.
    /// </summary>
    /// <param name="key">The configuration key to delete.</param>
    /// <param name="serviceId">
    /// The identifier of the associated service, or <see langword="null"/> for a global configuration.
    /// </param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    /// <exception cref="ArgumentException"><paramref name="key"/> is <see langword="null"/> or empty.</exception>
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
