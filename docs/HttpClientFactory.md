# HttpClientFactory

The `HttpClientFactory` provides a centralized mechanism for creating and configuring `HttpClient` instances within the `dotnet-service-scaffold` project. It abstracts the complexities of client initialization, such as setting base URLs, managing authentication headers, and applying standardized request policies, ensuring consistent behavior across the service architecture.

## API

### `public HttpClientFactory()`
Initializes a new instance of the `HttpClientFactory`.

### `public HttpClient CreateClient()`
Creates a default `HttpClient` instance with standard headers.
*   **Returns:** A configured `HttpClient` instance.
*   **Throws:** `InvalidOperationException` if client initialization fails.

### `public HttpClient CreateAuthenticatedClient(string username, string password)`
Creates an `HttpClient` instance configured with Basic Authentication credentials.
*   **Parameters:**
    *   `username` (string): The username for authentication.
    *   `password` (string): The password for authentication.
*   **Returns:** An `HttpClient` instance with the `Authorization` header set.
*   **Throws:** `ArgumentNullException` if `username` or `password` is null.

### `public HttpClient CreateBearerClient(string token)`
Creates an `HttpClient` instance configured with a Bearer token for authorization.
*   **Parameters:**
    *   `token` (string): The JWT or access token.
*   **Returns:** An `HttpClient` instance with the `Authorization` header set to `Bearer <token>`.
*   **Throws:** `ArgumentNullException` if `token` is null or empty.

### `public HttpClient CreateClientWithBaseUrl(string baseUrl)`
Creates an `HttpClient` instance configured with a specific base URL.
*   **Parameters:**
    *   `baseUrl` (string): The base URL for the client.
*   **Returns:** An `HttpClient` instance with the `BaseAddress` property set.
*   **Throws:** `UriFormatException` if `baseUrl` is invalid.

## Usage

### Basic Usage
```csharp
var factory = new HttpClientFactory();
var client = factory.CreateClient();
var response = await client.GetAsync("/api/data");
```

### Authenticated Usage
```csharp
var factory = new HttpClientFactory();
var client = factory.CreateBearerClient("my-secure-token");
var response = await client.GetAsync("/api/protected-resource");
```

## Notes

- **Thread-Safety:** The `HttpClientFactory` is designed to be thread-safe when used in a singleton context, but individual `HttpClient` instances returned by the factory should not be mutated concurrently in ways that alter shared headers.
- **Resource Management:** While the factory manages the creation of clients, caller applications are responsible for disposing of the `HttpClient` instances or allowing the underlying infrastructure to manage their lifecycle efficiently to prevent socket exhaustion.
- **Base URLs:** When using `CreateClientWithBaseUrl`, ensure the provided URL is well-formed; an invalid URI will result in an exception during client instantiation.
