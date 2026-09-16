# UserRepository

`UserRepository` is the Entity Framework Core repository for `User` entities. It implements `IUserRepository` and inherits the standard CRUD operations from `Repository<User>`.

## Responsibilities

- Query users by email address.
- Return active, unlocked users in full-name order.
- Return users whose locks have not expired, ordered by lock expiration.
- Check whether an email address is already in use.
- Load a user together with the user's API keys.
- Search email addresses and full names with paging.
- Provide the inherited create, read, update, delete, existence-check, and save operations for users.
- Pass cancellation tokens to asynchronous Entity Framework Core queries and log repository operations.

Queries return tracked entities because the repository does not use `AsNoTracking`. Except for the inherited write methods, the user-specific methods do not save or modify data.

## Construction

```csharp
public UserRepository(
    ServiceScaffoldDbContext context,
    ILogger<UserRepository> logger)
```

Both arguments are required. The constructor throws `ArgumentNullException` when either argument is `null`.

## User-specific public methods

All cancellation tokens are optional and default to `CancellationToken.None`.

### `GetByEmailAsync`

```csharp
Task<User?> GetByEmailAsync(
    string email,
    CancellationToken cancellationToken = default)
```

Returns the first user whose `Email` exactly equals `email`, or `null` when there is no match. The comparison behavior, including case sensitivity, is determined by the configured database provider and collation. A `null` or empty email causes an `ArgumentException` (with `ArgumentNullException` used for `null`).

### `GetActiveUsersAsync`

```csharp
Task<IEnumerable<User>> GetActiveUsersAsync(
    CancellationToken cancellationToken = default)
```

Returns users for which `IsActive` is `true` and `IsLocked` is `false`, ordered by `FullName` ascending. An active user with an expired lock is still excluded if its `IsLocked` flag has not been cleared.

### `GetLockedUsersAsync`

```csharp
Task<IEnumerable<User>> GetLockedUsersAsync(
    CancellationToken cancellationToken = default)
```

Returns users for which `IsLocked` is `true` and `LockedUntil` is later than the current UTC time. Results are ordered by `LockedUntil` descending. Locked users with no expiration or with an expired lock are not returned.

### `EmailExistsAsync`

```csharp
Task<bool> EmailExistsAsync(
    string email,
    CancellationToken cancellationToken = default)
```

Returns `true` when any user has an `Email` exactly equal to `email`; otherwise, returns `false`. Database collation determines case sensitivity. A `null` or empty email causes an `ArgumentException` (with `ArgumentNullException` used for `null`).

### `GetWithApiKeysAsync`

```csharp
Task<User?> GetWithApiKeysAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
```

Returns the user with the specified `Id`, eagerly loading the `ApiKeys` navigation collection as part of the query. Returns `null` when the user does not exist.

### `SearchUsersAsync`

```csharp
Task<IEnumerable<User>> SearchUsersAsync(
    string query,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
```

Trims the query, converts it to lowercase using invariant culture, and searches for users whose lowercased `Email` or `FullName` contains it. Results are ordered by `FullName` ascending, then paged with `Skip((page - 1) * pageSize)` and `Take(pageSize)`.

A `null` or empty query causes an `ArgumentException`; a whitespace-only query returns an empty collection. The method does not validate `page` or `pageSize`, so callers should supply a one-based positive page number and a positive page size.

## Inherited public methods

The following methods are inherited from `Repository<User>`:

- `GetByIdAsync(Guid id, CancellationToken cancellationToken = default)`
- `GetAllAsync(CancellationToken cancellationToken = default)`
- `AddAsync(User entity, CancellationToken cancellationToken = default)`
- `UpdateAsync(User entity, CancellationToken cancellationToken = default)`
- `DeleteAsync(Guid id)`
- `ExistsAsync(Guid id, CancellationToken cancellationToken = default)`
- `SaveChangesAsync()`

`AddAsync`, `UpdateAsync`, and `DeleteAsync` persist their changes internally. See `Repository<T>` for the generic methods' detailed error-handling behavior.

## Example usage

Prefer depending on `IUserRepository` so the caller is decoupled from the Entity Framework Core implementation:

```csharp
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;

public sealed class UserLookupService
{
    private readonly IUserRepository _users;

    public UserLookupService(IUserRepository users)
    {
        _users = users;
    }

    public async Task<User?> FindUserAsync(
        string email,
        CancellationToken cancellationToken)
    {
        if (!await _users.EmailExistsAsync(email, cancellationToken))
        {
            return null;
        }

        return await _users.GetByEmailAsync(email, cancellationToken);
    }

    public Task<IEnumerable<User>> SearchAsync(
        string query,
        int page,
        CancellationToken cancellationToken) =>
        _users.SearchUsersAsync(
            query,
            page,
            pageSize: 25,
            cancellationToken: cancellationToken);
}
```

Register the interface and implementation as scoped services alongside the scoped database context:

```csharp
services.AddScoped<IUserRepository, UserRepository>();
```
