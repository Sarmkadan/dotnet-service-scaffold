#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Events;

/// <summary>
/// Interface for handling a specific type of domain event.
/// Implementations should handle the business logic that responds to the event.
/// Use dependency injection to register handlers in the DI container.
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    /// <summary>
    /// Handles the domain event. Implement the business logic for responding to the event.
    /// </summary>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
