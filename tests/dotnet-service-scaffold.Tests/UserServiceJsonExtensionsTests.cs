using System.Text.Json;
using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class UserServiceJsonExtensionsTests
{
    private readonly UserService _userService;

    public UserServiceJsonExtensionsTests()
    {
        // Use NSubstitute to create null-like dependencies for the constructor, 
        // as JsonSerializer uses reflection and won't invoke the constructor.
        var userRepository = Substitute.For<IUserRepository>();
        var apiKeyRepository = Substitute.For<IApiKeyRepository>();
        var logger = Substitute.For<ILogger<UserService>>();
        
        _userService = new UserService(userRepository, apiKeyRepository, logger);
    }

    [Fact]
    public void ToJson_ReturnsEmptyObjectJson_ForServiceInstance()
    {
        // Act
        var json = _userService.ToJson();

        // Assert
        Assert.Equal("{}", json);
    }

    [Fact]
    public void ToJson_ThrowsArgumentNullException_WhenValueIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((UserService)null!).ToJson());
    }

    [Fact]
    public void FromJson_ReturnsNull_WhenJsonIsEmpty()
    {
        // Act
        var user = UserServiceJsonExtensions.FromJson(string.Empty);

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public void FromJson_ReturnsNull_WhenJsonIsWhitespace()
    {
        // Act
        var user = UserServiceJsonExtensions.FromJson("   ");

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public void FromJson_ThrowsInvalidOperationException_WhenJsonIsValid()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => UserServiceJsonExtensions.FromJson("{\"id\":\"00000000-0000-0000-0000-000000000000\"}"));
    }

    [Fact]
    public void TryFromJson_ReturnsFalse_WhenJsonIsInvalid()
    {
        // Act
        var result = UserServiceJsonExtensions.TryFromJson("invalid", out var user);

        // Assert
        Assert.False(result);
        Assert.Null(user);
    }

    [Fact]
    public void TryFromJson_ThrowsInvalidOperationException_WhenJsonIsValidObject()
    {
        // Act & Assert
        UserService? user;
        Assert.Throws<InvalidOperationException>(() => UserServiceJsonExtensions.TryFromJson("{}", out user));
    }
}
