#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using NSubstitute;
using Xunit;
using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using DotnetServiceScaffold.Domain.Exceptions;

namespace DotnetServiceScaffold.Tests.Application.Services;

/// <summary>
/// Tests for the UserService class.
/// </summary>
public class UserServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly UserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserServiceTests"/> class.
    /// </summary>
    public UserServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _userService = new UserService(_userRepository);
    }

    /// <summary>
    /// Tests that GetUserByIdAsync returns the user when the user exists.
    /// </summary>
    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new User { Id = userId, Username = "testuser" };
        _userRepository.GetUserByIdAsync(userId).Returns(expectedUser);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().Be(expectedUser);
        await _userRepository.Received(1).GetUserByIdAsync(userId);
    }

    /// <summary>
    /// Tests that GetUserByIdAsync returns null when the user does not exist.
    /// </summary>
    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetUserByIdAsync(userId).Returns((User)null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
        await _userRepository.Received(1).GetUserByIdAsync(userId);
    }

    /// <summary>
    /// Tests that CreateUserAsync returns the user when the user is created successfully.
    /// </summary>
    [Fact]
    public async Task CreateUserAsync_ShouldReturnUser_WhenUserIsCreatedSuccessfully()
    {
        // Arrange
        var newUser = new User { Username = "newuser", Email = "new@example.com" };
        _userRepository.AddUserAsync(Arg.Any<User>()).Returns(Task.CompletedTask);
        _userRepository.GetUserByUsernameAsync(newUser.Username).Returns((User)null);

        // Act
        var result = await _userService.CreateUserAsync(newUser);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(newUser.Username);
        result.Email.Should().Be(newUser.Email);
        await _userRepository.Received(1).AddUserAsync(Arg.Is<User>(u => u.Username == newUser.Username));
    }

    /// <summary>
    /// Tests that CreateUserAsync throws an exception when the username already exists.
    /// </summary>
    [Fact]
    public async Task CreateUserAsync_ShouldThrowException_WhenUsernameAlreadyExists()
    {
        // Arrange
        var existingUser = new User { Username = "existinguser", Email = "existing@example.com" };
        _userRepository.GetUserByUsernameAsync(existingUser.Username).Returns(existingUser);

        // Act
        Func<Task> action = async () => await _userService.CreateUserAsync(existingUser);

        // Assert
        await action.Should().ThrowAsync<ServiceScaffoldException>()
                    .WithMessage("Username 'existinguser' is already taken.");
        await _userRepository.DidNotReceive().AddUserAsync(Arg.Any<User>());
    }

    /// <summary>
    /// Tests that UpdateUserAsync updates the user when the user exists.
    /// </summary>
    [Fact]
    public async Task UpdateUserAsync_ShouldUpdateUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId, Username = "olduser", Email = "old@example.com" };
        var updatedUser = new User { Id = userId, Username = "updateduser", Email = "updated@example.com" };

        _userRepository.GetUserByIdAsync(userId).Returns(existingUser);
        _userRepository.UpdateUserAsync(Arg.Any<User>()).Returns(Task.CompletedTask);

        // Act
        await _userService.UpdateUserAsync(updatedUser);

        // Assert
        await _userRepository.Received(1).GetUserByIdAsync(userId);
        await _userRepository.Received(1).UpdateUserAsync(Arg.Is<User>(u => u.Username == updatedUser.Username));
    }

    /// <summary>
    /// Tests that UpdateUserAsync throws an exception when the user does not exist.
    /// </summary>
    [Fact]
    public async Task UpdateUserAsync_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updatedUser = new User { Id = userId, Username = "nonexistent", Email = "nonexistent@example.com" };

        _userRepository.GetUserByIdAsync(userId).Returns((User)null);

        // Act
        Func<Task> action = async () => await _userService.UpdateUserAsync(updatedUser);

        // Assert
        await action.Should().ThrowAsync<ServiceScaffoldException>()
                    .WithMessage($"User with ID '{userId}' not found.");
        await _userRepository.DidNotReceive().UpdateUserAsync(Arg.Any<User>());
    }

    /// <summary>
    /// Tests that DeleteUserAsync deletes the user when the user exists.
    /// </summary>
    [Fact]
    public async Task DeleteUserAsync_ShouldDeleteUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId, Username = "todelete" };
        _userRepository.GetUserByIdAsync(userId).Returns(existingUser);
        _userRepository.DeleteUserAsync(userId).Returns(Task.CompletedTask);

        // Act
        await _userService.DeleteUserAsync(userId);

        // Assert
        await _userRepository.Received(1).GetUserByIdAsync(userId);
        await _userRepository.Received(1).DeleteUserAsync(userId);
    }
}
