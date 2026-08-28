namespace DotnetServiceScaffold.Tests.Logging
{
    public interface ILogContextServiceJsonExtensionsTests
    {
        void ToJson_WithValidService_ReturnsCorrectJson();
        void ToJson_WithIndentation_ProducesIndentedJson();
        void ToJson_NullService_ThrowsArgumentNullException();
        void FromJson_ValidJson_ReturnsServiceWithProperties();
        void FromJson_NullJson_ThrowsArgumentNullException();
        void FromJson_EmptyJson_ReturnsNull();
        void FromJson_InvalidJson_ThrowsJsonException();
        void TryFromJson_ValidJson_ReturnsTrueAndService();
        void TryFromJson_InvalidJson_ReturnsFalse();
        void TryFromJson_NullJson_ThrowsArgumentNullException();
        void TryFromJson_EmptyJson_ReturnsFalse();
    }
}