#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Presentation.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace DotnetServiceScaffold.Presentation.Middleware;

/// <summary>
/// Custom authentication handler for API key validation. Validates API keys from
/// the X-Api-Key header via <see cref="IApiKeyService"/> and sets up a principal carrying the user
/// and the key's scopes for authorization checks.
/// </summary>
public class ApiKeyAuthenticationMiddleware : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IApiKeyService _apiKeyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyAuthenticationMiddleware"/> class.
    /// </summary>
    /// <param name="options">The authentication scheme options monitor.</param>
    /// <param name="logger">The logger factory used by the authentication base class.</param>
    /// <param name="encoder">The URL encoder used by the authentication base class.</param>
    /// <param name="apiKeyService">The service that validates presented API keys.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="apiKeyService"/> is <see langword="null"/>.</exception>
    public ApiKeyAuthenticationMiddleware(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyService apiKeyService)
        : base(options, logger, encoder)
    {
        ArgumentNullException.ThrowIfNull(apiKeyService);

        _apiKeyService = apiKeyService;
    }

    /// <summary>
    /// Handles the authentication process by extracting and validating API key from headers.
    /// Creates an authenticated principal if the key is valid.
    /// </summary>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if API key header exists
        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationMiddlewareConstants.ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return AuthenticateResult.NoResult();
        }

        // Compare every supplied X-Api-Key header value against the first one in
        // constant time. Ordinary string equality on secrets leaks timing
        // information that can be exploited in timing attacks.
        foreach (var headerValue in apiKeyHeaderValues)
        {
            if (string.IsNullOrWhiteSpace(headerValue) || !FixedTimeEquals(headerValue, providedApiKey))
            {
                return AuthenticateResult.Fail(ApiKeyAuthenticationMiddlewareConstants.InvalidApiKeyError);
            }
        }

        try
        {
            var validation = await _apiKeyService.ValidateAsync(
                providedApiKey,
                Context.Connection.RemoteIpAddress?.ToString(),
                Context.RequestAborted);

            // Every failure reason maps to the same client-facing error; the specific reason is logged by the service.
            if (!validation.IsValid || validation.User is null || validation.ApiKey is null)
            {
                return AuthenticateResult.Fail(ApiKeyAuthenticationMiddlewareConstants.InvalidApiKeyError);
            }

            var user = validation.User;
            var apiKey = validation.ApiKey;

            // Create claims for the authenticated user, plus one scope claim per granted scope
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.Email), // Changed from user.Username to user.Email
                new(ApiKeyClaimTypes.ApiKeyId, apiKey.Id.ToString())
            };
            claims.AddRange(ParseScopes(apiKey).Select(scope => new Claim(ApiKeyClaimTypes.Scope, scope)));

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ApiKeyAuthenticationMiddlewareConstants.ApiKeyValidationLogMessage);
            return AuthenticateResult.Fail(ApiKeyAuthenticationMiddlewareConstants.ApiKeyValidationError);
        }
    }

    /// <summary>
    /// Returns the challenge response when authentication fails. Informs client that
    /// API key authentication is required.
    /// </summary>
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = ApiKeyAuthenticationMiddlewareConstants.StatusCodeUnauthorized;
        Response.ContentType = ApiKeyAuthenticationMiddlewareConstants.JsonContentType;
        return Response.WriteAsJsonAsync(new
        {
            error = ApiKeyAuthenticationMiddlewareConstants.UnauthorizedError,
            message = ApiKeyAuthenticationMiddlewareConstants.ApiKeyRequiredMessage
        });
    }

    /// <summary>
    /// Splits a key's stored scope list into individual scopes.
    /// </summary>
    /// <param name="apiKey">The key whose scopes to read.</param>
    /// <returns>The granted scopes, or an empty sequence when the key has none.</returns>
    private static IEnumerable<string> ParseScopes(ApiKey apiKey) =>
        (apiKey.AllowedScopes ?? string.Empty)
            .Split(ApiKeyAuthenticationMiddlewareConstants.ScopeSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>
    /// Constant-time comparison of two API key strings. Length is compared first
    /// (length alone is not secret); the byte comparison runs in fixed time via
    /// <see cref="CryptographicOperations.FixedTimeEquals"/> to prevent timing attacks.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.</exception>
    public static bool FixedTimeEquals(string? left, string? right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);

        return leftBytes.Length == rightBytes.Length
            && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}

/// <summary>
/// Configuration options for API key authentication scheme.
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = ApiKeyAuthenticationMiddlewareConstants.DefaultScheme;
    public string Scheme => DefaultScheme;
    public string AuthenticationType = DefaultScheme;
}
