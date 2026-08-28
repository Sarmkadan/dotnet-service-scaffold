#nullable enable

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Fluent builder for <see cref="ApiKey"/> instances.
/// </summary>
public class ApiKeyBuilder
{
    private Guid _id;
    private Guid _userId;
    private User? _user;
    private string _name = default!;
    private string _keyHash = default!;
    private string _keyPrefix = default!;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime? _expiresAt;
    private DateTime? _lastUsedAt;
    private bool _isActive = true;
    private string? _allowedIps;
    private string? _allowedScopes;
    private long _apiCallsCount;
    private string? _description;

    /// <summary>
    /// Sets the unique identifier for the API key.
    /// </summary>
    /// <param name="id">The API key identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ApiKeyBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the user identifier.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ApiKeyBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    /// <summary>
    /// Sets the user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ApiKeyBuilder WithUser(User? user)
    {
        _user = user;
        return this;
    }

    /// <summary>
    /// Sets the name. Must not be null, empty, or whitespace.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null, empty, or whitespace.</exception>
    public ApiKeyBuilder WithName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be whitespace.", nameof(name));
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the key hash. Must not be null, empty, or whitespace.
    /// </summary>
    /// <param name="keyHash">The key hash.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="keyHash"/> is null, empty, or whitespace.</exception>
    public ApiKeyBuilder WithKeyHash(string keyHash)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyHash);
        if (string.IsNullOrWhiteSpace(keyHash))
            throw new ArgumentException("KeyHash cannot be whitespace.", nameof(keyHash));
        _keyHash = keyHash;
        return this;
    }

    /// <summary>
    /// Sets the key prefix. Must not be null, empty, or whitespace.
    /// </summary>
    /// <param name="keyPrefix">The key prefix.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="keyPrefix"/> is null, empty, or whitespace.</exception>
    public ApiKeyBuilder WithKeyPrefix(string keyPrefix)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyPrefix);
        if (string.IsNullOrWhiteSpace(keyPrefix))
            throw new ArgumentException("KeyPrefix cannot be whitespace.", nameof(keyPrefix));
        _keyPrefix = keyPrefix;
        return this;
    }

    /// <summary>
    /// Sets the creation timestamp.
    /// </summary>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ApiKeyBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Sets the expiration timestamp.
    /// </summary>
    /// <param name="expiresAt">The expiration timestamp.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ApiKeyBuilder WithExpiresAt(DateTime? expiresAt)
    {
        _expiresAt = expiresAt;
        return this;
    }

    /// <summary>
    /// Sets the last used timestamp.
    /// </summary>
    /// <param name="lastUsedAt">The last used timestamp.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ApiKeyBuilder WithLastUsedAt(DateTime? lastUsedAt)
    {
        _lastUsedAt = lastUsedAt;
        return this;
    }

    /// <summary>
    /// Sets whether the API key is active.
    /// </summary>
    /// <param name="isActive">Whether the API key is active.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ApiKeyBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    /// <summary>
    /// Sets the allowed IP addresses (comma-separated).
    /// </summary>
    /// <param name="allowedIps">The allowed IP addresses.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ApiKeyBuilder WithAllowedIps(string? allowedIps)
    {
        _allowedIps = allowedIps;
        return this;
    }

    /// <summary>
    /// Sets the allowed scopes (comma-separated).
    /// </summary>
    /// <param name="allowedScopes">The allowed scopes.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ApiKeyBuilder WithAllowedScopes(string? allowedScopes)
    {
        _allowedScopes = allowedScopes;
        return this;
    }

    /// <summary>
    /// Sets the API calls count.
    /// </summary>
    /// <param name="apiCallsCount">The API calls count.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ApiKeyBuilder WithApiCallsCount(long apiCallsCount)
    {
        _apiCallsCount = apiCallsCount;
        return this;
    }

    /// <summary>
    /// Sets the description.
    /// </summary>
    /// <param name="description">The description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ApiKeyBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Creates a builder pre-filled with values from an existing <see cref="ApiKey"/>.
    /// </summary>
    /// <param name="template">The API key to copy values from.</param>
    /// <returns>A new builder instance initialized with the template's values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static ApiKeyBuilder From(ApiKey template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new ApiKeyBuilder()
            .WithId(template.Id)
            .WithUserId(template.UserId)
            .WithUser(template.User)
            .WithName(template.Name)
            .WithKeyHash(template.KeyHash)
            .WithKeyPrefix(template.KeyPrefix)
            .WithCreatedAt(template.CreatedAt)
            .WithExpiresAt(template.ExpiresAt)
            .WithLastUsedAt(template.LastUsedAt)
            .WithIsActive(template.IsActive)
            .WithAllowedIps(template.AllowedIps)
            .WithAllowedScopes(template.AllowedScopes)
            .WithApiCallsCount(template.ApiCallsCount)
            .WithDescription(template.Description);
    }

    /// <summary>
    /// Builds the <see cref="ApiKey"/> instance with the current values.
    /// </summary>
    /// <returns>A fully configured <see cref="ApiKey"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public ApiKey Build()
    {
        // Validate required properties
        if (string.IsNullOrWhiteSpace(_name))
            throw new ArgumentException("Name is required.", nameof(_name));
        if (string.IsNullOrWhiteSpace(_keyHash))
            throw new ArgumentException("KeyHash is required.", nameof(_keyHash));
        if (string.IsNullOrWhiteSpace(_keyPrefix))
            throw new ArgumentException("KeyPrefix is required.", nameof(_keyPrefix));

        return new ApiKey
        {
            Id = _id,
            UserId = _userId,
            User = _user,
            Name = _name!,
            KeyHash = _keyHash!,
            KeyPrefix = _keyPrefix!,
            CreatedAt = _createdAt,
            ExpiresAt = _expiresAt,
            LastUsedAt = _lastUsedAt,
            IsActive = _isActive,
            AllowedIps = _allowedIps,
            AllowedScopes = _allowedScopes,
            ApiCallsCount = _apiCallsCount,
            Description = _description
        };
    }
}