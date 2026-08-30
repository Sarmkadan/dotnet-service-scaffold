#nullable enable
using System;
using System.Collections.Generic;
using DotnetServiceScaffold.Infrastructure.Integration;
using Xunit;

namespace DotnetServiceScaffold.Tests
{
    public class HttpClientFactoryValidationTests : IHttpClientFactoryValidationTests
    {
        // ---------- HttpClientFactory instance validation ----------
        // We cannot instantiate HttpClientFactory directly (it may be internal),
        // so we only test the null-handling behavior.

        [Fact]
        public void IsValid_NullFactory_ReturnsFalse()
        {
            bool isValid = HttpClientFactoryValidation.IsValid(null);
            Assert.False(isValid);
        }

        public Task IsValid_NullFactory_ReturnsFalseAsync(CancellationToken cancellationToken = default)
        {
            IsValid_NullFactory_ReturnsFalse();
            return Task.CompletedTask;
        }

        [Fact]
        public void EnsureValid_NullFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => HttpClientFactoryValidation.EnsureValid(null));
        }

        public Task EnsureValid_NullFactory_ThrowsArgumentNullExceptionAsync(CancellationToken cancellationToken = default)
        {
            EnsureValid_NullFactory_ThrowsArgumentNullException();
            return Task.CompletedTask;
        }

        // ---------- ValidateCreateClient ----------
        [Fact]
        public void ValidateCreateClient_ValidName_ReturnsEmpty()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateClient("my-client");
            Assert.Empty(problems);
        }

        public Task ValidateCreateClient_ValidName_ReturnsEmptyAsync(CancellationToken cancellationToken = default)
        {
            ValidateCreateClient_ValidName_ReturnsEmpty();
            return Task.CompletedTask;
        }

        [Fact]
        public void ValidateCreateClient_NullOrWhiteSpaceName_ReturnsProblem()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateClient(null);
            Assert.Contains(HttpClientFactoryValidationTestsConstants.ClientNameNullOrWhitespace, problems);
        }

        public Task ValidateCreateClient_NullOrWhiteSpaceName_ReturnsProblemAsync(CancellationToken cancellationToken = default)
        {
            ValidateCreateClient_NullOrWhiteSpaceName_ReturnsProblem();
            return Task.CompletedTask;
        }

        [Fact]
        public void ValidateCreateClient_NameTooLong_ReturnsProblem()
        {
            var longName = new string('a', HttpClientFactoryValidationTestsConstants.MaxClientNameLength + 1);
            var problems = HttpClientFactoryValidation.ValidateCreateClient(longName);
            Assert.Contains(HttpClientFactoryValidationTestsConstants.ClientNameTooLong, problems);
        }

        public Task ValidateCreateClient_NameTooLong_ReturnsProblemAsync(CancellationToken cancellationToken = default)
        {
            ValidateCreateClient_NameTooLong_ReturnsProblem();
            return Task.CompletedTask;
        }

        [Fact]
        public void EnsureValidCreateClient_InvalidName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => HttpClientFactoryValidation.EnsureValidCreateClient(""));
        }

        public Task EnsureValidCreateClient_InvalidName_ThrowsArgumentExceptionAsync(CancellationToken cancellationToken = default)
        {
            EnsureValidCreateClient_InvalidName_ThrowsArgumentException();
            return Task.CompletedTask;
        }

        // ---------- ValidateCreateAuthenticatedClient ----------
        [Fact]
        public void ValidateCreateAuthenticatedClient_ValidParameters_ReturnsEmpty()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateAuthenticatedClient("apikey", "client");
            Assert.Empty(problems);
        }

        public Task ValidateCreateAuthenticatedClient_ValidParameters_ReturnsEmptyAsync(CancellationToken cancellationToken = default)
        {
            ValidateCreateAuthenticatedClient_ValidParameters_ReturnsEmpty();
            return Task.CompletedTask;
        }

        [Fact]
        public void ValidateCreateAuthenticatedClient_NullApiKey_ReturnsProblem()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateAuthenticatedClient(null, "client");
            Assert.Contains(HttpClientFactoryValidationTestsConstants.ApiKeyNullOrWhitespace, problems);
        }

        public Task ValidateCreateAuthenticatedClient_NullApiKey_ReturnsProblemAsync(CancellationToken cancellationToken = default)
        {
            ValidateCreateAuthenticatedClient_NullApiKey_ReturnsProblem();
            return Task.CompletedTask;
        }

        [Fact]
        public void ValidateCreateAuthenticatedClient_ApiKeyTooLong_ReturnsProblem()
        {
            var longKey = new string('k', HttpClientFactoryValidationTestsConstants.MaxApiKeyLength + 1);
            var problems = HttpClientFactoryValidation.ValidateCreateAuthenticatedClient(longKey, "client");
            Assert.Contains(HttpClientFactoryValidationTestsConstants.ApiKeyTooLong, problems);
        }

        public Task ValidateCreateAuthenticatedClient_ApiKeyTooLong_ReturnsProblemAsync(CancellationToken cancellationToken = default)
        {
            ValidateCreateAuthenticatedClient_ApiKeyTooLong_ReturnsProblem();
            return Task.CompletedTask;
        }

        [Fact]
        public void EnsureValidCreateAuthenticatedClient_InvalidParameters_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => HttpClientFactoryValidation.EnsureValidCreateAuthenticatedClient("", null));
        }

        public Task EnsureValidCreateAuthenticatedClient_InvalidParameters_ThrowsArgumentExceptionAsync(CancellationToken cancellationToken = default)
        {
            EnsureValidCreateAuthenticatedClient_InvalidParameters_ThrowsArgumentException();
            return Task.CompletedTask;
        }

        // ---------- ValidateCreateBearerClient ----------
        [Fact]
        public void ValidateCreateBearerClient_ValidParameters_ReturnsEmpty()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateBearerClient("token", "client");
            Assert.Empty(problems);
        }

        public Task ValidateCreateBearerClient_ValidParameters_ReturnsEmptyAsync(CancellationToken cancellationToken = default)
        {
            ValidateCreateBearerClient_ValidParameters_ReturnsEmpty();
            return Task.CompletedTask;
        }

        [Fact]
        public void ValidateCreateBearerClient_NullToken_ReturnsProblem()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateBearerClient(null, "client");
            Assert.Contains(HttpClientFactoryValidationTestsConstants.BearerTokenNullOrWhitespace, problems);
        }

        public Task ValidateCreateBearerClient_NullToken_ReturnsProblemAsync(CancellationToken cancellationToken = default)
        {
            ValidateCreateBearerClient_NullToken_ReturnsProblem();
            return Task.CompletedTask;
        }

        [Fact]
        public void ValidateCreateBearerClient_TokenTooLong_ReturnsProblem()
        {
            var longToken = new string('t', HttpClientFactoryValidationTestsConstants.MaxBearerTokenLength + 1);
            var problems = HttpClientFactoryValidation.ValidateCreateBearerClient(longToken, "client");
            Assert.Contains(HttpClientFactoryValidationTestsConstants.BearerTokenTooLong, problems);
        }

        public Task ValidateCreateBearerClient_TokenTooLong_ReturnsProblemAsync(CancellationToken cancellationToken = default)
        {
            ValidateCreateBearerClient_TokenTooLong_ReturnsProblem();
            return Task.CompletedTask;
        }

        [Fact]
        public void EnsureValidCreateBearerClient_InvalidParameters_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => HttpClientFactoryValidation.EnsureValidCreateBearerClient("", ""));
        }

        public Task EnsureValidCreateBearerClient_InvalidParameters_ThrowsArgumentExceptionAsync(CancellationToken cancellationToken = default)
        {
            EnsureValidCreateBearerClient_InvalidParameters_ThrowsArgumentException();
            return Task.CompletedTask;
        }

        // ---------- ValidateCreateClientWithBaseUrl ----------
        [Fact]
        public void ValidateCreateClientWithBaseUrl_ValidParameters_ReturnsEmpty()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateClientWithBaseUrl("https://example.com", "client");
            Assert.Empty(problems);
        }

        public Task ValidateCreateClientWithBaseUrl_ValidParameters_ReturnsEmptyAsync(CancellationToken cancellationToken = default)
        {
            ValidateCreateClientWithBaseUrl_ValidParameters_ReturnsEmpty();
            return Task.CompletedTask;
        }

        [Fact]
        public void ValidateCreateClientWithBaseUrl_NullBaseUrl_ReturnsProblem()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateClientWithBaseUrl(null, "client");
            Assert.Contains(HttpClientFactoryValidationTestsConstants.BaseUrlNullOrWhitespace, problems);
        }

        public Task ValidateCreateClientWithBaseUrl_NullBaseUrl_ReturnsProblemAsync(CancellationToken cancellationToken = default)
        {
            ValidateCreateClientWithBaseUrl_NullBaseUrl_ReturnsProblem();
            return Task.CompletedTask;
        }

        [Fact]
        public void ValidateCreateClientWithBaseUrl_InvalidUri_ReturnsProblem()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateClientWithBaseUrl("not-a-uri", "client");
            Assert.Contains(HttpClientFactoryValidationTestsConstants.BaseUrlInvalidUri, problems);
        }

        public Task ValidateCreateClientWithBaseUrl_InvalidUri_ReturnsProblemAsync(CancellationToken cancellationToken = default)
        {
            ValidateCreateClientWithBaseUrl_InvalidUri_ReturnsProblem();
            return Task.CompletedTask;
        }

        [Fact]
        public void ValidateCreateClientWithBaseUrl_WrongScheme_ReturnsProblem()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateClientWithBaseUrl("ftp://example.com", "client");
            Assert.Contains("Base URL must use http:// or https:// scheme.", problems);
        }

        public Task ValidateCreateClientWithBaseUrl_WrongScheme_ReturnsProblemAsync(CancellationToken cancellationToken = default)
        {
            ValidateCreateClientWithBaseUrl_WrongScheme_ReturnsProblem();
            return Task.CompletedTask;
        }

        [Fact]
        public void EnsureValidCreateClientWithBaseUrl_InvalidParameters_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => HttpClientFactoryValidation.EnsureValidCreateClientWithBaseUrl("invalid", null));
        }

        public Task EnsureValidCreateClientWithBaseUrl_InvalidParameters_ThrowsArgumentExceptionAsync(CancellationToken cancellationToken = default)
        {
            EnsureValidCreateClientWithBaseUrl_InvalidParameters_ThrowsArgumentException();
            return Task.CompletedTask;
        }
    }
}
