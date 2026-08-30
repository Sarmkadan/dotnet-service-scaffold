#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Constants for <see cref="AuditLogJsonExtensions"/>.
/// </summary>
internal static class AuditLogJsonExtensionsConstants
{
    /// <summary>
    /// Error message for null or whitespace JSON input.
    /// </summary>
    public const string JsonNullOrWhitespaceErrorMessage = "JSON string cannot be null or whitespace.";
}