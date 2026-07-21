using System.Security.Cryptography;
using DotnetServiceScaffold.Shared.Utilities;
using FluentAssertions;
using Xunit;

namespace DotnetServiceScaffold.Tests.Shared.Utilities;

/// <summary>
/// Tests for the EncryptionUtility class.
/// </summary>
public class EncryptionUtilityTests
{
    /// <summary>
    /// Tests that HashPassword and VerifyPassword work correctly for round-trip operations.
    /// </summary>
    [Fact]
    public void HashPassword_VerifyPassword_RoundTrip_Success()
    {
        // Arrange
        const string password = "MySecurePassword123!";

        // Act
        string hash = EncryptionUtility.HashPassword(password);
        bool isValid = EncryptionUtility.VerifyPassword(password, hash);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        isValid.Should().BeTrue();
    }

    /// <summary>
    /// Tests that different passwords produce different hashes.
    /// </summary>
    [Fact]
    public void HashPassword_DifferentInputs_DifferentOutputs()
    {
        // Arrange
        const string password1 = "PasswordOne123!";
        const string password2 = "PasswordTwo456@";

        // Act
        string hash1 = EncryptionUtility.HashPassword(password1);
        string hash2 = EncryptionUtility.HashPassword(password2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    /// <summary>
    /// Tests that VerifyPassword correctly rejects wrong passwords.
    /// </summary>
    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        // Arrange
        const string password = "CorrectPassword123!";
        const string wrongPassword = "WrongPassword456@";

        // Act
        string hash = EncryptionUtility.HashPassword(password);
        bool isValid = EncryptionUtility.VerifyPassword(wrongPassword, hash);

        // Assert
        isValid.Should().BeFalse();
    }

    /// <summary>
    /// Tests that VerifyPassword handles invalid hash formats gracefully.
    /// </summary>
    [Fact]
    public void VerifyPassword_InvalidHash_ReturnsFalse()
    {
        // Arrange
        const string password = "AnyPassword123!";
        const string invalidHash = "not-a-valid-base64-hash!!!";

        // Act
        bool isValid = EncryptionUtility.VerifyPassword(password, invalidHash);

        // Assert
        isValid.Should().BeFalse();
    }

    /// <summary>
    /// Tests that VerifyPassword handles null or empty inputs gracefully.
    /// </summary>
    [Fact]
    public void VerifyPassword_NullOrEmptyInputs_ReturnsFalse()
    {
        // Arrange
        const string password = "ValidPassword123!";
        string hash = EncryptionUtility.HashPassword(password);

        // Act & Assert
        EncryptionUtility.VerifyPassword(null, hash).Should().BeFalse();
        EncryptionUtility.VerifyPassword(string.Empty, hash).Should().BeFalse();
        EncryptionUtility.VerifyPassword(password, null).Should().BeFalse();
        EncryptionUtility.VerifyPassword(password, string.Empty).Should().BeFalse();
        EncryptionUtility.VerifyPassword(null, null).Should().BeFalse();
        EncryptionUtility.VerifyPassword(string.Empty, string.Empty).Should().BeFalse();
    }

    /// <summary>
    /// Tests that EncryptAes and DecryptAes work correctly for round-trip operations.
    /// </summary>
    [Fact]
    public void EncryptAes_DecryptAes_RoundTrip_Success()
    {
        // Arrange
        const string plaintext = "This is a secret message!";
        byte[] key = EncryptionUtility.GenerateRandomBytes(32); // AES-256 key

        // Act
        string ciphertext = EncryptionUtility.EncryptAes(plaintext, key);
        string decrypted = EncryptionUtility.DecryptAes(ciphertext, key);

        // Assert
        ciphertext.Should().NotBeNullOrEmpty();
        decrypted.Should().Be(plaintext);
    }

    /// <summary>
    /// Tests that different plaintexts produce different ciphertexts with the same key.
    /// </summary>
    [Fact]
    public void EncryptAes_DifferentInputs_DifferentOutputs()
    {
        // Arrange
        const string plaintext1 = "First message";
        const string plaintext2 = "Second message";
        byte[] key = EncryptionUtility.GenerateRandomBytes(32);

        // Act
        string ciphertext1 = EncryptionUtility.EncryptAes(plaintext1, key);
        string ciphertext2 = EncryptionUtility.EncryptAes(plaintext2, key);

        // Assert
        ciphertext1.Should().NotBe(ciphertext2);
    }

    /// <summary>
    /// Tests that EncryptAes throws for null or empty plaintext.
    /// </summary>
    [Fact]
    public void EncryptAes_NullOrEmptyPlaintext_ThrowsArgumentException()
    {
        // Arrange
        byte[] key = EncryptionUtility.GenerateRandomBytes(32);

        // Act & Assert
        Action act1 = () => EncryptionUtility.EncryptAes(null, key);
        act1.Should().Throw<ArgumentException>()
            .WithMessage("*Plaintext cannot be null or empty*");

        Action act2 = () => EncryptionUtility.EncryptAes(string.Empty, key);
        act2.Should().Throw<ArgumentException>()
            .WithMessage("*Plaintext cannot be null or empty*");
    }

    /// <summary>
    /// Tests that EncryptAes throws for invalid key length.
    /// </summary>
    [Fact]
    public void EncryptAes_InvalidKeyLength_ThrowsArgumentException()
    {
        // Arrange
        const string plaintext = "Test message";
        byte[] tooShortKey = EncryptionUtility.GenerateRandomBytes(16); // 128-bit instead of 256-bit
        byte[] tooLongKey = EncryptionUtility.GenerateRandomBytes(64);  // 512-bit instead of 256-bit

        // Act & Assert
        Action act3 = () => EncryptionUtility.EncryptAes(plaintext, tooShortKey);
        act3.Should().Throw<ArgumentException>()
            .WithMessage("*Key must be 32 bytes for AES-256*");

        Action act4 = () => EncryptionUtility.EncryptAes(plaintext, tooLongKey);
        act4.Should().Throw<ArgumentException>()
            .WithMessage("*Key must be 32 bytes for AES-256*");
    }

    /// <summary>
    /// Tests that DecryptAes throws for null or empty ciphertext.
    /// </summary>
    [Fact]
    public void DecryptAes_NullOrEmptyCiphertext_ThrowsArgumentException()
    {
        // Arrange
        byte[] key = EncryptionUtility.GenerateRandomBytes(32);

        // Act & Assert
        Action act5 = () => EncryptionUtility.DecryptAes(null, key);
        act5.Should().Throw<ArgumentException>()
            .WithMessage("*Ciphertext cannot be null or empty*");

        Action act6 = () => EncryptionUtility.DecryptAes(string.Empty, key);
        act6.Should().Throw<ArgumentException>()
            .WithMessage("*Ciphertext cannot be null or empty*");
    }

    /// <summary>
    /// Tests that DecryptAes throws for invalid key length.
    /// </summary>
    [Fact]
    public void DecryptAes_InvalidKeyLength_ThrowsArgumentException()
    {
        // Arrange
        const string plaintext = "Test message";
        byte[] key = EncryptionUtility.GenerateRandomBytes(32);
        string ciphertext = EncryptionUtility.EncryptAes(plaintext, key);
        byte[] invalidKey = EncryptionUtility.GenerateRandomBytes(16); // Wrong size

        // Act & Assert
        Action act7 = () => EncryptionUtility.DecryptAes(ciphertext, invalidKey);
        act7.Should().Throw<ArgumentException>()
            .WithMessage("*Key must be 32 bytes for AES-256*");
    }

    /// <summary>
    /// Tests that DecryptAes throws for invalid ciphertext.
    /// </summary>
    [Fact]
    public void DecryptAes_InvalidCiphertext_ThrowsInvalidOperationException()
    {
        // Arrange
        byte[] key = EncryptionUtility.GenerateRandomBytes(32);
        const string invalidCiphertext = "invalid-base64!!!";

        // Act & Assert
        Action act8 = () => EncryptionUtility.DecryptAes(invalidCiphertext, key);
        act8.Should().Throw<InvalidOperationException>()
            .WithMessage("*Failed to decrypt data*");
    }

    /// <summary>
    /// Tests that GenerateRandomBytes produces the correct length and different values.
    /// </summary>
    [Fact]
    public void GenerateRandomBytes_CorrectLengthAndDifferentValues()
    {
        // Act
        byte[] bytes1 = EncryptionUtility.GenerateRandomBytes(16);
        byte[] bytes2 = EncryptionUtility.GenerateRandomBytes(16);
        byte[] bytes3 = EncryptionUtility.GenerateRandomBytes(32);

        // Assert
        bytes1.Length.Should().Be(16);
        bytes2.Length.Should().Be(16);
        bytes3.Length.Should().Be(32);
        bytes1.Should().NotEqual(bytes2); // Very unlikely to be equal
    }

    /// <summary>
    /// Tests that GenerateRandomBytes throws for non-positive length.
    /// </summary>
    [Fact]
    public void GenerateRandomBytes_NonPositiveLength_ThrowsArgumentException()
    {
        // Act & Assert
        Action act1 = () => EncryptionUtility.GenerateRandomBytes(0);
        act1.Should().Throw<ArgumentException>()
            .WithMessage("*Length must be positive*");

        Action act2 = () => EncryptionUtility.GenerateRandomBytes(-5);
        act2.Should().Throw<ArgumentException>()
            .WithMessage("*Length must be positive*");
    }

    /// <summary>
    /// Tests that GenerateSecureToken produces URL-safe Base64 strings.
    /// </summary>
    [Fact]
    public void GenerateSecureToken_ProducesUrlSafeBase64()
    {
        // Act
        string token = EncryptionUtility.GenerateSecureToken(32);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().NotContain("+"); // URL-safe Base64 replaces + with -
        token.Should().NotContain("/"); // URL-safe Base64 replaces / with _
        token.Should().NotContain("="); // URL-safe Base64 removes padding
    }

    /// <summary>
    /// Tests that GenerateSecureToken with default length produces 32 bytes worth of data.
    /// </summary>
    [Fact]
    public void GenerateSecureToken_DefaultLength()
    {
        // Act
        string token = EncryptionUtility.GenerateSecureToken(); // Default length

        // Assert
        // Default length is 32 bytes, which when Base64 encoded becomes 44 chars,
        // then minus padding (=) and with URL-safe replacements
        token.Length.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests that ComputeHmacSha256 produces consistent results.
    /// </summary>
    [Fact]
    public void ComputeHmacSha256_SameInputs_SameOutput()
    {
        // Arrange
        const string message = "Hello, World!";
        const string key = "secret-key-123";

        // Act
        string hash1 = EncryptionUtility.ComputeHmacSha256(message, key);
        string hash2 = EncryptionUtility.ComputeHmacSha256(message, key);

        // Assert
        hash1.Should().NotBeNullOrEmpty();
        hash1.Should().Be(hash2);
        hash1.Should().MatchRegex("^[0-9a-f]+$"); // Should be hex string
    }

    /// <summary>
    /// Tests that ComputeHmacSha256 produces different outputs for different inputs.
    /// </summary>
    [Fact]
    public void ComputeHmacSha256_DifferentInputs_DifferentOutputs()
    {
        // Arrange
        const string key = "same-key";
        const string message1 = "First message";
        const string message2 = "Second message";

        // Act
        string hash1 = EncryptionUtility.ComputeHmacSha256(message1, key);
        string hash2 = EncryptionUtility.ComputeHmacSha256(message2, key);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    /// <summary>
    /// Tests that ComputeHmacSha256 throws for null or empty inputs.
    /// </summary>
    [Fact]
    public void ComputeHmacSha256_NullOrEmptyInputs_ThrowsArgumentException()
    {
        // Act & Assert
        Action act1 = () => EncryptionUtility.ComputeHmacSha256(null, "key");
        act1.Should().Throw<ArgumentException>()
            .WithMessage("*Message and key cannot be null or empty*");

        Action act2 = () => EncryptionUtility.ComputeHmacSha256("", "key");
        act2.Should().Throw<ArgumentException>()
            .WithMessage("*Message and key cannot be null or empty*");

        Action act3 = () => EncryptionUtility.ComputeHmacSha256("message", null);
        act3.Should().Throw<ArgumentException>()
            .WithMessage("*Message and key cannot be null or empty*");

        Action act4 = () => EncryptionUtility.ComputeHmacSha256("message", "");
        act4.Should().Throw<ArgumentException>()
            .WithMessage("*Message and key cannot be null or empty*");
    }

    /// <summary>
    /// Tests that ComputeSha256 produces consistent results.
    /// </summary>
    [Fact]
    public void ComputeSha256_SameInputs_SameOutput()
    {
        // Arrange
        const string input = "Hello, World!";

        // Act
        string hash1 = EncryptionUtility.ComputeSha256(input);
        string hash2 = EncryptionUtility.ComputeSha256(input);

        // Assert
        hash1.Should().NotBeNullOrEmpty();
        hash1.Should().Be(hash2);
        hash1.Should().MatchRegex("^[0-9a-f]+$"); // Should be hex string
    }

    /// <summary>
    /// Tests that ComputeSha256 produces different outputs for different inputs.
    /// </summary>
    [Fact]
    public void ComputeSha256_DifferentInputs_DifferentOutputs()
    {
        // Arrange
        const string input1 = "First input";
        const string input2 = "Second input";

        // Act
        string hash1 = EncryptionUtility.ComputeSha256(input1);
        string hash2 = EncryptionUtility.ComputeSha256(input2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    /// <summary>
    /// Tests that ComputeSha256 throws for null or empty input.
    /// </summary>
    [Fact]
    public void ComputeSha256_NullOrEmptyInput_ThrowsArgumentException()
    {
        // Act & Assert
        Action act1 = () => EncryptionUtility.ComputeSha256(null);
        act1.Should().Throw<ArgumentException>()
            .WithMessage("*Input cannot be null or empty*");

        Action act2 = () => EncryptionUtility.ComputeSha256(string.Empty);
        act2.Should().Throw<ArgumentException>()
            .WithMessage("*Input cannot be null or empty*");
    }

    /// <summary>
    /// Tests that HashPassword throws for null or empty password.
    /// </summary>
    [Fact]
    public void HashPassword_NullOrEmptyPassword_ThrowsArgumentException()
    {
        // Act & Assert
        Action act1 = () => EncryptionUtility.HashPassword(null);
        act1.Should().Throw<ArgumentException>()
            .WithMessage("*Password cannot be null or empty*");

        Action act2 = () => EncryptionUtility.HashPassword(string.Empty);
        act2.Should().Throw<ArgumentException>()
            .WithMessage("*Password cannot be null or empty*");
    }

    /// <summary>
    /// Tests that HashPassword throws for password too short.
    /// </summary>
    [Fact]
    public void HashPassword_TooShortPassword_ThrowsArgumentException()
    {
        // Act & Assert
        Action act = () => EncryptionUtility.HashPassword("123"); // 3 chars
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password must be at least 8 characters*");
    }
}