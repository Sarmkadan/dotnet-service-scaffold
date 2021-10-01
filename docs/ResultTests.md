# ResultTests

`ResultTests` is a unit test class that validates the behaviour of the `Result` and `Result<T>` types within the `dotnet-service-scaffold` project. It ensures that success and failure states are correctly constructed, that error properties are properly populated from both explicit messages and exceptions, and that the `Map` operation correctly transforms values on success while preserving and propagating errors on failure.

## API

### `Success_NoArguments_ReturnsResultWithIsSuccessTrue`

Verifies that creating a `Result` using the parameterless success factory method produces an instance where `IsSuccess` is `true`. This test confirms the default success state is correctly initialised without requiring any additional data.

- **Parameters:** None (test method).
- **Return Value:** `void`.
- **Throws:** Does not throw; assertions within the method will cause the test to fail if the condition is not met.

### `Failure_WithMessageAndCode_SetsAllErrorProperties`

Validates that constructing a failure `Result` with an explicit error message and error code correctly populates the `Error` property (or equivalent error details). The test ensures `IsSuccess` is `false` and that both the message and code are stored and retrievable exactly as supplied.

- **Parameters:** None (test method).
- **Return Value:** `void`.
- **Throws:** Does not throw; assertions within the method will cause the test to fail if the error properties are incorrect.

### `Failure_FromException_CapturesMessageAndUsesTypeNameAsCode`

Confirms that when a failure `Result` is created from an exception, the exception’s `Message` is captured as the error message and the exception’s type name (e.g. `"InvalidOperationException"`) is used as the error code. The test typically throws or supplies a known exception and inspects the resulting `Result` for these derived values.

- **Parameters:** None (test method).
- **Return Value:** `void`.
- **Throws:** Does not throw; assertions within the method will cause the test to fail if the message or code derivation logic is flawed.

### `Map_OnSuccessResult_TransformsValueToNewType`

Tests the `Map` extension method on a successful `Result<T>`. It supplies a known value, applies a mapping function that transforms it to a different type, and asserts that the resulting `Result<U>` is a success containing the transformed value.

- **Parameters:** None (test method).
- **Return Value:** `void`.
- **Throws:** Does not throw; assertions within the method will cause the test to fail if the mapping is not applied or the value is not correctly transformed.

### `Map_OnFailureResult_PropagatesErrorWithoutCallingMapper`

Ensures that calling `Map` on a failed `Result<T>` does not invoke the provided mapping function and instead propagates the original failure state unchanged. The test verifies that the resulting `Result<U>` is a failure with the same error message and code as the original, and that any side effects in the mapper are not triggered.

- **Parameters:** None (test method).
- **Return Value:** `void`.
- **Throws:** Does not throw; assertions within the method will cause the test to fail if the mapper is erroneously invoked or the error is not propagated.

## Usage

### Example 1: Testing a service method that returns a Result

```csharp
[Fact]
public void CreateUser_WithValidInput_ReturnsSuccessResult()
{
    var service = new UserService();
    var result = service.CreateUser("valid-email@example.com", "securePassword123");

    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal("valid-email@example.com", result.Value.Email);
}
```

### Example 2: Testing error propagation through Map on failure

```csharp
[Fact]
public void ProcessOrder_WhenInventoryCheckFails_PropagatesError()
{
    var inventoryResult = Result.Failure("Insufficient stock", "STOCK_ERR");
    var mapperCalled = false;

    var processResult = inventoryResult.Map(item =>
    {
        mapperCalled = true;
        return new OrderConfirmation(item);
    });

    Assert.False(processResult.IsSuccess);
    Assert.Equal("Insufficient stock", processResult.Error.Message);
    Assert.Equal("STOCK_ERR", processResult.Error.Code);
    Assert.False(mapperCalled, "Mapper should not be invoked on a failure result.");
}
```

## Notes

- **Edge Cases:** The `Map` operation on a failure result must never execute the mapping delegate. Tests should guard against accidental invocation by tracking side effects or using mock functions that would throw if called. When constructing a failure from an exception with a null or empty `Message`, the behaviour should be explicitly defined—typically falling back to the exception type name or a default string.
- **Thread Safety:** The `Result` type itself is an immutable value object; once constructed, its state cannot change. This makes it inherently safe to share across threads without synchronisation. The test methods in `ResultTests` are standard xUnit facts that run independently and do not mutate shared state, so no concurrency concerns apply to the test execution.
