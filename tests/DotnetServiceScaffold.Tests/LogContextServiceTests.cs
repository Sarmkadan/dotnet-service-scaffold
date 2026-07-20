using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using DotnetServiceScaffold.Infrastructure.Logging;

namespace DotnetServiceScaffold.Tests
{
    public class LogContextServiceTests
    {
        [Fact]
        public void SetAndGetProperties()
        {
            // Arrange
            var logContextService = new LogContextService();
            var correlationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            logContextService.CorrelationId = correlationId.ToString();
            logContextService.UserId = userId.ToString();

            // Assert
            Assert.Equal(correlationId.ToString(), logContextService.CorrelationId);
            Assert.Equal(userId.ToString(), logContextService.UserId);
        }

        [Fact]
        public void OverwriteProperties()
        {
            // Arrange
            var logContextService = new LogContextService();
            var correlationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            logContextService.CorrelationId = correlationId.ToString();
            logContextService.UserId = userId.ToString();
            logContextService.CorrelationId = Guid.NewGuid().ToString();

            // Assert
            Assert.NotEqual(correlationId.ToString(), logContextService.CorrelationId);
            Assert.Equal(userId.ToString(), logContextService.UserId);
        }

        [Fact]
        public void MissingKeyBehavior()
        {
            // Arrange
            var logContextService = new LogContextService();

            // Act
            var correlationId = logContextService.CorrelationId;

            // Assert
            Assert.Null(correlationId);
        }

        [Fact]
        public void IsolationBetweenScopes()
        {
            // Arrange
            var logContextService = new LogContextService();
            var correlationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            logContextService.CorrelationId = correlationId.ToString();
            logContextService.UserId = userId.ToString();
            var scope = logContextService.PushProperties();
            logContextService.CorrelationId = Guid.NewGuid().ToString();
            scope.Dispose();

            // Assert
            Assert.Equal(correlationId.ToString(), logContextService.CorrelationId);
            Assert.Equal(userId.ToString(), logContextService.UserId);
        }
    }
}
