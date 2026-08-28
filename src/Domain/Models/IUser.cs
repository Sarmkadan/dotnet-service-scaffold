#nullable enable

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Interface representing a user in the system with authentication and profile information.
/// </summary>
public interface IUser
{
    Guid Id { get; set; }
    string Email { get; set; }
    string FullName { get; set; }
    string PasswordHash { get; set; }
    string? Role { get; set; }
    bool IsActive { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    DateTime? LastLoginAt { get; set; }
    string? ProfileImageUrl { get; set; }
    string? Bio { get; set; }
    int LoginAttempts { get; set; }
    bool IsLocked { get; set; }
    DateTime? LockedUntil { get; set; }
    ICollection<ApiKey> ApiKeys { get; set; }
    ICollection<ServiceRegistration> ManagedServices { get; set; }
    bool IsValid();
    bool IsAccountLocked();
    void RecordSuccessfulLogin();
    void RecordFailedLoginAttempt(int lockThreshold = 5);
}