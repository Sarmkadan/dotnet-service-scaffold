#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using Microsoft.AspNetCore.Authorization;

namespace DotnetServiceScaffold.Presentation.Authorization;

/// <summary>
/// Succeeds when the principal carries a scope claim that satisfies the requirement's scope.
/// </summary>
public sealed class ApiKeyScopeHandler : AuthorizationHandler<ApiKeyScopeRequirement>
{
    /// <summary>
    /// Evaluates the scope requirement against the principal's scope claims.
    /// </summary>
    /// <param name="context">The authorization context for the current request.</param>
    /// <param name="requirement">The requirement to evaluate.</param>
    /// <returns>A completed task. The requirement is marked succeeded when a matching scope is present.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="requirement"/> is <see langword="null"/>.</exception>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyScopeRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        var grantingScopes = ApiKeyScopes.GetGrantingScopes(requirement.RequiredScope);
        var isGranted = context.User.FindAll(ApiKeyClaimTypes.Scope).Any(claim => grantingScopes.Contains(claim.Value));

        if (isGranted)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
