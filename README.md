// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

# Service Scaffold

## Architecture

A service-registry / health-monitoring API: ASP.NET Core + EF Core (SQLite, WAL), Serilog, API-key authentication, in-process Prometheus-format metrics. Single project with layered folders (`src/Domain`, `src/Application`, `src/Infrastructure`, `src/Presentation`, `src/Shared`), plus xUnit tests and BenchmarkDotNet benchmarks. Entry point is `Program.cs`; several components (rate limiting, caching, service discovery, service mesh) are opt-in via extension methods rather than wired by default.

See [docs/architecture.md](docs/architecture.md) for the full breakdown: what is wired at startup, data flow, design decisions and trade-offs, extension points, and known limitations.

## Result

The `Result` type is a discriminated union pattern for handling success/failure scenarios in a functional style. It provides a type-safe way to represent operations that may fail without throwing exceptions, enabling better error handling through the type system. The pattern includes both non-generic (`Result`) and generic (`Result<T>`) variants, allowing you to carry successful values or error information throughout your application.

### Usage Examples

```csharp
// Create a successful result
var successResult = Result.Success();
Console.WriteLine(successResult.IsSuccess); // true

// Create a failed result with error message
var failureResult = Result.Failure("Invalid input data");
Console.WriteLine(failureResult.IsSuccess); // false
Console.WriteLine(failureResult.ErrorMessage); // "Invalid input data"

// Create a failed result with error code
var errorResult = Result.Failure("Database connection failed", "DB001");
Console.WriteLine(errorResult.ErrorCode); // "DB001"

// Work with generic Result<T>
Result<string> stringResult = Result.Success("Hello, World!");
if (stringResult.IsSuccess)
{
    Console.WriteLine(stringResult.Value); // "Hello, World!"
}

// Handle failure case
Result<int> numberResult = Result.Failure<int>("Invalid number format");
if (!numberResult.IsSuccess)
{
    Console.WriteLine(numberResult.ErrorMessage); // "Invalid number format"
}

// Use IfSuccess/IfFailure for side effects
Result.Success()
    .IfSuccess(() => Console.WriteLine("Operation completed successfully"))
    .IfFailure(error => Console.WriteLine($"Failed: {error}"));

// Chain operations using Map (non-generic)
var mappedResult = Result.Success()
    .Map(() => 42)
    .Map(value => value * 2);

// Chain operations using Map with generic Result<T>
Result<int> chainedResult = Result.Success(10)
    .Map(value => value + 5);
if (chainedResult.IsSuccess)
{
    Console.WriteLine(chainedResult.Value); // 15
}
```

## ServiceScaffoldException

The `ServiceScaffoldException` class serves as the base exception type for the service scaffold platform. It provides a consistent error handling mechanism with support for error codes, enabling structured error handling and reporting throughout the application. All service-specific exceptions in the platform inherit from this base class, allowing for centralized error handling and logging strategies.

### Usage Examples

```csharp
using System;
using DotnetServiceScaffold.Domain.Exceptions;

// Create a basic service scaffold exception
var exception = new ServiceScaffoldException("Service operation failed due to timeout");
Console.WriteLine(exception.Message); // "Service operation failed due to timeout"
Console.WriteLine(exception.ErrorCode); // null (no error code provided)

// Create an exception with an error code
var errorCodeException = new ServiceScaffoldException(
    "Database connection failed", 
    "DB_CONNECTION_FAILED"
);
Console.WriteLine(errorCodeException.ErrorCode); // "DB_CONNECTION_FAILED"

// Create an exception with inner exception
try
{
    // Some operation that might fail
    throw new InvalidOperationException("Invalid state");
}
catch (Exception ex)
{
    var wrappedException = new ServiceScaffoldException(
        "Service failed to process request", 
        "SERVICE_PROCESSING_ERROR",
        ex
    );
    Console.WriteLine(wrappedException.InnerException?.Message); // "Invalid state"
}

// Use with specific exception types
var notFoundException = new ServiceNotFoundException(Guid.NewGuid());
Console.WriteLine(notFoundException.Message); // "Service with ID [guid] not found"
Console.WriteLine(notFoundException.ErrorCode); // "SERVICE_NOT_FOUND"

var validationException = new ServiceValidationException("Invalid configuration value");
Console.WriteLine(validationException.Message); // "Invalid configuration value"
Console.WriteLine(validationException.ErrorCode); // "VALIDATION_ERROR"
Console.WriteLine(validationException.Errors.Count); // 1
```

## ResultExtensions

The `ResultExtensions` class provides utility methods for working with `Result` and `Result<T>` types, enabling operation chaining, result aggregation, and error handling. These extensions simplify common patterns like transforming successful results, combining multiple results, extracting values safely, and validating conditions.

### Usage Examples

```csharp
// Chain synchronous operations on successful results
var result = Result.Success()
    .Then<int>(_ => 42)
    .Then(value => value * 2);

// Chain asynchronous operations on successful results
var asyncResult = Result.Success()
    .ThenAsync(async _ => 
    {
        await Task.Delay(10);
        return "processed";
    });

// Convert non-generic Result to generic Result<T>
var genericResult = Result.Success().ToGeneric<string>();

// Combine multiple results into a single aggregated result
var combined = Result.Combine(
    Result.Success(),
    Result.Failure("Error 1"),
    Result.Failure("Error 2")
);

// Add validation to a successful result
var validated = Result.Success(25)
    .Also(value => 
    {
        if (value <= 0) 
            return Result.Failure("Value must be positive");
        return Result.Success();
    });

// Extract value or use a default on failure
var valueOrDefault = Result.Failure<int>("Invalid").GetValueOrDefault(0);

// Extract value or throw on failure
try 
{
    var value = Result.Success(42).GetValueOrThrow();
}
catch (Exception ex) 
{
    // Handle exception
}

// Get error details from a failed result
var (errorMessage, errorCode) = Result.Failure("Invalid", "ERR001").GetError();

// Create result based on a condition
var conditionResult = Result.FromCondition(
    42 > 20, 
    "Value must be greater than 20", 
    "VAL001"
);
```

These extensions provide a fluent API for handling success/failure scenarios while maintaining strong type safety and avoiding boilerplate error-checking code.

## ServiceConfigurationExtensions

The `ServiceConfigurationExtensions` class provides helper methods for retrieving and updating service configuration values with type safety and validation. It includes methods for common data types like `double`, `decimal`, `DateTime`, and `Guid`, as well as utilities for checking system configuration flags and updating values conditionally.

### Usage Example

```csharp
var config = GetServiceConfiguration(); // Assume this retrieves a ServiceConfiguration instance

// Retrieve a string value with a default
var apiKey = config.GetValueOrDefault("API_KEY", "default123");

// Check if this is a system-level configuration
if (config.IsSystemConfiguration())
{
    // Safely retrieve an enum value
    var mode = config.GetEnumValue<EnvironmentMode>("ENV_MODE");
    
    // Update a numeric value only if it has changed
    config.UpdateValueIfChanged("MAX_RETRIES", 5);
}
else
{
    // Retrieve a Guid value
    var serviceId = config.GetGuidValue("SERVICE_ID");
    
    // Get a decimal value for a timeout setting
    var timeout = config.GetDecimalValue("REQUEST_TIMEOUT");
}
```

This example demonstrates retrieving configuration values of different types, checking system configuration status, and conditionally updating values while ensuring type safety.

## HealthCheckRepositoryIntegrationTestsExtensions

`HealthCheckRepositoryIntegrationTestsExtensions` supplies a set of helper methods that make integration testing of the health‑check repository straightforward. The methods allow you to create single or multiple `HealthCheckResult` entries, retrieve them, count results for a specific service, and assert that a result matches expected values, all with async support.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Assume `repository` is an instance of the health‑check repository used in tests.
var repository = GetHealthCheckRepository(); // placeholder for test setup
var serviceId = Guid.NewGuid();

// Create and add a single health‑check result
var singleResult = await HealthCheckRepositoryIntegrationTestsExtensions
    .CreateAndAddHealthCheckResultAsync(repository, serviceId);

// Create several health‑check results at once
var multipleResults = await HealthCheckRepositoryIntegrationTestsExtensions
    .CreateMultipleHealthCheckResultsAsync(repository, serviceId, count: 3);

// Verify that the created result matches the expected service identifier
HealthCheckRepositoryIntegrationTestsExtensions.AssertHealthCheckResultMatches(singleResult, serviceId);

// Retrieve all health‑check results from the repository
List<HealthCheckResult> allResults = await HealthCheckRepositoryIntegrationTestsExtensions
    .GetAllHealthCheckResultsAsync(repository);

// Count how many results exist for a particular service
int resultCount = await HealthCheckRepositoryIntegrationTestsExtensions
    .CountHealthCheckResultsForServiceAsync(repository, serviceId);
```

These extension methods streamline the arrangement, execution, and verification phases of integration tests that involve health‑check data.

## DatabaseBenchmarks

The `DatabaseBenchmarks` class provides performance benchmarks for database operations. It measures the execution time of CRUD operations and query performance for common database interactions.

### Usage Example

```csharp
var benchmarks = new DatabaseBenchmarks();

await benchmarks.Setup();

// Run benchmarks
await benchmarks.CreateUser();
await benchmarks.ReadUserByEmail();
await benchmarks.UpdateUser();
await benchmarks.DeleteUser();
await benchmarks.CreateService();
await benchmarks.ListServices();
await benchmarks.BulkCreateUsers();
await benchmarks.TransactionCommit();

await benchmarks.Cleanup();
```

This example demonstrates how to use the `DatabaseBenchmarks` class to measure the performance of database operations.

## MetricsBenchmarks

The `MetricsBenchmarks` class provides performance benchmarks for in-process metric collection. It measures the overhead of incrementing counters, recording timings, and retrieving metrics.

### Usage Example

```csharp
var metrics = new MetricsService(NullLogger<MetricsService>.Instance);

// Pre-populate some counters
for (int i = 0; i < 50; i++)
{
    metrics.IncrementCounter("requests.total");
    metrics.RecordTiming("request.duration_ms", 10 + i % 200);
    metrics.RecordGauge("memory.mb", 128 + i * 0.5);
}

metrics.IncrementCounterNoTags();
metrics.IncrementCounterOneTag();
metrics.IncrementCounterThreeTags();
metrics.RecordTimingNoTags();
metrics.RecordTimingThreeTags();
metrics.RecordGauge();
var metricsSnapshot = metrics.GetMetricsAsync().Result;
```

This example demonstrates how to use the `MetricsBenchmarks` class to measure the performance of metric collection.

## PerformanceUtility

The `PerformanceUtility` class provides performance monitoring and measurement utilities for tracking execution time, memory usage, CPU utilization, and garbage collection statistics. It includes methods for measuring synchronous and asynchronous operations, retrieving system resource usage, and formatting performance data for logging and monitoring purposes.

## StringUtility

The `StringUtility` class provides utility methods for common string operations including truncation, URL slug generation, case conversion, sensitive data masking, validation, and text manipulation. These methods are optimized for performance with minimal allocations and include support for null safety throughout.

### Usage Examples

```csharp
using System;
using DotnetServiceScaffold.Shared.Utilities;

// Truncate long text for display (keeps last 4 characters visible)
string longText = "This is a very long text that needs to be truncated for display purposes";
string truncated = StringUtility.Truncate(longText, 30);
Console.WriteLine(truncated); // "This is a very long text..."

// Convert text to URL-friendly slug
string title = "My Awesome Blog Post Title!";
string slug = StringUtility.ToSlug(title);
Console.WriteLine(slug); // "my-awesome-blog-post-title"

// Convert PascalCase to snake_case
string pascalCase = "UserAccountServiceManager";
string snakeCase = StringUtility.ToSnakeCase(pascalCase);
Console.WriteLine(snakeCase); // "user_account_service_manager"

// Convert snake_case to camelCase
string snakeCaseText = "user_account_service_manager";
string camelCase = StringUtility.ToCamelCase(snakeCaseText);
Console.WriteLine(camelCase); // "userAccountServiceManager"

