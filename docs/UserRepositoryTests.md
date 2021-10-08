# UserRepositoryTests

Unit‑test class that validates the behavior of `UserRepository` against a test database. Each test method exercises a single CRUD operation and asserts the expected outcome.

## API

### UserRepositoryTests()
Initializes a new test fixture. The constructor configures an in‑memory Entity Framework Core `DbContext`, creates a `UserRepository` instance, and seeds any required test data.  
- **Parameters:** none  
- **Return:** a new `UserRepositoryTests` instance  
- **Throws:** `InvalidOperationException` if the in‑memory database cannot be created or if repository instantiation fails.

### AddUserAsync_ShouldAddUserToDatabase()
Verifies that `UserRepository.AddUserAsync` persists a new user record.  
- **Parameters:** none  
- **Return:** `Task` that completes when the assertion finishes  
- **Throws:** propagates any exception thrown by the repository (e.g., `DbUpdateException`) or `AssertFailedException` if the user is not found after insertion.

### GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
Confirms that `UserRepository.GetUserByIdAsync` returns the correct user when the identifier exists in the database.  
- **Parameters:** none  
- **Return:** `Task` that completes when the assertion finishes  
- **Throws:** propagates repository exceptions; throws `AssertFailedException` if the returned user is `null` or does not match the seeded data.

### GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
Ensures that `UserRepository.GetUserByIdAsync` returns `null` for an identifier that has no corresponding record.  
- **Parameters:** none  
- **Return:** `Task` that completes when the assertion finishes  
- **Throws:** propagates repository exceptions; throws `AssertFailedException` if a non‑null user is returned.

### GetUserByUsernameAsync_ShouldReturnUser_WhenUserExists()
Checks that `UserRepository.GetUserByUsernameAsync` retrieves the user associated with a known username.  
- **Parameters:** none  
- **Return:** `Task` that completes when the assertion finishes  
- **Throws:** propagates repository exceptions; throws `AssertFailedException` if the result is `null` or mismatched.

### GetUserByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
Validates that `UserRepository.GetUserByUsernameAsync` returns `null` when the username is not present.  
- **Parameters:** none  
- **Return:** `Task` that completes when the assertion finishes  
- **Throws:** propagates repository exceptions; throws `AssertFailedException` if a non‑null user is returned.

### UpdateUserAsync_ShouldUpdateUserInDatabase()
Tests that `UserRepository.UpdateUserAsync` correctly modifies an existing user’s properties.  
- **Parameters:** none  
- **Return:** `Task` that completes when the assertion finishes  
- **Throws:** propagates repository exceptions; throws `AssertFailedException` if the updated values are not reflected in the database.

### DeleteUserAsync_ShouldRemoveUserFromDatabase()
Confirms that `UserRepository.DeleteUserAsync` removes a user record from the database.  
- **Parameters:** none  
- **Return:** `Task` that completes when the assertion finishes  
- **Throws:** propagates repository exceptions; throws `AssertFailedException` if the user still exists after deletion.

### Dispose()
Releases the underlying `DbContext` and any unmanaged resources held by the test fixture.  
- **Parameters:** none  
- **Return:** `void`  
- **Throws:** `ObjectDisposedException` if called after the fixture has already been disposed.

## Usage

```csharp
using Xunit;
using DotnetServiceScaffold.Tests;

public class UserRepositoryTestsDemo : IClassFixture<UserRepositoryTests>
{
    private readonly UserRepositoryTests _tests;

    public UserRepositoryTestsDemo(UserRepositoryTests tests)
    {
        _tests = tests;
    }

    [Fact]
    public async Task AddUserAsync_PersistsNewUser()
    {
        // Arrange & Act are encapsulated in the test method.
        await _tests.AddUserAsync_ShouldAddUserToDatabase();
        // No further assertions needed; the test method throws on failure.
    }
}
```

A simpler ad‑hoc usage when running tests manually:

```csharp
using System.Threading.Tasks;
using DotnetServiceScaffold.Tests;

var testFixture = new UserRepositoryTests();
await testFixture.GetUserByIdAsync_ShouldReturnUser_WhenUserExists();
// Dispose when finished to clean up the in‑memory database.
testFixture.Dispose();
```

## Notes

- Each test method assumes a clean database state; the constructor seeds data that is specific to the asserted scenario.  
- The class is **not thread‑safe**; sharing a single instance across concurrent test executions may lead to data collisions because the underlying `DbContext` is mutable.  
- Calling `Dispose` more than once will result in an `ObjectDisposedException`; test frameworks typically invoke `Dispose` automatically via `IAsyncLifetime` or `IDisposable` semantics.  
- If the underlying repository throws unexpected exceptions (e.g., due to mis‑configured connection strings), the test methods will propagate those exceptions, causing the test to fail with the original exception type rather than an assertion failure.  
- No static state is retained between instances; creating multiple `UserRepositoryTests` objects yields isolated test fixtures.
