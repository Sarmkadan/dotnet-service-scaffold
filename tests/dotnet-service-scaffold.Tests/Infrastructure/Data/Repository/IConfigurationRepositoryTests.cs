#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Threading.Tasks;

namespace DotnetServiceScaffold.Tests.IntegrationTests;

/// <summary>
/// Contract for tests of the ConfigurationRepository class.
/// </summary>
public interface IConfigurationRepositoryTests
{
    Task AddConfigurationAsync_ShouldAddConfigurationToDatabase();

    Task GetConfigurationByIdAsync_ShouldReturnConfiguration_WhenConfigurationExists();

    Task GetConfigurationByIdAsync_ShouldReturnNull_WhenConfigurationDoesNotExist();

    Task GetConfigurationByKeyAsync_ShouldReturnConfiguration_WhenConfigurationExists();

    Task GetConfigurationByKeyAsync_ShouldReturnNull_WhenConfigurationDoesNotExist();

    Task UpdateConfigurationAsync_ShouldUpdateConfigurationInDatabase();

    Task DeleteConfigurationAsync_ShouldRemoveConfigurationFromDatabase();
}