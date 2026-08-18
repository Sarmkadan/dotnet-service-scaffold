using DotnetServiceScaffold.Domain.Models;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class UserTests
{
    private User CreateValidUser() => new User
    {
        Email = "test@example.com",
        FullName = "Test User",
        PasswordHash = "hashed_password"
    };

    [Fact]
    public void IsValid_ReturnsTrue_ForValidUser()
    {
        var user = CreateValidUser();
        Assert.True(user.IsValid());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void IsValid_ReturnsFalse_ForInvalidEmail(string? email)
    {
        var user = CreateValidUser();
        user.Email = email!;
        Assert.False(user.IsValid());
    }

    [Fact]
    public void IsAccountLocked_ReturnsFalse_WhenNotLocked()
    {
        var user = CreateValidUser();
        user.IsLocked = false;
        Assert.False(user.IsAccountLocked());
    }

    [Fact]
    public void IsAccountLocked_ReturnsTrue_WhenLockedAndNotExpired()
    {
        var user = CreateValidUser();
        user.IsLocked = true;
        user.LockedUntil = DateTime.UtcNow.AddMinutes(10);
        Assert.True(user.IsAccountLocked());
    }

    [Fact]
    public void IsAccountLocked_ReturnsFalse_WhenLockedButExpired()
    {
        var user = CreateValidUser();
        user.IsLocked = true;
        user.LockedUntil = DateTime.UtcNow.AddMinutes(-10);
        Assert.False(user.IsAccountLocked());
    }

    [Fact]
    public void RecordSuccessfulLogin_ResetsAttemptsAndUnlock()
    {
        var user = CreateValidUser();
        user.LoginAttempts = 3;
        user.IsLocked = true;

        user.RecordSuccessfulLogin();

        Assert.Equal(0, user.LoginAttempts);
        Assert.False(user.IsLocked);
        Assert.Null(user.LockedUntil);
        Assert.NotNull(user.LastLoginAt);
    }

    [Fact]
    public void RecordFailedLoginAttempt_IncrementsAttemptsAndLocksAccount()
    {
        var user = CreateValidUser();
        user.LoginAttempts = 4;

        user.RecordFailedLoginAttempt(lockThreshold: 5);

        Assert.Equal(5, user.LoginAttempts);
        Assert.True(user.IsLocked);
        Assert.NotNull(user.LockedUntil);
    }

    [Fact]
    public void UpdateLastActivity_UpdatesTimestamp()
    {
        var user = CreateValidUser();
        var oldUpdatedAt = user.UpdatedAt;
        
        // Ensure some time passes for update check
        Thread.Sleep(10);

        user.UpdateLastActivity();

        Assert.True(user.UpdatedAt > oldUpdatedAt);
    }
}
