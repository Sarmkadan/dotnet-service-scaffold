#nullable enable
namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// Constants for UserController to avoid magic strings and numbers.
/// </summary>
internal static class UserControllerConstants
{
    // HTTP status codes
    public const int InternalServerErrorStatusCode = 500;

    // Logging messages
    public const string RegistrationValidationError = "Registration validation error: {Errors}";
    public const string RegistrationError = "Registration error";
    public const string FailedAuthenticationAttempt = "Failed authentication attempt for {Email}";
    public const string LoginError = "Login error for {Email}";
    public const string UserNotFoundLog = "User not found: {UserId}";
    public const string ErrorRetrievingUserLog = "Error retrieving user {UserId}";
    public const string PasswordChangeFailedForUserLog = "Password change failed for user {UserId}";
    public const string PasswordChangeErrorLog = "Password change error for user {UserId}";
    public const string ErrorUnlockingUserLog = "Error unlocking user {UserId}";
    public const string SearchQueryParameterQRequired = "Search query parameter 'q' is required";
    public const string ErrorSearchingUsersWithQueryLog = "Error searching users with query: {Query}";
    public const string ErrorSearchingUsersLog = "Error searching users";

    // Response messages
    public const string InvalidEmailOrPassword = "Invalid email or password";
    public const string UserNotFoundResponse = "User not found";
    public const string CurrentPasswordIsIncorrect = "Current password is incorrect";
    public const string PasswordChangeFailedResponse = "Password change failed";
    public const string ErrorUnlockingUserResponse = "Error unlocking user";
    public const string SearchQueryParameterQRequiredResponse = "Search query parameter 'q' is required";
    public const string ValidationFailed = "Validation failed";
    public const string RegistrationFailed = "Registration failed";
    public const string LoginFailed = "Login failed";
    public const string ErrorRetrievingUser = "Error retrieving user";
    public const string PasswordChangeFailed = "Password change failed";
    public const string ErrorUnlockingUser = "Error unlocking user";
    public const string ErrorSearchingUsers = "Error searching users";
    public const string PasswordChangedSuccessfully = "Password changed successfully";
    public const string UserAccountUnlocked = "User account unlocked";

    // Magic numbers for pagination
    public const int MinimumPageNumber = 1;
    public const int DefaultPageSize = 10;
    public const int MinimumPageSize = 10;
    public const int MaximumPageSize = 100;
}