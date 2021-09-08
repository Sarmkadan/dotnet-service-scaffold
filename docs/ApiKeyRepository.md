# ApiKeyRepository

A repository class that encapsulates data access operations for API keys in the `dotnet-service-scaffold` project. It interacts with the `ServiceScaffoldDbContext` to perform CRUD operations and logging for API key entities.

## API

### `ApiKeyRepository(ServiceScaffoldDbContext context, ILogger<ApiKeyRepository> logger)`

Initializes a new instance of the `ApiKeyRepository` class.

**Parameters**
- `context`: The database context used to interact with the data store.
- `logger`: The logger instance used for logging operations and errors.

**Remarks**
This constructor is inherited from the base repository class.

---

### `Task<ApiKey?> GetByKeyPrefixAsync(string keyPrefix)`

Retrieves an API key entity by its partial key prefix.

**Parameters**
- `keyPrefix`: The partial key string used to locate the API key.

**Returns**
- A `Task` resolving to the `ApiKey` entity if found, or `null` if not found.

**Exceptions**
- Throws `ArgumentNullException` if `keyPrefix` is `null`.
- Throws `ArgumentException` if `keyPrefix` is empty or whitespace.

---

### `Task<ApiKey?> GetByFullKeyHashAsync(string fullKeyHash)`

Retrieves an API key entity by its full hashed key.

**Parameters**
- `fullKeyHash`: The full hashed key string used to locate the API key.

**Returns**
- A `Task` resolving to the `ApiKey` entity if found, or `null` if not found.

**Exceptions**
- Throws `ArgumentNullException` if `fullKeyHash` is `null`.
- Throws `ArgumentException` if `fullKeyHash` is empty or whitespace.

---
### `Task<IEnumerable<ApiKey>> GetActiveApiKeysForUserAsync(int userId)`

Retrieves all active API keys associated with a specific user.

**Parameters**
- `userId`: The identifier of the user whose API keys are to be retrieved.

**Returns**
- A `Task` resolving to an `IEnumerable<ApiKey>` containing all active API keys for the specified user.

**Exceptions**
- Throws `ArgumentOutOfRangeException` if `userId` is less than or equal to zero.

## Usage

### Retrieving an API key by partial key prefix
