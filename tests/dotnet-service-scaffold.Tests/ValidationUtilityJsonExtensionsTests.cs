using Xunit;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotnetServiceScaffold.Shared.Utilities;

namespace DotnetServiceScaffold.Tests
{
    public class ValidationUtilityJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var validationResult = "Validation result";

            // Act
            var json = ValidationUtilityJsonExtensions.ToJson(validationResult);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("Message", json);
            Assert.Contains(validationResult, json);
        }

        [Fact]
        public void ToJson_Indented_ReturnsIndentedJsonString()
        {
            // Arrange
            var validationResult = "Validation result";

            // Act
            var json = ValidationUtilityJsonExtensions.ToJson(validationResult, true);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("Message", json);
            Assert.Contains(validationResult, json);
            Assert.Contains("\n", json);
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsValidationResult()
        {
            // Arrange
            var json = "{\"Message\":\"Validation result\"}";

            // Act
            var validationResult = ValidationUtilityJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(validationResult);
            Assert.Equal("Validation result", validationResult);
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => ValidationUtilityJsonExtensions.FromJson(null));
        }

        [Fact]
        public void FromJson_InvalidJson_ReturnsNull()
        {
            // Arrange
            var json = "Invalid json";

            // Act
            var validationResult = ValidationUtilityJsonExtensions.FromJson(json);

            // Assert
            Assert.Null(validationResult);
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrueAndValidationResult()
        {
            // Arrange
            var json = "{\"Message\":\"Validation result\"}";

            // Act
            var success = ValidationUtilityJsonExtensions.TryFromJson(json, out var validationResult);

            // Assert
            Assert.True(success);
            Assert.NotNull(validationResult);
            Assert.Equal("Validation result", validationResult);
        }

        [Fact]
        public void TryFromJson_NullJson_ReturnsFalseAndNull()
        {
            // Act
            var success = ValidationUtilityJsonExtensions.TryFromJson(null, out var validationResult);

            // Assert
            Assert.False(success);
            Assert.Null(validationResult);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "Invalid json";

            // Act
            var success = ValidationUtilityJsonExtensions.TryFromJson(json, out var validationResult);

            // Assert
            Assert.False(success);
            Assert.Null(validationResult);
        }
    }
}
