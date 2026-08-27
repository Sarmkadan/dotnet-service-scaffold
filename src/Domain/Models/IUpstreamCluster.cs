#nullable enable
namespace DotnetServiceScaffold.Domain.Models
{
    public interface IUpstreamCluster
    {
        string Name { get; set; }
        string Endpoint { get; set; }
        int HealthyHosts { get; set; }
        int TotalHosts { get; set; }
        bool CircuitBreakerOpen { get; set; }
        decimal GetHealthPercent();
    }
}