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
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace DotnetServiceScaffold.Presentation.Middleware;

/// <summary>
/// Custom authentication handler for API key validation. Validates API keys from
/// the X-Api-Key header and sets up user principal for authorization checks.
/// Uses database lookup to validate keys against registered users.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private const string ApiKeyHeaderName = "X-Api-Key";
    private readonly IUserService _userService;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IUserService userService)
        : base(options, logger, encoder)
    {
        _userService = userService;
    }

    /// <summary>
    /// Handles the authentication process by extracting and validating API key from headers.
    /// Creates an authenticated principal if the key is valid.
    /// </summary>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if API key header exists
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
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
                return AuthenticateResult.Fail("Invalid API key");
            }
        }

        try
        {
            // Validate the API key against the database
            var user = await _userService.ValidateApiKeyAsync(providedApiKey);

            if (user is null)
            {
                return AuthenticateResult.Fail("Invalid API key");
            }

            // Create claims for the authenticated user
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Email) // Changed from user.Username to user.Email
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating API key");
            return AuthenticateResult.Fail("API key validation failed");
        }
    }

    /// <summary>
    /// Returns the challenge response when authentication fails. Informs client that
    /// API key authentication is required.
    /// </summary>
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.ContentType = "application/json";
        return Response.WriteAsJsonAsync(new
        {
            error = "Unauthorized",
            message = "API key is required. Provide it in the X-Api-Key header."
        });
    }

    /// <summary>
    /// Constant-time comparison of two API key strings. Length is compared first
    /// (length alone is not secret); the byte comparison runs in fixed time via
    /// <see cref="CryptographicOperations.FixedTimeEquals"/> to prevent timing attacks.
    /// </summary>
    private static bool FixedTimeEquals(string? left, string? right)
    {
        if (left is null || right is null)
        {
            return false;
        }

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
    public const string DefaultScheme = "ApiKey";
    public string Scheme => DefaultScheme;
    public string AuthenticationType = DefaultScheme;
}
