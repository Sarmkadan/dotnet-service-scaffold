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

    public UserService(IUserRepository userRepository, IApiKeyRepository apiKeyRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _apiKeyRepository = apiKeyRepository;
        _logger = logger;
    }

    public async Task<User> CreateUserAsync(string email, string fullName, string password)
    {
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

            var created = await _userRepository.AddAsync(user);
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

    public async Task<User?> GetUserByEmailAsync(string email)
    {
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

    public async Task<User?> AuthenticateUserAsync(string email, string password)
    {
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
                await _userRepository.UpdateAsync(user);
                _logger.LogWarning("Failed login attempt for {Email}", email);
                return null;
            }

            user.RecordSuccessfulLogin();
            await _userRepository.UpdateAsync(user);
            _logger.LogInformation("User authenticated successfully: {Email}", email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user: {Email}", email);
            throw new DataAccessException($"Error authenticating user: {email}", ex);
        }
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "User is required");

        if (!user.IsValid())
            throw new ServiceValidationException("User data is invalid");

        try
        {
            user.UpdatedAt = DateTime.UtcNow;
            var updated = await _userRepository.UpdateAsync(user);
            _logger.LogInformation("User updated: {Email}", user.Email);
            return updated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
            throw new DataAccessException($"Error updating user: {user.Id}", ex);
        }
    }

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

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
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

    public async Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(oldPassword))
            throw new ArgumentNullException(nameof(oldPassword), "Old password is required");
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentNullException(nameof(newPassword), "New password is required");

        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
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
            await _userRepository.UpdateAsync(user);

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

    public async Task UnlockUserAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                throw new ServiceScaffoldException($"User {userId} not found", "USER_NOT_FOUND");

            user.IsLocked = false;
            user.LockedUntil = null;
            user.LoginAttempts = 0;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
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

    public async Task<User?> GetUserWithApiKeysAsync(Guid userId)
    {
        try
        {
            return await _userRepository.GetWithApiKeysAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with API keys: {UserId}", userId);
            throw new DataAccessException($"Error retrieving user with API keys: {userId}", ex);
        }
    }

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

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPasswordHash(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
