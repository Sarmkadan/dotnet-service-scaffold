# RateLimitingMiddleware

`RateLimitingMiddleware` limits requests with an in-memory token bucket for each
client. It allows requests while a token is available and returns HTTP 429 when
the client's bucket is empty.

## Configuration

The middleware constructor receives a `RateLimitOptions` instance directly from
dependency injection. Register that instance as a singleton before adding the
middleware to the pipeline:

```csharp
using DotnetServiceScaffold.Presentation.Middleware;

builder.Services.AddSingleton(new RateLimitOptions
{
    AnonymousRequestsPerMinute = 60,
    AuthenticatedRequestsPerMinute = 300
});
```

`RateLimitOptions` has the following settings:

| Setting | Default | Purpose |
| --- | ---: | --- |
| `AnonymousRequestsPerMinute` | `60` | Initial bucket capacity for clients that are not authenticated. |
| `AuthenticatedRequestsPerMinute` | `300` | Initial bucket capacity for authenticated clients. |

Despite their names, these settings determine bucket capacity; they do not
change the refill rate. The implementation refills every bucket at one token per
second. Consequently, a new anonymous client can make a burst of 60 requests
and a new authenticated client can make a burst of 300 requests, after which
both regain capacity at one request per second.

The scaffold's `AddApiAuthentication()` extension already registers a singleton
`RateLimitOptions` with the default values. Register a custom instance instead
of calling that extension only when the application supplies authentication and
rate-limit registration separately.

## Client identification

The middleware chooses one bucket key per request:

1. If the request principal contains a non-empty
   `ClaimTypes.NameIdentifier`, the key is `user:<claim value>`.
2. Otherwise, the key is `ip:<remote IP address>`.
3. If no remote IP is available, the shared key is `ip:unknown`.

Authentication must therefore run before this middleware if authenticated
requests are expected to receive the authenticated capacity and a per-user
bucket. If it has not populated `HttpContext.User`, requests are grouped by IP
and use the anonymous capacity.

Requests whose path starts with `/health` bypass rate limiting entirely. This
includes `/health` and child paths such as `/health/ready`.

## Algorithm

Buckets are stored in a process-wide, thread-safe dictionary. On a client's
first request, the middleware creates a full bucket whose capacity is selected
from `RateLimitOptions` according to the current authentication state.

For each subsequent request:

1. The bucket adds `elapsed seconds * 1` tokens, capped at its capacity.
2. If at least one token is available, it consumes one token and calls the next
   middleware.
3. If fewer than one token is available, it rejects the request with HTTP 429.

Access to an individual bucket is locked, so token checks and updates for that
client are atomic. The buckets are static and are not expired or removed. They
are also local to one application process, so limits are not shared between
servers or replicas.

A bucket's capacity is fixed when that client key is first seen. Changing the
options at runtime, or changing whether the same key is treated as authenticated,
does not resize an existing bucket. Restarting the process clears all buckets.

## Responses

An allowed response receives these headers before the remaining pipeline runs:

| Header | Value |
| --- | --- |
| `X-RateLimit-Limit` | The limit selected for the current request. |
| `X-RateLimit-Remaining` | Whole tokens remaining after the request token is consumed. |

A rejected request does not call later middleware. It has status `429 Too Many
Requests`, content type `application/json`, and a `Retry-After` header containing
the number of whole seconds until the next token is expected. Its JSON body has
this shape:

```json
{
  "error": "Too Many Requests",
  "message": "Rate limit exceeded. Please try again later.",
  "retryAfter": 1
}
```

## Example usage

The following setup authenticates the request before applying the limiter, then
maps both a bypassed health endpoint and a limited API endpoint:

```csharp
using DotnetServiceScaffold.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
    .AddCookie();
builder.Services.AddAuthorization();
builder.Services.AddSingleton(new RateLimitOptions
{
    AnonymousRequestsPerMinute = 30,
    AuthenticatedRequestsPerMinute = 120
});

var app = builder.Build();

app.UseAuthentication();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/api/profile", (HttpContext context) =>
    Results.Ok(new { name = context.User.Identity?.Name }))
    .RequireAuthorization();

app.Run();
```

When using `AddApiAuthentication()`, its current `UseApplicationMiddleware()`
helper places `RateLimitingMiddleware` before authentication. Register the
middleware explicitly in the order shown above when per-user identification and
the authenticated capacity are required.
