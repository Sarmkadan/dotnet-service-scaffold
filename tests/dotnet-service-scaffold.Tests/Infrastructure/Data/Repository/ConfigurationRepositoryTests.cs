#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data;
using DotnetServiceScaffold.Infrastructure.Data.Repository;

namespace DotnetServiceScaffold.Tests.IntegrationTests;

/// <summary>
/// Tests for the ConfigurationRepository class.
/// </summary>
public class ConfigurationRepositoryTests : IntegrationTestBase
{
    private readonly ConfigurationRepository _configurationRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationRepositoryTests"/> class.
    /// </summary>
    public ConfigurationRepositoryTests()
    {
        _configurationRepository = new ConfigurationRepository(DbContext);
    }

    /// <summary>
    /// Tests that adding a configuration to the database works correctly.
    /// </summary>
    [Fact]
    public async Task AddConfigurationAsync_ShouldAddConfigurationToDatabase()
    {
        // Arrange
        var config = new ServiceConfiguration { Id = Guid.NewGuid(), Key = "TestConfig", Value = "TestValue" };

        // Act
        await _configurationRepository.AddAsync(config);

        // Assert
        var addedConfig = await DbContext.ServiceConfigurations.FirstOrDefaultAsync(c => c.Id == config.Id);
        addedConfig.Should().NotBeNull();
        addedConfig?.Key.Should().Be("TestConfig");
    }

    /// <summary>
    /// Tests that getting a configuration by ID works correctly when the configuration exists.
    /// </summary>
    [Fact]
    public async Task GetConfigurationByIdAsync_ShouldReturnConfiguration_WhenConfigurationExists()
    {
        // Arrange
        var config = new ServiceConfiguration { Id = Guid.NewGuid(), Key = "TestConfig1", Value = "TestValue1" };
        await _configurationRepository.AddAsync(config);

        // Act
        var result = await _configurationRepository.GetByIdAsync(config.Id);

        // Assert
        result.Should().Be(config);
    }

    /// <summary>
    /// Tests that getting a configuration by ID returns null when the configuration does not exist.
    /// </summary>
    [Fact]
    public async Task GetConfigurationByIdAsync_ShouldReturnNull_WhenConfigurationDoesNotExist()
    {
        // Arrange
        // No arrangement needed as we are testing for a non-existent ID.

        // Act
        var result = await _configurationRepository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that getting a configuration by key works correctly when the configuration exists.
    /// </summary>
    [Fact]
    public async Task GetConfigurationByKeyAsync_ShouldReturnConfiguration_WhenConfigurationExists()
    {
        // Arrange
        var config = new ServiceConfiguration { Id = Guid.NewGuid(), Key = "TestKey2", Value = "TestValue2" };
        await _configurationRepository.AddAsync(config);

        // Act
        var result = await _configurationRepository.GetByKeyAsync("TestKey2");

        // Assert
        result.Should().Be(config);
    }

    /// <summary>
    /// Tests that getting a configuration by key returns null when the configuration does not exist.
    /// </summary>
    [Fact]
    public async Task GetConfigurationByKeyAsync_ShouldReturnNull_WhenConfigurationDoesNotExist()
    {
        // Arrange
        // No arrangement needed for non-existent key.

        // Act
        var result = await _configurationRepository.GetByKeyAsync("NonExistentKey");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that updating a configuration in the database works correctly.
    /// </summary>
    [Fact]
    public async Task UpdateConfigurationAsync_ShouldUpdateConfigurationInDatabase()
    {
        // Arrange
        var config = new ServiceConfiguration { Id = Guid.NewGuid(), Key = "OldKey", Value = "OldValue" };
        await _configurationRepository.AddAsync(config);
        
        // Ensure entity is detached to simulate real update scenario where entity might be fetched
        // and then updated, not directly tracked by current context
        DbContext.Entry(config).State = EntityState.Detached;

        config.Value = "NewValue";

        // Act
        await _configurationRepository.UpdateAsync(config);

        // Assert
        var updatedConfig = await DbContext.ServiceConfigurations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == config.Id);
        updatedConfig.Should().NotBeNull();
        updatedConfig?.Value.Should().Be("NewValue");
    }

    /// <summary>
    /// Tests that deleting a configuration from the database works correctly.
    /// </summary>
    [Fact]
    public async Task DeleteConfigurationAsync_ShouldRemoveConfigurationFromDatabase()
    {
        // Arrange
        var config = new ServiceConfiguration { Id = Guid.NewGuid(), Key = "ToDelete", Value = "Value" };
        await _configurationRepository.AddAsync(config);

        // Act
        await _configurationRepository.DeleteAsync(config.Id);

        // Assert
        var deletedConfig = await DbContext.ServiceConfigurations.FirstOrDefaultAsync(c => c.Id == config.Id);
        deletedConfig.Should().BeNull();
    }
}
