#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace DotnetServiceScaffold.Tests.Application.Services;

/// <summary>
/// Constants for ConfigurationServiceTests.
/// </summary>
internal static class ConfigurationServiceTestsConstants
{
    // Test data keys and values
    public const string TestConfigKey = "TestConfig";
    public const string TestConfigValue = "TestValue";
    public const string ExistingKey = "ExistingKey";
    public const string ExistingValue = "ExistingValue";
    public const string NonExistentKey = "NonExistentKey";
    public const string NewConfigKey = "NewConfig";
    public const string NewConfigValue = "NewValue";
    public const string OldKey = "OldKey";
    public const string OldValue = "OldValue";
    public const string UpdatedKey = "UpdatedKey";
    public const string UpdatedValue = "UpdatedValue";
    public const string NonExistent = "NonExistent";

    // Error message formats
    public const string ConfigurationKeyAlreadyExistsFormat = "Configuration with key '{0}' already exists.";
    public const string ConfigurationNotFoundByIdFormat = "Configuration with ID '{0}' not found.";
}