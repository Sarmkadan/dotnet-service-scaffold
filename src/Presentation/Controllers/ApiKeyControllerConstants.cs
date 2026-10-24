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
    public const string ListApiKeys = "";
    public const string CreateApiKey = "";
    public const string RevokeApiKey = "{id}";
    public const string RotateApiKey = "{id}/rotate";

    // Audit messages
    public const string AuditKeyCreated = "User {0} created API key {1}";
    public const string AuditKeyRevoked = "User {0} revoked API key {1}";
    public const string AuditKeyRotated = "User {0} rotated API key {1} into {2}";

    // Log messages
    public const string LogGetAuthInfoRequested = "User {UserId} requested auth info";
    public const string LogGetAuthInfoError = "Error retrieving auth info for user {UserId}";
    public const string LogRotateApiKey = "User {UserId} rotated API key {ApiKeyId}";
    public const string LogRotateApiKeyError = "Error rotating API key {ApiKeyId} for user {UserId}";

    // Error messages
    public const string ErrorFailedToRetrieveAuthInfo = "Failed to retrieve authentication info";
    public const string ErrorFailedToRotateApiKey = "Failed to rotate API key";
}