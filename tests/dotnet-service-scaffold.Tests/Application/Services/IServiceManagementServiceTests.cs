#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using NSubstitute;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Domain.Exceptions;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;

namespace DotnetServiceScaffold.Tests.Application.Services;

/// <summary>
/// Interface for ServiceManagementServiceTests to enable test extraction and mocking.
/// </summary>
public interface IServiceManagementServiceTests
{
    Task RegisterServiceAsync_ShouldRegisterService_WhenInputsAreValid();
    Task RegisterServiceAsync_ShouldThrowValidationException_WhenInputsAreInvalid(string serviceName, string endpoint, string healthCheckUrl, string expectedError);
    Task RegisterServiceAsync_ShouldThrowException_WhenOwnerNotFound();
    Task RegisterServiceAsync_ShouldThrowValidationException_WhenServiceNameAlreadyExists();
    Task GetServiceAsync_ShouldReturnService_WhenFound();
    Task UnregisterServiceAsync_ShouldDeleteService();
    Task UnregisterServiceAsync_ShouldThrowNotFoundException_WhenServiceNotFound();
    Task DisableServiceAsync_ShouldDisableService();
    Task EnableServiceAsync_ShouldEnableService();
    Task GetServiceSuccessRateAsync_ShouldReturnSuccessRate();
    Task GetServiceSuccessRateAsync_ShouldReturn100_WhenNoRequests();
}