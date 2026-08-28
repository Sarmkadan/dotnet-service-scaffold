#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace DotnetServiceScaffold.Tests.Infrastructure.Configuration;

/// <summary>
/// Constants for DeploymentConfigurationTests.
/// </summary>
internal static class DeploymentConfigurationTestsConstants
{
    // Systemd service unit constants
    public const string Systemd_Description = "Description=";
    public const string Systemd_User = "User=";
    public const string Systemd_WorkingDirectory = "WorkingDirectory=";
    public const string Systemd_ExecStart = "ExecStart=";
    public const string Systemd_SyslogIdentifier = "SyslogIdentifier=";
    public const string Systemd_ReadWritePaths = "ReadWritePaths=";

    // Caddy configuration constants
    public const string Caddy_Comment = "# Caddy reverse proxy configuration for ";
    public const string Caddy_OpenBrace = " {";
    public const string Caddy_ReverseProxy = "reverse_proxy localhost:";
    public const string Caddy_OutputFile = "output file ";
    public const string Caddy_LogOpenBrace = "/caddy.log {";

    // Environment file constants
    public const string EnvFile_Comment = "# Environment variables for ";
    public const string EnvFile_Urls = "ASPNETCORE_URLS=http://localhost:";
    public const string EnvFile_ConnectionString = "ConnectionStrings__DefaultConnection=Data Source=";
    public const string EnvFile_DbPath = "/scaffold.db";
    public const string EnvFile_ServiceName = "SERVICE_NAME=";
    public const string EnvFile_ServiceVersion = "SERVICE_VERSION=";

    // Deployment guide constants
    public const string DeploymentGuide_Comment = "# Deployment Guide for ";
    public const string DeploymentGuide_AddUser = "sudo useradd -r -s /bin/false ";
    public const string DeploymentGuide_Mkdir = "sudo mkdir -p ";
    public const string DeploymentGuide_Curl = "curl https://";
    public const string DeploymentGuide_HealthCheck = "/health";
    public const string DeploymentGuide_DbPathInfo = "- Database files are stored in ";

    // Systemd security settings constants
    public const string SystemdSecurity_NoNewPrivileges = "NoNewPrivileges=true";
    public const string SystemdSecurity_PrivateTmp = "PrivateTmp=true";
    public const string SystemdSecurity_ProtectSystem = "ProtectSystem=strict";
    public const string SystemdSecurity_ProtectHome = "ProtectHome=true";

    // Caddy health check constants
    public const string CaddyHealthCheck_Uri = "uri /health";
    public const int HealthCheckIntervalSeconds = 30;
    public const int HealthCheckTimeoutSeconds = 5;
    public static readonly int[] HealthCheckUnhealthyStatusCodes = { 500, 502, 503 };

    // Environment file production constant
    public const string EnvFile_Production = "ASPNETCORE_ENVIRONMENT=Production";

    // ExecStart DLL constant
    public const string ExecStart_Dll = "DotnetServiceScaffold.dll";
}