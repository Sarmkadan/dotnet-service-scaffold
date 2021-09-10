# Result

The `Result` type represents the outcome of an operation that can either succeed or fail, optionally carrying an error message and/or error code. Its generic counterpart `Result<T>` additionally holds a value of type `T` when the operation succeeds, enabling functional‑style chaining while preserving explicit error handling.

## API

### Result (non‑generic)

| Member | Purpose | Parameters | Return Value | Exceptions |
|--------|---------|------------|--------------|------------|
| `IsSuccess` | Indicates whether the operation succeeded. | None | `true` for success, `false` for failure. | None |
| `ErrorMessage` | Human‑readable description of the failure, if any. | None | `string?` containing the message; `null` when successful. | None |
| `ErrorCode` | Machine‑readable identifier for the failure, if any. | None | `string?` containing the code; `null` when successful. | None |
| `Success` | Creates a successful `Result` instance. | None | A `Result` with `IsSuccess == true`. | None |
| `Failure(string errorMessage)` | Creates a failed `Result` with the supplied message. | `errorMessage`: Description of the failure. | A `Result` with `IsSuccess == false` and `ErrorMessage` set. | None |
| `Failure(string errorMessage, string errorCode)` | Creates a failed `Result` with the supplied message and code. | `errorMessage`: Description of the failure.<br>`errorCode`: Identifier for the failure. | A `Result` with `IsSuccess == false`, `ErrorMessage` and `ErrorCode` set. | None |

### Result<T>

| Member | Purpose | Parameters | Return Value | Exceptions |
|--------|---------|------------|--------------|------------|
| `IsSuccess` | Indicates whether the operation succeeded and a value. | None | `true` for success, `false` for failure. | None |
| `Value` | The successfully produced value, if any. | None | `T?` containing the value when `IsSuccess` is `true`; otherwise `default(T)`. | None |
| `ErrorMessage` | Human‑readable description of the failure, if any. | None | `string?` containing the message; `null` when successful. | None |
| `ErrorCode` | Machine‑readable identifier for the failure, if any. | None | `string?` containing the code; `null` when successful. | None |
| `Success(T value)` | Creates a successful `Result<T>` containing the given value. | `value`: The value to store. | A `Result<T>` with `IsSuccess == true` and `Value` set to `value`. | None |
| `Failure(string errorMessage)` | Creates a failed `Result<T>` with the supplied message. | `errorMessage`: Description of the failure. | A `Result<T>` with `IsSuccess == false` and `ErrorMessage` set. | None |
| `Failure(string errorMessage, string errorCode)` | Creates a failed `Result<T>` with the supplied message and code. | `errorMessage`: Description of the failure.<br>`errorCode`: Identifier for the failure. | A `Result<T>` with `IsSuccess == false`, `ErrorMessage` and `ErrorCode` set. | None |
| `FromResult(Result result)` | Converts a non‑generic `Result` into a `Result<T>`, preserving failure information. | `result`: The `Result` to convert. | A `Result<T>` that mirrors the success/failure state of `result`; if successful, `Value` is `default(T)`. | None |
| `Map<TNext>(Func<T, TNext> mapper)` | Transforms the success value using a synchronous function, preserving failure state. | `mapper`: Function to apply to the success value. | A `Result<TNext>` that is successful with `mapper(Value)` when the source is successful; otherwise a failure preserving the original error information. | Throws if `mapper` throws. |
| `MapAsync<TNext>(Func<T, Task<TNext>> mapperAsync)` | Transforms the success value using an asynchronous function, preserving failure state. | `mapperAsync`: Async function to apply to the success value. | A `Task<Result<TNext>>` that completes successfully with `await mapperAsync(Value)` when the source is successful; otherwise completes with a failure preserving the original error information. | Throws if `mapperAsync` throws; exceptions are captured in the returned `Task`. |
| `IfSuccess(Action<T> onSuccess)` | Executes the supplied action only when the operation succeeded. | `onSuccess`: Action to invoke with the success value. | None | Throws if `onSuccess` throws. |
| `IfFailure(Action<string?, string?> onFailure)` | Executes the supplied action only when the operation failed, providing the error details. | `onFailure`: Action to invoke with the error message and error code (both may be `null`). | None | Throws if `onFailure` throws. |

## Usage

### Example 1: Validation with non‑generic `Result`

```csharp
Result ValidateAge(int age)
{
    if (age < 0)
        return Result.Failure("Age cannot be negative.", "NEG_AGE");
    if (age > 150)
        return Result.Failure("Age is unrealistically high.", "HIGH_AGE");
    return Result.Success;
}

// Usage
var result = ValidateAge(-5);
if (result.IsSuccess)
{
    Console.WriteLine("Age is valid.");
}
else
{
    Console.WriteLine($"Validation failed: {result.ErrorMessage} (code: {result.ErrorCode})");
}
```

### Example 2: Parsing and mapping with generic `Result<T>`

```csharp
Result<int> ParseNumber(string input)
{
    if (int.TryParse(input, out var number))
        return Result.Success(number);
    return Result.Failure("Not a valid integer.", "PARSE_FAIL");
}

Result<string> FormatAsHex(Result<int> numberResult) =>
    numberResult.Map(n => n.ToString("X4"));

// Usage
var parsed = ParseNumber("255");
var hex = FormatAsHex(parsed);

if (hex.IsSuccess)
{
    Console.WriteLine($"Hex value: {hex.Value}"); // prints "00FF"
}
else
{
    Console.WriteLine($"Error: {hex.ErrorMessage}");
}
```

## Notes

- Accessing `Value` on a failed `Result<T>` returns the default value for `T`; callers should check `IsSuccess` first to avoid silently using a default.
- The `FromResult` method allows lifting a non‑generic outcome into the generic type while preserving any error information; the resulting value will be `default(T)` on success.
- All members are immutable after construction; the type contains no mutable state, making it safe for concurrent read‑access from multiple threads without additional synchronization.
- Neither the synchronous nor asynchronous mapping methods catch exceptions thrown by the supplied mapper; such exceptions propagate outward (or are captured in the returned `Task` for `MapAsync`). Callers should handle exceptions inside the mapper if they wish to convert them into a failed `Result`.
