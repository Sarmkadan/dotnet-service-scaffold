#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text;
using System.Text.Json;

namespace DotnetServiceScaffold.Infrastructure.Metrics;

/// <summary>
/// Converts the internal metrics dictionary produced by <see cref="IMetricsService"/>
/// into the Prometheus text exposition format (version 0.0.4).
///
/// Counter and gauge metrics are emitted as single time-series entries.
/// Timer metrics are expanded into four series: _sum, _count, _min_ms, and _max_ms.
/// </summary>
public sealed class PrometheusFormatter : IPrometheusFormatter
{
    /// <inheritdoc/>
    public string Format(Dictionary<string, object> metrics, string applicationName = "app")
    {
        ArgumentNullException.ThrowIfNull(metrics);

        var sb = new StringBuilder();
        var prefix = SanitizeName(applicationName);

        foreach (var (rawKey, value) in metrics)
        {
            var (baseName, labels) = ParseKey(rawKey);
            var metricName = $"{prefix}_{SanitizeName(baseName)}";
            var labelStr = labels.Count > 0
                ? "{" + string.Join(",", labels.Select(label => $"{SanitizeName(label.Key)}=\"{EscapeLabel(label.Value)}\"")) + "}"
                : string.Empty;

            var props = ToDictionary(value);
            if (props is null)
            {
                continue;
            }

            var type = props.GetValueOrDefault("type")?.ToString() ?? "gauge";

            switch (type)
            {
                case "counter":
                    sb.AppendLine($"# HELP {metricName} Counter metric.");
                    sb.AppendLine($"# TYPE {metricName} counter");
                    sb.AppendLine($"{metricName}_total{labelStr} {FormatValue(props.GetValueOrDefault("value"))}");
                    break;

                case "gauge":
                    sb.AppendLine($"# HELP {metricName} Gauge metric.");
                    sb.AppendLine($"# TYPE {metricName} gauge");
                    sb.AppendLine($"{metricName}{labelStr} {FormatValue(props.GetValueOrDefault("value"))}");
                    break;

                case "timer":
                    sb.AppendLine($"# HELP {metricName} Timer metric (milliseconds).");
                    sb.AppendLine($"# TYPE {metricName} summary");
                    sb.AppendLine($"{metricName}_sum{labelStr} {FormatValue(props.GetValueOrDefault("totalMs"))}");
                    sb.AppendLine($"{metricName}_count{labelStr} {FormatValue(props.GetValueOrDefault("count"))}");
                    sb.AppendLine($"{metricName}_min_ms{labelStr} {FormatValue(props.GetValueOrDefault("minMs"))}");
                    sb.AppendLine($"{metricName}_max_ms{labelStr} {FormatValue(props.GetValueOrDefault("maxMs"))}");
                    break;

                default:
                    sb.AppendLine($"# TYPE {metricName} untyped");
                    sb.AppendLine($"{metricName}{labelStr} 0");
                    break;
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string SanitizeName(string name)
    {
        var chars = name.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            var current = chars[i];
            if (!char.IsLetterOrDigit(current) && current != '_' && current != ':')
            {
                chars[i] = '_';
            }
        }

        var sanitized = new string(chars).TrimStart('_', ':');
        return string.IsNullOrWhiteSpace(sanitized) ? "metric" : sanitized;
    }

    private static string EscapeLabel(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);

    private static string FormatValue(object? value) =>
        value switch
        {
            null => "0",
            JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.Number =>
                jsonElement.GetDouble().ToString("G", CultureInfo.InvariantCulture),
            JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.String &&
                                          double.TryParse(jsonElement.GetString(), CultureInfo.InvariantCulture, out var parsed) =>
                parsed.ToString("G", CultureInfo.InvariantCulture),
            _ => Convert.ToDouble(value, CultureInfo.InvariantCulture).ToString("G", CultureInfo.InvariantCulture)
        };

    private static (string BaseName, Dictionary<string, string> Labels) ParseKey(string key)
    {
        var bracketIndex = key.IndexOf('[');
        if (bracketIndex < 0)
        {
            return (key, new Dictionary<string, string>());
        }

        var baseName = key[..bracketIndex];
        var tagSection = key[(bracketIndex + 1)..].TrimEnd(']');
        var labels = new Dictionary<string, string>();

        foreach (var part in tagSection.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var equalsIndex = part.IndexOf('=');
            if (equalsIndex > 0)
            {
                labels[part[..equalsIndex].Trim()] = part[(equalsIndex + 1)..].Trim();
            }
        }

        return (baseName, labels);
    }

    private static Dictionary<string, object?>? ToDictionary(object value)
    {
        if (value is Dictionary<string, object?> dict)
        {
            return dict;
        }

        if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
        {
            return jsonElement.EnumerateObject()
                .ToDictionary(property => property.Name, property => (object?)property.Value);
        }

        var result = new Dictionary<string, object?>();
        foreach (var property in value.GetType().GetProperties())
        {
            result[property.Name] = property.GetValue(value);
        }

        return result.Count > 0 ? result : null;
    }
}
