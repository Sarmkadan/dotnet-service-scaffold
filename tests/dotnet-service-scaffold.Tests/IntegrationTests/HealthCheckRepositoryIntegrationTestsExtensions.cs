#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace DotnetServiceScaffold.Tests.IntegrationTests;

public static class HealthCheckRepositoryIntegrationTestsExtensions
{
    /// <summary>
    /// Creates a new health check result with the specified parameters and adds it to the database.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="serviceId">The service ID</param>
    /// <param name="status">The health status</param>
    /// <param name="responseTimeMs">Response time in milliseconds</param>
    /// <param name="details">Optional details</param>
    /// <returns>The created health check result</returns>
    public static async Task<HealthCheckResult> CreateAndAddHealthCheckResultAsync(
        this HealthCheckRepositoryIntegrationTests test,
        Guid serviceId,
        HealthStatus status = HealthStatus.Healthy,
        int responseTimeMs = 150,
        string? details = null)
    {
        var result = new HealthCheckResult
        {
            ServiceId = serviceId,
            Status = status,
            CheckedAt = DateTime.UtcNow,
            ResponseTimeMs = responseTimeMs,
            Details = details ?? "Test health check"
        };

        await test._healthCheckRepository.AddHealthCheckResultAsync(result);
        await test.DbContext.SaveChangesAsync();

        return result;
    }

    /// <summary>
    /// Creates multiple health check results for the same service and returns them.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="serviceId">The service ID</param>
    /// <param name="count">Number of results to create</param>
    /// <param name="statuses">Optional status sequence</param>
    /// <param name="responseTimes">Optional response times</param>
    /// <returns>List of created health check results</returns>
    public static async Task<List<HealthCheckResult>> CreateMultipleHealthCheckResultsAsync(
        this HealthCheckRepositoryIntegrationTests test,
        Guid serviceId,
        int count,
        HealthStatus[]? statuses = null,
        int[]? responseTimes = null)
    {
        var results = new List<HealthCheckResult>();
        var random = new Random();

        for (int i = 0; i < count; i++)
        {
            var result = new HealthCheckResult
            {
                ServiceId = serviceId,
                Status = statuses != null && i < statuses.Length ? statuses[i] : (HealthStatus)random.Next(0, 3),
                CheckedAt = DateTime.UtcNow.AddMinutes(-i),
                ResponseTimeMs = responseTimes != null && i < responseTimes.Length ? responseTimes[i] : random.Next(50, 500),
                Details = $"Health check #{i}"
            };
            results.Add(result);
        }

        await test.DbContext.HealthCheckResults.AddRangeAsync(results);
        await test.DbContext.SaveChangesAsync();

        return results;
    }

    /// <summary>
    /// Asserts that a health check result has the expected properties.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="result">The health check result</param>
    /// <param name="expectedServiceId">Expected service ID</param>
    /// <param name="expectedStatus">Expected status</param>
    /// <param name="expectedResponseTimeMs">Expected response time</param>
    /// <param name="expectedDetails">Expected details</param>
    public static void AssertHealthCheckResultMatches(
        this HealthCheckRepositoryIntegrationTests test,
        HealthCheckResult result,
        Guid expectedServiceId,
        HealthStatus expectedStatus = HealthStatus.Healthy,
        int expectedResponseTimeMs = 150,
        string? expectedDetails = null)
    {
        result.Should().NotBeNull();
        result.ServiceId.Should().Be(expectedServiceId);
        result.Status.Should().Be(expectedStatus);
        result.ResponseTimeMs.Should().Be(expectedResponseTimeMs);
        if (expectedDetails != null)
        {
            result.Details.Should().Be(expectedDetails);
        }
    }

    /// <summary>
    /// Gets all health check results from the database and returns them as a list.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <returns>All health check results in the database</returns>
    public static async Task<List<HealthCheckResult>> GetAllHealthCheckResultsAsync(this HealthCheckRepositoryIntegrationTests test)
    {
        return await test.DbContext.HealthCheckResults.ToListAsync();
    }

    /// <summary>
    /// Counts the number of health check results for a specific service.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="serviceId">The service ID to count results for</param>
    /// <returns>Number of health check results for the service</returns>
    public static async Task<int> CountHealthCheckResultsForServiceAsync(
        this HealthCheckRepositoryIntegrationTests test,
        Guid serviceId)
    {
        return await test.DbContext.HealthCheckResults
            .CountAsync(h => h.ServiceId == serviceId);
    }
}