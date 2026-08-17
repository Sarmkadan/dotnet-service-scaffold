#nullable enable
using System;
using DotnetServiceScaffold.Domain.Models;
using Xunit;
using FluentAssertions;

namespace DotnetServiceScaffold.Tests;

public class ServiceConfigurationTests
{
    private ServiceConfiguration CreateValidConfiguration()
    {
        return new ServiceConfiguration
        {
            Id = Guid.NewGuid(),
            Key = "TestKey",
            Value = "TestValue",
            ConfigType = "string",
            ServiceId = Guid.NewGuid(),
            IsEncrypted = false,
            IsSystemConfig = false,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void Properties_AreInitializedCorrectly_WhenSetViaObjectInitializer()
    {
        var id = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var createdAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var config = new ServiceConfiguration
        {
            Id = id,
            Key = "TestKey",
            Value = "TestValue",
            ConfigType = "string",
            ServiceId = serviceId,
            IsEncrypted = true,
            IsSystemConfig = true,
            Description = "Test description",
            CreatedAt = createdAt
        };

        config.Id.Should().Be(id);
        config.Key.Should().Be("TestKey");
        config.Value.Should().Be("TestValue");
        config.ConfigType.Should().Be("string");
        config.ServiceId.Should().Be(serviceId);
        config.IsEncrypted.Should().BeTrue();
        config.IsSystemConfig.Should().BeTrue();
        config.Description.Should().Be("Test description");
        config.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void GetIntValue_ParsesValidInteger_AndReturnsDefaultForInvalid()
    {
        var config = CreateValidConfiguration();

        config.Value = "42";
        config.GetIntValue().Should().Be(42);

        config.Value = "not-a-number";
        config.GetIntValue(99).Should().Be(99);

        config.Value = string.Empty;
        config.GetIntValue(-1).Should().Be(-1);
    }

    [Fact]
    public void GetBoolValue_HandlesTrueFalseAndInvalidValues()
    {
        var config = CreateValidConfiguration();

        config.Value = "true";
        config.GetBoolValue().Should().BeTrue();
        config.Value = "1";
        config.GetBoolValue().Should().BeTrue();
        config.Value = "YES";
        config.GetBoolValue().Should().BeTrue();

        config.Value = "false";
        config.GetBoolValue().Should().BeFalse();
        config.Value = "0";
        config.GetBoolValue().Should().BeFalse();
        config.Value = "no";
        config.GetBoolValue().Should().BeFalse();

        config.Value = "not-boolean";
        config.GetBoolValue(true).Should().BeTrue();
    }

    [Fact]
    public void GetTimeSpanValue_ParsesValidValue_AndReturnsDefaultForInvalid()
    {
        var config = CreateValidConfiguration();

        config.Value = "00:30:00";
        config.GetTimeSpanValue().Should().Be(TimeSpan.FromMinutes(30));

        config.Value = "not-a-timespan";
        config.GetTimeSpanValue(TimeSpan.FromMinutes(5)).Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void ValidateValue_ReturnsTrueForValidValues_AndFalseForInvalid()
    {
        var config = CreateValidConfiguration();

        config.Value = "123";
        config.ConfigType = "integer";
        config.ValidateValue().Should().BeTrue();
        config.Value = "not-a-number";
        config.ValidateValue().Should().BeFalse();

        config.Value = "true";
        config.ConfigType = "boolean";
        config.ValidateValue().Should().BeTrue();
        config.Value = "not-boolean";
        config.ValidateValue().Should().BeFalse();

        config.Value = "01:30:00";
        config.ConfigType = "timespan";
        config.ValidateValue().Should().BeTrue();
        config.Value = "not-a-timespan";
        config.ValidateValue().Should().BeFalse();

        config.Value = "https://example.com";
        config.ConfigType = "url";
        config.ValidateValue().Should().BeTrue();
        config.Value = "not-a-url";
        config.ValidateValue().Should().BeFalse();

        config.Value = "any string";
        config.ConfigType = "string";
        config.ValidateValue().Should().BeTrue();
        config.Value = string.Empty;
        config.ValidateValue().Should().BeFalse();

        config.Value = "any value";
        config.ConfigType = null;
        config.ValidateValue().Should().BeTrue();
    }

    [Fact]
    public void GetMaskedValue_RedactsSensitiveKeys_AndReturnsValueForOthers()
    {
        var config = CreateValidConfiguration();
        config.Value = "actual-secret-value";

        config.Key = "Password";
        config.GetMaskedValue().Should().Be("***REDACTED***");
        config.Key = "API_KEY";
        config.GetMaskedValue().Should().Be("***REDACTED***");
        config.Key = "AuthToken";
        config.GetMaskedValue().Should().Be("***REDACTED***");
        config.Key = "SecretKey";
        config.GetMaskedValue().Should().Be("***REDACTED***");

        config.Key = "SomeOtherConfig";
        config.GetMaskedValue().Should().Be("actual-secret-value");
        config.Key = "ConnectionString";
        config.GetMaskedValue().Should().Be("actual-secret-value");
    }

    [Fact]
    public void UpdateValue_UpdatesValueTimestampAndUser_WhenCalled()
    {
        var config = CreateValidConfiguration();
        var originalUpdatedAt = config.UpdatedAt;
        var userId = Guid.NewGuid();

        config.UpdateValue("NewValue", userId);

        config.Value.Should().Be("NewValue");
        config.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        config.UpdatedByUserId.Should().Be(userId);
    }

    [Fact]
    public void UpdateValue_LeavesUpdatedByUserIdNull_WhenUserIdNotProvided()
    {
        var config = CreateValidConfiguration();
        var originalUpdatedAt = config.UpdatedAt;

        config.UpdateValue("NewValue");

        config.Value.Should().Be("NewValue");
        config.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        config.UpdatedByUserId.Should().BeNull();
    }
}