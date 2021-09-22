# ConfigurationService

The `ConfigurationService` provides an asynchronous interface for managing application settings within the `dotnet-service-scaffold` ecosystem. It supports CRUD operations for structured service configurations and offers typed helper methods to retrieve specific setting values directly, abstracting away underlying storage mechanisms and type conversion logic.

## API

### Constructors

#### `public ConfigurationService()`
Initializes a new instance of the `ConfigurationService` class.

### Methods

#### `public async Task<ServiceConfiguration?> GetConfigurationAsync`
Retrieves a single configuration entry by its identifier.
*   **Parameters**: Accepts a key or identifier (specific parameter signature inferred from usage context) to locate the configuration.
*   **Return Value**: Returns a `ServiceConfiguration` object if found; otherwise, returns `null`.
*   **Exceptions**: Throws if the underlying data store is unavailable or if the request times out.

#### `public async Task<IEnumerable<ServiceConfiguration>> GetAllConfigurationsAsync`
Retrieves all configuration entries across all services.
*   **Parameters**: None.
*   **Return Value**: Returns an enumerable collection of `ServiceConfiguration` objects. Returns an empty collection if no configurations exist.
*   **Exceptions**: Throws if the underlying data store is unavailable.

#### `public async Task<IEnumerable<ServiceConfiguration>> GetServiceConfigurationsAsync`
Retrieves all configuration entries scoped to a specific service.
*   **Parameters**: Accepts a service identifier to filter the results.
*   **Return Value**: Returns an enumerable collection of `ServiceConfiguration` objects associated with the specified service.
*   **Exceptions**: Throws if the service identifier is invalid or the data store is unreachable.

#### `public async Task<ServiceConfiguration> SetConfigurationAsync`
Creates or updates a configuration entry.
*   **Parameters**: Accepts a `ServiceConfiguration` object containing the data to persist.
*   **Return Value**: Returns the persisted `ServiceConfiguration` object, potentially including updated metadata such as version stamps or timestamps.
*   **Exceptions**: Throws if the input object is null, invalid, or if a concurrency conflict occurs during the update.

#### `public async Task DeleteConfigurationAsync`
Removes a specific configuration entry.
*   **Parameters**: Accepts the identifier of the configuration to delete.
*   **Return Value**: Returns a completed `Task`.
*   **Exceptions**: Throws if the identifier does not exist or if the user lacks permission to delete the resource.

#### `public async Task<int> GetConfigIntAsync`
Retrieves a configuration value specifically typed as an integer.
*   **Parameters**: Accepts the key of the configuration setting.
*   **Return Value**: Returns the value as an `int`.
*   **Exceptions**: Throws if the key does not exist or if the stored value cannot be converted to an integer.

#### `public async Task<bool> GetConfigBoolAsync`
Retrieves a configuration value specifically typed as a boolean.
*   **Parameters**: Accepts the key of the configuration setting.
*   **Return Value**: Returns the value as a `bool`.
*   **Exceptions**: Throws if the key does not exist or if the stored value cannot be converted to a boolean.

#### `public async Task<string> GetConfigStringAsync`
Retrieves a configuration value specifically typed as a string.
*   **Parameters**: Accepts the key of the configuration setting.
*   **Return Value**: Returns the value as a `string`.
*   **Exceptions**: Throws if the key does not exist.

#### `public async Task<TimeSpan> GetConfigTimeSpanAsync`
Retrieves a configuration value specifically typed as a `TimeSpan`.
*   **Parameters**: Accepts the key of the configuration setting.
*   **Return Value**: Returns the value as a `TimeSpan`.
*   **Exceptions**: Throws if the key does not exist or if the stored value cannot be parsed into a `TimeSpan`.

## Usage

### Retrieving and Updating a Service Configuration
The following example demonstrates fetching a specific service configuration, modifying a property, and persisting the changes.

```csharp
public async Task UpdateTimeoutSettingAsync(ConfigurationService configService, string serviceId)
{
    // Retrieve the specific configuration for the service
    var config = await configService.GetConfigurationAsync(serviceId);
    
    if (config == null)
    {
        // Handle missing configuration (e.g., create new)
        return;
    }

    // Modify the setting
    config.TimeoutSeconds = 120;

    // Persist the updated configuration
    var updatedConfig = await configService.SetConfigurationAsync(config);
    
    Console.WriteLine($"Configuration updated at {updatedConfig.LastModified}");
}
```

### Reading Typed Primitive Values
This example illustrates using the typed helper methods to retrieve specific settings without manual casting or parsing.

```csharp
public async Task InitializeFeatureFlagsAsync(ConfigurationService configService)
{
    try
    {
        // Retrieve typed values directly
        bool isEnabled = await configService.GetConfigBoolAsync("Feature.NewDashboard.Enabled");
        int maxRetries = await configService.GetConfigIntAsync("Feature.NewDashboard.MaxRetries");
        TimeSpan cacheDuration = await configService.GetConfigTimeSpanAsync("Feature.NewDashboard.CacheTtl");

        if (isEnabled)
        {
            Console.WriteLine($"Dashboard enabled with {maxRetries} retries and {cacheDuration.TotalMinutes}m cache.");
        }
    }
    catch (KeyNotFoundException)
    {
        // Handle cases where specific keys are missing
        Console.WriteLine("Required feature flags are missing.");
    }
    catch (InvalidCastException)
    {
        // Handle type mismatches in the stored data
        Console.WriteLine("Configuration type mismatch detected.");
    }
}
```

## Notes

*   **Null Handling**: The `GetConfigurationAsync` method explicitly returns `null` if a configuration is not found, whereas the typed getter methods (e.g., `GetConfigIntAsync`) throw an exception under the same condition. Callers must check for null on the former and wrap the latter in try-catch blocks if key existence is uncertain.
*   **Type Safety**: The typed retrieval methods (`GetConfigIntAsync`, `GetConfigBoolAsync`, etc.) will throw an exception if the underlying stored value exists but cannot be strictly converted to the requested type. Ensure data integrity before calling these helpers.
*   **Concurrency**: As an asynchronous service interacting with persistent storage, race conditions may occur if multiple instances attempt to `SetConfigurationAsync` on the same key simultaneously. Implement optimistic concurrency control patterns using version tokens if available in the `ServiceConfiguration` model.
*   **Thread Safety**: The async nature of the methods implies they are designed for non-blocking I/O. While the service instance itself should be thread-safe for concurrent read operations, write operations (`SetConfigurationAsync`, `DeleteConfigurationAsync`) should be coordinated externally if strict ordering is required.
