using System;
using System.Collections.Generic;

namespace DotnetServiceScaffold.Tests
{
    public interface IHttpClientFactoryValidationTests
    {
        void IsValid_NullFactory_ReturnsFalse();
        Task IsValid_NullFactory_ReturnsFalseAsync(CancellationToken cancellationToken = default);
        void EnsureValid_NullFactory_ThrowsArgumentNullException();
        Task EnsureValid_NullFactory_ThrowsArgumentNullExceptionAsync(CancellationToken cancellationToken = default);
        void ValidateCreateClient_ValidName_ReturnsEmpty();
        Task ValidateCreateClient_ValidName_ReturnsEmptyAsync(CancellationToken cancellationToken = default);
        void ValidateCreateClient_NullOrWhiteSpaceName_ReturnsProblem();
        Task ValidateCreateClient_NullOrWhiteSpaceName_ReturnsProblemAsync(CancellationToken cancellationToken = default);
        void ValidateCreateClient_NameTooLong_ReturnsProblem();
        Task ValidateCreateClient_NameTooLong_ReturnsProblemAsync(CancellationToken cancellationToken = default);
        void EnsureValidCreateClient_InvalidName_ThrowsArgumentException();
        Task EnsureValidCreateClient_InvalidName_ThrowsArgumentExceptionAsync(CancellationToken cancellationToken = default);
        void ValidateCreateAuthenticatedClient_ValidParameters_ReturnsEmpty();
        Task ValidateCreateAuthenticatedClient_ValidParameters_ReturnsEmptyAsync(CancellationToken cancellationToken = default);
        void ValidateCreateAuthenticatedClient_NullApiKey_ReturnsProblem();
        Task ValidateCreateAuthenticatedClient_NullApiKey_ReturnsProblemAsync(CancellationToken cancellationToken = default);
        void ValidateCreateAuthenticatedClient_ApiKeyTooLong_ReturnsProblem();
        Task ValidateCreateAuthenticatedClient_ApiKeyTooLong_ReturnsProblemAsync(CancellationToken cancellationToken = default);
        void EnsureValidCreateAuthenticatedClient_InvalidParameters_ThrowsArgumentException();
        Task EnsureValidCreateAuthenticatedClient_InvalidParameters_ThrowsArgumentExceptionAsync(CancellationToken cancellationToken = default);
        void ValidateCreateBearerClient_ValidParameters_ReturnsEmpty();
        Task ValidateCreateBearerClient_ValidParameters_ReturnsEmptyAsync(CancellationToken cancellationToken = default);
        void ValidateCreateBearerClient_NullToken_ReturnsProblem();
        Task ValidateCreateBearerClient_NullToken_ReturnsProblemAsync(CancellationToken cancellationToken = default);
        void ValidateCreateBearerClient_TokenTooLong_ReturnsProblem();
        Task ValidateCreateBearerClient_TokenTooLong_ReturnsProblemAsync(CancellationToken cancellationToken = default);
        void EnsureValidCreateBearerClient_InvalidParameters_ThrowsArgumentException();
        Task EnsureValidCreateBearerClient_InvalidParameters_ThrowsArgumentExceptionAsync(CancellationToken cancellationToken = default);
        void ValidateCreateClientWithBaseUrl_ValidParameters_ReturnsEmpty();
        Task ValidateCreateClientWithBaseUrl_ValidParameters_ReturnsEmptyAsync(CancellationToken cancellationToken = default);
        void ValidateCreateClientWithBaseUrl_NullBaseUrl_ReturnsProblem();
        Task ValidateCreateClientWithBaseUrl_NullBaseUrl_ReturnsProblemAsync(CancellationToken cancellationToken = default);
        void ValidateCreateClientWithBaseUrl_InvalidUri_ReturnsProblem();
        Task ValidateCreateClientWithBaseUrl_InvalidUri_ReturnsProblemAsync(CancellationToken cancellationToken = default);
        void ValidateCreateClientWithBaseUrl_WrongScheme_ReturnsProblem();
        Task ValidateCreateClientWithBaseUrl_WrongScheme_ReturnsProblemAsync(CancellationToken cancellationToken = default);
        void EnsureValidCreateClientWithBaseUrl_InvalidParameters_ThrowsArgumentException();
        Task EnsureValidCreateClientWithBaseUrl_InvalidParameters_ThrowsArgumentExceptionAsync(CancellationToken cancellationToken = default);
    }
}