using System;

internal static class ProblemDetailsFactoryTestsConstants
{
    public const string BadRequestUrl = "https://example.com/errors/bad-request";
    public const string BadRequestTitle = "Bad Request";
    public const string InvalidInputDataDetail = "Invalid input data";
    public const string ValidationFailedUrl = "https://example.com/errors/validation-failed";
    public const string UnprocessableEntityTitle = "Unprocessable Entity";
    public const string ValidationFailedDetail = "Validation failed";
    public const string DefaultProblemType = "about:blank";
    public const string TestErrorCode = "TEST_ERROR_CODE";
    public const string ValidationErrorCode = "VALIDATION_ERROR";
    public const string TraceIdKey = "traceId";
    public const string ErrorCodeKey = "errorCode";
    public const string ProblemDetailsContentType = "application/problem+json";
    public const string CustomFieldKey = "customField";
    public const string CustomFieldValue = "customValue";
    public const string NumericFieldKey = "numericField";
    public const int NumericFieldValue = 42;
    public const string NullFieldKey = "nullField";
    public const string TestTraceId = "test-trace-id-123";
    public const int StatusCodeBadRequest = 400;
    public const int StatusCodeNotFound = 404;
    public const int StatusCodeUnprocessableEntity = 422;
    public const int StatusCodeInternalServerError = 500;
}