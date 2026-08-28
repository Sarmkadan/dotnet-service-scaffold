using System;
using System.Collections.Generic;
using System.Text.Json;
using DotnetServiceScaffold.Infrastructure.Logging;
using Xunit;

namespace DotnetServiceScaffold.Tests.Logging
{
    public class LogContextServiceJsonExtensionsTests : ILogContextServiceJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithValidService_ReturnsCorrectJson()
        {
            // Arrange
            var service = new LogContextService();
            service.AddProperty("stringKey", "value");
            service.AddProperty("intKey", 42);

            // Act
            var json = service.ToJson();

            // Assert
            var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
            Assert.NotNull(dict);
            Assert.Equal(2, dict!.Count);
            Assert.Equal("value", dict["stringKey"]?.ToString());
            // JsonSerializer deserializes numbers as JsonElement by default, so we need to handle that
            if (dict["intKey"] is JsonElement element && element.ValueKind == JsonValueKind.Number)
            {
                Assert.Equal(42, element.GetInt32());
            }
            else
            {
                Assert.Equal(42, Convert.ToInt32(dict["intKey"]));
            }
        }

        [Fact]
        public void ToJson_WithIndentation_ProducesIndentedJson()
        {
            // Arrange
            var service = new LogContextService();
            service.AddProperty("key", "value");

            // Act
            var json = service.ToJson(indented: true);

            // Assert
            Assert.Contains("\n", json); // indented JSON contains line breaks
        }

        [Fact]
        public void ToJson_NullService_ThrowsArgumentNullException()
        {
            // Arrange
            LogContextService? service = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => service!.ToJson());
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsServiceWithProperties()
        {
            // Arrange
            var original = new LogContextService();
            original.AddProperty("a", "alpha");
            original.AddProperty("b", 123);
            var json = original.ToJson();

            // Act
            var result = LogContextServiceJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
            var props = result!.GetProperties();
            Assert.Equal(2, props.Count);
            Assert.Equal("alpha", props["a"]?.ToString());

            if (props["b"] is JsonElement element && element.ValueKind == JsonValueKind.Number)
            {
                Assert.Equal(123, element.GetInt32());
            }
            else
            {
                Assert.Equal(123, Convert.ToInt32(props["b"]));
            }
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => LogContextServiceJsonExtensions.FromJson(json!));
        }

        [Fact]
        public void FromJson_EmptyJson_ReturnsNull()
        {
            // Arrange
            var json = "   ";

            // Act
            var result = LogContextServiceJsonExtensions.FromJson(json);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var json = "{ invalid json }";

            // Act & Assert
            Assert.Throws<JsonException>(() => LogContextServiceJsonExtensions.FromJson(json));
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndService()
        {
            // Arrange
            var service = new LogContextService();
            service.AddProperty("x", "y");
            var json = service.ToJson();

            // Act
            var success = LogContextServiceJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
            var props = result!.GetProperties();
            Assert.Single(props);
            Assert.Equal("y", props["x"]?.ToString());
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            // Arrange
            var json = "{ not: valid }";

            // Act
            var success = LogContextServiceJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => LogContextServiceJsonExtensions.TryFromJson(json!, out _));
        }

        [Fact]
        public void TryFromJson_EmptyJson_ReturnsFalse()
        {
            // Arrange
            var json = "";

            // Act
            var success = LogContextServiceJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }
    }
}
