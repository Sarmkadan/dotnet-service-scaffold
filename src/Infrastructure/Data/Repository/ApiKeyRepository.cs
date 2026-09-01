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
    public ApiKeyRepository(ServiceScaffoldDbContext context, ILogger<ApiKeyRepository> logger) : base(context, logger)
    {
    }

    public async Task<ApiKey?> GetByKeyPrefixAsync(string keyPrefix)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyPrefix);
        _logger.LogInformation("Getting API key by key prefix {KeyPrefix}", keyPrefix);
        var result = await _dbSet.FirstOrDefaultAsync(ak => ak.KeyPrefix == keyPrefix);
        _logger.LogInformation("Finished getting API key by key prefix {KeyPrefix}. Found: {Found}", keyPrefix, result != null);
        return result;
    }

    public async Task<ApiKey?> GetByFullKeyHashAsync(string keyHash)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyHash);
        _logger.LogInformation("Getting API key by full key hash {KeyHash}", keyHash);
        var result = await _dbSet.FirstOrDefaultAsync(ak => ak.KeyHash == keyHash);
        _logger.LogInformation("Finished getting API key by full key hash {KeyHash}. Found: {Found}", keyHash, result != null);
        return result;
    }

    public async Task<IEnumerable<ApiKey>> GetActiveApiKeysForUserAsync(Guid userId)
    {
        _logger.LogInformation("Getting active API keys for user {UserId}", userId);
        var result = await _dbSet.Where(ak => ak.UserId == userId && ak.IsActive && (!ak.ExpiresAt.HasValue || ak.ExpiresAt > DateTime.UtcNow))
                           .ToListAsync();
        _logger.LogInformation("Finished getting active API keys for user {UserId}. Count: {Count}", userId, result.Count);
        return result;
    }
}
