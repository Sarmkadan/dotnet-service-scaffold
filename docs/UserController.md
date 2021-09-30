# UserController

The `UserController` class provides RESTful endpoints for user management within the `dotnet-service-scaffold` project. It handles user registration, authentication, profile retrieval, password changes, and account unlocking. This controller is designed to interact with client applications via HTTP requests, returning appropriate responses based on the operation's outcome.

## API

### `UserController`
- **Purpose**: Initializes the controller with required dependencies (e.g., services for user management, authentication, and logging).
- **Parameters**: None (dependencies are injected via constructor).
- **Thread Safety**: Thread-safe, as ASP.NET Core controllers are stateless and instantiated per request.

### `Task<IActionResult> Register(RegisterRequest request)`
- **Purpose**: Registers a new user with the provided credentials and profile information.
- **Parameters**:
  - `request` (`RegisterRequest`): Contains required fields for registration (e.g., `Username`, `Password`, `Email`).
- **Return Value**:
  - `201 Created` if registration succeeds.
  - `400 BadRequest` if the request is invalid (e.g., missing fields, duplicate username/email).
  - `500 InternalServerError` if an unexpected error occurs.
- **Throws**: None (errors are returned as `IActionResult`).

### `Task<IActionResult> Login(LoginRequest request)`
- **Purpose**: Authenticates an existing user and returns an access token or session identifier.
- **Parameters**:
  - `request` (`LoginRequest`): Contains credentials (`Username`, `Password`).
- **Return Value**:
  - `200 OK` with authentication token/session data if login succeeds.
  - `401 Unauthorized` if credentials are invalid.
  - `403 Forbidden` if the account is locked or disabled.
  - `500 InternalServerError` if an unexpected error occurs.
- **Throws**: None.

### `Task<IActionResult> GetUser()`
- **Purpose**: Retrieves the profile information of the currently authenticated user.
- **Parameters**: None (user identity is inferred from the request context).
- **Return Value**:
  - `200 OK` with user profile data if the user is authenticated.
  - `401 Unauthorized` if the request lacks valid authentication.
  - `404 NotFound` if the user does not exist.
  - `500 InternalServerError` if an unexpected error occurs.
- **Throws**: None.

### `Task<IActionResult> ChangePassword(ChangePasswordRequest request)`
- **Purpose**: Updates the password of the currently authenticated user.
- **Parameters**:
  - `request` (`ChangePasswordRequest`): Contains `CurrentPassword` and `NewPassword`.
- **Return Value**:
  - `200 OK` if the password is successfully updated.
  - `400 BadRequest` if the request is invalid (e.g., weak password, mismatched current password).
  - `401 Unauthorized` if the request lacks valid authentication.
  - `500 InternalServerError` if an unexpected error occurs.
- **Throws**: None.

### `Task<IActionResult> UnlockUser(string username)`
- **Purpose**: Unlocks a user account that has been locked due to failed login attempts.
- **Parameters**:
  - `username` (`string`): The username of the account to unlock.
- **Return Value**:
  - `200 OK` if the account is successfully unlocked.
  - `400 BadRequest` if the username is invalid or empty.
  - `403 Forbidden` if the requesting user lacks administrative privileges.
  - `404 NotFound` if the user does not exist.
  - `500 InternalServerError` if an unexpected error occurs.
- **Throws**: None.

### `record RegisterRequest`
- **Purpose**: Encapsulates the required fields for user registration.
- **Properties**:
  - `Username` (`string`): Unique identifier for the user.
  - `Password` (`string`): Plaintext password (handled securely during processing).
  - `Email` (`string`): User's email address.
  - Additional optional fields (e.g., `FirstName`, `LastName`) may be included.

### `record LoginRequest`
- **Purpose**: Encapsulates the credentials for user authentication.
- **Properties**:
  - `Username` (`string`): User's username.
  - `Password` (`string`): User's password.

### `record ChangePasswordRequest`
- **Purpose**: Encapsulates the fields required for password updates.
- **Properties**:
  - `CurrentPassword` (`string`): The user's existing password for verification.
  - `NewPassword` (`string`): The new password to set.

## Usage

### Example 1: Registering a New User
