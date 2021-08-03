// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// User-specific repository with additional queries for user management.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ServiceScaffoldDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _dbSet
            .Where(u => u.IsActive && !u.IsLocked)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetLockedUsersAsync()
    {
        return await _dbSet
            .Where(u => u.IsLocked && u.LockedUntil > DateTime.UtcNow)
            .OrderByDescending(u => u.LockedUntil)
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task<User?> GetWithApiKeysAsync(Guid userId)
    {
        return await _dbSet
            .Include(u => u.ApiKeys)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
}
