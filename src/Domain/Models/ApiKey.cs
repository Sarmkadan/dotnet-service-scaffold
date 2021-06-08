// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// API authentication key for programmatic access to the scaffold system.
/// </summary>
public class ApiKey
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    public User? User { get; set; }

    [Required]
    [StringLength(255)]
    public required string Name { get; set; }

    [Required]
    [StringLength(500)]
    public required string KeyHash { get; set; }

    [Required]
    [StringLength(50)]
    public required string KeyPrefix { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public DateTime? LastUsedAt { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(1000)]
    public string? AllowedIps { get; set; }

    [StringLength(500)]
    public string? AllowedScopes { get; set; }

    public long ApiCallsCount { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Checks if the API key is currently valid for use.
    /// </summary>
    public bool IsValid()
    {
        if (!IsActive)
            return false;

        if (ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt)
            return false;

        return !string.IsNullOrEmpty(KeyHash) && !string.IsNullOrEmpty(KeyPrefix);
    }

    /// <summary>
    /// Determines if the key has expired.
    /// </summary>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt;
    }

    /// <summary>
    /// Gets the number of days until expiration, or null if no expiration.
    /// </summary>
    public int? GetDaysUntilExpiration()
    {
        if (!ExpiresAt.HasValue)
            return null;

        var days = (int)(ExpiresAt.Value - DateTime.UtcNow).TotalDays;
        return Math.Max(0, days);
    }

    /// <summary>
    /// Checks if the source IP is in the allowed list (if configured).
    /// </summary>
    public bool IsIpAllowed(string sourceIp)
    {
        if (string.IsNullOrWhiteSpace(AllowedIps))
            return true;

        var allowedIps = AllowedIps.Split(',', StringSplitOptions.TrimEntries);
        return allowedIps.Contains(sourceIp);
    }

    /// <summary>
    /// Checks if the requested scope is in the allowed scopes list.
    /// </summary>
    public bool HasScope(string requestedScope)
    {
        if (string.IsNullOrWhiteSpace(AllowedScopes))
            return true;

        var scopes = AllowedScopes.Split(',', StringSplitOptions.TrimEntries);
        return scopes.Contains(requestedScope) || scopes.Contains("*");
    }

    /// <summary>
    /// Records that the API key was used successfully.
    /// </summary>
    public void RecordUsage()
    {
        LastUsedAt = DateTime.UtcNow;
        ApiCallsCount++;
    }

    /// <summary>
    /// Revokes the API key (disables it).
    /// </summary>
    public void Revoke()
    {
        IsActive = false;
    }
}
