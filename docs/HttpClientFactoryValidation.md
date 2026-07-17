# HttpClientFactoryValidation

The `HttpClientFactoryValidation` class provides a centralized, static utility for validating configuration parameters required to instantiate `HttpClient` instances within the `dotnet-service-scaffold` framework. It exposes a consistent pattern of validation methods—returning error lists, boolean validity flags, or throwing exceptions upon failure—across various client creation scenarios, including basic, authenticated, bearer-token, and base-URL-specific configurations. This ensures that runtime errors related to invalid URIs or missing credentials are caught early in the initialization pipeline.

## API

### Validate
Validates general configuration parameters common to all client creation strategies.
*   **Returns**: `IReadOnlyList<string>` containing error messages if validation fails; an empty list if successful.
*   **Throws**: None.

### IsValid
Determines whether the general configuration parameters are valid without retrieving specific error messages.
*   **Returns**: `bool` indicating `true` if valid, `false` otherwise.
*   **Throws**: None.

### EnsureValid
Validates general configuration parameters and throws an exception if any errors are detected.
*   **Returns**: `void`.
*   **Throws**: Throws an exception (typically `InvalidOperationException` or `ArgumentException`) containing the aggregated validation errors if the configuration is invalid.

### ValidateCreateClient
Validates parameters specifically required for creating a standard `HttpClient`.
*   **Returns**: `IReadOnlyList<string>` containing error messages if validation fails; an empty list if successful.
*   **Throws**: None.

### IsValidCreateClient
Determines whether the parameters for creating a standard `HttpClient` are valid.
*   **Returns**: `bool` indicating `true` if valid, `false` otherwise.
*   **Throws**: None.

### EnsureValidCreateClient
Validates parameters for creating a standard `HttpClient` and throws an exception if invalid.
*   **Returns**: `void`.
*   **Throws**: Throws an exception if the parameters are invalid.

### ValidateCreateAuthenticatedClient
Validates parameters required for creating an `HttpClient` with generic authentication credentials.
*   **Returns**: `IReadOnlyList<string>` containing error messages if validation fails; an empty list if successful.
*   **Throws**: None.

### IsValidCreateAuthenticatedClient
Determines whether the parameters for creating an authenticated `HttpClient` are valid.
*   **Returns**: `bool` indicating `true` if valid, `false` otherwise.
*   **Throws**: None.

### EnsureValidCreateAuthenticatedClient
Validates parameters for creating an authenticated `HttpClient` and throws an exception if invalid.
*   **Returns**: `void`.
*   **Throws**: Throws an exception if the parameters are invalid.

### ValidateCreateBearerClient
Validates parameters specifically required for creating an `HttpClient` configured with a Bearer token.
*   **Returns**: `IReadOnlyList<string>` containing error messages if validation fails; an empty list if successful.
*   **Throws**: None.

### IsValidCreateBearerClient
Determines whether the parameters for creating a Bearer-token `HttpClient` are valid.
*   **Returns**: `bool` indicating `true` if valid, `false` otherwise.
*   **Throws**: None.

### EnsureValidCreateBearerClient
Validates parameters for creating a Bearer-token `HttpClient` and throws an exception if invalid.
*   **Returns**: `void`.
*   **Throws**: Throws an exception if the parameters are invalid.

### ValidateCreateClientWithBaseUrl
Validates parameters required for creating an `HttpClient` initialized with a specific base address.
*   **Returns**: `IReadOnlyList<string>` containing error messages if validation fails; an empty list if successful.
*   **Throws**: None.

### IsValidCreateClientWithBaseUrl
Determines whether the parameters for creating an `HttpClient` with a base URL are valid.
*   **Returns**: `bool` indicating `true` if valid, `false` otherwise.
*   **Throws**: None.

### EnsureValidCreateClientWithBaseUrl
Validates parameters for creating an `HttpClient` with a base URL and throws an exception if invalid.
*   **Returns**: `void`.
*   **Throws**: Throws an exception if the parameters are invalid.

## Usage

### Example 1: Pre-flight Validation with Error Reporting
This example demonstrates how to use the `Validate` methods to collect error messages before attempting to instantiate a client, allowing for graceful error handling or logging.

```csharp
using System;
using System.Linq;
using DotNetServiceScaffold; // Hypothetical namespace based on project name

public class ClientInitializer
{
    public void InitializeBearerClient(string? token, string? baseUrl)
    {
        var errors = HttpClientFactoryValidation.ValidateCreateBearerClient();
        
        if (errors.Any())
        {
            Console.WriteLine("Configuration invalid:");
            foreach (var error in errors)
            {
                Console.WriteLine($"- {error}");
            }
            return;
        }

        // Proceed to create the client using the factory
        // var client = factory.CreateBearerClient(token, baseUrl);
    }
}
```

### Example 2: Fail-Fast Initialization
This example utilizes the `EnsureValid` methods to enforce strict configuration requirements during application startup, causing the process to halt immediately if the environment is misconfigured.

```csharp
using System;
using DotNetServiceScaffold;

public class StartupService
{
    public void ConfigureAuthenticatedClient()
    {
        try
        {
            // Throws immediately if credentials or URI formats are invalid
            HttpClientFactoryValidation.EnsureValidCreateAuthenticatedClient();
            
            // Proceed with registration or instantiation
            // services.AddHttpClient("Authenticated", ...);
        }
        catch (Exception ex)
        {
            // Log critical configuration failure
            Console.Error.WriteLine($"Critical failure in HTTP client configuration: {ex.Message}");
            throw;
        }
    }
}
```

## Notes

*   **Thread Safety**: As all members of `HttpClientFactoryValidation` are static and appear to be stateless utility functions (accepting no instance state and returning new collections or booleans), the class is inherently thread-safe for concurrent calls.
*   **Return Value Mutability**: The validation methods returning `IReadOnlyList<string>` provide a snapshot of errors at the time of invocation. While the list interface is read-only, callers should treat the contents as immutable diagnostic data.
*   **Exception Behavior**: The `EnsureValid*` methods are designed for fail-fast scenarios. They aggregate all validation errors into a single exception message rather than throwing on the first encountered error, providing comprehensive diagnostic information in the exception payload.
*   **Validation Scope**: Each specific validation method (e.g., `ValidateCreateBearerClient` vs `ValidateCreateClientWithBaseUrl`) targets distinct parameter sets. Calling a specialized validator does not implicitly run the general `Validate` logic unless explicitly implemented internally; consumers should select the method matching their specific client creation intent.
