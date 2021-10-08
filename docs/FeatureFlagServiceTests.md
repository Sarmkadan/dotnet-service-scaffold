# FeatureFlagServiceTests

Unit tests for the `FeatureFlagService` class, verifying behavior around feature flag management including enabling/disabling features, rollout percentage configuration, and feature registration.

## API

### `FeatureFlagServiceTests()`

Constructor for the test class. Initializes a fresh instance of `FeatureFlagService` with default configuration before each test.

### `void IsEnabled_ShouldReturnTrue_WhenFeatureIsEnabled()`

Verifies that `IsEnabled` returns `true` when a feature is explicitly enabled with a rollout percentage of 100.

**Parameters:** None
**Return value:** None
**Throws:** None

### `void IsEnabled_ShouldReturnFalse_WhenFeatureIsDisabled()`

Verifies that `IsEnabled` returns `false` when a feature is explicitly disabled.

**Parameters:** None
**Return value:** None
**Throws:** None

### `void IsEnabled_ShouldReturnFalse_WhenFeatureNotFound()`

Verifies that `IsEnabled` returns `false` when querying for a non-existent feature.

**Parameters:** None
**Return value:** None
**Throws:** None

### `void EnableFeature_ShouldSetFeatureToEnabled()`

Verifies that calling `EnableFeature` sets the specified feature’s state to enabled.

**Parameters:** None
**Return value:** None
**Throws:** None

### `void DisableFeature_ShouldSetFeatureToDisabled()`

Verifies that calling `DisableFeature` sets the specified feature’s state to disabled.

**Parameters:** None
**Return value:** None
**Throws:** None

### `void SetRolloutPercentage_ShouldUpdatePercentage()`

Verifies that `SetRolloutPercentage` updates the rollout percentage for a given feature.

**Parameters:** None
**Return value:** None
**Throws:** None

### `void SetRolloutPercentage_ShouldThrowArgumentException_ForInvalidPercentage()`

Verifies that `SetRolloutPercentage` throws an `ArgumentException` when provided an invalid percentage (outside the range 0–100).

**Parameters:** None
**Return value:** None
**Throws:** `ArgumentException` when percentage is < 0 or > 100

### `void RegisterFeature_ShouldAddNewFeature()`

Verifies that `RegisterFeature` adds a new feature to the service with default disabled state.

**Parameters:** None
**Return value:** None
**Throws:** None

### `void GetAllFlags_ShouldReturnAllRegisteredFlags()`

Verifies that `GetAllFlags` returns a collection containing all registered features.

**Parameters:** None
**Return value:** None
**Throws:** None

## Usage
