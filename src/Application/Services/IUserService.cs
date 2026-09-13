#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Exceptions;
using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Service interface for user management and authentication.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Creates a new user with the specified email address, full name, and password.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="fullName">The user's full name.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task whose result is the created user.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/>, <paramref name="fullName"/>, or <paramref name="password"/> is null or whitespace.</exception>
    /// <exception cref="ServiceValidationException">Thrown when the email format is invalid, the email is already registered, or the password is too short.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while creating the user.</exception>
    Task<User> CreateUserAsync(string email, string fullName, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by email address.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task whose result is the user, or <see langword="null"/> if no matching user exists.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> is null or whitespace.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while retrieving the user.</exception>
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user with the specified email address and password.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task whose result is the authenticated user, or <see langword="null"/> if authentication fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> or <paramref name="password"/> is null or whitespace.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs during authentication.</exception>
    Task<User?> AuthenticateUserAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the specified user.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task whose result is the updated user.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <see langword="null"/>.</exception>
    /// <exception cref="ServiceValidationException">Thrown when the user data is invalid.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while updating the user.</exception>
    Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the user with the specified identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ServiceScaffoldException">Thrown when the user is not found.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while deleting the user.</exception>
    Task DeleteUserAsync(Guid userId);

    /// <summary>
    /// Retrieves all active users.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task whose result is the collection of active users.</returns>
    /// <exception cref="DataAccessException">Thrown when an error occurs while retrieving active users.</exception>
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a password for the user with the specified email address.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The password to validate.</param>
    /// <returns>A task whose result is <see langword="true"/> if the password is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> or <paramref name="password"/> is null or whitespace.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while validating the password.</exception>
    Task<bool> ValidatePasswordAsync(string email, string password);

    /// <summary>
    /// Changes the password for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="oldPassword">The user's current password.</param>
    /// <param name="newPassword">The user's new password.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task whose result is <see langword="true"/> if the password was changed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="oldPassword"/> or <paramref name="newPassword"/> is null or whitespace.</exception>
    /// <exception cref="ServiceScaffoldException">Thrown when the user is not found.</exception>
    /// <exception cref="ServiceValidationException">Thrown when the new password is too short.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while changing the password.</exception>
    Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlocks the user with the specified identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to unlock.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ServiceScaffoldException">Thrown when the user is not found.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while unlocking the user.</exception>
    Task UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user and their associated API keys.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task whose result is the user with API keys, or <see langword="null"/> if no matching user exists.</returns>
    /// <exception cref="DataAccessException">Thrown when an error occurs while retrieving the user.</exception>
    Task<User?> GetUserWithApiKeysAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an API key and retrieves its associated user.
    /// </summary>
    /// <param name="apiKey">The API key to validate.</param>
    /// <returns>A task whose result is the associated user, or <see langword="null"/> if the API key is invalid or expired.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="apiKey"/> is null or whitespace.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while validating the API key.</exception>
    Task<User?> ValidateApiKeyAsync(string apiKey);

    /// <summary>
    /// Searches for users whose details match the specified query.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="page">The one-based page number. Values less than one are normalized to one.</param>
    /// <param name="pageSize">The number of results per page. Values outside the supported range are normalized to a value from 1 through 100.</param>
    /// <returns>A task whose result is the collection of matching users.</returns>
    /// <exception cref="DataAccessException">Thrown when an error occurs while searching for users.</exception>
    Task<IEnumerable<User>> SearchUsersAsync(string query, int page = 1, int pageSize = 10);
}
