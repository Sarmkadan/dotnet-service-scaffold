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
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(255)]
    public required string Email { get; set; }

    [Required]
    [StringLength(255)]
    public required string FullName { get; set; }

    [Required]
    public required string PasswordHash { get; set; }

    [StringLength(50)]
    public string? Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    [StringLength(500)]
    public string? ProfileImageUrl { get; set; }

    [StringLength(1000)]
    public string? Bio { get; set; }

    public int LoginAttempts { get; set; }

    public bool IsLocked { get; set; }

    public DateTime? LockedUntil { get; set; }

    // Navigation
    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();

    public ICollection<ServiceRegistration> ManagedServices { get; set; } = new List<ServiceRegistration>();

    public override string ToString() => $"User {{ Id = {Id}, Email = {Email}, FullName = {FullName}, PasswordHash = {PasswordHash}, Role = {Role}, IsActive = {IsActive} }}";

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

    public override bool Equals(object? obj)
    {
        return Equals(obj as User);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Email, FullName, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt);
    }

    public static bool operator ==(User? left, User? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(User? left, User? right)
    {
        return !(left == right);
    }
}
