#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// API endpoints for user management and authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new user account.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.CreateUserAsync(request.Email, request.FullName, request.Password, cancellationToken);

        return CreatedAtAction(nameof(GetUser), new { userId = user.Id }, new
        {
            success = true,
            data = new
            {
                user.Id,
                user.Email,
                user.FullName,
                user.CreatedAt
            }
        });
    }

    /// <summary>
    /// Authenticates a user and returns user information.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.AuthenticateUserAsync(request.Email, request.Password, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning(UserControllerConstants.FailedAuthenticationAttempt, request.Email);
            return Unauthorized(new { error = UserControllerConstants.InvalidEmailOrPassword });
        }

        return Ok(new
        {
            success = true,
            data = new
            {
                user.Id,
                user.Email,
                user.FullName,
                user.LastLoginAt
            }
        });
    }

    /// <summary>
    /// Retrieves user information by ID.
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userService.GetUserWithApiKeysAsync(userId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning(UserControllerConstants.UserNotFoundLog, userId);
            return NotFound(new { error = UserControllerConstants.UserNotFoundResponse });
        }

        return Ok(new
        {
            success = true,
            data = ProjectUser(user, includeApiKeyCount: true)
        });
    }

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    [HttpPost("{userId}/change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(Guid userId, [FromBody] ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _userService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword, cancellationToken);

        if (!success)
        {
            _logger.LogWarning(UserControllerConstants.PasswordChangeFailedForUserLog, userId);
            return BadRequest(new { error = "Current password is incorrect" });
        }

        return Ok(new { success = true, message = UserControllerConstants.PasswordChangedSuccessfully });
    }

    /// <summary>
    /// Unlocks a user account that is locked due to failed login attempts.
    /// </summary>
    [HttpPost("{userId}/unlock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlockUser(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _userService.UnlockUserAsync(userId, cancellationToken);
        return Ok(new { success = true, message = UserControllerConstants.UserAccountUnlocked });
    }

    /// <summary>
    /// Searches for users by name or email (case-insensitive) with pagination.
    /// </summary>
    /// <param name="q">Search query to match against user email or full name</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page (max 100)</param>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchUsers([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            _logger.LogWarning(UserControllerConstants.SearchQueryParameterQRequired);
            return BadRequest(new { error = UserControllerConstants.SearchQueryParameterQRequiredResponse });
        }

        if (page < UserControllerConstants.MinimumPageNumber) page = UserControllerConstants.MinimumPageNumber;
        if (pageSize < UserControllerConstants.MinimumPageSize) pageSize = UserControllerConstants.DefaultPageSize;
        if (pageSize > UserControllerConstants.MaximumPageSize) pageSize = UserControllerConstants.MaximumPageSize;

        var users = await _userService.SearchUsersAsync(q, page, pageSize);

        return Ok(new
        {
            success = true,
            data = new
            {
                results = users.Select(user => ProjectUser(user, includeApiKeyCount: false)),
                page,
                pageSize,
                total = users.Count()
            }
        });
    }

    private static object ProjectUser(User user, bool includeApiKeyCount)
    {
        if (includeApiKeyCount)
        {
            return new
            {
                user.Id,
                user.Email,
                user.FullName,
                user.IsActive,
                user.CreatedAt,
                user.LastLoginAt,
                apiKeyCount = user.ApiKeys.Count
            };
        }

        return new
        {
            user.Id,
            user.Email,
            user.FullName,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt
        };
    }

    public record RegisterRequest(string Email, string FullName, string Password);
    public record LoginRequest(string Email, string Password);
    public record ChangePasswordRequest(string OldPassword, string NewPassword);
}
