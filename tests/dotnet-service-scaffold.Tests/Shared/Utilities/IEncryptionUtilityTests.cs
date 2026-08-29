namespace DotnetServiceScaffold.Tests.Shared.Utilities;

/// <summary>
/// Interface for EncryptionUtilityTests.
/// </summary>
public interface IEncryptionUtilityTests
{
    void HashPassword_VerifyPassword_RoundTrip_Success();
    void HashPassword_DifferentInputs_DifferentOutputs();
    void VerifyPassword_WrongPassword_ReturnsFalse();
    void VerifyPassword_InvalidHash_ReturnsFalse();
    void VerifyPassword_NullOrEmptyInputs_ReturnsFalse();
    void EncryptAes_DecryptAes_RoundTrip_Success();
    void EncryptAes_DifferentInputs_DifferentOutputs();
    void EncryptAes_NullOrEmptyPlaintext_ThrowsArgumentException();
    void EncryptAes_InvalidKeyLength_ThrowsArgumentException();
    void DecryptAes_NullOrEmptyCiphertext_ThrowsArgumentException();
    void DecryptAes_InvalidKeyLength_ThrowsArgumentException();
    void DecryptAes_InvalidCiphertext_ThrowsInvalidOperationException();
    void GenerateRandomBytes_CorrectLengthAndDifferentValues();
    void GenerateRandomBytes_NonPositiveLength_ThrowsArgumentException();
    void GenerateSecureToken_ProducesUrlSafeBase64();
    void GenerateSecureToken_DefaultLength();
    void ComputeHmacSha256_SameInputs_SameOutput();
    void ComputeHmacSha256_DifferentInputs_DifferentOutputs();
    void ComputeHmacSha256_NullOrEmptyInputs_ThrowsArgumentException();
    void ComputeSha256_SameInputs_SameOutput();
}