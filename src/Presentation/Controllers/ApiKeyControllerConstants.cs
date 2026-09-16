using System;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// Constants for ApiKeyController to avoid hardcoded strings.
/// </summary>
public static class ApiKeyControllerConstants
{
    // Routes
    public const string RouteBase = "api/apikeys";
    public const string GetAuthInfo = "info";
    public const string RotateApiKey = "{id}/rotate";

    // Log messages
    public const string LogGetAuthInfoRequested = "User {UserId} requested auth info";
    public const string LogGetAuthInfoError = "Error retrieving auth info for user {UserId}";
    public const string LogRotateApiKey = "User {UserId} rotated API key {ApiKeyId}";
    public const string LogRotateApiKeyError = "Error rotating API key {ApiKeyId} for user {UserId}";

    // Error messages
    public const string ErrorFailedToRetrieveAuthInfo = "Failed to retrieve authentication info";
    public const string ErrorFailedToRotateApiKey = "Failed to rotate API key";
}