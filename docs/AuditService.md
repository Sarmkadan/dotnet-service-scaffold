# AuditService

Provides asynchronous logging and retrieval of audit records for actions, entities, and users. Supports tracking successful actions, failed actions, and periodic cleanup of stale logs.

## API

### `public AuditService`

Initializes a new instance of the audit logging service. Dependencies such as a database context or logging provider are injected via constructor parameters.

### `public async Task LogActionAsync`

Logs a user action to the audit trail.

- **Parameters**
  - `userId`: Identifier of the user performing the action.
  - `action`: Description of the action performed.
  - `entityType`: Optional type of entity involved in the action.
  - `entityId`: Optional identifier of the entity involved in the action.
  - `metadata`: Optional additional context as key-value pairs.

- **Return value**
  - A `Task` representing the asynchronous operation.

- **Exceptions**
  - Throws `ArgumentNullException` if `userId` or `action` is `null`.
  - Throws `ArgumentException` if `userId` or `action` is empty.

### `public async Task<AuditLog?> GetAuditLogAsync`

Retrieves a single audit log entry by its unique identifier.

- **Parameters**
  - `logId`: The unique identifier of the audit log entry.

- **Return value**
  - An `AuditLog` instance if found; otherwise, `null`.

- **Exceptions**
  - Throws `ArgumentException` if `logId` is empty.

### `public async Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync`

Retrieves all audit logs associated with a specific user.

- **Parameters**
  - `userId`: The unique identifier of the user.

- **Return value**
  - An `IEnumerable<AuditLog>` containing all logs for the user, possibly empty.

- **Exceptions**
  - Throws `ArgumentNullException` if `userId` is `null`.
  - Throws `ArgumentException` if `userId` is empty.

### `public async Task<IEnumerable<AuditLog>> GetEntityAuditLogsAsync`

Retrieves all audit logs associated with a specific entity.

- **Parameters**
  - `entityType`: The type of the entity.
  - `entityId`: The unique identifier of the entity.

- **Return value**
  - An `IEnumerable<AuditLog>` containing all logs for the entity, possibly empty.

- **Exceptions**
  - Throws `ArgumentNullException` if `entityType` or `entityId` is `null`.
  - Throws `ArgumentException` if `entityType` or `entityId` is empty.

### `public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync`

Retrieves the most recent audit logs, limited by a specified count.

- **Parameters**
  - `count`: Maximum number of logs to return.

- **Return value**
  - An `IEnumerable<AuditLog>` containing up to `count` most recent logs, possibly empty.

- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `count` is less than zero.

### `public async Task<IEnumerable<AuditLog>> GetFailedActionsAsync`

Retrieves all audit logs representing failed actions.

- **Return value**
  - An `IEnumerable<AuditLog>` containing all logs for failed actions, possibly empty.

### `public async Task LogFailedActionAsync`

Logs a failed user action to the audit trail.

- **Parameters**
  - `userId`: Identifier of the user performing the action.
  - `action`: Description of the action that failed.
  - `entityType`: Optional type of entity involved in the action.
  - `entityId`: Optional identifier of the entity involved in the action.
  - `failureReason`: Reason for the failure.

- **Return value**
  - A `Task` representing the asynchronous operation.

- **Exceptions**
  - Throws `ArgumentNullException` if `userId`, `action`, or `failureReason` is `null`.
  - Throws `ArgumentException` if `userId`, `action`, or `failureReason` is empty.

### `public async Task CleanupOldLogsAsync`

Removes audit logs older than a specified retention period.

- **Parameters**
  - `retentionDays`: Minimum age in days of logs to retain.

- **Return value**
  - A `Task` representing the asynchronous operation.

- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `retentionDays` is less than zero.

## Usage
