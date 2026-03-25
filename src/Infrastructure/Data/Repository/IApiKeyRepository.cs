#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Interface for API key data access operations.
/// </summary>
public interface IApiKeyRepository : IRepository<ApiKey>
{
    Task<ApiKey?> GetByKeyPrefixAsync(string keyPrefix);
    Task<ApiKey?> GetByFullKeyHashAsync(string keyHash);
    Task<IEnumerable<ApiKey>> GetActiveApiKeysForUserAsync(Guid userId);
}
