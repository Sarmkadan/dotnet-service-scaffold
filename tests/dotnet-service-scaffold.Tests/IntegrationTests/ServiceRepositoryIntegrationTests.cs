#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Integration tests for the ServiceRepository class.
/// </summary>
public class ServiceRepositoryIntegrationTests : IntegrationTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceRepositoryIntegrationTests"/> class.
    /// </summary>
    public ServiceRepositoryIntegrationTests()
    {
        _serviceRepository = new ServiceRepository(DbContext);
    }

    /// <summary>
    /// Tests that adding a service registration to the database is successful.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AddServiceRegistrationAsync_ShouldAddServiceToDatabase()
    {
        // Arrange
        var service = new ServiceRegistration
        {
            Name = "TestService",
            Description = "A service for testing",
            BaseUrl = "http://test.com",
            Status = ServiceStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _serviceRepository.AddServiceRegistrationAsync(service);

        // Assert
        var savedService = await DbContext.ServiceRegistrations.FirstOrDefaultAsync(s => s.Name == service.Name);
        savedService.Should().NotBeNull();
        savedService.Name.Should().Be(service.Name);
        savedService.Description.Should().Be(service.Description);
    }

    /// <summary>
    /// Tests that getting a service registration by ID returns the service when found.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetServiceRegistrationByIdAsync_ShouldReturnService_WhenFound()
    {
        // Arrange
        var service = new ServiceRegistration
        {
            Name = "AnotherService",
            Description = "Another service for testing",
            BaseUrl = "http://another.com",
            Status = ServiceStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        await DbContext.ServiceRegistrations.AddAsync(service);
        await DbContext.SaveChangesAsync();
        var serviceId = service.Id;

        // Act
        var foundService = await _serviceRepository.GetServiceRegistrationByIdAsync(serviceId);

        // Assert
        foundService.Should().NotBeNull();
        foundService.Id.Should().Be(serviceId);
    }

    /// <summary>
    /// Tests that getting a service registration by ID returns null when not found.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetServiceRegistrationByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        // Act
        var foundService = await _serviceRepository.GetServiceRegistrationByIdAsync(serviceId);

        // Assert
        foundService.Should().BeNull();
    }

    /// <summary>
    /// Tests that updating a service registration in the database is successful.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateServiceRegistrationAsync_ShouldUpdateServiceInDatabase()
    {
        // Arrange
        var service = new ServiceRegistration
        {
            Name = "ServiceToUpdate",
            Description = "Initial description",
            BaseUrl = "http://initial.com",
            Status = ServiceStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        await DbContext.ServiceRegistrations.AddAsync(service);
        await DbContext.SaveChangesAsync();

        service.Description = "Updated description";
        service.Status = ServiceStatus.Inactive;

        // Act
        await _serviceRepository.UpdateServiceRegistrationAsync(service);

        // Assert
        var updatedService = await DbContext.ServiceRegistrations.FindAsync(service.Id);
        updatedService.Should().NotBeNull();
        updatedService.Description.Should().Be("Updated description");
        updatedService.Status.Should().Be(ServiceStatus.Inactive);
    }

    /// <summary>
    /// Tests that deleting a service registration from the database is successful.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteServiceRegistrationAsync_ShouldRemoveServiceFromDatabase()
    {
        // Arrange
        var service = new ServiceRegistration
        {
            Name = "ServiceToDelete",
            Description = "Service to be deleted",
            BaseUrl = "http://delete.com",
            Status = ServiceStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        await DbContext.ServiceRegistrations.AddAsync(service);
        await DbContext.SaveChangesAsync();
        var serviceId = service.Id;

        // Act
        await _serviceRepository.DeleteServiceRegistrationAsync(serviceId);

        // Assert
        var deletedService = await DbContext.ServiceRegistrations.FindAsync(serviceId);
        deletedService.Should().BeNull();
    }

    /// <summary>
    /// Tests that getting all service registrations returns all services.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetAllServiceRegistrationsAsync_ShouldReturnAllServices()
    {
        // Arrange
        var service1 = new ServiceRegistration { Name = "S1", BaseUrl = "http://s1.com", Status = ServiceStatus.Active, CreatedAt = DateTime.UtcNow };
        var service2 = new ServiceRegistration { Name = "S2", BaseUrl = "http://s2.com", Status = ServiceStatus.Inactive, CreatedAt = DateTime.UtcNow };
        await DbContext.ServiceRegistrations.AddRangeAsync(service1, service2);
        await DbContext.SaveChangesAsync();

        // Act
        var allServices = await _serviceRepository.GetAllServiceRegistrationsAsync();

        // Assert
        allServices.Should().HaveCount(2);
        allServices.Should().ContainEquivalentOf(service1);
        allServices.Should().ContainEquivalentOf(service2);
    }

    /// <summary>
    /// Tests that getting all service registrations returns an empty collection when no services exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetAllServiceRegistrationsAsync_ShouldReturnEmpty_WhenNoServices()
    {
        // Act
        var allServices = await _serviceRepository.GetAllServiceRegistrationsAsync();

        // Assert
        allServices.Should().BeEmpty();
    }
}
