#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using NSubstitute;
using Xunit;
using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using DotnetServiceScaffold.Domain.Exceptions;

namespace DotnetServiceScaffold.Tests.Application.Services;

/// <summary>
/// Tests for the ConfigurationService class.
/// </summary>
public class ConfigurationServiceTests : IConfigurationServiceTests
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly ConfigurationService _configurationService;

    /// <summary>
    /// Initializes a new instance of the ConfigurationServiceTests class.
    /// </summary>
    public ConfigurationServiceTests()
    {
        _configurationRepository = Substitute.For<IConfigurationRepository>();
        _configurationService = new ConfigurationService(_configurationRepository);
    }

    /// <summary>
    /// Tests that GetConfigurationByIdAsync returns the configuration when it exists.
    /// </summary>
    /// <param name="configId">The ID of the configuration to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetConfigurationByIdAsync_ShouldReturnConfiguration_WhenConfigurationExists()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var expectedConfig = new ServiceConfiguration { Id = configId, Key = ConfigurationServiceTestsConstants.TestConfigKey, Value = ConfigurationServiceTestsConstants.TestConfigValue };
        _configurationRepository.GetConfigurationByIdAsync(configId).Returns(expectedConfig);

        // Act
        var result = await _configurationService.GetConfigurationByIdAsync(configId);

        // Assert
        result.Should().Be(expectedConfig);
        await _configurationRepository.Received(1).GetConfigurationByIdAsync(configId);
    }

    /// <summary>
    /// Tests that GetConfigurationByIdAsync returns null when the configuration does not exist.
    /// </summary>
    /// <param name="configId">The ID of the configuration to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetConfigurationByIdAsync_ShouldReturnNull_WhenConfigurationDoesNotExist()
    {
        // Arrange
        var configId = Guid.NewGuid();
        _configurationRepository.GetConfigurationByIdAsync(configId).Returns((ServiceConfiguration)null);

        // Act
        var result = await _configurationService.GetConfigurationByIdAsync(configId);

        // Assert
        result.Should().BeNull();
        await _configurationRepository.Received(1).GetConfigurationByIdAsync(configId);
    }

    /// <summary>
    /// Tests that GetConfigurationByKeyAsync returns the configuration when it exists.
    /// </summary>
    /// <param name="configKey">The key of the configuration to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetConfigurationByKeyAsync_ShouldReturnConfiguration_WhenConfigurationExists()
    {
        // Arrange
        var configKey = ConfigurationServiceTestsConstants.ExistingKey;
        var expectedConfig = new ServiceConfiguration { Id = Guid.NewGuid(), Key = configKey, Value = ConfigurationServiceTestsConstants.ExistingValue };
        _configurationRepository.GetConfigurationByKeyAsync(configKey).Returns(expectedConfig);

        // Act
        var result = await _configurationService.GetConfigurationByKeyAsync(configKey);

        // Assert
        result.Should().Be(expectedConfig);
        await _configurationRepository.Received(1).GetConfigurationByKeyAsync(configKey);
    }

    /// <summary>
    /// Tests that GetConfigurationByKeyAsync returns null when the configuration does not exist.
    /// </summary>
    /// <param name="configKey">The key of the configuration to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetConfigurationByKeyAsync_ShouldReturnNull_WhenConfigurationDoesNotExist()
    {
        // Arrange
        var configKey = ConfigurationServiceTestsConstants.NonExistentKey;
        _configurationRepository.GetConfigurationByKeyAsync(configKey).Returns((ServiceConfiguration)null);

        // Act
        var result = await _configurationService.GetConfigurationByKeyAsync(configKey);

        // Assert
        result.Should().BeNull();
        await _configurationRepository.Received(1).GetConfigurationByKeyAsync(configKey);
    }

    /// <summary>
    /// Tests that CreateConfigurationAsync returns the created configuration when successful.
    /// </summary>
    /// <param name="newConfig">The new configuration to create.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task CreateConfigurationAsync_ShouldReturnConfiguration_WhenCreatedSuccessfully()
    {
        // Arrange
        var newConfig = new ServiceConfiguration { Key = ConfigurationServiceTestsConstants.NewConfigKey, Value = ConfigurationServiceTestsConstants.NewConfigValue };
        _configurationRepository.AddConfigurationAsync(Arg.Any<ServiceConfiguration>()).Returns(Task.CompletedTask);
        _configurationRepository.GetConfigurationByKeyAsync(newConfig.Key).Returns((ServiceConfiguration)null);

        // Act
        var result = await _configurationService.CreateConfigurationAsync(newConfig);

        // Assert
        result.Should().NotBeNull();
        result.Key.Should().Be(newConfig.Key);
        result.Value.Should().Be(newConfig.Value);
        await _configurationRepository.Received(1).AddConfigurationAsync(Arg.Is<ServiceConfiguration>(c => c.Key == newConfig.Key));
    }

    /// <summary>
    /// Tests that CreateConfigurationAsync throws an exception when the key already exists.
    /// </summary>
    /// <param name="existingConfig">The existing configuration with the same key.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task CreateConfigurationAsync_ShouldThrowException_WhenKeyAlreadyExists()
    {
        // Arrange
        var existingConfig = new ServiceConfiguration { Key = ConfigurationServiceTestsConstants.ExistingKey, Value = ConfigurationServiceTestsConstants.ExistingValue };
        _configurationRepository.GetConfigurationByKeyAsync(existingConfig.Key).Returns(existingConfig);

        // Act
        Func<Task> action = async () => await _configurationService.CreateConfigurationAsync(existingConfig);

        // Assert
        await action.Should().ThrowAsync<ServiceScaffoldException>()
                    .WithMessage(string.Format(ConfigurationServiceTestsConstants.ConfigurationKeyAlreadyExistsFormat, existingConfig.Key));
        await _configurationRepository.DidNotReceive().AddConfigurationAsync(Arg.Any<ServiceConfiguration>());
    }

    /// <summary>
    /// Tests that UpdateConfigurationAsync updates the configuration when it exists.
    /// </summary>
    /// <param name="updatedConfig">The updated configuration to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateConfigurationAsync_ShouldUpdateConfiguration_WhenConfigurationExists()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var existingConfig = new ServiceConfiguration { Id = configId, Key = ConfigurationServiceTestsConstants.OldKey, Value = ConfigurationServiceTestsConstants.OldValue };
        var updatedConfig = new ServiceConfiguration { Id = configId, Key = ConfigurationServiceTestsConstants.UpdatedKey, Value = ConfigurationServiceTestsConstants.UpdatedValue };

        _configurationRepository.GetConfigurationByIdAsync(configId).Returns(existingConfig);
        _configurationRepository.UpdateConfigurationAsync(Arg.Any<ServiceConfiguration>()).Returns(Task.CompletedTask);
        _configurationRepository.GetConfigurationByKeyAsync(updatedConfig.Key).Returns((ServiceConfiguration)null);

        // Act
        await _configurationService.UpdateConfigurationAsync(updatedConfig);

        // Assert
        await _configurationRepository.Received(1).GetConfigurationByIdAsync(configId);
        await _configurationRepository.Received(1).UpdateConfigurationAsync(Arg.Is<ServiceConfiguration>(c => c.Key == updatedConfig.Key));
    }

    /// <summary>
    /// Tests that UpdateConfigurationAsync throws an exception when the configuration does not exist.
    /// </summary>
    /// <param name="updatedConfig">The updated configuration to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateConfigurationAsync_ShouldThrowException_WhenConfigurationDoesNotExist()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var updatedConfig = new ServiceConfiguration { Id = configId, Key = ConfigurationServiceTestsConstants.NonExistent, Value = "Value" };

        _configurationRepository.GetConfigurationByIdAsync(configId).Returns((ServiceConfiguration)null);

        // Act
        Func<Task> action = async () => await _configurationService.UpdateConfigurationAsync(updatedConfig);

        // Assert
        await action.Should().ThrowAsync<ServiceScaffoldException>()
                    .WithMessage(string.Format(ConfigurationServiceTestsConstants.ConfigurationNotFoundByIdFormat, configId));
        await _configurationRepository.DidNotReceive().UpdateConfigurationAsync(Arg.Any<ServiceConfiguration>());
    }
}
