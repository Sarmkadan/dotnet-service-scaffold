using System;
using DotnetServiceScaffold.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DotnetServiceScaffold.Tests
{
    /// <summary>
    /// Tests for <see cref="StructuredLoggingOptions"/> and its extension methods,
    /// focusing on validation of configuration combinations.
    /// </summary>
    public class StructuredLoggingOptionsTests
    {
        [Fact]
        public void DefaultOptions_ShouldBeValid()
        {
            var options = new StructuredLoggingOptions();

            var exception = Record.Exception(() => options.Validate());

            Assert.Null(exception);
        }

        [Fact]
        public void WithApplicationName_ShouldSetName()
        {
            var options = new StructuredLoggingOptions()
                .WithApplicationName("MyApp");

            Assert.Equal("MyApp", options.ApplicationName);
        }

        [Fact]
        public void WithMinimumLevel_ShouldSetLevel()
        {
            var options = new StructuredLoggingOptions()
                .WithMinimumLevel(LogLevel.Warning);

            Assert.Equal("Warning", options.MinimumLevel);
        }

        [Fact]
        public void WithCorrelationIdHeader_ShouldSetHeader()
        {
            var options = new StructuredLoggingOptions()
                .WithCorrelationIdHeader("X-My-Correlation");

            Assert.Equal("X-My-Correlation", options.CorrelationIdHeader);
        }

        [Fact]
        public void WithoutMachineNameEnrichment_ShouldDisable()
        {
            var options = new StructuredLoggingOptions()
                .WithoutMachineNameEnrichment();

            Assert.False(options.EnrichWithMachineName);
        }

        [Fact]
        public void Validate_ShouldThrow_WhenMinimumLevelIsEmpty()
        {
            var options = new StructuredLoggingOptions
            {
                MinimumLevel = string.Empty
            };

            var ex = Assert.Throws<ArgumentException>(() => options.Validate());

            Assert.Equal(nameof(StructuredLoggingOptions.MinimumLevel), ex.ParamName);
        }

        [Fact]
        public void Validate_ShouldThrow_WhenMinimumLevelIsInvalid()
        {
            var options = new StructuredLoggingOptions
            {
                MinimumLevel = "NotARealLevel"
            };

            var ex = Assert.Throws<ArgumentException>(() => options.Validate());

            Assert.Equal(nameof(StructuredLoggingOptions.MinimumLevel), ex.ParamName);
        }
    }
}
