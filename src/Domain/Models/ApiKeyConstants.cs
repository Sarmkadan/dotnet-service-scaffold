#nullable enable
namespace DotnetServiceScaffold.Domain.Models
{
    /// <summary>
    /// Constants used by <see cref="ApiKey"/>.
    /// </summary>
    internal static class ApiKeyConstants
    {
        // String length limits (mirroring the original data‑annotation values)
        public const int NameMaxLength = 255;
        public const int KeyHashMaxLength = 500;
        public const int KeyPrefixMaxLength = 50;
        public const int AllowedIpsMaxLength = 1000;
        public const int AllowedScopesMaxLength = 500;
        public const int DescriptionMaxLength = 1000;

        // Miscellaneous numeric constants
        public const int Zero = 0;

        // Common string literals
        public const string WildcardScope = "*";
    }
}
