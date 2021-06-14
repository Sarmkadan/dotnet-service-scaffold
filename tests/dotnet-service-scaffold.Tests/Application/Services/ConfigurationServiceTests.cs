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

public class ConfigurationServiceTests
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly ConfigurationService _configurationService;

    public ConfigurationServiceTests()
    {
        _configurationRepository = Substitute.For<IConfigurationRepository>();
        _configurationService = new ConfigurationService(_configurationRepository);
    }

    [Fact]
    public async Task GetConfigurationByIdAsync_ShouldReturnConfiguration_WhenConfigurationExists()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var expectedConfig = new ServiceConfiguration { Id = configId, Key = "TestConfig", Value = "TestValue" };
        _configurationRepository.GetConfigurationByIdAsync(configId).Returns(expectedConfig);

        // Act
        var result = await _configurationService.GetConfigurationByIdAsync(configId);

        // Assert
        result.Should().Be(expectedConfig);
        await _configurationRepository.Received(1).GetConfigurationByIdAsync(configId);
    }

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

    [Fact]
    public async Task GetConfigurationByKeyAsync_ShouldReturnConfiguration_WhenConfigurationExists()
    {
        // Arrange
        var configKey = "ExistingKey";
        var expectedConfig = new ServiceConfiguration { Id = Guid.NewGuid(), Key = configKey, Value = "ExistingValue" };
        _configurationRepository.GetConfigurationByKeyAsync(configKey).Returns(expectedConfig);

        // Act
        var result = await _configurationService.GetConfigurationByKeyAsync(configKey);

        // Assert
        result.Should().Be(expectedConfig);
        await _configurationRepository.Received(1).GetConfigurationByKeyAsync(configKey);
    }

    [Fact]
    public async Task GetConfigurationByKeyAsync_ShouldReturnNull_WhenConfigurationDoesNotExist()
    {
        // Arrange
        var configKey = "NonExistentKey";
        _configurationRepository.GetConfigurationByKeyAsync(configKey).Returns((ServiceConfiguration)null);

        // Act
        var result = await _configurationService.GetConfigurationByKeyAsync(configKey);

        // Assert
        result.Should().BeNull();
        await _configurationRepository.Received(1).GetConfigurationByKeyAsync(configKey);
    }

    [Fact]
    public async Task CreateConfigurationAsync_ShouldReturnConfiguration_WhenCreatedSuccessfully()
    {
        // Arrange
        var newConfig = new ServiceConfiguration { Key = "NewConfig", Value = "NewValue" };
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

    [Fact]
    public async Task CreateConfigurationAsync_ShouldThrowException_WhenKeyAlreadyExists()
    {
        // Arrange
        var existingConfig = new ServiceConfiguration { Key = "ExistingKey", Value = "ExistingValue" };
        _configurationRepository.GetConfigurationByKeyAsync(existingConfig.Key).Returns(existingConfig);

        // Act
        Func<Task> action = async () => await _configurationService.CreateConfigurationAsync(existingConfig);

        // Assert
        await action.Should().ThrowAsync<ServiceScaffoldException>()
                    .WithMessage($"Configuration with key '{existingConfig.Key}' already exists.");
        await _configurationRepository.DidNotReceive().AddConfigurationAsync(Arg.Any<ServiceConfiguration>());
    }

    [Fact]
    public async Task UpdateConfigurationAsync_ShouldUpdateConfiguration_WhenConfigurationExists()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var existingConfig = new ServiceConfiguration { Id = configId, Key = "OldKey", Value = "OldValue" };
        var updatedConfig = new ServiceConfiguration { Id = configId, Key = "UpdatedKey", Value = "UpdatedValue" };

        _configurationRepository.GetConfigurationByIdAsync(configId).Returns(existingConfig);
        _configurationRepository.UpdateConfigurationAsync(Arg.Any<ServiceConfiguration>()).Returns(Task.CompletedTask);
        _configurationRepository.GetConfigurationByKeyAsync(updatedConfig.Key).Returns((ServiceConfiguration)null);

        // Act
        await _configurationService.UpdateConfigurationAsync(updatedConfig);

        // Assert
        await _configurationRepository.Received(1).GetConfigurationByIdAsync(configId);
        await _configurationRepository.Received(1).UpdateConfigurationAsync(Arg.Is<ServiceConfiguration>(c => c.Key == updatedConfig.Key));
    }

    [Fact]
    public async Task UpdateConfigurationAsync_ShouldThrowException_WhenConfigurationDoesNotExist()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var updatedConfig = new ServiceConfiguration { Id = configId, Key = "NonExistent", Value = "Value" };

        _configurationRepository.GetConfigurationByIdAsync(configId).Returns((ServiceConfiguration)null);

        // Act
        Func<Task> action = async () => await _configurationService.UpdateConfigurationAsync(updatedConfig);

        // Assert
        await action.Should().ThrowAsync<ServiceScaffoldException>()
                    .WithMessage($"Configuration with ID '{configId}' not found.");
        await _configurationRepository.DidNotReceive().UpdateConfigurationAsync(Arg.Any<ServiceConfiguration>());
    }
}
