# Response formatting and content negotiation

The types in `src/Infrastructure/Formatting/` separate response serialization from request-handling code. `IResponseFormatter` defines the formatting strategy, the three built-in implementations produce JSON, XML, or CSV, and `ResponseFormatterFactory` selects a strategy from a requested media type.

The factory performs formatter selection; it does not parse an HTTP `Accept` header or write an HTTP response. The caller is responsible for choosing one media type from the request, asking the factory for a formatter, calling `FormatAsync`, and setting the response `Content-Type` to the selected formatter's `MediaType`.

## Formatter contract

`IResponseFormatter` exposes three members:

- `MediaType` identifies the content type produced by the formatter.
- `CanFormat(string mediaType)` reports whether the formatter recognizes a requested media type.
- `FormatAsync(object? data)` serializes the supplied value to a string.

Despite the nullable contract, null handling differs among the built-in implementations: JSON and CSV throw `ArgumentNullException`, while XML returns an empty string. Callers that need a common null representation should handle null before invoking a formatter.

## Built-in formatters

| Formatter | Produced media type | Accepted media types | Formatting behavior |
| --- | --- | --- | --- |
| `JsonResponseFormatter` | `application/json` | `application/json` (including parameters) and types ending in `+json` | Uses camel-case property names, omits null properties, writes enums as camel-case strings, and converts `DateTime` values to UTC ISO 8601 text. |
| `XmlResponseFormatter` | `application/xml` | `application/xml`, `text/xml` (including parameters), and types ending in `+xml` | Uses `XmlSerializer` with empty namespaces. A null value produces an empty string. |
| `CsvResponseFormatter` | `text/csv; charset=utf-8` | `text/csv` and `application/csv` (including parameters) | Reflects public instance properties into a header and data rows. Collections use the first item's properties; an empty collection produces an empty string. Fields containing separators or control characters are quoted, embedded quotes are doubled, and formula-like prefixes are neutralized. |

CSV is best suited to a single object or a homogeneous collection of objects. Property values are rendered with `ToString()`, so culture-sensitive values should be normalized before formatting when a stable representation is required.

## Factory selection flow

`ResponseFormatterFactory` starts with registrations for `application/json`, `text/csv`, `application/csv`, `application/xml`, and `text/xml`. JSON is the default.

When `GetFormatter(mediaType)` is called, selection proceeds as follows:

1. A null, empty, or whitespace media type selects JSON.
2. An exact, case-insensitive registration match is used when present.
3. Otherwise, the factory asks registered formatters whether `CanFormat(mediaType)` is true. This supports values such as `application/json; charset=utf-8` and vendor suffixes such as `application/vnd.example+json`.
4. If nothing matches, the factory falls back to JSON.

Because an unsupported request falls back to JSON, use `IsMediaTypeSupported` before `GetFormatter` when the application should return HTTP 406 instead. A null or whitespace value is considered supported because it selects the default. `GetSupportedMediaTypes` returns the registered keys, and `RegisterFormatter` can add or replace a registration. Complete registrations during application startup; the backing dictionary is not designed for concurrent writes.

The factory accepts one media type at a time. An HTTP header containing multiple values, quality weights, or wildcards must be parsed and prioritized by the HTTP layer before it calls the factory.

## Example

The following minimal API endpoint chooses the most highly weighted `Accept` value, rejects unsupported explicit media types, formats the response, and uses the formatter's produced content type:

```csharp
using DotnetServiceScaffold.Infrastructure.Formatting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IResponseFormatterFactory, ResponseFormatterFactory>();

var app = builder.Build();

app.MapGet("/reports", async (
    HttpContext context,
    IResponseFormatterFactory formatterFactory) =>
{
    var requestedMediaType = context.Request.GetTypedHeaders().Accept?
        .OrderByDescending(value => value.Quality ?? 1)
        .Select(value => value.MediaType.Value)
        .FirstOrDefault();

    if (requestedMediaType is not null &&
        !formatterFactory.IsMediaTypeSupported(requestedMediaType))
    {
        return Results.StatusCode(StatusCodes.Status406NotAcceptable);
    }

    var report = new[]
    {
        new { Id = 1, Name = "North", Status = "Ready" },
        new { Id = 2, Name = "South", Status = "Pending" }
    };

    var formatter = formatterFactory.GetFormatter(requestedMediaType);
    var body = await formatter.FormatAsync(report);

    return Results.Content(body, formatter.MediaType);
});

app.Run();
```

For no `Accept` value, this endpoint returns JSON. Requests for `application/xml` or `text/csv` select those formatters. An explicit unsupported type, such as `text/plain`, receives HTTP 406 rather than the factory's normal JSON fallback.
