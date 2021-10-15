# UserRepositoryTestsExtensions

Utility extension class that provides factory and assertion helpers for `UserRepository` integration tests. It encapsulates common setup and verification patterns—creating single or multiple test users, checking existence by identity or email—so that test methods remain concise and focused on the scenario under test.

## API

### CreateAndAddTestUserAsync

```csharp
public static async Task<UserRepository> CreateAndAddTestUserAsync(
    this UserRepository repository,
    string? email = null,
    string? name = null)
```

Creates a single test user with the given optional `email` and `name` (defaults are applied when arguments are null), persists it through the repository, and returns the repository instance for fluent chaining.

- **Parameters**  
  `repository` — the repository to operate on.  
  `email` — optional email address; if null, a unique test email is generated.  
  `name` — optional display name; if null, a default test name is used.

- **Returns**  
  The same `UserRepository` instance, enabling further fluent calls.

- **Exceptions**  
  Throws if the underlying persistence operation fails (e.g., constraint violation from a duplicate email when a fixed value is supplied).

---

### AssertUserExistsAsync

```csharp
public static async Task AssertUserExistsAsync(
    this UserRepository repository,
    Guid userId)
```

Asserts that a user with the specified `userId` exists in the repository. If the user is not found, the assertion fails (typically via the test framework’s assertion mechanism).

- **Parameters**  
  `repository` — the repository to query.  
  `userId` — the unique identifier of the expected user.

- **Returns**  
  A completed task on success; never returns a value.

- **Exceptions**  
  Relies on the underlying assertion library to signal failure (e.g., by throwing an assertion exception) when the user does not exist.

---

### CreateTestUsersAsync

```csharp
public static async Task<IReadOnlyList<User>> CreateTestUsersAsync(
    this UserRepository repository,
    int count)
```

Creates `count` distinct test users, persists them, and returns the list of created `User` objects.

- **Parameters**  
  `repository` — the repository to operate on.  
  `count` — the number of users to create; must be a positive integer.

- **Returns**  
  An `IReadOnlyList<User>` containing the persisted users in creation order.

- **Exceptions**  
  Throws `ArgumentOutOfRangeException` when `count` is zero or negative.  
  Throws if any individual persistence operation fails.

---

### AssertUserWithEmailExistsAsync

```csharp
public static async Task<bool> AssertUserWithEmailExistsAsync(
    this UserRepository repository,
    string email)
```

Checks whether a user with the exact `email` exists in the repository and asserts that it does. Returns a boolean indicating existence, allowing the caller to perform additional conditional logic after the assertion.

- **Parameters**  
  `repository` — the repository to query.  
  `email` — the email address to search for; must not be null.

- **Returns**  
  `true` if a user with the specified email exists; otherwise the assertion fails before a return value is produced.

- **Exceptions**  
  Throws `ArgumentNullException` when `email` is null.  
  Relies on the assertion framework to fail when no matching user is found.

---

## Usage

### Example 1: Single-user setup with fluent chaining

```csharp
[Test]
public async Task GetById_ReturnsUser_WhenUserExists()
{
    // Arrange
    var repository = new UserRepository(connectionFactory);
    var userId = Guid.NewGuid();

    await repository
        .CreateAndAddTestUserAsync(email: "alice@example.com", name: "Alice")
        .AssertUserExistsAsync(userId);

    // Act
    var result = await repository.GetByIdAsync(userId);

    // Assert
    Assert.That(result, Is.Not.Null);
    Assert.That(result.Email, Is.EqualTo("alice@example.com"));
}
```

### Example 2: Bulk creation and email verification

```csharp
[Test]
public async Task SearchByEmail_ReturnsCorrectUsers_AfterBulkInsert()
{
    // Arrange
    var repository = new UserRepository(connectionFactory);
    IReadOnlyList<User> users = await repository.CreateTestUsersAsync(count: 5);

    string targetEmail = users[2].Email;

    // Act + Assert
    bool exists = await repository.AssertUserWithEmailExistsAsync(targetEmail);
    Assert.That(exists, Is.True);

    var searchResults = await repository.SearchByEmailAsync(targetEmail);
    Assert.That(searchResults, Has.Exactly(1).Items);
}
```

---

## Notes

- **Generated defaults** — When `email` or `name` are omitted in `CreateAndAddTestUserAsync`, the implementation generates values that are unique per call (e.g., using a GUID-based suffix). This avoids unintended collisions across tests, provided tests do not share a persistent store without cleanup.
- **Assertion behavior** — `AssertUserExistsAsync` and `AssertUserWithEmailExistsAsync` are intended to fail the active test immediately when the condition is not met. They do not throw standard exceptions for missing data; they delegate to the configured test assertion framework.
- **Thread safety** — These methods are designed for sequential test execution. They perform no internal synchronization and assume exclusive access to the underlying data store during the test. Concurrent use from multiple tests against a shared repository instance or database may produce race conditions or false assertion failures.
- **Return value of `AssertUserWithEmailExistsAsync`** — The boolean return is guaranteed to be `true` when the method completes without throwing. It exists primarily to allow inline conditional logic (e.g., logging or additional verification) without requiring a separate query.
- **`CreateTestUsersAsync` count validation** — Passing zero or a negative count throws immediately. Passing an extremely large count may cause test timeouts or resource exhaustion depending on the underlying store; no internal throttling is applied.