// Mask sensitive information (keeps first and last 4 characters visible)
string apiKey = "sk_live_1234567890abcdef";
string maskedKey = StringUtility.MaskSensitive(apiKey);
Console.WriteLine(maskedKey); // "sk_l***ef"

// Generate a random string for tokens or session IDs
string randomToken = StringUtility.GenerateRandomString(32);
Console.WriteLine(randomToken); // "XyZ9aBcD3eFgH4iJ5kL6mN7oP8qR"

// Validate email format
bool isValidEmail = StringUtility.IsValidEmail("user@example.com");
Console.WriteLine(isValidEmail); // true

bool isInvalidEmail = StringUtility.IsValidEmail("invalid-email");
Console.WriteLine(isInvalidEmail); // false

// Strip HTML tags from user-provided content
string htmlContent = "<p>Hello <strong>World</strong>!</p>";
string plainText = StringUtility.StripHtmlTags(htmlContent);
Console.WriteLine(plainText); // "Hello World!"

// Repeat a string multiple times
string repeated = StringUtility.Repeat("ha", 3);
Console.WriteLine(repeated); // "hahaha"
```

## ValidationUtility

The `ValidationUtility` class provides utility methods for validating input data including strings, collections, and common formats like URLs, email addresses, phone numbers, passwords, GUIDs, IP addresses, and JSON. The validation methods throw `ArgumentException` or `ArgumentNullException` on validation failures, making them suitable for parameter validation in public APIs and service methods.

### Usage Examples

```csharp
using System;
using System.Collections.Generic;
using DotnetServiceScaffold.Shared.Utilities;

// Validate that a string is not null or empty
ValidationUtility.ValidateNotNullOrEmpty("username", nameof(username));

// Validate that a value is within a specified range
ValidationUtility.ValidateRange(42, 0, 100, nameof(age));

// Validate that a string length is within specified bounds
ValidationUtility.ValidateLength("password123", 8, 64, nameof(password));

// Validate a password meets minimum security requirements
bool isStrongPassword = ValidationUtility.IsPasswordStrong("SecureP@ssw0rd");
Console.WriteLine(isStrongPassword); // true

bool isWeakPassword = ValidationUtility.IsPasswordStrong("weak");
Console.WriteLine(isWeakPassword); // false

// Validate a URL is properly formatted
bool isValidUrl = ValidationUtility.IsValidUrl("https://example.com/api/users");
Console.WriteLine(isValidUrl); // true

bool isInvalidUrl = ValidationUtility.IsValidUrl("not-a-url");
Console.WriteLine(isInvalidUrl); // false

// Validate a phone number
bool isValidPhone = ValidationUtility.IsValidPhoneNumber("+1-555-123-4567");
Console.WriteLine(isValidPhone); // true

// Validate an email address
bool isValidEmail = ValidationUtility.IsValidEmail("user@example.com");
Console.WriteLine(isValidEmail); // true

bool isInvalidEmail = ValidationUtility.IsValidEmail("invalid-email");
Console.WriteLine(isInvalidEmail); // false

// Validate a UUID/GUID string
bool isValidGuid = ValidationUtility.IsValidGuid("550e8400-e29b-41d4-a716-446655440000");
Console.WriteLine(isValidGuid); // true

// Validate an IP address (IPv4 or IPv6)
bool isValidIp = ValidationUtility.IsValidIpAddress("192.168.1.1");
Console.WriteLine(isValidIp); // true

bool isValidIpv6 = ValidationUtility.IsValidIpAddress("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
Console.WriteLine(isValidIpv6); // true

// Validate a JSON string can be parsed
bool isValidJson = ValidationUtility.IsValidJson("{\"name\":\"John\",\"age\":30}");
Console.WriteLine(isValidJson); // true

// Validate a collection is not null or empty
ValidationUtility.ValidateCollectionNotEmpty(new List<string> { "item1", "item2" }, nameof(items));

// Validate that a value matches a regex pattern
bool matchesPattern = ValidationUtility.MatchesPattern("ABC-123", "^[A-Z]{3}-\\d{3}$");
Console.WriteLine(matchesPattern); // true
```

## CollectionUtility

The `CollectionUtility` class provides utility methods for common collection operations including batching, chunking, grouping, filtering, and manipulation. These methods help simplify working with collections by providing functional-style operations that maintain immutability and support LINQ-style chaining.

### Usage Examples

```csharp
using System;
using System.Collections.Generic;
using DotnetServiceScaffold.Shared.Utilities;

// Batch a large collection into smaller chunks for processing
var numbers = Enumerable.Range(1, 100);
var batches = numbers.Batch(10);
foreach (var batch in batches)
{
    Console.WriteLine(string.Join(", ", batch));
}

// Chunk a collection into groups of specified size
var fruits = new List<string> { "apple", "banana", "orange", "grape", "kiwi", "melon" };
var chunks = fruits.Chunk(2);
foreach (var chunk in chunks)
{
    Console.WriteLine($"Chunk: {string.Join(", ", chunk)}");
}

// Check if two collections contain the same elements (order-independent)
var list1 = new List<int> { 1, 2, 3, 4, 5 };
var list2 = new List<int> { 5, 4, 3, 2, 1 };
bool sameElements = list1.ContainsSameElements(list2);
Console.WriteLine(sameElements); // true

// Get common elements between two collections
var setA = new HashSet<int> { 1, 2, 3, 4, 5 };
var setB = new HashSet<int> { 4, 5, 6, 7, 8 };
var common = setA.GetCommon(setB);
Console.WriteLine(string.Join(", ", common)); // "4, 5"

// Get difference between two collections
var allUsers = new List<string> { "alice", "bob", "charlie", "david" };
var activeUsers = new List<string> { "alice", "charlie" };
var inactive = allUsers.GetDifference(activeUsers);
Console.WriteLine(string.Join(", ", inactive)); // "bob, david"

// Flatten a nested collection
var nested = new List<List<int>>
{
    new List<int> { 1, 2, 3 },
    new List<int> { 4, 5 },
    new List<int> { 6, 7, 8, 9 }
};
var flat = nested.Flatten();
Console.WriteLine(string.Join(", ", flat)); // "1, 2, 3, 4, 5, 6, 7, 8, 9"

// Shuffle a collection randomly
var deck = new List<string> { "Ace", "King", "Queen", "Jack", "10", "9" };
var shuffled = deck.Shuffle();
Console.WriteLine(string.Join(", ", shuffled));

// Remove duplicates while preserving order
var withDuplicates = new List<int> { 1, 2, 3, 2, 4, 1, 5, 3 };
var unique = withDuplicates.DistinctPreservingOrder();
Console.WriteLine(string.Join(", ", unique)); // "1, 2, 3, 4, 5"

// Group a collection by a key into a dictionary
var people = new List<(string Name, string Department)>
{
    ("Alice", "Engineering"),
    ("Bob", "Marketing"),
    ("Charlie", "Engineering"),
    ("David", "HR"),
    ("Eve", "Marketing")
};
var grouped = people.GroupByToDictionary(p => p.Department);
foreach (var kvp in grouped)
{
    Console.WriteLine($"{kvp.Key}: {string.Join(", ", kvp.Value.Select(p => p.Name))}");
}

// Split a collection based on a predicate
var numbers2 = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
var (even, odd) = numbers2.Partition(n => n % 2 == 0);
Console.WriteLine($"Even: {string.Join(", ", even)}"); // "2, 4, 6, 8, 10"
Console.WriteLine($"Odd: {string.Join(", ", odd)}"); // "1, 3, 5, 7, 9"

// Check if a collection is null or empty
List<string>? nullList = null;
bool isNullOrEmpty = nullList.IsNullOrEmpty();
Console.WriteLine(isNullOrEmpty); // true

var emptyList = new List<string>();
isNullOrEmpty = emptyList.IsNullOrEmpty();
Console.WriteLine(isNullOrEmpty); // true

// Check if a collection has items
var populatedList = new List<string> { "item1", "item2" };
bool hasItems = populatedList.HasItems();
Console.WriteLine(hasItems); // true

// Execute an action on each item
var colors = new List<string> { "red", "green", "blue" };
colors.ForEach(color => Console.WriteLine($"Color: {color}"));

// Execute an action on each item with index
var letters = new List<char> { 'a', 'b', 'c', 'd' };
letters.ForEach((letter, index) => Console.WriteLine($"Index {index}: {letter}"));
```

## ReflectionUtility

The `ReflectionUtility` class provides reflection-based utilities for type inspection, property access, method invocation, and attribute discovery. It simplifies common reflection patterns with strongly-typed methods that handle null safety, case-insensitive lookups, and error conditions gracefully. The utility is useful for dynamic configuration loading, plugin architectures, serialization frameworks, and testing utilities where runtime type inspection is required.

### Usage Examples

```csharp
using System;
using System.Linq;
using System.Reflection;
using DotnetServiceScaffold.Shared.Utilities;

// Define a sample class with attributes for demonstration
public class SampleEntity
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime CreatedAt { get; set; }
    
    [Obsolete("Use newer method instead")]
    public void OldMethod() { }
    
    public void NewMethod() { }
}

// Get all public properties of a type
PropertyInfo[] properties = ReflectionUtility.GetPublicProperties(typeof(SampleEntity));
Console.WriteLine($"Found {properties.Length} properties");
// Output: Found 3 properties

// Get a property value
var entity = new SampleEntity { Name = "John", Age = 30, CreatedAt = DateTime.UtcNow };
string name = (string)ReflectionUtility.GetPropertyValue(entity, "Name")!;
Console.WriteLine(name); // Output: John

// Set a property value
bool setSuccess = ReflectionUtility.SetPropertyValue(entity, "Age", 31);
Console.WriteLine(setSuccess); // Output: True
Console.WriteLine(entity.Age); // Output: 31

// Work with attributes
bool hasObsolete = ReflectionUtility.HasAttribute<ObsoleteAttribute>(
    typeof(SampleEntity).GetMethod("OldMethod")!);
Console.WriteLine(hasObsolete); // Output: True

// Get all types in an assembly that inherit from a base class
var types = ReflectionUtility.GetTypesByBaseClass(
    Assembly.GetExecutingAssembly(),
    typeof(SampleEntity)
);
Console.WriteLine($"Found {types.Count()} types");

// Get all types that implement an interface
var interfaceTypes = ReflectionUtility.GetTypesByInterface(
    Assembly.GetExecutingAssembly(),
    typeof(IEquatable<>)
);

// Get all public methods
MethodInfo[] methods = ReflectionUtility.GetPublicMethods(typeof(SampleEntity));
Console.WriteLine($"Found {methods.Length} methods");

// Get a specific method
MethodInfo? method = ReflectionUtility.GetMethod(typeof(SampleEntity), "NewMethod");
Console.WriteLine(method?.Name); // Output: NewMethod

// Invoke a method
object? result = ReflectionUtility.InvokeMethod(entity, "NewMethod");
Console.WriteLine(result); // Output: (null - void method)

// Create an instance
object? instance = ReflectionUtility.CreateInstance(typeof(SampleEntity));
Console.WriteLine(instance?.GetType().Name); // Output: SampleEntity

// Work with nullable types
bool isNullable = ReflectionUtility.IsNullableType(typeof(int?));
Console.WriteLine(isNullable); // Output: True

Type? underlyingType = ReflectionUtility.GetUnderlyingType(typeof(int?));
Console.WriteLine(underlyingType?.Name); // Output: Int32

// Check if a type is a collection
bool isList = ReflectionUtility.IsCollectionType(typeof(System.Collections.Generic.List<string>));
Console.WriteLine(isList); // Output: True

bool isString = ReflectionUtility.IsCollectionType(typeof(string));
Console.WriteLine(isString); // Output: False

// Get collection element type
Type? elementType = ReflectionUtility.GetCollectionElementType(typeof(int[]));
Console.WriteLine(elementType?.Name); // Output: Int32

// Convert values between types
object? converted = ReflectionUtility.ConvertValue("42", typeof(int));
Console.WriteLine(converted); // Output: 42

