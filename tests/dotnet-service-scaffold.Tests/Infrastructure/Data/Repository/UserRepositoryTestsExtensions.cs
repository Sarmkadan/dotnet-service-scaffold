#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Extension methods for UserRepositoryTests to provide additional testing utilities.
/// </summary>
public static class UserRepositoryTestsExtensions
{
    /// <summary>
    /// Creates and adds a test user to the database and returns the repository instance.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="username">The username for the test user</param>
    /// <param name="email">The email for the test user</param>
    /// <param name="fullName">The full name for the test user</param>
    /// <param name="passwordHash">The password hash for the test user</param>
    /// <returns>The UserRepository instance</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="username"/>, <paramref name="email"/>, or <paramref name="passwordHash"/> is null or whitespace.</exception>
    public static async Task<UserRepository> CreateAndAddTestUserAsync(
        this UserRepositoryTests test,
        string username,
        string email,
        string fullName = UserRepositoryTestsExtensionsConstants.DefaultFullName,
        string passwordHash = UserRepositoryTestsExtensionsConstants.DefaultPasswordHash)
    {
        ArgumentException.ThrowIfNullOrEmpty(username);
        ArgumentException.ThrowIfNullOrEmpty(email);
        ArgumentException.ThrowIfNullOrEmpty(passwordHash);

        // Arrange
        using var context = new ServiceScaffoldDbContext(test._dbContextOptions);
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<UserRepository>();
        var userRepository = new UserRepository(context, logger);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            FullName = fullName,
            PasswordHash = passwordHash,
            IsActive = true
        };

        // Act
        await userRepository.AddAsync(user);

        return userRepository;
    }

    /// <summary>
    /// Verifies that a user exists in the database with the specified properties.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="userRepository">The repository instance</param>
    /// <param name="expectedUsername">The expected username</param>
    /// <param name="expectedEmail">The expected email</param>
    /// <param name="expectedFullName">The expected full name</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="userRepository"/>, <paramref name="expectedUsername"/>, <paramref name="expectedEmail"/>, or <paramref name="expectedFullName"/> is null.</exception>
    public static async Task AssertUserExistsAsync(
        this UserRepositoryTests test,
        UserRepository userRepository,
        string expectedUsername,
        string expectedEmail,
        string expectedFullName = "Test User")
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentException.ThrowIfNullOrEmpty(expectedUsername);
        ArgumentException.ThrowIfNullOrEmpty(expectedEmail);
        ArgumentException.ThrowIfNullOrEmpty(expectedFullName);

        // Act - Query for user by email
        var result = await userRepository.GetByEmailAsync(expectedEmail);

        // Assert
        result.Should().NotBeNull("User should exist in database");
        result?.Username.Should().Be(expectedUsername, UserRepositoryTestsExtensionsConstants.UsernameShouldMatch);
        result?.Email.Should().Be(expectedEmail, UserRepositoryTestsExtensionsConstants.EmailShouldMatch);
        result?.FullName.Should().Be(expectedFullName, UserRepositoryTestsExtensionsConstants.FullNameShouldMatch);
    }

    /// <summary>
    /// Creates multiple test users and returns them as a collection.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="count">Number of users to create</param>
    /// <returns>Collection of created users</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is less than 1.</exception>
    public static async Task<IReadOnlyList<User>> CreateTestUsersAsync(
        this UserRepositoryTests test,
        int count)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        var users = new List<User>();

        // Arrange
        using var context = new ServiceScaffoldDbContext(test._dbContextOptions);
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<UserRepository>();
        var userRepository = new UserRepository(context, logger);

        for (int i = 0; i < count; i++)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = $"testuser{i}",
                Email = $"test{i}@example.com",
                FullName = $"Test User {i}",
                PasswordHash = $"hash-{i}",
                IsActive = true
            };
            users.Add(user);
            await userRepository.AddAsync(user);
        }

        return users.AsReadOnly();
    }

    /// <summary>
    /// Verifies that a user with the specified email exists in the database.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="userRepository">The repository instance</param>
    /// <param name="email">The email to check</param>
    /// <returns>True if user exists, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="userRepository"/> or <paramref name="email"/> is null.</exception>
    public static async Task<bool> AssertUserWithEmailExistsAsync(
        this UserRepositoryTests test,
        UserRepository userRepository,
        string email)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentException.ThrowIfNullOrEmpty(email);

        // Act
        var result = await userRepository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull(UserRepositoryTestsExtensionsConstants.UserWithEmailShouldExistFormat, email);
        return result != null;
    }

    /// <summary>
    /// Creates and adds a test user with minimal required properties.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="username">The username for the test user</param>
    /// <param name="email">The email for the test user</param>
    /// <returns>Tuple containing the created user and repository instance</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="test"/>, <paramref name="username"/>, or <paramref name="email"/> is null.</exception>
    public static async Task<(User User, UserRepository Repository)> CreateTestUserWithResultAsync(
        this UserRepositoryTests test,
        string username,
        string email)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentException.ThrowIfNullOrEmpty(username);
        ArgumentException.ThrowIfNullOrEmpty(email);

        // Arrange
        using var context = new ServiceScaffoldDbContext(test._dbContextOptions);
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<UserRepository>();
        var userRepository = new UserRepository(context, logger);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            FullName = "Test User",
            PasswordHash = "test-hash",
            IsActive = true
        };

        // Act
        await userRepository.AddAsync(user);

        return (user, userRepository);
    }
}