using Xunit;
using System;
using System.Collections.Generic;
using DotnetServiceScaffold.Domain.Exceptions;

namespace DotnetServiceScaffold.Tests.Domain.Exceptions;

public class ServiceScaffoldExceptionTests
{
    [Fact]
    public void ServiceScaffoldException_Constructor_WithMessageAndErrorCode_SetsProperties()
    {
        // Arrange
        var expectedMessage = "Test error";
        var expectedCode = "TEST_CODE";

        // Act
        var exception = new ServiceScaffoldException(expectedMessage, expectedCode);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
        Assert.Equal(expectedCode, exception.ErrorCode);
    }

    [Fact]
    public void ServiceScaffoldException_Constructor_WithInnerException_PreservesInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var expectedMessage = "Outer error";
        var expectedCode = "OUTER_CODE";

        // Act
        var exception = new ServiceScaffoldException(expectedMessage, expectedCode, innerException);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
        Assert.Equal(expectedCode, exception.ErrorCode);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void ServiceNotFoundException_Constructor_WithGuid_SetsMessageAndErrorCode()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        // Act
        var exception = new ServiceNotFoundException(serviceId);

        // Assert
        Assert.Contains(serviceId.ToString(), exception.Message);
        Assert.Equal("SERVICE_NOT_FOUND", exception.ErrorCode);
    }

    [Fact]
    public void ServiceNotFoundException_Constructor_WithString_SetsMessageAndErrorCode()
    {
        // Arrange
        var serviceName = "TestService";

        // Act
        var exception = new ServiceNotFoundException(serviceName);

        // Assert
        Assert.Contains(serviceName, exception.Message);
        Assert.Equal("SERVICE_NOT_FOUND", exception.ErrorCode);
    }

    [Fact]
    public void ServiceValidationException_Constructor_WithMessage_SetsErrorsList()
    {
        // Arrange
        var errorMessage = "Validation failed";

        // Act
        var exception = new ServiceValidationException(errorMessage);

        // Assert
        Assert.Equal("VALIDATION_ERROR", exception.ErrorCode);
        Assert.Single(exception.Errors);
        Assert.Contains(errorMessage, exception.Errors);
    }

    [Fact]
    public void ServiceValidationException_Constructor_WithErrorsList_SetsErrorsList()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2" };

        // Act
        var exception = new ServiceValidationException(errors);

        // Assert
        Assert.Equal("VALIDATION_ERROR", exception.ErrorCode);
        Assert.Equal(2, exception.Errors.Count);
        Assert.Equal(errors, exception.Errors);
    }
}
