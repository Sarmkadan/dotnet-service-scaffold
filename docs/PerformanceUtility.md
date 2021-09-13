# PerformanceUtility
The `PerformanceUtility` class provides a set of static methods and properties for measuring and monitoring the performance of .NET applications. It offers functionality for measuring execution time, memory usage, CPU usage, and garbage collection statistics, as well as formatting elapsed time and byte values. Additionally, it includes a method for retrying operations with exponential backoff.

## API
### Static Methods
* `MeasureMs`: Measures the execution time of the current thread in milliseconds.
* `MeasureMs<T>`: Measures the execution time of the current thread in milliseconds and returns the result of type `T`.
	+ Parameters: None
	+ Return Value: A tuple containing the result of type `T` and the elapsed time in milliseconds
* `MeasureMsAsync`: Measures the execution time of the current asynchronous operation in milliseconds.
	+ Parameters: None
	+ Return Value: A task that returns the elapsed time in milliseconds
* `MeasureMsAsync<T>`: Measures the execution time of the current asynchronous operation in milliseconds and returns the result of type `T`.
	+ Parameters: None
	+ Return Value: A task that returns a tuple containing the result of type `T` and the elapsed time in milliseconds
* `GetMemoryUsageMb`: Returns the current memory usage of the process in megabytes.
	+ Parameters: None
	+ Return Value: The current memory usage in megabytes
* `GetMemoryStats`: Returns detailed memory statistics.
	+ Parameters: None
	+ Return Value: A `MemoryStats` object containing detailed memory statistics
* `GetCpuUsagePercent`: Returns the current CPU usage of the process as a percentage.
	+ Parameters: None
	+ Return Value: The current CPU usage as a percentage
* `GetGcStats`: Returns garbage collection statistics.
	+ Parameters: None
	+ Return Value: A `GarbageCollectionStats` object containing garbage collection statistics
* `FormatElapsedTime`: Formats an elapsed time value as a string.
	+ Parameters: None (assumed to be an overload or extension method)
	+ Return Value: A formatted string representation of the elapsed time
* `FormatBytes`: Formats a byte value as a string.
	+ Parameters: None (assumed to be an overload or extension method)
	+ Return Value: A formatted string representation of the byte value
* `RetryWithBackoffAsync<T>`: Retries an operation with exponential backoff.
	+ Parameters: None (assumed to be an overload or extension method)
	+ Return Value: A task that returns the result of type `T`

### Instance Properties
* `WorkingSetMb`: Gets the current working set of the process in megabytes.
* `PrivateMemoryMb`: Gets the current private memory usage of the process in megabytes.
* `PeakWorkingSetMb`: Gets the peak working set of the process in megabytes.
* `Gen0Collections`: Gets the number of generation 0 garbage collections.
* `Gen1Collections`: Gets the number of generation 1 garbage collections.
* `Gen2Collections`: Gets the number of generation 2 garbage collections.
* `TotalMemoryBytes`: Gets the total memory usage of the process in bytes.

## Usage
The following example demonstrates how to use the `MeasureMs` method to measure the execution time of a simple operation:
```csharp
var startTime = DateTime.Now;
// Perform some operation
var elapsedTime = PerformanceUtility.MeasureMs();
Console.WriteLine($"Elapsed time: {elapsedTime}ms");
```
The following example demonstrates how to use the `RetryWithBackoffAsync` method to retry an operation with exponential backoff:
```csharp
var result = await PerformanceUtility.RetryWithBackoffAsync(async () =>
{
    // Perform some operation that may fail
    await Task.Delay(100);
    return "Success";
});
Console.WriteLine($"Result: {result}");
```

## Notes
* The `MeasureMs` and `MeasureMsAsync` methods measure the execution time of the current thread or asynchronous operation, respectively. They do not account for time spent waiting for other threads or operations to complete.
* The `GetMemoryUsageMb` and `GetMemoryStats` methods return the current memory usage and detailed memory statistics, respectively. These values may fluctuate rapidly and should be used as a general guideline rather than an exact measurement.
* The `GetCpuUsagePercent` method returns the current CPU usage of the process as a percentage. This value may not reflect the actual CPU usage of the process, as it may be affected by other processes and system activity.
* The `GetGcStats` method returns garbage collection statistics. These statistics may not reflect the actual garbage collection activity of the process, as it may be affected by other processes and system activity.
* The `FormatElapsedTime` and `FormatBytes` methods format elapsed time and byte values as strings, respectively. These methods may not be suitable for all use cases and should be used with caution.
* The `RetryWithBackoffAsync` method retries an operation with exponential backoff. This method may not be suitable for all use cases and should be used with caution, as it may lead to increased latency and resource usage.
* The `PerformanceUtility` class is not thread-safe. Access to its members should be synchronized using a lock or other synchronization mechanism to ensure thread safety.
