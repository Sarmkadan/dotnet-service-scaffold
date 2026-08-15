using DotnetServiceScaffold.Domain.Models;
using Xunit;

namespace DotnetServiceScaffold.Tests
{
    public class UpstreamClusterValidationTests
    {
        [Fact]
        public void Validate_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var cluster = new UpstreamCluster { Name = "Test", Endpoint = "https://example.com" };

            // Act
            var result = UpstreamClusterValidation.Validate(cluster);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => UpstreamClusterValidation.Validate(null));
        }

        [Fact]
        public void IsValid_HappyPath_ReturnsTrue()
        {
            // Arrange
            var cluster = new UpstreamCluster { Name = "Test", Endpoint = "https://example.com" };

            // Act
            var result = UpstreamClusterValidation.IsValid(cluster);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_InvalidInput_ReturnsFalse()
        {
            // Arrange
            var cluster = new UpstreamCluster { Name = string.Empty, Endpoint = string.Empty };

            // Act
            var result = UpstreamClusterValidation.IsValid(cluster);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            // Arrange
            var cluster = new UpstreamCluster { Name = "Test", Endpoint = "https://example.com" };

            // Act and Assert
            UpstreamClusterValidation.EnsureValid(cluster);
        }

        [Fact]
        public void EnsureValid_InvalidInput_ThrowsArgumentException()
        {
            // Arrange
            var cluster = new UpstreamCluster { Name = string.Empty, Endpoint = string.Empty };

            // Act and Assert
            Assert.Throws<ArgumentException>(() => UpstreamClusterValidation.EnsureValid(cluster));
        }

        [Fact]
        public void EnsureValid_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => UpstreamClusterValidation.EnsureValid(null));
        }
    }
}
