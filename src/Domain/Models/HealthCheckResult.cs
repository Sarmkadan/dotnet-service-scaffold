#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotnetServiceScaffold.Domain.Enums;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Records the results of a health check performed on a service.
/// </summary>
public sealed class HealthCheckResult : IHealthCheckResult, IEquatable<HealthCheckResult>
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Service))]
    public Guid ServiceId { get; set; }

    public ServiceRegistration? Service { get; set; }

    public HealthStatus Status { get; set; }

    public int? HttpStatusCode { get; set; }

    public long ResponseTimeMs { get; set; }

    [StringLength(1000)]
    public string? ErrorMessage { get; set; }

    [StringLength(4000)]
    public string? ResponseBody { get; set; }

    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    public string? CheckMethod { get; set; }

    [StringLength(255)]
    public string? CheckEndpoint { get; set; }

    public decimal? CpuUsagePercent { get; set; }

    public decimal? MemoryUsagePercent { get; set; }

    public long? DiskUsageBytes { get; set; }

    /// <summary>
    /// Determines if this health check result indicates a healthy service.
    /// </summary>
    public bool IsHealthy()
    {
        return Status == HealthStatus.Healthy &&
               HttpStatusCode >= 200 &&
               HttpStatusCode < 300;
    }

    /// <summary>
    /// Determines if the response time is within acceptable parameters.
    /// </summary>
    public bool IsResponseTimeAcceptable(long thresholdMs = 5000)
    {
        return ResponseTimeMs <= thresholdMs;
    }

    /// <summary>
    /// Checks if system resources are within acceptable ranges.
    /// </summary>
    public bool AreResourcesHealthy(decimal cpuThreshold = 90, decimal memoryThreshold = 85)
    {
        if (CpuUsagePercent.HasValue && CpuUsagePercent > cpuThreshold)
            return false;

        if (MemoryUsagePercent.HasValue && MemoryUsagePercent > memoryThreshold)
            return false;

        return true;
    }

    /// <summary>
    /// Gets a human-readable summary of the health check result.
    /// </summary>
    public string GetSummary()
    {
        var parts = new List<string> { $"Status: {Status}" };

        if (HttpStatusCode.HasValue)
            parts.Add($"HTTP {HttpStatusCode}");

        parts.Add($"Response Time: {ResponseTimeMs}ms");

        if (CpuUsagePercent.HasValue)
            parts.Add($"CPU: {CpuUsagePercent:F1}%");

        if (MemoryUsagePercent.HasValue)
            parts.Add($"Memory: {MemoryUsagePercent:F1}%");

        if (!string.IsNullOrEmpty(ErrorMessage))
            parts.Add($"Error: {ErrorMessage}");

        return string.Join(" | ", parts);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true if the current object is equal to the <paramref name="other">parameter</paramref>; otherwise, false.</returns>
    public bool Equals(HealthCheckResult? other)
    {
        if (other is null)
            return false;

        return Id == other.Id &&
               ServiceId == other.ServiceId &&
               EqualityComparer<ServiceRegistration?>.Default.Equals(Service, other.Service) &&
               Status == other.Status &&
               HttpStatusCode == other.HttpStatusCode &&
               ResponseTimeMs == other.ResponseTimeMs &&
               ErrorMessage == other.ErrorMessage &&
               ResponseBody == other.ResponseBody;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as HealthCheckResult);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, ServiceId, Service, Status, HttpStatusCode, ResponseTimeMs, ErrorMessage, ResponseBody);
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>true if the objects are equal; otherwise, false.</returns>
    public static bool operator ==(HealthCheckResult? left, HealthCheckResult? right)
    {
        return EqualityComparer<HealthCheckResult>.Default.Equals(left, right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>true if the objects are not equal; otherwise, false.</returns>
    public static bool operator !=(HealthCheckResult? left, HealthCheckResult? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return $"HealthCheckResult {{ Id = {Id}, ServiceId = {ServiceId}, Service = {Service}, Status = {Status}, HttpStatusCode = {HttpStatusCode}, ResponseTimeMs = {ResponseTimeMs} }}";
    }
}
