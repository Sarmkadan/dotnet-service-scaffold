#nullable enable
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
public sealed class ApiKey : IApiKey, IEquatable<ApiKey>
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    public User? User { get; set; }

    [Required]
    [StringLength(ApiKeyConstants.NameMaxLength)]
    public required string Name { get; set; }

    [Required]
    [StringLength(ApiKeyConstants.KeyHashMaxLength)]
    public required string KeyHash { get; set; }

    [Required]
    [StringLength(ApiKeyConstants.KeyPrefixMaxLength)]
    public required string KeyPrefix { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public DateTime? LastUsedAt { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(ApiKeyConstants.AllowedIpsMaxLength)]
    public string? AllowedIps { get; set; }

    [StringLength(ApiKeyConstants.AllowedScopesMaxLength)]
    public string? AllowedScopes { get; set; }

    public long ApiCallsCount { get; set; }

    [StringLength(ApiKeyConstants.DescriptionMaxLength)]
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
        return Math.Max(ApiKeyConstants.Zero, days);
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
        return scopes.Contains(requestedScope) || scopes.Contains(ApiKeyConstants.WildcardScope);
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

    public bool Equals(ApiKey? other)
    {
        if (other is null)
            return false;

        return Id.Equals(other.Id)
            && UserId.Equals(other.UserId)
            && Equals(User, other.User)
            && Name == other.Name
            && KeyHash == other.KeyHash
            && KeyPrefix == other.KeyPrefix
            && CreatedAt.Equals(other.CreatedAt)
            && ExpiresAt.Equals(other.ExpiresAt);
    }

    public override bool Equals(object? obj)
    {
        if (obj is ApiKey other)
            return Equals(other);

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, UserId, User, Name, KeyHash, KeyPrefix, CreatedAt, ExpiresAt);
    }

    public static bool operator ==(ApiKey? left, ApiKey? right)
    {
        if (left is null)
            return right is null;

        return left.Equals(right);
    }

    public static bool operator !=(ApiKey? left, ApiKey? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"ApiKey {{ Id = {Id}, UserId = {UserId}, User = {User}, Name = {Name}, KeyHash = {KeyHash}, KeyPrefix = {KeyPrefix} }}";
    }
}
