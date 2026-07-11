# HealthCheckServiceTests

`HealthCheckServiceTests` is the unit test class for the `HealthCheckService` component within the `dotnet-service-scaffold` project. It validates the core behaviors of health check execution, history retrieval, and result recording, ensuring that the service correctly reports aggregate health status, manages historical records, and delegates persistence operations to the underlying repository.

## API

### HealthCheckServiceTests

Default parameterless constructor. Initializes the test class and any shared test infrastructure (such as mock repositories, service instances under test, or fixture data) required by the individual test methods. No explicit parameters or return value.

### async Task PerformHealthCheckAsync_ShouldReturnHealthy_WhenAllComponentsAreHealthy

**Purpose:** Verifies that the service’s health check execution returns a healthy status when every monitored component reports a healthy state.

**Parameters:** None (parameterless test method).

**Return Value:** A completed `Task` representing the asynchronous test operation. Assertions within the method confirm that the result status is “Healthy” and that no unhealthy components are present.

**Throws:** Relies on test assertion failures (e.g., via `Assert` or equivalent) if the actual status differs from the expected healthy result. Does not throw expected exceptions under normal validation.

### async Task PerformHealthCheckAsync_ShouldReturnUnhealthy_WhenAnyComponentIsUnhealthy

**Purpose:** Confirms that the aggregate health status becomes unhealthy when at least one component reports a degraded or failing state, even if other components remain healthy.

**Parameters:** None.

**Return Value:** A completed `Task`. Assertions verify that the returned status is “Unhealthy” and that the failing component details are included in the result.

**Throws:** Assertion failures if the service incorrectly returns a healthy status when unhealthy components exist.

### async Task GetHealthCheckHistoryAsync_ShouldReturnAllHistory

**Purpose:** Ensures that requesting health check history returns all previously recorded entries when history data is present.

**Parameters:** None.

**Return Value:** A completed `Task`. Assertions validate that the returned collection contains the expected number of entries and matches the recorded data.

**Throws:** Assertion failures if the history count is incorrect or entries are missing.

### async Task GetHealthCheckHistoryAsync_ShouldReturnEmpty_WhenNoHistoryExists

**Purpose:** Validates that the history retrieval method returns an empty collection when no health check results have been recorded.

**Parameters:** None.

**Return Value:** A completed `Task`. Assertions confirm that the result is an empty collection (not null) and contains zero entries.

**Throws:** Assertion failures if the result is null or contains unexpected entries.

### async Task RecordHealthCheckResultAsync_ShouldCallRepositoryAdd

**Purpose:** Tests that recording a health check result correctly delegates the persistence operation to the repository’s add method exactly once.

**Parameters:** None.

**Return Value:** A completed `Task`. Assertions verify that the repository’s `Add` method was invoked with the expected health check result object.

**Throws:** Assertion failures if the repository method is not called, is called multiple times, or receives an incorrect argument.

### async Task RecordHealthCheckResultAsync_ShouldSetTimestamp

**Purpose:** Confirms that when a health check result is recorded, the service assigns a timestamp to the result object before persisting it.

**Parameters:** None.

**Return Value:** A completed `Task`. Assertions check that the timestamp property on the recorded result is set to a non-default value close to the current time.

**Throws:** Assertion failures if the timestamp is default (unset) or significantly diverges from the expected recording time.

## Usage

```csharp
// Example 1: Testing healthy scenario with mocked healthy components
[Test]
public async Task PerformHealthCheckAsync_AllHealthy_ReturnsHealthyStatus()
{
    // Arrange
    var healthyComponents = new List<IHealthComponent>
    {
        Mock.Of<IHealthComponent>(c => c.CheckHealthAsync() == Task.FromResult(HealthStatus.Healthy)),
        Mock.Of<IHealthComponent>(c => c.CheckHealthAsync() == Task.FromResult(HealthStatus.Healthy))
    };
    var service = new HealthCheckService(healthyComponents, mockRepository.Object);

    // Act
    var result = await service.PerformHealthCheckAsync();

    // Assert
    Assert.AreEqual(HealthStatus.Healthy, result.Status);
    Assert.IsEmpty(result.UnhealthyComponents);
}
```

```csharp
// Example 2: Verifying history retrieval returns persisted entries
[Test]
public async Task GetHealthCheckHistoryAsync_WithExistingRecords_ReturnsAllEntries()
{
    // Arrange
    var storedResults = new List<HealthCheckResult>
    {
        new HealthCheckResult { Status = HealthStatus.Healthy, Timestamp = DateTime.UtcNow.AddHours(-2) },
        new HealthCheckResult { Status = HealthStatus.Unhealthy, Timestamp = DateTime.UtcNow.AddHours(-1) }
    };
    mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(storedResults);
    var service = new HealthCheckService(components, mockRepository.Object);

    // Act
    var history = await service.GetHealthCheckHistoryAsync();

    // Assert
    Assert.AreEqual(2, history.Count());
    mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
}
```

## Notes

- **Test isolation:** Each test method operates independently and should not rely on shared mutable state across tests. Mock setups and service instances are typically recreated per test to avoid cross-test contamination.
- **Timestamp precision:** The `RecordHealthCheckResultAsync_ShouldSetTimestamp` test should account for minor clock skew by using approximate time comparisons (e.g., within a few seconds tolerance) rather than exact equality.
- **Repository interaction:** Tests that verify repository calls assume a mocked repository. The actual persistence behavior (transactionality, concurrency handling) is the responsibility of the repository implementation and is not covered by these service-level tests.
- **Empty history vs. null:** The `GetHealthCheckHistoryAsync_ShouldReturnEmpty_WhenNoHistoryExists` test explicitly distinguishes between a null return and an empty collection. Implementations must return an empty enumerable, not null, when no records exist.
- **Thread safety:** These tests validate logical behavior in a single-threaded test context. They do not exercise concurrent calls to the service methods. Any thread-safety guarantees must be verified separately if required by the production environment.
