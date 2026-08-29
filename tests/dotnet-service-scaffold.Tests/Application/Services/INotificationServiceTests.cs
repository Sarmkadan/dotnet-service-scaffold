#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetServiceScaffold.Tests.Application.Services;

/// <summary>
/// Interface for the <see cref="NotificationServiceTests"/> class.
/// </summary>
public interface INotificationServiceTests
{
    Task SendNotificationAsync_ShouldReturnTrue_OnSuccess();
    Task SendNotificationAsync_ShouldReturnTrue_OnSuccessWithDifferentType;
    Task SendEmailAsync_ShouldReturnTrue_OnSuccess();
    Task SendBulkNotificationAsync_ShouldReturnCorrectCount();
    Task SendBulkNotificationAsync_ShouldHandleEmptyUserList();
    Task SendAlertAsync_ShouldReturnTrue_OnSuccess();
    Task SendAlertAsync_ShouldReturnTrue_WithoutDetails();
}