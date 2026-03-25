#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Publishes domain events to all registered handlers. Implements the Mediator pattern
/// to decouple event sources from event handlers. Handlers are discovered via dependency injection.
/// </summary>
public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventPublisher> _logger;

    public DomainEventPublisher(IServiceProvider serviceProvider, ILogger<DomainEventPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Publishes an event to all registered handlers. Executes handlers sequentially.
    /// Logs any exceptions from handlers but continues publishing to other handlers.
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));

        _logger.LogInformation(
            "Publishing domain event {EventType} with ID {EventId}",
            @event.EventType, @event.EventId);

        // Get all handlers for this event type
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(typeof(TEvent));
        var handlers = _serviceProvider.GetServices(handlerType);

        if (!handlers.Any())
        {
            _logger.LogWarning(
                "No handlers registered for event type {EventType}",
                @event.EventType);
            return;
        }

        var tasks = handlers
            .OfType<IDomainEventHandler<TEvent>>() // Safely cast to the concrete handler interface
            .Select(handler => handler.HandleAsync(@event, cancellationToken))
            .ToList();

        try
        {
            await Task.WhenAll(tasks);
            _logger.LogInformation(
                "Domain event {EventType} published to {HandlerCount} handler(s)",
                @event.EventType, handlers.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing domain event {EventType}",
                @event.EventType);
            throw;
        }
    }

    /// <summary>
    /// Publishes multiple events. Useful for domain aggregates that produce multiple events.
    /// </summary>
    public async Task PublishMultipleAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        var eventList = events?.ToList() ?? new List<IDomainEvent>();

        _logger.LogInformation(
            "Publishing {EventCount} domain events",
            eventList.Count);

        foreach (var @event in eventList)
        {
            // Use reflection to call PublishAsync with the correct generic type
            var publishMethod = GetType()
                .GetMethod(
                    nameof(PublishAsync),
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new Type[] { @event.GetType(), typeof(CancellationToken) },
                    null)
                ?.MakeGenericMethod(@event.GetType());

            if (publishMethod is null)
                continue;

            await (dynamic)publishMethod.Invoke(this, new object?[] { @event, cancellationToken })!;
        }
    }
}

/// <summary>
/// Interface for domain event publisher.
/// </summary>
public interface IDomainEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
    Task PublishMultipleAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
