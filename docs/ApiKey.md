# ApiKey

Represents an API key entity used for authenticating and authorizing service requests. Tracks usage metrics, expiration, allowed scopes, and IP restrictions for security and governance purposes.

## API

### Properties

- **`Id`** (Guid)
  Unique identifier for the API key. Immutable after creation.

- **`UserId`** (Guid)
  Identifier of the user associated with this API key. Used to link the key to a specific user account.

- **`User`** (User?)
  Navigation property referencing the associated user. May be null if the user is not loaded or does not exist.

- **`Name`** (string, required)
  Human-readable name or label for the API key. Used for identification and management purposes.

- **`KeyHash`** (string, required)
  Cryptographic hash of the API key value. Stored securely; the raw key is never persisted.

- **`KeyPrefix`** (string, required)
  First 8 characters of the raw API key. Used for display and partial identification without exposing the full key.

- **`CreatedAt`** (DateTime)
  Timestamp indicating when the API key was generated.

- **`ExpiresAt`** (DateTime?, nullable)
  Optional expiration timestamp for the API key. If null, the key does not expire.

- **`LastUsedAt`** (DateTime?, nullable)
  Timestamp of the last recorded usage of the API key. Updated via `RecordUsage`.

- **`IsActive`** (bool)
  Indicates whether the API key is currently active and usable. Derived from `IsValid` and `IsExpired`.

- **`AllowedIps`** (string?, nullable)
  Comma-separated list of allowed IP addresses or CIDR ranges. If null or empty, all IPs are permitted.

- **`AllowedScopes`** (string?, nullable)
  Comma-separated list of allowed permission scopes. If null or empty, no scopes are restricted.

- **`ApiCallsCount`** (long)
  Total number of API calls made using this key. Incremented via `RecordUsage`.

- **`Description`** (string?, nullable)
  Optional descriptive text providing context or purpose for the API key.

- **`IsValid`** (bool)
  Indicates whether the API key is structurally valid (e.g., not malformed or revoked). Does not consider expiration.

- **`IsExpired`** (bool)
  Indicates whether the API key has passed its `ExpiresAt` timestamp, if set.

- **`GetDaysUntilExpiration`** (int?, nullable)
  Returns the number of days remaining until the key expires. Returns null if the key does not expire or has already expired.

### Methods

- **`RecordUsage()`** (void)
  Records a usage event for the API key. Increments `ApiCallsCount`, updates `LastUsedAt` to the current UTC time, and marks the key as active. No parameters or return value. Does not throw under normal operation.

## Usage