// Get properties with a specific attribute
PropertyInfo[] obsoleteProps = ReflectionUtility.GetPropertiesWithAttribute<ObsoleteAttribute>(
    typeof(SampleEntity)
).ToArray();
Console.WriteLine($"Found {obsoleteProps.Length} properties with Obsolete attribute");
```

## DateTimeUtility

The `DateTimeUtility` class provides utility methods for common date/time operations including age calculation, relative time formatting, business hours checking, and various datetime boundary calculations. All methods work with UTC dates for consistency and provide flexible reference date parameters for testing scenarios.

### Usage Examples

```csharp
using System;
using DotnetServiceScaffold.Shared.Utilities;

// Calculate age from birth date
var birthDate = new DateTime(1990, 5, 15);
int age = DateTimeUtility.CalculateAge(birthDate);
Console.WriteLine($"Age: {age} years"); // Age: 36 years

// Calculate age as of a specific date (for testing)
var referenceDate = new DateTime(2026, 7, 16);
int ageAsOfDate = DateTimeUtility.CalculateAge(birthDate, referenceDate);
Console.WriteLine($"Age as of {referenceDate:yyyy-MM-dd}: {ageAsOfDate} years");

// Get relative time formatting
var pastDate = DateTime.UtcNow.AddHours(-2);
string? relativePast = DateTimeUtility.GetRelativeTime(pastDate);
Console.WriteLine(relativePast); // "2 hours ago"

var futureDate = DateTime.UtcNow.AddDays(3);
string? relativeFuture = DateTimeUtility.GetRelativeTime(futureDate);
Console.WriteLine(relativeFuture); // "3 days from now"

// Check if current time is within business hours (Monday-Friday, 9am-5pm UTC)
bool isBusinessHours = DateTimeUtility.IsBusinessHours(DateTime.UtcNow);
Console.WriteLine($"Is business hours: {isBusinessHours}");

// Get start/end of day boundaries
var now = DateTime.UtcNow;
DateTime startOfDay = DateTimeUtility.GetStartOfDay(now);
DateTime endOfDay = DateTimeUtility.GetEndOfDay(now);
Console.WriteLine($"Start of day: {startOfDay}");
Console.WriteLine($"End of day: {endOfDay}");

// Get start of week (Monday)
DateTime startOfWeek = DateTimeUtility.GetStartOfWeek(now);
Console.WriteLine($"Start of week: {startOfWeek:yyyy-MM-dd}");

// Get start of month
DateTime startOfMonth = DateTimeUtility.GetStartOfMonth(now);
Console.WriteLine($"Start of month: {startOfMonth:yyyy-MM-dd}");

// Check if a date is in the past, future, or today
var yesterday = DateTime.UtcNow.AddDays(-1);
var tomorrow = DateTime.UtcNow.AddDays(1);

Console.WriteLine($"Yesterday is past: {DateTimeUtility.IsPast(yesterday)}"); // true
Console.WriteLine($"Tomorrow is future: {DateTimeUtility.IsFuture(tomorrow)}"); // true
Console.WriteLine($"Today is today: {DateTimeUtility.IsToday(now)}"); // true

// Parse ISO 8601 duration string
TimeSpan duration = DateTimeUtility.ParseIsoDuration("P3DT4H5M6S");
Console.WriteLine($"Duration: {duration}"); // 3.04:05:06
```

## EncryptionUtility

The `EncryptionUtility` class provides cryptographic operations for secure password handling, data encryption, and message authentication. It includes methods for hashing passwords with PBKDF2, AES-256-GCM encryption/decryption, generating secure random tokens, and computing HMAC-SHA256 signatures. All cryptographic operations use .NET's built-in security libraries with proper key sizes and authenticated encryption modes.

### Usage Examples

```csharp
using System;
using System.Text;
using DotnetServiceScaffold.Shared.Utilities;

// Hash a password for secure storage
string password = "MySecurePassword123!";
string hashedPassword = EncryptionUtility.HashPassword(password);
Console.WriteLine($"Hashed password: {hashedPassword}");

// Verify a password against stored hash
bool isValid = EncryptionUtility.VerifyPassword(password, hashedPassword);
Console.WriteLine($"Password verification: {(isValid ? "Valid" : "Invalid")}");

// Generate a secure AES-256 key (32 bytes)
byte[] aesKey = EncryptionUtility.GenerateRandomBytes(32);

// Encrypt sensitive data
string sensitiveData = "Sensitive user information";
string encryptedData = EncryptionUtility.EncryptAes(sensitiveData, aesKey);
Console.WriteLine($"Encrypted data: {encryptedData}");

// Decrypt the data
string decryptedData = EncryptionUtility.DecryptAes(encryptedData, aesKey);
Console.WriteLine($"Decrypted data: {decryptedData}");

// Generate a secure random token for session management
string secureToken = EncryptionUtility.GenerateSecureToken(32);
Console.WriteLine($"Secure token: {secureToken}");

// Compute HMAC-SHA256 signature for API request signing
string apiKey = "your-secret-api-key";
string requestBody = "{\"userId\": 123}";
string signature = EncryptionUtility.ComputeHmacSha256(requestBody, apiKey);
Console.WriteLine($"HMAC signature: {signature}");

// Compute SHA256 hash for checksums
string fileContent = "file content to hash";
string fileHash = EncryptionUtility.ComputeSha256(fileContent);
Console.WriteLine($"File hash: {fileHash}");
```

### Usage Examples

```csharp
using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Shared.Utilities;

// Measure synchronous operation execution time
long syncElapsedMs = PerformanceUtility.MeasureMs(() =>
{
    // Simulate work
    for (int i = 0; i < 1000000; i++)
    {
        var x = i * 2;
    }
});
Console.WriteLine($"Synchronous operation took {syncElapsedMs}ms");

// Measure synchronous operation with result and execution time
var (result, elapsedMs) = PerformanceUtility.MeasureMs(() =>
{
    // Simulate work that returns a value
    Task.Delay(50).Wait();
    return "Operation completed";
});
Console.WriteLine($"Result: {result}, Time: {elapsedMs}ms");

// Measure asynchronous operation execution time
long asyncElapsedMs = await PerformanceUtility.MeasureMsAsync(async () =>
{
    await Task.Delay(100);
});
Console.WriteLine($"Asynchronous operation took {asyncElapsedMs}ms");

// Measure asynchronous operation with result and execution time
var (asyncResult, asyncTimeMs) = await PerformanceUtility.MeasureMsAsync(async () =>
{
    await Task.Delay(75);
    return 42;
});
Console.WriteLine($"Async result: {asyncResult}, Time: {asyncTimeMs}ms");

// Get current memory usage in MB
double memoryUsageMb = PerformanceUtility.GetMemoryUsageMb();
Console.WriteLine($"Current memory usage: {memoryUsageMb:F2}MB");

// Get detailed memory statistics
var memoryStats = PerformanceUtility.GetMemoryStats();
Console.WriteLine($"Working set: {memoryStats.WorkingSetMb:F2}MB, " +
                 $"Private memory: {memoryStats.PrivateMemoryMb:F2}MB, " +
                 $"Peak working set: {memoryStats.PeakWorkingSetMb:F2}MB");

// Get CPU usage percentage
double cpuUsage = PerformanceUtility.GetCpuUsagePercent();
Console.WriteLine($"Current CPU usage: {cpuUsage:F1}%");

// Get garbage collection statistics
await Task.Delay(100); // Give GC a chance to run
var gcStats = PerformanceUtility.GetGcStats();
Console.WriteLine($"GC Collections - Gen0: {gcStats.Gen0Collections}, " +
                 $"Gen1: {gcStats.Gen1Collections}, " +
                 $"Gen2: {gcStats.Gen2Collections}");

// Format elapsed time for display
string formattedTime = PerformanceUtility.FormatElapsedTime(syncElapsedMs);
Console.WriteLine($"Formatted time: {formattedTime}");

// Format bytes for display
string formattedBytes = PerformanceUtility.FormatBytes(1024 * 1024);
Console.WriteLine($"Formatted bytes: {formattedBytes}");

// Retry with exponential backoff (useful for transient failures)
int attemptCount = 0;
var retryResult = await PerformanceUtility.RetryWithBackoffAsync(async () =>
{
    attemptCount++;
    if (attemptCount < 3)
    {
        throw new InvalidOperationException("Temporary failure");
    }
    return "Success after retries";
}, maxAttempts: 5, initialDelayMs: 100);
Console.WriteLine($"Retry result: {retryResult}, Attempts: {attemptCount}");

// Access instance properties for detailed monitoring
var perfInstance = new PerformanceUtility();
Console.WriteLine($"Working set: {perfInstance.WorkingSetMb:F2}MB");
Console.WriteLine($"Private memory: {perfInstance.PrivateMemoryMb:F2}MB");
Console.WriteLine($"Peak working set: {perfInstance.PeakWorkingSetMb:F2}MB");
Console.WriteLine($"Gen0 collections: {perfInstance.Gen0Collections}");
Console.WriteLine($"Gen1 collections: {perfInstance.Gen1Collections}");
Console.WriteLine($"Gen2 collections: {perfInstance.Gen2Collections}");
```

## CacheBenchmarks

`CacheBenchmarks` contains a set of BenchmarkDotNet benchmarks that also expose their public members for ad‑hoc usage. It demonstrates typical in‑memory cache operations such as reading, writing, checking existence, and the get‑or‑set pattern against an `InMemoryCacheService`.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Benchmarks;

public static async Task Main()
{
    var benchmarks = new CacheBenchmarks();

    // Initialise the in‑memory cache with some data
    await benchmarks.Setup();

    // Cache hit – should return the pre‑populated list
    var hitResult = await benchmarks.CacheHit();
    Console.WriteLine($"Cache hit returned {hitResult?.Services?.Count ?? 0} services.");

    // Cache miss – returns null
    var missResult = await benchmarks.CacheMiss();
    Console.WriteLine($"Cache miss returned {(missResult == null ? "null" : "data")}.");

    // Write a new entry
    await benchmarks.CacheSet();

    // Check if a key exists
    bool exists = await benchmarks.Exists();
    Console.WriteLine($"Key 'services:all' exists: {exists}");

    // GetOrSet hot path (cache hit, factory not invoked)
    var hotResult = await benchmarks.GetOrSetHit();

    // GetOrSet cold path (cache miss, factory invoked)
    var coldResult = await benchmarks.GetOrSetMiss();

    // Example of iterating over the cached services
    if (hitResult?.Services != null)
    {
        foreach (var svc in hitResult.Services)
        {
            Console.WriteLine($"{svc.Id} - {svc.Name} (Healthy: {svc.IsHealthy})");
        }
    }

    // Clean up resources
    benchmarks.Cleanup();
}
```

The example uses the real public members of `CacheBenchmarks` (`Setup`, `Cleanup`, `CacheHit`, `CacheMiss`, `CacheSet`, `Exists`, `GetOrSetHit`, `GetOrSetMiss`) and the `CachedService` properties (`Id`, `Name`, `IsHealthy`) to illustrate typical cache interactions.

## InMemoryCacheService

The `InMemoryCacheService` provides a lightweight, in-memory caching implementation using `ConcurrentDictionary` for single-node deployments or development environments. It supports automatic expiration of cached entries, pattern-based removal, and efficient synchronous operations on cache hits to minimize overhead. The service automatically cleans up expired entries on a configurable interval and provides methods for common cache operations including get, set, check existence, and bulk operations.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());

var serviceProvider = services.BuildServiceProvider();

// Resolve the cache service
var cacheService = serviceProvider.GetRequiredService<InMemoryCacheService>();

// Cache a value with 5-minute expiration
var userData = new { Id = 1, Name = "John Doe", Email = "john@example.com" };
await cacheService.SetAsync("user:1:profile", userData, TimeSpan.FromMinutes(5));

// Retrieve a cached value
var cachedUser = await cacheService.GetAsync<object>("user:1:profile");
if (cachedUser != null)
{
Console.WriteLine($"Retrieved user: {cachedUser.Name}");
}

