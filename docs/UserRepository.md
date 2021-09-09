# UserRepository

A repository class that provides data access operations for `User` entities using Entity Framework Core. It encapsulates queries and commands related to user management, including retrieval by email, status filtering, and API key associations.

## API

### `UserRepository(ServiceScaffoldDbContext context, ILogger<UserRepository> logger)`

Initializes a new instance of the `UserRepository` with the specified database context and logger.

- **Parameters**
  - `context`: The `ServiceScaffoldDbContext` instance used for database operations.
  - `logger`: The `ILogger<UserRepository>` instance used for logging operational details.

### `async Task<User?> GetByEmailAsync(string email)`

Retrieves a user by their email address, if it exists.

- **Parameters**
  - `email`: The email address to search for.
- **Return value**
  - A `Task` resolving to the `User` instance if found, or `null` otherwise.
- **Exceptions**
  - Throws `ArgumentException` if `email` is `null` or whitespace.

### `async Task<IEnumerable<User>> GetActiveUsersAsync()`

Retrieves all users whose account is currently active.

- **Return value**
  - A `Task` resolving to an `IEnumerable<User>` containing all active users.
- **Exceptions**
  - None.

### `async Task<IEnumerable<User>> GetLockedUsersAsync()`

Retrieves all users whose account is currently locked.

- **Return value**
  - A `Task` resolving to an `IEnumerable<User>` containing all locked users.
- **Exceptions**
  - None.

### `async Task<bool> EmailExistsAsync(string email)`

Checks whether a user with the specified email address exists in the system.

- **Parameters**
  - `email`: The email address to check.
- **Return value**
  - A `Task<bool>` resolving to `true` if the email exists, otherwise `false`.
- **Exceptions**
  - Throws `ArgumentException` if `email` is `null` or whitespace.

### `async Task<User?> GetWithApiKeysAsync(Guid userId)`

Retrieves a user by their unique identifier, including their associated API keys in a single query.

- **Parameters**
  - `userId`: The unique identifier of the user to retrieve.
- **Return value**
  - A `Task` resolving to the `User` instance if found (with `ApiKeys` loaded), or `null` otherwise.
- **Exceptions**
  - None.

## Usage
