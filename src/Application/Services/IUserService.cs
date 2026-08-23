#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Service interface for user management and authentication.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="fullName">User full name</param>
    /// <param name="password">User password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created user</returns>
    Task<User> CreateUserAsync(string email, string fullName, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by email.
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, otherwise null</returns>
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user.
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="password">User password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The authenticated user, otherwise null</returns>
    Task<User?> AuthenticateUserAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default);

    Task DeleteUserAsync(Guid userId);

    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);

    Task<bool> ValidatePasswordAsync(string email, string password);

    Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);

    Task UnlockUserAsync(Guid userId);

    Task<User?> GetUserWithApiKeysAsync(Guid userId);
    Task<User?> ValidateApiKeyAsync(string apiKey);

    Task<IEnumerable<User>> SearchUsersAsync(string query, int page = 1, int pageSize = 10);
}
