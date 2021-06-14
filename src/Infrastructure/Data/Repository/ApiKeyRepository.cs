// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Repository for managing API key data.
/// </summary>
public class ApiKeyRepository : Repository<ApiKey>, IApiKeyRepository
{
    public ApiKeyRepository(ServiceScaffoldDbContext context) : base(context)
    {
    }

    public async Task<ApiKey?> GetByKeyPrefixAsync(string keyPrefix)
    {
        return await _dbSet.FirstOrDefaultAsync(ak => ak.KeyPrefix == keyPrefix);
    }

    public async Task<ApiKey?> GetByFullKeyHashAsync(string keyHash)
    {
        return await _dbSet.FirstOrDefaultAsync(ak => ak.KeyHash == keyHash);
    }

    public async Task<IEnumerable<ApiKey>> GetActiveApiKeysForUserAsync(Guid userId)
    {
        return await _dbSet.Where(ak => ak.UserId == userId && ak.IsActive && (!ak.ExpiresAt.HasValue || ak.ExpiresAt > DateTime.UtcNow))
                           .ToListAsync();
    }
}
