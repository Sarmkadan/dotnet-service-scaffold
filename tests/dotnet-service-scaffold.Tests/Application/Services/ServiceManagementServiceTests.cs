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
/// Contains unit tests for <see cref="ServiceManagementService"/> functionality.
/// Tests service registration, retrieval, activation state changes, and success rate calculations.
/// </summary>
public class ServiceManagementServiceTests : IServiceManagementServiceTests
{
	private readonly IServiceRepository _serviceRepository;
	private readonly IUserRepository _userRepository;
	private readonly IAuditService _auditService;
	private readonly ILogger<ServiceManagementService> _logger;
	private readonly ServiceManagementService _serviceManagementService;

	/// <summary>
	/// Initializes a new instance of the <see cref="ServiceManagementServiceTests"/> class.
	/// Sets up mock dependencies for testing <see cref="ServiceManagementService"/> functionality.
	/// </summary>
	public ServiceManagementServiceTests()
	{
		_serviceRepository = Substitute.For<IServiceRepository>();
		_userRepository = Substitute.For<IUserRepository>();
		_auditService = Substitute.For<IAuditService>();
		_logger = Substitute.For<ILogger<ServiceManagementService>>();
		_serviceManagementService = new ServiceManagementService(
			_serviceRepository,
			_userRepository,
			_auditService,
			_logger);
	}

	[Fact]
	public async Task RegisterServiceAsync_ShouldRegisterService_WhenInputsAreValid()
	{
		// Arrange
		var serviceName = "NewService";
		var endpoint = "http://newservice.com";
		var healthCheckUrl = "http://newservice.com/health";
		var ownerId = Guid.NewGuid();
		var owner = new User { Id = ownerId, Username = ServiceManagementServiceTestsConstants.TestUserName };

		_serviceRepository.GetByNameAsync(serviceName).Returns((ServiceRegistration)null);
		_serviceRepository.AddAsync(Arg.Any<ServiceRegistration>()).Returns(ci => ci.Arg<ServiceRegistration>());

		// Act
		var result = await _serviceManagementService.RegisterServiceAsync(serviceName, endpoint, healthCheckUrl, ownerId);

		// Assert
		result.Should().NotBeNull();
		result.ServiceName.Should().Be(serviceName);
		result.Endpoint.Should().Be(endpoint);
		result.HealthCheckUrl.Should().Be(healthCheckUrl);
		result.OwnerId.Should().Be(ownerId);
		await _serviceRepository.Received(1).AddAsync(Arg.Any<ServiceRegistration>());
		await _auditService.Received(1).LogActionAsync(ownerId, "Create", "ServiceRegistration", result.Id, string.Format(ServiceManagementServiceTestsConstants.RegisteredServiceLogFormat, serviceName));
	}

	[Theory]
	[InlineData("", "http://e.com", "http://h.com", "Service name is required")]
	[InlineData("N", "", "http://h.com", "Service endpoint is required")]
	[InlineData("N", "http://e.com", "", "Health check URL is required")]
	[InlineData("N", "invalid-url", "http://h.com", "Invalid service endpoint URL")]
	[InlineData("N", "http://e.com", "invalid-url", "Invalid health check URL")]
	public async Task RegisterServiceAsync_ShouldThrowValidationException_WhenInputsAreInvalid(
		string serviceName, string endpoint, string healthCheckUrl, string expectedError)
	{
		// Arrange
		var ownerId = Guid.NewGuid();
		var owner = new User { Id = ownerId, Username = ServiceManagementServiceTestsConstants.TestUserName };
		_userRepository.GetByIdAsync(ownerId).Returns(owner);

		// Act
		Func<Task> act = async () => await _serviceManagementService.RegisterServiceAsync(serviceName, endpoint, healthCheckUrl, ownerId);

		// Assert
		await act.Should().ThrowAsync<ServiceValidationException>()
			.WithMessage(expectedError);
	}

	[Fact]
	public async Task RegisterServiceAsync_ShouldThrowException_WhenOwnerNotFound()
	{
		// Arrange
		var serviceName = "NewService";
		var endpoint = "http://newservice.com";
		var healthCheckUrl = "http://newservice.com/health";
		var ownerId = Guid.NewGuid();

		_userRepository.GetByIdAsync(ownerId).Returns((User)null);

		// Act
		Func<Task> act = async () => await _serviceManagementService.RegisterServiceAsync(serviceName, endpoint, healthCheckUrl, ownerId);

		// Assert
		await act.Should().ThrowAsync<ServiceScaffoldException>()
			.WithMessage("Service owner not found")
			.And.ErrorCode.Should().Be("OWNER_NOT_FOUND");
	}

	[Fact]
	public async Task RegisterServiceAsync_ShouldThrowValidationException_WhenServiceNameAlreadyExists()
	{
		// Arrange
		var serviceName = "ExistingService";
		var endpoint = "http://existing.com";
		var healthCheckUrl = "http://existing.com/health";
		var ownerId = Guid.NewGuid();
		var owner = new User { Id = ownerId, Username = ServiceManagementServiceTestsConstants.TestUserName };
		var existingService = new ServiceRegistration { ServiceName = serviceName };

		_userRepository.GetByIdAsync(ownerId).Returns(owner);
		_serviceRepository.GetByNameAsync(serviceName).Returns(existingService);

		// Act
		Func<Task> act = async () => await _serviceManagementService.RegisterServiceAsync(serviceName, endpoint, healthCheckUrl, ownerId);

		// Assert
		await act.Should().ThrowAsync<ServiceValidationException>()
			.WithMessage("Service name already registered");
	}

