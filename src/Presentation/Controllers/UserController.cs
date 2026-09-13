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

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="userService">The service used to manage users and authentication.</param>
    /// <param name="logger">The logger used to record controller activity.</param>
    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new user account.
    /// </summary>
    /// <param name="request">The details of the user account to create.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>An action result containing the newly created user.</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
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
    /// <param name="request">The credentials used to authenticate the user.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>An action result containing the authenticated user, or an unauthorized response.</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
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
    /// <param name="userId">The identifier of the user to retrieve.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>An action result containing the user information, or a not-found response.</returns>
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
    /// <param name="userId">The identifier of the user whose password is changed.</param>
    /// <param name="request">The current and replacement passwords.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>An action result indicating whether the password was changed.</returns>
    [HttpPost("{userId}/change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(Guid userId, [FromBody] ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
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
    /// <param name="userId">The identifier of the user account to unlock.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>An action result indicating that the user account was unlocked.</returns>
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
    /// <param name="q">The search query to match against user email addresses or full names.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The number of users per page, up to 100.</param>
    /// <returns>An action result containing the matching users and pagination information.</returns>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchUsers([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        ArgumentException.ThrowIfNullOrEmpty(q);
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

    /// <summary>
    /// Represents the information required to register a user.
    /// </summary>
    /// <param name="Email">The user's email address.</param>
    /// <param name="FullName">The user's full name.</param>
    /// <param name="Password">The user's password.</param>
    public record RegisterRequest(string Email, string FullName, string Password);

    /// <summary>
    /// Represents the credentials required to authenticate a user.
    /// </summary>
    /// <param name="Email">The user's email address.</param>
    /// <param name="Password">The user's password.</param>
    public record LoginRequest(string Email, string Password);

    /// <summary>
    /// Represents the information required to change a user's password.
    /// </summary>
    /// <param name="OldPassword">The user's current password.</param>
    /// <param name="NewPassword">The user's replacement password.</param>
    public record ChangePasswordRequest(string OldPassword, string NewPassword);
}
