#nullable enable

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Fluent builder for <see cref="AuditLog"/> instances.
/// </summary>
public class AuditLogBuilder
{
    private Guid _id;
    private Guid? _userId;
    private User? _user;
    private string _actionName = default!;
    private string _entityType = default!;
    private Guid? _entityId;
    private string? _oldValues;
    private string? _newValues;
    private string? _status;
    private string? _ipAddress;

    /// <summary>
    /// Sets the unique identifier for the audit log.
    /// </summary>
    /// <param name="id">The audit log identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AuditLogBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the user identifier.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AuditLogBuilder WithUserId(Guid? userId)
    {
        _userId = userId;
        return this;
    }

    /// <summary>
    /// Sets the user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AuditLogBuilder WithUser(User? user)
    {
        _user = user;
        return this;
    }

    /// <summary>
    /// Sets the action name. Must not be null, empty, or whitespace.
    /// </summary>
    /// <param name="actionName">The action name.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="actionName"/> is null, empty, or whitespace.</exception>
    public AuditLogBuilder WithActionName(string actionName)
    {
        ArgumentException.ThrowIfNullOrEmpty(actionName);
        if (string.IsNullOrWhiteSpace(actionName))
            throw new ArgumentException("ActionName cannot be whitespace.", nameof(actionName));
        _actionName = actionName;
        return this;
    }

    /// <summary>
    /// Sets the entity type. Must not be null, empty, or whitespace.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="entityType"/> is null, empty, or whitespace.</exception>
    public AuditLogBuilder WithEntityType(string entityType)
    {
        ArgumentException.ThrowIfNullOrEmpty(entityType);
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("EntityType cannot be whitespace.", nameof(entityType));
        _entityType = entityType;
        return this;
    }

    /// <summary>
    /// Sets the entity identifier.
    /// </summary>
    /// <param name="entityId">The entity identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AuditLogBuilder WithEntityId(Guid? entityId)
    {
        _entityId = entityId;
        return this;
    }

    /// <summary>
    /// Sets the old values (JSON representation of the entity before the action).
    /// </summary>
    /// <param name="oldValues">The old values.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AuditLogBuilder WithOldValues(string? oldValues)
    {
        _oldValues = oldValues;
        return this;
    }

    /// <summary>
    /// Sets the new values (JSON representation of the entity after the action).
    /// </summary>
    /// <param name="newValues">The new values.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AuditLogBuilder WithNewValues(string? newValues)
    {
        _newValues = newValues;
        return this;
    }

    /// <summary>
    /// Sets the status of the action (e.g., Success, Failed).
    /// </summary>
    /// <param name="status">The status.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AuditLogBuilder WithStatus(string? status)
    {
        _status = status;
        return this;
    }

    /// <summary>
    /// Sets the IP address from which the action originated.
    /// </summary>
    /// <param name="ipAddress">The IP address.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AuditLogBuilder WithIpAddress(string? ipAddress)
    {
        _ipAddress = ipAddress;
        return this;
    }

    /// <summary>
    /// Creates a builder pre-filled with values from an existing <see cref="AuditLog"/>.
    /// </summary>
    /// <param name="template">The audit log to copy values from.</param>
    /// <returns>A new builder instance initialized with the template's values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static AuditLogBuilder From(AuditLog template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new AuditLogBuilder()
            .WithId(template.Id)
            .WithUserId(template.UserId)
            .WithUser(template.User)
            .WithActionName(template.ActionName)
            .WithEntityType(template.EntityType)
            .WithEntityId(template.EntityId)
            .WithOldValues(template.OldValues)
            .WithNewValues(template.NewValues)
            .WithStatus(template.Status)
            .WithIpAddress(template.IpAddress);
    }

    /// <summary>
    /// Builds the <see cref="AuditLog"/> instance with the current values.
    /// </summary>
    /// <returns>A fully configured <see cref="AuditLog"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public AuditLog Build()
    {
        // Validate required properties
        if (string.IsNullOrWhiteSpace(_actionName))
            throw new ArgumentException("ActionName is required.", nameof(_actionName));
        if (string.IsNullOrWhiteSpace(_entityType))
            throw new ArgumentException("EntityType is required.", nameof(_entityType));

        return new AuditLog
        {
            Id = _id,
            UserId = _userId,
            User = _user,
            ActionName = _actionName!,
            EntityType = _entityType!,
            EntityId = _entityId,
            OldValues = _oldValues,
            NewValues = _newValues,
            Status = _status,
            IpAddress = _ipAddress
        };
    }
}