# StructuredLoggingOptions

`StructuredLoggingOptions` serves as the configuration schema for customizing structured logging behavior within the application. It allows developers to configure diagnostic enrichment, correlation ID propagation, and severity thresholds, ensuring consistent and actionable log telemetry across distributed services.

## API

### ApplicationName
Gets or sets the identifier for the application used in log metadata.

### EnrichWithMachineName
Gets or sets a value indicating whether to enrich log events with the local machine's hostname.

### EnrichWithEnvironment
Gets or sets a value indicating whether to enrich log events with the hosting environment name (e.g., "Development", "Production").

### EnableCorrelationId
Gets or sets a value indicating whether to enable tracking and propagation of correlation IDs across requests.

### CorrelationIdHeader
Gets or sets the name of the HTTP header used for capturing and propagating the correlation ID.

### EnrichWithRequestContext
Gets or sets a value indicating whether to enrich log events with request context, such as URL path, HTTP method, and user identification.

### MinimumLevel
Gets or sets the minimum severity level required for log events to be processed (e.g., "Information", "Warning", "Error").

## Usage

### Configuring via Options Pattern
Typically, these options are bound from the `appsettings.json` file during application startup.

```csharp
// Inside Program.cs or Startup.cs
public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<StructuredLoggingOptions>(
        configuration.GetSection("Logging:StructuredOptions"));
}
```

### Manual Configuration
These options can also be instantiated and configured programmatically for custom service initialization.

```csharp
var options = new StructuredLoggingOptions
{
    ApplicationName = "PaymentService",
    EnableCorrelationId = true,
    CorrelationIdHeader = "X-Service-Correlation-ID",
    MinimumLevel = "Information"
};
```

## Notes

- **Thread Safety:** `StructuredLoggingOptions` is designed as a POCO (Plain Old C# Object). Once the configuration has been bound or initialized during the application startup phase, it should be treated as read-only. Concurrent read access is thread-safe; however, modifying these properties during runtime is not recommended and may result in inconsistent logging behavior.
- **Validation:** This class does not perform internal validation. If an invalid `MinimumLevel` string is provided, the underlying logging provider may revert to its default behavior, often defaulting to "Information" or the global logging level.
- **Correlation ID Behavior:** If `EnableCorrelationId` is set to `true` but `CorrelationIdHeader` is left null or empty, the application may fail to extract the correlation ID from incoming HTTP requests or might fall back to a default header name depending on the downstream logging infrastructure implementation.
