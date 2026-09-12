#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Exceptions;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Service for user management, authentication, and password operations.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly ILogger<UserService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="apiKeyRepository">The API key repository.</param>
    /// <param name="logger">The logger.</param>
    public UserService(IUserRepository userRepository, IApiKeyRepository apiKeyRepository, ILogger<UserService> logger)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(apiKeyRepository);
        ArgumentNullException.ThrowIfNull(logger);
        _userRepository = userRepository;
        _apiKeyRepository = apiKeyRepository;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new user with the specified email, full name, and password.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="fullName">The user's full name.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created user.</returns>
    /// <exception cref="ArgumentNullException">Thrown when email, fullName, or password is null or whitespace.</exception>
    /// <exception cref="ServiceValidationException">Thrown when the email format is invalid, email is already registered, or password is too short.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while creating the user.</exception>
    public async Task<User> CreateUserAsync(string email, string fullName, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email), "Email is required");
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentNullException(nameof(fullName), "Full name is required");
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password), "Password is required");

        if (!email.Contains("@"))
            throw new ServiceValidationException("Invalid email format");

        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser is not null)
                throw new ServiceValidationException("Email already registered");

            if (password.Length < 8)
                throw new ServiceValidationException("Password must be at least 8 characters");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FullName = fullName,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _userRepository.AddAsync(user, cancellationToken);
            _logger.LogInformation("User created: {Email}", email);
            return created;
        }
        catch (ServiceValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", email);
            throw new DataAccessException($"Error creating user: {email}", ex);
        }
    }

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user if found, otherwise null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when email is null or whitespace.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while retrieving the user.</exception>
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email), "Email is required");

        try
        {
            return await _userRepository.GetByEmailAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
            throw new DataAccessException($"Error retrieving user by email: {email}", ex);
        }
    }

    /// <summary>
    /// Authenticates a user with the specified email and password.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authenticated user if successful, otherwise null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when email or password is null or whitespace.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs during authentication.</exception>
    public async Task<User?> AuthenticateUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email), "Email is required");
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password), "Password is required");

        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
            {
                _logger.LogWarning("Authentication failed: user not found {Email}", email);
                return null;
            }

            if (user.IsAccountLocked())
            {
                _logger.LogWarning("Authentication attempt on locked account: {Email}", email);
                return null;
            }

            if (!VerifyPasswordHash(password, user.PasswordHash))
            {
                user.RecordFailedLoginAttempt();
                await _userRepository.UpdateAsync(user, cancellationToken);
                _logger.LogWarning("Failed login attempt for {Email}", email);
                return null;
            }

            user.RecordSuccessfulLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);
            _logger.LogInformation("User authenticated successfully: {Email}", email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user: {Email}", email);
            throw new DataAccessException($"Error authenticating user: {email}", ex);
        }
    }

    /// <summary>
    /// Updates the specified user.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated user.</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
    /// <exception cref="ServiceValidationException">Thrown when the user data is invalid.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while updating the user.</exception>
    public async Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);

        if (!user.IsValid())
            throw new ServiceValidationException("User data is invalid");

        try
        {
            user.UpdatedAt = DateTime.UtcNow;
            var updated = await _userRepository.UpdateAsync(user, cancellationToken);
            _logger.LogInformation("User updated: {Email}", user.Email);
            return updated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
            throw new DataAccessException($"Error updating user: {user.Id}", ex);
        }
    }

    /// <summary>
    /// Deletes the user with the specified ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to delete.</param>
    /// <exception cref="ServiceScaffoldException">Thrown when the user with the specified ID is not found.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while deleting the user.</exception>
    public async Task DeleteUserAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                throw new ServiceScaffoldException($"User {userId} not found", "USER_NOT_FOUND");

            await _userRepository.DeleteAsync(userId);
            _logger.LogInformation("User deleted: {UserId}", userId);
        }
        catch (ServiceScaffoldException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            throw new DataAccessException($"Error deleting user: {userId}", ex);
        }
    }

    /// <summary>
    /// Retrieves all active users.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of active users.</returns>
    /// <exception cref="DataAccessException">Thrown when an error occurs while retrieving active users.</exception>
    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return await _userRepository.GetActiveUsersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active users");
            throw new DataAccessException("Error retrieving active users", ex);
        }
    }

    /// <summary>
    /// Validates the password for the user with the specified email.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The password to validate.</param>
    /// <returns>True if the password is valid, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when email or password is null or whitespace.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while validating the password.</exception>
    public async Task<bool> ValidatePasswordAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email), "Email is required");
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password), "Password is required");

        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return false;

            return VerifyPasswordHash(password, user.PasswordHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password for user: {Email}", email);
            throw new DataAccessException($"Error validating password for user: {email}", ex);
        }
    }

    /// <summary>
    /// Changes the password for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="oldPassword">The user's current password.</param>
    /// <param name="newPassword">The user's new password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the password was changed successfully, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when oldPassword or newPassword is null or whitespace.</exception>
    /// <exception cref="ServiceScaffoldException">Thrown when the user with the specified ID is not found.</exception>
    /// <exception cref="ServiceValidationException">Thrown when the new password is too short.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while changing the password.</exception>
    public async Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(oldPassword))
            throw new ArgumentNullException(nameof(oldPassword), "Old password is required");
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentNullException(nameof(newPassword), "New password is required");

        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
                throw new ServiceScaffoldException($"User {userId} not found", "USER_NOT_FOUND");

            if (!VerifyPasswordHash(oldPassword, user.PasswordHash))
            {
                _logger.LogWarning("Password change failed - invalid old password for user {UserId}", userId);
                return false;
            }

            if (newPassword.Length < 8)
                throw new ServiceValidationException("New password must be at least 8 characters");

            user.PasswordHash = HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);

            _logger.LogInformation("Password changed for user: {UserId}", userId);
            return true;
        }
        catch (ServiceScaffoldException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            throw new DataAccessException($"Error changing password for user: {userId}", ex);
        }
    }

    /// <summary>
    /// Unlocks the user with the specified ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to unlock.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ServiceScaffoldException">Thrown when the user with the specified ID is not found.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while unlocking the user.</exception>
    public async Task UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
                throw new ServiceScaffoldException($"User {userId} not found", "USER_NOT_FOUND");

            user.IsLocked = false;
            user.LockedUntil = null;
            user.LoginAttempts = 0;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);
            _logger.LogInformation("User unlocked: {UserId}", userId);
        }
        catch (ServiceScaffoldException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking user: {UserId}", userId);
            throw new DataAccessException($"Error unlocking user: {userId}", ex);
        }
    }

    /// <summary>
    /// Retrieves a user with their associated API keys.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user with API keys if found, otherwise null.</returns>
    /// <exception cref="DataAccessException">Thrown when an error occurs while retrieving the user with API keys.</exception>
    public async Task<User?> GetUserWithApiKeysAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return await _userRepository.GetWithApiKeysAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with API keys: {UserId}", userId);
            throw new DataAccessException($"Error retrieving user with API keys: {userId}", ex);
        }
    }

    /// <summary>
    /// Validates the specified API key and returns the associated user.
    /// </summary>
    /// <param name="apiKey">The API key to validate.</param>
    /// <returns>The user associated with the API key if valid, otherwise null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when apiKey is null or whitespace.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while validating the API key.</exception>
    public async Task<User?> ValidateApiKeyAsync(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentNullException(nameof(apiKey), "API key is required");

        try
        {
            if (apiKey.Length < 8) // API keys should be long enough to have a prefix
            {
                return null;
            }

            var keyPrefix = apiKey.Substring(0, Math.Min(8, apiKey.Length)); // Use a reasonable prefix length
            var apiKeyEntity = await _apiKeyRepository.GetByKeyPrefixAsync(keyPrefix);

            if (apiKeyEntity is null || !apiKeyEntity.IsValid())
            {
                _logger.LogWarning("Invalid or expired API key attempt with prefix: {KeyPrefix}", keyPrefix);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(apiKey, apiKeyEntity.KeyHash))
            {
                _logger.LogWarning("API key verification failed for prefix: {KeyPrefix}", keyPrefix);
                return null;
            }

            // Key is valid, update usage stats
            apiKeyEntity.RecordUsage();
            await _apiKeyRepository.UpdateAsync(apiKeyEntity);

            // Fetch the user associated with the API key, including their API keys if needed by other parts of the system
            return await _userRepository.GetWithApiKeysAsync(apiKeyEntity.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating API key");
            throw new DataAccessException("Error validating API key", ex);
        }
    }

    /// <summary>
    /// Hashes the specified password using BCrypt.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password.</returns>
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Verifies that the specified password matches the hashed value.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="hash">The hashed password to compare against.</param>
    /// <returns>True if the password matches the hash, otherwise false.</returns>
    private bool VerifyPasswordHash(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    /// <summary>
    /// Searches for users matching the specified query.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="page">The page number (1-based). Default is 1.</param>
    /// <param name="pageSize">The number of results per page. Default is 10, maximum is 100.</param>
    /// <returns>A collection of users matching the search query.</returns>
    /// <exception cref="DataAccessException">Thrown when an error occurs while searching for users.</exception>
    public async Task<IEnumerable<User>> SearchUsersAsync(string query, int page = 1, int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            _logger.LogDebug("Search query is empty, returning empty result");
            return Enumerable.Empty<User>();
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        try
        {
            _logger.LogInformation("Searching users with query: {Query}, page: {Page}, pageSize: {PageSize}", query, page, pageSize);
            return await _userRepository.SearchUsersAsync(query, page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with query: {Query}", query);
            throw new DataAccessException($"Error searching users: {query}", ex);
        }
    }
}
