namespace DotnetServiceScaffold.Tests
{
    public interface IStringUtilityTests
    {
        void Truncate_StringLongerThanMaxLength_TruncatesAndAppendsSuffix();
        void Truncate_NullOrEmptyInput_ReturnsEmptyString(string? input);
        void ToSnakeCase_CamelCaseOrPascalInput_InsertsUnderscoresBeforeUpperCaseLetters(string input, string expected);
        void MaskSensitive_LongApiKey_KeepsEdgeCharactersAndMasksMiddle();
        void IsValidEmail_VariousInputs_ReturnsExpectedValidationOutcome(string email, bool expected);
    }
}