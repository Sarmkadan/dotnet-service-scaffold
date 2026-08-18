using DotnetServiceScaffold.Domain.Models;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class ApiKeyTests
{
    private ApiKey CreateValidApiKey() => new ApiKey
    {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        Name = "Test Key",
        KeyHash = "hash123",
        KeyPrefix = "pref123"
    };

    [Fact]
    public void IsValid_ReturnsTrue_ForValidKey()
    {
        var key = CreateValidApiKey();
        Assert.True(key.IsValid());
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenInactive()
    {
        var key = CreateValidApiKey();
        key.IsActive = false;
        Assert.False(key.IsValid());
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenExpired()
    {
        var key = CreateValidApiKey();
        key.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        Assert.False(key.IsValid());
    }

    [Fact]
    public void GetDaysUntilExpiration_ReturnsCorrectValue()
    {
        var key = CreateValidApiKey();
        // Set expiration 5 days and 1 hour from now to avoid edge cases where it's slightly less than 5 days
        var expiresAt = DateTime.UtcNow.AddDays(5).AddHours(1);
        key.ExpiresAt = expiresAt;

        var days = key.GetDaysUntilExpiration();

        Assert.Equal(5, days);
    }

    [Fact]
    public void IsIpAllowed_ReturnsTrue_WhenNoIpsConfigured()
    {
        var key = CreateValidApiKey();
        Assert.True(key.IsIpAllowed("192.168.1.1"));
    }

    [Fact]
    public void IsIpAllowed_ReturnsTrue_WhenIpIsAllowed()
    {
        var key = CreateValidApiKey();
        key.AllowedIps = "192.168.1.1,10.0.0.1";
        Assert.True(key.IsIpAllowed("10.0.0.1"));
    }

    [Fact]
    public void HasScope_ReturnsTrue_WhenWildcardPresent()
    {
        var key = CreateValidApiKey();
        key.AllowedScopes = "read,*";
        Assert.True(key.HasScope("write"));
    }

    [Fact]
    public void RecordUsage_UpdatesLastUsedAndCount()
    {
        var key = CreateValidApiKey();
        var initialCount = key.ApiCallsCount;

        key.RecordUsage();

        Assert.NotNull(key.LastUsedAt);
        Assert.Equal(initialCount + 1, key.ApiCallsCount);
    }

    [Fact]
    public void Revoke_SetsIsActiveToFalse()
    {
        var key = CreateValidApiKey();
        key.Revoke();
        Assert.False(key.IsActive);
    }
}
