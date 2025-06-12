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
public class AuditLog
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

    /// <summary>
    /// Creates a summary of the audit log entry for display purposes.
    /// </summary>
    public string GetSummary()
    {
        var actor = User?.FullName ?? "System";
        return $"{actor} performed {ActionName} on {EntityType} " +
               $"({EntityId}) at {CreatedAt:O}";
    }

    /// <summary>
    /// Determines if the action was successful or failed based on status.
    /// </summary>
    public bool WasSuccessful()
    {
        return Status == "Success" || Status == null;
    }

    /// <summary>
    /// Gets a human-readable action description.
    /// </summary>
    public string GetActionDescription()
    {
        return ActionName switch
        {
            "Create" => "Created",
            "Update" => "Updated",
            "Delete" => "Deleted",
            "Restore" => "Restored",
            "Login" => "Logged in",
            "Logout" => "Logged out",
            "Export" => "Exported",
            "Import" => "Imported",
            _ => ActionName
        };
    }
}
