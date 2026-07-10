# ExternalApiClientExtensions

The `ExternalApiClientExtensions` class provides a set of extension methods for `HttpClient` designed to standardize and simplify HTTP communication with external services. These methods encapsulate resilient communication patterns, specifically incorporating automated retry mechanisms to handle transient network errors or service unavailability gracefully. By utilizing these extensions, consumers of external APIs within the `dotnet-service-scaffold` framework can reduce boilerplate code while ensuring increased reliability for HTTP-based interactions.

## API

### GetWithRetryAsync&lt;T&gt;
Executes a GET request to the specified URI with an integrated retry policy.
- **Parameters:**
  - `HttpClient client`: The `HttpClient` instance used to send the request.
  - `string requestUri`: The URI to send the request to.
  - `CancellationToken cancellationToken`: A cancellation token to cancel the operation.
- **Returns:** A task representing the asynchronous operation, containing the deserialized object of type `T` on success, or `null` if the request failed after retries.
- **Throws:** `HttpRequestException` if an error occurs during the request process that is not handled by the retry policy.

### PostWithRetryAsync&lt;T&gt;
Executes a POST request to the specified URI with an integrated retry policy.
- **Parameters:**
  - `HttpClient client`: The `HttpClient` instance used to send the request.
  - `string requestUri`: The URI to send the request to.
  - `object content`: The content to be serialized and sent in the request body.
  - `CancellationToken cancellationToken`: A cancellation token to cancel the operation.
- **Returns:** A task representing the asynchronous operation, containing the deserialized object of type `T` from the response, or `null` if the request failed after retries.
- **Throws:** `HttpRequestException` if an error occurs during the request process that is not handled by the retry policy.

### PutWithRetryAsync&lt;T&gt;
Executes a PUT request to the specified URI with an integrated retry policy.
- **Parameters:**
  - `HttpClient client`: The `HttpClient` instance used to send the request.
  - `string requestUri`: The URI to send the request to.
  - `object content`: The content to be serialized and sent in the request body.
  - `CancellationToken cancellationToken`: A cancellation token to cancel the operation.
- **Returns:** A task representing the asynchronous operation, containing the deserialized object of type `T` from the response, or `null` if the request failed after retries.
- **Throws:** `HttpRequestException` if an error occurs during the request process that is not handled by the retry policy.

### DeleteWithRetryAsync
Executes a DELETE request to the specified URI with an integrated retry policy.
- **Parameters:**
  - `HttpClient client`: The `HttpClient` instance used to send the request.
  - `string requestUri`: The URI to send the request to.
  - `CancellationToken cancellationToken`: A cancellation token to cancel the operation.
- **Returns:** A task representing the asynchronous operation, returning `true` if the request succeeded, otherwise `false`.
- **Throws:** `HttpRequestException` if an error occurs during the request process that is not handled by the retry policy.

## Usage

```csharp
// GET Example
var httpClient = new HttpClient();
var user = await httpClient.GetWithRetryAsync<UserDto>("https://api.example.com/users/123", cancellationToken);

if (user != null)
{
    Console.WriteLine($"Retrieved user: {user.Name}");
}
```

```csharp
// POST Example
var httpClient = new HttpClient();
var newProduct = new ProductDto { Name = "New Widget" };
var createdProduct = await httpClient.PostWithRetryAsync<ProductDto>("https://api.example.com/products", newProduct, cancellationToken);

if (createdProduct != null)
{
    Console.WriteLine($"Created product with ID: {createdProduct.Id}");
}
```

## Notes

- **Transient Failures:** These methods are intended to handle transient errors (e.g., HTTP 408 Request Timeout, 5xx server errors). Non-transient errors (e.g., 400 Bad Request, 401 Unauthorized) generally result in immediate failure rather than triggering a retry.
- **Thread Safety:** These extension methods rely on the underlying `HttpClient` instance, which is thread-safe. However, ensure that the `HttpClient` is managed correctly (e.g., using `IHttpClientFactory`) to avoid socket exhaustion.
- **Serialization:** These methods assume the existence of appropriate serialization configuration within the application for mapping JSON responses to the target type `T`.
- **Cancellation:** Always pass a valid `CancellationToken` to these methods to ensure that long-running operations can be canceled appropriately when a request context is aborted.
