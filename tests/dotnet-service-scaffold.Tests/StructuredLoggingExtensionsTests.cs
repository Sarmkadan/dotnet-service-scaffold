#nullable enable

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog;
using Xunit;
using DotnetServiceScaffold.Infrastructure.Logging;

namespace DotnetServiceScaffold.Tests;

public class StructuredLoggingExtensionsTests
{
    [Fact]
    public void AddStructuredLogging_ValidArgs_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>())
            .Build();

        // Act
        var result = services.AddStructuredLogging(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(services, d => d.ServiceType == typeof(ILogContextService) && d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddStructuredLogging_NullServices_Throws()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => StructuredLoggingExtensions.AddStructuredLogging(null!, configuration));
    }

    [Fact]
    public void AddStructuredLogging_NullConfiguration_Throws()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddStructuredLogging(null!));
    }

    [Fact]
    public void UseCorrelationId_ValidArgs_RegistersMiddleware()
    {
        // Arrange
        var appMock = new Mock<IApplicationBuilder>();
        appMock.Setup(a => a.UseMiddleware(It.IsAny<Type>(), It.IsAny<object[]>())).Returns(appMock.Object);

        // Act
        var result = StructuredLoggingExtensions.UseCorrelationId(appMock.Object);

        // Assert
        appMock.Verify(a => a.UseMiddleware(typeof(CorrelationIdMiddleware)), Times.Once);
        Assert.Same(appMock.Object, result);
    }

    [Fact]
    public void UseCorrelationId_NullApp_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => StructuredLoggingExtensions.UseCorrelationId(null!));
    }

    [Fact]
    public void EnrichFromOptions_ValidArgs_ReturnsConfig()
    {
        // Arrange
        var loggerConfig = new LoggerConfiguration();
        var options = new StructuredLoggingOptions
        {
            ApplicationName = "TestApp",
            EnrichWithMachineName = true,
            EnrichWithEnvironment = true
        };

        // Act
        var result = loggerConfig.EnrichFromOptions(options);

        // Assert
        Assert.NotNull(result);
        Assert.Same(loggerConfig, result);
    }

    [Fact]
    public void EnrichFromOptions_NullLoggerConfig_Throws()
    {
        var options = new StructuredLoggingOptions();
        Assert.Throws<ArgumentNullException>(() => StructuredLoggingExtensions.EnrichFromOptions(null!, options));
    }

    [Fact]
    public void EnrichFromOptions_NullOptions_Throws()
    {
        var loggerConfig = new LoggerConfiguration();
        Assert.Throws<ArgumentNullException>(() => loggerConfig.EnrichFromOptions(null!));
    }
}
