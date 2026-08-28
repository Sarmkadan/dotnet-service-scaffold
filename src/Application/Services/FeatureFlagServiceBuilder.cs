#nullable enable

using System;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Fluent builder for <see cref="FeatureFlagInfo"/> instances.
/// </summary>
public class FeatureFlagServiceBuilder
{
    private string? _name;
    private string? _description;
    private bool _isEnabled;
    private int _rolloutPercentage;
    private DateTime _createdAt;
    private DateTime _lastModified;

    /// <summary>
    /// Sets the name of the feature flag.
    /// </summary>
    /// <param name="name">The feature flag name.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null, empty, or whitespace.</exception>
    public FeatureFlagServiceBuilder WithName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the description of the feature flag.
    /// </summary>
    /// <param name="description">The feature flag description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FeatureFlagServiceBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets whether the feature flag is enabled.
    /// </summary>
    /// <param name="isEnabled">Whether the feature flag is enabled.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FeatureFlagServiceBuilder WithIsEnabled(bool isEnabled)
    {
        _isEnabled = isEnabled;
        return this;
    }

    /// <summary>
    /// Sets the rollout percentage for the feature flag (0-100).
    /// </summary>
    /// <param name="rolloutPercentage">The rollout percentage.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="rolloutPercentage"/> is less than 0 or greater than 100.</exception>
    public FeatureFlagServiceBuilder WithRolloutPercentage(int rolloutPercentage)
    {
        if (rolloutPercentage < 0 || rolloutPercentage > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(rolloutPercentage), "Rollout percentage must be between 0 and 100.");
        }

        _rolloutPercentage = rolloutPercentage;
        return this;
    }

    /// <summary>
    /// Sets the creation timestamp of the feature flag.
    /// </summary>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FeatureFlagServiceBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Sets the last modified timestamp of the feature flag.
    /// </summary>
    /// <param name="lastModified">The last modified timestamp.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FeatureFlagServiceBuilder WithLastModified(DateTime lastModified)
    {
        _lastModified = lastModified;
        return this;
    }

    /// <summary>
    /// Creates a builder pre-filled with values from an existing <see cref="FeatureFlagInfo"/>.
    /// </summary>
    /// <param name="template">The feature flag info to copy values from.</param>
    /// <returns>A new builder instance initialized with the template's values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static FeatureFlagServiceBuilder From(FeatureFlagInfo template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new FeatureFlagServiceBuilder()
            .WithName(template.Name!)
            .WithDescription(template.Description)
            .WithIsEnabled(template.IsEnabled)
            .WithRolloutPercentage(template.RolloutPercentage)
            .WithCreatedAt(template.CreatedAt)
            .WithLastModified(template.LastModified);
    }

    /// <summary>
    /// Builds the <see cref="FeatureFlagInfo"/> instance with the current values.
    /// </summary>
    /// <returns>A fully configured <see cref="FeatureFlagInfo"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public FeatureFlagInfo Build()
    {
        // Validate required properties
        if (_name == null)
            throw new ArgumentException("Name is required.", nameof(_name));

        return new FeatureFlagInfo
        {
            Name = _name,
            Description = _description,
            IsEnabled = _isEnabled,
            RolloutPercentage = _rolloutPercentage,
            CreatedAt = _createdAt,
            LastModified = _lastModified
        };
    }
}