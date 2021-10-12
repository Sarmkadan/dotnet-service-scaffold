# ErrorHandlingMiddlewareTests

Unit tests for `ErrorHandlingMiddleware` that verify exception handling behavior and HTTP status code mapping in ASP.NET Core applications.

## API

### `ErrorHandlingMiddlewareTests`
The test class that contains unit tests for `ErrorHandlingMiddleware` exception handling scenarios.

### `InvokeAsync_ShouldCatchGenericExceptionAndReturn500`
Verifies that the middleware catches generic exceptions and returns HTTP 500 (Internal Server Error) with a generic error message.

- **Parameters**: None
- **Return value**: `Task` representing the asynchronous test execution
- **Throws**: Standard test framework exceptions on assertion failures

### `InvokeAsync_ShouldCatchServiceScaffoldExceptionAndReturn400`
Verifies that the middleware catches `ServiceScaffoldException` and returns HTTP 400 (Bad Request) with a generic error message.

- **Parameters**: None
- **Return value**: `Task` representing the asynchronous test execution
- **Throws**: Standard test framework exceptions on assertion failures

### `InvokeAsync_ShouldCatchArgumentNullExceptionAndReturn400`
Verifies that the middleware catches `ArgumentNullException` and returns HTTP 400 (Bad Request) with a generic error message.

- **Parameters**: None
- **Return value**: `Task` representing the asynchronous test execution
- **Throws**: Standard test framework exceptions on assertion failures

### `InvokeAsync_ShouldCatchArgumentExceptionAndReturn400`
Verifies that the middleware catches `ArgumentException` and returns HTTP 400 (Bad Request) with a generic error message.

- **Parameters**: None
- **Return value**: `Task` representing the asynchronous test execution
- **Throws**: Standard test framework exceptions on assertion failures

### `InvokeAsync_ShouldCatchInvalidOperationExceptionAndReturn409`
Verifies that the middleware catches `InvalidOperationException` and returns HTTP 409 (Conflict) with a generic error message.

- **Parameters**: None
- **Return value**: `Task` representing the asynchronous test execution
- **Throws**: Standard test framework exceptions on assertion failures

### `InvokeAsync_ShouldCatchKeyNotFoundExceptionAndReturn404`
Verifies that the middleware catches `KeyNotFoundException` and returns HTTP 404 (Not Found) with a generic error message.

- **Parameters**: None
- **Return value**: `Task` representing the asynchronous test execution
- **Throws**: Standard test framework exceptions on assertion failures

### `InvokeAsync_ShouldReturnGenericMessageInProduction`
Verifies that the middleware returns a generic error message when the application is in production environment.

- **Parameters**: None
- **Return value**: `Task` representing the asynchronous test execution
- **Throws**: Standard test framework exceptions on assertion failures

## Usage
