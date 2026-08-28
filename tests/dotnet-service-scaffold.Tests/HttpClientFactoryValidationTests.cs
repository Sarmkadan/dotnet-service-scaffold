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

        [Fact]
        public void EnsureValid_NullFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => HttpClientFactoryValidation.EnsureValid(null));
        }

        // ---------- ValidateCreateClient ----------
        [Fact]
        public void ValidateCreateClient_ValidName_ReturnsEmpty()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateClient("my-client");
            Assert.Empty(problems);
        }

        [Fact]
        public void ValidateCreateClient_NullOrWhiteSpaceName_ReturnsProblem()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateClient(null);
            Assert.Contains("Client name cannot be null, empty, or whitespace.", problems);
        }

        [Fact]
        public void ValidateCreateClient_NameTooLong_ReturnsProblem()
        {
            var longName = new string('a', 101);
            var problems = HttpClientFactoryValidation.ValidateCreateClient(longName);
            Assert.Contains("Client name cannot exceed 100 characters.", problems);
        }

        [Fact]
        public void EnsureValidCreateClient_InvalidName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => HttpClientFactoryValidation.EnsureValidCreateClient(""));
        }

        // ---------- ValidateCreateAuthenticatedClient ----------
        [Fact]
        public void ValidateCreateAuthenticatedClient_ValidParameters_ReturnsEmpty()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateAuthenticatedClient("apikey", "client");
            Assert.Empty(problems);
        }

        [Fact]
        public void ValidateCreateAuthenticatedClient_NullApiKey_ReturnsProblem()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateAuthenticatedClient(null, "client");
            Assert.Contains("API key cannot be null, empty, or whitespace.", problems);
        }

        [Fact]
        public void ValidateCreateAuthenticatedClient_ApiKeyTooLong_ReturnsProblem()
        {
            var longKey = new string('k', 501);
            var problems = HttpClientFactoryValidation.ValidateCreateAuthenticatedClient(longKey, "client");
            Assert.Contains("API key cannot exceed 500 characters.", problems);
        }

        [Fact]
        public void EnsureValidCreateAuthenticatedClient_InvalidParameters_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => HttpClientFactoryValidation.EnsureValidCreateAuthenticatedClient("", null));
        }

        // ---------- ValidateCreateBearerClient ----------
        [Fact]
        public void ValidateCreateBearerClient_ValidParameters_ReturnsEmpty()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateBearerClient("token", "client");
            Assert.Empty(problems);
        }

        [Fact]
        public void ValidateCreateBearerClient_NullToken_ReturnsProblem()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateBearerClient(null, "client");
            Assert.Contains("Bearer token cannot be null, empty, or whitespace.", problems);
        }

        [Fact]
        public void ValidateCreateBearerClient_TokenTooLong_ReturnsProblem()
        {
            var longToken = new string('t', 2001);
            var problems = HttpClientFactoryValidation.ValidateCreateBearerClient(longToken, "client");
            Assert.Contains("Bearer token cannot exceed 2000 characters.", problems);
        }

        [Fact]
        public void EnsureValidCreateBearerClient_InvalidParameters_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => HttpClientFactoryValidation.EnsureValidCreateBearerClient("", ""));
        }

        // ---------- ValidateCreateClientWithBaseUrl ----------
        [Fact]
        public void ValidateCreateClientWithBaseUrl_ValidParameters_ReturnsEmpty()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateClientWithBaseUrl("https://example.com", "client");
            Assert.Empty(problems);
        }

        [Fact]
        public void ValidateCreateClientWithBaseUrl_NullBaseUrl_ReturnsProblem()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateClientWithBaseUrl(null, "client");
            Assert.Contains("Base URL cannot be null, empty, or whitespace.", problems);
        }

        [Fact]
        public void ValidateCreateClientWithBaseUrl_InvalidUri_ReturnsProblem()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateClientWithBaseUrl("not-a-uri", "client");
            Assert.Contains("Base URL must be a valid absolute URI.", problems);
        }

        [Fact]
        public void ValidateCreateClientWithBaseUrl_WrongScheme_ReturnsProblem()
        {
            var problems = HttpClientFactoryValidation.ValidateCreateClientWithBaseUrl("ftp://example.com", "client");
            Assert.Contains("Base URL must use http:// or https:// scheme.", problems);
        }

        [Fact]
        public void EnsureValidCreateClientWithBaseUrl_InvalidParameters_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => HttpClientFactoryValidation.EnsureValidCreateClientWithBaseUrl("invalid", null));
        }
    }
}
