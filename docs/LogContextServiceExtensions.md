# LogContextServiceExtensions
The `LogContextServiceExtensions` class provides a set of extension methods for working with log context services in the `dotnet-service-scaffold` project. These methods enable the addition of properties to log contexts, measurement of execution times, and management of context scopes. They are designed to simplify logging and diagnostics in .NET applications.

## API
* `public static void AddProperties`: Adds properties to the log context. This method does not return any value and does not throw any exceptions based on its signature, but the actual implementation may throw exceptions depending on the properties being added.
* `public static void AddRequestProperties`: Adds request-specific properties to the log context. Similar to `AddProperties`, this method does not return any value and its exception behavior depends on the implementation.
* `public static void WithContextScope`: Creates a scope for the log context. This method does not return any value and its exception behavior is implementation-dependent.
* `public static T WithContextScope<T>`: Creates a scope for the log context and returns a value of type `T`. The return value is dependent on the implementation, and exceptions may be thrown based on the specific use case.
* `public static bool TryGetProperty<T>`: Attempts to retrieve a property of type `T` from the log context. Returns `true` if the property is successfully retrieved, `false` otherwise. This method does not throw exceptions based on its signature.
* `public static Stopwatch MeasureExecutionTime`: Measures the execution time of a block of code. Returns a `Stopwatch` object representing the execution time. This method does not throw exceptions based on its signature.
* `public static (T Result, Stopwatch Stopwatch) MeasureExecutionTime<T>`: Measures the execution time of a block of code that returns a value of type `T`. Returns a tuple containing the result of the execution and a `Stopwatch` object representing the execution time. This method does not throw exceptions based on its signature.

## Usage
The following examples demonstrate how to use the `LogContextServiceExtensions` class:
```csharp
// Example 1: Adding properties to the log context
LogContextServiceExtensions.AddProperties(new { UserId = 123, Operation = "CreateUser" });

// Example 2: Measuring execution time
var (result, stopwatch) = LogContextServiceExtensions.MeasureExecutionTime<int>(() =>
{
    // Code to measure execution time
    Thread.Sleep(1000);
    return 42;
});
Console.WriteLine($"Result: {result}, Execution Time: {stopwatch.ElapsedMilliseconds}ms");
```

## Notes
When using the `LogContextServiceExtensions` class, consider the following:
* The `AddProperties` and `AddRequestProperties` methods may throw exceptions if the properties being added are invalid or if there is an issue with the log context.
* The `WithContextScope` methods create a new scope for the log context, which may affect the behavior of other logging and diagnostics components.
* The `TryGetProperty` method returns `false` if the property is not found in the log context, but does not throw an exception.
* The `MeasureExecutionTime` methods return `Stopwatch` objects that can be used to measure execution times, but do not throw exceptions based on their signatures.
* The `LogContextServiceExtensions` class is designed to be thread-safe, but the actual implementation of the methods may have thread-safety implications depending on the specific use case.
