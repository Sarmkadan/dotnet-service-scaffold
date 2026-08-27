#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Records audit trails for system actions performed by users.
/// </summary>
public sealed class AuditLog : IEquatable<AuditLog>
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(User))]
    public Guid? UserId { get; set; }

    public User? User { get; set; }

    [Required]
    [StringLength(100)]
    public required string ActionName { get; set; }

    [Required]
    [StringLength(50)]
    public required string EntityType { get; set; }

    public Guid? EntityId { get; set; }

    [StringLength(4000)]
    public string? OldValues { get; set; }

    [StringLength(4000)]
    public string? NewValues { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [StringLength(255)]
    public string? IpAddress { get; set; }

    [StringLength(1000)]
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool Equals(AuditLog? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) &&
               UserId.Equals(other.UserId) &&
               Equals(User, other.User) &&
               ActionName == other.ActionName &&
               EntityType == other.EntityType &&
               EntityId.Equals(other.EntityId) &&
               OldValues == other.OldValues &&
               NewValues == other.NewValues;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is AuditLog other && Equals(other));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, UserId, User, ActionName, EntityType, EntityId, OldValues, NewValues);
    }

    public static bool operator ==(AuditLog? left, AuditLog? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(AuditLog? left, AuditLog? right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Creates a summary of the audit log entry for display purposes.
    /// </summary>
    public string GetSummary()
    {
        var actor = User?.FullName ?? AuditLogConstants.DefaultActor;
        return string.Format(AuditLogConstants.SummaryFormat, actor, ActionName, EntityType, EntityId, CreatedAt);
    }

    /// <summary>
    /// Determines if the action was successful or failed based on status.
    /// </summary>
    public bool WasSuccessful()
    {
        return Status == AuditLogConstants.SuccessStatus || Status is null;
    }

    /// <summary>
    /// Gets a human‑readable action description.
    /// </summary>
    public string GetActionDescription()
    {
        return ActionName switch
        {
            AuditLogConstants.ActionCreate => AuditLogConstants.DescriptionCreated,
            AuditLogConstants.ActionUpdate => AuditLogConstants.DescriptionUpdated,
            AuditLogConstants.ActionDelete => AuditLogConstants.DescriptionDeleted,
            AuditLogConstants.ActionRestore => AuditLogConstants.DescriptionRestored,
            AuditLogConstants.ActionLogin => AuditLogConstants.DescriptionLoggedIn,
            AuditLogConstants.ActionLogout => AuditLogConstants.DescriptionLoggedOut,
            AuditLogConstants.ActionExport => AuditLogConstants.DescriptionExported,
            AuditLogConstants.ActionImport => AuditLogConstants.DescriptionImported,
            _ => ActionName
        };
    }
}
