#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Repository interface for User entity operations.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);

    Task<IEnumerable<User>> GetActiveUsersAsync();

    Task<IEnumerable<User>> GetLockedUsersAsync();

    Task<bool> EmailExistsAsync(string email);

    Task<User?> GetWithApiKeysAsync(Guid userId);

    Task<IEnumerable<User>> SearchUsersAsync(string query, int page, int pageSize);
}
