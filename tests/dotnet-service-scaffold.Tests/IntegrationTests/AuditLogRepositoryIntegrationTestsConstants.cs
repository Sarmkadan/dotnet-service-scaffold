#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace DotnetServiceScaffold.Tests.IntegrationTests
{
    /// <summary>
    /// Constants for AuditLogRepositoryIntegrationTests.
    /// </summary>
    internal static class AuditLogRepositoryIntegrationTestsConstants
    {
        // Action values
        public const string LoginAction = "Login";
        public const string LogoutAction = "Logout";
        public const string UpdateProfileAction = "UpdateProfile";
        public const string DeleteDataAction = "DeleteData";
        public const string Action1 = "Action1";
        public const string Action2 = "Action2";

        // EntityType values
        public const string UserEntityType = "User";
        public const string DataEntityType = "Data";
        public const string Type1EntityType = "Type1";
        public const string Type2EntityType = "Type2";

        // Details values
        public const string UserLoggedInSuccessfullyDetails = "User logged in successfully";
        public const string UserLoggedOutDetails = "User logged out";
        public const string ProfileUpdatedDetails = "Profile updated";
        public const string ProfileUpdatedWithNewEmailDetails = "Profile updated with new email";
        public const string DataDeletedDetails = "Data deleted";
        public const string Details1 = "Details1";
        public const string Details2 = "Details2";
    }
}