// Check if a key exists
bool exists = await cacheService.ExistsAsync("user:1:profile");
Console.WriteLine($"Key exists: {exists}");

// Get value or set it using a factory (cache-aside pattern)
var serviceConfig = await cacheService.GetOrSetAsync(
"service:config",
async () => 
{
// Expensive operation - only called if cache misses
await Task.Delay(100);
return new { Timeout = 30, Retries = 3, Enabled = true };
},
TimeSpan.FromHours(1)
);

Console.WriteLine($"Service config: Timeout={serviceConfig?.Timeout}, Retries={serviceConfig?.Retries}");

// Remove a specific key
await cacheService.RemoveAsync("user:1:profile");

// Remove all keys matching a pattern (e.g., all user sessions)
await cacheService.RemoveByPatternAsync("user:*:sessions");

// Clear the entire cache
await cacheService.ClearAsync();

// Access cache metadata
var entry = cacheService.GetAsync<object>("some:key").Result;
if (entry != null)
{
Console.WriteLine($"Value created at: {entry.CreatedAt}");
Console.WriteLine($"Value expires at: {entry.ExpiresAt}");
}
```

## ConfigurationRepository

The `ConfigurationRepository` provides data access methods for application and service configurations, allowing for easy retrieval, existence checking, and management of configuration settings stored in the database. It leverages `ServiceScaffoldDbContext` to perform these operations and includes built-in logging for transparency and debugging.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddDbContext<ServiceScaffoldDbContext>(options =>
    options.UseSqlite("Data Source=service-scaffold.db"));
services.AddLogging(configure => configure.AddConsole());

var serviceProvider = services.BuildServiceProvider();

// Resolve the configuration repository
var configRepository = new ConfigurationRepository(
    serviceProvider.GetRequiredService<ServiceScaffoldDbContext>(),
    serviceProvider.GetRequiredService<ILogger<ConfigurationRepository>>());

var serviceId = Guid.NewGuid();

// Get a configuration by key and service ID
var config = await configRepository.GetByKeyAsync("FEATURE_FLAG_X", serviceId);

// Check if a configuration key exists
bool exists = await configRepository.KeyExistsAsync("API_TIMEOUT", serviceId);

// Get all configurations for a service
var configs = await configRepository.GetByServiceIdAsync(serviceId);

// Delete a configuration by key
await configRepository.DeleteByKeyAsync("API_TIMEOUT", serviceId);
```


## ServiceCollectionExtensions

The `ServiceCollectionExtensions` class provides extension methods for registering infrastructure and application services in the dependency injection container. It centralizes service configuration for better maintainability and consistency across the application, including application services, integration services, caching, background services, and API authentication.

## Repository

The `Repository<T>` class is a generic repository implementation that provides standard CRUD operations for Entity Framework Core entities. It abstracts common database operations like retrieving entities by ID, getting all entities, adding, updating, and deleting entities, checking existence, and saving changes. The repository includes built-in logging and error handling for robust data access operations.

### Usage Example

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection with DbContext
var services = new ServiceCollection();
services.AddDbContext<ServiceScaffoldDbContext>(options =>
    options.UseSqlite("Data Source=service-scaffold.db"));
services.AddLogging(configure => configure.AddConsole());

var serviceProvider = services.BuildServiceProvider();

// Resolve the repository for a specific entity type
var userRepository = serviceProvider.GetRequiredService<Repository<User>>();

// Add a new user
var newUser = new User
{
    Id = Guid.NewGuid(),
    Username = "johndoe",
    Email = "john.doe@example.com",
    CreatedAt = DateTime.UtcNow
};
var addedUser = await userRepository.AddAsync(newUser);
Console.WriteLine($"Added user: {addedUser.Id}");

// Get a user by ID
var existingUser = await userRepository.GetByIdAsync(addedUser.Id);
if (existingUser != null)
{
    Console.WriteLine($"Retrieved user: {existingUser.Username}");
}

// Check if a user exists
bool userExists = await userRepository.ExistsAsync(addedUser.Id);
Console.WriteLine($"User exists: {userExists}");

// Get all users
var allUsers = await userRepository.GetAllAsync();
Console.WriteLine($"Total users: {allUsers.Count()}");

// Update a user
if (existingUser != null)
{
    existingUser.Email = "john.doe.updated@example.com";
    var updatedUser = await userRepository.UpdateAsync(existingUser);
    Console.WriteLine($"Updated user email to: {updatedUser.Email}");
}

// Delete a user
await userRepository.DeleteAsync(addedUser.Id);
Console.WriteLine("User deleted successfully");

// Save changes explicitly (though AddAsync/UpdateAsync/DeleteAsync call this internally)
await userRepository.SaveChangesAsync();
```

This example demonstrates how to use the `Repository<T>` class for common CRUD operations with proper dependency injection setup, error handling through the repository's built-in logging, and type-safe operations for any entity type.

## UserRepository

The `UserRepository` provides data access methods for user management operations. It handles user retrieval by email, filtering users by status (active/locked), email existence checks, and loading users with their associated API keys. This repository is typically used by authentication services, user management APIs, and any component that needs to work with user data.

### Usage Example

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection with DbContext
var services = new ServiceCollection();
services.AddDbContext<ServiceScaffoldDbContext>(options =>
    options.UseSqlite("Data Source=service-scaffold.db"));
services.AddLogging(configure => configure.AddConsole());

var serviceProvider = services.BuildServiceProvider();

// Resolve the user repository
var userRepository = new UserRepository(
    serviceProvider.GetRequiredService<ServiceScaffoldDbContext>(),
    serviceProvider.GetRequiredService<ILogger<UserRepository>>());

var userId = Guid.NewGuid();

// Get a user by email
var user = await userRepository.GetByEmailAsync("user@example.com");
if (user != null)
{
    Console.WriteLine($"Found user: {user.Username}");
}

// Check if an email exists
bool emailExists = await userRepository.EmailExistsAsync("user@example.com");
Console.WriteLine($"Email exists: {emailExists}");

// Get all active users
var activeUsers = await userRepository.GetActiveUsersAsync();
Console.WriteLine($"Active users: {activeUsers.Count()}");

// Get all locked users
var lockedUsers = await userRepository.GetLockedUsersAsync();
Console.WriteLine($"Locked users: {lockedUsers.Count()}");

// Get a user with their API keys (for authentication scenarios)
var userWithKeys = await userRepository.GetWithApiKeysAsync(userId);
if (userWithKeys != null)
{
    Console.WriteLine($"User has {userWithKeys.ApiKeys.Count} API keys");
}
```

## DeploymentConfiguration

The `DeploymentConfiguration` class provides deployment utilities for generating production-ready configuration files including systemd service units, Caddy reverse proxy configurations, environment files, and comprehensive deployment guides. It centralizes deployment best practices for .NET applications running on Linux with systemd and Caddy, enabling consistent and repeatable deployments across different environments.

### Usage Example

```csharp
using DotnetServiceScaffold.Infrastructure.Configuration;

// Create deployment options with custom configuration
var options = new DeploymentOptions
{
    ServiceName = "user-api",
    ServiceDescription = "User Management API Service",
    ServiceUser = "apiuser",
    ApplicationPath = "/opt/user-api",
    DataPath = "/var/lib/user-api",
    LogPath = "/var/log/user-api",
    ServerDomain = "api.example.com",
    ApplicationPort = 5001,
    DotnetPath = "/usr/bin/dotnet",
    ServiceVersion = "2.1.0"
};

// Generate systemd service unit file
string systemdService = DeploymentConfiguration.GenerateSystemdServiceUnit(options);
Console.WriteLine("Systemd Service Unit:");
Console.WriteLine(systemdService);

// Generate Caddy reverse proxy configuration
string caddyConfig = DeploymentConfiguration.GenerateCaddyConfiguration(options);
Console.WriteLine("Caddy Configuration:");
Console.WriteLine(caddyConfig);

// Generate environment file for systemd
string envFile = DeploymentConfiguration.GenerateEnvironmentFile(options);
Console.WriteLine("Environment File:");
Console.WriteLine(envFile);

// Generate comprehensive deployment guide
string deploymentGuide = DeploymentConfiguration.GenerateDeploymentGuide(options);
Console.WriteLine("Deployment Guide:");
Console.WriteLine(deploymentGuide);
```

## ExceptionExtensions

The `ExceptionExtensions` class provides utility methods for working with exceptions, enabling robust error handling, debugging, and logging. These extensions simplify common patterns like extracting full error messages, checking exception types, determining retryability, and formatting error responses for APIs. The methods handle exception chains and provide safe defaults for user-facing messages.

### Usage Example

```csharp
using DotnetServiceScaffold.Shared.Extensions;
using System;
using System.IO;
using System.Net;
using System.Net.Http;

// Example exception chain for demonstration
try
{
    // Simulate a chain of exceptions
    throw new InvalidOperationException("Database operation failed",
        new IOException("Disk I/O error occurred",
            new UnauthorizedAccessException("Access denied to data directory")));
}
catch (Exception ex)
{
    // Get the complete error message chain
    string fullMessage = ex.GetFullMessage();
    Console.WriteLine($"Full error message: {fullMessage}");
    // Output: Full error message: Database operation failed -> Disk I/O error occurred -> Access denied to data directory

    // Get the complete stack trace including inner exceptions
    string fullStackTrace = ex.GetFullStackTrace();
    Console.WriteLine($"Full stack trace available ({fullStackTrace.Length} characters)");

    // Check if exception is of a specific type
    bool isIOException = ex.Is<IOException>();
    Console.WriteLine($"Is IOException: {isIOException}");
    
    bool isTimeoutException = ex.Is<TimeoutException>();
    Console.WriteLine($"Is TimeoutException: {isTimeoutException}");

    // Find a specific exception type in the chain
    var ioException = ex.FindInnerException<IOException>();
    if (ioException != null)
    {
        Console.WriteLine($"Found IOException: {ioException.Message}");
    }

    // Get a safe user-facing error message
    string userMessage = ex.GetSafeMessage();
    Console.WriteLine($"User message: {userMessage}");
    // Output: User message: The requested operation is not valid in the current state.

    // Get appropriate HTTP status code for the exception
    int statusCode = ex.GetHttpStatusCode();
    Console.WriteLine($"HTTP status code: {statusCode}");
    // Output: HTTP status code: 409 (Conflict for InvalidOperationException)

    // Check if exception is retryable
    bool shouldRetry = ex.IsRetryable();
    Console.WriteLine($"Should retry: {shouldRetry}");
    // Output: Should retry: True (IOException is retryable)

    // Convert exception to error object for API responses
    var errorObject = ex.ToErrorObject();
    Console.WriteLine($"Error object: {errorObject}");
    
    // Get structured log message
    string logMessage = ex.ToLogMessage("DatabaseService");
    Console.WriteLine($"Log message preview: {logMessage.Substring(0, Math.Min(100, logMessage.Length))}...");
}

// Example with HttpRequestException
try
{
    throw new HttpRequestException("Failed to connect to external API", null, HttpStatusCode.ServiceUnavailable);
}
catch (Exception ex)
{
    // These methods work with any exception type
    Console.WriteLine($"Safe message: {ex.GetSafeMessage()}");
    Console.WriteLine($"Status code: {ex.GetHttpStatusCode()}");
    Console.WriteLine($"Is retryable: {ex.IsRetryable()}");
}
```

These extension methods provide a comprehensive toolkit for exception handling, making it easier to implement consistent error handling patterns across your application while maintaining clean separation between technical details and user-facing messages.

### Usage Examples

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection with DbContext
var services = new ServiceCollection();
services.AddDbContext<ServiceScaffoldDbContext>(options =>
    options.UseSqlite("Data Source=service-scaffold.db"));
services.AddLogging(configure => configure.AddConsole());

var serviceProvider = services.BuildServiceProvider();

// Resolve the service repository
var serviceRepository = new ServiceRepository(
    serviceProvider.GetRequiredService<ServiceScaffoldDbContext>(),
    serviceProvider.GetRequiredService<ILogger<ServiceRepository>>());

