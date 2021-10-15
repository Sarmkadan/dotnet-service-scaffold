# ResultExtensions

Extension methods for working with `Result` and `Result<T>` types, enabling fluent composition of operations and error handling in a functional style.

## API

### `public static Result<TNext> Then<T, TNext>(this Result<T> result, Func<T, TNext> onSuccess)`

Applies a transformation to the value of a successful `Result<T>` if it exists, returning a new `Result<TNext>`.
- **Parameters**:
  - `result`: The source `Result<T>`.
  - `onSuccess`: A function to transform the value if the result is successful.
- **Return value**: A new `Result<TNext>` containing the transformed value if the source was successful; otherwise, the original error.
- **Exceptions**: Throws `ArgumentNullException` if `onSuccess` is `null`.

### `public static async Task<Result<TNext>> ThenAsync<T, TNext>(this Result<T> result, Func<T, Task<TNext>> onSuccess)`

Asynchronously applies a transformation to the value of a successful `Result<T>` if it exists, returning a new `Result<TNext>`.
- **Parameters**:
  - `result`: The source `Result<T>`.
  - `onSuccess`: An async function to transform the value if the result is successful.
- **Return value**: A `Task<Result<TNext>>` containing the transformed value if the source was successful; otherwise, the original error.
- **Exceptions**: Throws `ArgumentNullException` if `onSuccess` is `null`.

### `public static Result<T> ToGeneric<T>(this Result result)`

Converts a non-generic `Result` into a generic `Result<T>` with a default value.
- **Parameters**:
  - `result`: The source `Result`.
- **Return value**: A `Result<T>` representing success if the source was successful; otherwise, the original error.
- **Exceptions**: None.

### `public static Result Combine(this IEnumerable<Result> results)`

Combines multiple `Result` instances into a single `Result`, succeeding only if all inputs succeed.
- **Parameters**:
  - `results`: An enumerable of `Result` instances to combine.
- **Return value**: A `Result` indicating success if all inputs succeeded; otherwise, the first encountered error.
- **Exceptions**: Throws `ArgumentNullException` if `results` is `null`.

### `public static Result<T[]> Combine<T>(this IEnumerable<Result<T>> results)`

Combines multiple `Result<T>` instances into a single `Result<T[]>`, succeeding only if all inputs succeed.
- **Parameters**:
  - `results`: An enumerable of `Result<T>` instances to combine.
- **Return value**: A `Result<T[]>` containing the values if all inputs succeeded; otherwise, the first encountered error.
- **Exceptions**: Throws `ArgumentNullException` if `results` is `null`.

### `public static Result<T> Also<T>(this Result<T> result, Action<T> onSuccess)`

Executes an action on the value of a successful `Result<T>` if it exists, returning the original result.
- **Parameters**:
  - `result`: The source `Result<T>`.
  - `onSuccess`: An action to perform on the value if the result is successful.
- **Return value**: The original `Result<T>`.
- **Exceptions**: Throws `ArgumentNullException` if `onSuccess` is `null`.

### `public static Result Also(this Result result, Action onSuccess)`

Executes an action if the `Result` is successful, returning the original result.
- **Parameters**:
  - `result`: The source `Result`.
  - `onSuccess`: An action to perform if the result is successful.
- **Return value**: The original `Result`.
- **Exceptions**: Throws `ArgumentNullException` if `onSuccess` is `null`.

### `public static T GetValueOrDefault<T>(this Result<T> result)`

Retrieves the value of a successful `Result<T>`, or returns the default value of `T` if the result failed.
- **Parameters**:
  - `result`: The source `Result<T>`.
- **Return value**: The contained value if successful; otherwise, `default(T)`.

### `public static T GetValueOrThrow<T>(this Result<T> result)`

Retrieves the value of a successful `Result<T>`, or throws an exception if the result failed.
- **Parameters**:
  - `result`: The source `Result<T>`.
- **Return value**: The contained value if successful.
- **Exceptions**: Throws `InvalidOperationException` if the result is a failure.

### `public static (string? ErrorMessage, string? ErrorCode) GetError(this Result result)`

Extracts the error details from a failed `Result`.
- **Parameters**:
  - `result`: The source `Result`.
- **Return value**: A tuple containing the error message and error code if the result failed; otherwise, `(null, null)`.

### `public static (string? ErrorMessage, string? ErrorCode) GetError<T>(this Result<T> result)`

Extracts the error details from a failed `Result<T>`.
- **Parameters**:
  - `result`: The source `Result<T>`.
- **Return value**: A tuple containing the error message and error code if the result failed; otherwise, `(null, null)`.

### `public static Result FromCondition(bool condition, string? errorMessage = null, string? errorCode = null)`

Creates a `Result` based on a boolean condition, failing if the condition is false.
- **Parameters**:
  - `condition`: The condition to evaluate.
  - `errorMessage`: Optional error message if the condition fails.
  - `errorCode`: Optional error code if the condition fails.
- **Return value**: A `Result` indicating success if the condition is true; otherwise, a failure with the provided error details.

### `public static Result<T> FromCondition<T>(bool condition, Func<T> valueFactory, string? errorMessage = null, string? errorCode = null)`

Creates a `Result<T>` based on a boolean condition, failing if the condition is false.
- **Parameters**:
  - `condition`: The condition to evaluate.
  - `valueFactory`: A function to produce the value if the condition is true.
  - `errorMessage`: Optional error message if the condition fails.
  - `errorCode`: Optional error code if the condition fails.
- **Return value**: A `Result<T>` containing the produced value if the condition is true; otherwise, a failure with the provided error details.
- **Exceptions**: Throws `ArgumentNullException` if `valueFactory` is `null`.

## Usage
