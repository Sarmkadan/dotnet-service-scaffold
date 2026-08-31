#nullable enable

using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Domain.Exceptions;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class UserServiceCrudTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly UserService _userService;

    public UserServiceCrudTests()
    {
        var apiKeyRepository = Substitute.For<IApiKeyRepository>();
        var logger = Substitute.For<ILogger<UserService>>();
        _userService = new UserService(_userRepository, apiKeyRepository, logger);
    }

    [Fact]
    public async Task CreateUserAsync_WithValidInput_AddsAndReturnsUser()
    {
        const string email = "new.user@example.com";
        const string fullName = "New User";
        const string password = "password123";
        _userRepository.GetByEmailAsync(email).Returns((User?)null);
        _userRepository.AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<User>());

        var result = await _userService.CreateUserAsync(email, fullName, password);

        result.Email.Should().Be(email);
        result.FullName.Should().Be(fullName);
        result.PasswordHash.Should().NotBeNullOrWhiteSpace().And.NotBe(password);
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(user => user.Email == email && user.FullName == fullName),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateUserAsync_WithInvalidEmail_ThrowsValidationException()
    {
        Func<Task> act = () => _userService.CreateUserAsync("invalid-email", "New User", "password123");

        await act.Should().ThrowAsync<ServiceValidationException>()
            .WithMessage("Invalid email format");
        await _userRepository.DidNotReceive().AddAsync(
            Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserByEmailAsync_WhenUserExists_ReturnsUser()
    {
        var expected = CreateValidUser();
        _userRepository.GetByEmailAsync(expected.Email).Returns(expected);

        var result = await _userService.GetUserByEmailAsync(expected.Email);

        result.Should().BeSameAs(expected);
        await _userRepository.Received(1).GetByEmailAsync(expected.Email);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        const string email = "missing@example.com";
        _userRepository.GetByEmailAsync(email).Returns((User?)null);

        var result = await _userService.GetUserByEmailAsync(email);

        result.Should().BeNull();
        await _userRepository.Received(1).GetByEmailAsync(email);
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidUser_UpdatesAndReturnsUser()
    {
        var user = CreateValidUser();
        var originalUpdatedAt = user.UpdatedAt;
        _userRepository.UpdateAsync(user, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _userService.UpdateUserAsync(user);

        result.Should().BeSameAs(user);
        user.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateUserAsync_WithInvalidUser_ThrowsValidationException()
    {
        var user = CreateValidUser();
        user.FullName = "";

        Func<Task> act = () => _userService.UpdateUserAsync(user);

        await act.Should().ThrowAsync<ServiceValidationException>()
            .WithMessage("User data is invalid");
        await _userRepository.DidNotReceive().UpdateAsync(
            Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserExists_DeletesUser()
    {
        var user = CreateValidUser();
        _userRepository.GetByIdAsync(user.Id).Returns(user);

        await _userService.DeleteUserAsync(user.Id);

        await _userRepository.Received(1).DeleteAsync(user.Id);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId).Returns((User?)null);

        Func<Task> act = () => _userService.DeleteUserAsync(userId);

        await act.Should().ThrowAsync<ServiceScaffoldException>()
            .WithMessage($"User {userId} not found");
        await _userRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>());
    }

    private static User CreateValidUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "user@example.com",
        FullName = "Test User",
        PasswordHash = "stored-password-hash",
        CreatedAt = DateTime.UtcNow.AddDays(-1),
        UpdatedAt = DateTime.UtcNow.AddMinutes(-1)
    };
}
