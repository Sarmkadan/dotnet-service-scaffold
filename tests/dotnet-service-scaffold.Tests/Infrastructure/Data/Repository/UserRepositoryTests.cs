#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static UserRepositoryTestsConstants;

/// <summary>
/// Tests for the UserRepository class.
/// </summary>
public class UserRepositoryTests : IDisposable, IUserRepositoryTests
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ServiceScaffoldDbContext> _dbContextOptions;

    /// <summary>
    /// Initializes a new instance of the UserRepositoryTests class.
    /// </summary>
    public UserRepositoryTests()
    {
        _connection = new SqliteConnection(InMemoryConnectionString);
        _connection.Open();

        _dbContextOptions = new DbContextOptionsBuilder<ServiceScaffoldDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Ensure the database is created
        using var context = new ServiceScaffoldDbContext(_dbContextOptions);
        context.Database.EnsureCreated();
    }

    /// <summary>
    /// Tests that adding a user to the database works correctly.
    /// </summary>
    [Fact]
    public async Task AddUserAsync_ShouldAddUserToDatabase()
    {
        // Arrange
        using var context = new ServiceScaffoldDbContext(_dbContextOptions);
        var userRepository = new UserRepository(context);
        var user = new User { Id = Guid.NewGuid(), Username = TestUserUsername, Email = TestUserEmail, FullName = TestUserFullName };

        // Act
        await userRepository.AddUserAsync(user);

        // Assert
        var addedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        addedUser.Should().NotBeNull();
        addedUser?.Username.Should().Be(TestUserUsername);
    }

    /// <summary>
    /// Tests that getting a user by ID works correctly when the user exists.
    /// </summary>
    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        using var context = new ServiceScaffoldDbContext(_dbContextOptions);
        var userRepository = new UserRepository(context);
        var user = new User { Id = Guid.NewGuid(), Username = TestUserUsername, Email = TestUserEmail, FullName = TestUserFullName };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await userRepository.GetUserByIdAsync(user.Id);

        // Assert
        result.Should().Be(user);
    }

    /// <summary>
    /// Tests that getting a user by ID returns null when the user does not exist.
    /// </summary>
    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        using var context = new ServiceScaffoldDbContext(_dbContextOptions);
        var userRepository = new UserRepository(context);

        // Act
        var result = await userRepository.GetUserByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that getting a user by username works correctly when the user exists.
    /// </summary>
    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        using var context = new ServiceScaffoldDbContext(_dbContextOptions);
        var userRepository = new UserRepository(context);
        var user = new User { Id = Guid.NewGuid(), Username = UniqueUserUsername, Email = UniqueUserEmail, FullName = UniqueUserFullName };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await userRepository.GetUserByUsernameAsync(UniqueUserUsername);

        // Assert
        result.Should().Be(user);
    }

    /// <summary>
    /// Tests that getting a user by username returns null when the user does not exist.
    /// </summary>
    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        using var context = new ServiceScaffoldDbContext(_dbContextOptions);
        var userRepository = new UserRepository(context);

        // Act
        var result = await userRepository.GetUserByUsernameAsync(NonExistentUsername);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that updating a user in the database works correctly.
    /// </summary>
    [Fact]
    public async Task UpdateUserAsync_ShouldUpdateUserInDatabase()
    {
        // Arrange
        using var context = new ServiceScaffoldDbContext(_dbContextOptions);
        var userRepository = new UserRepository(context);
        var user = new User { Id = Guid.NewGuid(), Username = OriginalUserUsername, Email = OriginalUserEmail, FullName = OriginalUserFullName };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        context.Entry(user).State = EntityState.Detached; // Detach to simulate a fresh object from an update operation

        user.Username = UpdatedUserUsername;
        user.Email = UpdatedUserEmail;

        // Act
        await userRepository.UpdateUserAsync(user);

        // Assert
        var updatedUser = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser?.Username.Should().Be(UpdatedUserUsername);
        updatedUser?.Email.Should().Be(UpdatedUserEmail);
    }

    /// <summary>
    /// Tests that deleting a user from the database works correctly.
    /// </summary>
    [Fact]
    public async Task DeleteUserAsync_ShouldRemoveUserFromDatabase()
    {
        // Arrange
        using var context = new ServiceScaffoldDbContext(_dbContextOptions);
        var userRepository = new UserRepository(context);
        var user = new User { Id = Guid.NewGuid(), Username = UserToDeleteUsername, Email = UserToDeleteEmail, FullName = UserToDeleteFullName };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        await userRepository.DeleteUserAsync(user.Id);

        // Assert
        var deletedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        deletedUser.Should().BeNull();
    }

    /// <summary>
    /// Releases unmanaged resources and performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
