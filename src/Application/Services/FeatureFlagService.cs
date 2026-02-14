#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Serilog;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Service for managing feature flags to enable/disable features at runtime.
/// Allows gradual feature rollout, A/B testing, and quick disable of problematic features.
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    private readonly Dictionary<string, FeatureFlag> _flags;
    private readonly ILogger<FeatureFlagService> _logger;

    public FeatureFlagService(ILogger<FeatureFlagService> logger)
    {
        _logger = logger;
        _flags = new Dictionary<string, FeatureFlag>(StringComparer.OrdinalIgnoreCase)
        {
            // Initialize default feature flags
            { "audit_logging", new FeatureFlag("audit_logging", "Enable audit logging", true) },
            { "rate_limiting", new FeatureFlag("rate_limiting", "Enable rate limiting", true) },
            { "health_checks", new FeatureFlag("health_checks", "Enable periodic health checks", true) },
            { "webhooks", new FeatureFlag("webhooks", "Enable webhook support", true) },
            { "metrics", new FeatureFlag("metrics", "Enable metrics collection", true) },
            { "advanced_analytics", new FeatureFlag("advanced_analytics", "Enable advanced analytics", false) }
        };

        _logger.LogInformation("FeatureFlagService initialized with {Count} flags", _flags.Count);
    }

    /// <summary>
    /// Checks if a feature is enabled.
    /// </summary>
    public bool IsEnabled(string featureName)
    {
        if (!_flags.TryGetValue(featureName, out var flag))
        {
            _logger.LogWarning("Feature flag '{FeatureName}' not found, defaulting to false", featureName);
            return false;
        }

        return flag.IsEnabled;
    }

    /// <summary>
    /// Checks if a feature is enabled for a specific user.
    /// Allows per-user feature toggling for A/B testing.
    /// </summary>
    public bool IsEnabledForUser(string featureName, Guid userId)
    {
        if (!IsEnabled(featureName))
            return false;

        if (!_flags.TryGetValue(featureName, out var flag))
            return false;

        // Simple user-based rollout: hash user ID to determine inclusion
        if (flag.RolloutPercentage < 100)
        {
            var hash = userId.GetHashCode() % 100;
            return hash < flag.RolloutPercentage;
        }

        return true;
    }

    /// <summary>
    /// Enables a feature.
    /// </summary>
    public void EnableFeature(string featureName)
    {
        if (_flags.TryGetValue(featureName, out var flag))
        {
            flag.IsEnabled = true;
            flag.LastModified = DateTime.UtcNow;
            _logger.LogInformation("Feature '{FeatureName}' enabled", featureName);
        }
        else
        {
            _logger.LogWarning("Feature '{FeatureName}' not found for enabling", featureName);
        }
    }

    /// <summary>
    /// Disables a feature.
    /// </summary>
    public void DisableFeature(string featureName)
    {
        if (_flags.TryGetValue(featureName, out var flag))
        {
            flag.IsEnabled = false;
            flag.LastModified = DateTime.UtcNow;
            _logger.LogInformation("Feature '{FeatureName}' disabled", featureName);
        }
        else
        {
            _logger.LogWarning("Feature '{FeatureName}' not found for disabling", featureName);
        }
    }

    /// <summary>
    /// Sets the rollout percentage for a feature (for gradual rollout).
    /// </summary>
    public void SetRolloutPercentage(string featureName, int percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Rollout percentage must be between 0 and 100", nameof(percentage));

        if (_flags.TryGetValue(featureName, out var flag))
        {
            flag.RolloutPercentage = percentage;
            flag.LastModified = DateTime.UtcNow;
            _logger.LogInformation(
                "Feature '{FeatureName}' rollout percentage set to {Percentage}%",
                featureName, percentage);
        }
        else
        {
            _logger.LogWarning("Feature '{FeatureName}' not found for rollout", featureName);
        }
    }

    /// <summary>
    /// Registers a new feature flag.
    /// </summary>
    public void RegisterFeature(string featureName, string description, bool initiallyEnabled = false)
    {
        if (_flags.ContainsKey(featureName))
        {
            _logger.LogWarning("Feature '{FeatureName}' already registered", featureName);
            return;
        }

        _flags[featureName] = new FeatureFlag(featureName, description, initiallyEnabled);
        _logger.LogInformation(
            "Feature '{FeatureName}' registered (enabled: {Enabled})",
            featureName, initiallyEnabled);
    }

    /// <summary>
    /// Gets all feature flags.
    /// </summary>
    public IEnumerable<FeatureFlagInfo> GetAllFlags()
    {
        return _flags.Values.Select(f => new FeatureFlagInfo
        {
            Name = f.Name,
            Description = f.Description,
            IsEnabled = f.IsEnabled,
            RolloutPercentage = f.RolloutPercentage,
            CreatedAt = f.CreatedAt,
            LastModified = f.LastModified
        });
    }

    /// <summary>
    /// Gets a specific feature flag.
    /// </summary>
    public FeatureFlagInfo? GetFlag(string featureName)
    {
        if (!_flags.TryGetValue(featureName, out var flag))
            return null;

        return new FeatureFlagInfo
        {
            Name = flag.Name,
            Description = flag.Description,
            IsEnabled = flag.IsEnabled,
            RolloutPercentage = flag.RolloutPercentage,
            CreatedAt = flag.CreatedAt,
            LastModified = flag.LastModified
        };
    }
}

/// <summary>
/// Interface for feature flag service.
/// </summary>
public interface IFeatureFlagService
{
    bool IsEnabled(string featureName);
    bool IsEnabledForUser(string featureName, Guid userId);
    void EnableFeature(string featureName);
    void DisableFeature(string featureName);
    void SetRolloutPercentage(string featureName, int percentage);
    void RegisterFeature(string featureName, string description, bool initiallyEnabled = false);
    IEnumerable<FeatureFlagInfo> GetAllFlags();
    FeatureFlagInfo? GetFlag(string featureName);
}

/// <summary>
/// Internal class for feature flag state.
/// </summary>
internal class FeatureFlag
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsEnabled { get; set; }
    public int RolloutPercentage { get; set; } = 100;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    public FeatureFlag(string name, string description, bool isEnabled)
    {
        Name = name;
        Description = description;
        IsEnabled = isEnabled;
    }
}

/// <summary>
/// DTO for feature flag information.
/// </summary>
public class FeatureFlagInfo
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public int RolloutPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
}
