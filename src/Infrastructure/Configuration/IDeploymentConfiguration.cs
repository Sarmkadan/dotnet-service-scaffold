namespace DotnetServiceScaffold.Infrastructure.Configuration;

/// <summary>
/// Interface for deployment configuration.
/// </summary>
public interface IDeploymentConfiguration
{
    string ServiceName { get; set; }
    string ServiceDescription { get; set; }
    string ServiceUser { get; set; }
    string ApplicationPath { get; set; }
    string DataPath { get; set; }
    string LogPath { get; set; }
    string ServerDomain { get; set; }
    int ApplicationPort { get; set; }
    string DotnetPath { get; set; }
    string ServiceVersion { get; set; }
}