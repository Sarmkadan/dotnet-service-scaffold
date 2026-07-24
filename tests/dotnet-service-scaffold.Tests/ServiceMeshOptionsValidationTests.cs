using Xunit;
using DotnetServiceScaffold.Infrastructure.ServiceMesh;

namespace DotnetServiceScaffold.Tests;

public class ServiceMeshOptionsValidationTests
{
    [Fact]
    public void Validate_ReturnsEmptyList_ForValidConfiguration()
    {
        // Arrange
        var options = new ServiceMeshOptions
        {
            AdminEndpoint = "http://localhost:15000",
            ReadinessTimeoutSeconds = 30,
            MeshName = "production-mesh"
        };

        // Act
        var result = options.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_ReturnsErrors_ForInvalidAdminEndpoint()
    {
        // Arrange
        var options = new ServiceMeshOptions
        {
            AdminEndpoint = "ftp://invalid.com/", // Wrong scheme and trailing slash
            ReadinessTimeoutSeconds = 30,
            MeshName = "mesh"
        };

        // Act
        var result = options.Validate();

        // Assert
        Assert.Contains("ServiceMesh.AdminEndpoint must use http:// or https:// scheme.", result);
        Assert.Contains("ServiceMesh.AdminEndpoint must not end with a trailing slash.", result);
    }

    [Fact]
    public void Validate_ReturnsErrors_ForBoundaryValues()
    {
        // Arrange
        var options = new ServiceMeshOptions
        {
            AdminEndpoint = "http://localhost:15000",
            ReadinessTimeoutSeconds = 61, // > 60
            MeshName = new string('a', 51) // > 50
        };

        // Act
        var result = options.Validate();

        // Assert
        Assert.Contains("ServiceMesh.ReadinessTimeoutSeconds should not exceed 60 seconds", result);
        Assert.Contains("ServiceMesh.MeshName must be 50 characters or less.", result);
    }

    [Fact]
    public void Validate_ReturnsErrors_ForWhitespaceInMeshName()
    {
        // Arrange
        var options = new ServiceMeshOptions
        {
            AdminEndpoint = "http://localhost:15000",
            ReadinessTimeoutSeconds = 30,
            MeshName = "mesh with spaces"
        };

        // Act
        var result = options.Validate();

        // Assert
        Assert.Contains("ServiceMesh.MeshName must not contain whitespace characters other than hyphens.", result);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenValidationPasses()
    {
        // Arrange
        var options = new ServiceMeshOptions
        {
            AdminEndpoint = "https://example.com",
            ReadinessTimeoutSeconds = 10,
            MeshName = "valid"
        };

        // Act
        var isValid = options.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenValidationFails()
    {
        // Arrange
        var options = new ServiceMeshOptions
        {
            AdminEndpoint = "", // Invalid
            ReadinessTimeoutSeconds = 10,
            MeshName = "valid"
        };

        // Act
        var isValid = options.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange
        ServiceMeshOptions? options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => options!.EnsureValid());
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_WhenOptionsAreInvalid()
    {
        // Arrange
        var options = new ServiceMeshOptions
        {
            AdminEndpoint = "invalid",
            ReadinessTimeoutSeconds = -1,
            MeshName = "   "
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => options.EnsureValid());
        Assert.Contains("ServiceMeshOptions validation failed", ex.Message);
    }
}
