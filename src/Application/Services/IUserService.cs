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
    Task<User> CreateUserAsync(string email, string fullName, string password);

    Task<User?> GetUserByEmailAsync(string email);

    Task<User?> AuthenticateUserAsync(string email, string password);

    Task<User> UpdateUserAsync(User user);

    Task DeleteUserAsync(Guid userId);

    Task<IEnumerable<User>> GetActiveUsersAsync();

    Task<bool> ValidatePasswordAsync(string email, string password);

    Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);

    Task UnlockUserAsync(Guid userId);

    Task<User?> GetUserWithApiKeysAsync(Guid userId);
    Task<User?> ValidateApiKeyAsync(string apiKey);
}
