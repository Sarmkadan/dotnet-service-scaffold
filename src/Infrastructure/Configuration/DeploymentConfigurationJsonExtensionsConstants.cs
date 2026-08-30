#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DotnetServiceScaffold.Infrastructure.Configuration;

/// <summary>
/// Constants for <see cref="DeploymentConfigurationJsonExtensions"/>.
/// </summary>
internal static class DeploymentConfigurationJsonExtensionsConstants
{
    /// <summary>
    /// The property naming policy used for JSON serialization and deserialization.
    /// </summary>
    public static readonly JsonNamingPolicy PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

    /// <summary>
    /// Indicates whether JSON should be written with indentation by default.
    /// </summary>
    public static readonly bool WriteIndented = false;

    /// <summary>
    /// The type info resolver used for JSON serialization and deserialization.
    /// </summary>
    public static readonly DefaultJsonTypeInfoResolver TypeInfoResolver = new DefaultJsonTypeInfoResolver();

    /// <summary>
    /// Indicates whether property name matching during deserialization is case-insensitive.
    /// </summary>
    public static readonly bool PropertyNameCaseInsensitive = true;

    /// <summary>
    /// The reference handler used for JSON serialization and deserialization.
    /// </summary>
    public static readonly ReferenceHandler ReferenceHandler = ReferenceHandler.IgnoreCycles;
}