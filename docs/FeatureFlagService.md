# FeatureFlagService

The `FeatureFlagService` provides runtime feature flag management for enabling or disabling features based on user targeting, rollout percentages, and explicit enablement. It supports dynamic configuration of feature states, audit tracking via timestamps, and retrieval of detailed flag information.

## API

### `public FeatureFlagService`

Constructs a new instance of the feature flag service. The service is initialized with no flags by default and requires explicit registration of features via `RegisterFeature`.

### `public bool IsEnabled(string featureName)`

Determines whether a feature is enabled globally, regardless of user targeting or rollout percentage.

- **Parameters**
  - `featureName`: The name of the feature to check.
- **Returns**
  - `true` if the feature is enabled globally; otherwise, `false`.
- **Throws**
  - `ArgumentNullException` if `featureName` is `null`.
  - `KeyNotFoundException` if the feature is not registered.

### `public bool IsEnabledForUser(string featureName, string userId)`

Determines whether a feature is enabled for a specific user, considering both global enablement and rollout percentage.

- **Parameters**
  - `featureName`: The name of the feature to check.
  - `userId`: The unique identifier of the user.
- **Returns**
  - `true` if the feature is enabled for the user; otherwise, `false`.
- **Throws**
  - `ArgumentNullException` if `featureName` or `userId` is `null`.
  - `KeyNotFoundException` if the feature is not registered.

### `public void EnableFeature(string featureName)`

Enables a feature globally, overriding any rollout percentage or user targeting.

- **Parameters**
  - `featureName`: The name of the feature to enable.
- **Throws**
  - `ArgumentNullException` if `featureName` is `null`.
  - `KeyNotFoundException` if the feature is not registered.

### `public void DisableFeature(string featureName)`

Disables a feature globally, overriding any rollout percentage or user targeting.

- **Parameters**
  - `featureName`: The name of the feature to disable.
- **Throws**
  - `ArgumentNullException` if `featureName` is `null`.
  - `KeyNotFoundException` if the feature is not registered.

### `public void SetRolloutPercentage(string featureName, int percentage)`

Sets the percentage of users for whom the feature should be enabled via rollout.

- **Parameters**
  - `featureName`: The name of the feature.
  - `percentage`: The rollout percentage (0–100).
- **Throws**
  - `ArgumentNullException` if `featureName` is `null`.
  - `ArgumentOutOfRangeException` if `percentage` is not between 0 and 100.
  - `KeyNotFoundException` if the feature is not registered.

### `public void RegisterFeature(FeatureFlag flag)`

Registers a new feature flag with the service. The flag is created with the provided name, description, initial enablement state, and rollout percentage. The `CreatedAt` and `LastModified` timestamps are set to the current UTC time.

- **Parameters**
  - `flag`: The feature flag to register.
- **Throws**
  - `ArgumentNullException` if `flag` or `flag.Name` is `null`.
  - `ArgumentException` if `flag.Name` is empty or whitespace.
  - `InvalidOperationException` if a feature with the same name is already registered.

### `public IEnumerable<FeatureFlagInfo> GetAllFlags()`

Retrieves all registered feature flags as lightweight information objects.

- **Returns**
  - An enumerable of `FeatureFlagInfo` objects containing name, description, enablement state, rollout percentage, and timestamps.
- **Throws**
  - None.

### `public FeatureFlagInfo? GetFlag(string featureName)`

Retrieves detailed information about a specific feature flag.

- **Parameters**
  - `featureName`: The name of the feature.
- **Returns**
  - A `FeatureFlagInfo` object if the feature exists; otherwise, `null`.
- **Throws**
  - `ArgumentNullException` if `featureName` is `null`.

## Usage

### Example 1: Basic Feature Flag Management
