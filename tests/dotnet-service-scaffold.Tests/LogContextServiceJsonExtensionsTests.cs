using System;
using System.Collections.Generic;
using System.Text.Json;
using DotnetServiceScaffold.Infrastructure.Logging;
using Xunit;

namespace DotnetServiceScaffold.Tests.Logging
{
    /// <summary>
    /// Tests for the <see cref="LogContextServiceJsonExtensions"/> class, which provides JSON serialization and deserialization extensions for <see cref="LogContextService"/>.
    /// </summary>
    public class LogContextServiceJsonExtensionsTests : ILogContextServiceJsonExtensionsTests
    {
        /// <summary>
        /// Verifies that <see cref="LogContextServiceJsonExtensions.ToJson(LogContextService)"/> serializes a <see cref="LogContextService"/> with string and integer properties to the expected JSON string.
        /// </summary>
        [Fact]
        public void ToJson_WithValidService_ReturnsCorrectJson()
        {
            // Arrange
            var service = new LogContextService();
            service.AddProperty(LogContextServiceJsonExtensionsTestsConstants.StringKey, LogContextServiceJsonExtensionsTestsConstants.TestValue);
            service.AddProperty(LogContextServiceJsonExtensionsTestsConstants.IntKey, LogContextServiceJsonExtensionsTestsConstants.TestNumberFortyTwo);

            // Act
            var json = service.ToJson();

            // Assert
            var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
            Assert.NotNull(dict);
            Assert.Equal(LogContextServiceJsonExtensionsTestsConstants.TestCountTwo, dict!.Count);
            Assert.Equal(LogContextServiceJsonExtensionsTestsConstants.TestValue, dict[LogContextServiceJsonExtensionsTestsConstants.StringKey]?.ToString());
            // JsonSerializer deserializes numbers as JsonElement by default, so we need to handle that
            if (dict[LogContextServiceJsonExtensionsTestsConstants.IntKey] is JsonElement element && element.ValueKind == JsonValueKind.Number)
            {
                Assert.Equal(LogContextServiceJsonExtensionsTestsConstants.TestNumberFortyTwo, element.GetInt32());
            }
            else
            {
                Assert.Equal(LogContextServiceJsonExtensionsTestsConstants.TestNumberFortyTwo, Convert.ToInt32(dict[LogContextServiceJsonExtensionsTestsConstants.IntKey]));
            }
        }

        /// <summary>
        /// Verifies that <see cref="LogContextServiceJsonExtensions.ToJson(LogContextService,bool)"/> with <paramref name="indented"/> set to true produces JSON containing line breaks (indentation).
        /// </summary>
        [Fact]
        public void ToJson_WithIndentation_ProducesIndentedJson()
        {
            // Arrange
            var service = new LogContextService();
            service.AddProperty(LogContextServiceJsonExtensionsTestsConstants.TestKeyForIndentation, LogContextServiceJsonExtensionsTestsConstants.TestValueForIndentation);

            // Act
            var json = service.ToJson(indented: true);

            // Assert
            Assert.Contains("\n", json); // indented JSON contains line breaks
        }

        /// <summary>
        /// Verifies that <see cref="LogContextServiceJsonExtensions.ToJson(LogContextService)"/> throws an <see cref="ArgumentNullException"/> when called on a null <see cref="LogContextService"/>.
        /// </summary>
        [Fact]
        public void ToJson_NullService_ThrowsArgumentNullException()
        {
            // Arrange
            LogContextService? service = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => service!.ToJson());
        }

