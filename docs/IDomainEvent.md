# IDomainEvent

Base interface for domain events in the `dotnet-service-scaffold` project. Defines the minimal contract required for events published within the domain layer, ensuring consistent identification, timing, and type information for event sourcing and event-driven architectures.

## API

### `EventId`
Unique identifier for the event instance. Must be set when the event is created and remain immutable thereafter.

- **Type**: `Guid`
- **Purpose**: Provides a stable reference to the event for tracking, deduplication, and correlation.
- **Usage**: Assigned once during event construction; never modified after creation.

### `OccurredAt`
Timestamp indicating when the event occurred within the domain.

- **Type**: `DateTime`
- **Purpose**: Records the moment the domain state change was recognized, enabling temporal analysis and event replay.
- **Usage**: Set to the current UTC time when the event is instantiated; not modified thereafter.
- **Note**: Precision is limited to `DateTime` (not `DateTimeOffset`), so timezone context must be managed externally.

### `EventType`
Abstract property representing the fully qualified type name of the concrete event.

- **Type**: `abstract string`
- **Purpose**: Enables polymorphic event handling and serialization without runtime type discovery.
- **Usage**: Must be implemented by derived types to return a unique, stable string (e.g., `"Namespace.EventName"`).
- **Returns**: A non-null, non-empty string identifying the event type.

### `AggregateId`
Identifier of the aggregate root to which this event pertains.

- **Type**: `Guid?`
- **Purpose**: Links the event to its originating aggregate for consistency and replay.
- **Usage**: Optional (`null` if not applicable); must be set when the event is created if the event originates from an aggregate.
- **Note**: If `null`, the event may represent a system-level or external occurrence not tied to a specific aggregate.
