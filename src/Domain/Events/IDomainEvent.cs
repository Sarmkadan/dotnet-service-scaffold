#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Events;

/// <summary>
/// Base interface for domain events. Domain events represent something significant that
/// occurred in the domain. They allow decoupled communication between different parts of
/// the application. All domain events should implement this interface.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this specific event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// The UTC timestamp when this event occurred.
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// The type of event that occurred (e.g., "user.created", "service.started").
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// Optional aggregate root ID that this event relates to.
    /// </summary>
    Guid? AggregateId { get; }
}

/// <summary>
/// Base class for domain events providing common functionality.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
    public Guid? AggregateId { get; set; }
}
