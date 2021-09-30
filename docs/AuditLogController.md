# AuditLogController

The `AuditLogController` provides HTTP endpoints for querying and retrieving audit log entries from the system. It exposes paginated listing of all audit events, retrieval of individual audit records by identifier, and filtering of audit logs scoped to a specific user. The controller relies on a set of DTO properties to shape the response payload, including metadata for pagination and the collection of audit entries.

## API

### public AuditLogController

Constructor. Initializes a new instance of the controller with the required dependencies for audit log retrieval. No explicit parameters are listed in the public surface; dependencies are injected by the framework.

### public async Task<IActionResult> ListAuditLogs

Returns a paginated list of audit log entries.

- **Parameters**: Bound from the query string — `Page` (int), `PageSize` (int), and optional filters such as `ActionName` (string?), `EntityType` (string?), `UserId` (Guid?), and date-range constraints derived from `CreatedAt` (DateTime).
- **Returns**: `IActionResult` containing a 200 OK response with a payload that includes `Data` (the list of `AuditLogDto`), `Page`, `PageSize`, `TotalCount`, and `TotalPages`.
- **Throws**: May return a 400 Bad Request if pagination parameters are invalid (e.g., `Page` < 1, `PageSize` ≤ 0). Returns 401/403 for unauthenticated or unauthorized callers if authorization is enforced.

### public async Task<IActionResult> GetAuditLog

Retrieves a single audit log entry by its unique identifier.

- **Parameters**: `Id` (Guid) from the route.
- **Returns**: `IActionResult` with 200 OK and the `AuditLogDto` when found; 404 Not Found if no entry matches the provided `Id`.
- **Throws**: Standard authorization failures (401/403) when applicable.

### public async Task<IActionResult> GetUserAuditLogs

Retrieves all audit log entries associated with a specific user, with optional pagination and filtering.

- **Parameters**: `UserId` (Guid?) from the route or query, along with optional `Page`, `PageSize`, `ActionName`, `EntityType`, and `CreatedAt` filters.
- **Returns**: `IActionResult` with 200 OK containing a paginated result set identical in structure to `ListAuditLogs`, but pre-filtered to the given user.
- **Throws**: 400 Bad Request for invalid pagination values; 404 Not Found if the specified `UserId` does not exist (when user existence is validated); 401/403 for authorization failures.

### public Guid Id

Represents the unique identifier of an audit log entry. Used as a route parameter in `GetAuditLog` and exposed in each `AuditLogDto` within `Data`.

### public Guid? UserId

Optional identifier of the user who performed the audited action. Used as a filter parameter in `ListAuditLogs` and as the primary route parameter in `GetUserAuditLogs`. Nullable to accommodate system-initiated events that lack a user context.

### public string? ActionName

Optional filter for the name of the action performed (e.g., `"Create"`, `"Delete"`). Nullable; when omitted, no action-name filtering is applied.

### public string? EntityType

Optional filter for the type of entity affected (e.g., `"Order"`, `"UserProfile"`). Nullable; when omitted, results are not restricted by entity type.

### public string? Description

A human-readable description of the audit event. Exposed in each `AuditLogDto` within the response payload. Not used as a filter parameter on the controller surface.

### public DateTime CreatedAt

The timestamp when the audit event was recorded. Used as a range filter boundary in `ListAuditLogs` and `GetUserAuditLogs`. In response DTOs, it reflects the exact creation time of each entry.

### public List<AuditLogDto> Data

The collection of audit log entries returned in a paginated response. Each element contains `Id`, `UserId`, `ActionName`, `EntityType`, `Description`, and `CreatedAt`. This property is populated only in successful 200 responses.

### public int Page

The current page number (1-based) in a paginated response. Must be ≥ 1 when supplied as a request parameter.

### public int PageSize

The number of items per page in a paginated response. Must be > 0 when supplied as a request parameter. Typical upper bounds are enforced server-side to prevent excessive payloads.

### public int TotalCount

The total number of records matching the current filter criteria, regardless of pagination. Used by clients to compute pagination controls.

### public int TotalPages

The total number of pages available given `TotalCount` and `PageSize`. Computed as `Math.Ceiling(TotalCount / (double)PageSize)`.

## Usage

### Example 1: Retrieve a paginated list of all audit logs with optional filters

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public async Task FetchAuditLogsAsync(HttpClient client)
{
    var requestUri = "/api/auditlogs?page=1&pageSize=20&actionName=Delete&entityType=Document";

    var response = await client.GetAsync(requestUri);
    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadFromJsonAsync<AuditLogListResponse>();
    Console.WriteLine($"Page {result.Page} of {result.TotalPages} ({result.TotalCount} total records)");

    foreach (var entry in result.Data)
    {
        Console.WriteLine($"[{entry.CreatedAt:O}] {entry.ActionName} on {entry.EntityType}: {entry.Description}");
    }
}

// DTO matching the controller's response shape
public class AuditLogListResponse
{
    public List<AuditLogDto> Data { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? ActionName { get; set; }
    public string? EntityType { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Example 2: Fetch audit logs for a specific user and handle a missing user

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public async Task FetchUserAuditLogsAsync(HttpClient client, Guid userId)
{
    var requestUri = $"/api/users/{userId}/auditlogs?page=1&pageSize=10";

    var response = await client.GetAsync(requestUri);

    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        Console.WriteLine($"User {userId} not found or has no audit entries.");
        return;
    }

    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadFromJsonAsync<AuditLogListResponse>();
    Console.WriteLine($"User {userId} has {result.TotalCount} audit entries.");

    foreach (var entry in result.Data)
    {
        Console.WriteLine($"{entry.ActionName} — {entry.Description}");
    }
}
```

## Notes

- **Pagination edge cases**: When `Page` exceeds `TotalPages`, the controller may return an empty `Data` list rather than a 400 error; clients should handle this gracefully. Supplying a `PageSize` of zero or a negative value typically results in a 400 Bad Request.
- **Nullable filters**: `ActionName`, `EntityType`, and `UserId` are nullable. When all are omitted, the endpoint returns unfiltered results. Supplying an empty string for `ActionName` or `EntityType` may be treated as “no filter” or may match entries with an empty action/entity name, depending on the implementation.
- **Thread safety**: The controller itself is stateless with respect to the listed public members; all state flows through the request context and the underlying data store. Thread safety depends on the scoped lifetime of the controller instance and the thread safety of the injected services. No shared mutable state is exposed through the listed members.
- **Authorization**: The listed members do not include authorization attributes in the public surface, but typical deployments enforce authentication and role-based access on audit log endpoints. Unauthorized requests will yield 401 or 403 responses before reaching the action methods.
- **DateTime precision**: `CreatedAt` is exposed as `DateTime` with no explicit time-zone offset. Clients should assume UTC and convert to local time as needed. When used as a filter, range semantics (inclusive/exclusive boundaries) should be verified against the actual implementation.
