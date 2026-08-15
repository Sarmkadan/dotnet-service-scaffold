using System;
using System.Collections.Generic;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class ConfigurationRepositoryValidationTests
{
    private static ServiceConfiguration CreateValidConfiguration()
    {
        return new ServiceConfiguration
        {
            Key = "ValidKey",
            Value = "ValidValue",
            ConfigType = "Type",
            Description = "A valid description.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ServiceId = Guid.NewGuid()
        };
    }

    [Fact]
    public void Validate_ReturnsEmpty_WhenConfigurationIsValid()
    {
        var config = CreateValidConfiguration();

        var result = config.Validate();

        Assert.Empty(result);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenConfigurationIsValid()
    {
        var config = CreateValidConfiguration();

        var result = config.IsValid();

        Assert.True(result);
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_WhenConfigurationIsValid()
    {
        var config = CreateValidConfiguration();

        var exception = Record.Exception(() => config.EnsureValid());

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenConfigurationIsNull()
    {
        ServiceConfiguration? config = null;

        Assert.Throws<ArgumentNullException>(() => config!.Validate());
        Assert.Throws<ArgumentNullException>(() => config!.IsValid());
        Assert.Throws<ArgumentNullException>(() => config!.EnsureValid());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ReturnsError_ForInvalidKey(string? key)
    {
        var config = CreateValidConfiguration();
        config.Key = key!;

        var errors = config.Validate();

        Assert.Contains("Configuration Key cannot be null or whitespace.", errors);
    }

    [Fact]
    public void Validate_ReturnsError_WhenKeyExceedsMaxLength()
    {
        var config = CreateValidConfiguration();
        config.Key = new string('a', 256); // 256 > 255

        var errors = config.Validate();

        Assert.Contains("Configuration Key cannot exceed 255 characters.", errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ReturnsError_ForInvalidValue(string? value)
    {
        var config = CreateValidConfiguration();
        config.Value = value!;

        var errors = config.Validate();

        Assert.Contains("Configuration Value cannot be null or whitespace.", errors);
    }

    [Fact]
    public void Validate_ReturnsError_WhenValueExceedsMaxLength()
    {
        var config = CreateValidConfiguration();
        config.Value = new string('b', 4001); // 4001 > 4000

        var errors = config.Validate();

        Assert.Contains("Configuration Value cannot exceed 4000 characters.", errors);
    }

    [Fact]
    public void Validate_ReturnsError_WhenConfigTypeExceedsMaxLength()
    {
        var config = CreateValidConfiguration();
        config.ConfigType = new string('c', 51); // 51 > 50

        var errors = config.Validate();

        Assert.Contains("Configuration Type cannot exceed 50 characters.", errors);
    }

    [Fact]
    public void Validate_ReturnsError_WhenDescriptionExceedsMaxLength()
    {
        var config = CreateValidConfiguration();
        config.Description = new string('d', 1001); // 1001 > 1000

        var errors = config.Validate();

        Assert.Contains("Configuration Description cannot exceed 1000 characters.", errors);
    }

    [Fact]
    public void Validate_ReturnsError_WhenCreatedAtIsDefault()
    {
        var config = CreateValidConfiguration();
        config.CreatedAt = default;

        var errors = config.Validate();

        Assert.Contains("Configuration CreatedAt must be set to a valid date.", errors);
    }

    [Fact]
    public void Validate_ReturnsError_WhenCreatedAtIsInFuture()
    {
        var config = CreateValidConfiguration();
        config.CreatedAt = DateTime.UtcNow.AddHours(1);

        var errors = config.Validate();

        Assert.Contains("Configuration CreatedAt cannot be in the future.", errors);
    }

    [Fact]
    public void Validate_ReturnsError_WhenUpdatedAtIsDefault()
    {
        var config = CreateValidConfiguration();
        config.UpdatedAt = default;

        var errors = config.Validate();

        Assert.Contains("Configuration UpdatedAt must be set to a valid date.", errors);
    }

    [Fact]
    public void Validate_ReturnsError_WhenUpdatedAtIsInFuture()
    {
        var config = CreateValidConfiguration();
        config.UpdatedAt = DateTime.UtcNow.AddHours(1);

        var errors = config.Validate();

        Assert.Contains("Configuration UpdatedAt cannot be in the future.", errors);
    }

    [Fact]
    public void Validate_ReturnsError_WhenServiceIdIsEmptyGuid()
    {
        var config = CreateValidConfiguration();
        config.ServiceId = Guid.Empty;

        var errors = config.Validate();

        Assert.Contains("Configuration ServiceId cannot be an empty GUID.", errors);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenConfigurationHasMultipleErrors()
    {
        var config = CreateValidConfiguration();
        config.Key = "";
        config.Value = "";
        config.CreatedAt = default;

        var result = config.IsValid();

        Assert.False(result);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_WithAllErrors()
    {
        var config = CreateValidConfiguration();
        config.Key = "";
        config.Value = "";
        config.CreatedAt = default;

        var ex = Assert.Throws<ArgumentException>(() => config.EnsureValid());

        Assert.Contains("Configuration Key cannot be null or whitespace.", ex.Message);
        Assert.Contains("Configuration Value cannot be null or whitespace.", ex.Message);
        Assert.Contains("Configuration CreatedAt must be set to a valid date.", ex.Message);
    }
}
