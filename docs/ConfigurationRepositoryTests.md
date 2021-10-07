# ConfigurationRepositoryTests

The `ConfigurationRepositoryTests` class serves as the comprehensive test suite for validating the behavior of the configuration repository implementation within the `dotnet-service-scaffold` project. It verifies that CRUD operations (Create, Read, Update, Delete) interact correctly with the underlying data store, ensuring that configurations are persisted, retrieved by ID or key, updated, and deleted as expected, while also confirming correct handling of scenarios where requested entities do not exist.

## API

### `public ConfigurationRepositoryTests`
Initializes a new instance of the `ConfigurationRepositoryTests` class. This constructor sets up the necessary test context, including in-memory database contexts or mocked dependencies required to isolate repository tests from external infrastructure.

### `public async Task AddConfigurationAsync_ShouldAddConfigurationToDatabase`
Verifies that invoking the add operation successfully persists a new configuration entity to the database.
*   **Parameters**: None (test context is initialized internally).
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an assertion exception if the configuration is not found in the database after the operation or if the entity state is not correctly marked as added.

### `public async Task GetConfigurationByIdAsync_ShouldReturnConfiguration_WhenConfigurationExists`
Validates that requesting a configuration by its unique identifier returns the correct entity when it exists in the data store.
*   **Parameters**: None (test data is seeded internally).
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an assertion exception if the returned entity is null or if the properties of the returned entity do not match the seeded data.

### `public async Task GetConfigurationByIdAsync_ShouldReturnNull_WhenConfigurationDoesNotExist`
Ensures that requesting a configuration by an identifier that does not exist in the database returns `null` rather than throwing an error.
*   **Parameters**: None (uses a non-existent ID generated internally).
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an assertion exception if the result is not `null`.

### `public async Task GetConfigurationByKeyAsync_ShouldReturnConfiguration_WhenConfigurationExists`
Validates that requesting a configuration by its unique string key returns the correct entity when it exists in the data store.
*   **Parameters**: None (test data is seeded internally).
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an assertion exception if the returned entity is null or if the key of the returned entity does not match the query.

### `public async Task GetConfigurationByKeyAsync_ShouldReturnNull_WhenConfigurationDoesNotExist`
Ensures that requesting a configuration by a key that does not exist in the database returns `null`.
*   **Parameters**: None (uses a non-existent key generated internally).
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an assertion exception if the result is not `null`.

### `public async Task UpdateConfigurationAsync_ShouldUpdateConfigurationInDatabase`
Confirms that modifications made to an existing configuration entity are correctly persisted to the database.
*   **Parameters**: None (seeds an entity, modifies it, and saves).
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an assertion exception if the database values do not reflect the updated properties after the save operation.

### `public async Task DeleteConfigurationAsync_ShouldRemoveConfigurationFromDatabase`
Verifies that deleting an existing configuration removes the entity from the data store so it can no longer be retrieved.
*   **Parameters**: None (seeds an entity, deletes it, and verifies absence).
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an assertion exception if the entity still exists in the database after the delete operation.

## Usage

### Example 1: Running the Test Suite via CLI
To execute all configuration repository tests included in this class using the .NET CLI, navigate to the test project directory and run the following command. This will discover and run all methods marked with test attributes within `ConfigurationRepositoryTests`.

```bash
dotnet test --filter "FullyQualifiedName~ConfigurationRepositoryTests"
```

### Example 2: Extending Test Scenarios
Developers can extend this class to cover additional edge cases by adding new async test methods that follow the existing naming convention and pattern. Below is an example of how a new test might be structured within the class context.

```csharp
[Fact]
public async Task GetConfigurationByKeyAsync_ShouldBeCaseSensitive_WhenKeyDiffers()
{
    // Arrange
    var existingKey = "AppSettings:Database";
    var variantKey = "appsettings:database";
    await SeedConfigurationAsync(existingKey);

    // Act
    var result = await Repository.GetConfigurationByKeyAsync(variantKey);

    // Assert
    Assert.Null(result);
}
```

## Notes

*   **Execution Order**: As standard for xUnit/NUnit test classes, the execution order of methods within `ConfigurationRepositoryTests` is not guaranteed. Each test method must be fully isolated, setting up its own preconditions and tearing down its own state to prevent side effects between tests like `AddConfigurationAsync_ShouldAddConfigurationToDatabase` and `DeleteConfigurationAsync_ShouldRemoveConfigurationFromDatabase`.
*   **Asynchronous Nature**: All test members are asynchronous (`async Task`). Callers or test runners must await these tasks properly; failing to await them may result in tests passing falsely because assertions execute before the database operations complete.
*   **Null Handling**: The API explicitly tests for `null` returns in "not found" scenarios (`GetConfigurationByIdAsync_ShouldReturnNull_WhenConfigurationDoesNotExist` and `GetConfigurationByKeyAsync_ShouldReturnNull_WhenConfigurationDoesNotExist`). Implementations relying on this repository should not expect exceptions to be thrown for missing data, but rather handle null results gracefully.
*   **Thread Safety**: Test classes in this suite are typically instantiated once per test method by the test framework, ensuring instance-level isolation. However, if the underlying repository uses a shared static context or singleton database instance without proper locking, race conditions could occur if tests are run in parallel. Ensure the test infrastructure configures parallel execution limits appropriately if the underlying data store is not thread-safe.
