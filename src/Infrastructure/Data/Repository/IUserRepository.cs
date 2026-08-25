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
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<User>> GetLockedUsersAsync(CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetWithApiKeysAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<User>> SearchUsersAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default);
}
