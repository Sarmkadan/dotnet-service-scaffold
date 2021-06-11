// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Domain.Exceptions;
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
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _userService.CreateUserAsync(request.Email, request.FullName, request.Password);

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
        catch (ServiceValidationException ex)
        {
            _logger.LogWarning("Registration validation error: {Errors}", string.Join(", ", ex.Errors));
            return BadRequest(new { error = "Validation failed", details = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error");
            return StatusCode(500, new { error = "Registration failed" });
        }
    }

    /// <summary>
    /// Authenticates a user and returns user information.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _userService.AuthenticateUserAsync(request.Email, request.Password);

            if (user == null)
            {
                _logger.LogWarning("Failed authentication attempt for {Email}", request.Email);
                return Unauthorized(new { error = "Invalid email or password" });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for {Email}", request.Email);
            return StatusCode(500, new { error = "Login failed" });
        }
    }

    /// <summary>
    /// Retrieves user information by ID.
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(Guid userId)
    {
        try
        {
            var user = await _userService.GetUserWithApiKeysAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return NotFound(new { error = "User not found" });
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.IsActive,
                    user.CreatedAt,
                    user.LastLoginAt,
                    apiKeyCount = user.ApiKeys.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", userId);
            return StatusCode(500, new { error = "Error retrieving user" });
        }
    }

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    [HttpPost("{userId}/change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(Guid userId, [FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var success = await _userService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);

            if (!success)
            {
                _logger.LogWarning("Password change failed for user {UserId}", userId);
                return BadRequest(new { error = "Current password is incorrect" });
            }

            return Ok(new { success = true, message = "Password changed successfully" });
        }
        catch (ServiceScaffoldException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password change error for user {UserId}", userId);
            return StatusCode(500, new { error = "Password change failed" });
        }
    }

    /// <summary>
    /// Unlocks a user account that is locked due to failed login attempts.
    /// </summary>
    [HttpPost("{userId}/unlock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlockUser(Guid userId)
    {
        try
        {
            await _userService.UnlockUserAsync(userId);
            return Ok(new { success = true, message = "User account unlocked" });
        }
        catch (ServiceScaffoldException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking user {UserId}", userId);
            return StatusCode(500, new { error = "Error unlocking user" });
        }
    }
}

public record RegisterRequest(string Email, string FullName, string Password);
public record LoginRequest(string Email, string Password);
public record ChangePasswordRequest(string OldPassword, string NewPassword);
