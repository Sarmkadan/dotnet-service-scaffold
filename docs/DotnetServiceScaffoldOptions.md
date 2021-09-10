# DotnetServiceScaffoldOptions
The `DotnetServiceScaffoldOptions` type provides a set of configuration options for customizing the behavior of a .NET service scaffold. These options allow developers to fine-tune various aspects of the service, including health checks, security, CORS, rate limiting, and logging.

## API
The following public members are available:
* `HealthCheckInterval`: Specifies the interval between health checks in seconds.
* `HealthCheckTimeout`: Specifies the timeout for health checks in seconds.
* `MaxConcurrentHealthChecks`: Specifies the maximum number of concurrent health checks allowed.
* `MaintenanceMode`: Enables or disables maintenance mode for the service.
* `AuditLogRetentionDays`: Specifies the number of days to retain audit logs.
* `HealthCheckResultRetentionDays`: Specifies the number of days to retain health check results.
* `MaxFailedLoginAttempts`: Specifies the maximum number of failed login attempts allowed before account lockout.
* `AccountLockoutDurationMinutes`: Specifies the duration of account lockout in minutes.
* `PasswordMinimumLength`: Specifies the minimum length required for passwords.
* `EnableCors`: Enables or disables CORS support for the service.
* `AllowedOrigins`: Specifies a list of allowed origins for CORS requests.
* `RateLimitPerMinute`: Specifies the rate limit for requests per minute.
* `MaxServiceRegistrations`: Specifies the maximum number of service registrations allowed.
* `MaxResponseSize`: Specifies the maximum size of responses in bytes.
* `EnableDetailedErrors`: Enables or disables detailed error messages for the service.
* `DefaultPageSize`: Specifies the default page size for pagination.
* `MaxPageSize`: Specifies the maximum page size allowed for pagination.
* `CacheDurationSeconds`: Specifies the duration of cache in seconds.
* `EnableRequestLogging`: Enables or disables request logging for the service.
* `MaxCollectionSize`: Specifies the maximum size of collections allowed.

## Usage
Here are two examples of using the `DotnetServiceScaffoldOptions` type:
```csharp
// Example 1: Configuring health checks and security
var options = new DotnetServiceScaffoldOptions
{
    HealthCheckInterval = 30,
    HealthCheckTimeout = 10,
    MaxConcurrentHealthChecks = 5,
    MaintenanceMode = false,
    MaxFailedLoginAttempts = 3,
    AccountLockoutDurationMinutes = 30,
    PasswordMinimumLength = 12
};

// Example 2: Configuring CORS and rate limiting
var options = new DotnetServiceScaffoldOptions
{
    EnableCors = true,
    AllowedOrigins = new List<string> { "https://example.com" },
    RateLimitPerMinute = 100,
    MaxServiceRegistrations = 10,
    MaxResponseSize = 1024 * 1024
};
```

## Notes
When using the `DotnetServiceScaffoldOptions` type, consider the following edge cases and thread-safety remarks:
* The `HealthCheckInterval` and `HealthCheckTimeout` values should be carefully chosen to avoid overwhelming the service with health checks.
* The `MaxConcurrentHealthChecks` value should be set based on the available resources and expected load on the service.
* The `MaintenanceMode` flag should be used with caution, as it can impact the availability of the service.
* The `MaxFailedLoginAttempts` and `AccountLockoutDurationMinutes` values should be set based on the security requirements of the service.
* The `EnableCors` and `AllowedOrigins` settings should be carefully configured to avoid security vulnerabilities.
* The `RateLimitPerMinute` value should be set based on the expected load and performance requirements of the service.
* The `MaxServiceRegistrations` and `MaxResponseSize` values should be set based on the expected usage and performance requirements of the service.
* The `EnableDetailedErrors` flag should be used with caution, as it can impact the security and performance of the service.
* The `DefaultPageSize` and `MaxPageSize` values should be set based on the expected usage and performance requirements of the service.
* The `CacheDurationSeconds` value should be set based on the expected usage and performance requirements of the service.
* The `EnableRequestLogging` flag should be used with caution, as it can impact the performance of the service.
* The `MaxCollectionSize` value should be set based on the expected usage and performance requirements of the service.
* The `DotnetServiceScaffoldOptions` type is not thread-safe, and its instances should not be shared across multiple threads.
