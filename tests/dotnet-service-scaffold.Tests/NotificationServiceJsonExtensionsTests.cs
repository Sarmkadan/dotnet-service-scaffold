using System;
using System.Text.Json;
using DotnetServiceScaffold.Application.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class NotificationServiceJsonExtensionsTests
{
    private readonly NotificationService _notificationService;

    public NotificationServiceJsonExtensionsTests()
    {
        var logger = Substitute.For<ILogger<NotificationService>>();
        _notificationService = new NotificationService(logger);
    }

    [Fact]
    public void ToJson_ReturnsValidJson_WhenServiceIsValid()
    {
        // Act
        var json = _notificationService.ToJson();

        // Assert
        Assert.Equal("{}", json);
    }

    [Fact]
    public void ToJson_ThrowsArgumentNullException_WhenValueIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((NotificationService)null!).ToJson());
    }

    [Fact]
    public void FromJson_ThrowsInvalidOperationException_WhenJsonIsValid()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => NotificationServiceJsonExtensions.FromJson("{}"));
    }

    [Fact]
    public void FromJson_ThrowsArgumentNullException_WhenJsonIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => NotificationServiceJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void TryFromJson_ThrowsInvalidOperationException_WhenJsonIsInvalid()
    {
        // Act & Assert
        NotificationService? value;
        Assert.Throws<InvalidOperationException>(() => NotificationServiceJsonExtensions.TryFromJson("{invalid}", out value));
    }

    [Fact]
    public void TryFromJson_ThrowsInvalidOperationException_WhenJsonIsValid()
    {
        // Act & Assert
        NotificationService? value;
        Assert.Throws<InvalidOperationException>(() => NotificationServiceJsonExtensions.TryFromJson("{}", out value));
    }
}
