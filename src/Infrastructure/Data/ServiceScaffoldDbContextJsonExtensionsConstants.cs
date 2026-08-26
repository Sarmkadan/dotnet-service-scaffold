using System.Text.Json;

namespace DotnetServiceScaffold.Infrastructure.Data;

internal static class ServiceScaffoldDbContextJsonExtensionsConstants
{
    public static readonly JsonNamingPolicy PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    public static readonly bool WriteIndented = false;
    public static readonly bool PropertyNameCaseInsensitive = true;
}