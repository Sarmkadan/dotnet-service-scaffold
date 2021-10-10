# ServiceManagementServiceTests

The `ServiceManagementServiceTests` class serves as the comprehensive test suite for the `ServiceManagementService` component within the `dotnet-service-scaffold` project. It validates the correctness of service lifecycle operations, including registration, retrieval, enabling, disabling, and unregistration, while ensuring that appropriate exceptions are thrown under invalid conditions or when dependent entities are missing. Additionally, it verifies the accuracy of success rate calculations under various request scenarios.

## API

### Constructors

#### `public ServiceManagementServiceTests()`
Initializes a new instance of the `ServiceManagementServiceTests` class. This constructor typically sets up the necessary test fixtures, mocks, or in-memory databases required for executing the subsequent test methods.

### Test Methods

#### `public async Task RegisterServiceAsync_ShouldRegisterService_WhenInputsAreValid()`
Verifies that a service is successfully registered when all provided input parameters meet validation criteria and the owner exists.
*   **Parameters**: None (uses test fixture data).
*   **Return Value**: A `Task` that completes when the assertion passes.
*   **Throws**: Fails the test if the service is not registered or if an unexpected exception occurs.

#### `public async Task RegisterServiceAsync_ShouldThrowValidationException_WhenInputsAreInvalid()`
Ensures that a `ValidationException` (or equivalent) is thrown when attempting to register a service with malformed or missing required fields.
*   **Parameters**: None (uses test fixture data with invalid inputs).
*   **Return Value**: A `Task` that completes when the assertion passes.
*   **Throws**: Fails the test if no exception is thrown or if the thrown exception is not the expected validation type.

#### `public async Task RegisterServiceAsync_ShouldThrowException_WhenOwnerNotFound()`
Confirms that the registration process fails with a specific exception when the specified service owner does not exist in the system.
*   **Parameters**: None (uses test fixture data referencing a non-existent owner).
*   **Return Value**: A `Task` that completes when the assertion passes.
*   **Throws**: Fails the test if the operation succeeds or throws an unexpected exception type.

#### `public async Task RegisterServiceAsync_ShouldThrowValidationException_WhenServiceNameAlreadyExists()`
Validates that a `ValidationException` is raised when attempting to register a service using a name that is already associated with an existing active service.
*   **Parameters**: None (uses test fixture data with a duplicate name).
*   **Return Value**: A `Task` that completes when the assertion passes.
*   **Throws**: Fails the test if the duplicate check is bypassed or the wrong exception is thrown.

#### `public async Task GetServiceAsync_ShouldReturnService_WhenFound()`
Tests the retrieval logic to ensure it returns the correct service entity when a valid identifier is provided.
*   **Parameters**: None (uses test fixture data with an existing service ID).
*   **Return Value**: A `Task` that completes when the assertion passes.
*   **Throws**: Fails the test if the returned service is null, incorrect, or if an exception is thrown.

#### `public async Task UnregisterServiceAsync_ShouldDeleteService()`
Verifies that calling the unregistration method effectively removes the service from the storage medium or marks it as deleted.
*   **Parameters**: None (uses test fixture data).
*   **Return Value**: A `Task` that completes when the assertion passes.
*   **Throws**: Fails the test if the service persists after the operation.

#### `public async Task UnregisterServiceAsync_ShouldThrowNotFoundException_WhenServiceNotFound()`
Ensures that attempting to unregister a service that does not exist results in a `NotFoundException`.
*   **Parameters**: None (uses test fixture data with a non-existent service ID).
*   **Return Value**: A `Task` that completes when the assertion passes.
*   **Throws**: Fails the test if no exception is thrown or the exception type is incorrect.

#### `public async Task DisableServiceAsync_ShouldDisableService()`
Confirms that the disable operation successfully updates the service status to an inactive state.
*   **Parameters**: None (uses test fixture data).
*   **Return Value**: A `Task` that completes when the assertion passes.
*   **Throws**: Fails the test if the service status remains active.

#### `public async Task EnableServiceAsync_ShouldEnableService()`
Confirms that the enable operation successfully updates the service status from inactive to active.
*   **Parameters**: None (uses test fixture data).
*   **Return Value**: A `Task` that completes when the assertion passes.
*   **Throws**: Fails the test if the service status remains inactive.

#### `public async Task GetServiceSuccessRateAsync_ShouldReturnSuccessRate()`
Validates the calculation logic for service success rates based on historical request data (successes vs. failures).
*   **Parameters**: None (uses test fixture data with mixed request outcomes).
*   **Return Value**: A `Task` that completes when the assertion passes.
*   **Throws**: Fails the test if the calculated rate does not match the expected value.

