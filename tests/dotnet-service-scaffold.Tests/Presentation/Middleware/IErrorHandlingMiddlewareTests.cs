using System;
using System.Threading.Tasks;

/// <summary>
/// Interface for ErrorHandlingMiddlewareTests.
/// </summary>
public interface IErrorHandlingMiddlewareTests
{
    Task InvokeAsync_ShouldCatchGenericExceptionAndReturn500();
    Task InvokeAsync_ShouldCatchServiceScaffoldExceptionAndReturn400();
    Task InvokeAsync_ShouldCatchArgumentNullExceptionAndReturn400();
    Task InvokeAsync_ShouldCatchArgumentExceptionAndReturn400();
    Task InvokeAsync_ShouldCatchInvalidOperationExceptionAndReturn409();
    Task InvokeAsync_ShouldCatchKeyNotFoundExceptionAndReturn404();
    Task InvokeAsync_ShouldReturnGenericMessageInProduction();
}