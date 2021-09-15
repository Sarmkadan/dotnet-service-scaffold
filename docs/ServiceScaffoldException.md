# ServiceScaffoldException

`ServiceScaffoldException` serves as the abstract base exception class for the `dotnet-service-scaffold` framework. It provides a common foundation for all service-layer exceptions, carrying an optional machine-readable error code and a collection of human-readable error messages. Derived types represent specific failure categories such as missing resources, validation failures, health check degradation, authorization violations, data access problems, configuration errors, and resource exhaustion.

## API

### Properties

#### `ErrorCode`
`public string? ErrorCode`

Gets the optional, application-defined error code associated with the exception. This value is intended for programmatic handling and logging correlation. Returns `null` when no code has been assigned.

#### `Errors`
`public List<string> Errors`

Gets the list of human-readable error messages describing the failure. The collection is always non-null and may contain zero or more entries. Derived exception types and callers can populate this list to provide detailed diagnostic information.

### Constructors

#### `ServiceScaffoldException(string message)`
Initializes a new instance of the `ServiceScaffoldException` class with a specified error message. The `Errors` list is initialized as empty, and `ErrorCode` remains `null`.

| Parameter | Type     | Description                                      |
| --------- | -------- | ------------------------------------------------ |
| `message` | `string` | The error message that describes the exception.  |

**Remarks:** This constructor is typically called by derived types via `base(message)`.

#### `ServiceScaffoldException()`
Parameterless constructor. Initializes a new instance with an empty `Errors` list and no message or error code. Intended for scenarios where details are populated after instantiation.

#### `ServiceScaffoldException(string message, string? errorCode)`
Initializes a new instance with a specified error message and an application-defined error code.

| Parameter   | Type      | Description                                      |
| ----------- | --------- | ------------------------------------------------ |
| `message`   | `string`  | The error message that describes the exception.  |
| `errorCode` | `string?` | An optional, machine-readable error code.        |

#### `ServiceScaffoldException(string message, Exception innerException)`
Initializes a new instance with a specified error message and a reference to the inner exception that is the cause of this exception.

| Parameter        | Type        | Description                                         |
| ---------------- | ----------- | --------------------------------------------------- |
| `message`        | `string`    | The error message that describes the exception.     |
| `innerException` | `Exception` | The exception that caused the current exception.    |

### Derived Exception Types

The following types inherit from `ServiceScaffoldException` and represent specific failure categories. Each exposes constructors consistent with the base class pattern.

- **`ServiceNotFoundException`** — Thrown when a requested service, endpoint, or resource cannot be located. Constructors: parameterless, `(string message)`, `(string message, string? errorCode)`, `(string message, Exception innerException)`.
- **`ServiceValidationException`** — Thrown when input data fails service-level validation rules. Constructors: parameterless, `(string message)`, `(string message, string? errorCode)`, `(string message, Exception innerException)`.
- **`HealthCheckException`** — Thrown when a health check probe reports a degraded or unhealthy status. Constructor: `(string message)`.
- **`UnauthorizedException`** — Thrown when an operation is attempted without sufficient authorization. Constructor: `(string message)`.
- **`InvalidApiKeyException`** — Thrown when a supplied API key is missing, malformed, or revoked. Constructor: `(string message)`.
- **`DataAccessException`** — Thrown when an error occurs in the data access layer, such as database connectivity or query failures. Constructor: `(string message)`.
- **`ConfigurationException`** — Thrown when required configuration values are absent or invalid. Constructor: `(string message)`.
- **`ResourceExhaustedException`** — Thrown when a rate limit, quota, or capacity boundary is exceeded. Constructor: `(string message)`.

## Usage

### Example 1: Throwing and Catching a Validation Exception

```csharp
public void ProcessOrder(OrderRequest request)
{
    var errors = new List<string>();

    if (request.Quantity <= 0)
        errors.Add("Quantity must be greater than zero.");
    if (string.IsNullOrWhiteSpace(request.ProductId))
        errors.Add("ProductId is required.");

    if (errors.Count > 0)
    {
        var ex = new ServiceValidationException("Order validation failed.", "ORDER_VALIDATION_ERROR");
        ex.Errors.AddRange(errors);
        throw ex;
    }

    // Proceed with order processing.
}

try
{
    ProcessOrder(new OrderRequest { Quantity = 0, ProductId = "" });
}
catch (ServiceValidationException ex)
{
    Console.WriteLine($"Error Code: {ex.ErrorCode}");
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

### Example 2: Wrapping a Data Access Failure

```csharp
public CustomerProfile GetCustomerProfile(int customerId)
{
    try
    {
        return _repository.FetchProfile(customerId);
    }
    catch (SqlException sqlEx)
    {
        var dataEx = new DataAccessException(
            $"Failed to retrieve profile for customer {customerId}.",
            sqlEx);

        dataEx.Errors.Add("A temporary database error occurred. Please retry.");
        throw dataEx;
    }
}

try
{
    var profile = GetCustomerProfile(42);
}
catch (DataAccessException ex)
{
    _logger.LogError(ex, ex.ErrorCode);
    // Return a user-friendly message from ex.Errors.
}
```

## Notes

- **Thread Safety:** The `Errors` property exposes a `List<string>`, which is not inherently thread-safe. Concurrent modifications to the error list across multiple threads may result in race conditions or corruption. Callers should ensure that population of `Errors` occurs on a single thread before the exception is thrown and subsequently read from a single consumer thread.
- **Error Code Uniqueness:** The `ErrorCode` property is optional and not enforced for uniqueness across exception instances. Consumers relying on `ErrorCode` for programmatic branching should establish conventions within their application to avoid ambiguity.
- **Empty Errors List:** Derived exception types may be instantiated without populating `Errors`. Handlers should not assume the list contains entries and must guard against empty collections when enumerating.
- **Inner Exception Propagation:** When an inner exception is supplied via the `(string message, Exception innerException)` constructor, the base `Exception` class preserves it for debugging and logging. The `Errors` list remains independent and is not automatically populated from the inner exception’s message.
- **Derived Type Constructors:** Each derived exception type exposes a subset of the base constructors. Refer to the specific type’s signature to determine which overloads are available. The parameterless constructor is present only on `ServiceNotFoundException` and `ServiceValidationException`.
