# RateLimitingMiddleware

A middleware component that enforces rate limiting using a token bucket algorithm to control request throughput for both anonymous and authenticated users. It tracks request counts and enforces per-minute limits, returning appropriate HTTP 429 responses when limits are exceeded.

## API

### `RateLimitingMiddleware`
The constructor for the rate limiting middleware. Initializes the middleware with configuration for anonymous and authenticated user request limits.

Parameters:
- `anonymousRequestsPerMinute`: Maximum allowed requests per minute for anonymous users.
- `authenticatedRequestsPerMinute`: Maximum allowed requests per minute for authenticated users.

### `InvokeAsync`
Invokes the middleware to process an HTTP request and apply rate limiting.

Parameters:
- `context`: The `HttpContext` for the current request.
- `next`: The delegate representing the next middleware in the pipeline.

Return value:
- A `Task` representing the asynchronous operation.

Throws:
- `ArgumentNullException`: If `context` or `next` is `null`.

### `TokenBucket`
A nested class representing the token bucket state for a specific user or anonymous session.

### `AllowRequest`
Determines whether a new request should be allowed based on the current token count in the bucket.

Return value:
- `true` if the request is allowed; otherwise, `false`.

### `GetRemainingTokens`
Retrieves the current number of remaining tokens in the bucket.

Return value:
- The number of remaining tokens.

### `GetRetryAfterSeconds`
Calculates the number of seconds until the next token refill when the bucket is empty.

Return value:
- The number of seconds to wait before retrying, or `0` if tokens are available.

### `AnonymousRequestsPerMinute`
Gets the configured maximum requests per minute for anonymous users.

### `AuthenticatedRequestsPerMinute`
Gets the configured maximum requests per minute for authenticated users.

## Usage

### Example 1: Basic Middleware Registration
