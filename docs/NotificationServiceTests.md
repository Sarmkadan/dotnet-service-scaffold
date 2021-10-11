# NotificationServiceTests

`NotificationServiceTests` contains unit tests that verify the behavior of the notification‑sending functionality in the `dotnet-service-scaffold` project. Each test method exercises a specific scenario—such as successful delivery, handling of different notification types, bulk operations, and alert generation—and asserts that the underlying service returns the expected result.

## API

### `public async Task SendNotificationAsync_ShouldReturnTrue_OnSuccess`
- **Purpose**: Confirms that `NotificationService.SendNotificationAsync` returns `true` when a single notification is sent successfully.
- **Parameters**: None.
- **Return Value**: A `Task` that completes when the test finishes; the test fails if an exception is thrown or an assertion does not hold.
- **Throws**: May propagate any exception thrown by the system under test (e.g., invalid configuration) or an assertion exception from the test framework if the returned value is not `true`.

### `public async Task SendNotificationAsync_ShouldReturnTrue_OnSuccessWithDifferentType`
- **Purpose**: Verifies that the service correctly handles a notification of a different type and still returns `true` on success.
- **Parameters**: None.
- **Return Value**: A `Task` that completes when the test finishes; failure occurs if the result is not `true` or an unexpected exception is raised.
- **Throws**: Same as above; additionally, fails if the service does not support the supplied type.

### `public async Task SendEmailAsync_ShouldReturnTrue_OnSuccess`
- **Purpose**: Ensures that the email‑specific sending path (`SendEmailAsync`) yields a successful result (`true`).
- **Parameters**: None.
- **Return Value**: A `Task` completing when the test ends; the test is considered failed on any exception or a non‑true result.
- **Throws**: Propagates exceptions from the email sender or assertion failures.

### `public async Task SendBulkNotificationAsync_ShouldReturnCorrectCount`
- **Purpose**: Checks that bulk notification sending returns the exact number of successfully delivered notifications.
- **Parameters**: None.
- **Return Value**: A `Task` that completes after the bulk operation and its validation; test fails if the returned count does not match the expected count.
- **Throws**: May throw if the bulk operation encounters an error that prevents counting, or if an assertion fails.

### `public async Task SendBulkNotificationAsync_ShouldHandleEmptyUserList`
- **Purpose**: Validates that invoking the bulk send method with an empty user list does not cause an error and returns a count of zero.
- **Parameters**: None.
- **Return Value**: A `Task` completing after the call; the test fails if an exception is thrown or the returned count is not zero.
- **Throws**: Any unexpected exception from the service or an assertion failure.

### `public async Task SendAlertAsync_ShouldReturnTrue_OnSuccess`
- **Purpose**: Asserts that sending an alert via `SendAlertAsync` succeeds and returns `true`.
- **Parameters**: None.
- **Return Value**: A `Task` that finishes when the test completes; failure on exception or non‑true result.
- **Throws**: Propagates service exceptions or assertion errors.

### `public async Task SendAlertAsync_ShouldReturnTrue_WithoutDetails`
- **Purpose**: Confirms that an alert can be sent successfully when no optional details are supplied, still yielding a `true` result.
- **Parameters**: None.
- **Return Value**: A `Task` completing after the test; fails on any exception or if the result is not `true`.
- **Throws**: Same as above.

## Usage

```csharp
using System.Threading.Tasks;
using Xunit; // or whichever test framework the project uses

public class ExampleTestRunner
{
    public async Task RunAllTests()
    {
        var testClass = new NotificationServiceTests();

        await testClass.SendNotificationAsync_ShouldReturnTrue_OnSuccess();
        await testClass.SendNotificationAsync_ShouldReturnTrue_OnSuccessWithDifferentType();
        await testClass.SendEmailAsync_ShouldReturnTrue_OnSuccess();
        await testClass.SendBulkNotificationAsync_ShouldReturnCorrectCount();
        await testClass.SendBulkNotificationAsync_ShouldHandleEmptyUserList();
        await testClass.SendAlertAsync_ShouldReturnTrue_OnSuccess();
        await testClass.SendAlertAsync_ShouldReturnTrue_WithoutDetails();
    }
}
```

In practice, the tests are executed automatically by the test runner (`dotnet test`), which instantiates `NotificationServiceTests` and invokes each method marked as a test.

## Notes

- The test class does not maintain mutable state; each method relies on freshly created mocks or dependencies, making the tests safe to run in parallel without additional synchronization.
- If any test method depends on shared resources (e.g., a global mock repository), those resources must be reset between invocations to avoid cross‑test contamination.
- The methods declare no parameters; therefore, edge cases related to argument validation (such as `null` inputs) are not applicable here. Edge cases are instead expressed through the test scenarios themselves (empty user list, missing details, different notification types).
- Exceptions thrown by the system under test are not caught within the test methods; they bubble up and cause the test to fail, which is the expected behavior for a unit test suite.
