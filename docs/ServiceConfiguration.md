# ServiceConfiguration

Represents a configuration entry for a service, storing key-value pairs with metadata for encryption, system use, and validation. Used to manage service settings, track changes, and provide typed access to configuration values.

## API

### `Id`
A unique identifier for the configuration entry. Read-only; assigned at creation.

### `Key`
The configuration key (required). Must be non-null and non-empty.

### `Value`
The configuration value (required). Must be non-null and non-empty.

### `ConfigType`
Optional category or type for the configuration entry. Used for grouping or filtering.

### `ServiceId`
Optional identifier of the service this configuration belongs to. Null if global.

### `Service`
Optional navigation property to the associated `ServiceRegistration` instance.

### `IsEncrypted`
Indicates whether the `Value` is encrypted. Affects how values are stored and retrieved.

### `IsSystemConfig`
Indicates whether the configuration is managed by the system. System configurations may have restricted editing.

### `Description`
Optional human-readable description of the configuration entry.

### `CreatedAt`
Timestamp of when the configuration entry was created. Read-only.

### `UpdatedAt`
Timestamp of the last update to the configuration entry. Updated automatically on changes.

### `UpdatedByUserId`
Optional identifier of the user who last updated the configuration.

### `GetIntValue`