var serviceId = Guid.NewGuid();
var ownerId = Guid.NewGuid();

// Get a service by name
var serviceByName = await serviceRepository.GetByNameAsync("user-service");

// Get services by status
var activeServices = await serviceRepository.GetByStatusAsync(ServiceStatus.Healthy);
var unhealthyServices = await serviceRepository.GetByStatusAsync(ServiceStatus.Unhealthy);

// Get all enabled services
var enabledServices = await serviceRepository.GetEnabledServicesAsync();

// Get services by owner
var ownerServices = await serviceRepository.GetByOwnerAsync(ownerId);

// Get a service with its metrics
var serviceWithMetrics = await serviceRepository.GetWithMetricsAsync(serviceId, metricsCount: 20);
if (serviceWithMetrics != null)
{
    Console.WriteLine($"Service {serviceWithMetrics.ServiceName} has {serviceWithMetrics.Metrics.Count} metrics");
}

// Find unhealthy services
var unhealthyServicesList = await serviceRepository.GetUnhealthyServicesAsync();
Console.WriteLine($"Found {unhealthyServicesList.Count()} unhealthy services");

// Find services without recent health checks (threshold: 5 minutes)
var staleServices = await serviceRepository.GetServicesWithoutRecentHealthCheckAsync(minutesThreshold: 5);
Console.WriteLine($"Found {staleServices.Count()} services without recent health checks");

