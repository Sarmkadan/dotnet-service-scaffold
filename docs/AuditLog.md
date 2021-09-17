# AuditLog

The `AuditLog` class is a domain entity that records an audit trail entry for actions performed within the system. Each instance captures the identity of the acting user, the action and entity involved, the old and new state of the entity (as serialized strings), the outcome status, network metadata, and a timestamp. It provides convenience methods to produce a human-readable summary and to determine whether the action was successful.

## API

### `public Guid Id`
The unique identifier of this audit log entry. It is assigned when the entity is created and cannot be changed.

### `public Guid? UserId`
The identifier of the user who performed the action. May be `null` if the action was system-initiated or the user is unknown.

### `public User? User`
Navigation property to the `User` entity associated with `UserId`. This property is typically populated by the data access layer when the related user is loaded. May be `null` if the user is not loaded or does not exist.

### `public required string ActionName`
The name of the action performed (e.g., "Create", "Update", "Delete"). This property is required and must be set before the entity is considered valid.

### `public required string EntityType`
The type of the entity that was acted upon (e.g., "Order", "Invoice"). This property is required.

### `public Guid? EntityId`
The identifier of the specific entity instance that was acted upon. May be `null` if the action does not target a single entity.

### `public string? OldValues`
A serialized representation (typically JSON) of the entity’s state before the action. May be `null` if the previous state is not available.

### `public string? NewValues`
A serialized representation (typically JSON) of the entity’s state after the action. May be `null` if the new state is not available.

### `public string? Status`
The outcome status of the action (e.g., "Success", "Failure"). May be `null` if the status is not recorded.

### `public string? IpAddress`
The IP address of the client that performed the action. May be `null` if not captured.

### `public string? UserAgent`
The user agent string of the client that performed the action. May be `null` if not captured.

### `public DateTime CreatedAt`
The UTC timestamp when the audit log entry was created. This is typically set to the current time when the instance is created.

### `public string? Description`
An optional human-readable description of the action. May be `null`.

### `public string GetSummary()`
Returns a concise summary string that combines key fields (e.g., `ActionName`, `EntityType`, `EntityId`, `Status`, and `CreatedAt`).  
**Returns:** A `string` containing the summary.  
**Throws:** No exceptions are thrown under normal circumstances, but the method may rely on `ActionName` and `EntityType` being non-null (they are required by the type system).

### `public bool WasSuccessful`
A computed property that indicates whether the action was successful. It typically checks whether `Status` equals a known success value (e.g., `"Success"`).  
**Returns:** `true` if the action is considered successful; otherwise `false`.  
**Throws:** No exceptions.

### `public string GetActionDescription()`
Returns a detailed description of the action, often incorporating `ActionName`, `EntityType`, `EntityId`, `OldValues`, `NewValues`, and `Description`.  
**Returns:** A `string` containing the full description.  
**Throws:** No exceptions are thrown, but the method may assume that `ActionName` and `EntityType` are non-null.

## Usage

### Example 1: Creating and populating an AuditLog entry

```csharp
var audit = new AuditLog
{
    ActionName = "Update",
    EntityType = "Order",
    EntityId = orderId,
    OldValues = JsonSerializer.Serialize(oldOrder),
    NewValues = JsonSerializer.Serialize(newOrder),
    Status = "Success",
    IpAddress = "192.168.1.100",
    UserAgent = "Mozilla/5.0 ...",
    CreatedAt = DateTime.UtcNow,
    Description = "Updated order shipping address"
};

// If a user context is available:
audit.UserId = currentUserId;

// Persist via repository
await auditLogRepository.AddAsync(audit);
```

### Example 2: Using GetSummary and WasSuccessful for logging

```csharp
// After retrieving an audit log entry
AuditLog entry = await auditLogRepository.GetByIdAsync(auditId);

if (entry.WasSuccessful)
{
    Console.WriteLine($"Action succeeded: {entry.GetSummary()}");
}
else
{
    Console.WriteLine($"Action failed: {entry.GetActionDescription()}");
}
```

## Notes

- **Required members:** `ActionName` and `EntityType` are marked with the `required` modifier. The compiler enforces that they are set during object initialization. Failing to set them results in a compile-time error.
- **Nullable fields:** `UserId`, `User`, `EntityId`, `OldValues`, `NewValues`, `Status`, `IpAddress`, `UserAgent`, and `Description` are all nullable. Code that consumes these properties should handle `null` gracefully, especially when formatting output or performing comparisons.
- **Thread safety:** This class is not thread-safe. Concurrent reads and writes to the same instance are not synchronized. If audit log entries are shared across threads, consider using immutable records or defensive copying.
- **Serialization:** `OldValues` and `NewValues` are expected to be JSON strings, but the class does not enforce this. Consumers should validate or parse these strings carefully to avoid runtime exceptions.
- **Computed members:** `WasSuccessful` and the methods `GetSummary()` and `GetActionDescription()` rely on the state of other properties. Their behavior may change if the underlying properties are modified after the instance is created.