        /// <summary>
        /// Verifies that <see cref="LogContextServiceJsonExtensions.FromJson(string)"/> deserializes a JSON string into a <see cref="LogContextService"/> with the expected string and integer properties.
        /// </summary>
        [Fact]
        public void FromJson_ValidJson_ReturnsServiceWithProperties()
        {
            // Arrange
            var original = new LogContextService();
            original.AddProperty(LogContextServiceJsonExtensionsTestsConstants.TestKeyA, LogContextServiceJsonExtensionsTestsConstants.TestValueAlpha);
            original.AddProperty(LogContextServiceJsonExtensionsTestsConstants.TestKeyB, LogContextServiceJsonExtensionsTestsConstants.TestNumberOneHundredTwentyThree);
            var json = original.ToJson();

            // Act
            var result = LogContextServiceJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
            var props = result!.GetProperties();
            Assert.Equal(LogContextServiceJsonExtensionsTestsConstants.TestCountTwo, props.Count);
            Assert.Equal(LogContextServiceJsonExtensionsTestsConstants.TestValueAlpha, props[LogContextServiceJsonExtensionsTestsConstants.TestKeyA]?.ToString());

            if (props[LogContextServiceJsonExtensionsTestsConstants.TestKeyB] is JsonElement element && element.ValueKind == JsonValueKind.Number)
            {
                Assert.Equal(LogContextServiceJsonExtensionsTestsConstants.TestNumberOneHundredTwentyThree, element.GetInt32());
            }
            else
            {
                Assert.Equal(LogContextServiceJsonExtensionsTestsConstants.TestNumberOneHundredTwentyThree, Convert.ToInt32(props[LogContextServiceJsonExtensionsTestsConstants.TestKeyB]));
            }
        }

        /// <summary>
        /// Verifies that <see cref="LogContextServiceJsonExtensions.FromJson(string)"/> throws an <see cref="ArgumentNullException"/> when called with a null JSON string.
        /// </summary>
        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => LogContextServiceJsonExtensions.FromJson(json!));
        }

        /// <summary>
        /// Verifies that <see cref="LogContextServiceJsonExtensions.FromJson(string)"/> returns null when given a JSON string containing only whitespace.
        /// </summary>
        [Fact]
        public void FromJson_EmptyJson_ReturnsNull()
        {
            // Arrange
            var json = LogContextServiceJsonExtensionsTestsConstants.WhitespaceJson;

            // Act
            var result = LogContextServiceJsonExtensions.FromJson(json);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that <see cref="LogContextServiceJsonExtensions.FromJson(string)"/> throws a <see cref="JsonException"/> when given invalid JSON.
        /// </summary>
        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var json = LogContextServiceJsonExtensionsTestsConstants.InvalidJson1;

            // Act & Assert
            Assert.Throws<JsonException>(() => LogContextServiceJsonExtensions.FromJson(json));
        }

        /// <summary>
        /// Verifies that <see cref="LogContextServiceJsonExtensions.TryFromJson(string,out LogContextService?)"/> returns true and populates the output <see cref="LogContextService"/> when given valid JSON.
        /// </summary>
        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndService()
        {
            // Arrange
            var service = new LogContextService();
            service.AddProperty(LogContextServiceJsonExtensionsTestsConstants.TestKeyX, LogContextServiceJsonExtensionsTestsConstants.TestKeyY);
            var json = service.ToJson();

            // Act
            var success = LogContextServiceJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
            var props = result!.GetProperties();
            Assert.Single(props);
            Assert.Equal(LogContextServiceJsonExtensionsTestsConstants.TestKeyY, props[LogContextServiceJsonExtensionsTestsConstants.TestKeyX]?.ToString());
        }

        /// <summary>
        /// Verifies that <see cref="LogContextServiceJsonExtensions.TryFromJson(string,out LogContextService?)"/> returns false and sets the output to null when given invalid JSON.
        /// </summary>
        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            // Arrange
            var json = LogContextServiceJsonExtensionsTestsConstants.InvalidJson2;

            // Act
            var success = LogContextServiceJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that <see cref="LogContextServiceJsonExtensions.TryFromJson(string,out LogContextService?)"/> throws an <see cref="ArgumentNullException"/> when called with a null JSON string.
        /// </summary>
        [Fact]
        public void TryFromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => LogContextServiceJsonExtensions.TryFromJson(json!, out _));
        }

        /// <summary>
        /// Verifies that <see cref="LogContextServiceJsonExtensions.TryFromJson(string,out LogContextService?)"/> returns false and sets the output to null when given an empty JSON string.
        /// </summary>
        [Fact]
        public void TryFromJson_EmptyJson_ReturnsFalse()
        {
            // Arrange
            var json = LogContextServiceJsonExtensionsTestsConstants.EmptyJson;

            // Act
            var success = LogContextServiceJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }
    }
}