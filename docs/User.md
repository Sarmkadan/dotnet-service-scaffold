# User

Represents a registered user within the system, encapsulating identity, authentication state, profile metadata, and relationships to API keys and managed services. The type tracks login activity, enforces account locking after repeated failed attempts, and exposes convenience properties for validity and lock status.

## API

### Properties

#### `public Guid Id`
Unique identifier assigned at creation. Immutable after first persistence.

#### `public required string Email`
Email address used as the primary login credential. Must be supplied at instantiation; expected to be unique across active users.

#### `public required string FullName`
Display name for the user. Required at creation; used in UI and audit trails.

#### `public required string PasswordHash`
Cryptographic hash of the user’s password. Required at creation. Never stored or compared in plaintext. Consumers must hash before assignment.

#### `public string? Role`
Optional role label for authorization decisions (e.g. `"Admin"`, `"User"`). Null when no explicit role is assigned.

#### `public bool IsActive`
Indicates whether the user account is administratively enabled. Inactive accounts should be denied authentication regardless of lock state.

#### `public DateTime CreatedAt`
Timestamp of initial persistence. Set once and not updated thereafter.

#### `public DateTime UpdatedAt`
Timestamp of the most recent modification to any field on this record. Must be refreshed on every update.

#### `public DateTime? LastLoginAt`
Timestamp of the last successful login. Null if the user has never logged in.

#### `public string? ProfileImageUrl`
Optional URL to the user’s avatar or profile image. Null when no image is set.

#### `public string? Bio`
Optional short biography or description. Null when not provided.

#### `public int LoginAttempts`
Number of consecutive failed login attempts since the last successful login. Reset to zero on a successful login.

#### `public bool IsLocked`
Indicates whether the account is currently locked due to excessive failed login attempts. True when `LockedUntil` is set and in the future.

#### `public DateTime? LockedUntil`
If the account is locked, the UTC timestamp after which the lock expires and authentication may be retried. Null when the account is not locked.

#### `public ICollection<ApiKey> ApiKeys`
Navigation property to API keys owned by this user. Used for programmatic access tokens. Lazy-loaded or eagerly included depending on data-access configuration.

#### `public ICollection<ServiceRegistration> ManagedServices`
Navigation property to service registrations for which this user is designated as the manager. Lazy-loaded or eagerly included depending on data-access configuration.

#### `public bool IsValid`
Read-only derived property. Returns `true` when `IsActive` is `true` and the account is not locked (`IsLocked` is `false`). Represents whether the user is currently permitted to authenticate.

#### `public bool IsAccountLocked`
Read-only derived property. Returns `true` when `LockedUntil` has a value and that value is greater than the current UTC time. Evaluated at the moment of access; does not cache the result.

### Methods

#### `public void RecordSuccessfulLogin()`
Records a successful authentication event.
- **Behavior**: Sets `LastLoginAt` to the current UTC time, resets `LoginAttempts` to zero, clears `IsLocked` (sets to `false`), and sets `LockedUntil` to `null`.
- **Side effects**: Mutates `UpdatedAt`.
- **Throws**: No exceptions thrown by design.

#### `public void RecordFailedLoginAttempt()`
Records a failed authentication attempt.
- **Behavior**: Increments `LoginAttempts` by one. If `LoginAttempts` reaches or exceeds a predefined threshold (typically 5), sets `IsLocked` to `true` and `LockedUntil` to a future UTC timestamp (commonly 15 minutes from the current time).
- **Side effects**: Mutates `UpdatedAt`.
- **Throws**: No exceptions thrown by design. The locking threshold and duration are assumed to be constants or injected policy; this method enacts them without parameterisation.

## Usage

### Example 1: Authenticating a user and handling lockout

```csharp
public async Task<AuthenticationResult> AuthenticateAsync(
    User user,
    string providedPassword,
    IPasswordHasher hasher)
{
    if (!user.IsValid)
    {
        return AuthenticationResult.AccountInvalid;
    }

    bool passwordMatches = hasher.Verify(providedPassword, user.PasswordHash);
    if (!passwordMatches)
    {
        user.RecordFailedLoginAttempt();
        return AuthenticationResult.InvalidCredentials;
    }

    user.RecordSuccessfulLogin();
    return AuthenticationResult.Success;
}
```

### Example 2: Checking lock status before allowing a password reset flow

```csharp
public bool CanInitiatePasswordReset(User user)
{
    // Allow reset only if the account is active, even if temporarily locked.
    // Lockout prevents login, not administrative recovery.
    if (!user.IsActive)
    {
        return false;
    }

    // Optional: deny reset if permanently locked by an admin vs. automatic lock.
    if (user.IsLocked && user.LockedUntil > DateTime.UtcNow.AddHours(24))
    {
        return false; // abnormally long lock suggests manual intervention
    }

    return true;
}
```

## Notes

- **Lock threshold and duration**: The concrete values that trigger `IsLocked` in `RecordFailedLoginAttempt` are not exposed as parameters. Consumers must rely on the implementation defaults or configure them through a separate policy mechanism if available.
- **`IsValid` vs `IsAccountLocked`**: `IsValid` combines active status and lock state for a quick authentication gate. `IsAccountLocked` inspects only the temporal lock. An inactive user with an expired lock will still return `false` from `IsValid`.
- **Concurrency**: `RecordSuccessfulLogin` and `RecordFailedLoginAttempt` mutate shared state (`LoginAttempts`, `IsLocked`, `LockedUntil`, `UpdatedAt`). In multi-threaded scenarios (e.g. concurrent login attempts for the same user), external synchronisation or optimistic concurrency controls at the persistence layer are required. The methods themselves are not thread-safe.
- **Navigation properties**: `ApiKeys` and `ManagedServices` are collections whose contents depend on the object’s lifecycle context (attached vs. detached from a tracking session). Null or empty collections are possible when not loaded.
- **Derived properties**: `IsValid` and `IsAccountLocked` are computed on each access. Frequent evaluation in hot paths may warrant caching at the call site if the underlying fields are stable for the duration of an operation.
- **`UpdatedAt` maintenance**: Any mutation to a property should be accompanied by an update to `UpdatedAt`. The provided methods handle this internally; direct property assignments by external code must do so manually or rely on a persistence interceptor.
