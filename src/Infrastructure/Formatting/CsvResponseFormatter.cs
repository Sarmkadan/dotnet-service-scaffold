#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Formatter for CSV responses. Handles serialization of collections to CSV format
/// with proper escaping and header generation from property names.
/// </summary>
public class CsvResponseFormatter : IResponseFormatter
{
    /// <inheritdoc />
    public string MediaType => "text/csv; charset=utf-8";

    /// <summary>
    /// Formats a collection of objects as CSV. Generates a header row from property names
    /// and creates a row for each object in the collection.
    /// </summary>
    /// <param name="data">The data to format as CSV. Must not be <c>null</c>.</param>
    /// <returns>A CSV-formatted string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the formatter fails to serialize the supplied data.</exception>
    public Task<string> FormatAsync(object? data)
    {
        ArgumentNullException.ThrowIfNull(data);

        try
        {
            var sb = new StringBuilder();

            // Handle IEnumerable collections (but not a plain string)
            if (data is IEnumerable enumerable && !(data is string))
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
    /// <param name="mediaType">The media type to check. Must not be <c>null</c>.</param>
    /// <returns>True if this formatter can handle the media type; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaType"/> is <c>null</c>.</exception>
    public bool CanFormat(string mediaType) =>
        (mediaType.StartsWith("text/csv", StringComparison.OrdinalIgnoreCase) ||
         mediaType.StartsWith("application/csv", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Escapes a field value for CSV format according to RFC 4180.
    /// </summary>
    /// <remarks>
    /// Properly escapes fields containing:
    /// - The delimiter (comma)
    /// - The quote character (double quote)
    /// - Carriage return or line feed characters
    /// Also neutralizes CSV formula injection by prefixing fields starting with =, +, -, or @
    /// when they could be interpreted as formulas in Excel.
    /// </remarks>
    /// <param name="field">The field value to escape.</param>
    /// <returns>The properly escaped field value.</returns>
    private string EscapeField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        // Neutralize CSV formula injection attacks
        // Excel interprets fields starting with =, +, -, @ as formulas
        // Prefix with a space to prevent formula execution while preserving data
        if (field.StartsWith('=') || field.StartsWith('+') || field.StartsWith('-') || field.StartsWith('@'))
        {
            field = " " + field;
        }

        // Determine if quoting is required per RFC 4180
        bool needsQuoting = field.Contains(',') ||
                            field.Contains('"') ||
                            field.Contains('\n') ||
                            field.Contains('\r') ||
                            field.Contains('\t') ||
                            field.Contains('\f') ||
                            field.Contains('\v');

        if (needsQuoting)
        {
            // Escape existing quotes by doubling them
            var escaped = field.Replace("\"", "\"\"");
            // Wrap in quotes
            return $"\"{escaped}\"";
        }

        return field;
    }
}
