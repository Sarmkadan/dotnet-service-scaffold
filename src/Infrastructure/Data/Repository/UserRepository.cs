#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotnetServiceScaffold.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// User-specific repository with additional queries for user management.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="context">The database context used to access users.</param>
    /// <param name="logger">The logger used to record repository operations.</param>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="logger"/> is <see langword="null"/>.</exception>
    public UserRepository(ServiceScaffoldDbContext context, ILogger<UserRepository> logger) : base(context, logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);
    }

    /// <summary>
    /// Retrieves the user with the specified email address.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The matching user, or <see langword="null"/> if no user is found.</returns>
    /// <exception cref="ArgumentException"><paramref name="email"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled.</exception>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrEmpty(email);
        _logger.LogInformation("GetByEmailAsync called with {Email}", email);
        _logger.LogDebug("Querying user by email: {Email}", email);
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    /// <summary>
    /// Retrieves active users who are not locked, ordered by full name.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A collection of active, unlocked users.</returns>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled.</exception>
    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("GetActiveUsersAsync called");
        _logger.LogDebug("Querying active users");
        return await _dbSet
            .Where(u => u.IsActive && !u.IsLocked)
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves users whose locks have not expired, ordered by lock expiration in descending order.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A collection of currently locked users.</returns>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled.</exception>
    public async Task<IEnumerable<User>> GetLockedUsersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("GetLockedUsersAsync called");
        _logger.LogDebug("Querying locked users");
        return await _dbSet
            .Where(u => u.IsLocked && u.LockedUntil > DateTime.UtcNow)
            .OrderByDescending(u => u.LockedUntil)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Determines whether a user with the specified email address exists.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if a user has the specified email address; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="email"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled.</exception>
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrEmpty(email);
        _logger.LogInformation("EmailExistsAsync called with {Email}", email);
        _logger.LogDebug("Checking if email exists: {Email}", email);
        return await _dbSet.AnyAsync(u => u.Email == email, cancellationToken);
    }

    /// <summary>
    /// Retrieves a user and the user's API keys by unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The matching user with API keys loaded, or <see langword="null"/> if no user is found.</returns>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled.</exception>
    public async Task<User?> GetWithApiKeysAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("GetWithApiKeysAsync called with {UserId}", userId);
        _logger.LogDebug("Querying user with API keys: {UserId}", userId);
        return await _dbSet
            .Include(u => u.ApiKeys)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    /// <summary>
    /// Searches users by email address or full name and returns the requested page ordered by full name.
    /// </summary>
    /// <param name="query">The text to find in user email addresses or full names.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of users to return.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A collection containing the requested page of matching users, or an empty collection when <paramref name="query"/> contains only white-space characters.</returns>
    /// <exception cref="ArgumentException"><paramref name="query"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> is canceled while the database query is executing.</exception>
    public async Task<IEnumerable<User>> SearchUsersAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(query);
        if (string.IsNullOrWhiteSpace(query))
        {
            _logger.LogWarning("SearchUsersAsync called with empty query, returning empty result");
            _logger.LogDebug("Search query is empty, returning empty result");
            return Enumerable.Empty<User>();
        }

        _logger.LogInformation("SearchUsersAsync called with {Query}, page: {Page}, pageSize: {PageSize}", query, page, pageSize);
        _logger.LogDebug("Searching users with query: {Query}, page: {Page}, pageSize: {PageSize}", query, page, pageSize);

        var normalizedQuery = query.Trim().ToLowerInvariant();

        return await _dbSet
            .Where(u => u.Email.ToLower().Contains(normalizedQuery) || u.FullName.ToLower().Contains(normalizedQuery))
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
