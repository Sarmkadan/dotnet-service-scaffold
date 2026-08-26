#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotnetServiceScaffold.Shared.Extensions;

/// <summary>
/// Centralizes constant values used by <see cref="ExceptionExtensionsJsonExtensions"/>
/// to configure JSON serialization of exception data.
/// </summary>
internal static class ExceptionExtensionsJsonExtensionsConstants
{
    /// <summary>
    /// Default value indicating whether serialized JSON output is indented.
    /// </summary>
    public const bool DefaultWriteIndented = false;

    /// <summary>
    /// Indicates whether property names are matched case-insensitively during deserialization.
    /// </summary>
    public const bool PropertyNameCaseInsensitiveEnabled = true;

    /// <summary>
    /// Default naming policy applied to property names during serialization.
    /// </summary>
    public static readonly JsonNamingPolicy DefaultPropertyNamingPolicy = JsonNamingPolicy.CamelCase;

    /// <summary>
    /// Default rule controlling which properties are skipped during serialization.
    /// </summary>
    public static readonly JsonIgnoreCondition DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
}
