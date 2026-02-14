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

namespace DotnetServiceScaffold.Tests.IntegrationTests;

public class ServiceRepositoryIntegrationTests : IntegrationTestBase
{
    private readonly ServiceRepository _serviceRepository;

    public ServiceRepositoryIntegrationTests()
    {
        _serviceRepository = new ServiceRepository(DbContext);
    }

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

    [Fact]
    public async Task GetAllServiceRegistrationsAsync_ShouldReturnEmpty_WhenNoServices()
    {
        // Act
        var allServices = await _serviceRepository.GetAllServiceRegistrationsAsync();

        // Assert
        allServices.Should().BeEmpty();
    }
}
