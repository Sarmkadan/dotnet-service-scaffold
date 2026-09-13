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
/// Repository for managing API key data.
/// </summary>
public class ApiKeyRepository : Repository<ApiKey>, IApiKeyRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyRepository"/> class.
    /// </summary>
    /// <param name="context">The database context used to access API key data.</param>
    /// <param name="logger">The logger used to record repository operations.</param>
    public ApiKeyRepository(ServiceScaffoldDbContext context, ILogger<ApiKeyRepository> logger) : base(context, logger)
    {
    }

    /// <summary>
    /// Gets an API key by its key prefix.
    /// </summary>
    /// <param name="keyPrefix">The key prefix to search for.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the matching API key,
    /// or <see langword="null"/> when no API key has the specified prefix.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="keyPrefix"/> is <see langword="null"/> or empty.</exception>
    public async Task<ApiKey?> GetByKeyPrefixAsync(string keyPrefix)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyPrefix);
        _logger.LogInformation("Getting API key by key prefix {KeyPrefix}", keyPrefix);
        var result = await _dbSet.FirstOrDefaultAsync(ak => ak.KeyPrefix == keyPrefix);
        _logger.LogInformation("Finished getting API key by key prefix {KeyPrefix}. Found: {Found}", keyPrefix, result != null);
        return result;
    }

    /// <summary>
    /// Gets an API key by the hash of its full key value.
    /// </summary>
    /// <param name="keyHash">The full key hash to search for.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the matching API key,
    /// or <see langword="null"/> when no API key has the specified hash.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="keyHash"/> is <see langword="null"/> or empty.</exception>
    public async Task<ApiKey?> GetByFullKeyHashAsync(string keyHash)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyHash);
        _logger.LogInformation("Getting API key by full key hash {KeyHash}", keyHash);
        var result = await _dbSet.FirstOrDefaultAsync(ak => ak.KeyHash == keyHash);
        _logger.LogInformation("Finished getting API key by full key hash {KeyHash}. Found: {Found}", keyHash, result != null);
        return result;
    }

    /// <summary>
    /// Gets the active, unexpired API keys that belong to a user.
    /// </summary>
    /// <param name="userId">The identifier of the user whose API keys to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the user's active,
    /// unexpired API keys.
    /// </returns>
    public async Task<IEnumerable<ApiKey>> GetActiveApiKeysForUserAsync(Guid userId)
    {
        _logger.LogInformation("Getting active API keys for user {UserId}", userId);
        var result = await _dbSet.Where(ak => ak.UserId == userId && ak.IsActive && (!ak.ExpiresAt.HasValue || ak.ExpiresAt > DateTime.UtcNow))
                           .ToListAsync();
        _logger.LogInformation("Finished getting active API keys for user {UserId}. Count: {Count}", userId, result.Count);
        return result;
    }
}
