#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

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
    /// <returns>The UserRepository instance</returns>
    public static async Task<UserRepository> CreateAndAddTestUserAsync(
        this UserRepositoryTests test,
        string username,
        string email,
        string fullName = "Test User")
    {
        // Arrange
        using var context = new ServiceScaffoldDbContext(test._dbContextOptions);
        var userRepository = new UserRepository(context);
        var user = new User { Id = Guid.NewGuid(), Username = username, Email = email, FullName = fullName };

        // Act
        await userRepository.AddUserAsync(user);

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
    public static async Task AssertUserExistsAsync(
        this UserRepositoryTests test,
        UserRepository userRepository,
        string expectedUsername,
        string expectedEmail,
        string expectedFullName = "Test User")
    {
        // Act
        var result = await userRepository.GetUserByUsernameAsync(expectedUsername);

        // Assert
        result.Should().NotBeNull("User should exist in database");
        result?.Username.Should().Be(expectedUsername, "Username should match");
        result?.Email.Should().Be(expectedEmail, "Email should match");
        result?.FullName.Should().Be(expectedFullName, "Full name should match");
    }

    /// <summary>
    /// Creates multiple test users and returns them as a collection.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="count">Number of users to create</param>
    /// <returns>Collection of created users</returns>
    public static async Task<IReadOnlyList<User>> CreateTestUsersAsync(
        this UserRepositoryTests test,
        int count)
    {
        var users = new List<User>();

        // Arrange
        using var context = new ServiceScaffoldDbContext(test._dbContextOptions);
        var userRepository = new UserRepository(context);

        for (int i = 0; i < count; i++)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = $"testuser{i}",
                Email = $"test{i}@example.com",
                FullName = $"Test User {i}"
            };
            users.Add(user);
            await userRepository.AddUserAsync(user);
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
    public static async Task<bool> AssertUserWithEmailExistsAsync(
        this UserRepositoryTests test,
        UserRepository userRepository,
        string email)
    {
        // Act
        var result = await userRepository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull("User with email {0} should exist", email);
        return result != null;
    }
}