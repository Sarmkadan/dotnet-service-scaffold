namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

public interface IServiceDiscoveryOptions
{
    bool Enabled { get; set; }
    DiscoveryMode Mode { get; set; }
    LoadBalancingStrategy LoadBalancing { get; set; }
    TimeSpan CacheTtl { get; set; }
    TimeSpan RefreshInterval { get; set; }
    TimeSpan ResolutionTimeout { get; set; }
    DnsDiscoveryOptions Dns { get; set; }
    RegistryDiscoveryOptions Registry { get; set; }
    SelfRegistrationOptions SelfRegistration { get; set; }

    string SearchDomain { get; set; }
    bool PreferSrvRecords { get; set; }
    string DnsServerAddress { get; set; }
    int DnsServerPort { get; set; }
    int DefaultPort { get; set; }
    string DefaultScheme { get; set; }
    int MaxRetries { get; set; }
    TimeSpan SocketTimeout { get; set; }
    string AgentEndpoint { get; set; }
    string? AclToken { get; set; }
    bool OnlyHealthyInstances { get; set; }
}
