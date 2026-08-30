using System;
using System.Net;

namespace DotnetServiceScaffold.Tests;

internal static class ExternalApiClientExtensionsTestsConstants
{
    // Base URLs
    public const string LocalhostBaseAddress = "http://localhost/";
    public const string TestApiEndpoint = "api/test";
    public const string TestApiEndpointWithId = "api/test/1";

    // Error messages
    public const string ErrorContent = "Error";
    public const string FailedAfterRetriesMessage = "*failed after {0} retries*";

    // HTTP Headers
    public const string AuthorizationHeader = "Authorization";
    public const string BearerTokenValue = "Bearer token";
    public const string CustomHeader = "X-Custom";
    public const string CustomHeaderValue = "value";

    // Response properties
    public const string StatusPropertyName = "Status";
    public const string CreatedStatus = "Created";
    public const string UpdatedStatus = "Updated";

    // Retry counts
    public const int DefaultRetryCount = 2;
    public const int SingleRetryCount = 1;
    public const int TripleRetryCount = 3;

    // Invalid values for testing
    public const int InvalidZero = 0;
    public const int InvalidNegativeOne = -1;
    public const int InvalidNegativeFive = -5;
    public const int InvalidNegativeTen = -10;

    // HTTP Status Codes
    public static readonly HttpStatusCode InternalServerError = HttpStatusCode.InternalServerError;
    public static readonly HttpStatusCode Ok = HttpStatusCode.OK;
    public static readonly HttpStatusCode Created = HttpStatusCode.Created;
}