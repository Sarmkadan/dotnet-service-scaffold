#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Represents a user in the system with authentication and profile information.
/// </summary>
public class User : IUser, IEquatable<User>
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [Required]
    [StringLength(255)]
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the user's full name.
    /// </summary>
    [Required]
    [StringLength(255)]
    public required string FullName { get; set; }

    /// <summary>
    /// Gets or sets the hashed password of the user.
    /// </summary>
    [Required]
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Gets or sets the role assigned to the user.
    /// </summary>
    [StringLength(50)]
    public string? Role { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the timestamp when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the user was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp of the user's last successful login.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Gets or sets the URL of the user's profile image.
    /// </summary>
    [StringLength(500)]
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// Gets or sets a short biography of the user.
    /// </summary>
    [StringLength(1000)]
    public string? Bio { get; set; }

    /// <summary>
    /// Gets or sets the number of consecutive failed login attempts.
    /// </summary>
    public int LoginAttempts { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user account is currently locked.
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Gets or sets the timestamp until which the user account is locked.
    /// </summary>
    public DateTime? LockedUntil { get; set; }

    /// <summary>
    /// Gets the API keys associated with the user.
    /// </summary>
    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();

    /// <summary>
    /// Gets the service registrations managed by the user.
    /// </summary>
    public ICollection<ServiceRegistration> ManagedServices { get; set; } = new List<ServiceRegistration>();

    /// <summary>
    /// Returns a string representation of the user.
    /// </summary>
    /// <returns>A string containing the user's key properties.</returns>
    public override string ToString() => $"User {{ Id = {Id}, Email = {Email}, FullName = {FullName}, Role = {Role}, IsActive = {IsActive} }}";

    /// <summary>
    /// Validates that the user has the minimum required fields.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(FullName) &&
               !string.IsNullOrWhiteSpace(PasswordHash) &&
               Email.Contains("@");
    }

    /// <summary>
    /// Checks if the user is locked due to failed login attempts.
    /// </summary>
    public bool IsAccountLocked()
    {
        if (!IsLocked)
            return false;

        if (LockedUntil is null)
            return true;

        if (DateTime.UtcNow >= LockedUntil)
        {
            IsLocked = false;
            LockedUntil = null;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Records a successful login and resets attempt counter.
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        LoginAttempts = 0;
        IsLocked = false;
        LockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Increments failed login attempts and locks account if threshold exceeded.
    /// </summary>
    public void RecordFailedLoginAttempt(int lockThreshold = 5)
    {
        LoginAttempts++;
        UpdatedAt = DateTime.UtcNow;

        if (LoginAttempts >= lockThreshold)
        {
            IsLocked = true;
            LockedUntil = DateTime.UtcNow.AddMinutes(30);
        }
    }

    /// <summary>
    /// Updates the user's last activity timestamp.
    /// </summary>
    public void UpdateLastActivity()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether the specified user is equal to the current user.
    /// </summary>
    /// <param name="other">The user to compare with the current user.</param>
    /// <returns><see langword="true"/> if the specified user is equal to the current user; otherwise, <see langword="false"/>.</returns>
    public bool Equals(User? other)
    {
        if (other is null) return false;
        return Id == other.Id &&
               Email == other.Email &&
               FullName == other.FullName &&
               PasswordHash == other.PasswordHash &&
               Role == other.Role &&
               IsActive == other.IsActive &&
               CreatedAt == other.CreatedAt &&
               UpdatedAt == other.UpdatedAt;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current user.
    /// </summary>
    /// <param name="obj">The object to compare with the current user.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current user; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as User);
    }

    /// <summary>
    /// Returns a hash code for the current user.
    /// </summary>
    /// <returns>A hash code for the current user.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Email, FullName, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt);
    }

    /// <summary>
    /// Determines whether two user instances are equal.
    /// </summary>
    /// <param name="left">The first user to compare.</param>
    /// <param name="right">The second user to compare.</param>
    /// <returns><see langword="true"/> if the users are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(User? left, User? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two user instances are not equal.
    /// </summary>
    /// <param name="left">The first user to compare.</param>
    /// <param name="right">The second user to compare.</param>
    /// <returns><see langword="true"/> if the users are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(User? left, User? right)
    {
        return !(left == right);
    }
}
