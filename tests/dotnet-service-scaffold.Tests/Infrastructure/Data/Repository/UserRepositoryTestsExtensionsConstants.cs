#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Constants for UserRepositoryTestsExtensions to avoid magic strings.
/// </summary>
internal static class UserRepositoryTestsExtensionsConstants
{
    /// <summary>
    /// Default password hash for test users.
    /// </summary>
    public const string DefaultPasswordHash = "test-hash";

    /// <summary>
    /// Default full name for test users.
    /// </summary>
    public const string DefaultFullName = "Test User";

    /// <summary>
    /// Assertion message for user existence verification.
    /// </summary>
    public const string UserShouldExistInDatabase = "User should exist in database";

    /// <summary>
    /// Assertion message for username matching.
    /// </summary>
    public const string UsernameShouldMatch = "Username should match";

    /// <summary>
    /// Assertion message for email matching.
    /// </summary>
    public const string EmailShouldMatch = "Email should match";

    /// <summary>
    /// Assertion message for full name matching.
    /// </summary>
    public const string FullNameShouldMatch = "Full name should match";

    /// <summary>
    /// Format string for user with email existence assertion.
    /// </summary>
    public const string UserWithEmailShouldExistFormat = "User with email {0} should exist";
}