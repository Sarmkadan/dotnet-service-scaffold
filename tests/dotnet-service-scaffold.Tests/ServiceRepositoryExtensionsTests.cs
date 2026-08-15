using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotnetServiceScaffold.Tests.Infrastructure.Data.Repository
{
    public class ServiceRepositoryExtensionsTests
    {
        private readonly ServiceScaffoldDbContext _context;
        private readonly ServiceRepository _repository;

        public ServiceRepositoryExtensionsTests()
        {
            var options = new DbContextOptionsBuilder<ServiceScaffoldDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var loggerMock = new Mock<ILogger<ServiceScaffoldDbContext>>();
            var repoLoggerMock = new Mock<ILogger<ServiceRepository>>();
            
            _context = new ServiceScaffoldDbContext(options, loggerMock.Object);
            _repository = new ServiceRepository(_context, repoLoggerMock.Object);
        }

        [Fact]
        public async Task GetByNameAsync_ReturnsService_WhenExists()
        {
            var service = new ServiceRegistration { ServiceName = "TestSvc", HealthCheckUrl = "http://test", Endpoint = "http://test", Version = "1.0" };
            _context.ServiceRegistrations.Add(service);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByNameAsync("TestSvc");

            Assert.NotNull(result);
            Assert.Equal("TestSvc", result.ServiceName);
        }

        [Fact]
        public async Task GetByStatusAsync_ReturnsCorrectServices()
        {
            _context.ServiceRegistrations.AddRange(
                new ServiceRegistration { ServiceName = "Svc1", Status = ServiceStatus.Healthy, HealthCheckUrl = "http://1", Endpoint = "http://1", Version = "1.0" },
                new ServiceRegistration { ServiceName = "Svc2", Status = ServiceStatus.Unhealthy, HealthCheckUrl = "http://2", Endpoint = "http://2", Version = "1.0" }
            );
            await _context.SaveChangesAsync();

            var results = await _repository.GetByStatusAsync(ServiceStatus.Healthy);

            Assert.Single(results);
            Assert.Equal("Svc1", results.First().ServiceName);
        }

        [Fact]
        public async Task GetUnhealthyServicesAsync_ReturnsUnhealthyAndDegraded()
        {
            _context.ServiceRegistrations.AddRange(
                new ServiceRegistration { ServiceName = "Svc1", Status = ServiceStatus.Unhealthy, HealthCheckUrl = "http://1", Endpoint = "http://1", Version = "1.0" },
                new ServiceRegistration { ServiceName = "Svc2", Status = ServiceStatus.Degraded, HealthCheckUrl = "http://2", Endpoint = "http://2", Version = "1.0" },
                new ServiceRegistration { ServiceName = "Svc3", Status = ServiceStatus.Healthy, HealthCheckUrl = "http://3", Endpoint = "http://3", Version = "1.0" }
            );
            await _context.SaveChangesAsync();

            var results = await _repository.GetUnhealthyServicesAsync();

            Assert.Equal(2, results.Count());
        }

        [Fact]
        public async Task GetServiceCountsByStatusAsync_ReturnsCorrectCounts()
        {
            _context.ServiceRegistrations.AddRange(
                new ServiceRegistration { ServiceName = "Svc1", Status = ServiceStatus.Healthy, HealthCheckUrl = "http://1", Endpoint = "http://1", Version = "1.0" },
                new ServiceRegistration { ServiceName = "Svc2", Status = ServiceStatus.Healthy, HealthCheckUrl = "http://2", Endpoint = "http://2", Version = "1.0" },
                new ServiceRegistration { ServiceName = "Svc3", Status = ServiceStatus.Unhealthy, HealthCheckUrl = "http://3", Endpoint = "http://3", Version = "1.0" }
            );
            await _context.SaveChangesAsync();

            var results = await _repository.GetServiceCountsByStatusAsync();

            Assert.Equal(2, results[ServiceStatus.Healthy]);
            Assert.Equal(1, results[ServiceStatus.Unhealthy]);
        }
    }
}