	[Fact]
	public async Task GetServiceAsync_ShouldReturnService_WhenFound()
	{
		// Arrange
		var serviceId = Guid.NewGuid();
		var service = new ServiceRegistration { Id = serviceId, ServiceName = "TestService" };
		_serviceRepository.GetByIdAsync(serviceId).Returns(service);

		// Act
		var result = await _serviceManagementService.GetServiceAsync(serviceId);

		// Assert
		result.Should().Be(service);
	}

	[Fact]
	public async Task UnregisterServiceAsync_ShouldDeleteService()
	{
		// Arrange
		var serviceId = Guid.NewGuid();
		var service = new ServiceRegistration { Id = serviceId, ServiceName = "ServiceToDelete", OwnerId = Guid.NewGuid() };
		_serviceRepository.GetByIdAsync(serviceId).Returns(service);
		_serviceRepository.DeleteAsync(serviceId).Returns(Task.CompletedTask);

		// Act
		await _serviceManagementService.UnregisterServiceAsync(serviceId);

		// Assert
		await _serviceRepository.Received(1).DeleteAsync(serviceId);
		await _auditService.Received(1).LogActionAsync(service.OwnerId, "Delete", "ServiceRegistration", serviceId, string.Format(ServiceManagementServiceTestsConstants.UnregisteredServiceLogFormat, service.ServiceName));
	}

	[Fact]
	public async Task UnregisterServiceAsync_ShouldThrowNotFoundException_WhenServiceNotFound()
	{
		// Arrange
		var serviceId = Guid.NewGuid();
		_serviceRepository.GetByIdAsync(serviceId).Returns((ServiceRegistration)null);

		// Act
		Func<Task> act = async () => await _serviceManagementService.UnregisterServiceAsync(serviceId);

		// Assert
		await act.Should().ThrowAsync<ServiceNotFoundException>();
	}

	[Fact]
	public async Task DisableServiceAsync_ShouldDisableService()
	{
		// Arrange
		var serviceId = Guid.NewGuid();
		var service = new ServiceRegistration { Id = serviceId, ServiceName = "ServiceToDisable", IsActive = true };
		_serviceRepository.GetByIdAsync(serviceId).Returns(service);
		_serviceRepository.UpdateAsync(Arg.Any<ServiceRegistration>()).Returns(ci => ci.Arg<ServiceRegistration>());
		var reason = "Maintenance";

		// Act
		var result = await _serviceManagementService.DisableServiceAsync(serviceId, reason);

		// Assert
		result.IsActive.Should().BeFalse();
		result.DeactivationReason.Should().Be(reason);
		await _serviceRepository.Received(1).UpdateAsync(Arg.Is<ServiceRegistration>(s => s.Id == serviceId && !s.IsActive));
		await _auditService.Received(1).LogActionAsync(null, "Update", "ServiceRegistration", serviceId, string.Format(ServiceManagementServiceTestsConstants.DisabledServiceLogFormat, reason));
	}

	[Fact]
	public async Task EnableServiceAsync_ShouldEnableService()
	{
		// Arrange
		var serviceId = Guid.NewGuid();
		var service = new ServiceRegistration { Id = serviceId, ServiceName = "ServiceToEnable", IsActive = false, DeactivationReason = "Old reason" };
		_serviceRepository.GetByIdAsync(serviceId).Returns(service);
		_serviceRepository.UpdateAsync(Arg.Any<ServiceRegistration>()).Returns(ci => ci.Arg<ServiceRegistration>());

		// Act
		var result = await _serviceManagementService.EnableServiceAsync(serviceId);

		// Assert
		result.IsActive.Should().BeTrue();
		result.DeactivationReason.Should().BeNull();
		await _serviceRepository.Received(1).UpdateAsync(Arg.Is<ServiceRegistration>(s => s.Id == serviceId && s.IsActive));
		await _auditService.Received(1).LogActionAsync(null, "Update", "ServiceRegistration", serviceId, ServiceManagementServiceTestsConstants.ReenabledServiceLogMessage);
	}

	[Fact]
	public async Task GetServiceSuccessRateAsync_ShouldReturnSuccessRate()
	{
		// Arrange
		var serviceId = Guid.NewGuid();
		var service = new ServiceRegistration { Id = serviceId, ServiceName = "ServiceWithMetrics", TotalRequests = ServiceManagementServiceTestsConstants.SuccessRateTest_TotalRequests, SuccessfulRequests = ServiceManagementServiceTestsConstants.SuccessRateTest_SuccessfulRequests };
		_serviceRepository.GetByIdAsync(serviceId).Returns(service);

		// Act
		var rate = await _serviceManagementService.GetServiceSuccessRateAsync(serviceId);

		// Assert
		rate.Should().Be(90m);
	}

	[Fact]
	public async Task GetServiceSuccessRateAsync_ShouldReturn100_WhenNoRequests()
	{
		// Arrange
		var serviceId = Guid.NewGuid();
		var service = new ServiceRegistration { Id = serviceId, ServiceName = "ServiceWithNoMetrics", TotalRequests = ServiceManagementServiceTestsConstants.NoMetricsTest_TotalRequests, SuccessfulRequests = ServiceManagementServiceTestsConstants.NoMetricsTest_SuccessfulRequests };
		_serviceRepository.GetByIdAsync(serviceId).Returns(service);

		// Act
		var rate = await _serviceManagementService.GetServiceSuccessRateAsync(serviceId);

		// Assert
		rate.Should().Be(100m);
	}
}