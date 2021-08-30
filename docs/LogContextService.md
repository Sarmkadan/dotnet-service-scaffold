# LogContextService

The `LogContextService` provides a mechanism for maintaining and propagating structured logging properties across asynchronous execution boundaries. It enables the association of contextual metadata with specific logical operations, ensuring that all log events captured within the scope of these operations are automatically enriched with the defined data. This promotes consistent diagnostic information across distributed or multi-threaded service architectures by abstracting the complexities of context storage from the application logic.

## API

### AddProperty(string name, object? value)
Adds a single property to the current logging context.
*   **Parameters:**
    *   `name`: The key identifying the property.
    *   `value`: The value to be associated with the key.
*   **Returns:** `void`.
*   **Throws:** `ArgumentNullException` if `name` is null.

### GetProperties()
Retrieves a snapshot of the properties currently defined in the context.
*   **Returns:** `IReadOnlyDictionary<string, object?>` containing all currently configured properties.

### PushProperties(IDictionary<string, object?> properties)
Temporarily adds a collection of properties to the current context for the duration of the returned `IDisposable` scope.
*   **Parameters:**
    *   `properties`: A dictionary of key-value pairs to add to the context.
*   **Returns:** `IDisposable`. Disposing of this object removes the pushed properties from the context, restoring the previous state.

### Dispose()
Performs cleanup of resources held by the service.
*   **Returns:** `void`.

## Usage

### Attaching Persistent Context
```csharp
public void ProcessOrder(Order order, LogContextService logContext)
{
    logContext.AddProperty("OrderId", order.Id);
    logContext.AddProperty("UserId", order.UserId);

    // Subsequent logs within this execution flow will include OrderId and UserId
    _logger.LogInformation("Processing order.");
}
```

### Scoped Property Injection
```csharp
public async Task ExecuteSecureOperation(string transactionId, LogContextService logContext)
{
    var contextData = new Dictionary<string, object?> { { "TransactionId", transactionId } };
    
    using (logContext.PushProperties(contextData))
    {
        // This log entry includes TransactionId
        _logger.LogInformation("Operation started.");
        await _service.DoWorkAsync();
    }

    // This log entry does not include TransactionId
    _logger.LogInformation("Operation completed.");
}
```

## Notes

*   **Thread Safety:** The `LogContextService` is designed to be thread-safe regarding its storage mechanism. It typically utilizes `AsyncLocal<T>` internally to ensure that context remains isolated to the logical asynchronous execution flow, preventing data leakage between concurrent requests or tasks.
*   **Context Scope:** Modifications made via `AddProperty` persist for the remainder of the current execution flow. Use `PushProperties` when temporal scoping is required to ensure that properties are automatically removed after a specific block of code completes.
*   **Performance:** While lightweight, frequent updates to the context within tight loops should be avoided to minimize object allocation overhead associated with dictionary maintenance.
*   **Disposal:** While `LogContextService` implements `IDisposable`, it is typically managed by the dependency injection container. Manually calling `Dispose()` is generally unnecessary unless explicitly managing the service instance lifecycle outside of standard DI patterns.
