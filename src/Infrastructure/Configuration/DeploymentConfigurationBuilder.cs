using System;
using System.Collections.Generic;

namespace DotnetServiceScaffold.Infrastructure.Configuration;

/// <summary>
/// Builder for <see cref="DeploymentOptions"/>.
/// </summary>
public class DeploymentConfigurationBuilder
{
    private string? _serviceName;
    private string? _serviceDescription;
    private string? _serviceUser;
    private string? _applicationPath;
    private string? _dataPath;
    private string? _logPath;
    private string? _serverDomain;
    private int _applicationPort;
    private string? _dotnetPath;
    private string? _serviceVersion;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeploymentConfigurationBuilder"/> class.
    /// </summary>
    public DeploymentConfigurationBuilder()
    {
    }

    /// <summary>
    /// Creates a new builder instance from an existing <see cref="DeploymentOptions"/>.
    /// </summary>
    /// <param name="template">The template to start from.</param>
    /// <returns>A new builder instance pre-filled with the template's values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when template is null.</exception>
    public static DeploymentConfigurationBuilder From(DeploymentOptions template)
    {
        ArgumentNullException.ThrowIfNull(template);

        return new DeploymentConfigurationBuilder
        {
            _serviceName = template.ServiceName,
            _serviceDescription = template.ServiceDescription,
            _serviceUser = template.ServiceUser,
            _applicationPath = template.ApplicationPath,
            _dataPath = template.DataPath,
            _logPath = template.LogPath,
            _serverDomain = template.ServerDomain,
            _applicationPort = template.ApplicationPort,
            _dotnetPath = template.DotnetPath,
            _serviceVersion = template.ServiceVersion
        };
    }

    /// <summary>
    /// Sets the ServiceName.
    /// </summary>
    public DeploymentConfigurationBuilder WithServiceName(string serviceName)
    {
        ArgumentException.ThrowIfNullOrEmpty(serviceName);
        _serviceName = serviceName;
        return this;
    }

    /// <summary>
    /// Sets the ServiceDescription.
    /// </summary>
    public DeploymentConfigurationBuilder WithServiceDescription(string serviceDescription)
    {
        ArgumentException.ThrowIfNullOrEmpty(serviceDescription);
        _serviceDescription = serviceDescription;
        return this;
    }

    /// <summary>
    /// Sets the ServiceUser.
    /// </summary>
    public DeploymentConfigurationBuilder WithServiceUser(string serviceUser)
    {
        ArgumentException.ThrowIfNullOrEmpty(serviceUser);
        _serviceUser = serviceUser;
        return this;
    }

    /// <summary>
    /// Sets the ApplicationPath.
    /// </summary>
    public DeploymentConfigurationBuilder WithApplicationPath(string applicationPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(applicationPath);
        _applicationPath = applicationPath;
        return this;
    }

    /// <summary>
    /// Sets the DataPath.
    /// </summary>
    public DeploymentConfigurationBuilder WithDataPath(string dataPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(dataPath);
        _dataPath = dataPath;
        return this;
    }

    /// <summary>
    /// Sets the LogPath.
    /// </summary>
    public DeploymentConfigurationBuilder WithLogPath(string logPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(logPath);
        _logPath = logPath;
        return this;
    }

    /// <summary>
    /// Sets the ServerDomain.
    /// </summary>
    public DeploymentConfigurationBuilder WithServerDomain(string serverDomain)
    {
        ArgumentException.ThrowIfNullOrEmpty(serverDomain);
        _serverDomain = serverDomain;
        return this;
    }

    /// <summary>
    /// Sets the ApplicationPort.
    /// </summary>
    public DeploymentConfigurationBuilder WithApplicationPort(int applicationPort)
    {
        if (applicationPort <= 0 || applicationPort > 65535)
        {
            throw new ArgumentException("Port must be between 1 and 65535", nameof(applicationPort));
        }
        _applicationPort = applicationPort;
        return this;
    }

    /// <summary>
    /// Sets the DotnetPath.
    /// </summary>
    public DeploymentConfigurationBuilder WithDotnetPath(string dotnetPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(dotnetPath);
        _dotnetPath = dotnetPath;
        return this;
    }

    /// <summary>
    /// Sets the ServiceVersion.
    /// </summary>
    public DeploymentConfigurationBuilder WithServiceVersion(string serviceVersion)
    {
        ArgumentException.ThrowIfNullOrEmpty(serviceVersion);
        _serviceVersion = serviceVersion;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="DeploymentOptions"/> instance.
    /// </summary>
    /// <returns>A configured <see cref="DeploymentOptions"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public DeploymentOptions Build()
    {
        ValidateProperty(_serviceName, nameof(_serviceName));
        ValidateProperty(_serviceDescription, nameof(_serviceDescription));
        ValidateProperty(_serviceUser, nameof(_serviceUser));
        ValidateProperty(_applicationPath, nameof(_applicationPath));
        ValidateProperty(_dataPath, nameof(_dataPath));
        ValidateProperty(_logPath, nameof(_logPath));
        ValidateProperty(_serverDomain, nameof(_serverDomain));
        ValidateProperty(_dotnetPath, nameof(_dotnetPath));
        ValidateProperty(_serviceVersion, nameof(_serviceVersion));
        
        if (_applicationPort <= 0)
        {
            throw new ArgumentException("ApplicationPort is required and must be greater than 0.");
        }

        return new DeploymentOptions
        {
            ServiceName = _serviceName!,
            ServiceDescription = _serviceDescription!,
            ServiceUser = _serviceUser!,
            ApplicationPath = _applicationPath!,
            DataPath = _dataPath!,
            LogPath = _logPath!,
            ServerDomain = _serverDomain!,
            ApplicationPort = _applicationPort,
            DotnetPath = _dotnetPath!,
            ServiceVersion = _serviceVersion!
        };
    }

    private static void ValidateProperty(string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Property {propertyName} is required.", propertyName);
        }
    }
}
