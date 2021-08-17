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
/// User-specific repository with additional queries for user management.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
	public UserRepository(ServiceScaffoldDbContext context, ILogger<UserRepository> logger) : base(context, logger)
	{
	}

	public async Task<User?> GetByEmailAsync(string email)
	{
		_logger.LogDebug("Querying user by email: {Email}", email);
		return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
	}

	public async Task<IEnumerable<User>> GetActiveUsersAsync()
	{
		_logger.LogDebug("Querying active users");
		return await _dbSet
			.Where(u => u.IsActive && !u.IsLocked)
			.OrderBy(u => u.FullName)
			.ToListAsync();
	}

	public async Task<IEnumerable<User>> GetLockedUsersAsync()
	{
		_logger.LogDebug("Querying locked users");
		return await _dbSet
			.Where(u => u.IsLocked && u.LockedUntil > DateTime.UtcNow)
			.OrderByDescending(u => u.LockedUntil)
			.ToListAsync();
	}

	public async Task<bool> EmailExistsAsync(string email)
	{
		_logger.LogDebug("Checking if email exists: {Email}", email);
		return await _dbSet.AnyAsync(u => u.Email == email);
	}

	public async Task<User?> GetWithApiKeysAsync(Guid userId)
	{
		_logger.LogDebug("Querying user with API keys: {UserId}", userId);
		return await _dbSet
			.Include(u => u.ApiKeys)
			.FirstOrDefaultAsync(u => u.Id == userId);
	}
}