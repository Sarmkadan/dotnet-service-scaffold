// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Configuration;

/// <summary>
/// Deployment configuration options for systemd service and Caddy reverse proxy.
/// Provides templates and helpers for deploying the application in production environments.
/// </summary>
public class DeploymentConfiguration
{
    /// <summary>
    /// Generates a systemd service unit file for the application.
    /// </summary>
    public static string GenerateSystemdServiceUnit(DeploymentOptions options)
    {
        return $@"[Unit]
Description={options.ServiceDescription}
After=network.target

[Service]
Type=notify
User={options.ServiceUser}
WorkingDirectory={options.ApplicationPath}
ExecStart={options.DotnetPath} DotnetServiceScaffold.dll
Restart=on-failure
RestartSec=10
StandardOutput=journal
StandardError=journal
SyslogIdentifier={options.ServiceName}

# Security settings
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths={options.DataPath}

[Install]
WantedBy=multi-user.target
";
    }

    /// <summary>
    /// Generates a Caddy configuration block for reverse proxy and TLS.
    /// </summary>
    public static string GenerateCaddyConfiguration(DeploymentOptions options)
    {
        return $@"# Caddy reverse proxy configuration for {options.ServiceName}
{options.ServerDomain} {{
    # Encode gzip response
    encode gzip

    # Reverse proxy to the application
    reverse_proxy localhost:{options.ApplicationPort} {{
        # Headers to pass through
        header_upstream X-Forwarded-For {{{{http.request.header.X-Forwarded-For}}}}
        header_upstream X-Forwarded-Proto {{{{http.request.proto}}}}
        header_upstream X-Real-IP {{{{http.client.ip}}}}

        # Timeouts
        timeout 30s

        # Health check
        uri /health
        interval 30s
        timeout 5s
        unhealthy_status 500 502 503
        unhealthy_latency 5s
    }}

    # Logs
    log {{
        output file {options.LogPath}/caddy.log {{
            roll_size 100mb
            roll_keep 10
            roll_keep_for 168h
        }}
    }}

    # Rate limiting (optional)
    # rate_limit {{
    #     zone general {{
    #         key {{{{http.request.remote.ip}}}}
    #         rate 100r/s
    #     }}
    # }}
}}
";
    }

    /// <summary>
    /// Generates an environment file for systemd.
    /// </summary>
    public static string GenerateEnvironmentFile(DeploymentOptions options)
    {
        return $@"# Environment variables for {options.ServiceName}
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://localhost:{options.ApplicationPort}
DOTNET_CLI_TELEMETRY_OPTOUT=1

# Database
ConnectionStrings__DefaultConnection=Data Source={options.DataPath}/scaffold.db

# Logging
SERILOG_LOG_LEVEL=Information

# Application
SERVICE_NAME={options.ServiceName}
SERVICE_VERSION={options.ServiceVersion}
";
    }

    /// <summary>
    /// Generates deployment documentation with step-by-step instructions.
    /// </summary>
    public static string GenerateDeploymentGuide(DeploymentOptions options)
    {
        return $@"# Deployment Guide for {options.ServiceName}

## Prerequisites
- Linux server (Debian/Ubuntu recommended)
- .NET Runtime 10.0 or later
- Caddy web server (for HTTPS and reverse proxy)
- systemd (for service management)

## Installation Steps

### 1. Prepare the environment
```bash
sudo useradd -r -s /bin/false {options.ServiceUser}
sudo mkdir -p {options.ApplicationPath} {options.DataPath} {options.LogPath}
sudo chown {options.ServiceUser}:{options.ServiceUser} {options.DataPath} {options.LogPath}
sudo chmod 750 {options.DataPath} {options.LogPath}
```

### 2. Deploy the application
```bash
# Copy application files
sudo cp -r dotnet-service-scaffold/* {options.ApplicationPath}/
sudo chown -R {options.ServiceUser}:{options.ServiceUser} {options.ApplicationPath}
sudo chmod 755 {options.ApplicationPath}
```

### 3. Install systemd service
```bash
# Copy the service file
sudo cp systemd/scaffold.service /etc/systemd/system/{options.ServiceName}.service
sudo systemctl daemon-reload
sudo systemctl enable {options.ServiceName}
sudo systemctl start {options.ServiceName}
```

### 4. Configure Caddy
```bash
# Copy Caddy configuration
sudo cp caddy/Caddyfile /etc/caddy/Caddyfile
sudo systemctl reload caddy
```

### 5. Verify installation
```bash
# Check service status
sudo systemctl status {options.ServiceName}

# Test health check
curl https://{options.ServerDomain}/health

# Check logs
sudo journalctl -u {options.ServiceName} -f
```

## Monitoring

### View service logs
```bash
sudo journalctl -u {options.ServiceName} -n 100 -f
```

### Check service status
```bash
sudo systemctl status {options.ServiceName}
```

### Restart the service
```bash
sudo systemctl restart {options.ServiceName}
```

## Updates

### Update the application
```bash
sudo systemctl stop {options.ServiceName}
sudo cp -r dotnet-service-scaffold/* {options.ApplicationPath}/
sudo systemctl start {options.ServiceName}
```

## Security Notes

- Database files are stored in {options.DataPath}
- Ensure regular backups of {options.DataPath}
- Configure firewall to only allow ports 80 and 443
- Enable HTTPS with valid SSL certificates
- Keep .NET runtime and system packages updated
";
    }
}

/// <summary>
/// Options for deployment configuration.
/// </summary>
public class DeploymentOptions
{
    public string ServiceName { get; set; } = "dotnet-scaffold";
    public string ServiceDescription { get; set; } = "DotNet Service Scaffold Application";
    public string ServiceUser { get; set; } = "scaffold";
    public string ApplicationPath { get; set; } = "/opt/scaffold";
    public string DataPath { get; set; } = "/var/lib/scaffold";
    public string LogPath { get; set; } = "/var/log/scaffold";
    public string ServerDomain { get; set; } = "example.com";
    public int ApplicationPort { get; set; } = 5000;
    public string DotnetPath { get; set; } = "/usr/bin/dotnet";
    public string ServiceVersion { get; set; } = "1.0.0";
}
