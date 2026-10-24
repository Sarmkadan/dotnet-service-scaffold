#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// The result of validating a presented API key.
/// </summary>
/// <param name="Status">The validation outcome.</param>
/// <param name="ApiKey">The matched key. Populated only when <see cref="Status"/> is <see cref="ApiKeyValidationStatus.Valid"/>.</param>
/// <param name="User">The owning user. Populated only when <see cref="Status"/> is <see cref="ApiKeyValidationStatus.Valid"/>.</param>
public sealed record ApiKeyValidationResult(ApiKeyValidationStatus Status, ApiKey? ApiKey = null, User? User = null)
{
    /// <summary>
    /// Gets a value indicating whether the key authenticated successfully.
    /// </summary>
    public bool IsValid => Status == ApiKeyValidationStatus.Valid;
}
