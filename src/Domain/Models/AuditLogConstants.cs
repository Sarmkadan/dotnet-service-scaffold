namespace DotnetServiceScaffold.Domain.Models
{
    internal static class AuditLogConstants
    {
        // Default values
        public const string DefaultActor = "System";
        public const string SuccessStatus = "Success";

        // Action names
        public const string ActionCreate = "Create";
        public const string ActionUpdate = "Update";
        public const string ActionDelete = "Delete";
        public const string ActionRestore = "Restore";
        public const string ActionLogin = "Login";
        public const string ActionLogout = "Logout";
        public const string ActionExport = "Export";
        public const string ActionImport = "Import";

        // Human‑readable descriptions
        public const string DescriptionCreated = "Created";
        public const string DescriptionUpdated = "Updated";
        public const string DescriptionDeleted = "Deleted";
        public const string DescriptionRestored = "Restored";
        public const string DescriptionLoggedIn = "Logged in";
        public const string DescriptionLoggedOut = "Logged out";
        public const string DescriptionExported = "Exported";
        public const string DescriptionImported = "Imported";

        // Summary format string
        public const string SummaryFormat = "{0} performed {1} on {2} ({3}) at {4:O}";
    }
}
