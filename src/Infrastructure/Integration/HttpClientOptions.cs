#nullable enable

using System.ComponentModel.DataAnnotations;

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Options for configuring HttpClient instances.
/// </summary>
public class HttpClientOptions
{
    /// <summary>
    /// The default timeout in seconds for HTTP requests.
    /// </summary>
    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// The User-Agent header value for HTTP requests.
    /// </summary>
    [Required]
    public string UserAgent { get; set; } = "DotnetServiceScaffold/1.0";
}
