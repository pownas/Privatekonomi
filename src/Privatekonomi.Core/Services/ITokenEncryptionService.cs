namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for encrypting and decrypting sensitive tokens
/// </summary>
public interface ITokenEncryptionService
{
    /// <summary>
    /// Encrypts a token string
    /// </summary>
    string Encrypt(string plaintext);

    /// <summary>
    /// Decrypts an encrypted token
    /// </summary>
    string Decrypt(string ciphertext);

    /// <summary>
    /// Checks if a string is encrypted
    /// </summary>
    bool IsEncrypted(string value);
}