// Create and add a new service
var newService = new ServiceRegistration
{
    Id = serviceId,
    ServiceName = "api-gateway",
    Endpoint = "https://api.example.com/gateway",
    Description = "API Gateway service",
    Status = ServiceStatus.Healthy,
    IsEnabled = true,
    OwnerId = ownerId,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

await serviceRepository.AddAsync(newService);
await serviceRepository.SaveChangesAsync();

// Update service status
var existingService = await serviceRepository.GetByNameAsync("api-gateway");
if (existingService != null)
{
    existingService.Status = ServiceStatus.Degraded;
    existingService.UpdatedAt = DateTime.UtcNow;
    await serviceRepository.UpdateAsync(existingService);
}
```

This example demonstrates how to use the `ServiceRepository` for service-specific queries including retrieving services by various criteria, working with health statuses, managing service metrics, and performing CRUD operations on service registrations.

## DockerComposeOptions

The `DockerComposeOptions` class provides configuration for generating Docker Compose files. It controls service naming, port mappings, environment variables, resource limits, and optional infrastructure services like Caddy reverse proxy, Prometheus metrics, and Redis caching. Use this class to customize container deployment configurations for different environments.

### Usage Example

```csharp
using DotnetServiceScaffold.Infrastructure.DockerCompose;

// Create Docker Compose configuration for production deployment
var options = new DockerComposeOptions
{
    ServiceName = "my-api",
    ImageName = "mycompany/api-service:2.1.0",
    HostPort = 80,
    ContainerPort = 5000,
    Environment = "Production",
    ConnectionString = "Data Source=/app/data/production.db",
    
    // Add custom environment variables
    EnvironmentVariables = new Dictionary<string, string>
    {
        ["ASPNETCORE_ENVIRONMENT"] = "Production",
        ["LOG_LEVEL"] = "Information",
        ["ENABLE_METRICS"] = "true"
    },
    
    // Configure volumes for persistent data
    Volumes = new Dictionary<string, string>
    {
        ["api-data"] = "/app/data",
        ["api-logs"] = "/app/logs"
    },
    
    // Enable optional infrastructure services
    IncludeCaddy = true,
    CaddyDomain = "api.example.com",
    IncludePrometheus = true,
    IncludeRedis = true,
    
    // Resource limits
    CpuLimit = "2",
    MemoryLimit = "1G"
};

// The options can be passed to DockerComposeGenerator to create a docker-compose.yml file
// var composeContent = DockerComposeGenerator.Generate(options);
```

This example demonstrates how to configure `DockerComposeOptions` with service details, environment settings, resource constraints, and optional infrastructure components for containerized deployments.

## HealthCheckRepository

The `HealthCheckRepository` provides data access and analytics for service health check results. It extends the generic `Repository<T>` class to offer specialized methods for querying health check data including retrieving results by service ID, finding recent or failed results, calculating average response times, counting failures, and cleaning up old health check records. The repository is designed for monitoring dashboards, alerting systems, and service reliability analysis.

### Usage Example

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection with DbContext
var services = new ServiceCollection();
services.AddDbContext<ServiceScaffoldDbContext>(options =>
  options.UseSqlite("Data Source=service-scaffold.db"));
services.AddLogging(configure => configure.AddConsole());

var serviceProvider = services.BuildServiceProvider();

// Resolve the health check repository
var healthCheckRepository = new HealthCheckRepository(
  serviceProvider.GetRequiredService<ServiceScaffoldDbContext>(),
  serviceProvider.GetRequiredService<ILogger<HealthCheckRepository>>());

var serviceId = Guid.NewGuid();

// Get all health check results for a service
var allResults = await healthCheckRepository.GetByServiceIdAsync(serviceId);
Console.WriteLine($"Found {allResults.Count()} total health check results");

// Get recent health check results (last 20 checks)
var recentResults = await healthCheckRepository.GetRecentResultsAsync(serviceId, count: 20);
Console.WriteLine($"Found {recentResults.Count()} recent health check results");

// Get the latest health check result
var latestResult = await healthCheckRepository.GetLatestResultAsync(serviceId);
if (latestResult != null)
{
  Console.WriteLine($"Latest result - Healthy: {latestResult.IsHealthy}, Response Time: {latestResult.ResponseTimeMs}ms");
}

// Get failed health check results from the last 24 hours
var failedResults = await healthCheckRepository.GetFailedResultsAsync(serviceId, hoursBack: 24);
Console.WriteLine($"Found {failedResults.Count()} failed health checks in last 24 hours");

// Calculate average response time for the last hour
var avgResponseTime = await healthCheckRepository.GetAverageResponseTimeAsync(serviceId, minutesBack: 60);
Console.WriteLine($"Average response time: {avgResponseTime:F2}ms");

// Count failures in the last hour
var failureCount = await healthCheckRepository.GetFailureCountAsync(serviceId, minutesBack: 60);
Console.WriteLine($"Failure count: {failureCount}");

// Clean up old health check results (older than 30 days)
await healthCheckRepository.DeleteOldResultsAsync(serviceId, daysToKeep: 30);
Console.WriteLine("Old health check results cleanup completed");

// Add a new health check result
var newHealthCheck = new HealthCheckResult
{
  Id = Guid.NewGuid(),
  ServiceId = serviceId,
  IsHealthy = true,
  ResponseTimeMs = 42,
  StatusCode = 200,
  CheckedAt = DateTime.UtcNow,
  Details = "Service responded within acceptable time"
};

await healthCheckRepository.AddAsync(newHealthCheck);
await healthCheckRepository.SaveChangesAsync();
```

This example demonstrates how to use the `HealthCheckRepository` for querying health check data, calculating reliability metrics, and managing health check history for monitoring and alerting purposes.


### Usage Example

```csharp
using DotnetServiceScaffold.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

// Setup service collection with all required services
var services = new ServiceCollection();

// Register application services
services.AddApplicationServices();

// Register integration services for external API calls and webhooks
services.AddIntegrationServices();

// Register caching services
services.AddCachingServices();

// Register background services for periodic tasks
services.AddBackgroundServices();

// Register API key authentication and rate limiting
services.AddApiAuthentication();

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Resolve registered services
var domainEventPublisher = serviceProvider.GetRequiredService<IDomainEventPublisher>();
var cacheService = serviceProvider.GetRequiredService<ICacheService>();
var externalApiClient = serviceProvider.GetRequiredService<IExternalApiClient>();
var httpClientFactory = serviceProvider.GetRequiredService<ICustomHttpClientFactory>();
```

This example demonstrates how to use the `ServiceCollectionExtensions` methods to configure the dependency injection container with all infrastructure and application services needed for a typical ASP.NET Core application.

## HttpUtility

The `HttpUtility` class provides utility methods for common HTTP operations including authentication header creation, query string manipulation, URL building, status code checking, and response parsing. It simplifies working with `HttpClient` and HTTP responses by providing strongly-typed helpers for common patterns like Basic and Bearer authentication, query parameter handling, and status code classification.

### Usage Examples

```csharp
using System;
using System.Collections.Generic;
using DotnetServiceScaffold.Shared.Utilities;

// Create Basic authentication header
string authHeader = HttpUtility.CreateBasicAuthHeader("admin", "secure-password-123");
Console.WriteLine(authHeader);
// Output: Basic YWRtaW46c2VjdXJlLXBhc3N3b3JkLTEyMw==

// Create Bearer token authorization header
string bearerHeader = HttpUtility.CreateBearerAuthHeader("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...");
Console.WriteLine(bearerHeader);
// Output: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

// Parse Basic authentication header
var basicAuth = HttpUtility.ParseBasicAuthHeader(authHeader);
if (basicAuth.HasValue)
{
    Console.WriteLine($"Username: {basicAuth.Value.Username}, Password: {basicAuth.Value.Password}");
    // Output: Username: admin, Password: secure-password-123
}

// Parse Bearer token from header
string? token = HttpUtility.ParseBearerToken(bearerHeader);
Console.WriteLine(token);
// Output: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

// Build query string from parameters
var queryParams = new Dictionary<string, string>
{
    ["page"] = "1",
    ["limit"] = "10",
    ["search"] = "test query"
};
string queryString = HttpUtility.BuildQueryString(queryParams);
Console.WriteLine(queryString);
// Output: page=1&limit=10&search=test+query

// Parse query string back to dictionary
var parsedParams = HttpUtility.ParseQueryString(queryString);
foreach (var param in parsedParams)
{
    Console.WriteLine($"{param.Key}: {param.Value}");
}
// Output:
// page: 1
// limit: 10
// search: test query

// Build URL with path and query parameters
string apiUrl = HttpUtility.BuildUrl(
    "https://api.example.com/v1",
    "users",
    new Dictionary<string, string> { ["page"] = "1", ["limit"] = "10" }
);
Console.WriteLine(apiUrl);
// Output: https://api.example.com/v1/users?page=1&limit=10

// Check status code categories
int statusCode = 404;
Console.WriteLine($"Is success: {HttpUtility.IsSuccessStatusCode(statusCode)}"); // false
Console.WriteLine($"Is client error: {HttpUtility.IsClientErrorStatusCode(statusCode)}"); // true
Console.WriteLine($"Is server error: {HttpUtility.IsServerErrorStatusCode(statusCode)}"); // false
Console.WriteLine($"Is retryable: {HttpUtility.IsRetryableStatusCode(statusCode)}"); // false

// Get retry delay for retryable status codes
int? delayMs = HttpUtility.GetRetryDelayMs(429, attempt: 1);
Console.WriteLine(delayMs); // ~200ms (with jitter)

// Extract media type and charset from Content-Type header
string? contentType = "application/json; charset=utf-8";
Console.WriteLine(HttpUtility.GetMediaType(contentType)); // application/json
Console.WriteLine(HttpUtility.GetCharset(contentType)); // utf-8

// Mask sensitive information in URLs for logging
string sensitiveUrl = "https://api.example.com/login?username=admin&password=secret123&token=xyz";
string maskedUrl = HttpUtility.MaskSensitiveUrl(sensitiveUrl);
Console.WriteLine(maskedUrl);
// Output: https://api.example.com/login?username=admin&password=***MASKED***&token=***MASKED***
```

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using DotnetServiceScaffold.Shared.Utilities;

// Serialize an object to JSON
var user = new { Id = 1, Name = "John Doe", Email = "john@example.com", Roles = new[] { "Admin", "User" } };
string json = JsonUtility.Serialize(user);
Console.WriteLine(json);
// Output: {"Id":1,"Name":"John Doe","Email":"john@example.com","Roles":["Admin","User"]}

// Serialize with pretty printing (indented JSON)
string prettyJson = JsonUtility.SerializePretty(user);
Console.WriteLine(prettyJson);
/* Output:
{
  "Id": 1,
  "Name": "John Doe",
  "Email": "john@example.com",
  "Roles": [
    "Admin",
    "User"
  ]
}
*/

// Deserialize back to a strongly-typed object
var deserializedUser = JsonUtility.Deserialize<Dictionary<string, object>>(json);
Console.WriteLine(deserializedUser["Name"]); // "John Doe"

// Deserialize to dynamic type
dynamic dynamicUser = JsonUtility.DeserializeDynamic(json);
Console.WriteLine(dynamicUser.Name); // "John Doe"
Console.WriteLine(dynamicUser.Roles[0]); // "Admin"

// Extract a property value from JSON
var email = JsonUtility.GetProperty<string>(json, "Email");
Console.WriteLine(email); // "john@example.com"

// Check if a string is valid JSON
bool isValid = JsonUtility.IsValidJson(json);
Console.WriteLine(isValid); // true

// Get the JSON type
string jsonType = JsonUtility.GetJsonType(json);
Console.WriteLine(jsonType); // "Object"

// Merge two JSON strings
string json1 = "{\"Name\":\"John\",\"Age\":30}";
string json2 = "{\"Age\":31,\"City\":\"New York\"}";
string mergedJson = JsonUtility.MergeJson(json1, json2);
Console.WriteLine(mergedJson); // {"Name":"John","Age":31,"City":"New York"}

// Format JSON for better readability
string formattedJson = JsonUtility.FormatJson(json);
Console.WriteLine(formattedJson);
```

## ExternalApiClient

The `ExternalApiClient` is a generic HTTP client for calling external APIs. It handles JSON serialization, error responses, and provides a clean interface for common HTTP operations (GET, POST, PUT, DELETE) with built-in logging and validation. The client automatically deserializes responses into the requested type and throws appropriate exceptions for failed requests.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.Integration;

public class ApiService
{
    private readonly IExternalApiClient _apiClient;

    public ApiService(IExternalApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<User?> GetUserAsync(Guid userId)
    {
        var url = "https://api.example.com/users/{userId}";
        return await _apiClient.GetAsync<User>(url);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        var url = "https://api.example.com/products";
        return await _apiClient.PostAsync<Product>(url, product);
    }

    public async Task<Product> UpdateProductAsync(Guid productId, Product product)
    {
        var url = "https://api.example.com/products/{productId}";
        return await _apiClient.PutAsync<Product>(url, product);
    }

    public async Task<bool> DeleteProductAsync(Guid productId)
    {
        var url = "https://api.example.com/products/{productId}";
        return await _apiClient.DeleteAsync(url);
    }
}

// Example usage
var apiClient = new ExternalApiClient(httpClientFactory, logger);
var user = await apiClient.GetAsync<User>("https://api.example.com/users/123");
var createdProduct = await apiClient.PostAsync<Product>(
    "https://api.example.com/products",
    new { Name = "New Product", Price = 99.99 }
);
var updated = await apiClient.PutAsync<Product>(
    "https://api.example.com/products/456",
    new { Name = "Updated Product", Price = 129.99 }
);
var deleted = await apiClient.DeleteAsync("https://api.example.com/products/789");
```

This example demonstrates how to use the `ExternalApiClient` to perform common CRUD operations against external APIs with proper type safety and error handling.

## ResponseFormatterFactory

The `ResponseFormatterFactory` creates and manages response formatters for different media types, implementing the Factory pattern to decouple formatter selection from usage. It maintains a registry of available formatters (JSON, CSV) and selects the appropriate formatter based on the requested media type, falling back to JSON for unsupported types. The factory also allows registering custom formatters at runtime.

### Usage Example

```csharp
using System;
using System.Linq;
using DotnetServiceScaffold.Infrastructure.Formatting;
using DotnetServiceScaffold.Infrastructure.Integration;

// Create the factory with built-in formatters
var factory = new ResponseFormatterFactory();

// Get a formatter for a specific media type
var jsonFormatter = factory.GetFormatter("application/json");
var csvFormatter = factory.GetFormatter("text/csv");

// Check if a media type is supported
bool supportsJson = factory.IsMediaTypeSupported("application/json"); // true
bool supportsXml = factory.IsMediaTypeSupported("application/xml"); // false

// Get all supported media types
var supportedTypes = factory.GetSupportedMediaTypes().ToList();
Console.WriteLine(string.Join(", ", supportedTypes));
// Output: application/json, text/csv, application/csv

// Register a custom formatter for XML responses
factory.RegisterFormatter("application/xml", new XmlResponseFormatter());

// Now XML is supported
bool supportsXmlAfterRegistration = factory.IsMediaTypeSupported("application/xml"); // true
```

This example demonstrates creating a formatter factory, retrieving formatters for different media types, checking support for media types, and registering custom formatters.


## ServiceScaffoldDbContext

The `ServiceScaffoldDbContext` class is the Entity Framework Core DbContext for the service scaffold platform. It manages all database entities including users, service registrations, health checks, metrics, events, API keys, audit logs, and service configurations. The context provides methods for database initialization and schema management, and is designed to work with SQLite by default (with WAL journal mode for better write concurrency).

### Usage Example

```csharp
using DotnetServiceScaffold.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Configure DbContext with dependency injection
var services = new ServiceCollection();

// Register DbContext with SQLite (or your preferred database provider)
services.AddDbContext<ServiceScaffoldDbContext>(options =>
    options.UseSqlite("Data Source=service-scaffold.db"));

services.AddLogging(configure => configure.AddConsole());

var serviceProvider = services.BuildServiceProvider();

// Resolve the DbContext
var dbContext = serviceProvider.GetRequiredService<ServiceScaffoldDbContext>();

// Access DbSets for all entities
var users = dbContext.Users;
var serviceRegistrations = dbContext.ServiceRegistrations;
var healthChecks = dbContext.HealthCheckResults;
var metrics = dbContext.ServiceMetrics;
var events = dbContext.ServiceEvents;
var apiKeys = dbContext.ApiKeys;
var auditLogs = dbContext.AuditLogs;
var configurations = dbContext.ServiceConfigurations;

// Initialize the database schema (creates tables if they don't exist)
await dbContext.InitializeDatabaseAsync();

// Example: Query for healthy services
var healthyServices = await serviceRegistrations
    .Include(s => s.HealthCheckResults)
    .Where(s => s.HealthCheckResults.Any(h => h.IsHealthy))
    .ToListAsync();

// Example: Add a new service registration
var newService = new ServiceRegistration
{
    ServiceName = "user-service",
    Endpoint = "https://api.example.com/users",
    Description = "User management service",
    IsActive = true,
    CreatedAt = DateTime.UtcNow
};

await serviceRegistrations.AddAsync(newService);
await dbContext.SaveChangesAsync();

// Example: Record a health check result
var healthCheck = new HealthCheckResult
{
    ServiceId = newService.Id,
    IsHealthy = true,
    ResponseTimeMs = 42,
    StatusCode = 200,
    CheckedAt = DateTime.UtcNow,
    Details = "Service responded within acceptable time"
};

await healthChecks.AddAsync(healthCheck);
await dbContext.SaveChangesAsync();

// Example: Query metrics for a specific service
var serviceMetrics = await metrics
    .Where(m => m.ServiceId == newService.Id)
    .OrderByDescending(m => m.RecordedAt)
    .Take(100)
    .ToListAsync();
```

## StructuredLoggingOptions

The `StructuredLoggingOptions` class configures the structured logging pipeline for the application. It controls application identification, enrichment with contextual information, correlation ID handling, and minimum log level filtering. These options are typically bound from the `StructuredLogging` section in `appsettings.json`.


### Usage Example

```csharp
using DotnetServiceScaffold.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Configure options in appsettings.json
// {
//   "StructuredLogging": {
//     "ApplicationName": "MyProductionService",
//     "EnrichWithMachineName": true,
//     "EnrichWithEnvironment": true,
//     "EnableCorrelationId": true,
//     "CorrelationIdHeader": "X-Request-Id",
//     "EnrichWithRequestContext": true,
//     "MinimumLevel": "Debug"
//   }
// }

// Register logging services with the configured options
var services = new ServiceCollection();

services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConfiguration(configuration.GetSection("StructuredLogging"));
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

// Configure structured logging options
services.Configure<StructuredLoggingOptions>(configuration.GetSection("StructuredLogging"));

var serviceProvider = services.BuildServiceProvider();

// Resolve logger and options
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var loggingOptions = serviceProvider.GetRequiredService<IOptions<StructuredLoggingOptions>>().Value;

// Use the configured logging
logger.LogInformation("Application {ApplicationName} started in {EnvironmentName}",
    loggingOptions.ApplicationName,
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development");

// Log with correlation ID (automatically added to HTTP requests if EnableCorrelationId is true)
logger.LogWarning("Request processing failed");
```

This example shows how to configure and use `StructuredLoggingOptions` to customize the logging pipeline with application-specific settings and contextual enrichment.


## ServiceMeshOptions

The `ServiceMeshOptions` class configures the service mesh sidecar proxy integration. It controls connection settings to the sidecar admin API, readiness timeouts, mesh identification, and enables/disables mesh integration. These options are typically bound from the `ServiceMesh` section in `appsettings.json`.

### Usage Example

```csharp
using DotnetServiceScaffold.Infrastructure.ServiceMesh;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Configure options in appsettings.json
// {
// "ServiceMesh": {
// "AdminEndpoint": "http://localhost:15000",
// "ReadinessTimeoutSeconds": 5,
// "MeshName": "istio",
// "Enabled": true
// }
// }

// Setup service collection with service mesh integration
var services = new ServiceCollection();

// Register service mesh integration with configuration
services.AddServiceMeshIntegration(configuration);

var serviceProvider = services.BuildServiceProvider();

// Resolve the sidecar proxy service
var sidecarProxy = serviceProvider.GetRequiredService<ISidecarProxyService>();

// Check if service mesh is enabled
bool isEnabled = await sidecarProxy.IsServiceMeshEnabledAsync();

// Configure the web application to use service mesh headers
var builder = WebApplication.CreateBuilder();
builder.Services.AddServiceMeshIntegration(builder.Configuration);

var app = builder.Build();

// Add service mesh header propagation middleware
app.UseServiceMeshHeaders();

// Continue with application configuration...
app.Run();
```

This example demonstrates how to configure and use `ServiceMeshOptions` to integrate with a service mesh sidecar proxy, including registering services, checking mesh availability, and enabling header propagation middleware.


## DotnetServiceScaffoldOptions

The `DotnetServiceScaffoldOptions` class provides the root configuration for the DotnetServiceScaffold application. It binds from the `ApplicationSettings` section in `appsettings.json` and controls core application behaviors including health monitoring intervals, security settings, API behavior, caching, and service registration limits. These options are validated using DataAnnotations and provide sensible defaults for production use.

### Usage Example

```csharp
using DotnetServiceScaffold.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Configure options in appsettings.json
// {
//   "ApplicationSettings": {
//     "HealthCheckInterval": 60,
//     "HealthCheckTimeout": 10,
//     "MaxConcurrentHealthChecks": 5,
//     "MaintenanceMode": false,
//     "AuditLogRetentionDays": 90,
//     "HealthCheckResultRetentionDays": 30,
//     "MaxFailedLoginAttempts": 5,
//     "AccountLockoutDurationMinutes": 30,
//     "PasswordMinimumLength": 8,
//     "EnableCors": true,
//     "AllowedOrigins": ["https://example.com", "https://api.example.com"],
//     "RateLimitPerMinute": 60,
//     "MaxServiceRegistrations": 100,
//     "MaxResponseSize": 1048576,
//     "EnableDetailedErrors": false,
//     "DefaultPageSize": 50,
//     "MaxPageSize": 200,
//     "CacheDurationSeconds": 300,
//     "EnableRequestLogging": true,
//     "MaxCollectionSize": 1000,
//     "JwtTokenExpirationMinutes": 60,
//     "JwtSecret": "your-very-long-secret-key-here",
//     "DatabaseMigrationStrategy": "Auto"
//   }
// }

// Setup service collection with configuration
var services = new ServiceCollection();

// Configure DotnetServiceScaffold options
services.Configure<DotnetServiceScaffoldOptions>(configuration.GetSection("ApplicationSettings"));

// Register application services
services.AddApplicationServices();

var serviceProvider = services.BuildServiceProvider();

// Resolve the options
var options = serviceProvider.GetRequiredService<IOptions<DotnetServiceScaffoldOptions>>().Value;

// Use the configured options throughout the application
Console.WriteLine($"Health checks run every {options.HealthCheckInterval} seconds");
Console.WriteLine($"Maximum concurrent health checks: {options.MaxConcurrentHealthChecks}");
Console.WriteLine($"CORS enabled: {options.EnableCors}");
Console.WriteLine($"Rate limit: {options.RateLimitPerMinute} requests per minute");
```

## ServiceDiscoveryOptions

The `ServiceDiscoveryOptions` class configures service discovery behavior for locating and connecting to other services within a distributed system. It supports multiple discovery modes (DNS-based, registry-based, or hybrid), configurable load balancing strategies, caching policies, and self-registration capabilities. These options are typically bound from the `ServiceDiscovery` section in `appsettings.json`.

### Usage Example

```csharp
using System;
using DotnetServiceScaffold.Infrastructure.ServiceDiscovery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Configure options in appsettings.json
// {
// "ServiceDiscovery": {
// "Enabled": true,
// "Mode": "Hybrid",
// "LoadBalancing": "RoundRobin",
// "CacheTtl": "00:00:30",
// "RefreshInterval": "00:01:00",
// "ResolutionTimeout": "00:00:05",
// "SearchDomain": "cluster.local",
// "PreferSrvRecords": true,
// "DnsServerAddress": "8.8.8.8",
// "DnsServerPort": 53,
// "DefaultPort": 8080,
// "DefaultScheme": "https",
// "MaxRetries": 3,
// "SocketTimeout": "00:00:05",
// "AgentEndpoint": "http://localhost:8500",
// "AclToken": "your-acl-token-here",
// "OnlyHealthyInstances": true
// }
// }

// Setup service collection with service discovery configuration
var services = new ServiceCollection();

services.Configure<ServiceDiscoveryOptions>(options =>
{
    options.Enabled = true;
    options.Mode = DiscoveryMode.Hybrid;
    options.LoadBalancing = LoadBalancingStrategy.RoundRobin;
    options.CacheTtl = TimeSpan.FromSeconds(30);
    options.RefreshInterval = TimeSpan.FromMinutes(1);
    options.ResolutionTimeout = TimeSpan.FromSeconds(5);
    options.SearchDomain = "cluster.local";
    options.PreferSrvRecords = true;
    options.DnsServerAddress = "8.8.8.8";
    options.DnsServerPort = 53;
    options.DefaultPort = 8080;
    options.DefaultScheme = "https";
    options.MaxRetries = 3;
    options.SocketTimeout = TimeSpan.FromSeconds(5);
    options.AgentEndpoint = "http://localhost:8500";
    options.AclToken = "your-acl-token-here";
    options.OnlyHealthyInstances = true;

    // Configure DNS-specific options
    options.Dns.PreferSrvRecords = true;
    options.Dns.SearchDomain = "cluster.local";
    options.Dns.DnsServerAddress = "8.8.8.8";
    options.Dns.DefaultPort = 8080;
    options.Dns.DefaultScheme = "https";

    // Configure Registry-specific options
    options.Registry.ServiceName = "my-service";
    options.Registry.ServiceId = Guid.NewGuid().ToString();
    options.Registry.Ttl = TimeSpan.FromSeconds(30);

    // Configure SelfRegistration options
    options.SelfRegistration.Enabled = true;
    options.SelfRegistration.ServiceName = "user-service";
    options.SelfRegistration.AdvertiseHost = "192.168.1.100";
    options.SelfRegistration.AdvertisePort = 5001;
    options.SelfRegistration.Version = "1.0.0";
});

// Register service discovery components
services.AddServiceDiscovery(configuration);

var serviceProvider = services.BuildServiceProvider();

// Resolve service discovery service
var discoveryService = serviceProvider.GetRequiredService<IServiceDiscoveryService>();

// Discover services using configured options
var result = await discoveryService.DiscoverAsync("product-service");
if (result.IsSuccess && result.Value is { } instances)
{
    Console.WriteLine($"Found {instances.Count} instances");
}
```

This example demonstrates configuring `ServiceDiscoveryOptions` with DNS, registry, and self-registration settings, then using the discovery service to locate service instances based on the configured options.



## DnsServiceDiscoveryProvider

The `DnsServiceDiscoveryProvider` implements service discovery using DNS SRV records with automatic A-record fallback. It sends raw UDP DNS queries to query SRV records (not supported by `System.Net.Dns`) and falls back to standard DNS A-record lookups when SRV records are unavailable or return no results. The provider supports watching for service instance changes and automatically respects DNS TTL values for polling intervals.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.ServiceDiscovery;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup service collection with DNS service discovery
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.Configure<ServiceDiscoveryOptions>(options =>
{
    options.Dns.PreferSrvRecords = true;
    options.Dns.SearchDomain = "cluster.local";
    options.Dns.DnsServerAddress = "8.8.8.8";
    options.Dns.DefaultPort = 8080;
    options.Dns.DefaultScheme = "https";
});
services.AddSingleton<IServiceDiscoveryProvider, DnsServiceDiscoveryProvider>();

var serviceProvider = services.BuildServiceProvider();

// Resolve a service by name
var dnsProvider = serviceProvider.GetRequiredService<IServiceDiscoveryProvider>();
var resolutionResult = await dnsProvider.ResolveAsync("user-service");

if (resolutionResult.IsSuccess && resolutionResult.Value is { } records)
{
    Console.WriteLine($"Found {records.Count} instances of user-service:");
    foreach (var record in records)
    {
        var endpointUri = record.ToEndpointUri();
        Console.WriteLine($"  - {endpointUri} (Weight: {record.Weight}, Priority: {record.Priority})");
    }
}

// Watch for service instance changes (polls based on DNS TTL)
await foreach (var currentRecords in dnsProvider.WatchAsync("product-service"))
{
    Console.WriteLine($"Service instances updated: {currentRecords.Count} instances");
    foreach (var record in currentRecords)
    {
        Console.WriteLine($"  - {record.Host}:{record.Port}");
    }
}

// Check if DNS provider is available
bool isAvailable = await dnsProvider.IsAvailableAsync();
Console.WriteLine($"DNS provider available: {isAvailable}");
```

This example demonstrates configuring the DNS service discovery provider, resolving service instances by name, watching for changes, and checking provider availability. The provider automatically handles SRV record lookups with A-record fallback and respects DNS TTL values for polling intervals.


## MetricsService

The `MetricsService` provides in-process metrics collection for tracking application performance counters, gauges, and timing data. It's designed for lightweight, low-overhead metric collection within a single process and can be used to monitor application health, track request rates, measure operation durations, and expose metrics for debugging purposes. For production monitoring, integrate with Prometheus, Application Insights, or similar monitoring systems.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.Metrics;
using Microsoft.Extensions.Logging;

// Create metrics service with logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var metricsService = new MetricsService(loggerFactory.CreateLogger<MetricsService>());

// Track request counts with different endpoints
metricsService.IncrementCounter("api.requests.total");
metricsService.IncrementCounter("api.requests.total", 1, new Dictionary<string, string> { { "endpoint", "/users" } });
metricsService.IncrementCounter("api.requests.total", 1, new Dictionary<string, string> { { "endpoint", "/products" } });

// Track current memory usage as a gauge
metricsService.RecordGauge("system.memory.used_mb", 1024.5);
metricsService.RecordGauge("system.memory.used_mb", 1536.2, new Dictionary<string, string> { { "process", "worker" } });

// Measure operation duration
var result = await metricsService.MeasureAsync("api.database.query.duration_ms", async () =>
{
    await Task.Delay(150); // Simulate database query
    return "query completed";
});

// Record timing directly
metricsService.RecordTiming("api.external_api.response_time_ms", 250, new Dictionary<string, string> { { "api", "payment" } });

// Get all metrics for inspection or export
var allMetrics = await metricsService.GetMetricsAsync();

// Reset metrics when needed (e.g., during application restart)
await metricsService.ResetAsync();

// Example: Monitor HTTP request handling
public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MetricsService _metrics;

    public MetricsMiddleware(RequestDelegate next, MetricsService metrics)
    {
        _next = next;
        _metrics = metrics;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            _metrics.RecordTiming("http.request.duration_ms", sw.ElapsedMilliseconds,
                new Dictionary<string, string>
                {
                    { "method", context.Request.Method },
                    { "path", context.Request.Path },
                    { "status", context.Response.StatusCode.ToString() }
                });
            _metrics.IncrementCounter("http.requests.total");
        }
    }
}
```

This example demonstrates how to use `MetricsService` to track application performance metrics including counters for request counts, gauges for system resources, and timers for operation durations. The service supports tagging metrics with dimensions for detailed analysis and provides methods to retrieve and reset collected metrics.

## ISidecarProxyService

The `ISidecarProxyService` interface provides a contract for interacting with local sidecar proxy admin APIs compatible with Envoy (such as those injected by Istio, Consul Connect, or Linkerd). It exposes methods for checking proxy readiness, retrieving cluster information, managing connection draining during shutdown, and detecting whether the application is running inside a service mesh environment.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.ServiceMesh;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class ServiceMeshHealthReporter
{
    private readonly ISidecarProxyService _sidecarProxy;
    private readonly ILogger<ServiceMeshHealthReporter> _logger;

    public ServiceMeshHealthReporter(
        ISidecarProxyService sidecarProxy,
        ILogger<ServiceMeshHealthReporter> logger)
    {
        _sidecarProxy = sidecarProxy;
        _logger = logger;
    }

    public async Task ReportMeshStatusAsync()
    {
        // Check if service mesh integration is enabled and ready
        bool isEnabled = await _sidecarProxy.IsServiceMeshEnabledAsync();
        
        if (!isEnabled)
        {
            _logger.LogInformation("Service mesh integration is disabled or not available");
            return;
        }

        // Get detailed proxy information
        var proxyInfo = await _sidecarProxy.GetProxyInfoAsync();
        
        _logger.LogInformation("Sidecar Proxy Status:");
        _logger.LogInformation("- Version: {Version}", proxyInfo.ProxyVersion);
        _logger.LogInformation("- Status: {Status}", proxyInfo.Status);
        _logger.LogInformation("- Mesh: {MeshName}", proxyInfo.MeshName);
        _logger.LogInformation("- Clusters: {ClusterCount}", proxyInfo.UpstreamClusters.Count);

        // Check readiness status
        bool isReady = await _sidecarProxy.CheckReadinessAsync();
        _logger.LogInformation("Proxy readiness: {IsReady}", isReady);

        // Get upstream clusters information
        var clusters = await _sidecarProxy.GetUpstreamClustersAsync();
        
        foreach (var cluster in clusters)
        {
            _logger.LogInformation(
                "Cluster: {Name} - Healthy: {Healthy}/{Total} hosts",
                cluster.Name,
                cluster.HealthyHosts,
                cluster.TotalHosts);
        }
    }

    public async Task PrepareForShutdownAsync()
    {
        // Gracefully drain connections before application shutdown
        await _sidecarProxy.DrainConnectionsAsync(drainSeconds: 15);
        _logger.LogInformation("Application shutdown sequence initiated with connection draining");
    }
}

// Example usage in DI setup
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddSidecarProxyIntegration(); // Registers ISidecarProxyService

var serviceProvider = services.BuildServiceProvider();

var reporter = serviceProvider.GetRequiredService<ServiceMeshHealthReporter>();
await reporter.ReportMeshStatusAsync();

// During graceful shutdown
// await reporter.PrepareForShutdownAsync();
```

This example demonstrates how to use `ISidecarProxyService` to integrate with a service mesh sidecar proxy for health reporting, readiness checks, cluster monitoring, and graceful shutdown procedures.



## HttpClientFactory

The `HttpClientFactory` provides a centralized way to create configured `HttpClient` instances with standardized settings for timeouts, headers, and authentication. It wraps the default `IHttpClientFactory` from .NET's dependency injection system and adds convenience methods for common HTTP client configurations including authenticated clients with API keys, Bearer tokens, and custom base URLs.

## JsonResponseFormatter

The `JsonResponseFormatter` formats objects as JSON responses with consistent formatting, null handling, and date serialization. It uses camelCase property naming, ignores null values by default, and serializes dates in ISO 8601 UTC format. The formatter supports all JSON-based media types and provides custom date serialization through a dedicated converter.

### Usage Example

```csharp
using System;
using System.Text.Json;
using DotnetServiceScaffold.Infrastructure.Formatting;

// Create the JSON formatter with default options
var formatter = new JsonResponseFormatter();

// Check if the formatter can handle a media type
bool canFormatJson = formatter.CanFormat("application/json");
bool canFormatJsonCustom = formatter.CanFormat("application/json+custom");

// Format a simple object to JSON
var user = new { Id = 1, Name = "John Doe", Email = "john@example.com" };
string json = await formatter.FormatAsync(user);
Console.WriteLine(json);
// Output: {"id":1,"name":"John Doe","email":"john@example.com"}

// Format a null value
string nullJson = await formatter.FormatAsync(null);
Console.WriteLine(nullJson); // null

// Format an object with DateTime
var order = new { 
    Id = 101, 
    CreatedAt = DateTime.UtcNow,
    Status = "Processing"
};
string orderJson = await formatter.FormatAsync(order);
Console.WriteLine(orderJson);
// Output: {"id":101,"createdAt":"2024-07-15T14:30:45.1234567Z","status":"Processing"}

// Check media type support
bool supportsJson = formatter.CanFormat("application/json");
bool supportsXml = formatter.CanFormat("application/xml");
```

### Usage Example

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.Integration;
using Microsoft.Extensions.DependencyInjection;

// Setup in DI container
var services = new ServiceCollection();
services.AddHttpClient();
services.AddSingleton<ICustomHttpClientFactory, HttpClientFactory>();
var serviceProvider = services.BuildServiceProvider();

var factory = serviceProvider.GetRequiredService<ICustomHttpClientFactory>();

// Create a basic HTTP client with default configuration
var defaultClient = factory.CreateClient();

// Create an authenticated client with API key
var apiKeyClient = factory.CreateAuthenticatedClient("your-api-key-here");

// Create a Bearer token authenticated client
var bearerClient = factory.CreateBearerClient("your-bearer-token-here");

// Create a client with custom base URL
var customBaseClient = factory.CreateClientWithBaseUrl("https://api.example.com/v1");

// Use the clients for HTTP requests
var response = await defaultClient.GetAsync("/users/123");
var authResponse = await apiKeyClient.GetAsync("/products");
var bearerResponse = await bearerClient.PostAsync("/orders", new StringContent("{}"));
```

This example shows how to configure and use the `HttpClientFactory` to create different types of HTTP clients for various integration scenarios.

## StringBenchmarks

The `StringBenchmarks` class provides performance benchmarks for common string manipulation operations used throughout the application. It measures the execution time of case conversion, slug generation, sensitive data masking, and random string generation utilities that run on every request.

### Usage Example

```csharp
using DotnetServiceScaffold.Benchmarks;

var benchmarks = new StringBenchmarks();

// Convert camelCase to snake_case
string snakeCase = benchmarks.ToSnakeCase();
Console.WriteLine(snakeCase); // "user_account_service_manager"

// Convert PascalCase to snake_case
string pascalSnake = benchmarks.ToSnakeCasePascal();
Console.WriteLine(pascalSnake); // "user_account_service_manager"

// Convert snake_case to camelCase
string camelCase = benchmarks.ToCamelCase();
Console.WriteLine(camelCase); // "userAccountServiceManager"

// Mask sensitive data (keeps last 4 characters visible)
string maskedKey = benchmarks.MaskSensitive();
Console.WriteLine(maskedKey); // "***************beef"

// Generate random strings of different lengths
string random32 = benchmarks.GenerateRandomString32();
string random64 = benchmarks.GenerateRandomString64();
Console.WriteLine($"Random 32: {random32}");
Console.WriteLine($"Random 64: {random64}");


## RegistryServiceDiscoveryProvider

The `RegistryServiceDiscoveryProvider` facilitates interaction with a service discovery registry, enabling dynamic service registration, lookup, and health monitoring. It provides mechanisms to register and deregister service instances, resolve service endpoints by name, and watch for changes in service configurations.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.ServiceDiscovery;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// Assume dependencies (httpFactory, options, logger) are injected via DI
var provider = new RegistryServiceDiscoveryProvider(httpFactory, options, logger);

// Registering a service
var registration = new ServiceDiscoveryRecord { Name = "my-service", Address = "10.0.0.1", Port = 8080 };
var registerResult = await provider.RegisterAsync(registration);
if (registerResult.IsSuccess)
{
    Console.WriteLine("Service registered successfully.");
}

// Resolving a service by name
var servicesResult = await provider.ResolveAsync("my-service");
if (servicesResult.IsSuccess)
{
    foreach (var service in servicesResult.Value)
    {
        Console.WriteLine($"Service: {service.Name} at {service.Address}:{service.Port}");
    }
}

// Watching for service changes
await foreach (var serviceList in provider.WatchAsync())
{
    Console.WriteLine($"Received update with {serviceList.Count} services.");
}

// Deregistering a service
var deregisterResult = await provider.DeregisterAsync(registration);
```

## ServiceDiscoveryService

The `ServiceDiscoveryService` orchestrates service discovery operations across DNS-based and registry-based providers with caching, configurable load balancing, and self-registration lifecycle management. It provides methods to discover services, select healthy endpoints, register and deregister service instances, retrieve service statistics, and manage discovery cache.

## AuditLogRepository

The `AuditLogRepository` provides data access methods for audit logging functionality, enabling compliance tracking, security auditing, and operational monitoring. It supports querying audit logs by user, entity, time range, and status, as well as automated cleanup of old logs. This repository is typically used by application services to record and retrieve audit events for security investigations and compliance reporting.

### Usage Example

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddDbContext<ServiceScaffoldDbContext>(options =>
    options.UseSqlite("Data Source=service-scaffold.db"));
services.AddLogging(configure => configure.AddConsole());

var serviceProvider = services.BuildServiceProvider();

// Resolve the audit log repository
var auditLogRepository = serviceProvider.GetRequiredService<AuditLogRepository>();

// Record an audit event (typically done automatically by domain events or application services)
var auditEvent = new AuditLog
{
    UserId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
    EntityType = "User",
    EntityId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
    Action = "Update",
    Status = "Success",
    Changes = "{\"Name\":\"Old Name\",\"Name\":\"New Name\"}",
    Metadata = "{\"ipAddress\":\"192.168.1.100\",\"userAgent\":\"Mozilla/5.0...\"}",
    CreatedAt = DateTime.UtcNow
};

// In a real application, this would be added via DbContext
// await dbContext.AuditLogs.AddAsync(auditEvent);
// await dbContext.SaveChangesAsync();

// Query audit logs by user ID
var userLogs = await auditLogRepository.GetByUserIdAsync(
    Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
    count: 20
);

Console.WriteLine($"Found {userLogs.Count()} recent logs for user");

// Query audit logs by entity type and ID
var entityLogs = await auditLogRepository.GetByEntityAsync(
    "User",
    Guid.Parse("123e4567-e89b-12d3-a456-426614174000")
);

Console.WriteLine($"Found {entityLogs.Count()} logs for this entity");

// Get recent audit logs for monitoring dashboard
var recentLogs = await auditLogRepository.GetRecentLogsAsync(count: 100);

// Find failed actions for error analysis
var failedActions = await auditLogRepository.GetFailedActionsAsync(count: 50);

Console.WriteLine($"Found {failedActions.Count()} failed actions in last 50 logs");

// Clean up old logs (typically run as a background job)
await auditLogRepository.DeleteOldLogsAsync(daysToKeep: 90);

Console.WriteLine("Old logs cleanup completed");
```

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.ServiceDiscovery;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Setup service collection with service discovery
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());

// Configure service discovery options
services.Configure<ServiceDiscoveryOptions>(options =>
{
    options.Enabled = true;
    options.Mode = DiscoveryMode.Hybrid;
    options.LoadBalancing = LoadBalancingStrategy.RoundRobin;
    options.CacheTtl = TimeSpan.FromSeconds(30);
    options.ResolutionTimeout = TimeSpan.FromSeconds(5);
    
    options.SelfRegistration.Enabled = true;
    options.SelfRegistration.ServiceName = "user-service";
    options.SelfRegistration.AdvertiseHost = "192.168.1.100";
    options.SelfRegistration.AdvertisePort = 5001;
    options.SelfRegistration.Version = "1.0.0";
});

// Register service discovery components
services.AddServiceDiscovery(configuration);

var serviceProvider = services.BuildServiceProvider();

// Resolve the service discovery service
var discoveryService = serviceProvider.GetRequiredService<IServiceDiscoveryService>();

// Discover all instances of a service
var discoverResult = await discoveryService.DiscoverAsync("product-service");
if (discoverResult.IsSuccess && discoverResult.Value is { } instances)
{
    Console.WriteLine($"Found {instances.Count} instances of product-service:");
    foreach (var instance in instances)
    {
        Console.WriteLine($" - {instance.ToEndpointUri()} (Weight: {instance.Weight}, Priority: {instance.Priority}, Status: {instance.HealthStatus})");
    }
}

// Select a healthy endpoint using configured load balancing strategy
var endpointResult = await discoveryService.SelectEndpointAsync("payment-service");
if (endpointResult.IsSuccess && endpointResult.Value is { } selectedEndpoint)
{
    Console.WriteLine($"Selected endpoint: {selectedEndpoint.ToEndpointUri()}");
    
    // Use the endpoint for HTTP requests
    var httpClientFactory = serviceProvider.GetRequiredService<ICustomHttpClientFactory>();
    var client = httpClientFactory.CreateClientWithBaseUrl(selectedEndpoint.ToEndpointUri().ToString());
    var response = await client.GetAsync("/api/health");
}

// Register this service instance with the discovery backend
var registerResult = await discoveryService.RegisterSelfAsync();
if (registerResult.IsSuccess)
{
    Console.WriteLine("Service successfully registered with discovery backend.");
}

// Get statistics for a service
var statsResult = await discoveryService.GetServiceStatsAsync("user-service");
if (statsResult.IsSuccess && statsResult.Value is { } stats)
{
    Console.WriteLine($"Service stats for {stats.ServiceName}:");
    Console.WriteLine($" - Total instances: {stats.TotalInstances}");
    Console.WriteLine($" - Healthy: {stats.HealthyInstances}");
    Console.WriteLine($" - Degraded: {stats.DegradedInstances}");
    Console.WriteLine($" - Critical: {stats.CriticalInstances}");
    Console.WriteLine($" - Active source: {stats.ActiveSource}");
}

// Get list of all registered services (requires Registry or Hybrid mode)
var servicesResult = await discoveryService.GetRegisteredServicesAsync();
if (servicesResult.IsSuccess && servicesResult.Value is { } serviceNames)
{
    Console.WriteLine("Registered services:");
    foreach (var serviceName in serviceNames)
    {
        Console.WriteLine($" - {serviceName}");
    }
}

// Refresh cache for a specific service
await discoveryService.RefreshAsync("user-service");

// In ASP.NET Core application setup
var builder = WebApplication.CreateBuilder();
builder.Services.AddServiceDiscovery(builder.Configuration);

var app = builder.Build();
app.UseServiceDiscovery(); // Handles self-registration on startup and deregistration on shutdown

app.Run();
```