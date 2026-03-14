// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using NSubstitute;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DotnetServiceScaffold.Application.Services;

namespace DotnetServiceScaffold.Tests.Application.Services;

public class NotificationServiceTests
{
    private readonly ILogger<NotificationService> _logger;
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        _logger = Substitute.For<ILogger<NotificationService>>();
        _notificationService = new NotificationService(_logger);
    }

    [Fact]
    public async Task SendNotificationAsync_ShouldReturnTrue_OnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subject = "Test Subject";
        var message = "Test Message";

        // Act
        var result = await _notificationService.SendNotificationAsync(userId, subject, message);

        // Assert
        result.Should().BeTrue();
        _logger.Received(1).LogInformation(
            "Sending {NotificationType} notification to user {UserId}: {Subject}",
            NotificationType.Email, userId, subject);
    }

    [Fact]
    public async Task SendNotificationAsync_ShouldReturnTrue_OnSuccessWithDifferentType()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subject = "Test Subject";
        var message = "Test Message";

        // Act
        var result = await _notificationService.SendNotificationAsync(userId, subject, message, NotificationType.Sms);

        // Assert
        result.Should().BeTrue();
        _logger.Received(1).LogInformation(
            "Sending {NotificationType} notification to user {UserId}: {Subject}",
            NotificationType.Sms, userId, subject);
    }

    [Fact]
    public async Task SendEmailAsync_ShouldReturnTrue_OnSuccess()
    {
        // Arrange
        var email = "test@example.com";
        var subject = "Email Subject";
        var body = "<html><body><h1>Hello</h1></body></html>";

        // Act
        var result = await _notificationService.SendEmailAsync(email, subject, body);

        // Assert
        result.Should().BeTrue();
        _logger.Received(1).LogInformation("Sending email to {EmailAddress}: {Subject}", email, subject);
    }

    [Fact]
    public async Task SendBulkNotificationAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var subject = "Bulk Subject";
        var message = "Bulk Message";

        // Act
        var result = await _notificationService.SendBulkNotificationAsync(userIds, subject, message);

        // Assert
        result.Should().Be(userIds.Count);
        _logger.Received(1).LogInformation(
            "Sending bulk {NotificationType} notification to {Count} users: {Subject}",
            NotificationType.Email, userIds.Count, subject);
        _logger.Received(1).LogInformation(
            "Bulk notification completed: {SuccessCount}/{TotalCount} successful",
            userIds.Count, userIds.Count);
    }

    [Fact]
    public async Task SendBulkNotificationAsync_ShouldHandleEmptyUserList()
    {
        // Arrange
        var userIds = new List<Guid>();
        var subject = "Empty List";
        var message = "No users";

        // Act
        var result = await _notificationService.SendBulkNotificationAsync(userIds, subject, message);

        // Assert
        result.Should().Be(0);
        _logger.Received(1).LogInformation(
            "Sending bulk {NotificationType} notification to {Count} users: {Subject}",
            NotificationType.Email, 0, subject);
        _logger.Received(1).LogInformation(
            "Bulk notification completed: {SuccessCount}/{TotalCount} successful",
            0, 0);
    }

    [Fact]
    public async Task SendAlertAsync_ShouldReturnTrue_OnSuccess()
    {
        // Arrange
        var alertType = "Critical Error";
        var description = "Database connection lost";
        var details = "Detailed stack trace here";

        // Act
        var result = await _notificationService.SendAlertAsync(alertType, description, details);

        // Assert
        result.Should().BeTrue();
        _logger.Received(1).LogWarning(
            "Alert: {AlertType} - {Description}. Details: {Details}",
            alertType, description, details);
    }

    [Fact]
    public async Task SendAlertAsync_ShouldReturnTrue_WithoutDetails()
    {
        // Arrange
        var alertType = "Warning";
        var description = "High CPU usage";

        // Act
        var result = await _notificationService.SendAlertAsync(alertType, description);

        // Assert
        result.Should().BeTrue();
        _logger.Received(1).LogWarning(
            "Alert: {AlertType} - {Description}. Details: {Details}",
            alertType, description, "N/A");
    }
}
