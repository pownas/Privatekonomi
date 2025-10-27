using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class TokenEncryptionServiceTests
{
    private readonly ITokenEncryptionService _encryptionService;

    public TokenEncryptionServiceTests()
    {
        // Setup Data Protection for testing
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDataProtection();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var dataProtectionProvider = serviceProvider.GetRequiredService<IDataProtectionProvider>();
        _encryptionService = new TokenEncryptionService(dataProtectionProvider);
    }

    [Fact]
    public void Encrypt_ShouldReturnEncryptedString()
    {
        // Arrange
        var plaintext = "my-secret-token-12345";

        // Act
        var encrypted = _encryptionService.Encrypt(plaintext);

        // Assert
        Assert.NotNull(encrypted);
        Assert.NotEqual(plaintext, encrypted);
        Assert.StartsWith("ENC:", encrypted);
    }

    [Fact]
    public void Decrypt_ShouldReturnOriginalString()
    {
        // Arrange
        var plaintext = "my-secret-token-12345";
        var encrypted = _encryptionService.Encrypt(plaintext);

        // Act
        var decrypted = _encryptionService.Decrypt(encrypted);

        // Assert
        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void Encrypt_ShouldNotDoubleEncrypt()
    {
        // Arrange
        var plaintext = "my-secret-token-12345";
        var encrypted = _encryptionService.Encrypt(plaintext);

        // Act
        var encryptedAgain = _encryptionService.Encrypt(encrypted);

        // Assert
        Assert.Equal(encrypted, encryptedAgain);
    }

    [Fact]
    public void Decrypt_UnencryptedString_ShouldReturnAsIs()
    {
        // Arrange
        var plaintext = "not-encrypted-token";

        // Act
        var result = _encryptionService.Decrypt(plaintext);

        // Assert
        Assert.Equal(plaintext, result);
    }

    [Fact]
    public void IsEncrypted_ShouldDetectEncryptedStrings()
    {
        // Arrange
        var plaintext = "my-secret-token-12345";
        var encrypted = _encryptionService.Encrypt(plaintext);

        // Act & Assert
        Assert.True(_encryptionService.IsEncrypted(encrypted));
        Assert.False(_encryptionService.IsEncrypted(plaintext));
    }

    [Fact]
    public void Encrypt_NullOrEmpty_ShouldReturnAsIs()
    {
        // Arrange & Act & Assert
        Assert.Null(_encryptionService.Encrypt(null!));
        Assert.Equal(string.Empty, _encryptionService.Encrypt(string.Empty));
    }

    [Fact]
    public void Decrypt_NullOrEmpty_ShouldReturnAsIs()
    {
        // Arrange & Act & Assert
        Assert.Null(_encryptionService.Decrypt(null!));
        Assert.Equal(string.Empty, _encryptionService.Decrypt(string.Empty));
    }

    [Fact]
    public void RoundTrip_MultipleTokens_ShouldWork()
    {
        // Arrange
        var tokens = new[]
        {
            "access-token-abc123",
            "refresh-token-xyz789",
            "another-token-with-special-chars!@#$%"
        };

        // Act & Assert
        foreach (var token in tokens)
        {
            var encrypted = _encryptionService.Encrypt(token);
            var decrypted = _encryptionService.Decrypt(encrypted);
            
            Assert.Equal(token, decrypted);
            Assert.NotEqual(token, encrypted);
            Assert.True(_encryptionService.IsEncrypted(encrypted));
        }
    }
}
