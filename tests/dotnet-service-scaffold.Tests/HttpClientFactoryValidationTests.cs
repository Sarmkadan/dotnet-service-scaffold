using System;
using Xunit;
using DotnetServiceScaffold.Infrastructure.Integration;

namespace DotnetServiceScaffold.Tests
{
    public class HttpClientFactoryValidationTests
    {
        #region CreateClient

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void EnsureValidCreateClient_ThrowsWhenNameIsNullOrWhiteSpace(string? name)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => HttpClientFactoryValidation.EnsureValidCreateClient(name));
            Assert.Contains("Client name cannot be null, empty, or whitespace.", exception.Message);
        }

        [Fact]
        public void EnsureValidCreateClient_DoesNotThrowWhenNameIsValid()
        {
            // Arrange
            var validName = "MyClient";

            // Act & Assert
            var exception = Record.Exception(() => HttpClientFactoryValidation.EnsureValidCreateClient(validName));
            Assert.Null(exception);
        }

        #endregion

        #region CreateClientWithBaseUrl

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void EnsureValidCreateClientWithBaseUrl_ThrowsWhenBaseUrlIsNullOrWhiteSpace(string? baseUrl)
        {
            // Arrange
            var name = "Client";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => HttpClientFactoryValidation.EnsureValidCreateClientWithBaseUrl(baseUrl, name));
            Assert.Contains("Base URL cannot be null, empty, or whitespace.", exception.Message);
        }

        [Fact]
        public void EnsureValidCreateClientWithBaseUrl_ThrowsWhenBaseUrlIsNotAbsoluteUri()
        {
            // Arrange
            var baseUrl = "relative/path";
            var name = "Client";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => HttpClientFactoryValidation.EnsureValidCreateClientWithBaseUrl(baseUrl, name));
            Assert.Contains("Base URL must be a valid absolute URI.", exception.Message);
        }

        [Fact]
        public void EnsureValidCreateClientWithBaseUrl_ThrowsWhenBaseUrlHasInvalidScheme()
        {
            // Arrange
            var baseUrl = "ftp://example.com";
            var name = "Client";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => HttpClientFactoryValidation.EnsureValidCreateClientWithBaseUrl(baseUrl, name));
            Assert.Contains("Base URL must use http:// or https:// scheme.", exception.Message);
        }

        [Fact]
        public void EnsureValidCreateClientWithBaseUrl_DoesNotThrowWhenParametersAreValid()
        {
            // Arrange
            var baseUrl = "https://example.com/api";
            var name = "Client";

            // Act & Assert
            var exception = Record.Exception(() => HttpClientFactoryValidation.EnsureValidCreateClientWithBaseUrl(baseUrl, name));
            Assert.Null(exception);
        }

        [Fact]
        public void ValidateCreateClientWithBaseUrl_ReturnsProblemsForInvalidInput()
        {
            // Arrange
            string? baseUrl = "invalid";
            string? name = "";

            // Act
            var problems = HttpClientFactoryValidation.ValidateCreateClientWithBaseUrl(baseUrl, name);

            // Assert
            Assert.NotEmpty(problems);
            Assert.Contains("Base URL must be a valid absolute URI.", problems);
            Assert.Contains("Client name cannot be null, empty, or whitespace.", problems);
        }

        [Fact]
        public void ValidateCreateClientWithBaseUrl_ReturnsEmptyForValidInput()
        {
            // Arrange
            var baseUrl = "https://example.com";
            var name = "Client";

            // Act
            var problems = HttpClientFactoryValidation.ValidateCreateClientWithBaseUrl(baseUrl, name);

            // Assert
            Assert.Empty(problems);
        }

        #endregion
    }
}
