using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetServiceScaffold.Infrastructure.Data;

/// <summary>
/// Provides extension methods for the <see cref="ServiceScaffoldDbContext"/>.
/// </summary>
public static class ServiceScaffoldDbContextExtensions
{
    /// <summary>
    /// Retrieves a user by their email address asynchronously.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The <see cref="User"/> if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context or email is null.</exception>
    /// <exception cref="ArgumentException">Thrown when email is empty or whitespace.</exception>
    public static async Task<User?> GetUserByEmailAsync(this ServiceScaffoldDbContext context, string email)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrEmpty(email);

        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower(CultureInfo.InvariantCulture) == email.ToLower(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Retrieves all service registrations asynchronously.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <returns>A list of <see cref="ServiceRegistration"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    public static async Task<IReadOnlyList<ServiceRegistration>> GetServicesAsync(this ServiceScaffoldDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return await context.ServiceRegistrations
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves audit logs for a specific user asynchronously.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of <see cref="AuditLog"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    public static async Task<IReadOnlyList<AuditLog>> GetAuditLogsForUserAsync(this ServiceScaffoldDbContext context, Guid userId)
    {
        ArgumentNullException.ThrowIfNull(context);

        return await context.AuditLogs
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a configuration value by key and service ID asynchronously.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="key">The configuration key.</param>
    /// <param name="serviceId">The service ID.</param>
    /// <returns>The configuration value if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context or key is null.</exception>
    /// <exception cref="ArgumentException">Thrown when key is empty or whitespace.</exception>
    public static async Task<string?> GetConfigurationValueAsync(this ServiceScaffoldDbContext context, string key, Guid serviceId)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var config = await context.ServiceConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Key == key && c.ServiceId == serviceId);

        return config?.Value;
    }
}
