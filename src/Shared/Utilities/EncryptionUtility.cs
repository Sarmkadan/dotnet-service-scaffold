#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Utility class for encryption and hashing operations. Provides helpers for password hashing,
/// AES encryption, HMAC signing, and secure random generation. All operations use standard
/// .NET cryptography libraries. Never implement custom crypto.
/// </summary>
public static class EncryptionUtility
{
    /// <summary>
    /// Hashes a password using PBKDF2 with SHA256. Returns Base64-encoded salt + hash.
    /// The format allows verification without storing plaintext passwords.
    /// </summary>
    public static string HashPassword(string password)
    {
        ValidatePassword(password);

        byte[] salt = RandomNumberGenerator.GetBytes(16);

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            10000,
            HashAlgorithmName.SHA256,
            20); // 20 bytes for the hash

        // Combine salt and hash for storage
        byte[] combined = new byte[36];
        Buffer.BlockCopy(salt, 0, combined, 0, 16);
        Buffer.BlockCopy(hash, 0, combined, 16, 20);

        return Convert.ToBase64String(combined);
    }

    /// <summary>
    /// Verifies a password against a hash created by HashPassword().
    /// Returns true if the password matches the stored hash.
    /// </summary>
    public static bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            byte[] combined = Convert.FromBase64String(hash);

            if (combined.Length != 36)
                return false;

            byte[] salt = new byte[16];
            Buffer.BlockCopy(combined, 0, salt, 0, 16);

            byte[] storedHash = new byte[20];
            Buffer.BlockCopy(combined, 16, storedHash, 0, 20);

            byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                10000,
                HashAlgorithmName.SHA256,
                20); // 20 bytes for the hash

            // Constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Encrypts a string using AES-256-GCM. Returns Base64-encoded ciphertext with nonce and tag.
    /// </summary>
    public static string EncryptAes(string plaintext, byte[] key)
    {
        if (string.IsNullOrEmpty(plaintext))
            throw new ArgumentException("Plaintext cannot be null or empty", nameof(plaintext));

        if (key.Length != 32)
            throw new ArgumentException("Key must be 32 bytes for AES-256", nameof(key));

        // Explicitly specify tag size (16 bytes for 128-bit GCM tag)
        using (var aes = new AesGcm(key, 16))
        {
            byte[] nonce = new byte[12]; // 96 bits for GCM
            RandomNumberGenerator.GetBytes(nonce);

            var plainBytes = Encoding.UTF8.GetBytes(plaintext);
            var ciphertext = new byte[plainBytes.Length];
            var tag = new byte[16]; // 128-bit tag

            aes.Encrypt(nonce, plainBytes, ciphertext, tag);

            // Combine nonce + tag + ciphertext for storage
            var result = new byte[nonce.Length + tag.Length + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
            Buffer.BlockCopy(ciphertext, 0, result, nonce.Length + tag.Length, ciphertext.Length);

            return Convert.ToBase64String(result);
        }
    }

    /// <summary>
    /// Decrypts an AES-256-GCM encrypted string created by EncryptAes().
    /// </summary>
    public static string DecryptAes(string ciphertext, byte[] key)
    {
        if (string.IsNullOrEmpty(ciphertext))
            throw new ArgumentException("Ciphertext cannot be null or empty", nameof(ciphertext));

        if (key.Length != 32)
            throw new ArgumentException("Key must be 32 bytes for AES-256", nameof(key));

        try
        {
            var encryptedData = Convert.FromBase64String(ciphertext);

            if (encryptedData.Length < 28) // 12 (nonce) + 16 (tag) minimum
                throw new InvalidOperationException("Ciphertext is too short");

            // Explicitly specify tag size (16 bytes for 128-bit GCM tag)
            using (var aes = new AesGcm(key, 16))
            {
                var nonce = new byte[12];
                var tag = new byte[16];
                var cipher = new byte[encryptedData.Length - 28];

                Buffer.BlockCopy(encryptedData, 0, nonce, 0, 12);
                Buffer.BlockCopy(encryptedData, 12, tag, 0, 16);
                Buffer.BlockCopy(encryptedData, 28, cipher, 0, cipher.Length);

                var plaintext = new byte[cipher.Length];
                aes.Decrypt(nonce, cipher, tag, plaintext);

                return Encoding.UTF8.GetString(plaintext);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to decrypt data", ex);
        }
    }

    /// <summary>
    /// Generates a cryptographically secure random byte array.
    /// Useful for generating tokens, salts, and keys.
    /// </summary>
    public static byte[] GenerateRandomBytes(int length)
    {
        if (length <= 0)
            throw new ArgumentException("Length must be positive", nameof(length));

        byte[] data = new byte[length];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(data);
        }
        return data;
    }

    /// <summary>
    /// Generates a secure random string suitable for tokens (Base64-encoded).
    /// </summary>
    public static string GenerateSecureToken(int length = 32)
    {
        var bytes = GenerateRandomBytes(length);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    /// <summary>
    /// Computes an HMAC-SHA256 signature for a message. Used for signing API requests.
    /// </summary>
    public static string ComputeHmacSha256(string message, string key)
    {
        if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(key))
            throw new ArgumentException("Message and key cannot be null or empty");

        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }

    /// <summary>
    /// Computes SHA256 hash of a string. Used for checksums and fingerprinting.
    /// </summary>
    public static string ComputeSha256(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));

        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }

    /// <summary>
    /// Validates a password meets minimum security requirements.
    /// </summary>
    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        if (password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters", nameof(password));
    }
}
