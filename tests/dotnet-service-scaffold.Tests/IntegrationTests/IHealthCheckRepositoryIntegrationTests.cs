#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;

namespace DotnetServiceScaffold.Tests.IntegrationTests
{
    /// <summary>
    /// Interface for integration tests of the <see cref="HealthCheckRepository"/>.
    /// </summary>
    public interface IHealthCheckRepositoryIntegrationTests
    {
        Task AddHealthCheckResultAsync_ShouldAddResultToDatabase();
        Task GetHealthCheckResultsForServiceAsync_ShouldReturnResultsForService();
        Task GetHealthCheckResultsForServiceAsync_ShouldReturnEmpty_WhenNoResults();
        Task GetLatestHealthCheckResultForServiceAsync_ShouldReturnLatestResult();
        Task GetLatestHealthCheckResultForServiceAsync_ShouldReturnNull_WhenNoResults();
        Task DeleteHealthCheckResultAsync_ShouldRemoveResultFromDatabase();
    }
}