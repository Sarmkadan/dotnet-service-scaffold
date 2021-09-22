# UserService

Provides operations for creating, retrieving, updating, and deleting users, including authentication, password management, and API key handling.

## API

### `UserService`

Constructor. Initializes a new instance of the `UserService` with required dependencies for user management.

### `async Task<User> CreateUserAsync(string email, string password, string? displayName = null)`

Creates a new user with the specified email, password, and optional display name. The password is validated before creation. Returns the newly created `User` instance. Throws `ArgumentException` if the email is invalid or the password does not meet complexity requirements. Throws `InvalidOperationException` if a user with the given email already exists.

**Parameters:**
- `email`: The user's email address.
- `password`: The user's password.
- `displayName`: Optional display name for the user.

**Return value:**
- A `Task<User>` representing the asynchronous operation, containing the created user.

### `async Task<User?> GetUserByEmailAsync(string email)`

Retrieves a user by their email address. Returns `null` if no user with the specified email exists. Does not throw.

**Parameters:**
- `email`: The email address of the user to retrieve.

**Return value:**
- A `Task<User?>` representing the asynchronous operation, containing the user if found, otherwise `null`.

### `async Task<User?> AuthenticateUserAsync(string email, string password)`

Authenticates a user by email and password. Returns the authenticated `User` if successful, otherwise `null`. Does not throw.

**Parameters:**
- `email`: The user's email address.
- `password`: The user's password.

**Return value:**
- A `Task<User?>` representing the asynchronous operation, containing the authenticated user if credentials are valid, otherwise `null`.

### `async Task<User> UpdateUserAsync(User user)`

Updates an existing user with the provided `User` instance. Returns the updated `User`. Throws `ArgumentNullException` if the user is `null`. Throws `InvalidOperationException` if the user does not exist or if the update conflicts with existing data.

**Parameters:**
- `user`: The `User` instance containing updated user data.

**Return value:**
- A `Task<User>` representing the asynchronous operation, containing the updated user.

### `async Task DeleteUserAsync(string email)`

Deletes the user with the specified email address. Does not throw if the user does not exist.

**Parameters:**
- `email`: The email address of the user to delete.

**Return value:**
- A `Task` representing the asynchronous operation.

### `async Task<IEnumerable<User>> GetActiveUsersAsync()`

Retrieves all active users. Returns an empty enumerable if no active users exist. Does not throw.

**Return value:**
- A `Task<IEnumerable<User>>` representing the asynchronous operation, containing the active users.

### `async Task<bool> ValidatePasswordAsync(string password)`

Validates a password against configured complexity rules. Returns `true` if the password meets requirements, otherwise `false`. Does not throw.

**Parameters:**
- `password`: The password to validate.

**Return value:**
- A `Task<bool>` representing the asynchronous operation, indicating whether the password is valid.

### `async Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword)`

Changes the password for the user with the specified email. Returns `true` if the password was changed successfully, otherwise `false`. Throws `ArgumentException` if the new password does not meet complexity requirements. Throws `InvalidOperationException` if the user does not exist or the current password is incorrect.

**Parameters:**
- `email`: The user's email address.
- `currentPassword`: The user's current password.
- `newPassword`: The new password to set.

**Return value:**
- A `Task<bool>` representing the asynchronous operation, indicating whether the password change was successful.

### `async Task UnlockUserAsync(string email)`

Unlocks the user with the specified email address if they are currently locked. Does not throw if the user does not exist or is not locked.

**Parameters:**
- `email`: The email address of the user to unlock.

**Return value:**
- A `Task` representing the asynchronous operation.

### `async Task<User?> GetUserWithApiKeysAsync(string email)`

Retrieves a user by email along with their associated API keys. Returns `null` if no user with the specified email exists. Does not throw.

**Parameters:**
- `email`: The email address of the user to retrieve.

**Return value:**
- A `Task<User?>` representing the asynchronous operation, containing the user with API keys if found, otherwise `null`.

### `async Task<User?> ValidateApiKeyAsync(string apiKey)`

Validates an API key and returns the associated user if valid. Returns `null` if the API key is invalid or expired. Does not throw.

**Parameters:**
- `apiKey`: The API key to validate.

**Return value:**
- A `Task<User?>` representing the asynchronous operation, containing the associated user if the API key is valid, otherwise `null`.

## Usage
