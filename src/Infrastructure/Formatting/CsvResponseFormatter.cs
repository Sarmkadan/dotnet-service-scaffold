// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Text;

namespace DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Formatter for CSV responses. Handles serialization of collections to CSV format
/// with proper escaping and header generation from property names.
/// </summary>
public class CsvResponseFormatter : IResponseFormatter
{
    public string MediaType => "text/csv";

    /// <summary>
    /// Formats a collection of objects as CSV. Generates a header row from property names
    /// and creates a row for each object in the collection.
    /// </summary>
    public Task<string> FormatAsync(object? data)
    {
        if (data == null)
            return Task.FromResult(string.Empty);

        try
        {
            var sb = new StringBuilder();

            // Handle IEnumerable collections
            if (data is System.Collections.IEnumerable enumerable && !(data is string))
            {
                var items = enumerable.Cast<object>().ToList();

                if (items.Count == 0)
                    return Task.FromResult(string.Empty);

                var firstItem = items.First();
                var properties = firstItem.GetType().GetProperties(
                    BindingFlags.Public | BindingFlags.Instance);

                // Write header row
                var headers = properties.Select(p => EscapeField(p.Name));
                sb.AppendLine(string.Join(",", headers));

                // Write data rows
                foreach (var item in items)
                {
                    var values = properties.Select(p =>
                    {
                        var value = p.GetValue(item);
                        return EscapeField(value?.ToString() ?? string.Empty);
                    });

                    sb.AppendLine(string.Join(",", values));
                }
            }
            else
            {
                // Single object
                var properties = data.GetType().GetProperties(
                    BindingFlags.Public | BindingFlags.Instance);

                var headers = properties.Select(p => EscapeField(p.Name));
                sb.AppendLine(string.Join(",", headers));

                var values = properties.Select(p =>
                {
                    var value = p.GetValue(data);
                    return EscapeField(value?.ToString() ?? string.Empty);
                });

                sb.AppendLine(string.Join(",", values));
            }

            return Task.FromResult(sb.ToString());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to format object as CSV", ex);
        }
    }

    /// <summary>
    /// Determines if this formatter can handle the given media type.
    /// </summary>
    public bool CanFormat(string mediaType)
    {
        return !string.IsNullOrEmpty(mediaType) &&
               (mediaType.StartsWith("text/csv", StringComparison.OrdinalIgnoreCase) ||
                mediaType.StartsWith("application/csv", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Escapes a field value for CSV format. Wraps fields containing special characters
    /// in quotes and escapes existing quotes.
    /// </summary>
    private string EscapeField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        // If field contains comma, quotes, or newlines, wrap in quotes and escape existing quotes
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }

        return field;
    }
}
