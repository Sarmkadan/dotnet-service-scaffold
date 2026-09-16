# Resilience options and circuit-breaker state

`ResilienceOptions` configures retries, per-attempt timeouts, and the circuit breaker used by `ResilientHttpMessageHandler`. `CircuitBreakerState` tracks consecutive transient failures and decides whether an outbound attempt may proceed.

`AddIntegrationServices()` binds `ResilienceOptions` from the `Resilience` configuration section, validates it at application startup, and registers one thread-safe `CircuitBreakerState` singleton shared by the resilient HTTP clients.

## Configuration knobs

| Option | Default | Valid values | Effect |
| --- | ---: | --- | --- |
| `RetryCount` | `3` | `0` to `10` | Maximum retries after the initial attempt. `0` means one attempt with no retries. |
| `BaseDelayMilliseconds` | `200` | `1` to `60000` | Base for exponential retry backoff. Before retry `n`, the uncapped delay is `BaseDelayMilliseconds * 2^(n-1)`. |
| `MaxDelayMilliseconds` | `30000` | `1` to `300000`; must be at least `BaseDelayMilliseconds` | Caps the exponential delay before full jitter selects a value from zero up to that cap. |
| `PerAttemptTimeoutSeconds` | `10` | `1` to `300` | Timeout applied separately to every HTTP attempt. |
| `CircuitBreakerFailureThreshold` | `5` | `1` to `100` | Consecutive transient failures required to open the circuit. |
| `CircuitBreakerBreakDurationSeconds` | `30` | `1` to `3600` | Time an open circuit rejects attempts before an attempt can move it to half-open. |

Transient failures are HTTP 408, HTTP 429, all 5xx responses, network-level `HttpRequestException` instances, and per-attempt timeouts. Each failed attempt counts toward the threshold, including retries within the same request. A successful or non-transient HTTP response resets the consecutive-failure count.

Retry, delay, and timeout values are read from `IOptionsMonitor<ResilienceOptions>` at the start of each request. The failure threshold and break duration are captured when the singleton `CircuitBreakerState` is constructed, so changing those two values requires recreating the singleton (normally by restarting the application).

## Circuit-breaker states

| State | Attempts | Transition |
| --- | --- | --- |
| `Closed` | Allowed normally. | A success remains closed and clears the failure count. Reaching the configured consecutive-failure threshold moves the breaker to `Open`. |
| `Open` | Rejected by `TryEnter()` until the break duration elapses. The HTTP handler reports this as `BrokenCircuitException`. | The first `TryEnter()` after the break duration moves the breaker to `HalfOpen` and permits the attempt. |
| `HalfOpen` | Permitted to test whether the dependency has recovered. | `RecordSuccess()` closes the breaker and resets the count. `RecordFailure()` immediately reopens it and restarts the break duration. |

The state object synchronizes its phase and failure counter, so it can be shared across handlers. `Phase` exposes a thread-safe snapshot for diagnostics.

## Configuration example

Add a `Resilience` section to `appsettings.json`:

```json
{
  "Resilience": {
    "RetryCount": 2,
    "BaseDelayMilliseconds": 250,
    "MaxDelayMilliseconds": 5000,
    "PerAttemptTimeoutSeconds": 8,
    "CircuitBreakerFailureThreshold": 4,
    "CircuitBreakerBreakDurationSeconds": 20
  }
}
```

Register the integration services during startup; this performs the binding and startup validation:

```csharp
using DotnetServiceScaffold.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIntegrationServices();

var app = builder.Build();
app.Run();
```

For direct use or focused tests, construct and drive the state explicitly:

```csharp
using DotnetServiceScaffold.Infrastructure.Http;

var breaker = new CircuitBreakerState(
    failureThreshold: 2,
    breakDuration: TimeSpan.FromSeconds(30));

if (breaker.TryEnter())
{
    try
    {
        await CallDependencyAsync();
        breaker.RecordSuccess();
    }
    catch (HttpRequestException)
    {
        breaker.RecordFailure();
        throw;
    }
}

Console.WriteLine(breaker.Phase);
```

`CircuitBreakerState` rejects a `failureThreshold` below `1` and a zero or negative `breakDuration` with `ArgumentOutOfRangeException`.
