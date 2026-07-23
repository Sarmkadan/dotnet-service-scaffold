// tests/dotnet-service-scaffold.Tests/ResponseFormatterFactoryTests.cs
namespace DotnetServiceScaffold.Tests.Infrastructure.Formatting
{
    using Xunit;
    using System.Collections.Generic;
    using System.Linq;
    using DotnetServiceScaffold.Infrastructure.Formatting;

    public class ResponseFormatterFactoryTests
    {
        [Fact]
        public void Constructor_WithNoParameters_CreatesDefaultFormatter()
        {
            // Arrange
            var factory = new ResponseFormatterFactory();

            // Act
            var formatter = factory.GetFormatter(null);

            // Assert
            Assert.NotNull(formatter);
            Assert.IsType<JsonResponseFormatter>(formatter);
        }

        [Fact]
        public void GetFormatter_WithNullMediaType_ReturnsDefaultFormatter()
        {
            // Arrange
            var factory = new ResponseFormatterFactory();

            // Act
            var formatter = factory.GetFormatter(null);

            // Assert
            Assert.NotNull(formatter);
            Assert.IsType<JsonResponseFormatter>(formatter);
        }

        [Fact]
        public void GetFormatter_WithEmptyMediaType_ReturnsDefaultFormatter()
        {
            // Arrange
            var factory = new ResponseFormatterFactory();

            // Act
            var formatter = factory.GetFormatter(string.Empty);

            // Assert
            Assert.NotNull(formatter);
            Assert.IsType<JsonResponseFormatter>(formatter);
        }

        [Fact]
        public void GetFormatter_WithSupportedMediaType_ReturnsCorrectFormatter()
        {
            // Arrange
            var factory = new ResponseFormatterFactory();

            // Act
            var formatter = factory.GetFormatter("application/json");

            // Assert
            Assert.NotNull(formatter);
            Assert.IsType<JsonResponseFormatter>(formatter);
        }

        [Fact]
        public void GetFormatter_WithUnsupportedMediaType_ReturnsDefaultFormatter()
        {
            // Arrange
            var factory = new ResponseFormatterFactory();

            // Act
            var formatter = factory.GetFormatter("text/plain");

            // Assert
            Assert.NotNull(formatter);
            Assert.IsType<JsonResponseFormatter>(formatter);
        }

        [Fact]
        public void RegisterFormatter_WithNullMediaType_ThrowsArgumentException()
        {
            // Arrange
            var factory = new ResponseFormatterFactory();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => factory.RegisterFormatter(null, new JsonResponseFormatter()));
        }

        [Fact]
        public void RegisterFormatter_WithEmptyMediaType_ThrowsArgumentException()
        {
            // Arrange
            var factory = new ResponseFormatterFactory();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => factory.RegisterFormatter(string.Empty, new JsonResponseFormatter()));
        }

        [Fact]
        public void RegisterFormatter_WithSupportedMediaType_RegistersCorrectFormatter()
        {
            // Arrange
            var factory = new ResponseFormatterFactory();

            // Act
            factory.RegisterFormatter("application/json", new JsonResponseFormatter());

            // Assert
            Assert.Contains(factory.GetSupportedMediaTypes(), m => m == "application/json");
        }

        [Fact]
        public void GetSupportedMediaTypes_ReturnsListOfSupportedMediaTypes()
        {
            // Arrange
            var factory = new ResponseFormatterFactory();

            // Act
            var mediaTypes = factory.GetSupportedMediaTypes();

            // Assert
            Assert.NotNull(mediaTypes);
            Assert.Contains(mediaTypes, m => m == "application/json");
            Assert.Contains(mediaTypes, m => m == "text/csv");
        }

        [Fact]
        public void IsMediaTypeSupported_WithSupportedMediaType_ReturnsTrue()
        {
            // Arrange
            var factory = new ResponseFormatterFactory();

            // Act
            var supported = factory.IsMediaTypeSupported("application/json");

            // Assert
            Assert.True(supported);
        }

        [Fact]
        public void IsMediaTypeSupported_WithUnsupportedMediaType_ReturnsFalse()
        {
            // Arrange
            var factory = new ResponseFormatterFactory();

            // Act
            var supported = factory.IsMediaTypeSupported("text/plain");

            // Assert
            Assert.False(supported);
        }
    }
}
