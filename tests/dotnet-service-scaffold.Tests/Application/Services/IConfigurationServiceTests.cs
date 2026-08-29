#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Tests.Application.Services;

/// <summary>
/// Interface for ConfigurationServiceTests.
/// </summary>
public interface IConfigurationServiceTests
{
    Task GetConfigurationByIdAsync_ShouldReturnConfiguration_WhenConfigurationExists();
    Task GetConfigurationByIdAsync_ShouldReturnNull_WhenConfigurationDoesNotExist();
    Task GetConfigurationByKeyAsync_ShouldReturnConfiguration_WhenConfigurationExists();
    Task GetConfigurationByKeyAsync_ShouldReturnNull_WhenConfigurationDoesNotExist();
    Task CreateConfigurationAsync_ShouldReturnConfiguration_WhenCreatedSuccessfully();
    Task CreateConfigurationAsync_ShouldThrowException_WhenKeyAlreadyExists();
    Task UpdateConfigurationAsync_ShouldUpdateConfiguration_WhenConfigurationExists();
    Task UpdateConfigurationAsync_ShouldThrowException_WhenConfigurationDoesNotExist();
}