#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Provides validation helpers for AuditLogRepository to ensure repository instances
/// are in a valid state before operations that could affect data integrity.
/// </summary>
public static class AuditLogRepositoryValidation
{
    /// <summary>
    /// Validates the AuditLogRepository instance for common issues.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this AuditLogRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate context and logger (inherited from base Repository)
        if (value._context is null)
        {
            problems.Add("Repository context is null.");
        }

        if (value._logger is null)
        {
            problems.Add("Repository logger is null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the AuditLogRepository instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to check.</param>
    /// <returns>True if the repository is valid; otherwise, false.</returns>
    public static bool IsValid(this AuditLogRepository? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the AuditLogRepository instance is valid, throwing an exception
    /// with detailed validation messages if it is not.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the repository is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static void EnsureValid(this AuditLogRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"AuditLogRepository is invalid. Problems: {string.Join(" ", problems)}",
                nameof(value));
        }
    }

    /// <summary>
    /// Validates parameters for GetByUserIdAsync method.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="count">The maximum number of logs to return.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGetByUserIdParameters(
        Guid userId,
        int count = AuditLogRepositoryValidationConstants.DefaultGetByUserIdCount)
    {
        var problems = new List<string>();

        if (count <= 0)
        {
            problems.Add(AuditLogRepositoryValidationConstants.CountMustBeGreaterThanZero);
        }

        if (count > AuditLogRepositoryValidationConstants.MaxCount)
        {
            problems.Add(string.Format(
                AuditLogRepositoryValidationConstants.CountExceedsMaximumAllowedValueFormat,
                AuditLogRepositoryValidationConstants.MaxCount));
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the GetByUserIdAsync parameters are valid.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="count">The maximum number of logs to return.</param>
    /// <returns>True if the parameters are valid; otherwise, false.</returns>
    public static bool IsGetByUserIdParametersValid(
        Guid userId,
        int count = AuditLogRepositoryValidationConstants.DefaultGetByUserIdCount) =>
        ValidateGetByUserIdParameters(userId, count).Count == 0;

    /// <summary>
    /// Ensures that the GetByUserIdAsync parameters are valid, throwing an exception
    /// with detailed validation messages if they are not.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="count">The maximum number of logs to return.</param>
    /// <exception cref="ArgumentException">Thrown if the parameters are invalid.</exception>
    public static void EnsureGetByUserIdParametersValid(
        Guid userId,
        int count = AuditLogRepositoryValidationConstants.DefaultGetByUserIdCount)
    {
        var problems = ValidateGetByUserIdParameters(userId, count);

        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Format(
                AuditLogRepositoryValidationConstants.GetByUserIdParametersInvalidFormat,
                string.Join(" ", problems)));
        }
    }

    /// <summary>
    /// Validates parameters for GetByEntityAsync method.
    /// </summary>
    /// <param name="entityType">The type of entity.</param>
    /// <param name="entityId">The entity identifier.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if entityType is null.</exception>
    public static IReadOnlyList<string> ValidateGetByEntityParameters([NotNull] string? entityType, Guid entityId)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(entityType))
        {
            problems.Add(AuditLogRepositoryValidationConstants.EntityTypeCannotBeNullOrWhiteSpace);
        }
        else if (entityType.Length > AuditLogRepositoryValidationConstants.MaxEntityTypeLength)
        {
            problems.Add(string.Format(
                AuditLogRepositoryValidationConstants.EntityTypeExceedsMaxLengthFormat,
                AuditLogRepositoryValidationConstants.MaxEntityTypeLength));
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the GetByEntityAsync parameters are valid.
    /// </summary>
    /// <param name="entityType">The type of entity.</param>
    /// <param name="entityId">The entity identifier.</param>
    /// <returns>True if the parameters are valid; otherwise, false.</returns>
    public static bool IsGetByEntityParametersValid(string? entityType, Guid entityId) =>
        ValidateGetByEntityParameters(entityType, entityId).Count == 0;

    /// <summary>
    /// Ensures that the GetByEntityAsync parameters are valid, throwing an exception
    /// with detailed validation messages if they are not.
    /// </summary>
    /// <param name="entityType">The type of entity.</param>
    /// <param name="entityId">The entity identifier.</param>
    /// <exception cref="ArgumentException">Thrown if the parameters are invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if entityType is null.</exception>
    public static void EnsureGetByEntityParametersValid(string? entityType, Guid entityId)
    {
        ArgumentException.ThrowIfNullOrEmpty(entityType);

        var problems = ValidateGetByEntityParameters(entityType, entityId);

        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Format(
                AuditLogRepositoryValidationConstants.GetByEntityParametersInvalidFormat,
                string.Join(" ", problems)));
        }
    }

    /// <summary>
    /// Validates parameters for GetRecentLogsAsync method.
    /// </summary>
    /// <param name="count">The maximum number of logs to return.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGetRecentLogsParameters(
        int count = AuditLogRepositoryValidationConstants.DefaultGetRecentLogsCount)
    {
        var problems = new List<string>();

        if (count <= 0)
        {
            problems.Add(AuditLogRepositoryValidationConstants.CountMustBeGreaterThanZero);
        }

        if (count > AuditLogRepositoryValidationConstants.MaxCount)
        {
            problems.Add(string.Format(
                AuditLogRepositoryValidationConstants.CountExceedsMaximumAllowedValueFormat,
                AuditLogRepositoryValidationConstants.MaxCount));
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the GetRecentLogsAsync parameters are valid.
    /// </summary>
    /// <param name="count">The maximum number of logs to return.</param>
    /// <returns>True if the parameters are valid; otherwise, false.</returns>
    public static bool IsGetRecentLogsParametersValid(
        int count = AuditLogRepositoryValidationConstants.DefaultGetRecentLogsCount) =>
        ValidateGetRecentLogsParameters(count).Count == 0;

    /// <summary>
    /// Ensures that the GetRecentLogsAsync parameters are valid, throwing an exception
    /// with detailed validation messages if they are not.
    /// </summary>
    /// <param name="count">The maximum number of logs to return.</param>
    /// <exception cref="ArgumentException">Thrown if the parameters are invalid.</exception>
    public static void EnsureGetRecentLogsParametersValid(
        int count = AuditLogRepositoryValidationConstants.DefaultGetRecentLogsCount)
    {
        var problems = ValidateGetRecentLogsParameters(count);

        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Format(
                AuditLogRepositoryValidationConstants.GetRecentLogsParametersInvalidFormat,
                string.Join(" ", problems)));
        }
    }

    /// <summary>
    /// Validates parameters for GetFailedActionsAsync method.
    /// </summary>
    /// <param name="count">The maximum number of failed actions to return.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGetFailedActionsParameters(
        int count = AuditLogRepositoryValidationConstants.DefaultGetFailedActionsCount)
    {
        var problems = new List<string>();

        if (count <= 0)
        {
            problems.Add(AuditLogRepositoryValidationConstants.CountMustBeGreaterThanZero);
        }

        if (count > AuditLogRepositoryValidationConstants.MaxCount)
        {
            problems.Add(string.Format(
                AuditLogRepositoryValidationConstants.CountExceedsMaximumAllowedValueFormat,
                AuditLogRepositoryValidationConstants.MaxCount));
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the GetFailedActionsAsync parameters are valid.
    /// </summary>
    /// <param name="count">The maximum number of failed actions to return.</param>
    /// <returns>True if the parameters are valid; otherwise, false.</returns>
    public static bool IsGetFailedActionsParametersValid(
        int count = AuditLogRepositoryValidationConstants.DefaultGetFailedActionsCount) =>
        ValidateGetFailedActionsParameters(count).Count == 0;

    /// <summary>
    /// Ensures that the GetFailedActionsAsync parameters are valid, throwing an exception
    /// with detailed validation messages if they are not.
    /// </summary>
    /// <param name="count">The maximum number of failed actions to return.</param>
    /// <exception cref="ArgumentException">Thrown if the parameters are invalid.</exception>
    public static void EnsureGetFailedActionsParametersValid(
        int count = AuditLogRepositoryValidationConstants.DefaultGetFailedActionsCount)
    {
        var problems = ValidateGetFailedActionsParameters(count);

        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Format(
                AuditLogRepositoryValidationConstants.GetFailedActionsParametersInvalidFormat,
                string.Join(" ", problems)));
        }
    }

    /// <summary>
    /// Validates parameters for DeleteOldLogsAsync method.
    /// </summary>
    /// <param name="daysToKeep">The number of days to keep logs.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateDeleteOldLogsParameters(
        int daysToKeep = AuditLogRepositoryValidationConstants.DefaultDeleteOldLogsDaysToKeep)
    {
        var problems = new List<string>();

        if (daysToKeep <= 0)
        {
            problems.Add(AuditLogRepositoryValidationConstants.DaysToKeepMustBeGreaterThanZero);
        }

        if (daysToKeep > AuditLogRepositoryValidationConstants.MaxDeleteOldLogsDaysToKeep)
        {
            problems.Add(string.Format(
                AuditLogRepositoryValidationConstants.DaysToKeepExceedsMaximumAllowedValueFormat,
                AuditLogRepositoryValidationConstants.MaxDeleteOldLogsDaysToKeep));
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the DeleteOldLogsAsync parameters are valid.
    /// </summary>
    /// <param name="daysToKeep">The number of days to keep logs.</param>
    /// <returns>True if the parameters are valid; otherwise, false.</returns>
    public static bool IsDeleteOldLogsParametersValid(
        int daysToKeep = AuditLogRepositoryValidationConstants.DefaultDeleteOldLogsDaysToKeep) =>
        ValidateDeleteOldLogsParameters(daysToKeep).Count == 0;

    /// <summary>
    /// Ensures that the DeleteOldLogsAsync parameters are valid, throwing an exception
    /// with detailed validation messages if they are not.
    /// </summary>
    /// <param name="daysToKeep">The number of days to keep logs.</param>
    /// <exception cref="ArgumentException">Thrown if the parameters are invalid.</exception>
    public static void EnsureDeleteOldLogsParametersValid(
        int daysToKeep = AuditLogRepositoryValidationConstants.DefaultDeleteOldLogsDaysToKeep)
    {
        var problems = ValidateDeleteOldLogsParameters(daysToKeep);

        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Format(
                AuditLogRepositoryValidationConstants.DeleteOldLogsParametersInvalidFormat,
                string.Join(" ", problems)));
        }
    }
}
