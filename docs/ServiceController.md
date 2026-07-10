# ServiceController

The `ServiceController` class provides HTTP endpoints for managing service lifecycle operations within the `dotnet-service-scaffold` project. It facilitates service registration, retrieval, enablement/disablement, and health monitoring, acting as an interface between external clients and internal service management logic.

## API

### `ServiceController`
Constructor for the `ServiceController` class. Initializes dependencies required for service management operations.

### `Task<IActionResult> RegisterService`
Registers a new service with the system.

**Parameters:**
- None (request body contains `RegisterServiceRequest` data).

**Returns:**
- `IActionResult` representing the HTTP response:
  - `200 OK` if registration succeeds.
  - `400 Bad Request` if the request is invalid (e.g., missing required fields).
  - `409 Conflict` if a service with the same identifier already exists.

**Throws:**
- None explicitly, but underlying dependencies may throw exceptions (e.g., database errors).

---

### `Task<IActionResult> GetService`
Retrieves details of a specific service by its identifier.

**Parameters:**
- Query parameter: `serviceId` (string, required) – Unique identifier of the service.

**Returns:**
- `IActionResult` representing the HTTP response:
  - `200 OK` with service details if found.
  - `404 Not Found` if the service does not exist.

**Throws:**
- None explicitly.

---

### `Task<IActionResult> ListServices`
Lists all registered services in the system.

**Parameters:**
- None.

**Returns:**
- `IActionResult` representing the HTTP response:
  - `200 OK` with a collection of service details.

**Throws:**
- None explicitly.

---

### `Task<IActionResult> GetServicesByOwner`
Retrieves all services owned by a specific owner.

**Parameters:**
- Query parameter: `ownerId` (string, required) – Unique identifier of the owner.

**Returns:**
- `IActionResult` representing the HTTP response:
  - `200 OK` with a collection of services owned by the specified owner.
  - `404 Not Found` if no services are found for the owner.

**Throws:**
- None explicitly.

---

### `Task<IActionResult> DisableService`
Disables an active service, preventing it from being used until re-enabled.

**Parameters:**
- Request body: `DisableServiceRequest` (contains `serviceId` and optional `reason`).

**Returns:**
- `IActionResult` representing the HTTP response:
  - `200 OK` if the service was successfully disabled.
  - `400 Bad Request` if the request is invalid.
  - `404 Not Found` if the service does not exist.
  - `409 Conflict` if the service is already disabled.

**Throws:**
- None explicitly.

---

### `Task<IActionResult> EnableService`
Enables a previously disabled service, allowing it to resume normal operation.

**Parameters:**
- Query parameter: `serviceId` (string, required) – Unique identifier of the service.

**Returns:**
- `IActionResult` representing the HTTP response:
  - `200 OK` if the service was successfully enabled.
  - `400 Bad Request` if the request is invalid.
  - `404 Not Found` if the service does not exist.
  - `409 Conflict` if the service is already enabled.

**Throws:**
- None explicitly.

---

### `Task<IActionResult> GetUnhealthyServices`
Retrieves a list of services currently marked as unhealthy.

**Parameters:**
- None.

**Returns:**
- `IActionResult` representing the HTTP response:
  - `200 OK` with a collection of unhealthy services.

**Throws:**
- None explicitly.

---

### `record RegisterServiceRequest`
Data transfer object for service registration requests.

**Properties:**
- `ServiceId` (string) – Unique identifier for the service.
- `OwnerId` (string) – Unique identifier of the service owner.
- `ServiceName` (string) – Human-readable name of the service.
- `Metadata` (dictionary or object, optional) – Additional service metadata.

---

### `record DisableServiceRequest`
Data transfer object for service disablement requests.

**Properties:**
- `ServiceId` (string) – Unique identifier of the service to disable.
- `Reason` (string, optional) – Reason for disabling the service.