#### `public async Task GetServiceSuccessRateAsync_ShouldReturn100_WhenNoRequests()`
Ensures that the success rate defaults to 100% (or the defined baseline) when the service has no recorded request history, preventing division by zero errors.
*   **Parameters**: None (uses test fixture data with zero requests).
*   **Return Value**: A `Task` that completes when the assertion passes.
*   **Throws**: Fails the test if the return value is not 100 or if a division by zero exception occurs.

## Usage

The following examples demonstrate how these tests might be structured within an xUnit test class context, assuming the use of standard mocking frameworks like Moq and assertion libraries like FluentAssertions or Xunit.Assert.

### Example 1: Testing Valid Registration and Retrieval

```csharp
public class ServiceManagementServiceTests
{
    private readonly Mock<IServiceRepository> _repositoryMock;
    private readonly ServiceManagementService _service;

    public ServiceManagementServiceTests()
    {
        _repositoryMock = new Mock<IServiceRepository>();
        _service = new ServiceManagementService(_repositoryMock.Object);
    }

    [Fact]
    public async Task RegisterServiceAsync_ShouldRegisterService_WhenInputsAreValid()
    {
        // Arrange
        var ownerExists = true;
        _repositoryMock.Setup(r => r.OwnerExistsAsync(It.IsAny<string>())).ReturnsAsync(ownerExists);
        _repositoryMock.Setup(r => r.GetServiceByNameAsync(It.IsAny<string>())).ReturnsAsync((ServiceDto)null);
        
        var request = new RegisterServiceRequest { Name = "TestService", OwnerId = "owner-123" };

        // Act
        await _service.RegisterServiceAsync(request);

        // Assert
        _repositoryMock.Verify(r => r.AddServiceAsync(It.IsAny<ServiceDto>()), Times.Once);
    }

    [Fact]
    public async Task GetServiceAsync_ShouldReturnService_WhenFound()
    {
        // Arrange
        var expectedService = new ServiceDto { Id = "svc-1", Name = "TestService" };
        _repositoryMock.Setup(r => r.GetServiceByIdAsync("svc-1")).ReturnsAsync(expectedService);

        // Act
        var result = await _service.GetServiceAsync("svc-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestService", result.Name);
    }
}
```

### Example 2: Testing Exception Scenarios and Metrics

```csharp
public class ServiceManagementServiceTests
{
    // ... setup omitted for brevity ...

    [Fact]
    public async Task RegisterServiceAsync_ShouldThrowValidationException_WhenServiceNameAlreadyExists()
    {
        // Arrange
        var existingService = new ServiceDto { Name = "DuplicateService" };
        _repositoryMock.Setup(r => r.GetServiceByNameAsync("DuplicateService")).ReturnsAsync(existingService);
        
        var request = new RegisterServiceRequest { Name = "DuplicateService", OwnerId = "owner-123" };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.RegisterServiceAsync(request));
    }

    [Fact]
    public async Task GetServiceSuccessRateAsync_ShouldReturn100_WhenNoRequests()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetRequestStatsAsync(It.IsAny<string>()))
            .ReturnsAsync(new RequestStats { TotalRequests = 0, SuccessfulRequests = 0 });

        // Act
        var successRate = await _service.GetServiceSuccessRateAsync("svc-1");

        // Assert
        Assert.Equal(100.0, successRate);
    }
}
```

## Notes

*   **Asynchronous Execution**: All public members are asynchronous (`async Task`), indicating that the underlying service operations involve I/O-bound tasks such as database queries or network calls. Test implementations must await these tasks to avoid unobserved exceptions or race conditions during test execution.
*   **Exception Specificity**: The test suite distinguishes between different failure modes (e.g., `ValidationException` vs. `NotFoundException`). Implementations of the target service must ensure that specific exception types are thrown rather than generic `Exception` types to satisfy these tests.
*   **Edge Cases in Metrics**: The `GetServiceSuccessRateAsync_ShouldReturn100_WhenNoRequests` test highlights a critical edge case where the denominator in a percentage calculation is zero. The production code must handle this explicitly to prevent runtime errors.
*   **State Isolation**: Since these tests likely share a common test class instance or static mocks, care must be taken to ensure that the state of mocks (e.g., `Setup` configurations) is reset between tests if the test runner does not instantiate a new class instance for every test method.
*   **Thread Safety**: While the test methods themselves are sequential within a single test runner context, the underlying `ServiceManagementService` being tested should be thread-safe if it is intended for concurrent use in production, although these specific tests do not explicitly perform concurrent load testing.
