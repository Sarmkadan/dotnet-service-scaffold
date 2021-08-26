# ExternalApiClient

The `ExternalApiClient` provides a type-safe abstraction for performing HTTP operations, simplifying interaction with remote RESTful services. It manages common tasks such as request execution, JSON serialization/deserialization, and status code verification, allowing for cleaner service-level code.

## API

### ExternalApiClient()
Initializes a new instance of the `ExternalApiClient`.

### GetAsync<T>(string requestUri)
Performs an HTTP GET request to the specified URI and deserializes the JSON response into the requested type `T`.

*   **Parameters:** `string requestUri` (The endpoint URI).
*   **Returns:** A `Task<T?>` representing the deserialized response, or `null` if the response content is empty.
*   **Throws:** `HttpRequestException` for transport or HTTP errors; `JsonException` if deserialization fails.

### PostAsync<T>(string requestUri, object content)
Performs an HTTP POST request to the specified URI with the provided content, deserializing the JSON response into type `T`.

*   **Parameters:** `string requestUri` (The endpoint URI), `object content` (The payload to serialize).
*   **Returns:** A `Task<T?>` representing the deserialized response, or `null` if the response content is empty.
*   **Throws:** `HttpRequestException` for transport or HTTP errors; `JsonException` if deserialization fails.

### PutAsync<T>(string requestUri, object content)
Performs an HTTP PUT request to the specified URI with the provided content, deserializing the JSON response into type `T`.

*   **Parameters:** `string requestUri` (The endpoint URI), `object content` (The payload to serialize).
*   **Returns:** A `Task<T?>` representing the deserialized response, or `null` if the response content is empty.
*   **Throws:** `HttpRequestException` for transport or HTTP errors; `JsonException` if deserialization fails.

### DeleteAsync(string requestUri)
Performs an HTTP DELETE request to the specified URI.

*   **Parameters:** `string requestUri` (The endpoint URI).
*   **Returns:** A `Task<bool>` indicating whether the request was successful (true) or failed (false).
*   **Throws:** `HttpRequestException` for transport or HTTP errors.

## Usage

### Example 1: Fetching data with GET
```csharp
var client = new ExternalApiClient();
var user = await client.GetAsync<User>("/api/users/123");

if (user != null)
{
    Console.WriteLine($"User: {user.Name}");
}
```

### Example 2: Sending data with POST
```csharp
var client = new ExternalApiClient();
var newProduct = new { Name = "Gadget", Price = 29.99 };
var createdProduct = await client.PostAsync<Product>("/api/products", newProduct);

if (createdProduct != null)
{
    Console.WriteLine($"Created Product ID: {createdProduct.Id}");
}
```

## Notes

*   **Thread Safety:** The `ExternalApiClient` is designed to be thread-safe and is intended to be used with a long-lived or singleton `HttpClient` instance.
*   **Error Handling:** All methods (`GetAsync`, `PostAsync`, `PutAsync`) throw exceptions upon receiving non-success HTTP status codes (e.g., 4xx or 5xx). Consumers should implement appropriate `try-catch` blocks to handle network failures or API-level errors.
*   **Deserialization:** Ensure the requested type `T` is compatible with the JSON format returned by the external service. If the service returns a 204 No Content, the methods will return `null`.
