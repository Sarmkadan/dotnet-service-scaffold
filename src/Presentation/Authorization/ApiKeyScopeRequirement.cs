#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Authorization;

namespace DotnetServiceScaffold.Presentation.Authorization;

/// <summary>
/// Requires that the caller's API key grants a scope, directly or through a broader scope.
/// </summary>
/// <param name="RequiredScope">The scope the endpoint requires.</param>
public sealed record ApiKeyScopeRequirement(string RequiredScope) : IAuthorizationRequirement;
