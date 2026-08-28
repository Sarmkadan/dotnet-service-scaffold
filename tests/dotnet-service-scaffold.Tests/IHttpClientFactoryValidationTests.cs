using System;
using System.Collections.Generic;

namespace DotnetServiceScaffold.Tests
{
    public interface IHttpClientFactoryValidationTests
    {
        void IsValid_NullFactory_ReturnsFalse();
        void EnsureValid_NullFactory_ThrowsArgumentNullException();
        void ValidateCreateClient_ValidName_ReturnsEmpty();
        void ValidateCreateClient_NullOrWhiteSpaceName_ReturnsProblem();
        void ValidateCreateClient_NameTooLong_ReturnsProblem();
        void EnsureValidCreateClient_InvalidName_ThrowsArgumentException();
        void ValidateCreateAuthenticatedClient_ValidParameters_ReturnsEmpty();
        void ValidateCreateAuthenticatedClient_NullApiKey_ReturnsProblem();
        void ValidateCreateAuthenticatedClient_ApiKeyTooLong_ReturnsProblem();
        void EnsureValidCreateAuthenticatedClient_InvalidParameters_ThrowsArgumentException();
        void ValidateCreateBearerClient_ValidParameters_ReturnsEmpty();
        void ValidateCreateBearerClient_NullToken_ReturnsProblem();
        void ValidateCreateBearerClient_TokenTooLong_ReturnsProblem();
        void EnsureValidCreateBearerClient_InvalidParameters_ThrowsArgumentException();
        void ValidateCreateClientWithBaseUrl_ValidParameters_ReturnsEmpty();
        void ValidateCreateClientWithBaseUrl_NullBaseUrl_ReturnsProblem();
        void ValidateCreateClientWithBaseUrl_InvalidUri_ReturnsProblem();
        void ValidateCreateClientWithBaseUrl_WrongScheme_ReturnsProblem();
        void EnsureValidCreateClientWithBaseUrl_InvalidParameters_ThrowsArgumentException();
    }
}