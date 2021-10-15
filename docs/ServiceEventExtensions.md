# ServiceEventExtensions

Provides utility methods for working with `ServiceEvent` instances, including filtering, display formatting, service membership checks, and priority level extraction.

## API

### `public static bool IsRecent(ServiceEvent serviceEvent, TimeSpan threshold)`

Determines whether a service event is considered recent based on a configurable time threshold.

- **Parameters**
  - `serviceEvent`: The `ServiceEvent` instance to evaluate.
  - `threshold`: The maximum age for an event to be considered recent.
- **Return Value**
  - `true` if the event's timestamp is within the threshold; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `serviceEvent` is `null`.
  - Throws `ArgumentNullException` if `threshold` is not a valid `TimeSpan`.

---

### `public static string GetDisplayString(ServiceEvent serviceEvent)`

Generates a human-readable string representation of a service event.

- **Parameters**
  - `serviceEvent`: The `ServiceEvent` instance to format.
- **Return Value**
  - A formatted string containing event details such as timestamp, type, and description.
- **Exceptions**
  - Throws `ArgumentNullException` if `serviceEvent` is `null`.

---

### `public static bool BelongsToService(ServiceEvent serviceEvent, string serviceName)`

Checks whether a service event belongs to a specific service by name.

- **Parameters**
  - `serviceEvent`: The `ServiceEvent` instance to check.
  - `serviceName`: The name of the service to match against.
- **Return Value**
  - `true` if the event's service identifier matches `serviceName`; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `serviceEvent` or `serviceName` is `null`.

---
### `public static int GetPriorityLevel(ServiceEvent serviceEvent)`

Extracts the priority level of a service event as an integer.

- **Parameters**
  - `serviceEvent`: The `ServiceEvent` instance to evaluate.
- **Return Value**
  - An integer representing the event's priority level. Higher values indicate higher priority.
- **Exceptions**
  - Throws `ArgumentNullException` if `serviceEvent` is `null`.

## Usage
